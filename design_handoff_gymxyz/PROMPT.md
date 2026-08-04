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
4. Ouvre `design_handoff_gymxyz/design/GymXYZ Desktop.html` et
   `GymXYZ Mobile.html` dans un navigateur pour voir la cible, et bascule les
   marques dans le panneau « Tweaks ».

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
  météo des cours extérieurs, portail membre), **arrête-toi et demande-moi** — ne
  l'invente pas.
