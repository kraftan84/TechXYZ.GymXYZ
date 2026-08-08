# La météo et les vacances · brief de démarrage

> ⚠️ **Ce brief a été dépassé en cours de lot, le 2026-08-07.** La météo — son
> sujet principal — a été **abandonnée pour la V1** après instruction : trop de
> plomberie pour ce qu'elle rendait à ce stade. Tout ce qui suit sur la
> vigilance, les API candidates et la chaîne code postal → département est
> conservé comme **enquête**, parce que la question rouvrira ; ce n'est pas une
> description du produit.
>
> **Ce que le lot a effectivement livré** : la cloche retirée des deux shells,
> les URL du calendrier scolaire sorties dans `appsettings`, les coordonnées
> `Latitude`/`Longitude` supprimées de `Location`, et le réglage « afficher les
> vacances scolaires » par client — verrouillé, avec un avertissement, pour un
> client sans adresse. L'autorité reste `design_handoff_gymxyz/01-LOTS.md`.

Écrit le 2026-08-07, après la fusion de la PR 33. `main` est à `f21b822`,
**564 tests au vert**, lots 0 à 11 livrés plus « Rôles & cloisonnement » et le lot
du log.

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

**C'est le point 2 de la première version** (`design_handoff_gymxyz/01-LOTS.md`),
**redéfini le 2026-08-07** — voir juste en dessous.

---

## Ce que ce lot est devenu

Le point 2 portait deux chantiers : la cloche de notifications et la météo des
cours extérieurs. **La cloche est reportée**, sur décision du 2026-08-07 : elle
sort de la V1 et son point rouge est masqué en attendant.

C'est le bon arbitrage, et pour une raison de fond plutôt que de calendrier : la
météo est un **branchement** — un service derrière une interface, posé sur des
champs livrés au lot 4 — là où la cloche était une **décision de modèle déguisée
en écran**. Rien ne stocke une notification aujourd'hui ; `NotificationSetting`
est une *préférence*, pas un événement. Il n'y a donc rien à lister, et la cloche
demandait d'abord de décider ce qu'est une notification (dérivée des alertes déjà
calculées par l'Accueil, ou stockée avec un état « lu »), puis de faire dessiner
un écran que la maquette ne porte pas. Deux inconnues pour un lot qui en a déjà
une.

**Le lot devient donc :**

1. **La vigilance météo sur les cours extérieurs** — l'appel réel, décidé le
   2026-08-07 : le Bulletin Vigilance de Météo-France, par département. Une
   vraie prévision au point viendra plus tard, si elle vaut son prix.
2. **Les URL d'API dans `appsettings`** — les nouvelles, et les deux du calendrier
   scolaire qui sont aujourd'hui écrites en dur.
3. **Masquer la cloche** — deux fragments de balisage, ci-dessous.
4. **Un réglage « afficher les vacances scolaires »** par client — idée du
   2026-08-07, qui tombe sur exactement la même surface que la météo.

> **À reporter dans `01-LOTS.md`** : l'écart README n°2 y est encore écrit
> « **dans la V1**, pour les managers et les coachs ». Cette décision est
> **remplacée** — la cloche est reportée hors V1, « pour le moment », donc à
> revisiter et non à enterrer. La PR de ce lot doit corriger le tableau du point 2
> et l'écart n°2, sans quoi le document dit le contraire du produit.

---

## Le calendrier scolaire est-il vraiment branché ? Oui — vérifié

La question a été posée, et la réponse est nette : **le service appelle deux API
gouvernementales réelles, et elles répondent.**

`TechXyz.GymXyz.WebApp/Services/SchoolCalendarService.cs` :

| Source | Endpoint | Vérifié le 2026-08-07 |
|---|---|---|
| Jours fériés (Etalab) | `calendrier.api.gouv.fr/jours-feries/metropole/{year}.json` | **200**, ~60 ms |
| Vacances scolaires (Éducation nationale, Opendatasoft) | `data.education.gouv.fr/api/explore/v2.1/…/fr-en-calendrier-scolaire/records` | **200**, ~70 ms |

Et ça se voit à l'écran : le Planning affiche « **Zone A · Vacances d'Été** » dans
son bandeau et la pastille « Vacances d'Été » sur chacun des sept jours. Ce sont
de vraies données, pas un repli.

**Correction à une affirmation d'un brief précédent.** Il y était écrit que ces
deux API « ne répondent pas depuis cet environnement ». **C'est faux** : le réseau
sortant fonctionne ici. Le « Calendrier indisponible » observé venait des rendus
dégénérés à `innerWidth = 0` (entrée 6 du registre, retirée) — le même artefact
qui avait fait croire à deux défauts d'écran inexistants. Conséquence pratique et
bonne nouvelle : **la météo sera démontrable pour de vrai**, sans faux service.

**C'est bien l'API que data.gouv.fr référence** sous
`dataservices/api-calendrier-scolaire` : même éditeur (Éducation nationale), même
jeu de données `fr-en-calendrier-scolaire`, Licence Ouverte 2.0, sans clé. Le
produit est donc déjà sur la bonne source — rien à changer de ce côté. (La fiche
data.gouv cite `explore/v2.0`, le code appelle `v2.1` ; même API, version plus
récente.)

Deux détails du service qui méritent d'être connus avant d'en écrire un second :

- il filtre la population « Enseignants », dont les dates sont publiées à côté de
  celles des élèves et doubleraient chaque période ;
- la fenêtre de requête déborde jusqu'en février de l'année suivante, parce que
  l'année scolaire chevauche l'année civile.

---

## La météo — sur quelle API ?

> **Décision du 2026-08-07 : le Bulletin Vigilance de Météo-France**, par
> département, en attendant de voir si on peut faire mieux. Ce qui suit est
> l'enquête qui y mène — elle est gardée parce que la marche suivante (une vraie
> prévision au point) rouvrira exactement ces questions, et qu'il serait absurde
> de les instruire deux fois.

**Testé le 2026-08-07 depuis cet environnement**, pas seulement lu :

| Candidat | Clé ? | Réponse | Forme |
|---|---|---|---|
| **Open-Meteo** | non | **200** | prévision par lat/lon, code WMO, probabilité de précipitations, vent, 7–16 j |
| **Météo-France** (portail API) | oui | 401 sans jeton | grilles de modèle, vigilance, observations, radar, climatologie |
| OpenWeatherMap | oui | 401 sans clé | prévision par point |
| **Géoplateforme** `data.geopf.fr/geocodage` | non | **200** | géocodage adresse/CP → lat/lon |

Réponse réelle d'Open-Meteo pour Lyon 3ᵉ, sept jours, un seul appel :
codes WMO, températures max, probabilité de pluie, vent max. **C'est exactement la
forme dont un cours en plein air a besoin** — « est-ce qu'il pleut jeudi à 18 h au
Parc de la Tête d'Or » se lit directement.

**Le piège est juridique, pas technique.** Le tarif d'Open-Meteo est explicite :
« The free API is for **non-commercial use**, rate-limited to 10 000 calls/day,
and carries no uptime guarantee. » GymXYZ est un produit commercial : l'endpoint
gratuit convient au développement, **pas à la production**. Un abonnement payant
donne la licence commerciale, une clé et un SLA. L'attribution est requise
(CC BY 4.0) dans tous les cas.

### Et les API du gouvernement ?

Question posée le 2026-08-07, et elle a **deux réponses opposées** selon le
service.

**Le géocodage : oui, et c'est réglé.** L'**API Géoplateforme Géocodage**
(`https://data.geopf.fr/geocodage`, IGN, adossée à la BAN et à la BD TOPO) est
**ouverte, sans clé, sous Licence Ouverte 2.0, 50 requêtes/seconde, 99,5 % de
disponibilité**. Testée sur l'adresse réellement semée de GymXYZ :

```
GET https://data.geopf.fr/geocodage/search?q=14 rue de la Villette 69003 Lyon&limit=1
→ 200 · [4.861769, 45.760672] · "Rue de la Villette 69003 Lyon"
```

C'est **le** choix pour les coordonnées : officiel, gratuit, réutilisable
commercialement, et il remplace l'ancien `api-adresse.data.gouv.fr`. Aucune raison
d'aller ailleurs.

**La prévision : non, l'État n'en publie pas de prête à l'emploi.** Météo-France
publie **dix** API sur data.gouv.fr — vigilance, AROME, ARPEGE, AROME prévision
immédiate, observations, packages d'observation, radar, packages radar,
climatologie, statistiques d'ensemble ARPEGE. **Aucune n'est une prévision au
point ou à la commune.** Les modèles livrent des « champs d'analyse et de
prévision **en points de grille** », c'est-à-dire du GRIB. En tirer « il pleut
jeudi à 18 h au Parc de la Tête d'Or » veut dire choisir un modèle, une run, une
échéance, télécharger une couverture et la décoder — en .NET, sans bibliothèque
GRIB sérieuse. C'est un chantier, pas un branchement, et la décision du lot 4 dit
explicitement l'inverse. Toutes sont par ailleurs « ouvertes **avec un compte** » :
jeton obligatoire, vérifié (401 sur la vigilance comme sur le reste).

**Le point qui tranche vraiment le débat.** Open-Meteo accepte
`models=meteofrance_arome_france_hd` : **il sert le modèle de Météo-France
lui-même.** Le choix n'est donc pas « données publiques françaises contre données
privées » — c'est **qui décode le GRIB**. L'État publie la grille ; Open-Meteo
publie le point extrait de cette même grille. Payer un abonnement, c'est payer
l'extraction et le SLA, pas la donnée.

**Et il reste une API d'État directement utile — c'est elle qui a été retenue.**
L'**API Bulletin Vigilance** (`portail-api.meteofrance.fr`, produit
`DonneesPubliquesVigilance`) donne le niveau d'alerte **par département** : orage,
vent, canicule, pluie-inondation, neige-verglas. 60 requêtes/minute, 99,9 % de
disponibilité, compte et jeton requis, Licence Ouverte 2.0 — donc **réutilisable
commercialement**, ce qui est précisément ce qui manquait à Open-Meteo.

Pour « faut-il déplacer ce cours en plein air », une vigilance orange est le signal
**le plus décisif** qui existe, et c'est celui sur lequel un gérant décide
réellement. Elle ne dit pas « 43 % de pluie jeudi à 18 h » — mais la question à
laquelle le produit doit répondre aujourd'hui n'est pas « quel temps fera-t-il »,
c'est « dois-je annuler ». Elle y répond, officiellement, gratuitement et sans
problème de licence.

### La chaîne retenue — décidée le 2026-08-07 : la vigilance, pour commencer

**Décision : on prend le Bulletin Vigilance de Météo-France, et on verra plus tard
si on peut faire mieux.** C'est une API d'État, sous Licence Ouverte 2.0 — donc
**réutilisable commercialement**, ce qui referme d'un coup le problème de licence
d'Open-Meteo. 60 requêtes/minute, 99,9 % de disponibilité, compte et jeton requis.

**Et ça simplifie le lot bien plus qu'il n'y paraît.**

> **La vigilance est départementale, pas ponctuelle. Il n'y a donc rien à
> géocoder.**

Le département, c'est le début du code postal — que l'application extrait **déjà**,
dans `SchoolZones.ForPostcode` (`postcode[..2]`), pour trouver la zone scolaire. La
chaîne tombe de deux appels à un :

```
code postal du lieu  ──▶  département  ──▶  Bulletin Vigilance  ──▶  couleur + phénomène
   (déjà en base)         (déjà extrait)     Météo-France            par département
```

**Conséquence directe : la question ouverte du volet météo disparaît.** Il n'y a
plus à trancher « géocoder à la volée ou stocker lat/lon sur `Location` », donc
plus de risque de toucher au modèle, donc la décision du lot 4 — « la météo se
branche sans rouvrir le lot » — est respectée sans effort. C'était la seule
question du lot qui engageait autre chose que du code jetable.

**La correspondance code postal → département : tranchée le 2026-08-07.**

Le code postal donne le département par ses deux premiers chiffres, sauf là où ça
ne marche pas : la Corse (`20` n'existe pas — c'est `2A` / `2B`, et la vigilance
les distingue) et les DOM (trois chiffres, `971`…`976`).

**Ce qui a été décidé : ni table complète, ni exception.** L'application ne sert
que la **Haute-Savoie** pour le moment, donc le `74` — et la règle des deux
chiffres y est exacte. Construire les 101 départements aujourd'hui serait payer
une exhaustivité dont personne ne se sert.

Et la liste utile **existe déjà** : `SchoolZones.Build()` énumère les 96
départements métropolitains (le `74` compris) pour en déduire la zone scolaire.
Reconnaître un département, c'est donc vérifier que le préfixe est dans un
ensemble qu'on a déjà sous la main — quelques lignes, pas une table.

**Sur l'exception, une nuance qui compte, et c'est le seul point où je diverge.**
Lever une exception est juste sur le principe — « je ne sais pas répondre » ne doit
pas se déguiser en « rien à signaler » — mais **pas à cet endroit**. Ce service est
appelé pendant le rendu d'un écran : une exception y casse la page, et le lot qu'on
vient de livrer portait précisément sur un log où plus aucune exception ne traîne.
Une exception par rendu pour un code postal hors périmètre remettrait le bruit
qu'on vient de payer pour enlever.

**Donc : strict à l'écriture, indulgent à l'affichage.**

- **À l'affichage** — département non reconnu → le service rend **« indisponible »**,
  jamais « vert ». Pas d'exception, pas d'écran cassé, et rien qui ressemble à une
  absence d'alerte.
- **À l'écriture** — si on veut être strict, l'endroit utile est le **validateur**
  qui enregistre un lieu extérieur : refuser un code postal qu'on ne sait pas
  situer, au moment où quelqu'un le saisit et peut le corriger. C'est là qu'une
  erreur franche rend service ; en lecture, elle ne fait que casser.

Dire « pas d'alerte » quand on ne sait pas reste **le seul mode de panne
inacceptable de ce lot** — et c'est vrai quelle que soit la voie retenue.

**Ce qu'il faut écrire.** `IWeatherService` — ou plutôt `IVigilanceService`, puisque
c'est ce qu'il rend — sur le modèle exact de `ISchoolCalendarService` : interface
dans `Application/Interfaces`, implémentation côté WebApp, `IHttpClientFactory`
nommé, `IMemoryCache`, timeout court, tout échec avalé en « indisponible ». Deux
différences avec le calendrier scolaire, et elles comptent :

- **Le cache est court.** Le calendrier scolaire se met en cache 24 h parce qu'il
  ne bouge jamais ; une vigilance qui a 24 h ne vaut rien. Une heure au plus, et à
  décider au plan.
- **Il y a un jeton.** Même traitement que la clé Brevo : absente, le service se
  tait et l'écran n'affiche pas de vigilance — il ne tombe pas et ne ment pas.

**Ce que la vigilance sait faire, et ce qu'elle ne sait pas.** Elle répond « orange
orage sur le Rhône », ce qui est **le signal le plus décisif** pour annuler ou
déplacer un cours en plein air — c'est officiel, et c'est ce sur quoi un gérant
prend réellement sa décision. Elle ne répond pas « 43 % de pluie jeudi à 18 h » :
pas de probabilité, pas d'heure précise, pas de température. L'écran doit donc
être écrit pour dire une alerte, pas une prévision — et surtout ne pas promettre
une météo qu'il n'a pas.

### Quand on voudra faire mieux

La marche suivante est une vraie prévision au point, et le travail de recherche est
déjà fait, autant qu'il serve :

- **Le géocodage sera nécessaire à ce moment-là, et il est tranché** :
  Géoplateforme (`data.geopf.fr/geocodage`), sans clé, Licence Ouverte 2.0, testé
  ci-dessus sur l'adresse réelle de GymXYZ.
- **Le fournisseur de prévision restera un choix commercial**, pas technique :
  Open-Meteo est gratuit **en usage non commercial seulement**, et payer son
  abonnement revient à payer l'extraction du point — pas la donnée, puisqu'il sert
  déjà `meteofrance_arome_france_hd`. L'alternative est de décoder AROME soi-même.
- **L'interface protège le changement** : c'est très exactement pour ça qu'on
  écrit `IVigilanceService` derrière `Application/Interfaces` plutôt que d'appeler
  Météo-France depuis un composant.

### Les URL d'API vont dans `appsettings`

**Demandé le 2026-08-07, et ça vaut aussi pour l'existant.** Les deux URL du
calendrier scolaire sont aujourd'hui **écrites en dur** dans
`SchoolCalendarService.cs` (lignes 93 et 126-127). Ce lot les sort et pose les
nouvelles au même endroit, plutôt que d'ajouter deux constantes de plus au corps
d'un service.

Le gabarit est déjà écrit : `EmailOptions` (`SectionName`, valeurs par défaut dans
la classe, liées par `builder.Services.Configure<EmailOptions>(…)` à
`Program.cs:41`) et `TenantOptions`. Le même schéma donne quelque chose comme une
section `ExternalApis` portant les quatre URL — jours fériés, calendrier scolaire,
géocodage, prévision — plus les réglages qui les accompagnent naturellement :
timeout, durée de cache, et l'éventuelle clé du fournisseur de prévision.

Ce que ça achète, concrètement : une URL qui bouge — et une API publique bouge de
version, `v2.0` → `v2.1` en est déjà l'illustration — devient un changement de
configuration au lieu d'un déploiement. Ça permet aussi de **pointer les tests ou
le développement vers un faux serveur** sans toucher au code, ce qui est la seule
façon propre de démontrer un mode de panne.

**Deux limites à respecter**, sinon le remède coûte plus qu'il ne rapporte :
garder des **valeurs par défaut dans la classe d'options** — l'application démarre
sans qu'on ait rien à configurer — et **ne pas mettre la clé du fournisseur dans
un fichier versionné** : user-secrets ou variable d'environnement, exactement comme
`EmailOptions.ApiKey` le dit déjà.

**À trancher au plan :**

- **Faut-il refuser un code postal non situable à l'écriture** d'un lieu extérieur,
  ou seulement rendre « indisponible » à la lecture ? La lecture est tranchée ;
  l'écriture est un choix de rigueur, pas une nécessité — le périmètre est la
  Haute-Savoie.
- **La fenêtre.** La vigilance porte sur aujourd'hui et demain. Le Planning
  affiche une semaine : les cinq autres jours n'ont **rien** à montrer, et l'écran
  doit le dire sans laisser croire à une absence d'alerte.
- **Le repli se propose ou s'applique ?** Le lot 5 a posé que **rien ne déplace
  une séance sans qu'un humain le dise**. « Propose » est donc la réponse par
  défaut — reste à dire *où* la proposition apparaît et ce qu'il advient d'un refus.
- **Quels phénomènes comptent.** La vigilance en couvre plusieurs (vent, orages,
  pluie-inondation, canicule, neige-verglas…). Tous ne déplacent pas un cours de
  plein air de la même façon, et une pastille orange qui parle de submersion
  marine à Lyon 3ᵉ ferait perdre confiance au reste.
- **Le seuil d'affichage.** Le vert se montre-t-il, ou seulement à partir du
  jaune ? Afficher « rien à signaler » a une valeur — ça prouve que le service
  répond — mais ça remplit l'écran de bruit sept jours sur sept.
- **La durée du cache**, plus courte que les 24 h du calendrier scolaire.

---

## Masquer la cloche

Elle est à deux endroits, et le point rouge y est **écrit en dur** — il est allumé
en permanence et n'annonce rien :

- `TechXyz.GymXyz.WebApp/Components/Layout/Topbar.razor:11` (desktop) ;
- `TechXyz.GymXyz.WebApp/Components/Layout/MainLayout.razor:40` (mobile).

L'en-tête de `Topbar.razor` l'assume déjà (« present but inert at this lot »).
Masquer, ici, veut dire **retirer le fragment**, pas le passer en `display:none` :
un élément caché en CSS reste dans le DOM, dans l'ordre de tabulation et dans les
tests de balisage.

C'est la seule chose que ce lot ne peut pas laisser derrière lui : **une cloche
dit quelque chose de vrai, ou elle n'est pas là.** Un point rouge permanent
apprend aux gens à ne plus regarder les points rouges.

---

## Le réglage « afficher les vacances scolaires »

Idée du 2026-08-07 : chaque salle — ou chaque coach indépendant — décide si elle
veut voir les vacances scolaires. Elle est juste, et le terrain est déjà préparé.

**Ce qui existe.** `GymSettings.SchoolZone` (`GymSettings.cs:47`) est déjà stocké,
déduit du code postal par `SchoolZones.ForPostcode`, et le panneau Identité de
Réglages porte déjà une carte « calendrier scolaire » qui suit le code postal en
cours de saisie (`IdentitePanel.razor:12`). Le réglage se pose donc **dans un
panneau qui parle déjà de ça**, à côté de la zone — pas dans un nouvel écran.

**Pourquoi ça vaut le coup.** Leyssa Coaching est un coach solo qui entraîne des
adultes : les vacances scolaires ne lui disent rien, et la bande jaune sur ses sept
jours est du bruit permanent. Une salle avec des cours enfants, elle, en vit.

**Ce qu'il faut distinguer, et c'est le cœur du réglage.** Les **jours fériés** et
les **vacances scolaires** ne se valent pas : un jour férié change les horaires
d'ouverture de n'importe quelle salle, les vacances ne concernent que celles dont
le public suit le rythme scolaire. Le réglage demandé porte sur **les vacances**.
Les rendre coupables ensemble éteindrait aussi le férié, ce que personne n'a
demandé. Le lot 11 a d'ailleurs déjà séparé les deux à l'affichage : `.ferie` et
`.vac` partagent la rampe `warning` et c'est **l'icône** (étoile / soleil) qui les
distingue — donc deux choses distinctes rendues par le même composant, exactement
ce qu'il faut pour n'en éteindre qu'une.

**À trancher au plan :**

- **La valeur par défaut.** Afficher, ou masquer ? Le produit affiche aujourd'hui,
  et un défaut qui change ce que voient les clients existants est un choix, pas un
  détail.
- **La portée.** `GymSettings` est par client (`ITenantScoped`), donc c'est un
  réglage de salle, pas de personne — ce qui correspond à la demande. Un réglage
  par utilisateur serait une préférence, et le 01-LOTS a déjà écarté ce genre-là
  (écart n°5, densité d'affichage).
- **Où il agit.** Le bandeau du Planning au minimum. À vérifier : l'Accueil et la
  vue jour lisent-ils le même calendrier ?
- Un booléen de plus sur `GymSettings` est **additif** — pas de migration
  aujourd'hui (`EnsureCreated`, entrée 5 du registre), mais la valeur par défaut
  doit être écrite dans le seed comme dans le code.

---

## Critère d'acceptation

- La vigilance est appelée **pour de vrai** — le réseau sortant marche ici, donc
  une capture montrant une vraie vigilance est atteignable et attendue. Sans jeton,
  le service se tait ; il ne tombe pas.
- Un lieu extérieur météo-dépendant montre son niveau **là où quelqu'un le lit
  avant de décider**, et le service tombe sans casser l'écran : coupé, il rend
  « indisponible » comme le calendrier scolaire, jamais une exception au rendu.
- **Le `74` marche, et un code postal hors périmètre rend « indisponible », jamais
  « vert ».** Les deux à vérifier explicitement — un code postal corse suffit pour
  le second. Dire « pas d'alerte » quand on ne sait pas est la seule panne que ce
  lot ne peut pas se permettre. **Et aucune exception ne remonte au rendu** : le
  log reste aussi propre que le lot précédent l'a laissé.
- **L'écran ne promet pas une prévision.** La vigilance dit une alerte sur deux
  jours ; les cinq autres jours de la semaine ne montrent rien et le disent.
- Le repli est **proposé** — ou appliqué si le plan le décide et l'écrit — mais
  jamais silencieusement.
- **Aucune URL d'API n'est plus écrite en dur**, calendrier scolaire compris, et
  l'application démarre toujours sans configuration grâce aux valeurs par défaut.
  Aucune clé dans un fichier versionné.
- **La cloche a disparu** des deux shells, balisage compris.
- **Le réglage des vacances fait ce qu'il dit** : vérifié sur deux clients, dont un
  qui l'a éteint — et **le jour férié reste visible** quand les vacances sont
  éteintes.
- `dotnet test` vert, **zéro warning** dans les projets applicatifs. Le lot du log
  vient de ramener `TechXyz.GymXyz.WebApp` à zéro ; ne pas l'y remettre.
- **Le log reste lisible.** C'est le bénéfice qu'on vient de payer : une exception
  nouvelle s'y voit désormais. Elle se traite ou se consigne, elle ne se tolère pas.

---

## Pièges déjà payés

- **Avant de croire à un écran vide, lire `window.innerWidth`.** Le volet
  navigateur rapporte parfois **0** ; `ResponsiveModeService` lit cette largeur, un
  0 passe sous le seuil mobile, et la grille bureau se rend vide. **Attendre ne le
  lève pas** — un écran est resté vide 100 secondes ainsi, et ça a coûté une entrée
  de registre ouverte pour rien (entrée 6, retirée le jour même) **plus** une
  affirmation fausse sur le réseau dans le brief précédent. Forcer 1440 × 900, puis
  recharger.
- **Reproduire sur `main` non modifié prouve la reproductibilité, pas la
  réalité.** Si l'instrument est faussé, il l'est des deux côtés.
- **Deux contre-épreuves ne dépendent d'aucune mise en page** et tranchent en une
  commande : le HTML servi en SSR (`curl`), et le compte de lignes en SQL. Les
  utiliser **avant** d'écrire une entrée de registre.
- **La base de dev est recréée au démarrage** (`ResetDatabaseOnStartup`) et
  déconnecte tout le monde.
- **Un seul serveur peut tenir le port 5173.**
- **Le volet ne transmet aucun clic.** Poster les formulaires en JS. Pour la
  connexion, poser `.value` **sur les `fluent-text-field` eux-mêmes** : ce sont des
  éléments associés au formulaire qui postent leur propre valeur vide, donc un
  `input` caché ajouté à côté entre en collision et le serveur ne lie rien. Comptes
  de démo et mot de passe dans `DbInitializer`.
- **Un symptôme intermittent ne se juge pas sur une exécution.**

---

## Ce qui n'est pas dans ce lot

- **La cloche de notifications** — reportée hors V1 « pour le moment ». Quand elle
  reviendra, elle commencera par la question qu'elle n'a pas tranchée : une
  notification est-elle dérivée des alertes déjà calculées, ou stockée avec un état
  de lecture ? Et il faudra un écran de liste, que la maquette ne porte pas.
- **Point 3 — diffusion du planning en image.** Le bouton reste désactivé avec sa
  raison aux trois endroits (Accueil desktop, Accueil mobile, tête du Planning) ;
  les trois s'activent ensemble, et pas ici.
- **Points 4 et 5 — login & onboarding, portail super-admin.** Handoffs à fournir,
  et le 01-LOTS interdit de les commencer par morceaux en attendant.
- **Point 6 — comptes à casquettes multiples** (entrée 4 du registre).
- **Entrée 5 — migrations EF**, en dernier, juste avant un déploiement.
- **La Corse et les DOM** dans la correspondance des départements. Le périmètre
  est la Haute-Savoie ; la table complète se fera quand un client sortira de
  métropole, et le code doit rendre ça facile sans le préparer aujourd'hui.
- **SMS**, **recherche globale**, **portail membre** : hors V1, arbitrés le
  2026-08-07.

---

## Premier geste suggéré

1. **Sortir les URL dans `appsettings`** en commençant par les deux du calendrier
   scolaire, qui existent déjà et se déplacent sans rien casser. C'est la partie
   sûre, elle donne l'emplacement où poser les deux nouvelles, et elle se relit en
   une minute.
2. **Retirer la cloche** des deux shells — l'autre partie sûre.
3. **Demander le compte Météo-France** et son jeton — c'est la seule dépendance
   externe du lot, elle ne dépend pas de nous, et elle bloque la démonstration si
   on s'y prend le dernier jour.
4. **Écrire la correspondance code postal → département**, limitée à la
   métropole en réutilisant la liste que `SchoolZones` porte déjà, et son test :
   le `74` répond, un code postal hors périmètre rend « rien », et rien ne lève.
   Petite, isolée, et c'est elle qui porte le seul mode de panne inacceptable
   du lot.
5. **Écrire `IVigilanceService` en copiant le contrat de `SchoolCalendarService`**
   — timeout court, cache **court**, échec avalé, silencieux sans jeton.
6. **Décider où la vigilance se lit**, quels phénomènes comptent, si le vert
   s'affiche, et si le repli se propose ou s'applique.
7. **Poser le réglage des vacances à côté de la zone**, dans le panneau Identité,
   et trancher sa valeur par défaut.

Revenir avec un plan qui dit **quels phénomènes sont retenus et à partir de quelle
couleur on affiche**, ce que rend un département inconnu, et quelle valeur par
défaut prend le réglage des vacances. **Attendre la validation avant d'écrire.**
