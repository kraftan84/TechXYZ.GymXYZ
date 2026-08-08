# 07 — Console plateforme (super-admin TechXYZ)

> Prototype : `design/GymXYZ Console.html`
> Sources : `design/app/console-shell.jsx` (shell + primitives),
> `console-vue.jsx` (vue d'ensemble, clients, fiche client),
> `console-ops.jsx` (facturation, support, formules, santé, référentiels),
> `console-main.jsx` (routage + Tweaks), `console-data.js` (données),
> `console.css` (styles), et `auth-admin.jsx` pour les écrans **Demandes** et
> **Fiche demande**, réutilisés tels quels.
>
> **Ce document remplace le lot 7 « Administration »** de `01-LOTS.md`, qui
> décrivait deux panneaux dans l'écran Réglages. La console est désormais une
> **application à part**, avec son propre shell.

**Fidélité : high-fidelity.** Desktop uniquement (≥ 1240px) — voir §9.

---

## 1 · L'idée, et la décision qui la structure

La console est l'outil de **TechXYZ**, pas du client. Elle sert à faire tourner
la plateforme : traiter les demandes d'ouverture, suivre les clients, la
facturation, le support, la santé technique et les référentiels partagés.

> **Arbitrage retenu — le super-admin n'entre jamais dans un espace client.**
> Il voit des **compteurs agrégés** (nombre de membres, de cours, de
> réservations, de connexions), les **comptes gestionnaires**, la facturation et
> les tickets. Il ne voit **aucune fiche membre, aucun planning, aucun paiement
> d'adhérent**. Pour dépanner : appel ou partage d'écran.
>
> Le prototype expose ce choix en Tweak (`impersonation`, **faux par défaut**)
> pour montrer la variante. Si vous activez l'impersonation en production :
> bouton `danger` « Ouvrir l'espace », bandeau rouge permanent pendant toute la
> session, entrée au journal d'audit, et consentement contractuel du client.
> **Par défaut : ne l'implémentez pas.** C'est un choix RGPD, pas une
> préférence d'UI.

Cet arbitrage doit se traduire en code par une **frontière de requêtes** : les
queries de la console ne projettent que des agrégats, jamais une entité métier
d'un tenant. Le filtre global `TenantId` est **contourné explicitement** et
uniquement dans ces queries, par une méthode nommée (`IgnoreQueryFilters()`
encapsulée dans un `IPlatformQuery`), jamais au fil de l'eau.

---

## 2 · Shell

`.gxc-root` : colonne pleine hauteur, `overflow:hidden`.

### Bandeau plateforme `.gxc-top` — la signature de l'univers super-admin

`height:34px`, fond `#0C2236` (ink), texte `rgba(255,255,255,.72)`,
`font-size:var(--text-2xs)`, `letter-spacing:.04em`, `padding:0 18px`, gap 14.
Filet `::after` de 2px en bas : `linear-gradient(90deg,#00ABFC,rgba(0,171,252,0))`.

Contenu, de gauche à droite :
`TECH`*XYZ* (police d'affichage, 11px, `.12em`, `XYZ` en `#00ABFC` medium) │
« Console plateforme · **GymXYZ** » │ *(spacer)* │ pastille env
(`background:rgba(0,171,252,.16)`, texte `#7CD4FF`, point vert 6px `#22C55E`) :
« Production · v1.9.2 » │ « 6 clients · 1 322 membres gérés ».

Ces deux derniers blocs sont **calculés**, pas écrits en dur.

### Sidebar (256px, réutilise `.gx-sb` de l'app)

Marque `.gxc-brand` : `GYM`*XYZ* en police d'affichage 19px + sur-titre
« CONSOLE PLATEFORME » (`--text-2xs`, `.22em`, uppercase, atténué), séparé par un
filet bas.

Navigation, en 4 groupes (`CON_NAV`) :

| Groupe | Entrées | Icône (Lucide) | Badge |
|---|---|---|---|
| **Ce matin** | Vue d'ensemble | `house` | — |
| **Clients** | Demandes | `file-text` | nb « à traiter » |
| | Clients | `building-2` | — |
| | Facturation | `euro` | nb d'impayés |
| **Assistance** | Support | `mail` | nb de tickets non résolus |
| | Santé & journal | `cloud` | — |
| **Produit** | Formules & tarifs | `layers` | — |
| | Référentiels | `layout-grid` | — |

Pied de sidebar : lien « Démo commerciale » (`eye`) vers l'app de démo —
*artefact de maquette, à retirer en production* — et l'encart `.gx-theme-hint` :
« Vous ne voyez jamais les données d'un client. Chaque consultation est
**journalisée**. »

### Topbar (64px, `.gx-tb` de l'app)

Recherche globale (placeholder « Rechercher un client, un ticket, une facture… »),
pastille **SUPER-ADMIN** `.gx-adm-badge` (pill `--azure-50` / `--azure-800`,
10.5px bold uppercase), cloche avec point, séparateur, bloc utilisateur
(avatar + « Julien Roux » / « Super-admin · TechXYZ »).

La recherche n'a **pas de comportement défini** — même écart que dans l'app
(README, écart n°1). Proposition : clients + tickets + factures, `Ctrl+K`.

---

## 3 · Vue d'ensemble

Titre : « **Ce matin, jeudi 6 août 2026** » (date du jour, formatée FR),
sous-titre « Trois choses vous attendent. Le reste tourne. », bouton outline
**Actualiser** (`refresh-cw`).

**L'ordre est intentionnel : la file d'abord, les chiffres ensuite.**

### a) Carte « À traiter » — la file du matin

En-tête avec un chip warning « N sujets ». Contenu : pile de lignes
`.gxc-alert` (`padding:16px 18px`, filet haut entre les lignes, survol
`--surface-sunken`, chevron à droite qui passe en azure au survol, vignette 38px
carrée colorée par le ton : `brand` = `--azure-50`, `warn` = `--warning-50`,
`bad` = `--danger-50`).

Les trois lignes sont **dérivées de l'état**, pas listées en dur :
1. *N demandes d'ouverture attendent* → « La plus ancienne : {nom} · reçue le
   {date} » → **Demandes**.
2. *N tickets sans réponse depuis plus de 24 h* → liste des clients → **Support**.
3. *{client} — N échéances impayées, {montant}* → « Prélèvement rejeté le
   {date}. L'espace est suspendu depuis ce matin. » → **Facturation**.

Une ligne absente = la carte s'allège. Si les trois sont absentes, afficher un
état vide honnête (« Rien à traiter. Bonne journée. » est déjà le ton utilisé
dans le support).

### b) 4 KPI (`.gx-adm-kpis`, grille 4 × 1, gap 14)

Revenu mensuel récurrent (somme des `mrr` des clients **actifs**, delta vert)
· Clients actifs (+ sous-titre « 1 en essai · 1 suspendu ») · Membres gérés
(somme des membres, format FR `1 322`) · Tickets ouverts (« délai de réponse
moyen : 4 h »).

### c) Grille 1.35fr / 1fr

**Gauche — « Activité des espaces — 30 derniers jours »** (`connexions
gestionnaires` en légende) : tableau `.gx-tbl` — Client (avatar initiales + nom
+ `sous-domaine.gymxyz.fr` en dessous) · Formule · Membres · Connexions ·
Signal (chip de santé) · chevron. Ligne cliquable → fiche client.

**Droite, colonne** :
- **État de la plateforme** (chip succès « Tout tourne ») : lignes `.gxc-svc`
  (pastille 9px `ok`/`warn`/`bad`, nom + détail, métrique alignée à droite).
- **Derniers événements** (« journal ») : 5 dernières entrées d'audit en
  timeline `.gx-tl2`, icône `user` si l'auteur est « Vous », `zap` si c'est le
  système.

---

## 4 · Demandes d'ouverture

*(Écran partagé avec le prototype d'entrée — `auth-admin.jsx`.)*

### Liste

Titre « Demandes d'ouverture », sous-titre « Les structures qui veulent un espace
GymXYZ. À traiter dans l'ordre d'arrivée. », bouton outline **Exporter**.

4 KPI : À traiter (« dont 2 reçues aujourd'hui ») · En cours (« échange ou
complément attendu ») · Validées ce mois (delta vert « +2 vs juillet ») ·
Délai moyen de réponse « 1,4 j » (« objectif : 1 jour ouvré »).

Filtres `.gx-fchip` (pills, actif = fond `--color-primary` blanc, compteur à
`opacity:.72`) : **Toutes · À traiter · En cours · Validées · Refusées**.

Tableau : Structure (avatar initiales + nom + « ville · référence ») · Profil
(chip Coach/Salle) · Formule · Taille · Reçue · Statut · chevron.
En-tête de carte : « N demandes » + « Cliquez une ligne pour ouvrir la fiche ».

**Statuts** (`AD_STATUS`) : `a-traiter` → « À traiter », ton warning, icône
`alert-triangle` · `en-cours` → « En cours », ton brand, `clock` · `validee` →
« Validée », succès, `check` · `refusee` → « Refusée », neutre, `x`.

### Fiche demande

Fil d'Ariane « Demandes / {nom} », titre = nom, sous-titre « {réf} · reçue le
{date} · {source} ».

Actions (uniquement si `statut ∈ {à traiter, en cours}`) :
**Refuser** (ghost) · **Demander un complément** (outline, `mail`) ·
**Valider et ouvrir l'espace** (primary, `check`). Sinon : **Retour aux demandes**.

Grille `1.45fr / 1fr`.

**Colonne gauche** — 4 cartes :
1. *Structure / Activité* (chip Coach/Salle) : Nom · Localisation · SIRET ·
   Membres estimés (ou « Client·es suivi·es ») · Disciplines.
2. *Contact* : avatar 40px + nom + rôle, deux boutons outline `sm` portant
   l'e-mail (`mail`) et le téléphone (`phone`) — ce sont des `mailto:` / `tel:`.
3. *Formule & marque demandées* (chip de formule) : Adresse souhaitée
   (`sub.gymxyz.fr`) · Logo fourni (ou « Non — à demander ») ; sous un filet :
   pastille ronde 34px de la couleur d'accent + « Accent {label} » +
   « {hex} · contrastes à vérifier avant ouverture ».
4. *Message de la structure* : le verbatim, en italique, entre guillemets
   français.

**Colonne droite** — 3 cartes :
1. *Suivi* (chip de statut) : Référence · Assignée à (« Personne » si nulle) ·
   Origine.
2. *Activité* : timeline `.gx-tl2` — chaque entrée = titre, détail, horodatage,
   état `done`/`now`.
3. *Notes internes* (chip = nombre) : liste `.gxc`-style (avatar sm, auteur en
   gras, horodatage atténué, texte), état vide « Aucune note. Écrivez ce que vous
   voulez retrouver dans six mois. », puis un `textarea .gx-ta`
   (`min-height:74px`, bord 1.5px, focus = `--ring-brand`) + bouton outline
   **Ajouter la note**, désactivé si vide.

### Les trois modales (`.gx-modal`, 540px, radius 20, scrim `rgba(12,34,54,.5)`)

| Modale | Sous-titre | Contenu | Action |
|---|---|---|---|
| **Valider et ouvrir l'espace** | « {nom} passera en client actif. L'espace est créé avec la formule choisie, puis l'invitation part au contact. » | Adresse de l'espace (champ + suffixe `.gymxyz.fr`) · Habillage (liste : « Créer un thème sur-mesure » + les thèmes existants ; aide « Un thème sur-mesure est livré sous 3 jours ouvrés. ») · Formule (Essentiel/Pro/Sur-mesure) · case « Envoyer l'invitation à **{email}** tout de suite » (cochée) | primary **Valider la demande** |
| **Refuser la demande** | « Le contact reçoit un e-mail avec le motif. Les données sont supprimées sous 3 mois. » | Motif (liste, ci-dessous) · Message au contact (`textarea`, placeholder « Expliquez simplement pourquoi, et orientez vers une solution si vous en connaissez une. ») | **danger** Refuser la demande |
| **Demander un complément** | « La demande passe « en cours » en attendant la réponse. » | Ce qu'il manque (`textarea`) · note « Relance automatique après 7 jours sans réponse. » | primary **Envoyer la demande** (`send`) |

Motifs de refus : Besoin hors périmètre du produit · Structure hors zone
d'intervention · Informations invérifiables · Projet non abouti / sans budget ·
Doublon d'une demande existante.

### Effets

| Action | Statut | Assignée à | Entrée d'activité | Toast |
|---|---|---|---|---|
| Valider | `validee` | l'utilisateur courant | « Demande validée — Espace {sub}.gymxyz.fr · formule {plan} [· invitation envoyée] » (done) | « Espace ouvert pour {nom}. Invitation en route. » |
| Refuser | `refusee` | idem | « Demande refusée — {motif} » (done) | « Demande de {nom} refusée. Le contact est prévenu. » |
| Complément | `en-cours` | idem | « Complément demandé — {message} » (now) | « Complément demandé à {contact}. » |
| Note | inchangé | inchangé | — (note ajoutée) | « Note ajoutée à la fiche. » |

Toast `.gx-toast` : pill ink centrée en bas (`bottom:28px`), icône `check` verte,
`--shadow-lg`, disparition après **3,4 s**. En Blazor : `IUserFeedbackService`.

**« Valider » est la seule action qui provisionne** : création du `Tenant`
(slug, ThemeKey, formule, coordonnées), du premier compte `GymManager`, du seed
minimal, et envoi de l'invitation. C'est une **transaction** — si l'e-mail
échoue, l'espace existe quand même et l'invitation est rejouable.

---

## 5 · Clients

### Liste

Filtres : Tous · Actifs · En essai · Suspendus.
Tableau : Client (avatar + nom + sous-domaine) · Type · Formule · Membres ·
Mensuel (`—` en essai) · Paiement (chip d'état de facturation) · Statut ·
chevron.

Statuts client : `actif` → succès · `essai` → brand · `suspendu` → **danger**.
Sous le tableau, l'encart `.gxc-note` (fond `--surface-sunken`, icône
`shield-check`) rappelle la frontière de données — **le garder, c'est de la
documentation à l'écran** :
« La console n'affiche que des **compteurs agrégés** et les comptes
gestionnaires. Les fiches membres, plannings et paiements des adhérents restent
dans l'espace du client — vous n'y avez pas accès. »

### Fiche client

Fil d'Ariane, titre = nom, sous-titre « {sub}.gymxyz.fr · {type} · {ville} ·
client depuis le {date} ». Actions : chip de statut, **Écrire au contact**,
**Changer de formule**, et — *seulement si l'impersonation est activée* —
**Ouvrir l'espace** en `danger`.

Grille `1.5fr / 1fr`.

**Gauche**
1. *Usage* (chip de santé) : bandeau `.gxc-minis` de 4 mini-KPI séparés par des
   filets 1px (Membres + « plafond 150 / 600 / illimité » selon la formule ·
   Cours / semaine · Réservations 30 j · Connexions 30 j « comptes
   gestionnaires »), puis l'histogramme `.gxc-spark` : 12 barres
   (`height:78px`, `gap:4px`, `--azure-100`, la dernière en `--color-primary`),
   axe « il y a 12 semaines » → « cette semaine ». Note : « Réservations par
   semaine sur douze semaines. Aucune donnée nominative n'est remontée à la
   console. »
2. *Comptes gestionnaires* : tableau Personne (avatar + nom + e-mail) · Rôle ·
   Dernière connexion · État (chip succès « Actif » / warning « Invitation en
   attente »).
3. *Tickets de ce client* : lignes `.gxc-alert` cliquables → Support ; état vide
   « Aucun ticket. Ce client ne vous a jamais écrit. »

**Droite**
1. *Abonnement* : Formule · Montant (« Gratuit pendant l'essai » le cas échéant) ·
   Prochaine échéance · Moyen de paiement · Dernier règlement.
2. *Habillage appliqué* (chip `palette` : Défaut / Standard / Sur-mesure) :
   nuancier `.gxc-sw` (bandes 22×34px, radius 6), libellé du thème, « Livré le
   {date} », note « Les thèmes sur-mesure sont produits hors application. La
   console affiche celui qui est en ligne. »
3. *Dossier* : Adresse · Ouvert le · Demande d'origine (réf) · Contact · E-mail ·
   Téléphone.
4. Encart `.gxc-note` final, **deux variantes** selon l'arbitrage impersonation
   (texte exact dans `console-vue.jsx`).

**Santé client** (`health`) — libellés dérivés, pas saisis :
« Usage régulier » (succès) · « Essai — N jours restants » (brand) ·
« Usage en baisse — vacances scolaires » (warning) · « Aucune connexion depuis
N jours » (danger). Définir les seuils avec le métier avant de coder.

---

## 6 · Facturation

Sous-titre : « Ce qui est dû, ce qui est rentré, ce qui bloque. **L'encaissement
lui-même se fait chez votre banque.** » — la console **suit**, elle n'encaisse
pas. Pas de PSP dans ce lot.

4 KPI : MRR (delta vert) · Encaissé en août (« 4 factures sur 5 ») · En échec
(valeur en `--color-danger`) · Impayé cumulé (idem).

Carte **Action requise** : deux lignes `.gxc-alert` — relance de niveau 2 sur
l'impayé, et fin d'essai imminente sans moyen de paiement. Les deux ouvrent la
fiche client.

Filtres : Toutes · Payées · Rejetées · Impayées.
Tableau : Référence · Client · Période · Émise · Échéance · Montant · Statut ·
bouton ghost **PDF**.

États de facture (`CON_PAY`) : `paye` « Payée » succès · `echec` « Prélèvement
rejeté » danger · `impaye` « Impayée » danger · `attente` « En attente » warning.

**Règle métier à confirmer** : la suspension d'un espace après **2 échéances
impayées** est automatique dans les données de démo (journal : « Espace suspendu
(2 impayés) », auteur = Système). Confirmer le seuil, le préavis, et ce que voit
le client suspendu à la connexion (cf. `06`, §7).

---

## 7 · Support

Sous-titre : « Chaque ticket arrive du bouton « Aide » de l'app cliente, avec le
contexte technique déjà joint. » → **il faut donc ajouter un bouton « Aide » dans
l'app cliente** (non maquetté — à cadrer, probablement dans la topbar près de la
cloche).

4 KPI : Ouverts (« aucune réponse encore ») · En cours (« réponse envoyée, en
attente ») · Sans réponse > 24 h (valeur en `--color-warning`, « objectif :
aucun ») · Délai moyen de réponse (« sur 30 jours »).

Filtres : À traiter · Tous · Résolus.

Disposition `.gxc-two` : **liste 340px + détail**, `gap:18px`, `align-items:start`.

**Liste** `.gxc-tk` : pastille d'état, client en majuscules atténuées, âge à
droite, sujet en semibold, « {réf} · {personne} · {date} ». Sélection : fond
`--azure-50` + liseré interne gauche 3px `--color-primary`. Liste scrollable
(`max-height:calc(100vh - 300px)`). Vide : « Rien à traiter. Bonne journée. »

**Détail** :
- En-tête = sujet, chip d'état (`ouvert` warning / `en-cours` brand / `resolu`
  succès) et, si priorité haute, chip danger « Priorité haute ».
- Bandeau demandeur : avatar md, nom, « {rôle} · {client} », bouton outline
  **Voir la fiche client**.
- **Fil de discussion** `.gxc-msgs` : bulles 82% max, radius `--radius-lg` avec
  un coin cassé à 4px du côté de l'auteur ; `them` = `--surface-sunken` bordée,
  alignée à gauche ; `me` = `--azure-50` bordée `--azure-200`, alignée à droite.
  En-tête de bulle : auteur en gras + horodatage.
- **Note interne** `.gxc-hint` (fond `--warning-50`, bord `--warning-100`, texte
  `--warning-800`) : visible **uniquement de TechXYZ**, jamais envoyée au client.
  Le contraste visuel avec les bulles doit rester évident — c'est un garde-fou.
- Zone de réponse : `textarea` (placeholder « Répondre à {prénom}… Dites ce que
  vous avez compris, ce que vous faites, et quand. »), bouton primary **Envoyer
  la réponse** (désactivé si vide) et ghost **Marquer résolu**.
  Envoyer → statut `en-cours` + message ajouté au fil.
  Si résolu : « Ticket résolu. Le client peut répondre pour le rouvrir. »
- **Contexte joint automatiquement** : `dl.gxc-ctx`, grille 3 colonnes, cellules
  `--surface-sunken` séparées de 1px — Écran · URL · Espace · Navigateur ·
  Système · Version. Note : « Compte à l'origine : {e-mail}. **Aucune donnée
  d'adhérent n'est jointe au ticket.** »

Le contexte est **capturé côté client au clic sur « Aide »** : route courante,
user-agent, version de build, tenant, compte. Rien d'autre.

---

## 8 · Formules · Santé & journal · Référentiels

### Formules & tarifs

3 cartes `.gxc-plan` (grille 3 colonnes, gap 16, `padding:22px`). La carte
recommandée porte un filet spark de 3px en haut et un chip « Le plus vendu ».
Contenu : nom (police d'affichage) · prix + unité · pour qui · bloc **limites**
sur fond `--surface-sunken` · liste de fonctionnalités à coches azure · pied
« **N clients** · **M €** / mois » + bouton ghost **Modifier**.

| Formule | Prix | Limites | Clients | MRR |
|---|---|---|---|---|
| Essentiel | 49 € / mois | 150 membres · 1 lieu · 2 comptes | 3 | 147 € |
| Pro | 129 € / mois | 600 membres · 3 lieux · 10 comptes | 2 | 258 € |
| Sur-mesure | Sur devis | illimité | 1 | 240 € |

Note : « Un changement de tarif ne s'applique qu'aux nouveaux contrats. Les
clients en cours gardent leur prix jusqu'à leur date anniversaire. » → **le prix
est porté par l'abonnement du tenant, pas par la formule.** C'est une contrainte
de modèle, pas une phrase marketing.

### Santé & journal

Deux cartes côte à côte : **Services** (mêmes lignes `.gxc-svc` que la vue
d'ensemble : Application, Base de données, Envoi d'e-mails, Sauvegardes, avec
métrique) et **Incidents récents** (timeline, le premier en `now`).

Puis **Journal d'audit** : tableau Horodatage · Auteur (chip brand « Vous » /
neutre « Système ») · Action · Cible. Légende : « conservé 3 ans · exportable sur
demande d'un client ».

Le journal n'est pas décoratif : c'est la contrepartie de l'accès plateforme.
**Toute** consultation de fiche client, changement de formule, validation de
demande, réponse à un ticket et action système (prélèvement rejeté, suspension,
facture émise) y entre.

### Référentiels

« Les listes partagées par tous les espaces clients. Une modification ici est
visible partout. » Grille `1.1fr / 1fr`.

- **Disciplines** : tags `.gxc-tag` (pills, croix de retrait) + champ d'ajout.
  Note : « Retirer une discipline ne supprime rien chez les clients qui
  l'utilisent déjà : elle disparaît seulement des nouvelles saisies. » →
  **soft delete**, comme partout.
- **Calendrier scolaire** (chip succès « Synchronisé ») : Zone · Académies ·
  Prochaines vacances. Source : `data.education.gouv.fr`, date de synchro
  affichée. C'est l'`ISchoolCalendarService` du lot 2, vu côté plateforme.
- **E-mails automatiques** : Modèle (+ « modifié le ») · Déclencheur · Envoyés
  30 j · **Modifier**. 5 modèles : Invitation d'un gestionnaire · Confirmation de
  réservation · Rappel de séance (J-1) · Abonnement bientôt échu · Prélèvement
  rejeté. Note : « Chaque modèle est envoyé au nom du client, avec son habillage.
  Vous éditez le texte commun ; les variables (nom du cours, date) sont
  remplacées à l'envoi. »
  → **manquent** l'accusé de réception de demande et le refus de demande (cf.
  `06`). Les ajouter au référentiel.
  L'éditeur de modèle n'est **pas maquetté**.

---

## 9 · Responsive, densité, animations

- Console **desktop uniquement**. Point de rupture unique dans `console.css`
  (`max-width:1180px`) : `.gxc-two`, `.gxc-plans` passent en 1 colonne,
  `.gxc-ctx` et `.gxc-minis` en 2. En dessous de ~900px, afficher un message
  plutôt qu'un shell mobile bricolé — le super-admin travaille assis.
- Densité (compact / standard / confort) et animations : **Tweaks de maquette**,
  à ne pas porter.

## 10 · Données à ajouter (plateforme, hors tenant)

```
Tenant            (déjà prévu lot 0) + Statut: Actif|Essai|Suspendu, EssaiFinLe?,
                  PrixMensuel (porté par le tenant, pas par la formule)
DemandeOuverture  (cf. 06-ENTREE-AUTH-ONBOARDING.md)
Plan              Nom, Prix, Unite, Limites[], Fonctionnalites[], EstRecommande
Invoice           Ref (F-AAAA-NNNN), TenantId, Periode, Montant, EmiseLe, EchéanceLe,
                  Statut: Payee|Echec|Impayee|Attente
SupportTicket     Ref (SUP-NNNN), TenantId, Demandeur, Role, Sujet, Statut,
                  Priorite, OuvertLe, Contexte{Ecran,Url,Navigateur,Os,Version,
                  Compte,Tenant}, NoteInterne?
TicketMessage     TicketId, Auteur, Cote: Client|Plateforme, Texte, Horodatage
AuditEntry        Horodatage, ActeurUserId?|Systeme, Action, Cible, Ip?
ServiceHealth     (sondes, pas une table métier — job de supervision)
Referentiel       Disciplines[], ModelesEmail[]
```

Requêtes agrégées attendues (toutes `AsNoTracking`, projection server-side, **et
aucune entité de tenant renvoyée**) :
`GetPlatformOverviewQuery` · `GetTenantsQuery` · `GetTenantUsageQuery` (12
semaines) · `GetTenantAccountsQuery` · `GetInvoicesQuery` ·
`GetSupportTicketsQuery` · `GetAuditLogQuery` · `GetReferentialsQuery`.

Commandes : `ValidateDemandeCommand` (provisionne le tenant) ·
`RejectDemandeCommand` · `RequestMoreInfoCommand` · `AddDemandeNoteCommand` ·
`ReplyToTicketCommand` · `ResolveTicketCommand` · `UpdateTenantPlanCommand` ·
`AddDisciplineCommand` / `RemoveDisciplineCommand` ·
`UpdateEmailTemplateCommand`.

**Autorisation** : policy `PlatformAdmin` sur **toute** la zone console, vérifiée
côté serveur (pas de masquage d'UI). Le `PlatformAdmin` n'a pas de `TenantId`.

## 11 · À décider avant de coder

1. **Impersonation** : oui / non. Défaut recommandé : **non** (voir §1).
2. **Suspension automatique** après N impayés : seuil, préavis, message côté
   client.
3. **Facturation** : la console émet-elle les factures (numérotation, PDF, TVA)
   ou reflète-t-elle un outil comptable existant ? Le bouton « PDF » suppose la
   première réponse.
4. **Bouton « Aide »** dans l'app cliente : emplacement, champs, pièce jointe
   éventuelle (capture d'écran ?).
5. **Éditeur de modèles d'e-mail** : à maquetter avant implémentation.
6. **Plafonds de formule** : cohérence avec l'onboarding (80 vs 150 membres pour
   Essentiel) — une seule source.
