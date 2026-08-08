# 06 — Entrée : connexion, mot de passe, demande d'ouverture

> Prototype : `design/GymXYZ Auth & Onboarding.html`
> Sources : `design/app/auth-login.jsx` (desktop), `auth-onboarding.jsx`,
> `auth-mobile.jsx` (mobile), `auth-main.jsx` (routage), `auth-data.js` (données),
> `auth.css` (styles). Le traitement super-admin de la demande est spécifié dans
> `07-CONSOLE-PLATEFORME.md`.

**Fidélité : high-fidelity.** Couleurs, métriques, libellés FR définitifs.

## Ce que couvre ce lot

Le parcours complet avant l'app, sur **desktop et mobile**, pour les **trois
marques** :

| Route | Écran | Marque affichée |
|---|---|---|
| `login` | Connexion | **celle du client** (techxyz / teamtrainers / leyssa) |
| `forgot` | Mot de passe oublié | celle du client |
| `sent` | Lien envoyé | celle du client |
| `reset` | Nouveau mot de passe | celle du client |
| `reset-done` | Mot de passe modifié | celle du client |
| `ob` | Demande d'ouverture d'espace (6 étapes) | **GymXYZ uniquement** |
| `ob-sent` | Demande envoyée / en attente de validation | **GymXYZ uniquement** |

> **Règle structurante.** La connexion est *thémée client* : un membre de Team
> Trainer's ne doit jamais voir du bleu GymXYZ. La demande d'ouverture et tout ce
> qui est côté plateforme restent **chez GymXYZ**, en azure/ink.
> Dans le prototype : `AUTH_GYM_ONLY = ['ob','ob-sent','admin']` force
> `data-theme="techxyz"` sur ces routes (`auth-main.jsx`).

En Blazor : `data-theme` est posé **côté serveur** au rendu initial, depuis le
tenant résolu par sous-domaine (`teamtrainers.gymxyz.fr` → `teamtrainers`). Pas
de flash de thème. Sur `gymxyz.fr` (pas de tenant) → thème `techxyz`.

---

## 1 · Connexion (desktop)

### Layout

`.gx-auth` : `display:grid; grid-template-columns:1.02fr .98fr; height:100vh;
overflow:hidden`. Deux colonnes plein écran, jamais de scroll de page — seule la
colonne droite scrolle (`overflow-y:auto`).

**Colonne gauche — panneau de marque** (`.gx-auth-panel`)
- `padding:44px 52px 40px`, `display:flex; flex-direction:column`.
- Fond : `var(--gx-a-panel)`, un dégradé **par marque** (tokens ci-dessous).
- Deux décors, en `::before` / `::after`, `pointer-events:none` :
  - trame de points : `radial-gradient(var(--gx-a-dots) 1px, transparent 1px)`,
    `background-size:26px 26px`, `opacity:.55` — c'est le motif node-graph
    TechXYZ, discret ;
  - halo : cercle `46vmax`, `right:-16vmax; top:-14vmax`,
    `radial-gradient(circle, var(--gx-a-dots) 0%, transparent 66%)`, `opacity:.85`.
- Contenu : `Brand` (logo + wordmark de la marque, variante fond sombre) en haut,
  bloc central `margin:auto 0` (`max-width:520px`), pied de page en bas.
- Titre `h1` : police d'affichage thémée (`--font-display`), `font-size:
  calc(clamp(30px,3.2vw,44px) * var(--display-title-scale,1))`, `line-height:1.08`,
  `margin:0 0 16px`, `text-wrap:pretty`.
- Sous-titre : `var(--text-lg)`, `line-height:1.5`, couleur `--gx-a-dim`,
  `max-width:26em`.
- 3 pastilles (`.gx-a-pill`) : icône 16 + libellé, `padding:8px 14px`,
  `border-radius:var(--radius-pill)`, `border:1px solid var(--gx-a-hair)`,
  `font-size:var(--text-sm)`, gap 10px, `margin-top:26px`.
  Libellés : **Planning** (`calendar-days`), **Présences** (`check`),
  **Abonnements** (`credit-card`).
- Pied (`.gx-a-pfoot`, `var(--text-xs)`, couleur `--gx-a-faint`, séparateurs
  1×12px) : « Hébergé en France » · « Conforme RGPD » · et, **si le thème n'est
  pas GymXYZ**, « Propulsé par GymXYZ ».

**Colonne droite — formulaire** (`.gx-auth-side`)
- `background:var(--surface-page)`, `display:grid; place-items:center`,
  `padding:40px 32px`, `overflow-y:auto`.
- `.gx-a-form` : `max-width:392px`, `display:flex; flex-direction:column; gap:17px`.

### Copie par marque (`AU_COPY`, `auth-login.jsx`)

| Thème | H1 | Sous-titre |
|---|---|---|
| `techxyz` | Votre espace de gestion | Planning, membres, présences et abonnements au même endroit. |
| `teamtrainers` | Le club, côté coulisses | Planning, présences et abonnements : tout le club dans une seule interface. |
| `leyssa` | Votre studio, en douceur | Séances, suivi des clientes et abonnements réunis au même endroit. |

### Tokens du panneau, par marque (`auth.css`)

| Token | techxyz (défaut) | teamtrainers | leyssa |
|---|---|---|---|
| `--gx-a-panel` | `linear-gradient(160deg,#0C2236 0%,#12304A 62%,#0C2236 100%)` | `linear-gradient(160deg,#1F1F23 0%,#101013 58%,#000 100%)` | `linear-gradient(165deg,#FDF3F5 0%,#F6DEE5 52%,#E9EFE0 100%)` |
| `--gx-a-fg` | `#fff` | `#fff` | `#4A2A38` |
| `--gx-a-dim` | `rgba(255,255,255,.68)` | `rgba(255,255,255,.66)` | `rgba(74,42,56,.72)` |
| `--gx-a-faint` | `rgba(255,255,255,.42)` | `rgba(255,255,255,.38)` | `rgba(74,42,56,.5)` |
| `--gx-a-accent` | `var(--azure-400)` | `#fff` | `var(--azure-600)` |
| `--gx-a-dots` | `rgba(0,171,252,.26)` | `rgba(255,255,255,.16)` | `rgba(203,91,116,.22)` |
| `--gx-a-hair` | `rgba(255,255,255,.14)` | `rgba(255,255,255,.13)` | `rgba(122,68,86,.14)` |

Le panneau **Leyssa est clair** : le composant `Brand` passe alors en variante
fond clair (`onDark={theme.id !== 'leyssa'}`). Ne pas coder « panneau = sombre ».

### Contenu du formulaire

1. `h1` « Connexion » (`.gx-a-h` : `--font-display`, `var(--text-3xl)`,
   `line-height:1.15`, `--text-strong`) + sous-titre « Accédez à votre tableau de
   bord. » (`var(--text-md)`, `--text-muted`).
2. Champ **E-mail** (`type=email`, placeholder `prenom@votre-salle.fr`).
3. Champ **Mot de passe** avec œil afficher/masquer : `.gx-a-pw` positionné,
   bouton `.gx-a-eye` `position:absolute; right:8px; bottom:0; height:var(--control-md)
   (44px); width:36px`, icône `eye` 18px, couleur `--text-subtle` → `--text-body`
   au survol. `aria-label` « Afficher » / « Masquer ».
4. Ligne : case **Rester connecté** (cochée par défaut) ↔ lien
   **« Mot de passe oublié ? »** (`.gx-link` : `--color-primary`, `var(--text-sm)`,
   semibold, souligné au survol, `--azure-700`).
5. Bouton **Se connecter**, `variant="primary"`, `width:100%`.
6. Séparateur « ou » (`.gx-a-hr` : filets `1px --border-subtle` de part et d'autre).
7. Pied conditionnel :
   - thème GymXYZ → « Vous découvrez GymXYZ ? » + lien **« Demander l'ouverture
     d'un espace »** → route `ob` ;
   - thème client → « Vous êtes membre de *{marque}* ? Connectez-vous depuis
     l'application mobile, avec le lien reçu par e-mail. » (**pas** de lien
     d'inscription : un membre n'ouvre pas d'espace).
8. Ligne d'aide : « Besoin d'aide ? support@gymxyz.fr · 04 50 00 00 00 »
   (`var(--text-xs)`, `--text-subtle`).

### Comportement

- Succès → l'app (`GymXYZ Desktop.html` dans le prototype). En production :
  Identity + redirection vers `/` du tenant.
- Erreur d'identifiants : **pas maquettée**. À implémenter en message d'erreur
  sous les deux champs, ton du système : « E-mail ou mot de passe incorrect. »
  Ne jamais indiquer lequel des deux est faux.
- Verrouillage après N tentatives : à décider (recommandé : ASP.NET Identity
  lockout, 5 tentatives / 15 min) — **demander avant de coder**.

---

## 2 · Mot de passe oublié → lien envoyé → réinitialisation

Même `AuthShell` (panneau + colonne) pour les 4 écrans.

**`forgot`** — lien retour « ← Revenir à la connexion » (`.gx-link.muted`), titre
« Mot de passe oublié », texte « Indiquez l'e-mail de votre compte : nous vous
envoyons un lien pour en choisir un nouveau. », champ e-mail, bouton **Envoyer le
lien** (pleine largeur), note `var(--text-xs)` : « Le lien est valable 30 minutes
et ne peut servir qu'une fois. »

> Réponse **toujours identique**, que le compte existe ou non (pas d'énumération
> de comptes). Le prototype passe à `sent` sans vérifier.

**`sent`** — variante centrée (`.gx-a-center`) : pastille `.gx-a-big` (74px,
cercle, `background:var(--azure-50)`, icône `mail` 32px `--color-primary`), titre
« Vérifiez votre boîte », texte avec l'e-mail en gras, encart `.gx-a-note`
(fond `--azure-50`, bord `--azure-100`, radius `--radius-md`, padding 14/16,
icône `clock`) : « Rien reçu au bout de deux minutes ? Regardez dans les
indésirables, puis renvoyez le lien. » Boutons : **Ouvrir le lien (démo)** —
*artefact de maquette, à supprimer* — **Renvoyer le lien** (outline) et lien
retour.
En production, prévoir un **anti-spam sur le renvoi** (1 envoi / 60 s).

**`reset`** — titre « Nouveau mot de passe », deux champs mot de passe (avec œil),
**jauge de robustesse** `.gx-a-strength` : 3 segments `height:4px; flex:1;
border-radius:2px`, fond `--surface-sunken`, `mid` = `--warning-500`, `on` =
`--color-success`. Règle du prototype (`auStrength`) : +1 si ≥ 8 caractères,
+1 si majuscules **et** minuscules, +1 si chiffre ou caractère spécial.
Note sous le champ : « 12 caractères minimum, avec majuscules, minuscules et un
chiffre. »
⚠️ **Incohérence connue** : la note dit 12, la jauge compte à partir de 8.
Trancher côté produit (recommandé : 12 partout) et aligner jauge + validation
serveur.
Erreur de confirmation : « Les deux mots de passe ne sont pas identiques. »
Bouton **Enregistrer le mot de passe**, lien **Annuler**.

**`reset-done`** — pastille `.gx-a-big.ok` (`--success-50` / `--color-success`,
icône `check` 34px), titre « Mot de passe modifié », texte « Vous pouvez vous
connecter avec votre nouveau mot de passe. **Les autres appareils ont été
déconnectés.** » (donc : invalider les sessions et le security stamp), bouton
**Se connecter**.

---

## 3 · Demande d'ouverture d'espace (`ob`) — 6 étapes

Écran **public**, marque GymXYZ, hors tenant. C'est le formulaire commercial :
il crée une **Demande**, pas un compte.

### Chrome

- `.gx-ob` : `min-height:100vh`, fond `--surface-page`.
- **Barre haute** `.gx-ob-top` collante : fond
  `color-mix(in srgb, var(--surface-card) 88%, transparent)`,
  `backdrop-filter:blur(10px)`, bord bas `1px --border-subtle`. Intérieur
  `max-width:1120px; padding:14px 28px` : marque GymXYZ à gauche, à droite
  « Déjà un espace ? **Se connecter** ».
- **Barre de progression** `.gx-ob-prog` : `height:3px`, fond `--surface-sunken`,
  remplissage `--color-primary`, largeur = `round(step/6 × 100)%`
  (donc 0 % à l'étape 1, 83 % à l'étape 6 — la barre n'atteint 100 % qu'à l'envoi).
- **Stepper** `.gx-ob-steps` : 6 pastilles, `max-width:780px`,
  `justify-content:space-between`, séparateurs 14×1px.
  État : `.on` (fond `--azure-50`, texte `--azure-800`, pastille `--color-primary`
  blanche), `.done` (pastille `--success-50` / `--color-success` avec un `check` 12px),
  neutre (`--surface-sunken` / `--text-subtle`). Numéro dans un cercle 20px.
  Étapes : **Profil · Structure · Contact · Formule · Marque · Récapitulatif**.
- **Carte** `.gx-ob-card` : `max-width:780px`, `border-radius:20px`,
  `border:1px solid --border-subtle`, `box-shadow:var(--shadow-md)`,
  animation d'entrée `gx-ob-rise` (translateY 10px → 0, opacity 0 → 1, `.32s`,
  `--ease-out`) **rejouée à chaque changement d'étape** (`key={step}`).
  - En-tête `.gx-ob-h` (`padding:28px 32px 0`) : eyebrow uppercase
    (`--text-2xs`, bold, `--tracking-wider`, `--color-brand`), `h2` en police
    d'affichage `var(--text-2xl)`, paragraphe `var(--text-md)` `--text-muted`
    `max-width:56ch`.
  - Corps `.gx-ob-body` : `padding:24px 32px 28px`, **grille 2 colonnes**,
    `gap:16px`. `.full` = pleine largeur, `.stack` = colonne pleine largeur.
  - Pied `.gx-ob-foot` : `padding:16px 32px`, bord haut, fond `--surface-sunken`,
    « Étape N sur 6 » à gauche (`margin-right:auto`), puis **Retour** (ghost,
    chevron gauche — libellé « Connexion » à l'étape 1) et **Continuer**
    (primary, flèche droite) / **Envoyer ma demande** à l'étape 6.
- **Bandeau de réassurance** sous la carte (`.gx-ob-reassure`, `var(--text-xs)`,
  coches `--color-success`) : « Sans engagement · Aucun paiement à cette étape ·
  Réponse sous 1 jour ouvré · Données hébergées en France ».

### Étape 1 — Profil

Eyebrow « Bienvenue » · H2 « Vous gérez quoi, exactement ? » · « La suite du
formulaire s'adapte à votre réponse. Deux minutes, pas plus. »

Deux **cartes de choix** (`.gx-choice`, grille 2 colonnes, gap 14) :
`.gx-choice-card` — `border:1.5px solid --border-default`, `radius --radius-lg`,
`padding:20px`, icône 42px carrée (`--azure-50` / `--color-primary`, inversée en
plein azure quand sélectionnée), titre, description, 3 puces à coche verte.
Survol : `translateY(-2px)` + `--shadow-md`. Sélection : bord `--color-primary`,
`box-shadow:var(--glow-spark)`, pastille de coche 22px en haut à droite.

| | Salle de sport ou club (`building-2`) | Coach indépendant·e (`user`) |
|---|---|---|
| Desc | Une équipe, des cours collectifs, plusieurs créneaux par jour. | Vous travaillez seul·e, en studio, à domicile ou en extérieur. |
| Puces | Plusieurs coachs et lieux · Cours collectifs et présences · Abonnements et relances | Séances individuelles et petits groupes · Suivi de vos client·es · Cartes de séances et paiements |

Note : « Vous gérez un réseau de plusieurs salles ? Choisissez « Salle de sport » :
nous ajouterons les autres lieux avec vous. »

**Ce choix pilote tout le reste du formulaire** (`solo = profile === 'coach'`) :
libellés, champs adresse vs zone, listes de tailles.

### Étape 2 — Structure / Activité

| Champ | Salle | Coach | Obligatoire |
|---|---|---|---|
| Nom | « Nom de la structure » — *Atlas Training Club* | « Nom de votre activité » — *Naj Coaching* | oui |
| SIRET | *918 402 551 00019* | idem + aide « Laissez vide si votre immatriculation est en cours. » | non |
| Taille | « Nombre de membres » → `sizes` | « Client·es suivi·es » → `sizesSolo` | non |
| Localisation | Adresse (`map-pin`) + Code postal + Ville | « Zone d'intervention » (`map-pin`) — *Thonon-les-Bains et 30 km alentour* | non |
| Disciplines | « Disciplines proposées » | « Spécialités » | non |

`sizes` : Moins de 50 membres · 50 à 150 · 150 à 400 · 400 à 800 · Plus de 800.
`sizesSolo` : Moins de 20 clients · 20 à 50 · 50 à 120 · Plus de 120.
Aide disciplines : « Séparez par des virgules. Cela nous sert à préparer vos
modèles de cours. »

Le **code postal** conditionne la zone de vacances scolaires du futur espace
(`gxZoneForZip`, cf. lot 2) — le récupérer ici évite de le redemander.

### Étape 3 — Contact & compte

Prénom* · Nom* · Rôle (liste : Gérant·e / Responsable administratif / Coach &
gérant·e / Président·e d'association / Autre) · Téléphone (`phone`) ·
E-mail professionnel* (`mail`, pleine largeur, aide « Nous y envoyons l'accusé de
réception, puis vos identifiants. ») · Mot de passe (pleine largeur, avec œil +
jauge de robustesse), aide « 12 caractères minimum. **Il sera actif à l'ouverture
de votre espace.** »

> Décision produit à confirmer : le mot de passe est saisi ici mais le compte
> n'existe qu'à la validation. Deux implémentations possibles — (a) stocker le
> hash sur la Demande et créer l'utilisateur à la validation, (b) ne rien
> stocker et envoyer un lien d'activation. **(b) est plus sain** (RGPD : rien à
> purger, pas de secret dormant). Si (b) est retenu, **retirer le champ** du
> formulaire plutôt que de le laisser décoratif.

### Étape 4 — Formule

3 cartes `.gx-plan` (grille 3 colonnes, gap 13, `padding:18px 16px`, mêmes états
survol/sélection que les cartes de choix). Ruban `.gx-ribbon` en haut à gauche
(`top:-9px; left:14px`, fond `--color-primary`, texte blanc uppercase 10.5px).
Prix en police d'affichage `var(--text-xl)`, unité en Montserrat `--text-xs`.

| Formule | Prix | Pour | Inclus |
|---|---|---|---|
| **Essentiel** | 49 € / mois | Coach indépendant ou petite structure, jusqu'à 80 membres. | Planning & réservations · Fiches membres · Abonnements et relances |
| **Pro** *(ruban « Le plus demandé », sélectionnée par défaut)* | 129 € / mois | Salle avec équipe de coachs, jusqu'à 600 membres. | Tout Essentiel · Multi-lieux & multi-coachs · Présences, QR code, statistiques · Marque blanche complète |
| **Sur-mesure** | Sur devis | Réseau, franchise ou besoin métier spécifique. | Tout Pro · Développements dédiés · Reprise de vos données · Interlocuteur unique |

Encart `.gx-a-note` (icône `wallet`) : « Aucun paiement à cette étape, et pas de
carte à saisir. Après validation de votre demande, nous convenons d'un échange de
20 minutes puis vous recevez un devis. »

⚠️ Ces plafonds (**80 / 600 membres**) diffèrent de ceux de la console
(**150 / 600**, `07-CONSOLE-PLATEFORME.md`). **Une seule source de vérité** : la
table `Plans` de la plateforme, affichée ici. Corriger la copie commerciale en
conséquence, ne pas dupliquer les chiffres.

### Étape 5 — Marque dans l'application

Quatre blocs en colonne (`.stack`) :

1. **Votre logo** — zone de dépôt `.gx-drop` (`border:1.5px dashed --border-strong`,
   radius `--radius-lg`, `padding:22px`, fond `--surface-sunken`, vignette 46px).
   Vide : « Déposez votre logo ici / PNG ou SVG, fond transparent de préférence.
   Vous pourrez le changer plus tard. » + bouton **Parcourir**.
   Rempli : fond `--azure-50`, bord plein, coche azure, nom du fichier,
   « PNG · 84 Ko · fond transparent », bouton ghost **Retirer**.
   *Dans le prototype le dépôt est simulé* (clic → `logo-structure.png`).
   Production : `<InputFile>`, PNG/SVG/JPG, **2 Mo max**, antivirus, stockage hors
   webroot, servi via un endpoint autorisé.
2. **Couleur d'accent** — 6 pastilles `.gx-sw` de 38px (cercles, bord 2px
   `--text-strong` quand sélectionné, coche blanche 16px) :
   Azure `#00ABFC` · Graphite `#232327` · Rose `#CB5B74` · Sauge `#7E8E64` ·
   Ambre `#D08A2C` · Indigo `#4C5BD4`.
   Sous la ligne : « Sélection : **{label}**. Nous ajustons ensuite les contrastes
   pour rester lisible et accessible. » — c'est une **intention**, pas le thème
   final ; les thèmes sur-mesure sont produits hors application (cf. console).
3. **Adresse de votre espace** — champ + suffixe collé `.gx-dom .sfx`
   (`height:var(--control-md)`, fond `--surface-sunken`, bord `1.5px`, sans bord
   gauche, radius droite seulement) : `.gymxyz.fr`. Saisie normalisée en direct :
   `toLowerCase().replace(/[^a-z0-9-]/g,'')`. Message : « *{sub}*.gymxyz.fr est
   disponible. » en `--color-success`, sinon « Lettres, chiffres et tirets. Un nom
   de domaine à vous est possible plus tard. »
   ⚠️ Le prototype **ne vérifie pas** la disponibilité. Production : vérification
   serveur (débounce 400 ms) + **liste de sous-domaines réservés** (www, api, app,
   admin, console, mail, static…).
4. **Aperçu** `.gx-prev` — vignette 168px de haut, grille `88px 1fr` : fausse
   sidebar `#12181F` avec le nom tronqué à 11 caractères et 5 barres (la première
   à la couleur d'accent), corps `--surface-page` avec deux cartes fantômes et un
   bouton « Diffuser le planning » à la couleur choisie. **C'est un aperçu
   symbolique, pas un rendu du thème réel** — ne pas le sur-promettre.

### Étape 6 — Récapitulatif

Trois cartes `.gx-recap` (bord `--border-subtle`, radius `--radius-lg`, en-tête
`--surface-sunken` avec lien **Modifier** à droite qui renvoie à l'étape) :
**Structure/Activité** (→ étape 2), **Contact** (→ 3), **Formule & marque** (→ 4).
Contenu en `dl.gx-kv2` : grille `auto 1fr`, gap `10px 18px`, `dt` `--text-muted`,
`dd` aligné à droite, semibold `--text-strong`, `—` si vide.

Puis 3 consentements (`.gx-consent`, gap 14) :

1. **Obligatoire** — « J'accepte les *conditions générales* de GymXYZ et je
   confirme être habilité·e à engager la structure. »
2. **Obligatoire** — « J'autorise GymXYZ à traiter ces informations pour étudier
   ma demande. Données hébergées en France, supprimées sous 3 mois en cas de
   refus. *Politique de confidentialité*. »
3. Facultatif — « Je veux recevoir les nouveautés produit, environ une fois par
   trimestre. (facultatif) »

Le bouton **Envoyer ma demande** est `disabled` tant que 1 et 2 ne sont pas
cochés (`canSubmit = cgu && rgpd`).

> Le prototype **ne valide aucun champ**. Production : validation à chaque étape
> (bloquer « Continuer »), messages d'erreur sous les champs, ton du système
> (« Ce champ est requis. »). Champs requis minimum : nom, prénom, nom, e-mail,
> et sous-domaine.
> Ajouter un **anti-bot** (honeypot + limitation par IP) : le formulaire est
> public.

---

## 4 · Demande envoyée (`ob-sent`)

Même chrome, une seule carte. Barre haute : « Une question ? bonjour@gymxyz.fr ».

- Pastille verte `.gx-a-big.ok` + H2 « Demande envoyée ».
- « Merci *{prénom}* — votre demande pour **{structure}** est enregistrée et
  attend la validation de l'équipe GymXYZ. »
- Pastille de référence `.gx-ref` (pill, fond `--surface-sunken`, icône
  `file-text`) : « Référence **DEM-2026-0149** ». Format :
  `DEM-{année}-{séquence sur 4}`.
- Carte récap : Formule souhaitée · Adresse de l'espace · Reçue le · Statut
  = « En attente de validation ». En-tête : « Accusé de réception envoyé à
  {e-mail} ».
- **Ce qui se passe ensuite** — timeline `.gx-tl2` (pastilles 27px, filet
  vertical 1.5px `--border-subtle`, `done` = vert plein, `now` = azure plein) :
  1. *Demande reçue* — « Nous avons votre dossier. Un accusé de réception part
     par e-mail. » (done)
  2. *Vérification* — « Nous contrôlons les informations de la structure. Un jour
     ouvré en moyenne. » (now)
  3. *Échange de 20 minutes* — « Un appel pour cadrer vos besoins et votre
     planning de démarrage. »
  4. *Devis puis ouverture de l'espace* — « À la signature, votre espace est
     ouvert et vos comptes créés. »
- Pied : « Vous pouvez fermer cette page : tout est dans l'e-mail. » + bouton
  **Aller à la connexion**. *(Le bouton « Vue super-admin (démo) » est un
  raccourci de maquette — à ne pas porter.)*

**E-mail d'accusé de réception** : à écrire (pas maquetté). Doit contenir la
référence, le récapitulatif, et le délai annoncé. Modèle à ajouter au référentiel
d'e-mails de la console.

---

## 5 · Mobile

Même parcours, dans le shell mobile (`auth-mobile.jsx`, styles `.gx-ma*`).
Le prototype affiche l'écran dans un cadre iPhone **402 × 874** mis à l'échelle —
cadre = outil de maquette, pas un livrable.

- `.gx-ma` : colonne pleine hauteur, fond `--surface-page`.
- **Hero** `.gx-ma-hero` : fond `--gx-a-panel` (mêmes tokens de marque),
  `padding:64px 22px 26px` (les 64px laissent la place à la barre d'état),
  trame de points 22px, wordmark de la marque, `h1` 23px en police d'affichage,
  paragraphe `--text-sm`.
- **Corps** `.gx-ma-body` : `flex:1; overflow-y:auto; padding:22px 20px 26px;
  gap:15px` — c'est **la seule zone qui scrolle**.
- **Barre d'action** `.gx-ma-bar` : collée en bas, `padding:12px 20px 26px`
  (26px = zone home), bord haut, fond `--surface-card`.
- Onboarding mobile : mêmes 6 étapes, une par écran, cartes de choix et formules
  **empilées** (`.gx-ma-choice`, `.gx-ma-plans` en colonne), barre de progression
  `.gx-ma-prog` 3px sous le header, compteur « Étape N sur 6 » dans la barre basse.
- Cibles tactiles ≥ 44px partout (`--control-md`).

---

## 6 · Données & API

### Entité `DemandeOuverture` (nouvelle — hors `TenantId`, c'est du pré-tenant)

```
Id, Ref (DEM-AAAA-NNNN, unique)
Type            : Salle | Coach
Nom, Siret?, TailleLibelle?, Disciplines?
Adresse?, CodePostal?, Ville?  |  ZoneIntervention?   (selon Type)
ContactPrenom, ContactNom, ContactRole?, ContactEmail, ContactTelephone?
PlanDemande     : Essentiel | Pro | SurMesure
AccentHex, AccentLabel, SousDomaineSouhaite, LogoAssetPath?
Message?
Statut          : ATraiter | EnCours | Validee | Refusee
AssigneeUserId? , Source (Formulaire en ligne, Site vitrine, Recommandation…)
RecueLe, ConsentementCgu, ConsentementRgpd, OptinNewsletter
+ Activites[] (Titre, Detail, Horodatage, Etat: done|now)
+ NotesInternes[] (Auteur, Horodatage, Texte)
```

Purge : suppression automatique **3 mois après un refus** (c'est écrit au
consentement — un job de purge est obligatoire, pas optionnel).

### Commandes / requêtes

`SubmitDemandeOuvertureCommand` (public, anti-bot, validator complet) ·
`CheckSubdomainAvailabilityQuery` (public, débounce) ·
`RequestPasswordResetCommand` · `ResetPasswordCommand` (Identity) ·
Les commandes de traitement (valider / refuser / complément / note) sont
spécifiées dans `07-CONSOLE-PLATEFORME.md`.

## 7 · États à couvrir (absents de la maquette)

| État | Où | Attendu |
|---|---|---|
| Identifiants invalides | login | message sous le formulaire, générique |
| Compte verrouillé | login | message + délai, règle à décider |
| Espace suspendu (impayé) | login | message dédié — c'est le ticket SUP-0312 de la console |
| Lien de reset expiré / déjà utilisé | reset | écran « Ce lien n'est plus valable. » + renvoi |
| Validation par étape | onboarding | erreurs sous les champs, « Continuer » bloqué |
| Sous-domaine déjà pris | onboarding étape 5 | message en `--color-danger` + suggestion |
| Envoi en cours | onboarding étape 6 | bouton en état chargement, double-envoi impossible |
| Échec d'envoi | onboarding | conserver la saisie, proposer de réessayer |
