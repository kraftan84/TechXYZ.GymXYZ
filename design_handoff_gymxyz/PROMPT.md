# À coller dans Claude Code (premier message)

> Décompresse ce dossier à la racine du repo `TechXYZ.GymXYZ` (ou à côté), puis
> ouvre Claude Code dans le repo et colle le message ci-dessous. Un lot par
> session, une PR par lot.

---

Contexte : je veux implémenter les maquettes hi-fi de GymXYZ dans cette solution.
Le cahier d'implémentation est dans `design_handoff_gymxyz/`.

Avant d'écrire une ligne de code :

1. Lis `design_handoff_gymxyz/README.md`, puis `01-LOTS.md`.
2. Lis `README.md`, `IMPLEMENTATION_INSTRUCTIONS.md` et
   `.github/copilot-instructions.md` de ce repo. **Ses conventions prévalent** sur
   toute suggestion du handoff en matière d'architecture (CQRS/MediatR, accès
   direct à `IGymDbContext`, soft delete `IsActive`, commandes en 3 fichiers,
   `ISender` depuis le WebApp, `IUserFeedbackService`).
3. Explore le `Domain` et `GymDbContext` existants et **réconcilie** avec
   `05-DATA-MODEL.md` : garde les entités et noms existants, ne renomme pas en
   masse, signale-moi les écarts au lieu de les résoudre seul.
4. Ouvre les prototypes dans un navigateur pour voir la cible, et bascule les
   marques / surfaces dans le panneau « Tweaks » :
   - `design_handoff_gymxyz/design/GymXYZ Desktop.html` et `GymXYZ Mobile.html` (l'app) ;
   - `design/GymXYZ Auth & Onboarding.html` (connexion + demande d'ouverture, doc `06`) ;
   - `design/GymXYZ Console.html` (console super-admin, doc `07`) ;
   - `design/Planning diffusé - 3 styles.html` (l'image à publier, doc `08`).

Ensuite, propose-moi un plan pour le **lot 0 (socle)** — tenant + thème + shells
desktop/mobile + Identity — avant de coder. Attends ma validation.

Règles pour toute la durée du chantier :

- Un lot = une PR. Ne commence pas le lot suivant sans mon accord.
- Fidélité visuelle : reprends `app.css` / `mobile.css` / `themes.css` du handoff
  plutôt que de réécrire du CSS. **Aucune couleur, taille ou rayon en dur** :
  uniquement `var(--*)`.
- Fluent UI pour les contrôles (inputs, selects, switches, boutons, dialogs,
  toasts) thèmés à la marque ; layout et composants de données faits maison.
- Chaque lot livre **desktop et mobile** dans la même app responsive, sans
  dupliquer la logique métier.
- fr-FR, textes en dur, vouvoiement. Reprends les libellés du prototype **au mot**.
- Tests xUnit (Shouldly + Bogus) sur les handlers et validators ajoutés, dans le
  style des projets de test existants.
- Si une décision métier manque (récurrence du planning, règles de calcul du MRR,
  météo des cours extérieurs, portail membre, impersonation du super-admin, seuil
  de suspension pour impayé, parcours de diffusion du planning), **arrête-toi et
  demande-moi** — ne l'invente pas.
- Trois lots portent sur les écrans les plus récents et ont leur propre document :
  la **demande d'ouverture** (`06`), la **console plateforme** (`07`), le
  **planning diffusé** (`08`). L'ancien lot « Administration » est **caduc** —
  la console le remplace, et une partie en a déjà été livrée : lire l'encadré du
  lot 9 dans `01-LOTS.md` avant d'y toucher.

> **Ce message est celui du démarrage, et le chantier ne démarre plus.** Les lots
> 0 à 11 sont livrés. `01-LOTS.md` porte l'ordre de construction réel, l'état de
> chaque lot et ce qui reste pour la première version ; `LOT-13-BRIEF.md` porte
> la dette ouverte. **Les deux se lisent avant de planifier quoi que ce soit**, et
> les numéros de lot de ce document ne sont pas les leurs.
