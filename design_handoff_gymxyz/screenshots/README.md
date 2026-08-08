# Captures de référence

Cibles visuelles de l'implémentation, **une par écran et par marque**. Générées
depuis le prototype de `../design/`.

```
desktop/gymxyz/        10 écrans   ·  desktop/teamtrainers/  10 écrans
desktop/leyssa/         9 écrans   (pas de section Coachs : coach solo)
mobile/gymxyz/          5 écrans   ·  mobile/teamtrainers/    5 écrans
mobile/leyssa/          5 écrans
entree/                17 captures — connexion, mot de passe, demande d'ouverture
console/               15 captures — console plateforme super-admin
planning-diffuse/       3 affiches 1080 × 1350 — une par marque
```

## Comment les lire

- **Desktop** : cadre 1283 × 750 px (sidebar 256 + contenu). Les écrans sont
  scrollables dans l'app : la capture montre le haut de page, la spécification
  complète est dans `../03-SCREENS-DESKTOP.md`.
- **Mobile** : appareil 402 × 874 px (iPhone). Le bezel iOS est un artifice de
  présentation, à ne pas reproduire. `05-feuille-plus.png` montre la feuille
  « Plus » (le reste de la navigation).
- Les trois dossiers montrent **exactement le même produit et les mêmes données** :
  tout ce qui diffère vient des tokens de thème. C'est le critère de réussite du
  lot 0 — et le piège à éviter (aucun écran ne doit connaître la marque).

## Ce qu'il faut regarder en priorité

| Marque | Points de contrôle |
|---|---|
| **gymxyz** | Wordmark Orbitron `GYM`+`XYZ` azure · sidebar claire · actions `#0089CE`. |
| **teamtrainers** | Wordmark **Anton** · sidebar sombre `#1A1A1D` avec logo blanc · actions graphite `#232327` · canevas zinc plat · **statuts toujours colorés**. |
| **leyssa** | Wordmark **Dancing Script** · marque en cercle · canevas crème blush, encre prune, ombres rosées · **pas de section Coachs** · titres à plus grande échelle (script). |

## Écarts assumés dans les captures

- **Leyssa › Réglages** affiche encore l'adresse et la ville de la salle de démo.
  La règle produit est : pour un client solo sans adresse, afficher la **zone**
  (« Thonon et alentours »). Voir `../02-THEMING.md`.
- Les données sont identiques d'une marque à l'autre (même jeu de démo) — c'est
  volontaire, pour rendre le thèmage comparable.

---

# `entree/` — connexion, mot de passe, demande d'ouverture

Spécification : `../06-ENTREE-AUTH-ONBOARDING.md`. Prototype :
`../design/GymXYZ Auth & Onboarding.html`.

| Fichier | Écran |
|---|---|
| `01-login-gymxyz.png` | Connexion, thème GymXYZ (Orbitron, panneau ink + spark azure) |
| `02-login-teamtrainers.png` | Connexion, thème Team Trainer's (**Anton**, panneau graphite→noir) |
| `03-login-leyssa.png` | Connexion, thème Leyssa (**Dancing Script**, panneau **clair** rose/sauge) |
| `04-mot-de-passe-oublie.png` | Demande de lien |
| `05-lien-envoye.png` | Confirmation d'envoi |
| `06-nouveau-mot-de-passe.png` | Réinitialisation + jauge de robustesse |
| `07-mot-de-passe-modifie.png` | Confirmation |
| `08-` → `15-demande-etape*.png` | Les 6 étapes de la demande d'ouverture (+ aperçu de marque, + consentements) |
| `16-` / `17-demande-envoyee*.png` | Demande envoyée, en attente de validation |

**Les trois captures de connexion sont le vrai point de contrôle** : c'est la
même page, le même code, trois jeux de tokens. Le panneau Leyssa est clair (fond
rose/sauge, encre prune) — ne pas coder « panneau = sombre ».

À l'inverse, **les écrans `08` à `17` sont toujours en GymXYZ** : la demande
d'ouverture est hors tenant, elle ne porte jamais la marque d'un client.

# `console/` — console plateforme (super-admin TechXYZ)

Spécification : `../07-CONSOLE-PLATEFORME.md`. Prototype :
`../design/GymXYZ Console.html`. Captures faites à **1440 × 840** (la console est
un outil desktop ; en dessous de ~1180 px les grilles se dégradent).

Les fichiers `*-suite.png` montrent le bas du même écran (les pages sont longues).

| Fichier | Écran |
|---|---|
| `01` / `02-vue-ensemble` | La file du matin, KPI, activité des espaces, état plateforme, journal |
| `03-demandes-liste` | Demandes d'ouverture + filtres |
| `04` / `05-demande-fiche` | Fiche demande : structure, contact, marque, suivi, activité, notes |
| `06-demande-modale-valider` | Modale « Valider et ouvrir l'espace » |
| `07-clients-liste` | Clients + encart « pas d'accès aux données » |
| `08` / `09-client-fiche` | Usage agrégé, histogramme 12 semaines, comptes, abonnement, habillage |
| `10` / `11-facturation` | Action requise, factures, états de paiement |
| `12-support` | Liste 340px + fil de discussion + contexte technique joint |
| `13-formules-tarifs` | Les 3 formules et leur usage réel |
| `14-sante-journal` | Services, incidents, journal d'audit |
| `15-referentiels` | Disciplines, calendrier scolaire, modèles d'e-mail |

À regarder : le **bandeau plateforme ink** en haut (34 px, filet spark), la
pastille **SUPER-ADMIN**, et les deux encarts qui rappellent la frontière de
données. Aucun bouton « Ouvrir l'espace » n'apparaît : c'est l'arbitrage retenu
(pas d'impersonation).

# `planning-diffuse/` — l'image à publier

Spécification : `../08-PLANNING-DIFFUSE.md`. Prototype :
`../design/Planning diffusé - 3 styles.html`.

Trois affiches **1080 × 1350 px (format 4:5)**, en pleine résolution — ce sont
les cibles pixel, pas des vignettes.

| Fichier | Marque | Signature |
|---|---|---|
| `01-gymxyz.png` | GymXYZ | Ink navy, halos azure, Orbitron, grille 3 colonnes |
| `02-teamtrainers.png` | Team Trainer's | Monochrome, **Anton 84 px**, blocs alternés noir/blanc/gris |
| `03-leyssa.png` | Leyssa Coaching | Rose/sauge, **Dancing Script**, marque en cercle, grille 2 colonnes |

## Note technique sur ces captures

Anton et Dancing Script sont chargées depuis Google Fonts : le moteur de capture
ne les embarque pas toujours et le rendu retombe alors en Montserrat. Ces images
ont été produites avec les polices **inlinées en data-URI**. Le même piège
existera côté serveur au moment de générer l'image : auto-héberger les polices et
attendre `document.fonts.ready` avant la capture (voir `../08`, §7).
