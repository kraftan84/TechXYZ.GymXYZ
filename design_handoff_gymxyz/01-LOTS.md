# Plan d'implémentation — 13 lots

**Principe de découpage : par feature verticale.** Chaque lot livre le desktop
**et** le mobile de la même section, de l'entité EF jusqu'à l'écran. Raison : les
deux surfaces partagent entités, queries et thèmes ; les séparer par device
obligerait à rouvrir chaque handler une seconde fois, et à maintenir deux fois la
même incertitude métier.

**Ordre des marques** : tous les lots se font sur la marque **GymXYZ** (habillage
par défaut). Les deux marques clientes arrivent au **lot 11** — le mécanisme, lui,
est posé dès le lot 0, donc le lot 11 doit être court par construction (si le lot 11
demande de retoucher des écrans, c'est que le lot 0 a échoué).

**Un lot = une PR.** DoD commune à tous les lots, en plus de celle du repo :

- [ ] Frontières de couches respectées (`WebApp → Application → Domain`).
- [ ] Commandes/queries dans le style existant (`CreateXCommand` + `.Handler` + `.Validator`).
- [ ] Queries en `AsNoTracking()`, filtrage `IsActive` explicite, `TenantId` couvert par le filtre global.
- [ ] Tests xUnit (Shouldly + Bogus) sur les handlers et validators ajoutés.
- [ ] Écrans conformes au prototype **en desktop ET en mobile**, y compris états vides, chargement et erreur.
- [ ] Aucune couleur, taille de police ni rayon en dur : uniquement `var(--*)`.
- [ ] Anneau de focus visible sur tout élément interactif ; navigation clavier possible.
- [ ] Le basculement de thème ne casse rien (tester au minimum GymXYZ + un thème sombre bidon).
- [ ] `dotnet test` vert, aucune régression de démarrage du WebApp.

---

## Où en est le chantier — 2026-08-07

Lots **0 à 11 livrés et fusionnés**, plus un lot qui n'était pas dans ce plan.
`main` est à `531c005`, **564 tests au vert**.

| Lot | État |
|---|---|
| 0 Socle · 1 Membres · 2 Coachs · 3 Cours · 4 Lieux | ✅ |
| 5 Planning · 6 Présences · 7 Abonnements | ✅ |
| 8 Réglages · 9 Administration · 10 Accueil | ✅ |
| 11 Marques clientes | ✅ (3 PR) |
| **Rôles & cloisonnement** — hors plan, `LOT-ROLES-BRIEF.md` | ✅ (3 PR) |
| 12 Portail membre | **hors V1**, voir plus bas |

**Le lot « Rôles & cloisonnement » ne figurait pas ici.** Il s'est intercalé
après le lot 11 parce qu'une décision produit a vidé le lot 12 de son objet :
les membres ne se connectent pas. Ce qui restait debout, c'est le rôle `Coach`
— qui voyait jusque-là le chiffre d'affaires du club et ses réglages. Trois PR :
retrait des comptes membres du seed, politique par écran et par commande, puis
cloisonnement des données d'un coach à ses propres séances.

---

## Ce qui reste pour la première version

Séquence arrêtée le 2026-08-07. **Au bout de cette liste, la V1 est complète** —
tout ce qui n'y est pas est explicitement repoussé (section suivante).

| # | Chantier | Matière disponible |
|---|---|---|
| 1 | **Entrée 3 du registre + warnings de compilation** | `LOT-13-BRIEF.md` ; `CS8604` sur `Planning.razor:153`, `BL0008` sur `Login.razor:72` |
| 2 | ~~Cloche de notifications + météo des cours extérieurs~~ | **vidé le 2026-08-07** — voir juste en dessous |
| 3 | **Diffusion du planning en image** | décision du lot 10, tranchée ci-dessous |
| 4 | **Login & onboarding** | **handoff à fournir** |
| 5 | **Portail super-admin** | **handoff à fournir** |

> **Le point 2 n'existe plus, et la V1 tient en cinq points.** Ses deux chantiers
> sont tombés le même jour, pour deux raisons différentes :
>
> - **la cloche est reportée hors V1**, « pour le moment » — rien ne stocke une
>   notification, `NotificationSetting` est une *préférence* et non un événement,
>   il n'y a donc rien à lister ; et l'écran de liste que ça demanderait n'est pas
>   maquetté. À revisiter, pas à enterrer ;
> - **la météo est abandonnée** — trop de plomberie pour ce qu'elle rendait dans
>   une première version. Le détail est dans la décision du lot 4, plus bas.
>
> Ce qui a été livré du point 2 : la cloche **retirée** des deux shells (un point
> rouge allumé en permanence apprend à ne plus regarder les points rouges), les
> URL d'API sorties dans `appsettings`, et un réglage « afficher les vacances
> scolaires » par client. La numérotation ci-dessus est conservée telle quelle
> pour que les briefs déjà écrits continuent de désigner les mêmes chantiers.
| 6 | **Comptes à casquettes multiples sur plusieurs clients** | entrée 4 du registre, qui décrit déjà le remède et son risque |

**L'ordre porte deux dépendances, pas seulement une préférence.** Le point 6
arrive **après** les points 4 et 5 parce qu'il touche la fabrique de claims, la
connexion et l'impersonation : le faire avant obligerait à réécrire ce que
l'onboarding et la console viennent de poser. Mais il arrive **dans la V1**, et
pas après, parce que les points 4 et 5 sont précisément ce qui crée des comptes
et des clients pour de vrai — livrer l'onboarding sur un modèle où un compte ne
porte qu'un client, c'est écrire une deuxième fois la règle qu'on sait fausse.
Les deux handoffs doivent donc être lus en sachant que le point 6 suit.

Les points 4 et 5 attendent leur handoff et **ne se commencent pas par
morceaux** en attendant.

**Un septième chantier ferme la marche, hors de cette liste** : le passage aux
migrations EF (`EnsureCreated` → `Migrate`, entrée 5 du registre), placé **juste
avant un déploiement**. Tant que l'onboarding ne tourne qu'en développement, la
base reste jetable et le modèle peut encore bouger sans payer une migration par
PR ; la seule chose qui ne se négocie pas, c'est que la bascule soit faite
**avant la première inscription réelle**.

---

## Décisions prises le 2026-08-07

Elles referment des questions restées ouvertes dans ce document ou dans le
README du handoff. Chacune est reportée dans le lot concerné plus bas.

| Sujet | Décision |
|---|---|
| **Recherche globale** (écart n°1, proposée au lot 1) | **Hors V1.** La barre de la topbar reste inerte. |
| **Notifications / cloche** (écart n°2) | **Hors V1, « pour le moment »** (décision du 2026-08-07, qui remplace la précédente). La cloche a été **retirée** des deux shells : rien ne stocke une notification, et son point rouge était allumé en dur. À revisiter, pas à enterrer. |
| **Densité d'affichage** (écart n°5) | **Écartée.** Outil de maquette, non porté en produit. |
| **Météo des cours extérieurs** (lot 4) | **Abandonnée pour la V1** (décision du 2026-08-07, qui remplace la précédente). Le champ « lieu de repli » livré au lot 4 **reste** : le gérant le renseigne et le lit lui-même. |
| **« Diffuser le planning »** (lot 10) | **Génère une image du planning hebdomadaire**, téléchargeable par le manager pour les réseaux sociaux. Aucun envoi. |
| **Envoi SMS réel** (lot 8) | **Hors V1.** On s'en tient au stockage des préférences. |
| **Portail membre** (lot 12) | **Hors V1.** Les membres ne se connectent pas ; à reprendre plus tard, design d'abord. |

---

## Ordre d'exécution — pourquoi il a changé

L'ordre initial du handoff plaçait l'**Accueil** en lot 1. Le handoff signalait
lui-même la difficulté : les KPI et les alertes du tableau de bord sont
**calculés** à partir d'entités produites par le planning, les présences et les
abonnements. Le construire en premier obligeait soit à inventer des chiffres, soit
à créer au passage la moitié de trois autres lots sans leur écran.

L'ordre ci-dessous applique une règle simple : **les référentiels d'abord**
(personnes, offre, lieux), **puis les flux** (planning, présences, argent),
**puis le paramétrage**, **puis l'agrégat**, **puis les marques**. Chaque lot ne
consomme que des entités déjà livrées, et aucun lot ne demande de rouvrir un lot
précédent.

| Nouvel ordre | Lot | Réf. handoff initial |
|---|---|---|
| **0** | Socle : tenant, thème, shells, auth | 0 |
| **1** | Membres + fiche membre | 3 |
| **2** | Coachs | 5 |
| **3** | Cours (catalogue de modèles) | 4 |
| **4** | Lieux / studios | 9 |
| **5** | Planning | 2 |
| **6** | Présences (pointage) | 6 |
| **7** | Abonnements & encaissements | 10 |
| **8** | Réglages (côté client) | 8 |
| **9** | Administration (super-admin TechXYZ) | 7 |
| **10** | Accueil / tableau de bord | 1 |
| **11** | Marques clientes | 11 |
| **12** | Portail membre | 12 |

Trois déplacements portent la décision :

1. **Accueil 1 → 10.** Il agrège les lots 5, 6 et 7 ; construit après eux, il se
   fait une fois au lieu de deux. Contrepartie assumée : la démo commerciale n'a
   pas de tableau de bord avant la fin du chantier. Si ce point est bloquant,
   c'est le seul arbitrage à rouvrir.
2. **Lieux 9 → 4.** Le planning a besoin d'un lieu qui porte une capacité pour
   faire respecter ses invariants (pas de dépassement, pas de chevauchement).
   Livrer les lieux après le planning imposait d'ajouter ces champs deux fois.
   Contrepartie : au lot 4, la fiche de lieu affiche son planning du jour et sa
   heatmap **vides** — ce sont des vues dérivées, elles se remplissent seules au
   lot 5.
3. **Coachs 5 → 2.** Un modèle de cours et une séance référencent tous deux un
   coach. Coachs partage la base `Person` avec Membres : les faire consécutivement
   réutilise les mêmes patterns pendant qu'ils sont frais.

---

## Lot 0 — Socle : tenant, thème, shells, auth ✅ livré

**Le lot le plus important.** Tout le reste en dépend, et c'est lui qui rend le lot
11 trivial.

**Décisions prises**

| Sujet | Décision |
|---|---|
| Clés | `int` partout, homogène avec `EntityBase<int>` existant. |
| `Gym` / `Tenant` | `Tenant` est une nouvelle entité ; `Gym` reste la racine métier et passe dessous. `GetDefaultGymAsync` inchangé. |
| Filtre global EF | Sur **`TenantId` seul**. `IsActive` reste filtré explicitement, conformément aux conventions du repo. |
| Multi-tenant | Résolution par sous-domaine complète, repli config en dev, outrepassable par un `PlatformAdmin`. |
| Identity | Dans `GymDbContext` (`IdentityDbContext`), une seule base. `ApplicationUser` dans `Persistence/Identity`. |
| Tenant faisant foi | Le claim du cookie d'authentification, pas l'hôte — un circuit Blazor n'a pas d'hôte vérifiable. |
| Responsive | `matchMedia` à 900px → un seul arbre rendu, cookie anti-clignotement au premier rendu serveur. |
| Routes | URLs existantes conservées ; seuls les libellés suivent le prototype. |
| Migrations | `EnsureCreated` conservé, pas de migrations EF introduites. |

**Livré**

- `Tenant`, `ITenantScoped`, filtre global, estampillage du `TenantId` à l'écriture.
- Identity : 4 rôles, `ApplicationUser.TenantId`, claims de marque dans le cookie,
  écran de connexion en rendu statique, `Administration` protégée par policy.
- Styles du handoff repris tels quels (tokens `techxyz`, `themes.css`, `app.css`,
  `mobile.css`) ; `data-theme` rendu côté serveur ; pont CSS vers les tokens Fluent.
- Shells desktop et mobile, primitives partagées, icônes Lucide portées en Razor.
- Règle solo : la section Coachs disparaît de la navigation **et** sa route redirige.
- Seed : 1 tenant GymXYZ, 4 rôles, 1 gérant, le jeu de démo minimal.

**Dette reportée** : polices Anton et Dancing Script encore chargées depuis Google
Fonts (auto-hébergement au lot 11) · `IGymDbContext` ne peut pas exposer
`ApplicationUser`, une abstraction sera nécessaire au lot 8 · assets de marque non
copiés (lot 11).

---

## Lot 1 — Membres + fiche membre ✅ livré

*Réf. handoff initial : lot 3.* Ne dépend que du lot 0. Débloque les lots 6, 7 et 8.

**Desktop** (`design/app/screen-membres.jsx`) : tableau (avatar + nom, e-mail,
formule avec chip, jauge de crédits `3/10` ou `∞`, assiduité %, dernière venue,
statut), filtres, recherche, action « Ajouter un membre ». Fiche membre en
**drawer 520px** (le repo a déjà un `DrawerShell`) : identité, formule, paiements,
prochains cours, historique récent avec présent/absent.

**Mobile** (`design/app/mobile/screen-membres.jsx`) : liste + fiche plein écran
(retour dans le header).

**Application** : `GetMembersQuery` (filtres + pagination server-side),
`GetMemberDetailQuery`, `CreateMemberCommand`, `UpdateMemberCommand`,
`DeactivateMemberCommand` (soft delete).

**Domain** : `Member` s'enrichit de `JoinedOn` et `Notes`. Les colonnes qui
dépendent d'entités non encore livrées (formule, crédits, dernière venue) affichent
un état vide jusqu'à leur lot — elles ne sont pas simulées.

**Statuts** : `Actif` (succès), `Expire bientôt` (warning), `Inactif` (danger) —
**dérivés** de l'abonnement et de la dernière présence, pas saisis.

> **Décisions attendues avant de coder**
> 1. Seuils de statut : « expire bientôt » = échéance ≤ 7 jours ? « inactif » =
>    aucune venue depuis combien de temps ?
> 2. Assiduité : sur quelle fenêtre (30 j, 90 j, depuis l'inscription) et quel
>    dénominateur (séances inscrites ou séances proposées) ?
> 3. **Recherche globale** (écart n°1 du README) : traitée ici — membres + cours,
>    palette clavier `Ctrl+K` — ou repoussée à un lot dédié ?

---

## Lot 2 — Coachs ✅ livré

*Réf. handoff initial : lot 5.* Ne dépend que du lot 0. Précède Cours et Planning,
qui référencent tous deux un coach.

**Desktop** (`design/app/screen-coachs.jsx`) : grille de cartes 3 colonnes
(avatar, nom, rôle, chips de disciplines, cours/semaine, remplissage, note,
membres suivis, chip de disponibilité) + fiche (bio, certifications, disponibilités
sur 7 jours, séances de la semaine, contact).

**Mobile** (`design/app/mobile/screen-coachs.jsx`) : liste + fiche.

**Application** : `GetCoachesQuery`, `GetCoachDetailQuery`, `CreateCoachCommand`,
`UpdateCoachCommand`, `DeactivateCoachCommand`. Un coach peut être lié à un compte
Identity (rôle `Coach`) — le lien est facultatif (un coach existe sans compte).

**Domain** : `Coach` s'enrichit de `RoleLabel`, `Bio`, `JoinedOn`, `Availability`
(7 booléens L→D), `AwayUntil`, `Rating`, `UserId`. Entités `Discipline`,
`CoachDiscipline`, `CoachCertification`.

**Marque blanche** : la règle « pas de section Coachs quand `Tenant.IsSolo` » est
**déjà implémentée au lot 0**, navigation et route comprises. Rien à refaire ici,
seulement à vérifier.

---

## Lot 3 — Cours (catalogue de modèles) ✅ livré

*Réf. handoff initial : lot 4.* Dépend du lot 2 (coachs rattachés).

**Desktop** (`design/app/screen-cours.jsx`) : catalogue de **modèles** de cours en
cartes (icône de discipline, nom, discipline, durée, capacité, studio, niveau,
intensité, prix, séances/semaine, taux de remplissage, habitués, coachs
rattachés) + fiche modèle (description, prochaines occurrences, coachs).

**Mobile** (`design/app/mobile/screen-cours.jsx`) : liste + fiche.

**Application** : `GetCourseTemplatesQuery`, `GetCourseTemplateDetailQuery`,
`CreateCourseTemplateCommand`, `UpdateCourseTemplateCommand`,
`ArchiveCourseTemplateCommand`.

**Attention** : distinguer clairement **modèle** (catalogue, ce lot) et
**occurrence** (planning, lot 5). Le taux de remplissage et les habitués sont
calculés depuis les occurrences passées — donc vides jusqu'au lot 5.

> **Écart de modèle à résoudre ici.** Le `Domain` existant a `Lesson` (TPC) →
> `PrivateLesson` / `CollectiveLesson`, qui mélange modèle et occurrence. C'est le
> plus gros refactor du chantier : il commence à ce lot avec l'introduction de
> `CourseTemplate`, et se termine au lot 5 avec `Session`. Prévoir une stratégie de
> reprise des données de démo existantes.

---

## Lot 4 — Lieux / studios ✅ livré

*Réf. handoff initial : lot 9.* Ne dépend que du lot 0. Remonté avant le planning,
qui a besoin d'un lieu portant une capacité.

**Desktop** (`design/app/screen-salles.jsx`) : cartes 2 colonnes (nom, type, icône,
capacité, surface, étage, occupation, séances/semaine, chip de statut) + fiche
(note, équipements en chips, planning du jour, **heatmap d'occupation sur 7 jours**).

Trois natures de lieu, et c'est structurant : **studio** (interne), **extérieur**
(parc — adresse, dépendance météo, lieu de repli), **domicile** (chez le membre,
capacité 1, adresse portée par la fiche membre).

**Mobile** (`design/app/mobile/screen-salles.jsx`) : liste + fiche.

**Application** : `GetLocationsQuery`, `GetLocationDetailQuery`,
`CreateLocationCommand`, `UpdateLocationCommand`, `DeactivateLocationCommand`.

**Attendu de ce lot** : occupation, séances/semaine, planning du jour et heatmap
sont **dérivés des séances** et s'affichent donc en état vide jusqu'au lot 5.
C'est le prix assumé du déplacement ; ces vues n'ont aucune logique propre à
reprendre ensuite.

> **Écart de nommage à trancher ici.** Le « Lieu » de la maquette (capacité,
> surface, étage, équipements, `Kind`) correspond au `Room` existant, pas au
> `Location` existant (qui porte le site et son adresse). Décider du nommage avant
> de coder, et ne pas renommer en masse.

> **Décision attendue** : implémenter réellement la météo + repli automatique, ou
> se limiter au champ « lieu de repli » (recommandé pour ce lot).
>
> **Tranchée le 2026-08-07 : on s'en tient au champ « lieu de repli ».** L'appel
> météo réel a d'abord été pris dans la V1 le matin même, puis **abandonné** le
> soir après instruction — trop de plomberie pour ce qu'il rendait à ce stade.
> C'est cette seconde décision qui vaut.
>
> **Ce qui reste** : `IsWeatherDependent` et le lieu de repli, tels que le lot 4
> les a livrés. Le gérant les renseigne et les lit lui-même — « s'il pleut, on se
> replie au studio A » est une information utile écrite sur la fiche, même sans
> automatisation. Les coordonnées `Latitude`/`Longitude`, elles, ont été
> **retirées** : elles n'étaient stockées que pour cet appel, et rien ne les
> lisait.
>
> **Ce qu'il faudrait rouvrir le jour où la question revient** (l'enquête a été
> faite, autant qu'elle serve) :
>
> - **le géocodage est tranché** — API Géoplateforme (`data.geopf.fr/geocodage`,
>   IGN), sans clé, Licence Ouverte 2.0, testée sur l'adresse réelle du seed ;
> - **l'État ne publie aucune prévision au point** : les dix API Météo-France
>   livrent des grilles GRIB, pas « il pleut jeudi à 18 h au parc ». Décoder ça
>   en .NET est un chantier, pas un branchement ;
> - **Open-Meteo** a la bonne forme de réponse mais son offre gratuite est
>   **non commerciale** — l'abonnement payant achète l'extraction du point et le
>   SLA, pas la donnée, puisqu'il sert déjà `meteofrance_arome_france_hd` ;
> - **le Bulletin Vigilance** (Météo-France, par département, Licence Ouverte 2.0,
>   compte et jeton requis) était le candidat retenu avant l'abandon : il répond
>   « dois-je annuler » plutôt que « quel temps fera-t-il », ce qui est la vraie
>   question, et sans problème de licence ;
> - **le repli se proposerait, ne s'appliquerait pas** — le lot 5 a posé que rien
>   ne déplace une séance sans qu'un humain le dise.

---

## Lot 5 — Planning ✅ livré

*Réf. handoff initial : lot 2.* Dépend des lots 2, 3 et 4.

**Desktop** (`design/app/screen-planning.jsx`) : grille semaine `56px repeat(7,1fr)`,
heures 7→21, en-têtes de jour collants, blocs de cours (nom, coach, occupation),
navigation semaine précédente/suivante + « aujourd'hui », bandeau **calendrier
scolaire** (zone A/B/C déduite du code postal, jours fériés et vacances marqués sur
les jours concernés), vue jour en repli.

**Mobile** (`design/app/mobile/screen-planning.jsx`) : sélecteur de semaine
horizontal + agenda vertical du jour choisi.

**Application** : `GetWeekPlanningQuery(from, to)`, `CreateSessionCommand`,
`UpdateSessionCommand`, `CancelSessionCommand` (soft delete + notification aux
inscrits, cf. lot 8), `DuplicateWeekCommand` (utile, à confirmer).

**Service** : `ISchoolCalendarService` — fériés + vacances depuis les API
`api.gouv.fr` (`calendrier-scolaire`, `jours-feries`), **appelé côté serveur**,
caché en mémoire par année et par zone, tolérant à la panne (le planning s'affiche
sans le bandeau si l'API tombe — jamais d'exception au rendu). Le prototype
implémente déjà la logique zone/date dans `design/app/calendar.jsx` : la porter
telle quelle (`gxZoneForZip`, `gxDayInfo`, `gxNextEvents`).

**Attention** : c'est le lot le plus risqué en fidélité (grille dense + collant +
chevauchements). Prévoir du temps.

**Invariants à faire respecter** : une séance ne dépasse pas la capacité du lieu ·
deux séances du même lieu ne se chevauchent pas · un coach n'est pas sur deux
séances simultanées (alerte, pas blocage, s'il est marqué indisponible).

> **Décision attendue avant de coder** : **récurrence**. Matérialiser les
> occurrences (une ligne par séance, plus simple à interroger et à pointer,
> recommandé) ou stocker une règle et générer à la volée (plus souple, beaucoup
> plus de complexité de requête) ? Le prototype est compatible avec l'option
> matérialisée.

> **Fin du refactor commencé au lot 3** : `Session` (occurrence datée) remplace
> l'usage d'occurrence de `Lesson`.

---

## Lot 6 — Présences (pointage) ✅ livré

*Réf. handoff initial : lot 6 — inchangé.* Dépend des lots 1 et 5.

**Desktop** (`design/app/screen-presences.jsx`) : 4 KPI (taux d'assiduité +
évolution, séances à pointer, présents, no-shows), liste des séances par jour avec
état (`à pointer` / `en cours` / `pointée`) et compteur inscrits/présents, taux par
cours (barres), membres les plus absents. **Feuille de présence** : bandeau de 4
stats, liste des inscrits avec **contrôle segmenté Présent / Retard / Absent**,
enregistrement, retour.

**Mobile** (`design/app/mobile/screen-presences.jsx`) : liste de séances → feuille
avec segmenté au doigt (cibles ≥ 44px), barre d'action collante en bas.

**Application** : `GetAttendanceOverviewQuery`, `GetSessionRosterQuery`,
`CheckInMemberCommand`, `MarkAbsentCommand`, `CloseAttendanceSheetCommand`.

**Domain** : `Registration` porte inscription **et** présence — c'est ce que fait
la feuille de pointage (les inscrits sont la liste, le statut est la colonne).

**Attention** : le pointage est l'écran le plus utilisé du produit sur mobile, en
salle, parfois avec une main. Réactivité optimiste (mise à jour immédiate, erreur
via `IUserFeedbackService`), et **verrouillage** d'une feuille clôturée.

> **Décision attendue** : la réouverture d'une feuille clôturée est réservée au
> `GymManager` — confirmer, et dire si l'action doit être tracée.

C'est aussi ce lot qui produit enfin les colonnes « dernière venue » et
« assiduité » laissées vides au lot 1.

---

## Lot 7 — Abonnements & encaissements ✅ livré

*Réf. handoff initial : lot 10.* Dépend des lots 1 et 6 (le pointage décrémente les
carnets).

**Desktop** (`design/app/screen-abos.jsx`) : KPI (MRR + évolution, actifs, expirent,
en retard + montant), **formules** en 4 cartes (prix, unité, nb de membres,
description, modalité d'engagement, carte mise en avant), **suivi** des abonnements
(membre, formule, échéance ou crédits restants, jauge, montant, renouvellement
auto, statut), **encaissements** (date, membre, libellé, montant, moyen, statut
encaissé/rejeté).

**Mobile** (`design/app/mobile/screen-abos.jsx`) : onglets Suivi / Formules /
Encaissements.

**Application** : `GetSubscriptionOverviewQuery`, `GetPlansQuery`,
`CreatePlanCommand`, `UpdatePlanCommand`, `AssignSubscriptionCommand`,
`RenewSubscriptionCommand`, `RecordPaymentCommand`, `SendPaymentReminderCommand`.

**Attention** : deux natures d'abonnement à modéliser proprement — **récurrent**
(mensuel/annuel, échéance + renouvellement auto) et **carnet de crédits** (carte 10
séances, décrémentée par le pointage du lot 6, **une seule fois** : repointer ne
redécrémente pas). Aucun paiement en ligne dans ce lot (pas de PSP) : on
**enregistre** des encaissements réalisés hors ligne. Brancher un PSP est un lot à
part.

> **Décision attendue avant de coder** : **règle de calcul du MRR**. Somme
> normalisée mensuelle des abonnements récurrents actifs — et les carnets, dedans
> ou dehors ? S'ils entrent dans un CA lissé, c'est un second indicateur à nommer.

> **Écart de modèle à résoudre ici.** Le `Subscription` existant
> (`NumberOfLessons`, `CashedBy`) est remplacé par le trio `Plan` / `Subscription`
> / `Payment`.

Ce lot complète les colonnes « formule » et « crédits » laissées vides au lot 1.

---

## Lot 8 — Réglages (côté client) ✅ livré

*Réf. handoff initial : lot 8 — inchangé.* Dépend des lots 1 et 2.

**Desktop et mobile** (`design/app/screen-reglages.jsx` → `ScreenReglages`) :
4 panneaux.

- **Identité** : baseline, adresse, CP, ville, e-mail, téléphone, SIRET, capacité,
  horaires d'ouverture, carte « calendrier scolaire » (zone déduite du CP).
- **Équipe & accès** : liste de l'équipe (rôle, périmètre d'accès, dernière
  connexion), invitations en attente, accès des membres (total / avec compte /
  invités), invitation par e-mail.
- **Formules & tarifs** : les formules d'abonnement + moyens de paiement acceptés,
  devise, mention de TVA.
- **Notifications** : deux groupes (Membres & abonnements / Cours & présences) de
  bascules avec canaux Email/SMS.

**Application** : `GetGymSettingsQuery`, `UpdateGymIdentityCommand`,
`InviteTeamMemberCommand`, `UpdateTeamMemberAccessCommand`,
`RevokeAccessCommand`, `UpdateNotificationSettingsCommand`,
`UpdatePaymentMethodsCommand`.

**Dette du lot 0 à traiter ici** : `IGymDbContext` ne peut pas exposer
`ApplicationUser` (`Application` ne référence pas `Persistence`). Le panneau
« Équipe & accès » a besoin de lire les comptes : introduire une abstraction dans
`Application` (par exemple `IUserDirectory`), implémentée côté Persistence.

**Règle marque blanche** : quand `Tenant.AreaLabel` est renseigné (client solo,
sans adresse postale), le panneau Identité affiche la **zone** au lieu de
l'adresse. À vérifier ici, pas au lot 11.

**Attention** : barre d'enregistrement collante avec état « Enregistré » (le
prototype a `SaveBar`). Les bascules de notification ne déclenchent **pas** encore
d'envoi réel : stocker les préférences, brancher l'envoi dans un lot ultérieur.

> **Décision attendue** : fournisseur SMS et budget, si l'envoi réel doit être
> chiffré. Sinon, on s'en tient au stockage des préférences.
>
> **Tranchée le 2026-08-07 : pas de SMS dans la V1.** On s'en tient au stockage
> des préférences, tel que livré. Le canal e-mail, lui, existe déjà (Brevo, lot 8).

---

## Lot 9 — Administration (super-admin TechXYZ) ✅ livré

*Réf. handoff initial : lot 7.* Ne dépend que du lot 0, mais placé ici parce qu'il
pilote des clients dont le paramétrage n'a de sens qu'une fois les lots métier
livrés.

**Desktop et mobile** (`design/app/screen-reglages.jsx` → `ScreenAdministration`) :
navigation de sections à gauche + panneaux. Deux panneaux :

- **Apparence & marque** : choix du thème du client (cartes de thème avec aperçu
  des couleurs), marque, wordmark, baseline.
- **Facturation** : formule GymXYZ du client (`GymXYZ Pro`, 79 €/mois), échéance,
  moyen de paiement, historique de factures.

**Application** : `GetTenantsQuery`, `GetTenantDetailQuery`, `CreateTenantCommand`,
`UpdateTenantBrandingCommand`, `UpdateTenantPlanCommand`.

**Attention** : seul écran réservé à `PlatformAdmin` — l'autorisation par policy est
**déjà en place depuis le lot 0**, il reste à écrire l'écran. C'est ici que se
pilote la marque blanche : changer le thème d'un client depuis cet écran doit
repeindre son app, sans redéploiement. Le mécanisme est vérifié depuis le lot 0 ;
cet écran ne fait que l'exposer.

**À câbler ici** : l'impersonation d'un tenant par un `PlatformAdmin`
(`ITenantContext.UseTenant`, déjà disponible).

---

## Lot 10 — Accueil / tableau de bord ✅ livré

*Réf. handoff initial : lot 1.* Dépend des lots 5, 6 et 7 — d'où son déplacement en
fin de parcours.

**Desktop** (`design/app/screen-accueil.jsx`) : salutation + date, 4 KPI, carte
« semaine » (bande des 7 jours avec nombre de cours), liste des cours du jour
(heure, nom, studio · coach, jauge d'occupation, chip de statut), pile d'alertes
actionnables (abonnements qui expirent, paiements en retard, présences à pointer),
raccourcis. Chaque ligne et chaque alerte est cliquable et **navigue** vers la
section concernée.

**Mobile** (`design/app/mobile/screen-accueil.jsx`) : grand titre « Bonjour
<prénom> », KPI en tuiles 2×2, bande de semaine scrollable, cours du jour en
lignes compactes, alertes, CTA « Diffuser le planning ».

**Application** : `GetDashboardQuery` → un seul DTO agrégé (KPI + cours du jour +
alertes). Une seule requête aller-retour, projection server-side.

**Attention** : les KPI et les alertes sont **calculés**, pas stockés. Toutes les
règles nécessaires ont déjà été tranchées aux lots 1, 6 et 7 — ce lot les
réutilise, il n'en invente aucune. C'est tout l'intérêt de l'avoir décalé.

**À câbler ici** : le CTA « Diffuser le planning » et le compteur de la navigation
(Présences, Abonnements), laissés inertes depuis le lot 0.

> **Décision attendue** : que fait exactement « Diffuser le planning » ? Le
> prototype affiche « Dernière diffusion : dimanche dernier · vos membres notifiés
> par e-mail + app ». Si l'envoi réel est attendu, il dépend du canal choisi au
> lot 8.
>
> **Tranchée le 2026-08-07 : le bouton génère une image du planning
> hebdomadaire**, que le manager télécharge pour la publier sur les réseaux
> sociaux. **Aucun envoi, aucune notification** — la mention du prototype
> (« vos membres notifiés par e-mail + app ») ne décrit donc pas la V1 et ne doit
> pas être reprise telle quelle à l'écran. Le bouton est aujourd'hui rendu
> désactivé avec sa raison, à l'Accueil (desktop et mobile) **et** en tête du
> Planning : les trois s'activent ensemble, c'est le point 3 de la V1.
>
> À trancher au plan : le rendu se fait **côté serveur** (une image identique
> pour tout le monde, indépendante du navigateur) ou **côté client** depuis la
> grille déjà rendue ; et ce que l'image porte de la marque du client — le lot 11
> a rendu chaque tenant repeignable, une image qui ne suit pas son thème serait le
> seul endroit du produit qui l'ignore.

---

## Lot 11 — Marques clientes : Team Trainer's & Leyssa Coaching ✅ livré

*Réf. handoff initial : lot 11 — inchangé.*

Si les lots précédents ont été faits correctement, ce lot ne touche **aucun écran**.

- Deux blocs `[data-theme="teamtrainers"]` et `[data-theme="leyssa"]` : **déjà
  présents** dans `wwwroot/css/themes.css` depuis le lot 0, et vérifiés.
- Deux tenants en base avec `ThemeKey`, marque, coordonnées, `IsSolo` pour Leyssa.
- Assets de marque dans `wwwroot/assets/themes/` (non copiés au lot 0).
- Polices Anton et Dancing Script **auto-hébergées** (dette du lot 0 : elles
  viennent encore de Google Fonts, ce qui pose un problème RGPD).
- Vérifications par marque (détail dans `02-THEMING.md`) : sidebar sombre lisible
  pour Team Trainer's (logo blanc) ; Leyssa en mode solo (pas de section Coachs),
  marque en cercle, **aucune adresse postale** (zone uniquement : « Thonon »),
  contraste du rose vérifié sur les boutons pleins.
- Ajouter un test qui, pour chaque thème, instancie les pages principales et vérifie
  qu'aucun style en dur n'est apparu (au minimum : revue visuelle des 10 sections
  × 3 marques, à cocher).

**DoD** : capture des 10 sections × 3 marques, desktop et mobile.

---

## Lot 12 — Portail membre ❌ hors V1

*Réf. handoff initial : lot 12 — inchangé.*

Le rôle `Member` est demandé mais **aucun écran membre n'existe dans la maquette**.
Ne pas improviser. Périmètre probable : voir le planning, réserver/annuler une
place, consulter son abonnement et ses crédits, son historique de présence, ses
paiements. À maquetter d'abord (retour en phase design), puis lot d'implémentation.

> **Prérequis** : ce lot n'a pas d'entrée dans le plan tant que le design n'est pas
> livré. Il ne bloque aucun autre lot.

> **Sorti de la V1 le 2026-08-07, et pour une raison qui n'est pas un report.**
> Les membres **ne se connecteront pas** : l'absence de maquette n'était pas un
> retard, c'était une décision. Les quatre comptes `Member` que le seed ouvrait
> ont été retirés (lot rôles, PR 1) et `TeamAccessScopes.Assignable` ne permet
> plus d'en créer. Ce que ce lot promettait est repris autrement : les membres
> sont joints **par e-mail**, et une invitation à un cours se répondra présent /
> absent **sans connexion**. Ce parcours-là est un lot à part entière — jeton
> signé par séance et par personne, page publique non authentifiée, décision sur
> l'expiration — **hors V1 lui aussi**, et à ne pas entamer par morceaux.
> Si un vrai portail revient un jour, il repart du design.

---

## Récapitulatif des dépendances

| # | Lot | Dépend de | Produit ce qui manque à |
|---|---|---|---|
| 0 | Socle | — | tous |
| 1 | Membres + fiche | 0 | 6, 7, 8, 10 |
| 2 | Coachs | 0 | 3, 5, 8 |
| 3 | Cours (catalogue) | 0, 2 | 5 |
| 4 | Lieux | 0 | 5 |
| 5 | Planning | 2, 3, 4 | 6, 10 |
| 6 | Présences | 1, 5 | 7, 10 · complète les colonnes du lot 1 |
| 7 | Abonnements & encaissements | 1, 6 | 10 · complète les colonnes du lot 1 |
| 8 | Réglages | 1, 2 | — |
| 9 | Administration | 0 | — |
| 10 | Accueil / tableau de bord | 5, 6, 7 | — |
| 11 | Marques clientes | 0 → 10 | — |
| 12 | Portail membre | design d'abord | — |

**Vues dérivées temporairement vides.** Trois écrans affichent des colonnes ou des
panneaux dont la source arrive plus tard. C'est voulu, et c'est le seul coût de ce
réordonnancement :

| Écran | Vide jusqu'au lot | Quoi |
|---|---|---|
| Membres (1) | 6 et 7 | dernière venue, assiduité, formule, crédits |
| Coachs (2) | 5 | cours/semaine, remplissage, séances de la semaine |
| Cours (3) | 5 | séances/semaine, taux de remplissage, habitués |
| Lieux (4) | 5 | occupation, séances/semaine, planning du jour, heatmap |

Aucune de ces vues n'a de logique propre : elles se remplissent seules quand leur
source existe, sans rouvrir le lot.

## Décisions métier attendues, par lot

Récapitulatif de tout ce qui doit être tranché **avant** de coder le lot concerné.
Aucune de ces règles ne sera inventée.

| Lot | Décision |
|---|---|
| 1 | Seuils de statut membre (« expire bientôt », « inactif ») |
| 1 | Fenêtre et dénominateur du taux d'assiduité |
| 1 | Recherche globale : ici ou lot dédié ? |
| 3 | Stratégie de reprise des `Lesson` existantes vers `CourseTemplate` |
| 4 | Nommage `Location` / `Room` face au « Lieu » de la maquette |
| 4 | ~~Météo : appel réel ou simple champ « lieu de repli »~~ — **tranchée le 2026-08-07 : le champ seul** |
| 5 | Récurrence : occurrences matérialisées ou règle |
| 5 | `DuplicateWeekCommand` : utile ? |
| 6 | Réouverture d'une feuille clôturée : `GymManager` seul, action tracée ? |
| 7 | Règle de calcul du MRR ; les carnets y entrent-ils ? |
| 8 | Envoi réel des notifications : fournisseur SMS et budget |
| 10 | Comportement exact de « Diffuser le planning » |
| 12 | Périmètre du portail membre — maquettes d'abord |

**Toutes tranchées.** Celles des lots 1 à 9 l'ont été au plan de leur lot, avant
d'écrire du code, et sont consignées dans le lot correspondant. Les quatre
dernières — météo (4), SMS (8), diffusion (10), portail membre (12) — l'ont été
le 2026-08-07, section « Décisions prises » en tête de document. **Aucune règle
métier n'a été inventée en chemin.**

Les décisions qui restent à prendre ne sont plus dans ce tableau : elles
appartiennent aux deux handoffs à venir (**login & onboarding**, **portail
super-admin**) et à l'entrée 4 du registre de dette.

## Écarts du README — tous arbitrés le 2026-08-07

Ils étaient les cinq points que la maquette laissait sans réponse. Aucun n'est
resté ouvert.

1. **Recherche globale** — ~~proposition : lot 1~~ → **hors V1**. La barre de la
   topbar reste inerte, et c'est désormais un choix affiché, plus une dette : rien
   ne la promet à l'utilisateur ailleurs qu'en la voyant.
2. **Notifications** (cloche + point rouge) — **hors V1, « pour le moment »**
   (2026-08-07 ; remplace « dans la V1, pour les managers et les coachs »). La
   cloche a été **retirée** des deux shells plutôt que laissée inerte : son point
   rouge était allumé en dur et n'annonçait rien, et un point rouge permanent
   apprend à ne plus regarder les points rouges. Quand elle reviendra, il faudra
   d'abord trancher ce qu'est une notification — dérivée des alertes déjà
   calculées par l'Accueil, ou stockée avec un état « lu » — puis faire dessiner
   l'écran de liste que la maquette ne porte pas. S'y ajoute le périmètre du
   destinataire : un coach est cloisonné à ses séances depuis le lot rôles, ses
   notifications doivent l'être aussi, sinon la cloche annonce ce que l'écran
   refuse de montrer.
3. **Portail membre** — **hors V1**, les membres ne se connectent pas. Voir le
   lot 12.
4. **Météo / repli des cours extérieurs** — **abandonnée pour la V1**
   (2026-08-07 ; remplace « dans la V1, appel réel »). Le champ « lieu de repli »
   reste et suffit. Voir le lot 4 pour l'enquête, gardée pour le jour où la
   question reviendra.
5. **Densité d'affichage** — **écartée**. Outil de maquette, non porté ; ce serait
   une préférence utilisateur, à chiffrer à part si elle revient.
