# Modèle de données — proposition

**Statut : proposition, pas vérité.** Le repo a déjà un `Domain` et un
`GymDbContext`. **Lire l'existant d'abord** et réconcilier : si une entité existe
déjà sous un autre nom, garder le nom existant (le repo signale des incohérences
assumées `Coachs`/`Coaches`, `TechXYZ`/`TechXyz` — ne pas renommer en masse).

Ce document sert à deux choses : (1) s'assurer qu'aucun champ visible dans la
maquette n'a de trou en base, (2) fixer les invariants métier avant de coder.

Contraintes structurelles héritées du repo : toutes les entités dérivent de
`EntityBase<T>` avec `IsActive` (**soft delete**), et — décision de ce handoff —
implémentent `ITenantScoped` (`TenantId`).

---

## Tenant & identité

**`Tenant`** — un client GymXYZ.
`Name`, `Slug`, `ThemeKey` (`techxyz` | `teamtrainers` | `leyssa`), `DisplayName`,
`Baseline`, `Email`, `Phone`, `Siret`, `Address`, `Zip`, `City`, `AreaLabel`
(pour les clients sans adresse : « Thonon et alentours »), `Capacity`,
`IsSolo`, `LogoPath`, `LogoDarkPath`, `CircleLogo`, `GymPlan`, `PlanPrice`,
`PlanRenewalDate`, `OpeningHours` (collection jour/plage).

> `AreaLabel` + `IsSolo` ne sont pas cosmétiques : ils pilotent le masquage de la
> section Coachs et le remplacement de l'adresse par une zone.

**`ApplicationUser`** (Identity) — `TenantId` (nullable pour `PlatformAdmin`),
`DisplayName`, `Nickname`, `RoleLabel` (« Gérante », « Coach », « Accueil »),
`LastSeenAt`. Rôles : `GymManager`, `Coach`, `Member`, `PlatformAdmin`.

**`TeamMemberAccess`** — périmètre d'accès affiché dans Réglages › Équipe
(« Planning, cours & présences », « Membres & encaissements »).
**`Invitation`** — `Email`, `Role`, `SentAt`, `AcceptedAt`.

## Personnes

**`Member`** — `FirstName`, `LastName`, `Email`, `Phone`, `JoinedOn`,
`Address` (utile pour les séances à domicile), `Notes`.
Dérivés (**non stockés**) : statut (`Actif` / `Expire bientôt` / `Inactif`), taux
d'assiduité, dernière venue, crédits restants.

**`Coach`** — `FirstName`, `LastName`, `RoleLabel`, `Bio`, `Email`, `Phone`,
`JoinedOn`, `Availability` (7 booléens L→D), `AwayUntil`, `Rating`,
`UserId` (nullable — un coach peut exister sans compte).
**`CoachDiscipline`** (n-n avec `Discipline`) · **`CoachCertification`**
(`Label`).
Dérivés : cours/semaine, remplissage moyen, nombre de membres suivis.

## Offre

**`Discipline`** — `Name`, `IconKey`, `Tone`.

**`CourseTemplate`** — le **modèle** de cours (catalogue) : `Name`, `DisciplineId`,
`IconKey`, `DurationMinutes`, `Capacity`, `DefaultLocationId`, `Level`,
`Intensity`, `PriceLabel` (`Inclus` / `45 € / séance`), `Description`.
**`CourseTemplateCoach`** (n-n).
Dérivés : séances/semaine, remplissage, habitués.

**`Session`** — une **occurrence** datée : `CourseTemplateId`, `CoachId`,
`LocationId`, `StartsAt`, `EndsAt`, `Capacity` (copie au moment de la création :
changer un modèle ne doit pas réécrire l'histoire), `Status`
(`Scheduled` | `Live` | `Done` | `Cancelled`), `CancellationReason`,
`AttendanceClosedAt`.

> **Décision à prendre au lot 5 (Planning)** : récurrence. Deux options — matérialiser les
> occurrences (une ligne par séance, plus simple à interroger et à pointer,
> recommandé) ou stocker une règle de récurrence et générer à la volée (plus
> souple, beaucoup plus de complexité de requête). Le prototype est compatible avec
> l'option matérialisée.

**`Registration`** — inscription d'un membre à une séance : `SessionId`,
`MemberId`, `RegisteredAt`, `AttendanceStatus`
(`Pending` | `Present` | `Late` | `Absent`), `CheckedInAt`, `IsWaitlisted`.

> Une seule table porte inscription **et** présence : c'est ce que fait la feuille
> de pointage (les inscrits sont la liste, le statut est la colonne).

## Lieux

**`Location`** — `Name`, `Kind` (`Studio` | `Outdoor` | `Home`), `TypeLabel`
(« Cours collectifs », « Cycling & cardio »), `IconKey`, `Tone`, `Capacity`,
`AreaSqm`, `Floor`, `Note`, `Address`, `Latitude`, `Longitude`,
`WeatherDependent`, `FallbackLocationId`.
**`LocationEquipment`** — `Label`.
Dérivés : occupation %, séances/semaine, heatmap 7 jours.

## Abonnements & argent

**`Plan`** (formule) — `Name`, `Price`, `Unit` (`€ / mois`, `€ / carte`, `€ / an`),
`Kind` (`Recurring` | `CreditPack`), `CreditCount` (carnets), `ValidityMonths`,
`BillingLabel` (`Sans engagement`, `Engagement 12 mois`), `Description`,
`Tone`, `IsFeatured`.

**`Subscription`** — `MemberId`, `PlanId`, `StartedOn`, `EndsOn` (récurrent),
`CreditsRemaining` (carnet), `AutoRenew`, `Status`
(`Active` | `ExpiringSoon` | `Late` | `Ended`), `PriceLabel`.

**`Payment`** — `MemberId`, `SubscriptionId`, `Date`, `Label`, `Amount`,
`Method` (`Card` | `SepaDirectDebit` | `Cash` | `Cheque` | `PaymentLink`),
`Status` (`Collected` | `Rejected` | `Pending`).

> **Le MRR est calculé**, jamais stocké : somme normalisée mensuelle des
> abonnements récurrents actifs. Les carnets n'entrent pas dans le MRR (le préciser
> avec le métier : ils peuvent entrer dans un CA lissé, c'est un autre indicateur).
> Un carnet est décrémenté par le **pointage** (lot 6), pas par l'inscription —
> sinon un no-show consomme un crédit sans séance.

## Paramétrage

**`GymSettings`** (1 ligne par tenant) — `Currency`, `VatMention`,
`AcceptedPaymentMethods` (drapeaux), `SchoolZone` (dérivée du CP, en cache).
**`NotificationSetting`** — `GroupKey`, `Key`, `IsEnabled`, `Channels`
(Email/SMS). Six réglages dans la maquette : relance avant échéance, paiement en
retard, nouvelle inscription, rappel de cours, place libérée, annulation de cours.
**`Invoice`** (facturation du client à TechXYZ) — `Date`, `Reference`, `Amount`,
`Status`.

---

## Invariants à faire respecter par les validators

1. Une `Session` ne peut pas dépasser la capacité du `Location`.
2. Deux `Session` du même `Location` ne peuvent pas se chevaucher.
3. Un `Coach` ne peut pas être sur deux séances simultanées ; alerte (pas blocage)
   s'il est marqué indisponible ce jour-là ou en congé.
4. Une `Registration` sur une séance complète part en liste d'attente
   (`IsWaitlisted`), et la libération d'une place déclenche la notification
   « Place libérée ».
5. Une feuille de présence clôturée (`AttendanceClosedAt`) est en lecture seule ;
   seul un `GymManager` peut la réouvrir.
6. Pointer `Present`/`Late` sur un carnet décrémente **une fois** (idempotence :
   repointer ne redécrémente pas).
7. Un `Member` sans abonnement actif peut être inscrit mais l'écran signale le
   risque de paiement.
8. Une entité désactivée (`IsActive = false`) n'apparaît nulle part et ne peut pas
   recevoir de nouvel enfant.
9. Toute écriture porte le `TenantId` du contexte courant ; un `PlatformAdmin` ne
   peut écrire qu'après avoir explicitement choisi un tenant.

## Seed de démo

`design/app/data.js` contient **un jeu de données de démo crédible et cohérent**
(`GX_DATA`) : 6 membres, 6 coachs, 8 modèles de cours, 6 séances de pointage avec
listes nominatives, 4 formules, 8 abonnements, 5 encaissements, 6 lieux, tous les
réglages. **Le porter tel quel** dans l'initialisation de la base de développement :
c'est ce jeu qui sert aux démos commerciales, il est déjà relu et sans incohérence
(mêmes personnes d'un écran à l'autre, taux crédibles).

Semaine de référence de la démo : **8 → 14 juin 2026**. Garder ces dates ou
translater le jeu par rapport à « aujourd'hui » (préférable pour que la démo ne
vieillisse pas — dans ce cas, écrire les dates en décalages relatifs dans le seed).

Le seed dépend de `ResetDatabaseOnStartup` (existant) : **ne pas** recréer la base
sans que ce drapeau soit explicitement activé.
