# La diffusion du planning en image · brief de démarrage

Écrit le 2026-08-07, après la fusion de la PR 34. `main` est à `65b0ba3`,
**574 tests au vert**, lots 0 à 11 livrés plus « Rôles & cloisonnement », le lot
du log, et celui de la cloche et des vacances.

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

**C'est le point 3 de la première version** (`design_handoff_gymxyz/01-LOTS.md`),
et c'est désormais **le premier point qui reste** — le point 2 a été vidé le
2026-08-07 et la V1 tient en cinq points.

---

## La décision, et ce qu'elle contredit

> **Tranchée le 2026-08-07** : le bouton **génère une image du planning
> hebdomadaire**, que le manager télécharge pour la publier sur les réseaux
> sociaux. **Aucun envoi, aucune notification, aucun lien public.**

Il faut mesurer ce que ça veut dire, parce que **la maquette dessine autre chose,
et en détail**. `design/app/screen-planning.jsx:183` porte un `DiffusionModal`
complet : trois canaux à bascule (**notification push dans l'app**, **e-mail
récapitulatif**, **lien public partageable**), un **message optionnel**, un
« **Programmer plus tard** », et un bouton « **Diffuser maintenant** » avec une
icône d'envoi. L'en-tête annonce « 8 au 14 juin · 28 cours · **visible par 128
membres** ».

**Rien de tout ça n'est dans la V1.** C'est le seul endroit du produit où la
règle habituelle s'inverse : d'ordinaire **le prototype l'emporte sur les docs**,
mais ici une **décision métier explicite** l'emporte sur le prototype. La maquette
décrit une fonctionnalité qui n'a pas été retenue.

**Conséquence pratique** : ce lot ne « câble » pas le modal existant. Il conçoit
un écran que la maquette ne porte pas — comme la cloche l'aurait demandé, à ceci
près qu'ici on sait exactement ce que le bouton doit faire.

---

## Les quatre textes qui mentent déjà

C'est le point le plus urgent du lot, et il est indépendant du reste : **les
boutons désactivés annoncent aujourd'hui un envoi qui n'arrivera jamais.**

| Où | Ce qui est écrit | Le problème |
|---|---|---|
| `DashboardFilters.cs:116` | « L'**envoi** du planning **aux membres** n'est pas encore disponible : ce message n'a pas de réglage de notification. » | Promet un envoi et un réglage de notification. Les deux sont annulés. |
| `DashboardFilters.cs:128` | « La **diffusion aux membres** n'est pas encore active. Le planning reste consultable ici. » | Idem, en pied de la carte semaine. |
| `PlanningDesktop.razor:28` | « Disponible **avec les notifications**. » | Rattache le bouton à un lot qui n'existe plus. |
| `AccueilDesktop.razor:14` et `AccueilMobile.razor:19` | « Préparez la semaine et **diffusez-la à vos membres**. » | Le sous-titre de l'Accueil, sur les deux plateformes. |

Les trois premiers disparaissent avec l'activation. **Le quatrième reste après**,
et c'est celui qu'on oublie : « diffusez-la à vos membres » décrira toujours faux
une fonction qui produit une image à publier soi-même. Il se réécrit dans ce lot,
qu'on active les boutons ou non.

**Et il y a un cinquième bouton.** « Aperçu » (`AccueilDesktop.razor:16`) est
désactivé avec sa propre raison — « l'aperçu du planning arrive dans un lot
ultérieur ». Ce lot est celui-là, et « voir l'image avant de la télécharger » est
très exactement ce que ce bouton devrait faire. **À trancher** : est-ce qu'il
devient l'aperçu de l'image, ou est-ce qu'il reste désactivé ?

---

## Le vrai sujet : quelle image ?

**La maquette ne répond pas**, parce qu'elle dessinait un envoi. C'est la
question centrale du plan, et elle est de conception, pas de technique.

**La grille du Planning n'est pas une image publiable.** Sept colonnes sur une
quinzaine de lignes d'heures, c'est fait pour un écran de 1440 px et une souris.
Réduit au format d'un post Instagram, c'est illisible : les blocs font quelques
pixels de haut et le texte disparaît.

Ce qu'un post demande, c'est autre chose — un **format portrait ou carré**, le
nom de la salle en tête, la semaine, et les séances **en liste par jour** avec
l'heure, le cours et le coach. Autrement dit : **une mise en page à concevoir**,
pas une capture de l'existant.

**À trancher au plan :**

- **Le format.** Carré 1080×1080 (Instagram, Facebook), portrait 1080×1350, ou
  story 1080×1920 ? Un seul, ou un choix ? En proposer trois multiplie le travail
  de mise en page par trois ; en proposer un le rend inutilisable là où il ne
  rentre pas.
- **Ce que porte l'image.** Toutes les séances de la semaine, ou seulement les
  collectives ? Un cours **privé** (`Capacity == 1`) n'a rien à faire sur un post
  public, et un cours **complet** non plus, sans doute — le publier attire des
  gens vers une porte fermée. Les séances **annulées** encore moins.
- **Les places restantes.** « 13/16 » sur l'écran est une information de gestion.
  Sur un post, « il reste 3 places » est un argument commercial — et « complet »
  un repoussoir. À décider, et ce n'est pas la même donnée.
- **La semaine visée.** Le bouton du Planning connaît la semaine affichée ; celui
  de l'Accueil montre la semaine en cours. Et un manager qui publie le dimanche
  veut sans doute **la semaine suivante**. Trois réponses possibles pour deux
  boutons.
- **Les filtres.** Le Planning porte des filtres coach / lieu / format. L'image
  les respecte-t-elle ? Publier une semaine filtrée sur un seul coach sans le dire
  serait un piège ; les ignorer silencieusement en est un autre.

---

## Comment on fabrique l'image

**Aucune bibliothèque d'image n'est présente dans la solution** — vérifié : ni
SkiaSharp, ni ImageSharp, ni Playwright, ni QuestPDF, ni rien côté client. C'est
donc une dépendance à ajouter, et le choix engage le déploiement.

| Voie | Ce que ça coûte | Ce que ça donne |
|---|---|---|
| **SVG construit en C#**, rasterisé par le navigateur | rien à installer | un seul rendu, déterministe, thème résolu côté serveur |
| **SkiaSharp / ImageSharp**, dessin à la main | un paquet natif par plateforme | contrôle total, mais **un second moteur de rendu** qui divergera du Razor |
| **Navigateur headless** (Playwright) | ~150–300 Mo, un process à gérer | rend le vrai HTML, mais lourd à déployer pour une image |
| **Capture côté client** (`html2canvas`) | un JS tiers, hors CDN | suit le thème tout seul, mais capture une grille illisible (voir ci-dessus) |

**Ma préférence, à confirmer au plan : le SVG construit côté serveur.** Une seule
mise en page, écrite une fois, en C# testable ; les couleurs du thème résolues au
moment de la génération ; aucun binaire natif ; et le téléchargement peut être le
SVG lui-même ou un PNG rasterisé par le navigateur en trois lignes de canvas.
C'est aussi la seule voie où **le contenu de l'image se teste sans image** — on
assert sur du texte.

Le point qui tranche contre `html2canvas` n'est pas technique : **il capturerait
la grille**, et la grille n'est pas ce qu'on veut publier.

---

## La marque, et le piège du white label

Le lot 11 a rendu chaque client repeignable, et le principe du produit est que
**changer de marque ne change aucun écran**. Une image qui ne suivrait pas le
thème du client serait **le seul endroit du produit qui l'ignore** — et c'est
l'endroit le plus visible, puisqu'il finit sur un réseau social.

Deux règles déjà payées s'appliquent telles quelles :

- **Aucune marque GymXYZ sur l'image.** Le kettlebell a été retiré partout ; un
  client sans logo affiche **son nom seul**. Une image sortant du produit avec un
  « propulsé par GymXYZ » réintroduirait exactement ce qu'on a enlevé.
- **Les couleurs viennent des tokens du thème** (`themes.css`, blocs
  `[data-theme="…"]`), pas de constantes. La palette de statut
  (`success / warning / danger`) n'est jamais thémée — si l'image marque un
  « complet », c'est du `danger` partagé.

**À trancher au plan** : comment les tokens CSS arrivent jusqu'au générateur. Ils
sont aujourd'hui dans une feuille de style, donc lisibles par le navigateur et
**pas par le C#**. Soit on les duplique côté serveur — et ils divergeront —, soit
on les fait remonter, soit le thème du tenant expose ses quelques couleurs utiles
dans le modèle. C'est la seule vraie question d'architecture du lot.

---

## Qui a le droit de diffuser

**Un coach est cloisonné à ses propres séances depuis le lot rôles.** Une image
de la semaine entière du club lui donnerait, en un clic, exactement ce que ses
écrans lui refusent.

C'est le même piège que celui qui a fait reporter la cloche : *le périmètre du
contenu n'est pas celui du destinataire*. Il se traite ici, pas après.

**À trancher au plan** : le bouton est-il `IManagerOnly` — le seul marqueur qui
existe aujourd'hui (`Application/Interfaces/IManagerOnly.cs`) —, ou un coach
peut-il générer une image de **ses** séances ? La première réponse est la plus
sûre et la plus simple ; la seconde demande une image filtrée et se décide
maintenant, pas une fois le générateur écrit.

---

## Critère d'acceptation

- **Les trois boutons s'activent ensemble** — Accueil desktop, Accueil mobile,
  tête du Planning. C'est écrit dans la décision du lot 10 et c'est vérifiable :
  un bouton actif à côté d'un bouton désactivé pour la même fonction est un bug
  d'écran.
- **Aucun texte ne promet plus un envoi.** Les quatre libellés du tableau
  ci-dessus sont réécrits, sous-titres compris. Le mot « diffuser » peut rester —
  c'est le mot du prototype et du client — mais il ne doit plus dire « à vos
  membres ».
- **L'image est réellement produite et réellement téléchargée**, sur desktop et
  sur mobile. Une capture de l'image obtenue est attendue, pas seulement un test
  qui passe.
- **L'image suit le thème du client** : vérifié sur deux clients aux thèmes
  différents, et **aucune marque GymXYZ** sur aucune des deux.
- **Un coach ne peut pas obtenir ce que son écran lui cache** — quelle que soit
  la réponse retenue, elle est vérifiée.
- **L'image est lisible à la taille où elle sera vue.** Le critère n'est pas
  « le fichier existe » mais « on lit les cours sur un téléphone ».
- `dotnet test` vert, **zéro warning** dans les projets applicatifs.
- **Le log reste lisible.** Une génération qui échoue se traite ou se consigne ;
  elle ne remonte pas au rendu.

---

## Pièges déjà payés

- **Avant de croire à un écran vide, lire `window.innerWidth`.** Le volet
  navigateur rapporte parfois **0** ; `ResponsiveModeService` lit cette largeur,
  un 0 passe sous le seuil mobile, et la grille bureau se rend vide. Forcer
  1440 × 900, puis recharger.
- **Le volet ne transmet aucun clic** sur les formulaires. Poster en JS ; pour la
  connexion, poser `.value` **sur les `fluent-text-field` eux-mêmes**. En
  revanche un `.click()` en JS sur un bouton Blazor **fonctionne** — vérifié au
  lot précédent sur « Semaine suivante ».
- **Les comptes de démo** sont dans `DbInitializer` : `dwayne.johnson@gymxyz.fr`
  (GymXYZ), `aurelie.siquier@teamtrainers.fr`, `najate.amzil@leyssa-coaching.fr`
  — et non `najate@`, qui est l'adresse de contact du client. Mot de passe
  `GymXyz!2026`.
- **Le tenant vient des claims de l'utilisateur**, pas de l'hôte, dès qu'on est
  authentifié : changer de client = se reconnecter.
- **La base de dev est recréée au démarrage** et déconnecte tout le monde.
- **Un seul serveur peut tenir le port 5173.**

---

## Ce qui n'est pas dans ce lot

- **Tout envoi** : push, e-mail récapitulatif, lien public, message optionnel,
  programmation — les cinq éléments du `DiffusionModal` de la maquette. Décision
  du 2026-08-07.
- **La cloche de notifications** — hors V1, « pour le moment ». Quand elle
  reviendra, elle commencera par la question qu'elle n'a pas tranchée : une
  notification est-elle dérivée des alertes déjà calculées, ou stockée avec un
  état de lecture ?
- **La météo des cours extérieurs** — abandonnée. `IsWeatherDependent` et le lieu
  de repli restent, renseignés et lus par le gérant.
- **Points 4 et 5 — login & onboarding, portail super-admin.** Handoffs à
  fournir, et le 01-LOTS interdit de les commencer par morceaux en attendant.
- **Point 6 — comptes à casquettes multiples** (entrée 4 du registre).
- **Entrée 5 — migrations EF**, en dernier, juste avant un déploiement.
- **SMS**, **recherche globale**, **portail membre** : hors V1.

---

## Premier geste suggéré

1. **Réécrire les quatre textes** qui promettent un envoi. C'est la partie sûre,
   elle ne dépend d'aucune décision de rendu, et elle enlève un mensonge de
   l'écran dès aujourd'hui.
2. **Décider ce que l'image montre** — format, contenu, quelles séances,
   quelle semaine. C'est de la conception, c'est le vrai sujet, et rien de
   technique ne se décide avant.
3. **Décider comment les couleurs du thème atteignent le générateur.** C'est la
   seule question d'architecture, et elle conditionne le choix de la voie.
4. **Trancher le périmètre des rôles** avant d'écrire le générateur, pas après.
5. **Écrire la mise en page en SVG côté C#**, testable sur son contenu, puis le
   téléchargement.
6. **Vérifier à la taille réelle**, sur un téléphone, sur deux clients.

Revenir avec un plan qui dit **quelle image on produit** (format, contenu,
semaine), **comment le thème l'atteint**, **qui a le droit de la générer**, et ce
que devient le bouton « Aperçu ». **Attendre la validation avant d'écrire.**
