# Marque blanche — spécification de thèmage

Un produit, N marques. Un thème = **un jeu de custom properties CSS** scopé sur
`[data-theme="…"]`. Les écrans ne connaissent jamais une couleur, un logo ou un nom
de marque : ils lisent des tokens. Ajouter un client = ajouter un bloc de tokens +
une ligne en base. **Zéro modification d'écran.**

Fichier de référence, à reprendre tel quel : `design/app/themes.css`.

## Les deux couches d'un thème

Un thème n'est pas « le même dashboard avec un bouton d'une autre couleur ». Il agit
sur deux couches :

1. **Accent** — on re-pointe la rampe `--azure-50…900`. Le design system TechXYZ en
   dérive tous ses alias (`--color-primary`, `--border-brand`, `--ring-brand`…) et
   tous ses composants. Cela re-skinne les **actions**.
2. **Atmosphère** — on re-pointe les neutres bas (`--neutral-50…300`), l'encre
   (`--ink-900`, `--neutral-800`) et les tokens de sidebar (`--gx-sb-*`). Cela
   re-température **tout le canevas** : page, cartes, filets, titres, et le registre
   de la sidebar (slate clair / graphite sombre / blush doux).

**Jamais thèmé** : la palette de statut (succès/avertissement/danger gardent leur
sens partout), les espacements, les rayons, l'échelle typographique, Montserrat pour
le corps.

## Identité typographique

Chaque marque associe **une** police d'affichage (`--font-display`) à Montserrat
(`--font-sans`). La police d'affichage est réservée à : wordmark, titres de page,
titres de section/carte, noms de fiche. **Tout le reste** — corps, tableaux, labels,
boutons, chiffres de KPI — reste en Montserrat, toujours.

Comme les trois polices ont des encombrements très différents, chaque thème corrige
avec `--display-title-scale` et `--display-head-scale` : sans ça, Orbitron déborde
et Dancing Script devient illisible.

| Token | Rôle |
|---|---|
| `--font-display` / `--display-weight` | Police et graisse d'affichage. |
| `--display-ls` / `--display-tr` | Interlettrage, transformation de casse. |
| `--display-title-scale` / `--display-head-scale` | Facteurs d'échelle titres / têtes de carte. |
| `--font-accent` + `--accent-*` | Wordmark (police, graisse, interlettrage, casse). |
| `--wordmark-size` / `--wordmark-lh` | Taille et interligne du wordmark. |

---

## 1 · GymXYZ (`techxyz`) — habillage par défaut

Base TechXYZ telle quelle : azure `#00ABFC` (spark), actions `#0089CE`, encre navy
`#0C2236`, neutres slate froids.

- Affichage : **Orbitron** 700, `ls .01em`, `title-scale .9`, `head-scale .8`
  (Orbitron court large → on réduit).
- Wordmark : `GYM` + `XYZ` en accent, 20px, majuscules, `ls .02em`.
- Marque : **aucune** — GymXYZ s'affiche en **wordmark seul** (`markType: 'none'`).
- Sidebar : registre clair (surface carte, filet subtil, actif en `--azure-50`).
- Gérant de démo : Dwayne Johnson, « Gérant ».

## 2 · Team Trainer's (`teamtrainers`) — monochrome, énergie rock

Rampe accent → **graphite**. Les statuts restent colorés pour que l'UI ne perde
jamais son sens.

```
azure-50  #F4F4F5   azure-500 #353539   azure-800 #131316
azure-100 #E7E7EA   azure-600 #232327  ← actions solides (blanc AA)
azure-200 #D2D2D7   azure-700 #1B1B1E   azure-900 #0A0A0C
azure-300 #A8A8B0   azure-400 #6E6E76
```
Atmosphère : page `#F3F3F4` (zinc plat, aucun voile bleu), sunken `#EAEAEC`, filets
`#E2E2E5`, bordures `#D2D2D7`, encre `#1A1A1D`, corps `#2A2A2E`.

**Sidebar « vestiaire » sombre** : fond `#1A1A1D`, filet `#2C2C30`, texte `#C7C7CD`,
fort `#FFFFFF`, actif `rgba(255,255,255,.10)` avec barre et icône blanches,
compteurs inversés. → le logo de la sidebar doit basculer sur
`teamtrainers-white.png` (variante `onDark`).

- Affichage : **Anton** 400, `title-scale 1.02`, `head-scale 1` ; wordmark
  `TEAM TRAINER'S` en 23px.
- Focus/ombres regraphités (`--ring-brand`, `--shadow-brand`, `--glow-spark`).
- Gérante de démo : Aurélie Siquier, « Gérante ».

## 3 · Leyssa Coaching (`leyssa`) — rose doux, script élégant, coach solo

Rampe accent → **rose de marque**.

```
azure-50  #FCF1F4   azure-500 #CB5B74 ← rose de marque
azure-100 #F8DEE6   azure-600 #B54C66 ← actions solides (blanc AA)
azure-200 #F1C2D0   azure-700 #9A3F56
azure-300 #E59DB2   azure-800 #7E3446   azure-900 #652B39
azure-400 #D87693
```
Atmosphère chaude : page `#FBF5F4` (crème blush), sunken `#F4E9E9`, filets
`#EEDFE1`, bordures `#E2CDD0`, cartes `#FFFDFC` (blanc chaud, pas clinique), encre
`#4A2A38` (prune profonde), corps `#5A3F49`. Ombres **teintées rose**, pas navy.
Second ton décoratif : sauge `#EAF0E2` (`--brand-soft-2`), `--leyssa-sage #7E8E64`.

**Sidebar blush** : fond `#FCF1F2`, filet `#F0DEE1`, texte `#6E4654`, actif
`#F6DCE3` / `#9A3F56`.

- Affichage : **Dancing Script** 700, `title-scale 1.28`, `head-scale 1.42`
  (un script doit être plus gros pour rester lisible) ; wordmark
  `Leyssa Coaching` 28px, casse normale, `lh .9`.
- Marque : `leyssa-mark.png` affichée **en cercle** (`circle: true`).
- **Mode solo** (`IsSolo`) : pas de section « Coachs » dans la navigation ; toute
  route directe redirige vers l'accueil.
- **Aucune adresse postale** : la coach travaille en itinérance autour de Thonon.
  Partout où l'app afficherait une adresse, afficher la **zone** (« Thonon et
  alentours »). Vérifier notamment Réglages › Identité et les fiches de lieu.
- Coach de démo : Najate « Naj » Amzil, « Coach ». Tagline de marque :
  « Révélez-vous ». Instagram : `@leyssa_coaching`.

---

## Tokens de sidebar (contrat complet)

La sidebar est la surface la plus « marquée » : chaque thème lui donne son registre
sans qu'aucun écran ne bouge. Contrat à implémenter intégralement :

```
--gx-sb-bg            fond
--gx-sb-border        filet (droite + séparateurs)
--gx-sb-fg            texte des items
--gx-sb-strong        wordmark + marque
--gx-sb-group         titres de groupe
--gx-sb-icon          icônes au repos
--gx-sb-hover         fond au survol
--gx-sb-active-bg / -fg / -border / -bar / -icon    item actif (barre 3px à gauche)
--gx-sb-count-bg / -fg                              badge de compteur
--gx-sb-count-on-bg / -on-fg                        badge sur item actif
--gx-sb-accent        couleur d'accent du wordmark
--gx-tb-bg            fond verre de la topbar (dérivé de la surface carte)
```

## Règles non négociables

1. **Aucune couleur en dur** dans un composant Razor ni dans une classe CSS de
   fonctionnalité. Uniquement `var(--*)`.
2. **Les couleurs de statut ne se thèment pas.** Un paiement en retard est rouge
   chez les trois clients.
3. **Contraste** : les actions pleines utilisent la marche `-600` de la rampe pour
   que le texte blanc passe AA. Vérifier à chaque nouvelle marque.
4. **Le focus reste visible** dans tous les thèmes (`--ring-brand` re-teinté).
5. Le thème est **rendu côté serveur** dans l'attribut `data-theme` du `<html>` :
   pas de flash de mauvaise marque au chargement.
6. La transition douce entre thèmes du prototype (`.gx-app *` en transition) est un
   **artifice de démo** : en production, ne pas mettre une transition sur `*`
   (coût de rendu). Si vous gardez la bascule live pour les démos, cibler
   explicitement les propriétés utiles sur les conteneurs.

## Ajouter un client (procédure)

1. Un bloc `[data-theme="<slug>"]` : rampe accent (9 marches, `-600` accessible),
   neutres/encre si l'atmosphère change, tokens `--gx-sb-*`, police d'affichage +
   ses facteurs d'échelle, wordmark.
2. Assets de marque dans `wwwroot/assets/themes/` (variante fond sombre si la
   sidebar est sombre).
3. Une ligne `Tenants` : slug, `ThemeKey`, nom affiché, `IsSolo`, coordonnées.
4. Contrôler les 10 sections en desktop **et** mobile. Aucun `.razor` ne doit être
   modifié — si c'est nécessaire, c'est un token manquant : l'ajouter au contrat
   plutôt que de coder une exception.
