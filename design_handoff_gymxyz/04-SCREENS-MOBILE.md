# Écrans — mobile

> Référence normative : `design/GymXYZ Mobile.html` + `design/app/mobile/*.jsx` +
> `design/app/mobile.css`. Le prototype affiche le téléphone dans un cadre iOS
> (`design/app/ios-frame.jsx`) : **le cadre est un artifice de présentation**, à ne
> pas porter. Ce qui compte est le contenu du viewport.

## Principes

Ce n'est pas le desktop rétréci. C'est **la même app pensée pour une main, en
salle** : le gérant ou le coach pointe des présences, consulte le planning, vérifie
une fiche membre entre deux cours.

- **Cible tactile ≥ 44px** partout. Aucun texte sous 12px.
- Une seule colonne, cartes pleine largeur, listes à chevron.
- **Navigation par onglets** en bas, jamais de sidebar.
- Les sous-écrans (fiches, feuilles) sont **plein écran** avec retour dans le header
  — pas de drawer.
- Le reste de la navigation vit dans une **feuille « Plus »**.

## Shell — `design/app/mobile-shell.jsx`

```
┌────────────────────────────┐
│ Header collant             │  marque + actions (racine)
│                            │  ou  ‹ retour + titre (sous-écran)
├────────────────────────────┤
│ Corps scrollable           │  padding 16 / 16 / 108
│  (grand titre iOS ici)     │  entrée gx-m-rise 340ms
├────────────────────────────┤
│ Tab bar — 5 onglets        │
└────────────────────────────┘
```

**Header** (`.gx-m-head`, collant) — deux modes :
- *racine* : marque (34px + wordmark thèmé) à gauche, cloche (point rouge) + avatar
  (→ ouvre « Plus ») à droite, boutons ronds 40px.
- *sous-écran* : bouton retour 38px (chevron 24px), titre tronqué en police
  d'affichage.

**Grand titre** (`.gx-m-greet`) : vit **dans le corps scrollable**, pas dans le
header — il défile (comportement iOS). Titre en police d'affichage, sous-titre
`--text-sm` `--text-muted`.

**Tab bar** (`.gx-m-tabbar`) : Accueil · Planning · Présences *(badge de compteur)* ·
Membres · **Plus**. Icônes 24px + libellé, onglet actif en couleur de marque.

**Feuille « Plus »** (`MPlusSheet`) : scrim + feuille à poignée, en-tête avec titre
et croix. Contenu : bloc profil (avatar, nom, « rôle · marque ») puis grille
d'accès : Coachs *(masqué si solo)*, Cours, Abonnements, Lieux, Réglages,
Administration — chacun avec un sous-titre compté (« 6 coachs », « 112 actifs »).

**Primitives** : `MKpi` (tuile, variante `.three` pour 3 par ligne),
`MCard` + `MCardHead`, `MSection` (titre + « Voir tout ›»), `MBar` (7px),
`MRing`, `MChip`, `MIcTile` (42px, 54px en `.lg`, teintes par ton), `MEmpty`,
`.gx-m-row` (ligne de liste : icône/avatar, contenu, valeur à droite, chevron),
`.gx-m-cta` (bouton pleine largeur 50px).

**Recherche** : champ inline dans le corps (`.gx-m-search`, 42px), pas dans le header.

---

## Accueil — `design/app/mobile/screen-accueil.jsx`

Grand titre « Bonjour <prénom> » + date · KPI en tuiles (2 par ligne) · **bande de
semaine** horizontale scrollable (`.gx-m-weekstrip`, jours de 60px, marqueurs férié
/ vacances en haut à droite) · cours du jour en lignes compactes (heure 46px en
couleur de marque, nom + méta tronqués, occupation 56px avec mini-jauge) · alertes
(tuile d'icône 36px, titre, description, chevron) · CTA « Diffuser le planning ».

## Planning — `design/app/mobile/screen-planning.jsx`

Sélecteur de jour (même bande de semaine, jour actif encadré en couleur de marque)
puis **agenda vertical** du jour : heure, cours, coach · studio, occupation.
Bandeau calendrier scolaire condensé.

## Présences — `design/app/mobile/screen-presences.jsx`

Liste des séances (à pointer en tête) → **feuille de présence plein écran** :
compteur présents/inscrits, liste des inscrits avec **segmenté 3 états** au doigt
(présent / retard / absent, ligne atténuée si absent), barre d'action collante en
bas (`padding-bottom: 92px` réservé dans le corps).

## Membres — `design/app/mobile/screen-membres.jsx`

Recherche + liste (avatar, nom, formule, statut, chevron) → **fiche plein écran** :
identité, formule et crédits, paiements, prochains cours, historique récent.

## Cours — `design/app/mobile/screen-cours.jsx`

Liste des modèles (tuile de discipline, nom, durée · capacité · studio, remplissage)
→ fiche (description, prochaines occurrences, coachs).

## Coachs — `design/app/mobile/screen-coachs.jsx`

*(absent si tenant solo)* Liste (avatar, nom, rôle, disponibilité) → fiche (bio,
disciplines, disponibilités 7 jours, séances de la semaine, certifications, contact).

## Abonnements — `design/app/mobile/screen-abos.jsx`

Trois onglets : **Suivi** (abonnements par membre, échéance, jauge, statut) ·
**Formules** (cartes empilées) · **Encaissements** (lignes date/membre/montant/statut).
KPI en tête (MRR, actifs, en retard).

## Lieux — `design/app/mobile/screen-salles.jsx`

Liste (tuile d'icône, nom, type, occupation) → fiche (note, équipements en chips,
planning du jour, heatmap 7 jours compacte).

## Réglages / Administration — `design/app/mobile/screen-reglages.jsx`, `screen-administration.jsx`

Les sections deviennent une **liste de rubriques** ; chaque rubrique ouvre un
sous-écran plein écran avec ses champs (une colonne) et la barre d'enregistrement
collante.

---

## Points d'attention d'implémentation

1. **Une seule page Razor par section**, deux compositions de présentation. Ne pas
   dupliquer la logique de query ni les règles métier.
2. Le badge de compteur de l'onglet Présences vient de la même query que le KPI
   « à pointer » du dashboard — une seule source.
3. Les feuilles (sheets) doivent être fermables au scrim, à la croix, et à
   `Échap` au clavier.
4. Le corps réserve 108px en bas (tab bar + marge) ; une barre d'action collante
   ajoute encore ~92px de réserve dans le corps concerné.
5. L'animation d'entrée `gx-m-rise` (translation 10px, opacité .6 → 1, 340ms) est
   désactivable via une classe `gx-no-anim` — garder ce point de sortie pour
   `prefers-reduced-motion`.
