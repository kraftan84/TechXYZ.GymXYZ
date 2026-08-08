# La console plateforme · brief de démarrage

**Écrit le 2026-08-08**, après la livraison des deux PR de l'entrée. `main` est à
`6c2b0f7` : lots 0 à 11, rôles, log, cloche et vacances, planning diffusé, et
l'entrée complète — connexion, réinitialisation, demande d'ouverture.

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

**C'est le point 5 de la première version** (`01-LOTS.md`). Spécification :
`07-CONSOLE-PLATEFORME.md`. Prototype : `design/GymXYZ Console.html`.

> **La suite est verte** : 634 tests, quatre projets. Les quatorze `CS8618` de
> `Domain` sont antérieurs à tout ça et le resteront tant que personne ne décide
> de les traiter — la DoD annonce « zéro warning », ce n'est pas exact et ça ne
> l'a jamais été.

---

## Ce que les docs disent de faux, et qu'il faut corriger d'abord

### 1. Le marqueur existe déjà, et il ne s'appelle pas comme ça

`01-LOTS.md` et `07` §1 annoncent un `IPlatformQuery` à créer. **Il existe, il
s'appelle `IPlatformScoped`**, il a été introduit par la PR 2 de l'entrée, et il
couvre les commandes autant que les requêtes — d'où le nom.

Trois requêtes le portent aujourd'hui : `SubmitSpaceRequestCommand`,
`CheckSubdomainAvailabilityQuery`, `PurgeRefusedSpaceRequestsCommand`.
`PlatformScopedPerimeterTests` **épingle la liste exactement**. Conséquence
directe et voulue : *chaque* requête de la console fera échouer ce test tant
qu'elle n'aura pas été nommée. C'est le filet, pas un obstacle.

### 2. Deux requêtes lisent déjà à travers les clients sans le dire

`GetTenantsQuery` et `GetTenantDetailQuery` traversent tous les clients — elles
sont antérieures au marqueur et ne le portent pas. `TenantMemberCounter` appelle
`IgnoreQueryFilters()` en toutes lettres, encapsulé et commenté, mais hors
marqueur lui aussi.

**Premier geste de code du lot** : les rattacher. Tant qu'elles restent dehors,
la liste épinglée ment par omission, et c'est le genre de mensonge qui rend un
filet inutile.

### 3. L'écran `/administration` existe déjà et fonctionne

`07` dit « ce document remplace le lot 7 Administration ». En pratique il y a sur
`main` une page routable sous policy `PlatformAdmin`, avec `CustomerList`,
`MarquePanel`, `FacturationPanel`, `NewTenantDrawer`, en desktop **et** en
mobile. Ce n'est pas un lot qui part de rien : c'est un lot qui **remplace du
code vivant**, et `RoutePerimeterTests` nomme `Administration.razor`.

À trancher avant de commencer, pas en cours de route : la console absorbe cet
écran et il disparaît, ou les deux coexistent le temps de la bascule.

### 4. L'impersonation n'est pas une décision à prendre — c'est du code livré

`07` §1 recommande de **ne pas l'implémenter** et présente ça comme un arbitrage
RGPD ouvert. Sur `main` : entité `TenantImpersonation`, commandes
`BeginTenantImpersonationCommand` / `EndTenantImpersonationCommand`, deux pages
`/account`, `ImpersonationBar` dans `MainLayout`, claims dédiés, tests.

Le conseil du handoff s'adresse à un terrain vierge qui n'existe plus. **La vraie
question est : on garde, ou on retire et on supprime le code.** Les deux sont
défendables, aucune n'est gratuite, et « on verra » revient à garder par défaut
sans l'avoir décidé.

### 5. Ce qui est réglé, et qu'il ne faut pas rouvrir

`07` §11.6 demande de trancher les plafonds de formule (80 vs 150 membres). **Fait**
à la PR 2 de l'entrée : `PlatformPlans` porte le catalogue une fois, sur les
chiffres de la console — **150 / 600**. La console n'a pas à re-trancher ; elle a
à promouvoir ce catalogue en table, ce que `07` §10 demande sous le nom `Plan`.

### 6. Les noms de `07` §10 ne sont plus ceux du code

| `07` §10 dit | Le code dit |
|---|---|
| `DemandeOuverture` | `SpaceRequest` (+ `SpaceRequestActivity`, `SpaceRequestNote`) |
| `Statut`, `RecueLe`, `SousDomaineSouhaite` | `Status`, `ReceivedOn`, `RequestedSubdomain` |
| `IPlatformQuery` | `IPlatformScoped` |

Le domaine est en anglais ; le français est réservé à ce qu'un utilisateur lit.
La spec est un document de conception, pas une source de noms.

---

## L'état réel des données, entité par entité

| `07` §10 demande | Sur `main` | Reste à faire |
|---|---|---|
| `Tenant` + Statut/EssaiFinLe/PrixMensuel | `IsSuspended` seul ; `PlanPrice` existe | l'état d'essai et sa date de fin |
| `SpaceRequest` | **complet**, hors tenant, avec activités et notes | rien |
| `Plan` (catalogue) | `PlatformPlans`, statique | la table, et la migration du statique vers elle |
| `Invoice` | existe : `Reference`, `Date`, `Amount`, `Status` | période, émission, échéance |
| `SupportTicket`, `TicketMessage` | **absents** | tout |
| `AuditEntry` | **absent** | tout |
| `ServiceHealth` | absent (sondes, pas une table) | à cadrer |

Deux points qui ne se voient pas dans un tableau :

- **`Tenant.IsSuspended` a été ajouté par l'entrée avec l'écran de connexion qui
  le lit — et rien pour le basculer.** Cet écran, c'est ce lot. En attendant, une
  suspension se pose à la main en SQL.
- **`SpaceRequestNote` est modélisée et rien ne l'écrit.** Même raison : c'est la
  fiche demande qui l'écrit, et elle est ici.

---

## Le point d'architecture, et il est plus dur qu'il n'en a l'air

**La console ne projette que des agrégats.** Jamais une entité métier d'un
client : ni fiche membre, ni séance, ni paiement d'adhérent. C'est écrit à
l'écran dans deux encarts que `07` demande explicitement de garder — « c'est de
la documentation à l'écran ».

Le marqueur `IPlatformScoped` dit *où* le filtre ne s'applique pas. Il ne dit
rien de *ce qui est projeté*, et c'est là qu'est le risque : une requête
plateforme parfaitement nommée qui renvoie un `MemberDto` respecte le marqueur et
viole la règle. Le filet à poser en plus est un test qui vérifie **ce que les
requêtes de la console renvoient**, pas seulement qu'elles sont étiquetées.

Et le journal d'audit est la contrepartie de cet accès, pas une fonctionnalité :
`07` §8 demande que **toute** consultation de fiche client y entre. Une écriture
par handler s'oublie au vingtième ; c'est une préoccupation transversale, à
traiter comme le `ManagerOnlyBehaviour` traite la sienne.

---

## Les décisions attendues avant de coder

1. **L'impersonation** — garder le code livré, ou le retirer. Reformulée plus
   haut : ce n'est pas un choix d'implémentation, c'est un choix de suppression.
2. **L'écran `/administration` existant** — absorbé et retiré dans ce lot, ou
   maintenu en parallèle.
3. **La suspension automatique** après N impayés : seuil, préavis, et ce que voit
   le client suspendu — l'écran qui l'affiche existe déjà depuis l'entrée.
4. **La facturation** : la console émet-elle les factures (numérotation, PDF,
   TVA) ou reflète-t-elle un outil comptable ? Le bouton « PDF » de la maquette
   suppose la première réponse, et c'est la plus chère de loin.
5. **Le bouton « Aide » dans l'app cliente** : sans lui, **Support n'a aucune
   boîte d'entrée** et l'écran est une coquille. Il n'est pas maquetté. Soit il
   entre dans ce lot, soit Support en sort.
6. **L'éditeur de modèles d'e-mail** : non maquetté. Même arbitrage.
7. **Les migrations EF** — voir ci-dessous. C'est la décision la plus structurante
   des sept.

---

## L'échéance de la dette : la base cesse d'être jetable ici

C'est l'**entrée 5** du registre (`LOT-13-BRIEF.md`), et son échéance est
exactement ce lot. Aujourd'hui : aucune migration, `EnsureCreatedAsync()` au
démarrage, `ResetDatabaseOnStartup` qui recrée la base en développement. Ajouter
une entité est gratuit.

Ça cesse de l'être au moment précis où « Valider et ouvrir l'espace » crée un
vrai client. À partir de là, la base contient des données que personne n'accepte
de perdre, et la migration initiale n'est plus une formalité mais une bascule.

**À trancher au début du lot**, pas à la fin : la migration initiale prise sur le
schéma courant *avant* d'ajouter les entités de la console, ou après. Prise avant,
chaque entité du lot arrive avec sa migration et le mécanisme est rodé quand il
compte. Prise après, il faut la faire au moment où il y a le plus à perdre.

---

## Pièges déjà payés — ne les repayez pas

Ceux de l'entrée valent encore, en particulier :

- **`Tenant:RootDomain` vaut `localhost` en développement.**
  `http://teamtrainers.localhost:5173` sert la marque du client. Pas de
  `/etc/hosts`, pas de sudo. Le volet navigateur refuse un `navigate` vers un
  autre hôte : passer par `preview_start {url}`.
- **Comptes de démo** : `dwayne.johnson@gymxyz.fr`, `admin@techxyz.fr` (le
  super-admin, sans `TenantId`), `aurelie.siquier@teamtrainers.fr`,
  `najate.amzil@leyssa-coaching.fr`, coach `marine.debord@teamtrainers.fr`.
  Mot de passe **`GymXyz!2026!`** — douze caractères depuis l'entrée.
- **`dotnet build` ne met pas à jour le serveur en cours.** Arrêter et relancer.
- **Un seul serveur peut tenir le port 5173**, et c'est à celui qui l'ouvre de le
  fermer.

Et celui-ci est **récent, et il vise ce lot en particulier** :

> **`string.StartsWith` sur un paramètre ne se traduit pas chez MySQL.**
> *« Expression '[SqlParameterExpression] COLLATE utf8mb4_bin' … does not have a
> type mapping assigned »*, et dans un circuit ça ne tue pas la requête mais le
> circuit. **Le fournisseur en mémoire des tests l'exécute sans broncher** :
> onze tests verts n'ont rien dit, seul le passage navigateur contre MySQL l'a
> montré. Utiliser `EF.Functions.Like(colonne, prefixe + "%")`.
>
> Ça vise ce lot parce que la console cherche par préfixe partout : la
> **recherche globale** de la topbar, les références de facture, les
> sous-domaines, les tickets. Attendez-vous à le rencontrer plusieurs fois.

Deux autres à connaître :

- **La console n'aura rien à afficher au premier lancement.** `DbInitializer` ne
  seede aucune `SpaceRequest`, aucun ticket, aucune entrée de journal. Huit
  écrans vides ressemblent à huit écrans cassés — prévoir le seed **avec** les
  écrans, pas après. Les données de démonstration du prototype
  (`design/app/auth-data.js`, `console-data.js`) sont cohérentes et directement
  utilisables.
- **En développement l'e-mail ne part pas** : `LoggingEmailSender` remplace Brevo
  tant qu'aucune clé n'est posée. L'invitation du provisionnement et le refus de
  demande ne seront lisibles que dans le log du serveur.

---

## Le découpage — c'est le plus gros lot de la V1

Huit écrans, sept entités nouvelles ou étendues, une action qui provisionne, un
journal transverse, et une décision de migration. À titre indicatif, ma
préférence, **à valider** :

- **PR 1 — la console et les demandes.** Le shell (bandeau plateforme, sidebar,
  topbar), Vue d'ensemble, Demandes, Fiche demande et ses trois modales, le
  provisionnement en transaction, et la fondation du journal d'audit. C'est la
  PR qui rend le lot précédent utile : sans elle, les demandes s'empilent sans
  que rien ne puisse les traiter.
- **PR 2 — les clients.** Liste, fiche, requêtes agrégées (usage sur 12
  semaines, comptes gestionnaires), et le test qui vérifie qu'aucune entité de
  tenant ne sort.
- **PR 3 — facturation et support.** Dépend de la décision 4 et surtout de la 5 :
  sans bouton « Aide », Support sort de la PR.
- **PR 4 — formules, santé & journal, référentiels.** C'est là que le catalogue
  statique devient une table.

---

## Critère d'acceptation

- **Aucune entité de tenant ne sort d'une requête console** — vérifié par un test
  sur ce qui est projeté, pas seulement par le marqueur.
- **`IPlatformScoped` couvre tout ce qui traverse**, y compris les deux requêtes
  antérieures qui ne le portent pas encore.
- **« Valider et ouvrir l'espace » est une transaction** : le `Tenant`, le
  premier compte `GymManager` et le seed passent ensemble ou pas du tout, et
  **l'invitation est rejouable** si l'e-mail échoue.
- **Le journal d'audit est écrit par le code, pas par la bonne volonté** : une
  consultation de fiche client non journalisée est un défaut, pas un oubli.
- **La suspension a enfin son interrupteur**, et l'écran de connexion livré à
  l'entrée l'affiche pour de vrai.
- **Desktop uniquement, assumé** : sous ~900 px, un message, pas un shell mobile
  bricolé.
- **La question des migrations est tranchée et appliquée**, pas reportée.
- `dotnet test` vert, et le nombre de warnings inchangé.

---

## Ce qui n'est pas dans ce lot

- **Les comptes à casquettes multiples** (entrée 4 du registre) — point 6,
  délibérément après.
- **Le PSP** : la console *suit* la facturation, elle n'encaisse pas. `07` §6 est
  explicite.
- **L'écran de génération de l'affiche** (`08`, §6), toujours pas maquetté : le
  bouton « Aperçu » de l'Accueil l'attend, désactivé avec sa raison. C'est le
  seul aller-retour design encore ouvert sur du code livré.
- **Le dépôt de logo** de la demande d'ouverture, sorti de la PR 2 de l'entrée
  faute d'antivirus et de stockage : à rouvrir quand il y aura une histoire de
  fichiers, pas avant.

---

## Premier geste suggéré

1. **Ouvrir `design/GymXYZ Console.html`.** C'est la cible, elle est au pixel.
2. **Regarder `/administration` sur `main`**, et décider ce qu'il devient. C'est
   la seule décision qui change la forme de la PR 1.
3. **Trancher l'impersonation** — en sachant que la question est « retire-t-on du
   code livré », pas « en écrit-on ».
4. **Trancher les migrations**, et si c'est « avant », les faire avant tout le
   reste.
5. **Rattacher `GetTenantsQuery` et `GetTenantDetailQuery` au marqueur.** Petit,
   immédiat, et ça rend honnête le filet dont toute la suite dépend.

Revenir avec un plan qui dit **le découpage retenu**, **ce que devient
l'impersonation**, **ce que devient `/administration`**, **quand est prise la
migration initiale**, et **comment on prouve qu'aucune donnée d'adhérent ne
remonte**. **Attendre la validation avant d'écrire.**
