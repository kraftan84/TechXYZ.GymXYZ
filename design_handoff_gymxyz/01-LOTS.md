# Plan d'implémentation — 12 lots

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

## Lot 0 — Socle : tenant, thème, shells, auth

**Le lot le plus important.** Tout le reste en dépend, et c'est lui qui rend le lot
11 trivial. Ne pas le raccourcir.

**Domain / Persistence**
- `Tenant` : `Name`, `Slug`, `ThemeKey`, `DisplayName`, `Baseline`, coordonnées,
  `IsSolo` (coach indépendant), `GymPlan` (formule GymXYZ), `LogoAssetPath`.
- `ITenantScoped { Guid TenantId }` ; filtre global EF combiné avec le soft delete
  existant (`e => e.TenantId == _tenant.Current && e.IsActive`).
- `ITenantContext` (scoped) résolu depuis l'hôte, repli config en dev,
  outrepassable par un `PlatformAdmin`.
- Identity : rôles `GymManager`, `Coach`, `Member`, `PlatformAdmin`.
  `ApplicationUser` porte `TenantId` (nullable pour `PlatformAdmin`).
- Seed : 1 tenant GymXYZ + 1 gérant + le jeu de démo (voir `05-DATA-MODEL.md`).

**WebApp**
- Styles : `wwwroot/css/techxyz/` (tokens + styles.css + woff2), `themes.css`,
  `app.css`, `mobile.css` repris du prototype **sans réécriture**.
- `FluentDesignTheme` thèmé sur l'accent du tenant (`CustomColor` = accent solide,
  `--accent-fill-*` en cascade) ; neutres mappés sur la rampe slate ; police
  Montserrat ; `--control-corner-radius` 10px / `--layer-corner-radius` 14px ;
  hauteur de contrôle 44px.
- `<html data-theme="@ThemeKey">` posé côté serveur au rendu initial (pas de
  flash de thème).
- Shell desktop : `Sidebar` (256px, groupes Pilotage / Personnes / Offre & business
  / Lieux, pied « Administration », badges de compteur), `Topbar` (64px, recherche,
  cloche, engrenage → Réglages, bloc utilisateur), `Brand` (marque + wordmark
  thèmé, variante fond sombre).
- Shell mobile : `MHead` (mode marque / mode titre+retour), `MTabBar` (Accueil,
  Planning, Présences, Membres, Plus), `MSheet` + `MPlusSheet` (reste de la nav +
  profil).
- Primitives partagées : `PageHead`, `Crumb`, `CardHead`, `Kpi`, `Bar`, `Ring`,
  `Chip` (→ `FluentBadge` thèmé ou maison), `EmptyState`.
- Point de rupture responsive et pattern de bascule desktop/mobile **décidés et
  documentés ici**.
- Login (Identity) — le prototype n'a pas d'écran de login : reprendre le
  `LoginScreen` du kit `app/` du design system TechXYZ (fond encre, carte centrée,
  marque, champ e-mail/mot de passe, bouton primaire pleine largeur).

**Tests** : résolution du tenant (hôte connu / inconnu / admin), filtre global
(une entité d'un autre tenant est invisible), soft delete toujours actif, rôles.

**DoD spécifique** : naviguer entre 10 sections vides sans erreur ; changer
`ThemeKey` en base repeint toute l'app sans toucher un `.razor`.

---

## Lot 1 — Accueil / tableau de bord

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

**Attention** : les KPI et les alertes sont **calculés**, pas stockés. Définir les
règles avec le métier : « abonnement qui expire » = échéance ≤ 7 jours ; « paiement
en retard » = échéance dépassée et non encaissé ; « à pointer » = séance terminée
dont la feuille n'est pas clôturée.

---

## Lot 2 — Planning

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
chevauchements). Prévoir du temps. Modèle d'occurrence : `CourseTemplate` (modèle
de cours) → `Session` (occurrence datée) — voir `05-DATA-MODEL.md`.

---

## Lot 3 — Membres + fiche membre

**Desktop** (`design/app/screen-membres.jsx`) : tableau (avatar + nom, e-mail,
formule avec chip, jauge de crédits `3/10` ou `∞`, assiduité %, dernière venue,
statut), filtres, recherche, action « Ajouter un membre ». Fiche membre en
**drawer 520px** ou page : identité, formule, paiements, prochains cours, historique
récent avec présent/absent.

**Mobile** (`design/app/mobile/screen-membres.jsx`) : liste + fiche plein écran
(retour dans le header).

**Application** : `GetMembersQuery` (filtres + pagination server-side),
`GetMemberDetailQuery`, `CreateMemberCommand`, `UpdateMemberCommand`,
`DeactivateMemberCommand` (soft delete).

**Statuts** : `Actif` (succès), `Expire bientôt` (warning), `Inactif` (danger) —
**dérivés** de l'abonnement et de la dernière présence, pas saisis.

**Inclut la recherche globale** de la topbar si l'écart n°1 du README est tranché.

---

## Lot 4 — Cours (catalogue de modèles)

**Desktop** (`design/app/screen-cours.jsx`) : catalogue de **modèles** de cours en
cartes (icône de discipline, nom, discipline, durée, capacité, studio, niveau,
intensité, prix, séances/semaine, taux de remplissage, habitués, coachs
rattachés) + fiche modèle (description, prochaines occurrences, coachs).

**Mobile** (`design/app/mobile/screen-cours.jsx`) : liste + fiche.

**Application** : `GetCourseTemplatesQuery`, `GetCourseTemplateDetailQuery`,
`CreateCourseTemplateCommand`, `UpdateCourseTemplateCommand`,
`ArchiveCourseTemplateCommand`.

**Attention** : distinguer clairement **modèle** (catalogue, ce lot) et
**occurrence** (planning, lot 2). Le taux de remplissage et les habitués sont
calculés depuis les occurrences passées.

---

## Lot 5 — Coachs

**Desktop** (`design/app/screen-coachs.jsx`) : grille de cartes 3 colonnes
(avatar, nom, rôle, chips de disciplines, cours/semaine, remplissage, note,
membres suivis, chip de disponibilité) + fiche (bio, certifications, disponibilités
sur 7 jours, séances de la semaine, contact).

**Mobile** (`design/app/mobile/screen-coachs.jsx`) : liste + fiche.

**Application** : `GetCoachesQuery`, `GetCoachDetailQuery`, `CreateCoachCommand`,
`UpdateCoachCommand`, `DeactivateCoachCommand`. Un coach peut être lié à un compte
Identity (rôle `Coach`) — le lien est facultatif (un coach existe sans compte).

**Attention marque blanche** : quand `Tenant.IsSolo` est vrai (Leyssa), la section
**Coachs disparaît de la navigation** (sidebar desktop et sheet « Plus » mobile), et
toute route directe redirige vers l'accueil. Le prototype le fait déjà
(`theme.solo`). Implémenter la règle **ici**, pas au lot 11.

---

## Lot 6 — Présences (pointage)

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

**Attention** : le pointage est l'écran le plus utilisé du produit sur mobile, en
salle, parfois avec une main. Réactivité optimiste (mise à jour immédiate, erreur
via `IUserFeedbackService`), et **verrouillage** d'une feuille clôturée (réouverture
réservée au `GymManager`).

---

## Lot 7 — Administration (super-admin TechXYZ)

**Desktop et mobile** (`design/app/screen-reglages.jsx` → `ScreenAdministration`) :
navigation de sections à gauche + panneaux. Deux panneaux :
- **Apparence & marque** : choix du thème du client (cartes de thème avec aperçu
  des couleurs), marque, wordmark, baseline.
- **Facturation** : formule GymXYZ du client (`GymXYZ Pro`, 79 €/mois), échéance,
  moyen de paiement, historique de factures.

**Application** : `GetTenantsQuery`, `GetTenantDetailQuery`, `CreateTenantCommand`,
`UpdateTenantBrandingCommand`, `UpdateTenantPlanCommand`.

**Attention** : seul écran réservé à `PlatformAdmin` — autorisation par policy, pas
par masquage d'UI. C'est ici que se pilote la marque blanche : changer le thème d'un
client depuis cet écran doit repeindre son app, sans redéploiement.

---

## Lot 8 — Réglages (côté client)

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

**Attention** : barre d'enregistrement collante avec état « Enregistré » (le
prototype a `SaveBar`). Les bascules de notification ne déclenchent **pas** encore
d'envoi réel : stocker les préférences, brancher l'envoi dans un lot ultérieur (à
cadrer — SMS = coût et fournisseur à choisir).

---

## Lot 9 — Lieux / studios

**Desktop** (`design/app/screen-salles.jsx`) : cartes 2 colonnes (nom, type, icône,
capacité, surface, étage, occupation, séances/semaine, chip de statut) + fiche
(note, équipements en chips, planning du jour, **heatmap d'occupation sur 7 jours**).

Trois natures de lieu, et c'est structurant : **studio** (interne), **extérieur**
(parc — adresse, dépendance météo, lieu de repli), **domicile** (chez le membre,
capacité 1, adresse portée par la fiche membre).

**Mobile** (`design/app/mobile/screen-salles.jsx`) : liste + fiche.

**Application** : `GetLocationsQuery`, `GetLocationDetailQuery`,
`CreateLocationCommand`, `UpdateLocationCommand`, `DeactivateLocationCommand`.

**Décision attendue** : implémenter réellement la météo + repli automatique, ou se
limiter au champ « lieu de repli » (recommandé pour ce lot).

---

## Lot 10 — Abonnements & encaissements

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
séances, décrémentée par le pointage du lot 6). Le MRR est un calcul, pas un champ.
Aucun paiement en ligne dans ce lot (pas de PSP) : on **enregistre** des
encaissements réalisés hors ligne. Brancher un PSP est un lot à part.

---

## Lot 11 — Marques clientes : Team Trainer's & Leyssa Coaching

Si les lots précédents ont été faits correctement, ce lot ne touche **aucun écran**.

- Deux blocs `[data-theme="teamtrainers"]` et `[data-theme="leyssa"]` : déjà écrits
  dans `design/app/themes.css`, à reprendre tels quels.
- Deux tenants en base avec `ThemeKey`, marque, coordonnées, `IsSolo` pour Leyssa.
- Assets de marque dans `wwwroot/assets/themes/`.
- Polices Anton et Dancing Script auto-hébergées.
- Vérifications par marque (détail dans `02-THEMING.md`) : sidebar sombre lisible
  pour Team Trainer's (logo blanc) ; Leyssa en mode solo (pas de section Coachs),
  marque en cercle, **aucune adresse postale** (zone uniquement : « Thonon »),
  contraste du rose vérifié sur les boutons pleins.
- Ajouter un test qui, pour chaque thème, instancie les pages principales et vérifie
  qu'aucun style en dur n'est apparu (au minimum : revue visuelle des 10 sections
  × 3 marques, à cocher).

**DoD** : capture des 10 sections × 3 marques, desktop et mobile.

---

## Lot 12 — Portail membre (à cadrer avant de coder)

Le rôle `Member` est demandé mais **aucun écran membre n'existe dans la maquette**.
Ne pas improviser. Périmètre probable : voir le planning, réserver/annuler une
place, consulter son abonnement et ses crédits, son historique de présence, ses
paiements. À maquetter d'abord (retour en phase design), puis lot d'implémentation.

---

## Récapitulatif de l'ordre

| # | Lot | Dépend de |
|---|---|---|
| 0 | Socle (tenant, thème, shells, auth) | — |
| 1 | Accueil / tableau de bord | 0 (agrège 2,6,10 → poser des données de seed) |
| 2 | Planning | 0, 4 partiellement |
| 3 | Membres + fiche | 0 |
| 4 | Cours (catalogue) | 0 |
| 5 | Coachs | 0 |
| 6 | Présences | 2, 3 |
| 7 | Administration | 0 |
| 8 | Réglages | 0, 3 |
| 9 | Lieux | 0 |
| 10 | Abonnements & encaissements | 3 |
| 11 | Marques Team Trainer's & Leyssa | 0 → 10 |
| 12 | Portail membre | design d'abord |

**Note sur l'ordre demandé** : l'accueil (lot 1) agrège des données produites par
les lots 2, 6 et 10. Deux options — soit le construire tôt sur des données de seed
puis le raffiner après le lot 10 (recommandé : la démo commerciale a besoin de
l'accueil), soit le décaler. Le tableau ci-dessus retient la première option.
