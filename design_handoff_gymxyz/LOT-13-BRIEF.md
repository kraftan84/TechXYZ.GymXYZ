# Lot 13 — Dette technique & finitions · registre courant

Ouvert le 2026-08-06, à la fin du lot 9. `main` est à `c64245d` (lot 9 PR 1),
la PR 2 est en attente de fusion, **465 tests au vert**.

> **État au 2026-08-07, après le lot du log.** `main` est à `2fe1559`,
> **564 tests au vert**. Deux entrées ouvertes (**5**, **4**), trois fermées
> (3, 2, 1) et une **retirée** (6). L'entrée 3 est tombée au point 1 de la V1, à
> l'échéance annoncée. L'entrée 6 a été ouverte en la vérifiant puis retirée le
> jour même : les deux défauts qu'elle décrivait venaient du volet navigateur, pas
> de l'application — elle est gardée pour que la fausse alerte ne soit pas
> rapportée deux fois. L'échéance de l'entrée 4 a été fixée en arrêtant la
> séquence de la première version — elle y est le point 6, et
> `design_handoff_gymxyz/01-LOTS.md` porte cette séquence.

Ce document n'est pas un brief de démarrage comme les autres : c'est un
**registre qui se remplit au fil des lots**. Chaque lot découvre des choses
qu'il aurait été malhonnête de corriger en passant — parce qu'elles dépassent
son périmètre, parce qu'elles touchent toute l'application, ou parce qu'elles
demandent une décision produit. Elles s'écrivent ici au moment où on les voit,
avec ce qu'on savait à ce moment-là.

---

## Comment s'en servir

**À l'écriture d'une entrée** (pendant un lot, jamais après coup) :

- Ce qu'on a observé, et **où** — le fichier, l'écran, la commande qui le montre.
- Pourquoi ça n'a pas été corrigé sur place.
- La piste, avec un chiffrage.
- Le risque de la corriger, pas seulement celui de la laisser.

**Ce qui n'entre pas ici.** Le registre n'est pas un tiroir. N'y vont ni les
« ce serait mieux si », ni les préférences de style, ni ce qu'un lot à venir
couvre déjà. Une entrée décrit quelque chose qui **est faux ou dangereux
aujourd'hui**, avec de quoi le vérifier.

**Ce n'est pas une file d'attente.** Une entrée porte une échéance. « Avant le
lot 10 » veut dire avant le lot 10 : une dette qui produit déjà du bruit ne se
range pas en fin de programme sous prétexte qu'elle est ancienne.

---

## Entrées ouvertes

### 5. Aucune migration EF : le schéma est créé, jamais fait évoluer

**Ouverte le 2026-08-07**, en arrêtant la séquence de la V1. **Échéance fixée le
même jour : en dernier, juste avant un déploiement** — après les six points de
la V1, et hors d'eux.

C'est un arbitrage, et il tient à une condition : `EnsureCreated` ne coûte rien
tant que **personne d'autre que nous** ne remplit la base. Tant que l'onboarding
ne tourne qu'en développement, la base reste jetable et le modèle peut encore
bouger sans payer une migration par PR. Ce qui doit rester vrai jusque-là : **la
bascule se fait avant la première inscription réelle**, pas après. Le jour où une
donnée qu'on refuse de perdre est entrée, la migration initiale n'est plus prise
sur une base vide et l'entrée change de nature.

Cette entrée déroge à la règle du registre — elle n'est pas née d'un lot qui l'a
rencontrée, mais d'une décision du lot 0 qu'on écrit ici parce que la V1 vient de
lui donner une échéance. Elle est consignée comme telle, pas déguisée en
découverte.

**Observé.** `EnsureCreated` crée la base au démarrage et
`ResetDatabaseOnStartup` la recrée en développement. Aucune migration EF n'existe
dans `Persistence`. C'était le bon arbitrage tant que la base était jetable et
que le seul contenu était du jeu de démonstration : douze lots ont fait bouger le
modèle presque à chaque PR, et autant de migrations auraient été autant de
fichiers à relire pour rien.

**Ce qui change.** Les points 4 et 5 de la V1 — onboarding et portail super-admin
— créent des clients, des comptes et des données que personne n'accepte de perdre
au prochain démarrage. `EnsureCreated` ne sait pas faire évoluer un schéma
existant : il crée s'il n'y a rien, et ne fait rien sinon. La première
modification de modèle après la première vraie inscription est donc soit une
perte de données, soit du SQL écrit à la main.

**La piste.** Une migration initiale prise sur le schéma courant, puis le passage
de `EnsureCreated` à `Migrate` — en gardant `ResetDatabaseOnStartup` pour le
développement, qui reste utile et n'a pas à changer. Le travail n'est pas dans la
migration initiale, il est dans la bascule : le jour où on la fait, la base de
développement de chacun doit être recréée une fois, et le seed doit rester
rejouable.

**Le risque de la corriger.** Aucun sur le code applicatif. Le risque est de le
faire **après** l'onboarding plutôt qu'avant : à ce moment-là, la migration
initiale n'est plus prise sur une base vide mais sur une base qui contient déjà
un client réel.

### 4. Un compte ne peut porter qu'un client et qu'un rôle

**Ouverte le 2026-08-07**, pendant la PR 1 du lot « Rôles & cloisonnement ».
**Échéance précisée le 2026-08-07 : c'est le point 6 — et le dernier — de la
V1**, après les deux handoffs. L'entrée disait « avant qu'un vrai client ait deux
casquettes » ; l'onboarding et le portail super-admin sont précisément ce qui
crée des comptes pour de vrai, donc l'échéance tombe avec eux et pas après. Elle
passe **après** eux, et non avant, parce qu'elle touche la fabrique de claims, la
connexion et l'impersonation : la traiter d'abord obligerait à réécrire ce que
ces deux chantiers viennent de poser. Les deux handoffs doivent être écrits en
sachant qu'elle suit.

**Observé.** Najate Amzil est coach salariée chez Team Trainer's **et** gérante
de Leyssa Coaching. Le modèle ne sait pas l'écrire :

- `ApplicationUser.TenantId` est un `int?` **scalaire** (`ApplicationUser.cs:12`) ;
- le rôle Identity est porté **globalement** par le compte, pas par le couple
  compte × client (`AspNetUserRoles`) ;
- le cookie ne transporte qu'un `gymxyz:tenant_id`
  (`GymUserClaimsPrincipalFactory.cs:45-52`) ;
- `RequireUniqueEmail = true` (`Program.cs:94`) interdit même de réutiliser
  l'adresse.

**Ce qui a été fait à la place.** Deux comptes, deux adresses :
`najate.amzil@teamtrainers.fr` en `Coach`, `najate.amzil@leyssa-coaching.fr` en
`GymManager`. Elle se déconnecte pour changer de casquette. Le seed le pose et
`DbInitializerAccessTests.Seed_ShouldGiveNajateOneAccountPerHat` **l'épingle
comme un troc, pas comme une vérité** — le test dit ce qu'on abandonne le jour
où on le corrige.

**Pourquoi pas sur place.** C'est un lot, pas un correctif : une table
`UserTenantMembership (UserId, TenantId, Role)` remplace à la fois
`ApplicationUser.TenantId` et le rôle global, donc elle touche la fabrique de
claims, `TenantResolver`, l'impersonation du super-admin (qui est déjà une
forme de « changer de client »), `UserDirectory`, l'écran Équipe et la
connexion — plus un sélecteur de client à l'écran, qui est du dessin, pas du
code. Le glisser dans le lot rôles aurait fait passer le cloisonnement coach
derrière lui.

**Le risque de la corriger.** L'impersonation et l'appartenance deviennent le
même mécanisme sous deux noms. Si les deux ne sont pas unifiés d'un coup, on
obtient deux façons concurrentes de répondre « quel client suis-je en train de
lire », c'est-à-dire exactement la question dont dépend le filtre global.

## Entrées fermées

### 6. Deux écrans affichent un état vide ou une date absurde — **retirée**

> **Retirée le 2026-08-07, le jour même de son ouverture : les deux défauts
> n'existent pas.** L'entrée est gardée plutôt que supprimée parce qu'elle a
> déjà été rapportée une fois, et que ce qui l'a produite se reproduira.
>
> **Ce qui a été vérifié**, volet navigateur forcé à 1440×900 :
>
> - **Planning** affiche bien la grille de la semaine du 3 au 9 août — 13 blocs
>   à l'écran, la bande « Zone A · Vacances d'Été », et le trait de l'heure
>   courante au bon endroit.
> - **Présences** affiche « 7 août 2026 · 2 séances à pointer », un taux de
>   présence de 88 %, les deux feuilles du jour et les séances récentes. La date
>   est juste et le compte s'accorde à la pastille « 2 » de la navigation.
> - Le rendu SSR récupéré au `curl` était déjà complet et correct **avant même**
>   d'ouvrir un circuit : la grille entière y figure, sessions comprises.
>
> **Ce qui l'a produite.** Le volet navigateur rapportait
> **`window.innerWidth === 0`**. `ResponsiveModeService` lit cette largeur, un 0
> passe sous le seuil mobile, et l'écran bascule dans un état de mise en page
> dégénéré où la grille bureau se rend vide. Rien de tout cela n'appartient à
> l'application : ni la requête, ni les données, ni le rendu n'étaient en cause —
> la requête du planning renvoyait bien ses 16 séances, ce qui avait déjà été
> vérifié en SQL.
>
> **Ce qu'il faut en retenir.** Le registre avertissait déjà qu'un écran rend un
> état vide avant l'arrivée des données ; le piège ici est plus sournois, parce
> qu'attendre ne le lève pas — l'écran est resté vide 100 secondes. **Avant de
> croire à un écran vide, lire `window.innerWidth`** : à 0, ce qui est à l'écran
> ne dit rien de l'application. Comparer avec le HTML servi en SSR (`curl`), qui
> ne dépend d'aucune mise en page, tranche en une commande.
>
> **Ce que la rétractation ne couvre pas.** Elle ne dit pas que ces deux écrans
> sont exempts de défauts, seulement que **ces deux observations-là** n'en
> étaient pas.

### 3. `Connection must be valid and open` sur la requête de marque

> **Fermée le 2026-08-07**, au point 1 de la V1 et donc à l'échéance annoncée.
> La cause est nommée, et ce n'est aucune des deux pistes que l'entrée listait.
>
> **La cause.** `App.razor` et `TenantBoundary` résolvaient tous deux la marque
> **dans le même scope de requête, pendant la même passe de rendu statique**. Le
> `SemaphoreSlim` du resolver les sérialise, donc le second attendait le premier.
> Sur une page qui ouvre aussi un circuit, la requête HTTP se terminait — et son
> scope était disposé — avant que la requête du premier ne revienne : le
> `GymDbContext` transient, disposé avec le scope, emportait sa connexion.
> `CheckState` échouait alors **avant que la commande n'atteigne MySQL**.
>
> **Ce qui l'établit**, mesuré et non déduit :
>
> - **36 chargements SSR sans circuit : 0 occurrence. Chaque chargement avec
>   circuit : exactement 1.** Le message a besoin qu'un circuit démarre.
> - **La connexion n'avait jamais été ouverte** : `state=Closed`, aucun
>   `ServerThread`, là où toute connexion saine porte `state=Open` et un
>   identifiant serveur réel. Ce n'est donc pas une connexion périmée rendue par
>   le pool — il n'y avait aucune connexion physique derrière.
> - **Le contexte était en cours de disposition** : quelques microsecondes plus
>   tôt, sur le même thread et le même `DbContext`, une connexion précédente
>   passait en `disposing`.
> - **La réponse était déjà terminée** : le même resolver scoped rapportait
>   `HttpContext` présent au premier envoi, nul au second.
>
> **Pourquoi personne ne l'avait vu.** Le gate libérait ensuite `TenantBoundary`,
> qui rejouait la même requête sur un chemin vivant et réussissait — et comme
> `TenantResolver` n'affecte `_resolvedSlug` **qu'après** l'await, l'échec ne
> laissait aucun cache empoisonné. Ce n'était donc pas une reprise de
> `MySQLExecutionStrategy` : l'exception remontait bel et bien hors de
> `App.razor.OnInitializedAsync`, et c'est **une passe de rendu jetée** à chaque
> chargement de page, pas seulement une ligne de log.
>
> **Ce qui a été fait.** `TenantResolutionMiddleware` résout la marque une fois
> par requête, après `UseAuthentication` et avant tout rendu : la requête part
> alors dans le pipeline, où le scope lui survit par construction. Le resolver
> étant scoped et déjà porteur de son cache, `App.razor` et le prérendu de
> `TenantBoundary` sont désormais servis **de mémoire, sans aucune requête**. Le
> déclencheur est retiré, pas rétréci.
>
> Le middleware décide sur **l'endpoint**, pas sur l'en-tête `Accept` : une page
> d'erreur ré-exécutée et une navigation à la barre d'adresse arrivent toutes
> deux sans `Accept`, et les sauter laissait justement un composant résoudre en
> plein rendu. C'est une première version de ce correctif qui l'a montré.
>
> **Ce qui a été vérifié**, protocole de l'entrée rejoué sur le build final,
> instrumentation retirée — 36 chargements SSR sur les neuf écrans du gérant,
> plus connexion et navigations en circuit interactif sur Planning, Présences,
> Membres et Réglages :
>
> - **0 `Connection must be valid and open`** ;
> - **0 `Failed executing DbCommand`** — le point qui sépare un correctif d'un
>   déplacement : **aucune autre requête n'a hérité de l'échec** ;
> - 0 ligne de niveau Error, 0 `A second operation was started` ;
> - 564 tests au vert, **0 warning** dans `TechXyz.GymXyz.WebApp` ;
> - les écrans s'affichent, habillés et garnis.
>
> Les requêtes de marque passent de **trois à deux par chargement de page**
> (une requête, un circuit), et aucune des deux n'est plus émise depuis un rendu.
>
> **Les deux pistes de l'entrée, tranchées.** *Le connecteur est mis hors de
> cause* : `MySql.Data` refuse une connexion qui n'avait jamais été ouverte, ce
> que ferait n'importe quel fournisseur — **aucun changement de dépendance n'est
> justifié**, et Pomelo n'aurait fait que le formuler autrement. *La double
> passe est confirmée mais mal décrite* : la duplication nuisible n'était pas
> prérendu-contre-circuit à travers deux scopes, mais `App.razor` +
> `TenantBoundary` **à l'intérieur d'un seul**. La passe du circuit est une
> troisième requête, et elle réussissait toujours.
>
> **La troisième piste, absente de l'entrée, est morte.** Le second appelant de
> `Planning.razor:153` échouait au même taux qu'un écran sans aucune requête, et
> n'apparaît dans aucune trace d'échec. Il est retiré quand même — la marque est
> cascadée comme sur tous les autres écrans — ce qui emporte `CS8604` avec lui.
>
> **Ce que la fermeture ne couvre pas.**
>
> - **Le comportement en cas de vraie panne change.** Un seul appelant résout
>   désormais ; si la base est en peine, la requête échoue au lieu d'être
>   masquée par un second appelant qui réussissait. C'est plus honnête, mais
>   c'est un changement : une panne se verra maintenant.
> - **La passe du circuit interroge toujours** une fois par démarrage de
>   circuit. Elle n'a jamais échoué — le scope d'un circuit vit ce que vit
>   l'onglet — et la supprimer demanderait `PersistentComponentState`, qui n'a
>   pas sa place dans un lot sur le log.
> - **Rien n'a été exercé hors de `localhost`** : les sous-domaines clients
>   restent non testés, comme le note déjà l'entrée 2.
> - Les deux anomalies d'écran vues en passant ont été instruites et se sont
>   révélées être des artefacts du volet navigateur, pas des défauts : voir
>   l'**entrée 6**, retirée le jour même.

**Ouverte le 2026-08-06**, en corrigeant l'entrée 1 — dont elle était un
morceau, à tort. **Échéance révisée le 2026-08-07 : c'est le point 1 de la V1**,
donc le prochain travail. L'entrée disait « aucune tant qu'elle reste
invisible », et c'est toujours vrai — ce qui la fait passer devant n'est pas une
aggravation mais le calendrier : un log qui contient une exception connue et
tolérée est un log dans lequel on ne voit plus les nouvelles, et les deux
chantiers qui suivent (onboarding, super-admin) sont ceux où il faudra le lire.
Elle part avec les deux warnings de compilation du WebApp — `CS8604` sur
`Planning.razor:153` (`Slug` possiblement nul passé à `GetTenantBrandQuery`) et
`BL0008` sur `Login.razor:72` (propriété `[SupplyParameterFromForm]` avec
initialiseur, écrasable par un `null` au post) — le second touchant l'écran de
connexion, qui est justement ce que le point 4 rouvre.

**Observé.** `System.InvalidOperationException: Connection must be valid and
open`, levée dans `MySql.Data`, toujours sur la **même** requête : celle de
`GetTenantBrandQuery` (`SELECT ... FROM Tenants WHERE Slug = 'gymxyz'`). Aucune
autre requête ne tombe.

**Ce qui a été mesuré, et qui la sépare de l'entrée 1.** La fabrique de contexte
ne la change pas :

| Build | `A second operation was started` | `Connection must be valid and open` |
|---|---|---|
| `main` sans le correctif | absent sous cette charge | **présent** |
| avec la fabrique | absent | **présent, à l'identique** |

Même charge de part et d'autre : connexion, puis 32 chargements de page lus
jusqu'au bout sur les huit écrans, plus des navigations réelles (circuit
interactif) sur Planning, Présences, Membres et Administration. Le symptôme est
donc **antérieur et indépendant** du contexte partagé : il n'était pas causé par
ce que l'entrée 1 décrivait.

**Pourquoi ça ne casse rien aujourd'hui.** Les pages s'affichent complètes et
correctement habillées, et le log fait suivre l'échec d'un `Executed DbCommand`
réussi sur la même requête. Rien à l'écran ne le laisse voir — c'est pour ça
qu'il a pu vivre plusieurs lots dans le log sans que personne le remarque
autrement qu'en le lisant.

**Ce qui n'est pas su.** La cause. Deux pistes non départagées : le connecteur
`MySql.Data` qui rend une connexion du pool pas encore ouverte, et la double
passe de `TenantBoundary.OnInitializedAsync` (prérendu puis circuit). Ne pas
la refermer sans avoir tranché : une entrée qui dit « corrigé » sans savoir
pourquoi ne vaut rien à la personne qui reverra le message.

---

### 2. Un `PlatformAdmin` hors impersonation lit les données de GymXYZ

> **Fermée le 2026-08-07**, dans le lot 11 et donc à l'échéance annoncée —
> corrigée en **A + B**, C écarté pour la raison donnée plus bas. La décision
> produit a été prise au moment du plan du lot, avant d'écrire du code, comme
> l'entrée le demandait.
>
> **Ce qui a été fait.** `TenantResolver` ne replie plus sur l'hôte pour un
> principal authentifié sans claim de tenant : le tenant ambiant reste 0 et le
> filtre global ne laisse rien passer. La navigation métier disparaît — Réglages
> compris, ce sont les réglages du client — et il ne reste qu'Administration.
> L'URL directe répond pour elle-même via `CustomerScope`, qui affiche
> « Aucun client sélectionné » plutôt qu'un état vide mensonger. La console porte
> désormais la marque `ConsoleBrand` (TechXYZ, tenant 0) au lieu d'emprunter
> celle de GymXYZ.
>
> **Ce qui a été vérifié**, l'entrée demandant de prouver qu'aucun autre rôle ne
> perd l'accès : l'admin sans client ne voit plus aucune donnée et garde sa
> console ; l'admin entré chez Leyssa voit les six membres, le bandeau, et la
> ligne `TenantImpersonation` est ouverte — c'est la **ligne 1**, tout ce qui
> précède l'entrée n'ayant rien lu ; le gérant GymXYZ voit ses 36 membres ; le
> coach voit tout son écran ; l'anonyme garde le repli par hôte, seul cas pour
> lequel il avait été écrit.
>
> **Ce que la fermeture ne couvre pas.** Le repli par hôte reste le chemin des
> sous-domaines clients en production ; il n'a pas été exercé ailleurs qu'en
> test, faute d'un DNS de développement.

**Échéance proposée : avant le lot 11** (marques clientes), qui multiplie les
clients et donc l'exposition. **Demande une décision produit avant de coder.**

**Observé, et c'est plus qu'un défaut d'affichage.** Connecté en
`admin@techxyz.fr`, **sans être entré chez aucun client**, `/members` affiche
les 36 membres de GymXYZ avec leurs adresses e-mail. Vérifié le 2026-08-06.

Conséquences, dans l'ordre de gravité :

1. **La trace du lot 9 a un trou.** Aucune ligne `TenantImpersonation` n'est
   ouverte, puisque l'admin n'est pas passé par `/account/client`. Des données
   client sont lues sans que rien ne l'enregistre — ce que cette entité existe
   précisément pour empêcher.
2. **Aucun bandeau**, puisqu'`IsImpersonating` est faux. L'admin n'a rien à
   l'écran qui lui dise chez qui il est.
3. GymXYZ est choisi par défaut, pas par lui.

Ce n'est pas une élévation de privilège — un `PlatformAdmin` a le droit de voir
n'importe quel client. C'est que ce droit s'exerce **en silence**.

**Pourquoi.** `GymUserClaimsPrincipalFactory` n'écrit aucun claim de tenant pour
un `PlatformAdmin` — délibérément, commentaire du lot 0 à l'appui.
`TenantResolver.SlugFromClaims` renvoie donc null, on retombe sur
`ResolveSlugFromHost()`, qui renvoie `TenantOptions.DefaultSlug` — `gymxyz` en
développement. Le repli existe depuis le lot 0 pour habiller l'écran de
connexion avant authentification ; il n'a jamais été pensé pour un compte
authentifié qui n'appartient à personne.

**La décision à prendre**, parce qu'elle change ce qu'on code :

| Option | Ce que ça donne |
|---|---|
| **A — Un admin sans client ne voit aucune donnée métier** | `TenantResolver` ne replie plus sur l'hôte pour un principal authentifié sans tenant. Le filtre global ne laisse alors rien passer (tenant 0), et les écrans métier s'affichent vides. Il faut décider ce qu'ils disent : « Entrez chez un client depuis la console » plutôt qu'un état vide trompeur. |
| **B — La navigation métier disparaît pour lui** | La barre latérale ne montre que l'Administration tant qu'aucun client n'est choisi. Plus lisible, mais l'URL directe reste à traiter — donc A quand même, en dessous. |
| **C — Entrer chez un client devient obligatoire** | La console redirige vers `/account/client` dès qu'un écran métier est demandé. Le plus strict, et le seul qui garantisse une ligne de trace pour toute lecture. |

Penchant à défendre au moment du plan : **A + B**, avec C écarté pour une
raison précise — obliger l'impersonation pour lire une page ferait ouvrir une
ligne de trace à chaque coup d'œil, et une trace qui enregistre tout
n'enregistre plus rien.

**Taille.** Petit en code (une condition dans `TenantResolver`, un filtre de
navigation), moyen en vérification : c'est un changement d'autorisation de fait,
et il faut prouver qu'aucun autre rôle ne perd l'accès au passage.

---

### 1. Un seul `DbContext` par circuit, partagé par tout l'écran

**Fermée le 2026-08-06**, avant le lot 10 comme prévu — corrigée dans
`TechXyz.GymXyz.Persistence/Extensions/IServiceCollectionExtensions.cs` :
`IGymDbContext` est enregistré `Transient` depuis un
`IDbContextFactory<GymDbContext>` lui-même `Scoped`. Les 77 fichiers qui le
prennent n'ont pas changé, comme l'entrée l'annonçait.

**Ce qui a été vérifié.** 465 tests au vert, puis passage navigateur sur les
quatre écrans les plus bavards en circuit interactif : plus aucun
`A second operation was started on this context instance`, et la console charge
ses trois clients sans le toast rouge.

**Ce que la fermeture ne couvre pas.** L'entrée annonçait « la disparition des
**deux** messages » comme critère. Un seul a disparu : `Connection must be valid
and open` se reproduit à l'identique sur `main` sans le correctif, donc il ne
venait pas de là. Il part vivre sa propre vie en **entrée 3**.

**Reste ouvert par choix.** Un contexte servi `Transient` est tenu par le
conteneur jusqu'à la fin du scope, et le scope d'un circuit Blazor dure ce que
dure l'onglet : les instances s'accumulent au fil d'une longue session. Elles
sont petites et ne retiennent pas de connexion — les connexions retournent au
pool après chaque requête — mais ce n'est pas nul. Si ça devient mesurable, le
remède est connu et plus gros : les handlers prennent la fabrique et disposent
eux-mêmes leur contexte.

---

<!-- Ancienne description de l'entrée 1, gardée telle qu'écrite : -->

**Échéance proposée : avant le lot 10.** C'est la seule entrée urgente du
registre.

**Observé.** `Connection must be valid and open` et
`A second operation was started on this context instance` dans le log de
développement, sur plusieurs écrans. Vérifié au lot 9 PR 1 sur `/members` avec
le gérant GymXYZ — donc sur un chemin qu'aucun lot récent n'a touché : c'est
antérieur, pas une régression.

Le lot 9 PR 2 l'a rendu **fatal** pour la première fois : la console tire cinq
requêtes par chargement et perdait la course assez souvent pour finir sur un
toast rouge « Impossible de charger les clients ». Contourné en donnant à cet
écran son propre scope DI (`Administration.razor`, `InConsoleScopeAsync`) — un
contournement local et assumé, pas le remède.

**Pourquoi ça arrive.** `GymDbContext` est enregistré `Scoped`, donc partagé par
tous les composants d'un circuit Blazor. EF interdit deux opérations
simultanées sur une instance. `TenantResolver` porte déjà un `SemaphoreSlim`
pour exactement ça, avec le commentaire qui le dit : « la mise en page et la
frontière demandent toutes deux pendant la même passe de rendu ».

**La piste, et sa vraie taille.** Elle est plus petite qu'elle n'en a l'air :

- **Les 77 fichiers de `Application` qui prennent `IGymDbContext` ne changent
  pas.** Seul l'enregistrement change : `IGymDbContext` servi depuis un
  `IDbContextFactory<GymDbContext>`, donc une instance par handler.
- **`GymDbContext` reste `Scoped`** : `AddEntityFrameworkStores<GymDbContext>()`
  en a besoin, et `DbInitializer` le prend directement.
- Le filtre global lit `ITenantContext` **à travers l'instance de contexte**, et
  `ITenantContext` reste `Scoped` : chaque nouveau contexte lit donc toujours le
  tenant ambiant du circuit. À vérifier, c'est le point qui casserait tout.

**Ce qui rend ça sûr — vérifié, pas supposé.** Le changement fait perdre le
suivi d'entités partagé entre deux handlers d'une même requête. Deux recherches
disent que personne n'en dépend :

- aucun handler n'envoie de requête MediatR imbriquée (`ISender` / `IMediator`
  n'apparaît dans aucun `*.Handler.cs`) ;
- aucune transaction explicite (`BeginTransaction`) nulle part.

Autrement dit, **un handler est déjà une unité de travail**. C'est ce qui fait
la différence entre un changement d'enregistrement et une réécriture.

**Risque à ne pas sous-estimer.** Le rayon d'action est l'application entière,
et les symptômes actuels sont intermittents — donc « ça marche chez moi » ne
prouve rien. Ce lot demande la suite complète **plus** un passage navigateur sur
les écrans les plus bavards (Planning, Présences, Membres, Administration), et
la disparition des deux messages du log est le critère d'acceptation, pas
l'absence de plainte.

**À faire au passage** : retirer `InConsoleScopeAsync` d'`Administration.razor`
une fois la fabrique en place — sauf pour sa **première** raison d'être, qui
survit au correctif : la console est servie comme aucun client, et un scope
neuf ne résout aucun tenant. Garder le scope, réécrire le commentaire pour
qu'il ne parle plus d'une course qui n'existera plus.

---

## Ce que ce registre ne remplace pas

`01-LOTS.md` reste la source des lots métier et de leurs décisions attendues.
Ce document est l'inverse : ce que la **construction** a appris, que le hand-off
ne pouvait pas prévoir. Les deux se lisent avant de planifier un lot.
