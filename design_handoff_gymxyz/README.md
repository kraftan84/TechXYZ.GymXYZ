# Handoff : GymXYZ — implémentation Blazor de la maquette hi-fi

> À lire en premier. Ce dossier est le **cahier d'implémentation** pour Claude Code
> travaillant dans `github.com/kraftan84/TechXYZ.GymXYZ` (.NET 10 · Blazor Server ·
> MediatR · EF Core/MySQL · Fluent UI Blazor).

## Ordre de lecture

| Fichier | Contenu |
|---|---|
| `README.md` | Ce fichier : cadrage, règles du jeu, mapping maquette → archi, tokens, assets. |
| `PROMPT.md` | Le message à coller dans Claude Code pour démarrer. |
| `01-LOTS.md` | **Le plan de travail**, dans l'ordre de construction : 12 lots livrés + les 3 lots de ce second handoff. Un lot = une PR. **Sa numérotation n'est pas celle de ce README** — voir son encadré d'ouverture. |
| `LOT-*-BRIEF.md` | Les **briefs de démarrage** écrits lot par lot pendant la construction, plus `LOT-13-BRIEF.md`, le **registre de dette technique** (à lire avant de planifier). |
| `02-THEMING.md` | Marque blanche : les 3 marques, tous les tokens, polices, règles par marque. |
| `03-SCREENS-DESKTOP.md` | Spécification écran par écran (desktop). |
| `04-SCREENS-MOBILE.md` | Spécification écran par écran (mobile responsive). |
| `05-DATA-MODEL.md` | Entités proposées, enums, invariants, seed de démo. |
| `06-ENTREE-AUTH-ONBOARDING.md` | **Connexion, mot de passe, demande d'ouverture d'espace** (desktop + mobile, 3 marques). |
| `07-CONSOLE-PLATEFORME.md` | **Console super-admin TechXYZ** : demandes, clients, facturation, support, santé, référentiels. Remplace le lot 7. |
| `08-PLANNING-DIFFUSE.md` | **L'image « Diffuser le planning »** : 3 habillages, format, génération. |
| `screenshots/` | **Captures cibles** : l'app (10 desktop + 5 mobile × 3 marques), `entree/` (17), `console/` (15), `planning-diffuse/` (3 affiches 1080×1350). |
| `design/` | **Le prototype** (HTML/CSS/JSX) : la référence visuelle normative. |

---

## Overview

GymXYZ est **un seul produit de gestion de salle de sport, re-brandé par client**
(marque blanche). Le prototype couvre 10 sections métier sur deux surfaces
(desktop et mobile) et trois habillages de marque :

- **GymXYZ** — habillage par défaut, base TechXYZ (azure + Orbitron).
- **Team Trainer's** — salle de sport, monochrome noir/blanc, Anton.
- **Leyssa Coaching** — coach indépendante, rose/sauge, Dancing Script.

Le point différenciant du produit — et ce que le code doit préserver — est que
**changer de marque ne change aucun écran** : tout passe par des tokens CSS.

Trois ensembles s'ajoutent à l'app elle-même, chacun avec son document :
**l'entrée** (connexion et demande d'ouverture d'espace, `06`), **la console
plateforme** du super-admin TechXYZ (`07`), et **l'image de planning à publier**
(`08`).

## À propos des fichiers de design

Les fichiers de `design/` sont des **maquettes de référence réalisées en HTML/CSS +
React** (prototypes non fonctionnels : données en dur, pas de backend). **Ce n'est
pas du code de production à copier.** La mission est de **recréer ces écrans dans
la solution .NET/Blazor existante**, avec ses patterns établis (MediatR, EF Core,
Fluent UI, `IUserFeedbackService`), en respectant `IMPLEMENTATION_INSTRUCTIONS.md`
et `.github/copilot-instructions.md` du repo.

Le CSS, lui, **est réutilisable presque tel quel** : `design/app/app.css`,
`design/app/mobile.css` et `design/app/themes.css` sont écrits uniquement en
`var(--*)` sur les tokens TechXYZ. C'est le chemin le plus court vers la fidélité.

**Pour faire tourner le prototype**, ouvrir dans un navigateur (connexion requise :
React et Babel viennent d'un CDN) :

| Fichier | Contenu | Doc |
|---|---|---|
| `design/GymXYZ Desktop.html` | L'app, 10 sections, desktop | `03` |
| `design/GymXYZ Mobile.html` | L'app, mobile | `04` |
| `design/GymXYZ Auth & Onboarding.html` | Connexion, mot de passe, demande d'ouverture (desktop **et** mobile, 3 marques) | `06` |
| `design/GymXYZ Console.html` | Console plateforme super-admin | `07` |
| `design/Planning diffusé - 3 styles.html` | Les 3 affiches de planning (canevas, 3 frames côte à côte) | `08` |

Le panneau « Tweaks » (coin de l'écran) permet de basculer de marque, de surface
(desktop/mobile) et d'écran en direct — c'est le meilleur moyen de comprendre
l'étendue du thèmage.

Si vous ne pouvez pas l'ouvrir, `screenshots/` contient les mêmes écrans figés
pour les trois marques (voir `screenshots/README.md`).

## Fidélité

**High-fidelity.** Couleurs, typographie, espacements, états et copies FR sont
définitifs. Reproduire fidèlement : mêmes métriques, mêmes libellés, mêmes tons de
statut. En cas de conflit entre un doc et le prototype, **le prototype gagne** —
signaler l'écart plutôt que d'improviser.

Deux libertés explicitement accordées :
1. **Fluent UI pour les contrôles** (inputs, selects, switches, boutons, dialogs,
   toasts, datepickers) — thèmés aux couleurs de la marque. Le **layout reste
   maison** (grilles, cartes, jauges, planning, KPI) : aucun composant Fluent n'y
   correspond.
2. Un composant maison du prototype peut être remplacé par son équivalent Fluent
   si le rendu final est identique à l'œil.

---

## Décisions techniques validées

| Sujet | Décision |
|---|---|
| Rendu | **Blazor Server** (interactive server), projet `TechXyz.GymXyz.WebApp`. |
| Mobile | **Même application**, responsive. Pas de MAUI, pas de second projet. |
| Découpage | **Par feature verticale** : chaque lot livre desktop **et** mobile. |
| Données | EF Core / MySQL déjà en place — `IGymDbContext`, pas de repository. |
| CRUD | Réel dès le lot 1 (pas de mock en mémoire). |
| Auth | ASP.NET Core Identity local. |
| Rôles | `GymManager`, `Coach`, `Member`, `PlatformAdmin`. |
| i18n | fr-FR uniquement, **textes en dur** (pas de .resx). |
| Tests | xUnit sur les services/handlers (Shouldly + Bogus, comme l'existant). |
| Multi-marque | Voir la recommandation ci-dessous. |

### Recommandation multi-tenant (question ouverte tranchée)

**Une base, une colonne `TenantId`, un filtre global EF Core, résolution par
sous-domaine.** Raison : vous voulez un rôle **Super-admin TechXYZ qui gère les
clients** (écran Administration du prototype) — ça n'a de sens que si les clients
coexistent dans une même instance. Un déploiement par client rendrait cet écran
inutile et multiplierait les migrations à la main.

Concrètement, au lot 0 :
- Entité `Tenant` (client) : nom, slug, `ThemeKey`, coordonnées, formule GymXYZ.
- Interface `ITenantScoped { Guid TenantId { get; set; } }` sur toutes les entités
  métier ; `EntityBase<T>` reste inchangé (soft delete `IsActive` conservé).
- `HasQueryFilter(e => e.TenantId == _tenant.Current && e.IsActive)` posé dans
  `GymDbContext` (attention : combiner avec le filtre soft-delete existant, ne pas
  l'écraser).
- `ITenantContext` scoped, résolu depuis l'hôte (`teamtrainers.gymxyz.fr`) avec
  repli sur une valeur de config en dev ; le `PlatformAdmin` peut l'outrepasser
  (impersonation) pour l'écran Administration.
- Le thème est **une donnée du tenant**, pas une config de build.

Alternative si vous voulez lever le risque : garder le `TenantId` en base et le
filtre, mais figer `ITenantContext` sur un tenant unique lu dans la config. Vous
basculez en SaaS partagé plus tard sans migration. **Ne pas** livrer sans
`TenantId` : l'ajouter après coup coûte une migration sur toutes les tables et une
relecture de toutes les requêtes.

---

## Mapping maquette → architecture

| Élément de la maquette | Où il va dans la solution |
|---|---|
| `app/themes.css` (tokens de marque) | `WebApp/wwwroot/css/themes.css`, tel quel. |
| `app/app.css`, `app/mobile.css` | `WebApp/wwwroot/css/`, tel quel (puis isolation CSS par composant si vous préférez). |
| Tokens TechXYZ (`_ds/tokens/*.css`, `styles.css`) | `WebApp/wwwroot/css/techxyz/` + `@font-face` sur les woff2 auto-hébergés. |
| `app/shell.jsx` (Sidebar, Topbar, Brand, Kpi, Bar, Ring, PageHead, Crumb, CardHead, Chip) | `WebApp/Components/Layout/` + `WebApp/Components/Shared/` (composants Razor réutilisables). |
| `app/mobile-shell.jsx` (MHead, MTabBar, MSheet, MPlusSheet, MKpi…) | `WebApp/Components/Layout/Mobile/`. |
| `app/screen-*.jsx` | une page Razor par section dans `WebApp/Components/Pages/`. |
| `app/auth.css`, `app/auth-*.jsx` | zone `WebApp/Components/Account/` (parcours d'entrée) — voir `06`. |
| `app/console.css`, `app/console-*.jsx`, `app/auth-admin.jsx` | zone `WebApp/Components/Platform/`, policy `PlatformAdmin` — voir `07`. |
| `Planning diffusé - 3 styles.html` | un composant de rendu `PlanningPoster.razor` + capture headless — voir `08`. |
| `app/data.js` → `GX_DATA` | **seed de démo** en base (`Persistence/DataInitialization`), pas des constantes C#. |
| `app/data.js` → `GX_THEMES` | table `Tenants` + `ThemeKey`. |
| `app/calendar.jsx` (fériés + vacances scolaires) | service `ISchoolCalendarService` dans Application, appel `api.gouv.fr` côté serveur + cache mémoire. |
| Panneau « Tweaks » (thème, densité, animations) | **outil de maquette, à ne pas porter.** Exception : la densité peut devenir une préférence utilisateur plus tard (hors périmètre). |

### Conventions à respecter (rappel du repo)

- `WebApp` appelle **toujours** l'Application via `ISender`. Jamais de DbContext
  dans un composant.
- Commandes en trois fichiers : `CreateXCommand.cs` / `.Handler.cs` / `.Validator.cs`.
  Handler : `ValidateAndThrowAsync` d'abord, `false` si la cible est absente/inactive.
- Queries : `AsNoTracking()`, projection server-side, filtrage `IsActive` explicite.
- Suppression = **soft delete** (`IsActive = false`).
- Erreurs/succès utilisateur via `IUserFeedbackService`. Aucune exception non gérée
  pendant le rendu.
- Providers partagés dans `MainLayout.razor`.
- Ne pas introduire de Repository/UnitOfWork.
- Incohérences de nommage connues (`TechXYZ` vs `TechXyz`, `Coachs` vs `Coaches`) :
  **ne pas renommer en masse**, s'aligner sur le fichier voisin.

### Responsive : une seule app, deux mises en page

Les deux prototypes ne sont pas deux produits : ce sont **deux points de rupture du
même produit**. Règle d'implémentation :

- **≥ 1080px** : shell desktop (sidebar 256px + topbar 64px + contenu max 1240px).
- **< 900px** : shell mobile (header collant, corps scrollable, tab bar 5 onglets).
- Entre les deux : shell desktop avec les grilles dégradées (`@media (max-width:1080px)`
  déjà écrit dans `app.css`).
- Pas de duplication de logique : la **même page Razor** rend un layout ou l'autre
  (`<div class="gx-app">` vs `<div class="gx-m-app">`), ou deux composants de
  présentation nourris par le même ViewModel de query. Choisir un pattern au lot 0
  et le tenir.
- Un utilitaire JS/CSS unique décide du mode ; ne pas sniffer l'user-agent.

---

## Design tokens

Tous les tokens vivent déjà dans `design/_ds/.../tokens/`. **Ne pas les
retranscrire à la main en C#.** Les valeurs clés, pour référence :

**Marque TechXYZ / GymXYZ**
```
azure-50  #ECF8FF   azure-500 #00ABFC (spark)   azure-600 #0089CE (actions solides)
azure-100 #CFEEFF   azure-700 #066BA1           azure-800 #0B557F
ink-900   #0C2236 (encre)      ink-800 #122E47
```
**Neutres (slate froid)** — `0 #FFFFFF · 50 #F5F8FA · 100 #EAF0F4 · 200 #D9E2E9 ·
300 #BCC9D4 · 400 #8E9FAE · 500 #647888 · 600 #4A5C6B · 700 #364654 · 800 #243441 ·
900 #14222E`

**Statuts (jamais thèmés, ils portent du sens)** — succès `#16A571` · avertissement
`#E8920C` · danger `#E0473D` · info = azure.

**Alias sémantiques à utiliser dans le code** : `--color-primary` (=azure-600),
`--text-strong` / `--text-body` / `--text-muted` / `--text-subtle`,
`--surface-page` / `--surface-card` / `--surface-sunken`, `--border-subtle` /
`--border-default`, `--ring-brand` (focus 3px azure — obligatoire, RGAA).

**Rayons** : contrôles 10px (`--radius-md`), cartes 14px (`--radius-lg`), panneaux
20px, pills 999px. **Ombres** teintées navy, jamais gris/noir. **Focus** : anneau
azure 3px toujours visible.

**Typo** : Montserrat pour tout ce qui se lit (corps, UI, tableaux, chiffres KPI) ;
la police d'affichage est **thèmée** (`--font-display`) et réservée au wordmark,
titres de page, titres de carte et noms de fiche. Détail dans `02-THEMING.md`.

**Métriques de layout** (desktop) : sidebar 256px · topbar 64px · contenu
`padding 26px 30px 60px`, `max-width 1240px` · gouttières de grille 14–18px ·
recherche 340px/40px · drawer 520px · planning `grid 56px repeat(7,1fr)`, heures 7→21.

**Métriques mobile** : header collant, corps `padding 16px 16px 108px`, tab bar 5
onglets (cible ≥ 44px), sheets à poignée, entrée `gx-m-rise` 340ms.

**Motion** : entrées fondu + translation 8–16px, 140–360ms, `--ease-out`. Aucune
boucle décorative. `prefers-reduced-motion` respecté.

## Iconographie

**Lucide**, contour uniquement, ~1.75–2px, grille 24. Tailles 16 (méta) / 20
(boutons, champs) / 24 (nav). `--text-muted` au repos, `--color-primary` actif.
Le prototype embarque son propre jeu SVG inline (`design/app/icons.jsx`) — en
Blazor, prendre un package Lucide ou copier les paths depuis ce fichier ; **ne pas
mélanger** avec les icônes Fluent dans une même vue. Pas d'emoji.

Correspondance des noms du prototype → Lucide : `home` → `house` · `calendar` →
`calendar-days` · `check` → `check` · `users` → `users` · `user` → `user` ·
`dumbbell` → `dumbbell` · `card` → `credit-card` · `pin` → `map-pin` · `shield` →
`shield-check` · `palette` → `palette` · `trend` → `trending-up` · `target` →
`target` · `sparkles` → `sparkles` · `grid` → `layout-grid` · `tree` → `trees` ·
`bell` → `bell` · `settings` → `settings` · `search` → `search` · `alert` →
`alert-triangle` · `euro` → `euro` · `wallet` → `wallet` · `file` → `file-text` ·
`send` → `send` · `refresh` → `refresh-cw` · `clock` → `clock` · `x` → `x` ·
`chevR`/`chevL` → `chevron-right`/`chevron-left` · `arrowR` → `arrow-right` ·
`building` → `building-2`.

## Assets

Dans `design/assets/themes/` :
- `teamtrainers-mark.png` — marque Team Trainer's (fond clair).
- `teamtrainers-white.png` — version blanche (sidebar sombre du thème).
- `teamtrainers-full.png` — lockup complet.
- `leyssa-mark.png` — marque Leyssa (affichée en cercle, `circle: true`).
- GymXYZ n'a **pas de marque** : le thème par défaut s'affiche en wordmark seul
  (`markType: 'none'` dans `design/app/data.js`).

Polices auto-hébergées dans `design/_ds/.../assets/fonts/` (Orbitron latin,
Montserrat latin + italic, woff2). **Anton** et **Dancing Script** sont chargées
depuis Google Fonts par `themes.css` — pour la production, les auto-héberger aussi
(RGPD, perf, et évite les surprises de rendu).

## Copie & ton

Le produit parle **français, vouvoiement, phrases courtes, concret**. Les libellés
du prototype sont validés : les reprendre **au mot** (« Diffuser le planning », « À
pointer », « Expire bientôt », « Relancer », « Aucun dossier pour l'instant… »).
Formats FR : espace comme séparateur de milliers (`1 200`), virgule décimale,
`49 €` avec espace fine avant l'euro, heures en 24h (`18:30`), dates `9 juin 2026`.

## Écarts connus / à décider

0. Les écarts propres aux nouveaux écrans sont listés en fin de `06`, `07` et
   `08` (validation du formulaire, impersonation, suspension pour impayé,
   bouton « Aide », parcours de diffusion du planning).
1. **Recherche globale** (topbar desktop, header mobile) : présente visuellement,
   sans comportement défini dans la maquette. Proposition : lot 1 (Membres),
   recherche membres + cours, palette clavier `Ctrl+K`. À confirmer.
2. **Notifications** (cloche + point rouge) : aucun écran de liste n'existe.
   ~~Proposition : hors périmètre jusqu'à un lot dédié ; garder la cloche
   inerte.~~ **Tranché le 2026-08-07 : hors V1, « pour le moment », et la cloche
   est retirée** plutôt que gardée inerte — son point rouge était allumé en dur.
   Voir `01-LOTS.md`.
3. **Portail membre** : le rôle `Member` est demandé mais aucun écran membre n'est
   maquetté. Voir lot 12 (à cadrer avant de coder).
4. **Météo / repli des cours extérieurs** (fiche Lieu « Parc de la Tête d'Or ») :
   la maquette affiche un repli automatique. ~~Décider si le lot 4 (Lieux)
   implémente vraiment un appel météo ou juste le champ « lieu de repli ».~~
   **Tranché le 2026-08-07 : le champ seul**, l'appel météo est abandonné pour
   la V1. Le repli reste renseigné et lu par le gérant, sans automatisation.
   Voir `01-LOTS.md`.
5. **Densité d'affichage** (compact / standard / confort) : outil de maquette.
   Si vous la voulez en produit, c'est une préférence utilisateur, à chiffrer.
