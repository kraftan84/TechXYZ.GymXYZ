# Lot 8 — Réglages (côté client) · brief de démarrage

Écrit le 2026-08-06, juste après la fusion du lot 7. `main` est à `e0cf942`,
**325 tests au vert**, lots 0 à 7 livrés.

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

---

## Où en est le projet

Le lot 7 a fermé la boucle de l'argent. Ce qui reste vide n'est plus une donnée
métier mais **la configuration** : l'application n'a aujourd'hui aucune ligne de
réglages, et `Reglages.razor` est un `EmptyState`.

**Trois contrôles attendent explicitement le canal de messagerie de ce lot**, et
chacun est dessiné désactivé avec une infobulle qui le dit :

- « Relancer » sur la carte des absents — `PresencesDesktop.razor:13` (lot 6)
- « Relancer » dans le tiroir d'encaissement, et `SendPaymentReminderCommand`
  qui écrit déjà `Subscription.LastReminderSentOn` sans rien envoyer (lot 7)
- Prévenir les inscrits d'une séance annulée — `CancelSessionCommand.cs:12`

Le lot 8 est le dernier lot côté client avant l'Administration (lot 9) et le
tableau de bord (lot 10).

**Lire `Components/Features/Abonnements/` avant de commencer** — exemple le plus
frais, et son `GetPlansQuery` a été écrit sans paramètre exprès pour que le
panneau « Formules & tarifs » l'appelle tel quel.

---

## Périmètre

**Sources** : `01-LOTS.md` §« Lot 8 — Réglages (côté client) » (lignes 345–381),
`design/app/screen-reglages.jsx` (387 lignes), `design/app/mobile/screen-reglages.jsx`
(133 lignes), `03-SCREENS-DESKTOP.md` §9, `05-DATA-MODEL.md` §« Paramétrage »
(lignes 110–118), `data.js` → `GX_DATA.reglages`.

> **Attention au fichier partagé.** `screen-reglages.jsx` contient **deux**
> écrans. `ScreenReglages` = `RG_SECTIONS_USER`, les quatre panneaux de ce lot.
> `ScreenAdministration` = `RG_SECTIONS_ADMIN` (`SecMarque`, `SecFacturation`)
> et relève du **lot 9**. Ne pas construire `gx-theme-card`, `gx-theme-grid`,
> `gx-token`, `gx-tokens`, `gx-plan-hero`, `gx-disc-ic` : ils appartiennent tous
> à l'écran d'administration.

**Quatre panneaux**, navigation de sections à gauche (`gx-tabs` / `gx-tab`),
panneau à droite (`gx-set-panel`), champs en `gx-field-grid` (2 colonnes, `.full`
pour pleine largeur), et **barre d'enregistrement collante** (`gx-savebar`) avec
état « Enregistré ».

| Panneau | Contenu |
|---|---|
| **Identité** | nom, baseline, capacité, SIRET, adresse, CP, ville, e-mail, téléphone, horaires d'ouverture, carte « calendrier scolaire » (zone déduite du CP) |
| **Équipe & accès** | équipe (rôle, périmètre, dernière connexion), invitations en attente, KPI accès des membres (112 total / 98 avec compte / 6 invités), invitation par e-mail |
| **Formules & tarifs** | les formules d'abonnement, devise, régime de TVA, moyens de paiement acceptés (bascules) |
| **Notifications** | deux groupes (Membres & abonnements / Cours & présences), six réglages, canaux Email/SMS |

**Application** : `GetGymSettingsQuery`, `UpdateGymIdentityCommand`,
`InviteTeamMemberCommand`, `UpdateTeamMemberAccessCommand`, `RevokeAccessCommand`,
`UpdateNotificationSettingsCommand`, `UpdatePaymentMethodsCommand`.

---

## Décisions déjà prises — ne pas les rouvrir

### 1. Le kettlebell disparaît complètement

Le logo par défaut de GymXYZ ne doit plus apparaître **nulle part** : ni pour
l'habillage `techxyz`, ni comme repli. En marque blanche on garde **juste le
nom**.

Aujourd'hui `BrandLockup.razor:6` fait :

```razor
@if (Brand.MarkKind == TenantMarkKind.Kettlebell || MarkSource is null)
{
    <KettlebellMark Size="@MarkSize"/>
}
```

Autrement dit **tout tenant sans `LogoPath` hérite de la marque de GymXYZ** —
une fuite de marque dans un produit vendu en marque blanche.

Après : un `.mark` n'est rendu **que** s'il y a une image ; sinon le wordmark
seul. `KettlebellMark.razor` est supprimé.

**Piège de mise en page** : `.gx-brand .mark` est une boîte fixe de 38 px (34 px
en mobile) avec `gap: 10px`. Vider le contenu sans retirer le `<span>` laisse le
wordmark indenté de 48 px. Il faut ne pas rendre l'élément du tout.

**Trois surfaces à vérifier** : `Sidebar.razor` (sur fond sombre),
`Mobile/MobileHeader.razor`, `Account/Login.razor`.

**Conséquence à trancher en chemin** : `TenantMarkKind` n'a plus qu'une valeur
qui veut dire quelque chose. L'enum et la colonne `Tenant.MarkKind` se réduisent
à « `LogoPath` existe ou non ». **Recommandation : les retirer** (7 fichiers :
`TenantMarkKind.cs`, `Tenant.cs`, `TenantBrandDto.cs`,
`QueryableProjectionExtensions.cs:16`, `DbInitializer.cs:49`, `BrandLockup.razor`).
C'est une migration — la proposer, pas la faire en silence.

### 2. E-mail réel dans ce lot, SMS plus tard

L'e-mail part **pour de bon**. Les bascules SMS persistent sans envoyer, et le
disent.

**Aucune infrastructure d'envoi n'existe** : ni `IEmailSender`, ni SMTP, ni
paquet, ni configuration. Le chemin sortant est à créer de zéro — abstraction
côté `Application`, implémentation côté infrastructure, conformément à
`IMPLEMENTATION_INSTRUCTIONS.md`.

### 3. Les préférences sont stockées, l'envoi les respecte

Les six bascules et leurs canaux persistent. Chaque envoi de la PR 3 les
consulte avant de partir.

---

## Découpage proposé — trois PR séquentielles, toutes sur `main`

Jamais de PR empilée : le lot 5 s'est perdu entièrement comme ça
(voir [[pr-always-target-main]]).

### PR 1 — La plomberie : marque blanche et annuaire des comptes

Deux chantiers sans écran, qui débloquent le reste.

**a) Le lockup perd sa marque par défaut** — décision 1 ci-dessus.

**b) La dette du lot 0 : `IUserDirectory`.** `Application` ne référence pas
`Persistence`, donc `IGymDbContext` ne peut pas exposer `ApplicationUser`. Or
« Équipe & accès » doit lire les comptes (rôle, périmètre, dernière connexion)
et écrire des invitations. Il faut une abstraction dans `Application`,
implémentée côté `Persistence`. Un marqueur l'annonce déjà dans
`CoachDetailsPageDto.cs:29`.

> **C'est le gros du lot, pas les écrans.** À chiffrer avant de dessiner quoi que
> ce soit. Les lots 4, 5 et 7 ont tous fait atterrir leur refactor en premier et
> ça a marché à chaque fois.

### PR 2 — Les quatre panneaux

Entités à créer : `GymSettings` (une ligne par tenant — `Currency`,
`VatMention`, `AcceptedPaymentMethods`, `SchoolZone`) et `NotificationSetting`
(`GroupKey`, `Key`, `IsEnabled`, `Channels`).

`Tenant` porte **déjà** `Name`, `Baseline`, `Email`, `Phone`, `Siret`, `Street`,
`ZipCode`, `City`, `Capacity`, `AreaLabel` : le panneau Identité édite surtout le
tenant, et `GymSettings` reste petit.

**Rien ne stocke les horaires d'ouverture aujourd'hui** — trois lignes dans la
maquette (« Lundi – vendredi 06:30 – 22:00 », samedi, dimanche). À modéliser.

Les sept commandes/requêtes, la `SaveBar` collante, les deux présentations.

### PR 3 — L'envoi d'e-mail et les trois contrôles qui l'attendent

`SendPaymentReminderCommand` envoie enfin au lieu de seulement horodater ;
l'annulation de séance prévient les inscrits ; « Relancer » des absents s'active.
Chaque envoi passe par les préférences de la PR 2.

**À trancher au démarrage de cette PR — demander, ne pas supposer :**

- l'adresse d'expédition (celle du tenant ? une adresse de service ?)
- le fournisseur : SMTP maison ou service transactionnel
- le comportement en cas d'échec : une relance qui n'part pas doit-elle faire
  échouer la commande, ou passer en toast sans perdre ce qui a été enregistré ?

---

## Règle marque blanche à vérifier ici, pas au lot 11

Quand `Tenant.AreaLabel` est renseigné (client solo, sans adresse postale — le
cas de Leyssa Coaching, coach itinérante autour de Thonon), le panneau Identité
affiche la **zone** au lieu de l'adresse. `01-LOTS.md` demande explicitement que
ce soit vérifié dans ce lot.

---

## Ce qui existe déjà et fera gagner du temps

- **Les 17 classes CSS du prototype existent toutes**, vérifiées une par une
  contre `app.css` : `gx-set-panel`, `gx-set-intro`, `gx-field-grid`,
  `gx-savebar`, `gx-tabs`, `gx-tab`, `gx-team-row`, `gx-notif-row`, `gx-chan`,
  `gx-pay-row`, `gx-hours-row`, `gx-formrow`, `gx-seclab`, `gx-listrow`,
  `gx-grid3`, `gx-seg`, `gx-pagehead`. Mobile : `gx-m-set*` et `gx-m-setrow`,
  11 règles chacune. **Markup seul**, comme aux lots 4 à 7.
- **La navigation de sections, c'est `gx-tabs` / `gx-tab`** — il n'y a pas de
  classe dédiée type `gx-set-nav`.
- **`SchoolZones.ForPostcode` existe** (`Application/Common/SchoolZones.cs`,
  écrit au lot 5) : la carte calendrier scolaire a déjà sa règle.
- **`GetPlansQuery` est sans paramètre exprès** pour que le panneau Formules
  l'appelle tel quel — précédent `CalendarCard` du lot 5.
- **`GxMobileTabs` a été livré au lot 7** (`Components/Shared/`) précisément pour
  les sections mobiles de cet écran, avec `.gx-m-tabs` dans `mobile.css`.
- **`SubscriptionLabels.Label(PaymentMethod)`** existe déjà et donne
  « Prélèvement », « Espèces », « Chèque », « Lien de paiement » — les moyens de
  paiement du panneau Tarifs.
- Le jeu de démo de `GX_DATA.reglages` est complet : identité, 4 membres
  d'équipe, 1 invitation en attente, 112/98/6, devise + TVA
  « art. 293 B du CGI » + 5 moyens, et les 6 notifications.

---

## Pièges déjà payés sur les lots précédents

- Le **commentaire Razor à l'intérieur d'une balise** et la **collision de nom de
  classe CSS** du lot 6 — voir [[gymxyz-lot1-patterns]].
- Les **attributs ARIA d'état écrits en toutes lettres**, jamais liés à un bool :
  Blazor supprime l'attribut quand le bool est faux (commit `bf67bdb`).
- **`ExecuteUpdateAsync` ne marche pas** sur le provider InMemory des tests —
  charger et muter à la place (rencontré au lot 7 sur `UpdatePlanCommand`).
- La **base de dev est recréée au démarrage** et déconnecte l'utilisateur.
  Demander une connexion et s'attendre à redémarrer — voir
  [[gymxyz-lot4-verification-gaps]].
- La **navigation scriptée dans le navigateur** peut faire courir deux chargements
  sur le même `DbContext` et lever un toast d'erreur qui n'est pas une régression.
  Vérifier en cliquant dans l'app plutôt qu'en forçant l'URL.

---

## Premier geste suggéré

Lire `screen-reglages.jsx` en entier mais **ne retenir que `ScreenReglages`**,
puis l'écran mobile, puis `05-DATA-MODEL.md` §Paramétrage. Revenir avec :

1. le chiffrage de `IUserDirectory` — c'est lui qui décide de la taille du lot ;
2. la position sur `TenantMarkKind` (retiré ou gardé) ;
3. un plan qui fait atterrir la plomberie avant les écrans.

Attendre la validation avant d'écrire.
