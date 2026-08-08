# Lot 9 — Administration (super-admin TechXYZ) · brief de démarrage

Écrit le 2026-08-06, juste après la fusion du lot 8. `main` est à `e817626`,
**428 tests au vert**, lots 0 à 8 livrés.

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

---

## À lire avant tout le reste

**Le prototype ne dessine pas l'écran que le lot demande.** C'est le point
central de ce lot, et il faut le trancher avant de toucher au code.

`01-LOTS.md` décrit une **console multi-clients** : `GetTenantsQuery`,
`CreateTenantCommand`, et l'impersonation d'un client par un `PlatformAdmin`.

`ScreenAdministration` dessine autre chose : **deux panneaux qui parlent d'un
seul client**, celui qu'on regarde. « Apparence & marque » change le thème
courant ; « Facturation » affiche *votre* abonnement GymXYZ, *votre* carte
bancaire, *vos* factures. Il n'y a **aucune liste de clients** nulle part dans
la maquette.

L'écran mobile est encore plus explicite. `mobile/screen-administration.jsx`
titre « Votre compte {salle} chez GymXYZ », ne montre **que** la facturation, et
son en-tête dit en toutes lettres :

> *Mirrors the desktop "Administration" screen (white-label theme lives in
> Tweaks, not surfaced here).*

Autrement dit : dans le prototype, changer de client est un **outil de démo**
(le panneau Tweaks), pas une fonction du produit. La liste des clients n'a jamais
été dessinée parce que la maquette n'en a jamais eu besoin.

**La règle habituelle ne tranche pas ici.** « Le prototype gagne sur les docs »
sert quand les deux décrivent la même chose différemment. Ici ils décrivent deux
écrans différents, et le prototype n'a simplement pas dessiné celui que
`01-LOTS.md` commande. Il faut demander, pas choisir en silence.

**Trois lectures possibles, à faire arbitrer :**

| Lecture | Ce que ça donne |
|---|---|
| **A — Console TechXYZ** | Liste des clients, création, impersonation, puis marque et facturation *du client sélectionné*. C'est ce que demande `01-LOTS.md`. La liste et la création sont à dessiner de zéro. |
| **B — Compte du client** | Les deux panneaux tels que dessinés, sur le tenant courant : la salle voit sa propre marque et sa propre facture. Fidèle à la maquette, mais ne réserve plus rien à `PlatformAdmin` et laisse la console hors périmètre. |
| **C — Les deux** | B tel quel, plus un sélecteur de client en tête d'écran pour un `PlatformAdmin`. Le plus proche du produit visé, et le plus de travail. |

Recommandation à défendre au moment du plan : **A**, parce que la réservation
`PlatformAdmin` est déjà en place et n'a de sens que là, et parce que le lot 11
(marque blanche) s'appuiera dessus. Mais c'est une décision de produit, pas une
décision technique.

---

## Périmètre

**Sources** : `01-LOTS.md` §« Lot 9 » (lignes 383–406),
`design/app/screen-reglages.jsx` → `ScreenAdministration` (`SecMarque` lignes
93–152, `SecFacturation` lignes 292–336, `RG_SECTIONS_ADMIN` ligne 16),
`design/app/mobile/screen-administration.jsx` (67 lignes),
`03-SCREENS-DESKTOP.md` §10 (lignes 177–182),
`05-DATA-MODEL.md` §« Paramétrage » (l'entité `Invoice`),
`data.js` → `GX_THEMES` (le catalogue des 3 clients) et
`GX_DATA.reglages.facturation`.

**Attention au fichier partagé, dans l'autre sens.** `screen-reglages.jsx`
contient les deux écrans ; le lot 8 a construit `RG_SECTIONS_USER` et laissé
`RG_SECTIONS_ADMIN`. Ce lot prend ce qui reste — et **uniquement** ce qui reste :
`SecIdentite`, `SecEquipe`, `SecTarifs` et `SecNotifs` sont livrés et ne se
retouchent pas.

**Application** (d'après `01-LOTS.md`) : `GetTenantsQuery`,
`GetTenantDetailQuery`, `CreateTenantCommand`, `UpdateTenantBrandingCommand`,
`UpdateTenantPlanCommand`.

---

## Ce qui bloque aujourd'hui

Quatre choses à régler avant que l'écran puisse seulement être ouvert. Elles
sont plus importantes que les panneaux eux-mêmes.

### 1. Personne ne peut atteindre `/administration`

`Administration.razor` porte déjà `[Authorize(Policy = GymPolicies.PlatformAdmin)]`,
et `DbInitializer` sème bien les quatre rôles — mais **ne crée aucun compte
`PlatformAdmin`**. Le seul compte du jeu de démo est le gérant
`dwayne.johnson@gymxyz.fr`, qui est `GymManager`.

L'écran est donc inaccessible, aujourd'hui, à tout le monde. Premier geste du
lot : semer un compte super-admin (`TenantId` nul — `ApplicationUser` le prévoit
déjà : « Null for a PlatformAdmin »).

### 2. Il n'y a qu'un seul client en base

`DbInitializer` crée **un** `Tenant` (GymXYZ). Or :

- `themes.css` porte **trois** blocs : `techxyz`, `teamtrainers`, `leyssa` ;
- `GX_THEMES` décrit ces trois clients, avec leur gérant et leur marque ;
- le lot 8 a écrit un test sur Leyssa Coaching et sa zone « Thonon et alentours ».

Une liste de clients avec une ligne ne démontre rien. Semer les deux autres est
le prérequis pour que cet écran veuille dire quelque chose — et c'est aussi ce
qui prouvera enfin la promesse marque blanche de bout en bout.

### 3. Les logos des clients ne sont pas dans l'application

`Tenant.LogoPath`, `LogoDarkPath` et `CircleLogo` existent depuis le lot 0 et
**personne ne les remplit**. Les fichiers sont dans le hand-off, pas dans
`wwwroot` :

```
design_handoff_gymxyz/design/assets/themes/teamtrainers-mark.png
design_handoff_gymxyz/design/assets/themes/teamtrainers-white.png
design_handoff_gymxyz/design/assets/themes/teamtrainers-full.png
design_handoff_gymxyz/design/assets/themes/leyssa-mark.png
```

Depuis le lot 8 PR 1, un client sans logo affiche **son nom seul** — c'est le
comportement voulu, pas un bug. Mais tant que ces fichiers ne sont pas copiés,
Team Trainer's et Leyssa s'afficheront en texte, et la carte « Logo » du panneau
Apparence n'aura rien à montrer. Le `teamtrainers-white.png` est la variante
`LogoDarkPath` : c'est le seul endroit du produit qui exerce la bascule
clair/sombre du lockup.

### 4. `UseTenant` n'est pas de l'impersonation

`ITenantContext.UseTenant` existe et marche — mais lisez `TenantContext` avant de
prévoir quoi que ce soit : c'est un **override dans le scope courant**, qui se
rembobine au `Dispose`. Parfait pour « exécute cette requête-là comme le client
X » à l'intérieur d'un handler. **Insuffisant pour « voir l'application comme ce
client »** : le contexte est rempli une fois par requête HTTP ou par circuit
Blazor depuis les claims de l'utilisateur, donc l'override ne survit pas à une
navigation.

Une vraie impersonation demande de la persister — un claim, un cookie, ou une
valeur que `TenantScope` relit à chaque requête — **et une trace** : savoir qui
est entré chez quel client, et quand, n'est pas optionnel sur un produit qui
héberge les données de plusieurs salles. **À chiffrer sérieusement : c'est
probablement le gros du lot, comme `IUserDirectory` l'était au lot 8.**

---

## Ce qui manque au modèle

`Tenant` porte déjà `GymPlan`, `PlanPrice` et `PlanRenewalDate` — de quoi
alimenter le héros du panneau Facturation. Manquent :

- **`Invoice`**, décrite dans `05-DATA-MODEL.md` (`Date`, `Reference`, `Amount`,
  `Status`) et jamais créée. C'est la facturation du client **à TechXYZ**, à ne
  pas confondre avec `Payment`, qui est l'encaissement d'un membre par la salle.
- **Le moyen de paiement du client** (« Visa •4242, expire 08/27 »). Rien ne le
  stocke. **Ne pas stocker de numéro de carte** : ce qui s'affiche est une marque
  et quatre chiffres, c'est-à-dire ce qu'un prestataire de paiement renvoie, pas
  ce qu'on saisit. Sans prestataire, deux champs descriptifs suffisent — et le
  bouton « Modifier » reste désactivé, comme le lot 6 a traité la borne de
  pointage.
- **Le plafond de membres** (`112 / illimité` et sa barre à 42 %). `PlanPrice`
  existe, pas la formule ni son plafond.

---

## Le compte des membres, piège de requête

Le panneau Facturation compte les membres actifs du client. La liste des clients
en comptera un par ligne. Or **toutes les entités métier sont filtrées par
tenant** (`ITenantScoped` + filtre global), alors que `Tenant` ne l'est pas —
il est volontairement au-dessus du filtre.

Donc : lister les clients est trivial, mais **compter quoi que ce soit à
l'intérieur de chacun ne l'est pas**. Deux voies, à trancher :

- `UseTenant` autour de chaque compte — correct, respecte le filtre, mais c'est
  une requête par client ;
- `IgnoreQueryFilters()` avec un `GroupBy(TenantId)` — une seule requête, mais on
  désarme la protection qui empêche exactement ce genre de fuite. Si cette voie
  est choisie, elle doit l'être dans **un** endroit nommé et commenté, jamais
  dispersée.

---

## Découpage proposé — deux PR séquentielles, toutes sur `main`

Jamais de PR empilée : le lot 5 s'est perdu entièrement comme ça
(voir [[pr-always-target-main]]). Le lot 8 a montré que faire atterrir la
plomberie d'abord marche — c'est le quatrième lot d'affilée où ça marche.

### PR 1 — Le socle multi-clients

Sans écran, ou presque. Semer le compte `PlatformAdmin` et les deux clients
manquants ; copier les logos dans `wwwroot` et remplir `LogoPath` /
`LogoDarkPath` / `CircleLogo` ; créer `Invoice` ; décider et implémenter
l'impersonation persistée avec sa trace ; trancher la question du comptage.

C'est ici que le lot se gagne ou se perd. **À chiffrer avant de dessiner quoi
que ce soit.**

### PR 2 — Les deux panneaux

`GetTenantsQuery`, `GetTenantDetailQuery`, `CreateTenantCommand`,
`UpdateTenantBrandingCommand`, `UpdateTenantPlanCommand`, la coquille à sections
(la même `gx-tabs` que le lot 8), et les deux panneaux desktop + mobile.

Rappel : le mobile ne montre **que** la facturation, et le dit — ne pas inventer
un panneau marque sur téléphone que la maquette a délibérément écarté.

---

## Ce qui existe déjà et fera gagner du temps

- **Les classes CSS du lot 9 sont toutes là**, vérifiées une par une :
  `gx-theme-grid` (2), `gx-theme-card` (7), `gx-tokens` (1), `gx-token` (4),
  `gx-plan-hero` (2), `gx-listrow` (6), `gx-disc-ic` (7). Mobile :
  `gx-m-cta` (5), `gx-m-ic` (9), `gx-m-tag` (2). **Markup seul**, comme aux
  lots 4 à 8.
- **La coquille à sections est écrite** : `ReglagesDesktop` fait exactement ça
  avec `gx-tabs` / `gx-tab`, et `ReglagesSection` montre la forme. Le lot 9 est
  le même patron avec deux sections.
- **`SaveBar` existe** (`Components/Features/Reglages/`), collante, avec son état
  « Enregistré ». À réutiliser telle quelle, ou à remonter dans `Shared/` si les
  deux écrans la partagent.
- **`themes.css` est prêt pour les trois clients** — le mécanisme marque blanche
  est en place depuis le lot 0 et n'a jamais servi qu'à un seul client. Changer
  `Tenant.ThemeKey` repeint l'application, sans redéploiement : cet écran ne fait
  que l'exposer.
- **`GymPolicies.PlatformAdmin` et l'attribut sont posés** sur
  `Administration.razor` ; il ne reste que le contenu.
- **`TeamAccessScopes.Assignable` exclut déjà `PlatformAdmin`** : une salle ne
  peut pas se donner les clés de la plateforme depuis ses propres réglages. Ne
  pas défaire ça.
- **`IUserDirectory` (lot 8)** sait déjà lire les comptes d'un tenant, ce dont la
  fiche d'un client aura besoin.

---

## Pièges déjà payés sur les lots précédents

- **Un champ Fluent est deux éléments frères** (label + input) : posé nu dans une
  grille CSS, il mange deux cellules. Emballer chaque champ dans un `<div>`
  (rencontré au lot 8 sur la grille Identité).
- **Collision de nom de composant** : `NotificationsPanel` existait déjà dans
  `Layout/`. Vérifier avant de nommer un panneau (lot 8, attrapé à la
  compilation).
- Les **attributs ARIA d'état écrits en toutes lettres**, jamais liés à un bool :
  Blazor supprime l'attribut quand le bool est faux (commit `bf67bdb`).
- **`ExecuteUpdateAsync` ne marche pas** sur le provider InMemory des tests —
  charger et muter à la place (lot 7).
- **La base de dev est recréée au démarrage** et déconnecte l'utilisateur.
  Demander une connexion et s'attendre à redémarrer — voir
  [[gymxyz-lot4-verification-gaps]].
- **Le volet navigateur ne transmet aucun clic** à l'application : ni pointeur,
  ni `Entrée` sur un bouton focalisé. Vérifier en mesurant le DOM, faire cliquer
  l'utilisateur pour le reste, et **dire dans la PR ce qui n'a pas pu être
  atteint**.

---

## Premier geste suggéré

1. Lire `ScreenAdministration` **et** `mobile/screen-administration.jsx`, puis
   `01-LOTS.md` §Lot 9, et constater par soi-même l'écart décrit en tête.
2. **Poser la question A / B / C** avant tout chiffrage : le reste du lot en
   dépend entièrement.
3. Chiffrer l'impersonation persistée et sa trace — c'est le `IUserDirectory` de
   ce lot.
4. Revenir avec un plan qui fait atterrir le socle avant les écrans.

Attendre la validation avant d'écrire.
