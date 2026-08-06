# Lot 10 — Accueil / tableau de bord · brief de démarrage

Écrit le 2026-08-06, juste après la fusion du lot 9. `main` est à `a864353`,
**465 tests au vert**, lots 0 à 9 livrés.

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

---

## À lire avant tout le reste

**Les docs promettent 4 KPI que le prototype ne dessine pas.** C'est le premier
écart à trancher, et il ne se tranche pas comme celui du lot 9.

`01-LOTS.md` (§Lot 10) annonce « salutation + date, **4 KPI**, carte semaine… ».
`03-SCREENS-DESKTOP.md` §1 va plus loin et les décrit : « label en petites
capitales, valeur `--text-2xl` bold en chiffres tabulaires, delta coloré avec
flèche », plus la carte spark et son filet dégradé.

`design/app/screen-accueil.jsx` n'en contient aucun. L'écran est **planning-first**,
et son en-tête le dit dans son propre commentaire :

> *GymXYZ — Screen: Accueil (planning-first dashboard). Home → click a day → day detail.*

Ce qu'il dessine réellement, dans l'ordre :

1. `PageHead` « Bonjour <prénom> » avec deux actions — **Aperçu** et **Diffuser le planning** ;
2. la **carte semaine** : `WeekStrip` des 7 jours, puis un pied `gx-diffuse-foot`
   « Dernière diffusion : dimanche dernier · vos membres notifiés » ;
3. deux colonnes `1.4fr 1fr` : « Aujourd'hui · 3 cours » (`gx-classrow`) et
   « À surveiller » (`gx-alert`).

`mobile/screen-accueil.jsx` dit la même chose : titre, CTA « Diffuser le
planning », carte semaine, cours du jour, alertes. **Pas de tuiles KPI 2×2** non
plus, alors que `01-LOTS.md` en demande.

**Ici la règle habituelle tranche.** Contrairement au lot 9 — où docs et
maquette décrivaient deux écrans différents — les deux décrivent bien *le même*
écran, en désaccord sur son contenu. Le prototype gagne : **pas de KPI sur
l'Accueil**. À confirmer d'un mot, mais sans en faire une question ouverte.

À noter pour couper court au doute : `.gx-kpi` (8 règles) et `.gx-m-kpi` (11)
existent bien dans le CSS et servent déjà — aux Présences et aux Abonnements.
Leur présence ne prouve donc rien sur l'Accueil.

### L'écart va aussi dans l'autre sens

Le prototype contient un **écran de détail de journée** — `AccueilDay`, lignes
100–150 — que ni `01-LOTS.md` ni `03-SCREENS-DESKTOP.md` ne mentionnent. On y
arrive en cliquant un jour de la bande. Il porte un fil d'Ariane, trois actions
(Accueil, Jour suivant, Cours), un « Déroulé de la journée » en frise (`gx-tl`,
9 règles CSS déjà présentes), et deux cartes latérales — « Cette semaine » et
« Relances ».

C'est un vrai écran, pas une variante. **Trois lectures, à faire arbitrer :**

| Lecture | Ce que ça donne |
|---|---|
| **A — Accueil seul** | La bande de semaine navigue vers le Planning sur le jour choisi, comme le fait déjà le mobile (`onNavigate('planning')`). Le lot reste petit et rien n'est perdu : le Planning sait déjà afficher un jour. |
| **B — Accueil + détail de journée** | Fidèle au prototype desktop. Mais la frise recouvre largement la vue jour du Planning livrée au lot 5, et ses deux cartes latérales dupliquent la semaine et les alertes déjà présentes sur l'Accueil. |
| **C — Accueil, et la frise remplace la vue jour du Planning** | Le plus cohérent à l'arrivée, le plus intrusif : rouvre un écran livré. |

Recommandation à défendre au moment du plan : **A**, parce que le détail de
journée du prototype est en grande partie une redite d'un écran déjà livré, et
que la maquette mobile a elle-même tranché dans ce sens.

---

## Ce qui bloque, et qui ne se règle pas dans ce lot

### `LOT-13-BRIEF.md`, entrée 1 — un seul `DbContext` par circuit

**L'entrée porte l'échéance « avant le lot 10 », et ce n'est pas décoratif.**

Le lot 10 est précisément le lot qui va cogner dessus : `01-LOTS.md` demande
« **une seule requête aller-retour**, projection server-side », c'est-à-dire un
handler qui enchaîne le comptage de la semaine, les cours du jour et trois
agrégats d'alertes. Sur un contexte partagé par tout le circuit, c'est le profil
exact qui a fait échouer la console du lot 9 PR 2.

Le registre chiffre le correctif et montre qu'il est petit : les 77 fichiers
qui prennent `IGymDbContext` ne changent pas, seul l'enregistrement change.
**La PR 23 qui ouvre le registre n'est pas encore fusionnée** — à faire avant de
planifier ce lot.

Si la décision est de ne pas le corriger d'abord, alors le lot 10 doit prendre
son propre scope DI comme l'a fait `Administration.razor`, et le dire.

### `LOT-13-BRIEF.md`, entrée 2 — le `PlatformAdmin` sans client

Moins bloquant ici, mais l'Accueil est la **première** page qu'un super-admin
voit en se connectant : c'est là que « Bonjour <prénom> » et les données de
GymXYZ s'afficheront pour quelqu'un qui n'appartient à aucun client. Si l'entrée
2 est traitée avant, l'Accueil doit savoir quoi dire à un admin sans client
sélectionné — voir les options A/B/C du registre.

---

## Périmètre

**Sources** : `01-LOTS.md` §« Lot 10 » (lignes 413–441),
`03-SCREENS-DESKTOP.md` §1 (lignes 58–75),
`design/app/screen-accueil.jsx` (152 lignes, `ScreenAccueil` puis `AccueilDay`),
`design/app/mobile/screen-accueil.jsx` (84 lignes),
`data.js` → `GX_DATA.week`, `GX_DATA.todayClasses`, `GX_DATA.alerts`,
`GX_DATA.weekRange`.

**Application** : `GetDashboardQuery` → un seul DTO agrégé.

**Ce que le lot ne réinvente pas.** C'est tout l'intérêt de l'avoir décalé en
fin de parcours : les trois alertes se calculent avec des règles déjà tranchées
et déjà testées.

| Alerte du prototype | Ce qui existe déjà |
|---|---|
| « 4 abonnements expirent » | `SubscriptionStatusRules.Matches` + `HorizonFrom` (lot 7) |
| « 2 paiements en retard — 180 € » | `SubscriptionStatusRules` / `PaymentRules` (lot 7) |
| « Présences d'hier — 3 cours à pointer » | `AttendanceKpisDto.SheetsToPoint`, déjà calculé par `GetAttendanceOverviewQuery` (lot 6) |

**Ne pas recalculer ces règles dans un nouveau helper.** Si un chiffre de
l'Accueil diffère de celui de l'écran correspondant, c'est l'Accueil qui a tort,
et le lot 10 aura introduit deux vérités pour un même fait.

---

## « Diffuser le planning » — la décision attendue a une conséquence précise

`01-LOTS.md` la pose déjà. Ce que le brief ajoute, c'est ce qu'elle coûte :

**Il n'existe aucune clé de notification pour la diffusion.**
`NotificationKey` en compte six — `RenewalReminder`, `LatePayment`,
`NewRegistration`, `CourseReminder`, `SeatFreed`, `CourseCancelled` — et la
diffusion du planning n'en est pas. Or son commentaire dit pourquoi ça compte :
chaque envoi demande à ce modèle s'il a le droit de partir, et une clé absente
se lit comme « éteint ».

Donc un envoi réel demande, au minimum :

- une **septième clé** dans `NotificationKey` ;
- une **septième bascule** dans le panneau Notifications du lot 8, avec ses
  canaux — donc rouvrir un écran livré ;
- une **liste de destinataires** que personne n'a définie : tous les membres ?
  ceux qui ont un compte ? ceux qui ont une adresse ?

Recommandation à défendre : **ne pas envoyer au lot 10.** Enregistrer la
diffusion (une date sur le tenant ou une entité dédiée), afficher « Dernière
diffusion : … » — le prototype ne montre rien de plus — et laisser l'envoi réel
au lot qui possédera la septième clé. Le bouton reste actif et fait quelque
chose de vrai ; c'est le canal qui attend.

Le bouton **« Aperçu »**, à côté, n'est câblé à rien dans le prototype. Le
traiter comme la carte « Logo » du lot 9 : présent, désactivé, avec un `title`
qui dit pourquoi.

---

## À câbler ici, laissé inerte depuis le lot 0

- **Le compteur Présences** de la navigation. La plomberie existe :
  `AttendanceBadgeService` est publié par l'écran Présences et lu par `Sidebar`
  et `MobileTabBar`. Aujourd'hui le badge n'apparaît donc **qu'après** un
  passage par les Présences. L'Accueil est l'écran qui doit l'alimenter au
  premier chargement — c'est exactement ce que le commentaire du service décrit
  (« publié par l'écran qui a déjà lancé cette requête »).
- **Le compteur Abonnements**, que `03-SCREENS-DESKTOP.md` dessine (`badge 6`)
  et que rien ne remplit. `AttendanceBadgeService` ne porte qu'un compteur :
  soit on le généralise, soit on en ajoute un second sur le même patron. À
  trancher au plan — le patron est bon, c'est le nom qui devient faux.

---

## Ce qui existe déjà et fera gagner du temps

- **Tout le CSS du lot 10 est là**, vérifié classe par classe :
  `gx-weekstrip` (2), `gx-day` (+ `.on`, `.ferie`, `.vac`, `gx-day-mark`),
  `gx-classrow` (11), `gx-alert` (7), `gx-diffuse-foot` (3), `gx-tl` (9, pour le
  détail de journée si la lecture B est retenue). Mobile : `gx-m-weekstrip` (2),
  `gx-m-day` (10), `gx-m-class` (10), `gx-m-alert` (10), `gx-m-diffuse` (3).
  **Markup seul**, comme aux lots 4 à 9.
- **`GetWeekPlanningQuery` (lot 5)** sait déjà lire une semaine de séances : la
  bande des 7 jours et les cours du jour en sortent, sans nouvelle requête à
  inventer.
- **`Kpi.razor`, `GxBar`, `GxChip`, `PageHead`, `Crumb`, `EmptyState`** sont
  livrés et utilisés partout.
- **`ISchoolCalendarService`** alimente déjà les marques fériés/vacances de la
  bande — le prototype appelle `gxDayInfo(cal, date)` exactement comme le
  Planning du lot 5, qui l'a porté.
- **`ResponsiveModeService`** et le patron page/présentation des lots 8 et 9 :
  une page qui possède l'état, deux présentations qui ne prennent que des
  paramètres.

---

## Pièges déjà payés sur les lots précédents

- **`GxIcon` prend `Class`, jamais `Style`** — sinon exception au rendu, pas à la
  compilation (attrapé au lot 9 PR 2).
- **`[SupplyParameterFromForm]` sur un modèle sans champ** le met à null au post
  et `EditForm` lève (lot 9 PR 2, même famille que l'avertissement `BL0008`).
- **Un champ Fluent est deux éléments frères** : l'emballer dans un `<div>` dans
  une grille CSS (lot 8).
- **Les attributs ARIA d'état écrits en toutes lettres** : Blazor supprime
  l'attribut quand le bool est faux (commit `bf67bdb`).
- **`ExecuteUpdateAsync` ne marche pas** sur le provider InMemory des tests —
  charger et muter (lot 7).
- **La base de dev est recréée au démarrage** (`ResetDatabaseOnStartup`) et
  déconnecte l'utilisateur. Prévoir de se reconnecter.
- **Le volet navigateur ne transmet aucun clic.** Contournement qui a marché aux
  lots 8 et 9 : poster les formulaires en JS avec leur vrai jeton antiforgery, et
  déclencher les gestionnaires Blazor par `element.click()`. **Dire dans la PR ce
  qui n'a pas pu être atteint.**
- **Un écran qui charge en `OnParametersSetAsync` peut afficher son état vide
  avant sa réponse.** L'Accueil est l'écran le plus exposé : « Aucun cours
  aujourd'hui » affiché pendant le chargement est un mensonge. Le lot 9 PR 2 a
  posé le patron `IsLoaded`.

---

## Ménage à faire au passage

`TechXyz.GymXyz.WebApp/Components/Pages/Home.razor.js` **n'est importé par
personne** — vérifié : les seuls modules chargés sont `ReconnectModal.razor.js`
et `PlanningWeekGrid.razor.js`. C'est un reste de l'époque où le suivi de
défilement vivait sur l'Accueil. À supprimer, ou à réutiliser si la frise du
détail de journée en a besoin.

---

## Premier geste suggéré

1. Lire `screen-accueil.jsx` **en entier** — les deux écrans qu'il contient — et
   `mobile/screen-accueil.jsx`, puis constater par soi-même l'absence de KPI.
2. **Poser la question du détail de journée (A / B / C)** et faire confirmer
   l'abandon des KPI. Le reste du périmètre en dépend.
3. **Trancher l'ordre avec `LOT-13-BRIEF.md` entrée 1** : fabrique de
   `DbContext` d'abord, ou scope dédié pour cet écran et dette assumée.
4. Décider ce que fait « Diffuser le planning » en connaissant le prix d'un
   envoi réel — septième clé, septième bascule, liste de destinataires.
5. Revenir avec un plan qui fait atterrir `GetDashboardQuery` et ses règles
   réutilisées avant les deux présentations.

Attendre la validation avant d'écrire.
