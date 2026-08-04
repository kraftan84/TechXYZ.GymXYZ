# Captures de référence

Cibles visuelles de l'implémentation, **une par écran et par marque**. Générées
depuis le prototype de `../design/`.

```
desktop/gymxyz/        10 écrans   ·  desktop/teamtrainers/  10 écrans
desktop/leyssa/         9 écrans   (pas de section Coachs : coach solo)
mobile/gymxyz/          5 écrans   ·  mobile/teamtrainers/    5 écrans
mobile/leyssa/          5 écrans
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
