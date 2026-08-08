# Lot 11 — Marques clientes · brief de démarrage

Écrit le 2026-08-06, juste après la fusion du lot 10. `main` est à `a3a50a0`,
**486 tests au vert**, lots 0 à 10 livrés, plus la PR 24 (entrée 1 du registre).

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

---

## À lire avant tout le reste

`01-LOTS.md` dit du lot 11 : « Si les lots précédents ont été faits
correctement, ce lot ne touche **aucun écran**. » C'est vrai — et c'est plus
vrai que la liste à puces qui suit ne le laisse croire. **Quatre des cinq
travaux annoncés sont déjà faits.** Vérifié un par un :

| Ce que `01-LOTS.md` demande | État réel |
|---|---|
| Blocs `[data-theme="teamtrainers"]` et `[data-theme="leyssa"]` | **Faits.** `themes.css` lignes 113 et 177. |
| Deux tenants en base (`ThemeKey`, marque, coordonnées, `IsSolo`) | **Faits.** `DbInitializer` les sème avec wordmark, `LogoPath`, `CircleLogo`, `AreaLabel`, `IsSolo`. |
| Assets de marque dans `wwwroot/assets/themes/` | **Faits, ailleurs.** Les trois PNG sont dans `wwwroot/images/themes/`, et c'est ce chemin que la base contient. `wwwroot/assets/` **n'existe pas**. Le doc décrit une arborescence qui n'a jamais été celle du projet : ne pas déplacer les fichiers pour faire plaisir au doc. |
| Polices Anton et Dancing Script auto-hébergées | **Pas fait.** La seule vraie dette de ce lot — voir plus bas. |
| Test par thème + revue 10 sections × 3 marques | **Pas fait**, et le point 2 des blocages explique pourquoi ce n'est pas qu'un oubli. |

Autrement dit : **le lot 11 n'est pas un lot de construction, c'est un lot de
vérification** — plus une dette de conformité. Le plan devrait le dire au lieu
de dérouler la liste comme si tout restait à faire.

### Ce qui a été regardé en vrai, pas déduit

Les deux marques clientes ont été ouvertes dans le navigateur avec leurs comptes
de démo (`aurelie.siquier@teamtrainers.fr`, `najate.amzil@leyssa-coaching.fr`,
mot de passe `GymXyz!2026!`) :

- **Team Trainer's** : sidebar sombre, mark blanche (`teamtrainers-white.png`
  via `BrandLockup.OnDark`), Anton sur les titres, rampe graphite. Conforme.
- **Leyssa** : sidebar blush, mark en cercle, Dancing Script lisible à
  `title-scale 1.28`, roses partout. **Le mode solo marche** : pas de « Coachs »
  dans la navigation (`GxNavigation.Visible`), et `/coachs` en URL directe
  redirige (`Coachs.razor:66`, `CoachDetails.razor:78`).
- **La règle « aucune adresse postale »** est tenue : Réglages › Identité
  affiche « Zone d'intervention — vous intervenez sans local fixe : la zone
  remplace l'adresse postale ».

Rien de tout cela n'est à refaire. **Le lot doit le constater et passer à ce qui
manque.**

---

## La seule vraie dette : les polices viennent de Google

`themes.css` ligne 31 :

```css
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Dancing+Script:wght@600;700&display=swap');
```

**Mesuré dans le navigateur, pas supposé.** En ouvrant l'app sur Leyssa, les
requêtes sortantes sont exactement :

```
https://fonts.googleapis.com/css2?family=Anton&family=Dancing+Script:wght@600;700
https://fonts.gstatic.com/s/anton/v27/1Ptgg87LROyAm3Kz-C8CSKlv.woff2
https://fonts.gstatic.com/s/dancingscript/v29/If2RXTr6YS-zF4S-kcSWSVi_szLgiuEHiC4W.woff2
```

Ce sont **les trois seules requêtes externes de toute l'application**. L'adresse
IP de chaque visiteur part chez Google, sans consentement — c'est le problème
RGPD que `01-LOTS.md` signale, et l'arrêt du tribunal de Munich l'a rendu
concret pour tout le monde. Les héberger referme le dernier trou : l'app devient
entièrement autonome.

**Le correctif est mécanique et a déjà son précédent dans le dépôt.** Montserrat
et Orbitron sont auto-hébergées depuis le lot 0, dans
`wwwroot/css/techxyz/tokens/fonts.css` : `@font-face`, `src: url(...woff2)`,
`font-display: swap`, `unicode-range` latin. Anton et Dancing Script demandent
la même chose, plus les `.woff2` à déposer à côté des trois existants.

**Deux points à ne pas rater** : Dancing Script est chargée en **600 et 700**
(le thème Leyssa utilise 700), donc soit une variable, soit deux fichiers ; et
la licence des deux familles est SIL OFL, donc l'auto-hébergement est autorisé —
à mentionner dans la PR plutôt qu'à supposer.

---

## Ce qui bloque, et qui ne se règle pas en le contournant

### 1. `LOT-13-BRIEF.md`, entrée 2 — le `PlatformAdmin` sans client

**L'échéance est « avant le lot 11 », et c'est ce lot-ci.** L'entrée demande une
**décision produit avant de coder**, avec trois options (A / B / C) et un
penchant déjà argumenté pour **A + B**.

Elle porte sur ce lot plus que sur n'importe quel autre : le lot 11 *multiplie
les clients*, et l'entrée décrit un super-admin qui lit les données d'un client
sans être entré chez lui et sans qu'aucune ligne `TenantImpersonation` ne
l'enregistre. Trois marques au lieu d'une, c'est trois fois la surface.

À trancher au moment du plan : soit l'entrée 2 est traitée **dans** ce lot (elle
est petite en code — une condition dans `TenantResolver`, un filtre de
navigation — et moyenne en vérification), soit elle passe après et le lot 11 le
dit explicitement.

### 2. La DoD est infaisable avec les données actuelles

`01-LOTS.md` conclut : « **DoD** : capture des 10 sections × 3 marques, desktop
et mobile. » Soit 60 captures. Le problème n'est pas le nombre, c'est ce qu'il y
aurait dessus. Compté écran par écran :

| | GymXYZ | Team Trainer's | Leyssa |
|---|---|---|---|
| Membres | 36 | 8 | 3 (tous « Inactif ») |
| Coachs | 6 | **0** | *(masqué, solo)* |
| Cours, Lieux, Séances | garnis | **0** | **0** |
| Abonnements actifs | 35 | **0** | **0** |
| Présences (semaine) | 2 à pointer | **0** | **0** |

Une revue de thème sur des écrans vides ne prouve rien : on vérifierait des
états vides, pas la marque appliquée à un tableau, une jauge, une puce de
statut, un tiroir. Or c'est précisément là que se cachent les styles en dur.
L'Accueil de Team Trainer's affiche « 0 cours » sept fois et « Rien à
surveiller » — c'est joli et ça ne démontre rien.

**Donc la première décision du lot est celle-là** : garnir le seed des deux
clients (quelques coachs, un catalogue, une semaine de séances, des abonnements
et des présences), ou réduire la DoD à ce qui est démontrable. Recommandation à
défendre : **garnir**, parce que c'est aussi ce qui rendra les trois marques
présentables en démo, et parce qu'une DoD qu'on coche sur du vide n'est pas une
DoD. Le coût est dans `DbInitializer`, pas dans les écrans.

---

## Le test « aucun style en dur », et ce qu'il trouve déjà

`01-LOTS.md` demande « un test qui, pour chaque thème, instancie les pages
principales et vérifie qu'aucun style en dur n'est apparu ». Instancier les
pages ne dira rien : un thème est du CSS, et le rendu Blazor ne l'évalue pas.
**Ce qu'on peut tester réellement, c'est le balisage** — et une simple analyse
du dépôt le fait mieux qu'un test de rendu.

Lancée pendant l'écriture de ce brief, elle donne :

- **Couleurs en dur dans les `.razor` : zéro.** Pas un `#rrggbb`, pas un
  `rgba(`. La discipline a tenu sur dix lots.
- **Une taille de police en dur** :
  `Components/Features/Abonnements/AbonnementsMobile.razor:78` porte
  `style="font-size:9px"`. Unique occurrence, et exactement le genre de chose
  que ce test existe pour attraper.

Un test qui échoue sur cette seule ligne le jour où il est écrit est un bon
test. Le corriger au passage (`--text-2xs` vaut `0.6875rem`, soit 11px — donc
ce n'est pas un remplacement à l'identique : c'est un choix à faire voir).

---

## À regarder pendant la revue, parce que ce n'est pas évident

- **Les marques de jour de la bande d'accueil et du planning.** `.ferie` tire
  ses couleurs de la rampe accent (`--azure-100` / `--azure-800`), qui est
  *re-mappée par marque* : un jour férié est donc bleu chez GymXYZ, **graphite
  chez Team Trainer's et rose chez Leyssa**. `.vac` tire du warning, qui reste
  ambré partout. Est-ce voulu ? `02-THEMING.md` dit « les statuts restent
  colorés pour que l'UI ne perde jamais son sens » — un férié est-il un statut,
  ou une décoration de marque ? À trancher en regardant, pas en raisonnant.
- **Le contraste du rose Leyssa sur les boutons pleins**, que `01-LOTS.md`
  demande explicitement : `--azure-600 #B54C66` annoncé « blanc AA ». À mesurer,
  c'est une valeur, pas une impression.
- **La sidebar sombre de Team Trainer's** : compteurs inversés, barre active
  blanche. Les deux badges du lot 10 (Présences, Abonnements) sont neufs et
  n'ont **jamais été vus sur fond sombre** — ils utilisent `GxChip` en tons
  danger et warning.
- **Le mobile de chaque marque**, qui a sa propre feuille (`mobile.css`) et ne
  bénéficie d'aucune des vérifications faites en desktop.

---

## Pièges déjà payés

- **La base de dev est recréée au démarrage** (`ResetDatabaseOnStartup`) et
  déconnecte l'utilisateur. Prévoir de se reconnecter — et pour ce lot, de le
  faire **trois fois**, une par marque.
- **Le volet navigateur ne transmet aucun clic.** Poster les formulaires en JS
  avec leur vrai jeton antiforgery, déclencher les gestionnaires par
  `element.click()`. **Dire dans la PR ce qui n'a pas pu être atteint.**
- **En dev, l'hôte ne distingue pas les clients** : `localhost` retombe sur
  `TenantOptions.DefaultSlug` (`gymxyz`). On change de marque en **se connectant
  avec le compte du client**, pas par l'URL. C'est aussi ce qui rend l'entrée 2
  du registre visible.
- **`GxIcon` prend `Class`, jamais `Style`.**
- **Ne pas écrire de CSS de marque dans les composants.** Une correction trouvée
  pendant la revue se fait dans `themes.css` ou en token, jamais en `style=`
  local — sinon le lot 11 crée la dette qu'il est censé chercher.

---

## Premier geste suggéré

1. Ouvrir les trois marques soi-même, avec les trois comptes, et constater que
   le thème est déjà bon — pour ne pas planifier du travail déjà fait.
2. **Trancher la DoD** : garnir le seed des deux clients, ou réduire la revue.
   Tout le reste du périmètre en dépend.
3. **Trancher l'entrée 2 du registre** : dans ce lot, ou après en le disant.
   Elle demande une décision produit (A / B / C), pas seulement du code.
4. Décider ce que devient la marque de jour férié sous une marque cliente.
5. Revenir avec un plan où l'auto-hébergement des polices et le test de balisage
   sont le travail livrable, et la revue 3 marques la preuve.

Attendre la validation avant d'écrire.
