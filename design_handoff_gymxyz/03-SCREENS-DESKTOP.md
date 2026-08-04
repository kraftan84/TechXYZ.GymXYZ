# Écrans — desktop

> **Le prototype est la référence normative.** Ce document donne la structure, les
> métriques et les intentions ; pour chaque détail (ordre exact des colonnes, copie
> exacte, chip de statut), ouvrir le fichier de référence indiqué et lire le CSS
> associé dans `design/app/app.css`.

## Shell (toutes les pages)

```
┌──────────┬───────────────────────────────────────────────┐
│ Sidebar  │ Topbar (64px, verre)                          │
│ 256px    ├───────────────────────────────────────────────┤
│          │ Contenu — scroll, padding 26/30/60,           │
│          │ .gx-wrap max-width 1240px centré              │
└──────────┴───────────────────────────────────────────────┘
```

**Sidebar** (`.gx-sb`, `design/app/shell.jsx`) — marque en haut (38px + wordmark),
puis groupes :

| Groupe | Items |
|---|---|
| Pilotage | Accueil, Planning, Présences *(badge 3)* |
| Personnes | Membres, Coachs *(masqué si tenant solo)* |
| Offre & business | Cours, Abonnements *(badge 6)* |
| Lieux | Lieux |

Pied : **Administration** (réservé `PlatformAdmin`). Item actif : fond
`--gx-sb-active-bg`, barre 3px collée au bord gauche, icône et libellé en couleur
active, badge inversé. Item : `padding 9px 11px`, rayon 10px, icône 19px, gap 11px,
`--text-sm` semibold. Survol : `--gx-sb-hover`.

**Topbar** (`.gx-tb`) — hauteur 64px, fond verre (`--gx-tb-bg` + flou), filet bas.
Contient : champ de recherche (340px, max 38vw, 40px de haut, fond sunken, placeholder
« Rechercher un membre, un cours… »), espace flexible, cloche avec point rouge,
engrenage → Réglages, séparateur, bloc utilisateur (avatar + nom + « rôle · marque »).

**Fil d'Ariane** (`.gx-crumb`) : icône maison + segments cliquables, chevrons.
Utilisé sur les fiches (retour vers la liste).

**Tête de page** (`.gx-pagehead`) : titre en police d'affichage + sous-titre en
`--text-sm` `--text-muted`, actions alignées à droite.

**Grilles** : `.gx-grid2` (1fr 1fr, gap 18), `.gx-grid3` (3×, gap 14),
`.gx-grid4` (4×, gap 16), `.gx-col` (colonne, gap 18). Sous 1080px : grid2 → 1
colonne, grid4 → 2, formules → 2, salles → 1.

**Fiches en drawer** (`.gx-drawer`) : 520px, collé à droite, `--shadow-xl`, entrée
`gx-slidein` 280ms.

**État vide** (`.gx-empty`) : cercle d'icône, titre `--text-xl`, texte ≤ 420px,
bouton contour. Copie type : « Aucun dossier pour l'instant. Créez le premier pour
commencer. »

---

## 1 · Accueil — `design/app/screen-accueil.jsx`

**But** : en 5 secondes, savoir si la journée est sous contrôle.

- Salutation « Bonjour <prénom> » + date/ville.
- **4 KPI** (`.gx-kpi`) : label en petites capitales, valeur `--text-2xl` bold en
  chiffres tabulaires, delta coloré avec flèche. La carte « spark » porte un filet
  dégradé de 3px en haut.
- **Carte semaine** : bande des 7 jours (Lun 8 → Dim 14) avec nombre de cours, jour
  courant marqué.
- **Cours du jour** (`.gx-classrow`) : heure en couleur de marque (tabulaire), nom
  bold, méta « Studio · Coach », jauge d'occupation (`.gx-bar`) + `occ/cap`, chip
  de statut (`Complet` danger, `Privé` neutre). Ligne cliquable → Planning.
- **Alertes** (`.gx-alert`) : tuile d'icône teintée, titre, description, bouton
  d'action. Trois cas de la démo : « 4 abonnements expirent » (warning, Relancer),
  « 2 paiements en retard — 180 € à encaisser » (danger, Voir), « Présences d'hier —
  3 cours à pointer » (marque, Pointer). Chaque action **navigue** vers la section.

## 2 · Planning — `design/app/screen-planning.jsx`

**But** : voir et remplir la semaine.

- Barre d'outils : semaine précédente / suivante, « aujourd'hui », plage affichée
  (« Semaine du 8 au 14 juin »), bascule semaine/jour, action « Nouveau cours ».
- **Bandeau calendrier scolaire** (`CalendarBanner`) : zone (A/B/C, déduite du code
  postal du tenant), jours fériés et vacances tombant dans la semaine affichée.
- **Grille** (`.gx-cal`) : `grid-template-columns: 56px repeat(7, 1fr)`, heures
  7→21, en-têtes de jour et colonne d'heures **collants**, `max-height 560px`
  scrollable. Jours fériés / vacances marqués sur l'en-tête.
- **Blocs de cours** : nom, coach abrégé (« N. Lemoine »), occupation `14/20`.
  Couleur d'intensité selon le remplissage. Clic → fiche/édition de la séance.
- Vue jour (`daySlots`) : liste chronologique avec état « fait » / à venir.

## 3 · Présences — `design/app/screen-presences.jsx`

**But** : pointer vite, et repérer les décrocheurs.

- **4 KPI** : taux d'assiduité (`87 %`, delta `+4 pts ce mois`), séances à pointer
  (3), présents (52), no-shows (14).
- **Filtre** : Aujourd'hui / Hier / Semaine.
- **Séances** : icône de discipline teintée, heure, cours, coach · studio,
  inscrits/présents, état (`à pointer`, `en cours`, `pointée`), bouton « Pointer ».
- **Taux par cours** : barres horizontales triées (Power Cycle 96 % … Boxing 71 %).
- **Membres les plus absents** : avatar, nom, `5 absences / 6 séances`, dernière venue.
- **Feuille de présence** (`FeuillePresence`) : retour + titre de séance ; bandeau de
  4 stats (`.gx-pres-summary`, 4 colonnes) ; liste des inscrits (avatar, nom,
  formule) avec **contrôle segmenté** Présent / Retard / Absent (icône + libellé,
  tons succès/warning/danger) ; ligne grisée si absent ; bouton d'enregistrement.

## 4 · Membres — `design/app/screen-membres.jsx`

**But** : gérer le fichier adhérent et ouvrir une fiche en un clic.

- Tête de page + recherche + filtres + « Ajouter un membre ».
- **Tableau** (`.gx-tbl`) : avatar+nom / e-mail / formule (chip, `brand` pour
  Illimité) / crédits (jauge + `3/10` ou `∞`) / assiduité `%` / dernière venue /
  statut (`Actif` succès, `Expire bientôt` warning, `Inactif` danger). Ligne
  cliquable.
- **Fiche membre** : identité (avatar, nom, e-mail, téléphone, membre depuis),
  formule et crédits, **paiements** (date, libellé, montant, statut), **prochains
  cours** (inscrite), **historique récent** (Présente / Absente).

## 5 · Coachs — `design/app/screen-coachs.jsx`

*(section masquée quand le tenant est solo)*

- **Grille 3 colonnes** (`.gx-coachgrid`, gap 16) de cartes : avatar, nom, rôle
  (« Coach senior · co-fondatrice »), chips de disciplines, cours/semaine,
  remplissage %, note (`4,9`), membres suivis, chip de disponibilité (`Disponible`
  succès, `En congé` neutre + « jusqu'au 15 juin », `Cours pleins` warning).
- **Fiche** : bio, certifications (liste), **disponibilités** sur 7 jours (L→D),
  **séances de la semaine** (jour, heure, cours, occupation), contact, ancienneté.

## 6 · Cours — `design/app/screen-cours.jsx`

Catalogue de **modèles** (pas d'occurrences).

- Cartes : icône de discipline teintée, nom, discipline, durée, capacité, studio,
  niveau, intensité, prix (`Inclus` ou `45 € / séance`), séances/semaine,
  remplissage, habitués, avatars des coachs rattachés.
- **Fiche** : description, prochaines occurrences (jour, heure, studio, occupation),
  coachs.

## 7 · Abonnements — `design/app/screen-abos.jsx`

- **KPI** : MRR (`4 980 €`, `+6 % vs mai`), actifs (112), expirent (6), en retard
  (2 · `180 €`).
- **Formules** (`.gx-formules`, 4 colonnes) : nom, prix `49` + unité `€ / mois`,
  description, modalité (`Sans engagement`, `Engagement 12 mois`), nb de membres.
  Carte `featured` mise en avant (filet de marque).
- **Suivi** : membre, formule, échéance (`30 juin`, `3 séances restantes`,
  `Échue depuis 4 j`), jauge de progression, montant, renouvellement auto, statut.
- **Encaissements** : date, membre, libellé, montant, moyen (Prélèvement, Carte,
  Espèces), statut `Encaissé` / `Rejeté`.

## 8 · Lieux — `design/app/screen-salles.jsx`

- **Cartes 2 colonnes** (`.gx-salles`) : icône, nom, type, capacité, surface, étage,
  occupation %, séances/semaine, chip de statut (`Disponible`, `Forte demande`,
  `Accès libre`, `Beau temps`, `Sur rendez-vous`).
- **Fiche** : note descriptive, **équipements** en chips, **planning du jour**,
  **heatmap 7 jours** (une cellule par jour colorée par occupation : ≥90 % danger,
  ≥70 % marque, sinon `--azure-300`).
- Trois natures : studio interne · extérieur (adresse, météo, lieu de repli) ·
  domicile (capacité 1, adresse portée par la fiche membre).

## 9 · Réglages — `design/app/screen-reglages.jsx` (`ScreenReglages`)

Navigation de sections à gauche + panneau à droite (`.gx-set-panel`), champs en
`.gx-field-grid` (2 colonnes, gap 15/16, `.full` pour pleine largeur), **barre
d'enregistrement collante** (`SaveBar`) avec état « Enregistré ».

Sections : **Identité** (baseline, adresse, CP, ville, e-mail, téléphone, SIRET,
capacité, horaires, carte calendrier scolaire) · **Équipe & accès** (équipe avec
rôle/périmètre/dernière connexion, invitations en attente, accès des membres :
112 total / 98 avec compte / 6 invités) · **Formules & tarifs** (formules + moyens
de paiement, devise, mention TVA `art. 293 B du CGI`) · **Notifications** (2 groupes
de bascules avec canaux Email/SMS).

## 10 · Administration — `design/app/screen-reglages.jsx` (`ScreenAdministration`)

Même coquille, réservée `PlatformAdmin`. Sections : **Apparence & marque** (cartes
de thème `.gx-theme-grid` 3 colonnes avec pastilles de couleurs, sélection) ·
**Facturation** (formule `GymXYZ Pro` 79 €/mois, échéance `1 janvier 2027`, carte
Visa •4242 exp 08/27, historique de factures).
