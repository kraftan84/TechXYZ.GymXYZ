# Un log qu'on peut lire · brief de démarrage

Écrit le 2026-08-07, après la fusion de la PR 32. `main` est à `2fe1559`,
**564 tests au vert**, lots 0 à 11 livrés plus le lot « Rôles & cloisonnement ».

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

**C'est le point 1 de la première version** (`design_handoff_gymxyz/01-LOTS.md`) :
l'entrée 3 du registre de dette, et les deux warnings de compilation du WebApp.

---

## Pourquoi maintenant, alors que rien ne casse

L'entrée 3 dit elle-même que son échéance est « aucune tant qu'elle reste
invisible ». Elle passe devant pour une raison de calendrier, pas d'aggravation :
les deux chantiers suivants sont **login & onboarding** puis **portail
super-admin**, et ce sont ceux où il faudra lire le log pour de bon. Un log qui
contient déjà une exception connue et tolérée est un log dans lequel personne ne
voit arriver la suivante.

Autrement dit, **le livrable n'est pas « corriger un bug » mais « rendre le log
utilisable »**. Si l'enquête conclut que le message est inoffensif et que le
remède coûte plus qu'il ne rapporte, c'est une conclusion recevable — à condition
qu'elle soit écrite dans le registre avec ce qui l'établit. Ce qui n'est pas
recevable, c'est de la refermer sans savoir.

---

## Ce qui est su, et qui a déjà été mesuré

`System.InvalidOperationException: Connection must be valid and open`, levée dans
`MySql.Data`, **toujours sur la même requête** : celle de `GetTenantBrandQuery`
(`SELECT ... FROM Tenants WHERE Slug = 'gymxyz'`). Aucune autre requête ne tombe.

Le correctif de l'entrée 1 (fabrique de contexte) ne la change pas :

| Build | `A second operation was started` | `Connection must be valid and open` |
|---|---|---|
| `main` sans le correctif | absent sous cette charge | **présent** |
| avec la fabrique | absent | **présent, à l'identique** |

Même charge de part et d'autre : connexion, puis 32 chargements de page lus
jusqu'au bout sur les huit écrans, plus des navigations réelles (circuit
interactif) sur Planning, Présences, Membres et Administration. **C'est le
protocole à reprendre** — il a déjà servi à départager les deux messages, il
départagera les hypothèses.

Rien à l'écran ne le laisse voir : le log fait suivre l'échec d'un
`Executed DbCommand` **réussi** sur la même requête. C'est pour ça qu'il a vécu
plusieurs lots sans que personne le remarque autrement qu'en le lisant.

---

## Ce qui n'est pas su : la cause

Le registre nomme deux pistes non départagées. Le code en ajoute une troisième,
et donne de quoi les instruire toutes les trois.

**1. Le connecteur.** La persistance utilise `MySql.EntityFrameworkCore` 10.0.1
— le fournisseur d'Oracle, donc `MySql.Data`, appelé par `options.UseMySQL(...)`
dans `IServiceCollectionExtensions.cs:49`. C'est exactement l'assembly d'où
l'exception est levée. L'alternative connue est Pomelo, qui repose sur
`MySqlConnector`. **Mais changer de fournisseur est une décision, pas un
correctif** : c'est une dépendance de la couche de persistance, et ça demande la
suite complète plus un passage navigateur. À ne proposer qu'appuyé sur une
mesure, pas sur une réputation.

**2. La double passe.** `TenantBoundary.OnInitializedAsync` s'exécute deux fois —
prérendu, puis circuit — et appelle `TenantResolver.ResolveAsync` à chaque fois.
Le `TenantResolver` est `Scoped` et porte un `SemaphoreSlim` avec un cache
(`_resolvedSlug` / `_resolvedBrand`, `TenantResolver.cs:17-19`), donc il ne pose
la question qu'une fois **par scope** — mais le prérendu et le circuit sont deux
scopes. La requête de marque part donc au minimum deux fois par chargement de
page, et c'est la seule qui ait ce comportement. Ça n'explique pas l'exception à
soi seul ; ça explique pourquoi elle ne frappe que celle-là.

**3. Le second appelant, qui n'est pas dans le registre.**
`GetTenantBrandQuery` est envoyée depuis **deux** endroits : `TenantResolver`
(protégé par le gate) et `Planning.razor:153`, dans `LoadReferencesAsync`, à la
suite de trois autres requêtes et **sans gate**. C'est le seul écran qui
redemande la marque pour son compte. À vérifier tôt : si le message est plus
fréquent sur Planning que sur les sept autres écrans, la piste change de tête de
liste. S'il tombe autant partout, cet appel n'est pas la cause — mais il reste à
justifier, puisque la marque est déjà cascadée.

**Consigne du registre, à respecter** : ne pas refermer l'entrée sans avoir
tranché. « Corrigé » sans savoir pourquoi ne vaut rien à la personne qui reverra
le message.

---

## Les deux warnings

Ils partent avec, parce qu'ils sont deux lignes et qu'ils touchent le même
terrain.

- **`CS8604` — `Planning.razor:153`** : `Tenant.Slug` possiblement nul passé à
  `GetTenantBrandQuery`. C'est **la ligne du second appel** ci-dessus : si cet
  appel disparaît, le warning disparaît avec lui, et l'inverse n'est pas vrai.
  Les traiter ensemble, pas l'un après l'autre.
- **`BL0008` — `Login.razor:72`** : propriété `[SupplyParameterFromForm]` avec
  initialiseur, qui peut être écrasée par un `null` au post. Elle touche l'écran
  de connexion, que le point 4 de la V1 va rouvrir. Le corriger ici évite de le
  transporter dans un chantier qui aura ses propres questions.

Aucun autre warning dans les projets applicatifs ; les quatre restants sont dans
les tests (`CS8602`/`CS8604` sur `AttendanceQueryTests` et
`NotificationOutcomeLabelsTests`). Les prendre ou non est un arbitrage à poser au
plan — ils sont sans risque, mais un projet de test qui compile sans bruit rend
service au suivant.

---

## Critère d'acceptation

- La cause est **nommée**, et ce qui l'établit est écrit dans l'entrée 3 — que
  l'issue soit un correctif ou un classement sans suite argumenté.
- Le protocole de charge ci-dessus rejoué : **plus aucune occurrence** du message
  si l'entrée est fermée par un correctif ; sinon, le compte exact et ce qui le
  rend inoffensif.
- `dotnet test` vert, et **zéro warning** dans `TechXyz.GymXyz.WebApp`.
- L'entrée 3 est fermée ou réécrite dans `LOT-13-BRIEF.md`, à la manière des
  entrées 1 et 2 : ce qui a été fait, ce qui a été vérifié, ce que la fermeture
  **ne couvre pas**.

---

## Pièges déjà payés

- **La base de dev est recréée au démarrage** (`ResetDatabaseOnStartup`) et
  déconnecte tout le monde. Une session ouverte ailleurs meurt avec.
- **Un seul serveur peut tenir le port 5173.** Deux sessions ne cohabitent pas.
- **Le volet navigateur ne transmet aucun clic.** Poster les formulaires en JS
  avec leur vrai jeton antiforgery ; le formulaire de connexion utilise des
  `fluent-text-field` dont les champs s'appellent `Input.Email` et
  `Input.Password`.
- **Les écrans rendent un état vide avant l'arrivée des données.** Attendre la
  stabilisation avant de conclure — ça a failli faire déclarer une régression
  inexistante au lot 11.
- **Un symptôme intermittent ne se juge pas sur une exécution.** L'entrée 1 a
  été départagée en comparant deux builds sous la **même** charge ; « je ne le
  vois plus » n'est pas une mesure.
- **Vérifier contre `main` non modifié** avant de croire qu'un correctif a fait
  disparaître un symptôme.

---

## Ce qui n'est pas dans ce lot

- **L'entrée 4** (comptes à casquettes multiples) — point 6 de la V1, après les
  deux handoffs.
- **L'entrée 5** (migrations EF) — en dernier, juste avant un déploiement.
- **La cloche et la météo** — point 2, avec leurs propres décisions ouvertes.
- **Login & onboarding**, **portail super-admin** — handoffs à venir. Le warning
  de `Login.razor` est pris ici parce que c'est une ligne ; **l'écran, non**.

---

## Premier geste suggéré

1. **Reproduire et compter.** Lancer le serveur, rejouer le protocole de charge,
   et relever le nombre d'occurrences **par écran** — c'est ce chiffre qui
   départage la piste 3 des deux autres, et il n'existe pas encore.
2. **Instrumenter la double passe** : savoir si les deux envois de
   `GetTenantBrandQuery` par chargement viennent bien de deux scopes, et si
   l'exception tombe sur l'un des deux en particulier.
3. **Ne pas toucher au fournisseur** avant d'avoir ces deux mesures.
4. Revenir avec un plan qui dit **quelle hypothèse est retenue, sur quelle
   mesure**, et ce que coûte le remède correspondant.

Attendre la validation avant d'écrire.
