# 08 — Planning diffusé (image à publier)

> Prototype : `design/Planning diffusé - 3 styles.html` — un document unique,
> trois affiches côte à côte (une par marque). Pas de React, pas de JS : du HTML
> et du CSS, exactement ce qui doit être rendu en image.
> Dépendances : les tokens TechXYZ (`_ds/.../styles.css`), `app/themes.css`
> (polices de marque) et `assets/themes/leyssa-mark.png`.

**Fidélité : high-fidelity.** Ces trois affiches sont la cible pixel.

---

## 1 · Ce que c'est, et pourquoi ça existe

Le bouton **« Diffuser le planning »** existe déjà dans l'app (accueil mobile, et
en raccourci desktop). Ce lot en définit le résultat : **une image prête à
publier** sur Instagram, Facebook ou à imprimer en A5 — le planning de la semaine
aux couleurs du client.

C'est la fonctionnalité la plus visible commercialement du produit : c'est la
seule qui sort de l'app et qui est vue par les adhérents. Elle doit être belle
sans intervention du gérant.

> **Ce qui n'existe pas encore et qu'il faut cadrer avec le design avant de
> coder** : l'écran de génération lui-même (choix de la semaine, du format, quoi
> masquer, aperçu, partage). Le prototype livre **le rendu**, pas le parcours.
> Voir §6.

---

## 2 · Format et squelette commun

**1080 × 1350 px — format 4:5**, le plus haut autorisé par Instagram au fil, et
lisible en vignette. C'est le format de référence ; les variantes sont en §6.

```
.post   1080 × 1350, overflow hidden, colonne flex
  .head          en-tête de marque (hauteur libre)
  .days   flex:1 colonne — les 7 jours se partagent l'espace restant
    .day         flex:1, ligne, gap 26px, align-items:center
      .dcol      colonne fixe 150px — nom du jour + date
      .slots     grille 3 colonnes (2 pour Leyssa), gap 12, align-content:center
        .slot    colonne : .h (heure) · .n (nom du cours) · .m (méta, poussée en bas)
        .rest    « Repos » — occupe toute la largeur, centré
  .foot          pied : accroche + signature
```

Points structurants :
- Les 7 jours **se partagent la hauteur** (`flex:1` chacun) : une semaine chargée
  et une semaine calme produisent la même affiche, sans trou ni débordement.
- Les créneaux manquants sont des cellules **`visibility:hidden`** (et non
  absentes) : la grille garde son rythme, les colonnes restent alignées d'un jour
  à l'autre. En génération : compléter chaque journée jusqu'à 3 (ou 2) cellules.
- `.m { margin-top:auto }` : la méta est collée en bas de la carte, donc alignée
  entre voisines de hauteurs différentes.
- `.h { font-variant-numeric: tabular-nums; white-space: nowrap }` — les heures
  s'alignent en colonne.
- `.n { text-wrap: balance }` — les noms de cours sur deux lignes se coupent
  proprement.
- **Au-delà de 3 cours par jour** : le prototype ne le traite pas. Règle à
  retenir — afficher les 3 premiers et une cellule « +N autres », ou passer la
  journée en 4 colonnes. **À trancher avec le design**, pas au fil du code.

---

## 3 · Style 1 — GymXYZ (thème par défaut)

Ink navy, spark azure, Orbitron. `background:#0C2236`, texte blanc.

**Décor** : deux halos radiaux azure en `::before` / `::after` (900px en bas à
droite, `rgba(0,171,252,.30)` → transparent à 62 % ; 760px en haut à gauche,
`rgba(0,171,252,.16)` → transparent à 65 %). `pointer-events:none`.

**En-tête** (`padding:38px 60px 14px`)
- Wordmark `GYM`*XYZ* : Orbitron 34px, `GYM` bold 700 blanc, `XYZ` medium 500
  `#00ABFC`, `letter-spacing:.02em`.
- Eyebrow « PLANNING DE LA SEMAINE » : 20px, 600, `letter-spacing:.26em`,
  uppercase, `#00ABFC`, `margin-top:20px`.
- H1 « 8 → 14 juin » : Orbitron 700, **50px**, `line-height:1`.
- Sous-titre « 12 cours cette semaine » : 20px, 500, `rgba(255,255,255,.62)`.

**Jours** (`padding:0 60px 4px`)
- Filet haut `1px rgba(255,255,255,.12)` entre les jours (pas sur le premier).
- Nom du jour : 26px, 700, `letter-spacing:.12em`, uppercase (« Lun »).
- Date : 16px, 600, `rgba(255,255,255,.42)`, `.06em` (« 08/06 »).
- Créneau : fond `rgba(255,255,255,.055)`, bord `1px rgba(255,255,255,.12)`,
  radius 12, `padding:10px 16px`.
- Heure : **22px, 700, `#00ABFC`**. Nom : 23px, 700, blanc. Méta : 16px, 500,
  `rgba(255,255,255,.5)`.
- Variante `.full` (cours complet) : fond `rgba(0,171,252,.14)`, bord
  `rgba(0,171,252,.45)`, méta `#7ED0FF` en 700 — « Complet · liste d'attente ».
- « Repos » : 20px, 600, `.2em`, uppercase, `rgba(255,255,255,.26)`.

**Pied** (`padding:20px 60px 28px`, filet haut) : « Réservations **ouvertes** »
(26px 700, « ouvertes » en `#00ABFC`) · à droite `gymxyz.fr` (19px, 500, atténué).

---

## 4 · Style 2 — Team Trainer's

Monochrome, Anton, énergie « rock ». Fond blanc, texte `#1A1A1D`.

**En-tête sur bloc noir** (`background:#1A1A1D`, `padding:34px 60px 26px`)
- Wordmark « Team Trainer's » : Anton 34px, uppercase, `.03em`.
- H1 « PLANNING<br>DE LA SEMAINE » : Anton **84px**, `line-height:.9`, uppercase,
  blanc. C'est le geste typographique de la marque : énorme, serré, cadré.
- Règle `.rule` : dates (25px, 700, `.05em`, uppercase) · filet
  `flex:1; height:4px; rgba(255,255,255,.28)` · compteur « 11 cours » (19px, 600,
  `.16em`, uppercase, atténué).

**Jours** (`padding:0 60px`)
- Séparateur **bas** `2px solid #E2E2E5` (sauf dernier).
- Nom du jour : **Anton 42px**, uppercase. Date : 16px, 700, `.12em`, `#8C8C95`.
- Créneau, trois variantes — c'est le seul style qui joue sur le contraste des
  blocs, en alternance pour créer le rythme :
  - défaut : fond blanc, **bord 2px `#1A1A1D`**, radius **4** ;
  - `.k2` : fond `#1A1A1D`, texte blanc, méta `rgba(255,255,255,.62)` ;
  - `.k3` : fond `#EAEAEC`, bord de la même couleur.
- Heure : 21px, 800, `.02em`. Nom : **Anton 26px**, uppercase, `line-height:1.05`.
  Méta : 15px, 600, `#6E6E76`.
- « Repos » : Anton 30px, `.14em`, uppercase, `#D2D2D7`.

**Pied** : bandeau noir (`padding:22px 60px 30px`) — « RÉSERVATIONS OUVERTES »
(Anton 30px) · à droite « Team Trainer's » (20px, 700, `.06em`, atténué).

---

## 5 · Style 3 — Leyssa Coaching

Rose poudré / sauge, Dancing Script. Fond `#FBF5F4`, texte `#4A2A38`.
**Grille à 2 colonnes** (`.p-ly .slots{grid-template-columns:repeat(2,1fr)}`) :
moins de cours, plus d'air — c'est cohérent avec une coach seule.

**Décor** : un halo sauge en haut à droite (820px,
`rgba(234,240,226,.9)` → transparent à 68 %).

**En-tête centré** (`padding:44px 70px 24px`, `text-align:center`)
- Marque en **cercle 96px** (`border-radius:50%`, `object-fit:cover`, bord
  `2px #F1C2D0`) — `assets/themes/leyssa-mark.png`.
- Wordmark « Leyssa Coaching » : **Dancing Script 700, 58px**, `#B54C66`.
- Eyebrow « PROGRAMME DE LA SEMAINE » : 18px, 600, `.3em`, uppercase, `#B98C99`.
- H1 « Du 8 au 14 juin · Thonon » : Montserrat 700, 40px, `#4A2A38`.
  ⚠️ **Jamais d'adresse postale pour Leyssa** — la zone seulement (« Thonon »).

**Jours** (`padding:8px 70px 0`)
- Filet haut `1px #EEDFE1`.
- Nom du jour **en entier** (« Lundi », pas « Lun ») : Dancing Script 700, 40px,
  `#B54C66`. Date : 15px, 600, `.12em`, uppercase, `#B98C99`.
- Créneau : fond `#FCF1F4`, bord `1px #F1C2D0`, **radius 18**, `padding:11px 18px`.
  Variante `.sage` : fond `#EFF3E8`, bord `#D8E2C7`, heure `#67784F`.
- Heure : 19px, 700, `#9A3F56`. Nom : 23px, 700, `#4A2A38`. Méta : 15px, 500,
  `#8B6472`.
- « Repos » : 19px, 600, `.18em`, uppercase, `#D6BEC4`.

**Pied centré** (`padding:22px 70px 34px`) : « Révélez-vous » (Dancing Script 700,
42px, `#B54C66`) puis « Thonon · Réservations ouvertes » (19px, 600, `.12em`,
`#8B6472`, séparateur `#C9AEB5`).

---

## 6 · Le parcours de génération (à concevoir)

Ce qui manque, et qu'il faut maquetter avant de développer :

1. **Point d'entrée** — bouton « Diffuser le planning » (accueil mobile, raccourci
   desktop, et logiquement aussi depuis l'écran Planning).
2. **Paramètres** — semaine (par défaut la suivante), format (4:5 / 1:1 / 9:16),
   quoi afficher : coach, studio, places restantes, cours complets, séances
   privées (à **exclure** par défaut : « Coaching perso · sur rendez-vous » est
   déjà anonyme dans la maquette, mais un nom de client ne doit jamais y figurer).
3. **Aperçu** à l'échelle, avec l'habillage du tenant.
4. **Sortie** — téléchargement PNG, `navigator.share` sur mobile, copie dans le
   presse-papier. Une publication directe vers Instagram/Facebook suppose une
   intégration Meta : **hors périmètre**, à chiffrer à part.

### Formats à prévoir

| Usage | Dimensions | Adaptation |
|---|---|---|
| Fil Instagram / Facebook (référence) | **1080 × 1350** | tel quel |
| Carré | 1080 × 1080 | en-tête et pied resserrés, mêmes grilles |
| Story / Reel | 1080 × 1920 | plus d'air vertical, une zone sûre de 250px en haut et 320px en bas |
| Impression A5 | 148 × 210 mm à 300 dpi | export vectoriel/PDF plutôt que PNG |

---

## 7 · Implémentation recommandée (.NET)

Le rendu **est** du HTML/CSS : ne le réécrivez pas en dessin.

- **Rendre côté serveur** la même page (composant Razor `PlanningPoster.razor`
  alimenté par la query de planning), puis la **capturer en PNG** avec un
  navigateur headless (`PuppeteerSharp` / `Playwright .NET`), viewport
  1080 × 1350, `deviceScaleFactor: 2` si vous voulez un 2160 × 2700 pour
  l'impression.
- **Polices** : Orbitron, Anton, Dancing Script et Montserrat doivent être
  **auto-hébergées et embarquées** avant capture. C'est le piège n°1 : une police
  chargée depuis Google Fonts au moment du rendu tombe en Montserrat une fois sur
  deux et l'affiche perd son identité. Attendre `document.fonts.ready` avant la
  capture.
- **Habillage** : `data-theme="{tenant.ThemeKey}"` sur la racine, et un bloc de
  style par marque, comme dans le prototype. Un nouveau client = un bloc CSS, pas
  un nouveau composant.
- **Cache** : la même semaine + le même tenant + les mêmes options donnent la
  même image ; mettre en cache par empreinte et invalider à toute modification
  du planning de la semaine.
- **Accessibilité / partage** : générer en parallèle un **texte alternatif** (le
  planning en liste) — c'est ce que le gérant collera en légende du post.

## 8 · Données

Aucune entité nouvelle : la source est le planning du lot 2
(`GetWeekPlanningQuery`). Ce qui est projeté, par jour :

```
Jour      : nom court/long, date (jj/mm)
Créneaux[]: Heure (HHhMM), Nom du cours, Meta (studio · places | durée · coach |
            « Complet · liste d'attente »), Etat: normal | complet | accent
Repos     : true si aucun créneau publiable ce jour-là
En-tête   : plage de dates, nombre total de cours
```

**Règle de contenu** : l'affiche est publique. Aucun nom d'adhérent, aucun
effectif inscrit, aucun prix. Places **restantes**, oui ; nombre d'inscrits, non.
