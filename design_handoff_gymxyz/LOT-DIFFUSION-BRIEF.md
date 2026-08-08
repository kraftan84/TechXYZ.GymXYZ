# La diffusion du planning en image · brief de démarrage

**Réécrit le 2026-08-08**, après l'arrivée du second handoff. `main` est à
`5a3fa80`, lots 0 à 11 livrés plus « Rôles & cloisonnement », le lot du log, et
celui de la cloche et des vacances.

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

**C'est le point 3 de la première version** (`01-LOTS.md`), et le premier point
qui reste.

> **La suite n'est pas verte au moment d'écrire.** 573 tests passent, **1
> échoue** : `AttendanceQueryTests.GetAttendanceOverview_ShouldSplitOpenAndValidatedSheets`
> attend `SessionsToday == 1` pour une séance semée **aujourd'hui à 9 h**, alors
> que le KPI se compte sur `RecentAttendanceWindow(now)`, qui s'arrête à
> « maintenant ». **Il échoue tous les matins avant 9 h et passe le reste de la
> journée.** Ce n'est pas ce lot qui l'a cassé et ce n'est pas son sujet, mais on
> ne démarre pas sur une suite rouge : le corriger est une ligne, à faire en
> premier ou à part.

---

## Ce qui a changé depuis la première version de ce brief

Ce brief a été écrit une première fois le 2026-08-07, **avant** que le second
handoff n'arrive. `08-PLANNING-DIFFUSE.md` répond maintenant à la question qui
était présentée comme centrale — *quelle image ?* — et il y répond **au pixel**,
en trois habillages, un par marque. Il fournit aussi le prototype :
`design/Planning diffusé - 3 styles.html`, du HTML/CSS sans React ni JS,
c'est-à-dire exactement ce qui doit être rendu.

**Et il renverse la voie technique que ce brief recommandait.** Il faut le dire
franchement plutôt que de le lisser : je proposais de construire l'image en
**SVG côté C#**, et l'argument décisif contre le navigateur headless était qu'il
*capturerait la grille du Planning, illisible en vignette*. Cet argument **est
caduc** — l'affiche n'est pas la grille, c'est une mise en page à elle, et elle
est désormais spécifiée en HTML/CSS. Réécrire en dessin C# trois styles définis
au pixel, ce serait redessiner la spécification à la main et la faire diverger
au premier ajustement.

---

## Ce que le handoff tranche, et qu'on ne rouvre pas

| Question | Réponse du doc `08` |
|---|---|
| Le format | **1080 × 1350** (4:5), format de référence. Carré, story et A5 sont listés en §6 comme variantes ultérieures. |
| La mise en page | Squelette commun `.post / .head / .days / .day / .slots / .slot / .foot`, §2. Les 7 jours **se partagent la hauteur** — une semaine chargée et une semaine calme donnent la même affiche. |
| Les créneaux manquants | Cellules `visibility:hidden`, **pas** absentes : compléter chaque journée jusqu'à 3 cellules (2 pour Leyssa) pour que les colonnes restent alignées. |
| Les trois marques | §3, §4, §5, au pixel. Leyssa passe à **2 colonnes**, jour en toutes lettres, marque en cercle — et **jamais d'adresse postale**, la zone seule. |
| Ce que l'image porte | §8 : heure, nom du cours, méta (studio · places, ou durée · coach), état `normal / complet / accent`, « Repos » si rien de publiable. |
| Ce qu'elle ne porte jamais | **Aucun nom d'adhérent, aucun effectif inscrit, aucun prix.** Places **restantes** oui, nombre d'inscrits non. Séances privées **exclues par défaut**. |
| Comment la marque atteint le rendu | `data-theme="{tenant.ThemeKey}"` sur la racine + un bloc CSS par marque. **Un nouveau client = un bloc CSS, pas un composant.** |
| La source des données | `GetWeekPlanningQuery`, livrée au lot 5. **Aucune entité nouvelle.** |

C'était présenté comme « la seule vraie question d'architecture du lot » :
comment les tokens CSS atteignent le générateur, puisqu'ils vivent dans une
feuille de style que le C# ne lit pas. **La question disparaît** — si le rendu
est fait par un navigateur, les tokens sont lus là où ils sont.

---

## La voie technique, et ce qu'elle coûte vraiment

Le handoff recommande : **composant Razor `PlanningPoster.razor`** nourri par
`GetWeekPlanningQuery`, rendu côté serveur, puis **capturé en PNG** par un
navigateur headless (`Playwright .NET` ou `PuppeteerSharp`), viewport
1080 × 1350, `deviceScaleFactor: 2` pour l'impression.

**Ce que ça engage, et qui n'est pas dans le handoff** : aucune bibliothèque
d'image ni navigateur n'est présente dans la solution — vérifié. C'est donc une
dépendance nouvelle, de 150 à 300 Mo, **et un binaire de navigateur à installer
là où l'application tourne**. Ce n'est pas un paquet NuGet de plus : c'est une
contrainte de déploiement, à accepter en connaissance de cause. Elle arrive au
moment où le déploiement n'est pas encore fait — ce qui est plutôt le bon moment,
mais il faut le dire au plan et pas le découvrir ensuite.

**Le piège n°1 du handoff est déjà payé ici.** Il prévient qu'une police tirée de
Google Fonts au moment de la capture tombe en Montserrat une fois sur deux. Or
les cinq fichiers sont déjà auto-hébergés depuis le lot 11 :
`wwwroot/css/techxyz/assets/fonts/` porte Orbitron, Anton, Dancing Script,
Montserrat et son italique. **Reste la moitié qui n'est pas acquise** : attendre
`document.fonts.ready` **avant** la capture.

**À proposer au plan** : Playwright ou PuppeteerSharp, comment le navigateur est
présent en développement et en déploiement, et le cache — le handoff suggère une
empreinte (semaine + tenant + options) invalidée à toute modification du planning
de la semaine.

---

## Ce que le handoff ne tranche pas, et qui reste le vrai travail

**1. Le parcours de génération n'est pas maquetté** (§6), et le handoff demande
de repasser en design avant de le coder. Il décrit pourtant ce qu'il contiendrait :
semaine (par défaut **la suivante**), format, quoi afficher, aperçu à l'échelle,
sortie (téléchargement, `navigator.share`, presse-papier).

**C'est la décision de périmètre du lot, et elle vous revient.** Deux lectures se
défendent :

- **Livrer le rendu seul** — un bouton, la semaine suivante, le format de
  référence, téléchargement. Aucun écran à dessiner, la V1 avance, et le parcours
  se maquette pendant que le générateur tourne déjà.
- **Attendre le design du parcours** — conforme au handoff, mais ça met le point 3
  en attente d'un aller-retour design alors que les points 4 et 5 en attendent
  déjà un chacun.

Ma préférence, à valider : **le rendu seul**, parce que l'image est ce que la
décision du 2026-08-07 promettait et que le reste est du réglage. Mais c'est un
arbitrage, pas une évidence.

**2. Plus de 3 cours dans une journée** : le prototype ne le traite pas (§2).
Afficher les 3 premiers et une cellule « +N autres », ou passer la journée en 4
colonnes ? **À trancher avec le design**, pas au fil du code — et à trancher
quand même, parce qu'une semaine réelle le rencontrera.

**3. Quelle semaine.** Le handoff dit « par défaut la suivante ». Le bouton du
Planning, lui, connaît la semaine affichée, et celui de l'Accueil la semaine en
cours. **Trois réponses possibles pour deux boutons** : à unifier.

**4. Les filtres du Planning** (coach / lieu / format). L'image les respecte-t-elle ?
Publier une semaine filtrée sur un seul coach sans le dire est un piège ; les
ignorer en silence en est un autre.

**5. Qui a le droit de diffuser — et ici le code répond déjà à moitié.**
`GetWeekPlanningQuery.Handler` applique `CoachScope` **en plancher** depuis le lot
rôles : un coach qui demande la semaine n'obtient que **ses** séances. Donc un
coach qui génère une affiche obtiendrait une **semaine partielle présentée comme
le planning du club** — pas une fuite, mais un mensonge, et publié.

C'est le même piège que celui qui a fait reporter la cloche : *le périmètre du
contenu n'est pas celui du destinataire*. **À trancher** : le bouton est-il
`IManagerOnly` (le marqueur existe, `Application/Interfaces/IManagerOnly.cs`), ou
un coach peut-il produire une affiche de ses séances, explicitement titrée comme
telle ? La première réponse est la plus sûre, et elle se pose maintenant, pas
après le générateur.

---

## Les quatre textes qui mentent déjà

Indépendant de tout le reste, et sûr : **les boutons désactivés annoncent
aujourd'hui un envoi qui n'arrivera jamais.**

| Où | Ce qui est écrit | Le problème |
|---|---|---|
| `DashboardFilters.cs:116` | « L'**envoi** du planning **aux membres** n'est pas encore disponible… » | Promet un envoi et un réglage de notification. Les deux sont annulés. |
| `DashboardFilters.cs:128` | « La **diffusion aux membres** n'est pas encore active. » | Idem, en pied de la carte semaine. |
| `PlanningDesktop.razor:28` | « Disponible **avec les notifications**. » | Rattache le bouton à un lot qui n'existe plus. |
| `AccueilDesktop.razor:14`, `AccueilMobile.razor:19` | « Préparez la semaine et **diffusez-la à vos membres**. » | Le sous-titre de l'Accueil, sur les deux plateformes. |

Les trois premiers disparaissent avec l'activation. **Le quatrième reste après**,
et c'est celui qu'on oublie : « diffusez-la à vos membres » décrira toujours faux
une fonction qui produit une image à publier soi-même.

**Et il y a un cinquième bouton.** « Aperçu » (`AccueilDesktop.razor:16`) est
désactivé avec sa propre raison — « l'aperçu du planning arrive dans un lot
ultérieur ». Ce lot est celui-là, et le handoff met « aperçu à l'échelle » dans le
parcours (§6). **À trancher avec le périmètre** : il devient l'aperçu de l'image,
ou il reste désactivé si le parcours attend le design.

---

## Ce que la maquette de l'app dessine, et qui n'est pas dans la V1

`design/app/screen-planning.jsx:183` porte un `DiffusionModal` complet : trois
canaux à bascule (**push**, **e-mail**, **lien public partageable**), un message
optionnel, « Programmer plus tard », « Diffuser maintenant », et un en-tête
« 28 cours · **visible par 128 membres** ».

**Rien de tout ça n'est dans la V1.** C'est le seul endroit du produit où la règle
habituelle s'inverse : d'ordinaire **le prototype l'emporte sur les docs**, mais
ici une **décision métier explicite** l'emporte sur le prototype. Le doc `08` est
d'ailleurs d'accord — la publication directe vers Meta y est « hors périmètre, à
chiffrer à part ».

---

## Critère d'acceptation

- **Les trois boutons s'activent ensemble** — Accueil desktop, Accueil mobile,
  tête du Planning. Un bouton actif à côté d'un bouton désactivé pour la même
  fonction est un bug d'écran.
- **Aucun texte ne promet plus un envoi**, sous-titres compris. Le mot
  « diffuser » peut rester — c'est le mot du prototype et du client — mais il ne
  doit plus dire « à vos membres ».
- **L'image est réellement produite et réellement téléchargée**, desktop et
  mobile. Une capture de l'image obtenue est attendue, pas seulement un test vert.
- **Les trois habillages sont conformes au prototype**, vérifiés côte à côte avec
  `Planning diffusé - 3 styles.html`. C'est du high-fidelity : la cible est le
  pixel, pas l'approximation.
- **Les polices de marque sont dans l'image** — c'est le piège n°1 du handoff, et
  il ne se voit que sur l'image finale.
- **Aucune marque GymXYZ sur l'affiche d'un client**, et aucune adresse postale
  pour Leyssa.
- **Rien de privé sur l'affiche** : aucun nom d'adhérent, aucun effectif inscrit,
  aucun prix, aucune séance privée.
- **Un coach ne peut pas publier une semaine partielle en la faisant passer pour
  celle du club** — quelle que soit la réponse retenue, elle est vérifiée.
- **L'image est lisible à la taille où elle sera vue** : le critère n'est pas « le
  fichier existe » mais « on lit les cours sur un téléphone ».
- `dotnet test` vert — y compris le test du matin — et **zéro warning** dans les
  projets applicatifs.
- **Le log reste lisible** : une génération qui échoue se traite ou se consigne,
  elle ne remonte pas au rendu.

---

## Pièges déjà payés

- **Avant de croire à un écran vide, lire `window.innerWidth`.** Le volet
  navigateur rapporte parfois **0** ; `ResponsiveModeService` lit cette largeur,
  un 0 passe sous le seuil mobile, et la grille bureau se rend vide. Forcer
  1440 × 900, puis recharger.
- **Le volet ne transmet aucun clic** sur les formulaires. Poster en JS ; pour la
  connexion, poser `.value` **sur les `fluent-text-field` eux-mêmes**. En revanche
  un `.click()` en JS sur un bouton Blazor **fonctionne**.
- **Les comptes de démo** sont dans `DbInitializer` : `dwayne.johnson@gymxyz.fr`,
  `aurelie.siquier@teamtrainers.fr`, `najate.amzil@leyssa-coaching.fr` — et non
  `najate@`, qui est l'adresse de contact du client. Mot de passe `GymXyz!2026!`.
- **Le tenant vient des claims**, pas de l'hôte, dès qu'on est authentifié :
  changer de client = se reconnecter.
- **La base de dev est recréée au démarrage** et déconnecte tout le monde.
- **Un seul serveur peut tenir le port 5173.**
- **Les écrans rendent un état vide avant l'arrivée des données** : attendre la
  stabilisation avant de conclure à une régression.

---

## Ce qui n'est pas dans ce lot

- **Tout envoi** : push, e-mail, lien public, message, programmation — les cinq
  éléments du `DiffusionModal`. Décision du 2026-08-07.
- **La publication directe vers Instagram / Facebook** : intégration Meta, hors
  périmètre, à chiffrer à part (`08`, §6).
- **Les formats carré, story et A5** : listés en §6, après le format de référence.
- **La cloche de notifications** — hors V1, « pour le moment ».
- **La météo** — abandonnée. `IsWeatherDependent` et le lieu de repli restent.
- **Points 4 et 5 — l'entrée (doc `06`) et la console (doc `07`).** Leurs handoffs
  sont arrivés, mais ils viennent après, et `01-LOTS.md` interdit de les commencer
  par morceaux.
- **Point 6 — comptes à casquettes multiples** (entrée 4 du registre).
- **Entrée 5 — migrations EF**, en dernier, juste avant un déploiement.

---

## Premier geste suggéré

1. **Ouvrir `design/Planning diffusé - 3 styles.html` dans le navigateur.** C'est
   la cible, elle est déjà écrite en HTML/CSS, et elle vaut tous les résumés — y
   compris celui-ci.
2. **Corriger le test du matin**, pour partir d'une suite verte.
3. **Trancher le périmètre** : le rendu seul, ou le parcours complet avec un
   aller-retour design. Tout le reste du plan en dépend.
4. **Trancher le périmètre des rôles** avant d'écrire le générateur.
5. **Réécrire les quatre textes** qui promettent un envoi — c'est sûr, ça ne
   dépend d'aucune décision, et ça enlève un mensonge de l'écran dès aujourd'hui.
6. **Choisir le moteur de capture** et dire comment il vit en développement et au
   déploiement.
7. **Vérifier à la taille réelle**, sur un téléphone, sur les trois marques.

Revenir avec un plan qui dit **le périmètre retenu**, **qui a le droit de
générer**, **quel moteur de capture et à quel coût de déploiement**, et ce que
devient le bouton « Aperçu ». **Attendre la validation avant d'écrire.**
