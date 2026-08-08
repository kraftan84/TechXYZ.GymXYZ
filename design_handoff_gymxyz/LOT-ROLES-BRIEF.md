# Rôles & cloisonnement · brief de démarrage

Écrit le 2026-08-07, après la fusion du lot 11. `main` est à `6af799b`,
**504 tests au vert**, lots 0 à 11 livrés, entrées 1 et 2 du registre fermées.

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

> **Numérotation à fixer.** Ce lot n'existe pas dans `01-LOTS.md`. Il remplace le
> lot 12 (portail membre) en tête de file, pour la raison donnée plus bas. Le
> fichier est nommé par son sujet plutôt qu'avec un numéro inventé.

---

## Ce qui a changé, et qui annule le brief précédent

Un premier brief a été écrit le même jour, autour du portail membre. **Il partait
d'un modèle faux et a été supprimé.** Ce qui l'invalide, dans vos termes :

- **Les membres ne se connecteront pas.** C'est pour ça qu'aucun écran membre
  n'a été maquetté — l'absence de maquette n'était pas un retard, c'était une
  décision.
- **Les invitations à un cours passeront par e-mail**, avec une réponse présent
  / absent et **sans connexion**. La suite viendra après.
- **Login, onboarding d'une salle ou d'un coach, espace super-admin** : vous y
  travaillez. Hors de ce lot.

Le brief précédent faisait du portail membre le prochain lot et de la fuite vers
les membres son argument principal. Les deux tombent. **Ce qui reste debout, et
qui devient le sujet, c'est le rôle `Coach`.**

---

## Le modèle de rôles, tel que vous l'avez posé

| Rôle | Périmètre |
|---|---|
| **Super-admin** (TechXYZ) | La plateforme. Entre chez un client depuis la console, avec trace. |
| **Manager de salle** / **coach indépendant** | Tout, pour sa salle ou son entreprise. |
| **Coach** (en salle) | La **visu des cours**, et la **main sur ses propres cours**. |
| ~~Membre~~ | Pas de compte. |

Deux choses sont **déjà justes** dans le code, et ne sont pas à refaire :

- **Le super-admin est cloisonné** — c'est le lot 11 PR 3, fermé hier. Hors
  impersonation il ne voit aucune donnée métier, et entrer chez un client ouvre
  une ligne `TenantImpersonation`.
- **Le coach indépendant est déjà un gérant.** Najate (Leyssa) porte le rôle
  `GymManager` avec le libellé « Coach ». Le modèle distingue donc bien
  « coach indépendant, patron de son entreprise » de « coach salarié d'une
  salle », et la restriction à venir ne touchera pas Leyssa. Vérifié dans le
  seed, pas déduit du libellé.

---

## Ce que l'application fait aujourd'hui face à ce modèle

**Mesuré, écran par écran, connecté en `nora.lemoine@gymxyz.fr` (rôle `Coach`,
mot de passe `GymXyz!2026`).**

| Écran | Coach aujourd'hui | Attendu |
|---|---|---|
| `/administration` | **refusé** → `/account/acces-refuse` | refusé ✔ |
| `/cours`, `/planning` | ouvert | ouvert ✔ |
| `/presences` | ouvert, **toutes les séances** | ses séances |
| `/members` | ouvert, **les 36 fiches** | à trancher (voir plus bas) |
| `/abonnements` | ouvert — MRR, encaissements, impayés | **non** |
| `/reglages` | ouvert — identité, équipe, e-mail du club | **non** |
| `/coachs` | ouvert, toutes les fiches | à trancher |

Un coach salarié voit donc aujourd'hui **le chiffre d'affaires du club et ses
réglages**, y compris la gestion des accès de l'équipe.

**Pourquoi.** `Components/Pages/_Imports.razor:2` porte un `@attribute
[Authorize]` nu — authentifié, n'importe quel rôle. Une seule politique est
réellement appliquée, `PlatformAdmin`, sur trois écrans. Et surtout :

> `GymPolicies.GymManager` est **déclarée** dans `Program.cs:112` et
> **appliquée sur zéro écran.**

La politique existe, elle est correcte, personne ne s'en sert. C'est la
découverte utile : le lot est petit à câbler parce que la moitié est déjà écrite.

**À ne pas exagérer.** Côté commandes, une seule vérifie le rôle de l'appelant —
`ReopenAttendanceSheetCommand.Handler.cs:31`, la décision du lot 6. Les autres
mentions de `GymManager` dans `Application` protègent **la cible** (ne pas
rétrograder le dernier gérant), pas l'appelant. Donc : **lecture totale, écriture
largement ouverte, une seule action réellement gardée.** Le lot doit traiter les
deux, et la partie écriture est la moins visible des deux.

---

## « Ses propres cours » : le lien existe, rien ne le lit

C'est le point qui décide si ce lot est simple ou pas, et la réponse est bonne :

- `Coach.UserId` existe et **est rempli par le seed** (`DbInitializer`,
  `CreateAccountAsync` pour Nora, Samir et Léa).
- `Session.CoachId` porte le coach de la séance ; `CourseTemplateCoach` porte
  ceux d'un cours du catalogue.

Donc « mes cours » se dit déjà : compte connecté → `Coach` par `UserId` →
séances où `CoachId` correspond.

**Mais rien ne consomme `Coach.UserId` pour autoriser quoi que ce soit** — la
seule occurrence hors seed est un commentaire dans `CoachDetailsPageDto`. Il n'y
a donc rien à défaire, seulement à brancher.

---

## Les comptes membres, qui contredisent le modèle aujourd'hui

Le seed crée **quatre comptes avec le rôle `Member`** (`DbInitializer`, autour de
la ligne 994) : Laetitia Moriceau, Amina Benali, Sarah Cohen, Lucas Martin.

**Ils fonctionnent.** Connecté en `laetitia.moriceau@gymxyz.fr`, on obtient la
console de gestion complète et la liste des 36 adhérents avec leurs adresses
e-mail. Vérifié aujourd'hui, pas supposé.

Puisque les membres ne doivent pas se connecter, ces comptes n'ont pas de raison
d'être et sont le seul chemin par lequel cette fuite est atteignable. **Les
retirer du seed est la correction la plus rentable du lot** : une suppression,
pas une fonctionnalité.

À trancher en même temps, parce que ça se voit à l'écran :

- Le rôle `Member` reste-t-il dans `GymRoles.All` ? Il n'a plus de porteur, mais
  les `Invitation` semées en portent le nom.
- `Member.UserId` reste-t-il dans le modèle ? Il ne sert plus à se connecter,
  mais il servira peut-être à relier une réponse « présent / absent » reçue par
  e-mail à la bonne personne.
- Réglages › Équipe affiche les accès. Que devient la ligne d'un membre qui
  n'a plus de compte — disparue, ou « invité, sans accès » ?

---

## Ce qui n'est pas dans ce lot

À dire explicitement, parce que trois d'entre eux sont chez vous :

- **Les écrans de login** — ils existent, vous les retravaillez.
- **L'onboarding d'une salle ou d'un coach.**
- **L'espace super-admin.**
- **Le portail membre** — sans objet tant que les membres ne se connectent pas.
- **Les invitations e-mail présent / absent.** Un lot à part entière : il faut un
  jeton signé par séance et par personne, une page publique sans authentification,
  et une décision sur l'expiration. `Invitation` et l'envoi Brevo du lot 8
  existent, mais rien de ce parcours n'est construit. **Ne pas l'entamer par
  morceaux ici.**

---

## Décisions à trancher avant de coder

1. **Un coach voit-il la fiche des membres ?** Il en a besoin pour pointer sa
   séance — la feuille de présence est une liste de personnes. La question n'est
   donc pas « oui / non » mais **« la liste complète du club, ou seulement les
   inscrits à ses séances »**. C'est la seule question du lot qui change une
   requête plutôt qu'un attribut.
2. **Un coach voit-il les autres coachs ?** Lecture seule, ou pas du tout.
3. **`/presences` pour un coach** : ses séances uniquement, ou toutes en lecture
   avec pointage limité aux siennes ?
4. **Que voit un coach à l'Accueil ?** Le tableau de bord du lot 10 agrège du
   chiffre d'affaires et des alertes d'abonnement. Il lui faut soit une version
   réduite, soit une autre page d'entrée.
5. **Un écran refusé : page « accès refusé » ou entrée masquée ?** Le lot 11 a
   posé le patron pour le super-admin sans client — navigation masquée **et**
   URL qui répond pour elle-même. Le même patron s'applique, et `GxNavigation`
   sait déjà filtrer.

---

## Pièges déjà payés

- **La base de dev est recréée au démarrage** (`ResetDatabaseOnStartup`) et
  déconnecte tout le monde. Une session ouverte ailleurs meurt avec.
- **Un seul serveur peut tenir le port 5173.** Deux sessions ne cohabitent pas.
- **Le volet navigateur ne transmet aucun clic.** Poster les formulaires en JS
  avec leur vrai jeton antiforgery ; le formulaire de connexion utilise des
  `fluent-text-field` dont les champs s'appellent `Input.Email` et
  `Input.Password`.
- **Les écrans rendent un état vide avant l'arrivée des données** — « 0 membre »,
  « Aucun cours cette semaine », « 1 janvier 0001 ». Ce sont des rendus
  transitoires. **Attendre la stabilisation avant de conclure** : ça a failli
  faire déclarer une régression inexistante au lot 11, et c'est la base de
  données qui a tranché.
- **Un changement d'autorisation se prouve dans les deux sens.** Le lot 11 PR 3
  a dû montrer non seulement que l'admin perdait l'accès, mais que le gérant et
  le coach le gardaient. Ici ce sera l'inverse et il en faudra autant.
- **`GxIcon` prend `Class`, jamais `Style`.**
- **Aucune couleur, taille ou famille en dur dans le balisage** — un test échoue
  dessus depuis le lot 11.

---

## Premier geste suggéré

1. **Se connecter soi-même en `nora.lemoine@gymxyz.fr`** et ouvrir `/reglages`
   puis `/abonnements`, pour voir ce qu'un coach salarié voit aujourd'hui plutôt
   que de me croire sur parole.
2. **Répondre aux cinq décisions ci-dessus.** La première (les membres vus par
   un coach) est la seule qui change une requête ; les autres sont des attributs.
3. **Trancher le sort des comptes membres** et du rôle `Member`.
4. Revenir avec un plan où **le retrait des comptes membres** et **la politique
   par écran** sont le travail livrable, et la revue trois rôles la preuve.

Attendre la validation avant d'écrire.
