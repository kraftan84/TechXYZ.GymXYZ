# L'entrée : connexion & demande d'ouverture · brief de démarrage

**Écrit le 2026-08-08**, après la livraison du planning diffusé. `main` est à
`87acdc4`, lots 0 à 11 livrés, plus « Rôles & cloisonnement », le lot du log,
celui de la cloche et des vacances, et le planning diffusé.

Ceci est un **brief de démarrage, pas un plan** : le plan se propose et se fait
valider avant d'écrire du code.

**C'est le point 4 de la première version** (`01-LOTS.md`), et le premier qui
reste. Spécification : `06-ENTREE-AUTH-ONBOARDING.md`. Prototype :
`design/GymXYZ Auth & Onboarding.html`.

> **La suite est verte au moment d'écrire** : 592 tests, quatre projets, zéro
> warning dans les projets applicatifs. On démarre propre — c'est la première
> fois depuis plusieurs lots, et ça vaut la peine de le préserver.

---

## Ce que `01-LOTS.md` dit de faux, et qu'il faut corriger d'abord

> « Connexion, mot de passe oublié, réinitialisation (`06`, §1 et §2). **Les
> écrans existent déjà en code depuis le lot 0** ; ce lot les remplace par la
> maquette. »

**Non.** Vérifié sur `main` : `Components/Pages/Account/` contient quatre
fichiers — `Login.razor`, `AccessDenied.razor`, `Impersonation.razor`,
`ImpersonationExit.razor`. Il n'existe **aucun** écran de mot de passe oublié,
aucun écran de lien envoyé, aucun de réinitialisation, aucun de confirmation. Et
`grep GeneratePasswordResetToken` ne renvoie rien : **le parcours de
réinitialisation n'existe pas du tout**, ni écran, ni jeton, ni e-mail.

Ce n'est pas un détail de cadrage. La phrase fait croire à un lot de
remplacement de maquettes ; c'est en réalité **quatre écrans à créer**, plus la
plomberie Identity et l'e-mail qui va avec. À prendre en compte dans le
découpage avant de s'engager sur une taille.

Ce qui existe vraiment, et qui est réutilisable :

| Acquis | Où | Ce que ça donne |
|---|---|---|
| Connexion fonctionnelle | `Account/Login.razor` | rendu serveur statique (un cookie ne s'écrit pas depuis un circuit), `EmptyLayout`, CSS maison `.gx-login` |
| Résolution du tenant par l'hôte | `Services/TenantResolver.cs` | `teamtrainers.gymxyz.fr` → `teamtrainers`, repli `DefaultSlug` |
| Envoi d'e-mail | `IEmailSender` | Brevo en production, `LoggingEmailSender` en dev |
| Filet des routes | `RoutePerimeterTests` | chaque page routable est confrontée à l'accès qu'elle doit porter |

---

## Ce que la spec tranche, et qu'on ne rouvre pas

| Question | Réponse du doc `06` |
|---|---|
| Les écrans | 7 routes : `login`, `forgot`, `sent`, `reset`, `reset-done`, `ob` (6 étapes), `ob-sent` |
| La marque | **La connexion est thémée client.** Un adhérent de Team Trainer's ne voit jamais du bleu GymXYZ. La demande d'ouverture reste **chez GymXYZ**, toujours |
| Le panneau de gauche | Dégradé par marque, trame de points, halo — tokens `--gx-a-*` au pixel (§1) |
| Leyssa | Panneau **clair**, donc la marque passe en variante fond clair. Ne jamais coder « panneau = sombre » |
| Mot de passe oublié | **Réponse identique que le compte existe ou non** — pas d'énumération de comptes |
| Réinitialisation | Les autres appareils sont déconnectés (donc : invalider le *security stamp*) |
| La demande | Crée une **Demande**, pas un compte. 6 étapes, entité `DemandeOuverture` (§6) |
| Le choix de l'étape 1 | Salle vs coach indépendant — **il pilote tout le reste du formulaire** (libellés, adresse vs zone, listes de tailles) |
| Les consentements | Deux obligatoires, un facultatif. « Envoyer » désactivé tant que les deux premiers ne sont pas cochés |
| La purge | Suppression **3 mois après un refus** — c'est écrit au consentement, donc c'est une promesse, pas une option de rétention |

---

## Le point d'architecture

**La demande d'ouverture est hors tenant.** Ni `TenantId`, ni filtre global.
C'est le **seul** endroit du produit dans ce cas, et tout le socle du lot 0 est
bâti sur l'hypothèse inverse.

À traiter explicitement — pas en laissant le filtre global « ne rien trouver ».
Le lot rôles a posé le patron applicable : `IManagerOnly` est un marqueur
**greppable**, épinglé par un test qui force chaque nouvelle requête à dire de
quel côté de la ligne elle est. La console (point 5) prévoit la même chose sous
le nom `IPlatformQuery`. Ce lot introduit le premier cas, et le nommer
maintenant évite que le second soit écrit autrement.

---

## Les décisions attendues avant de coder

**1. Le mot de passe de l'étape 3.** Il est saisi au formulaire alors que le
compte n'existe qu'à la validation. Deux implémentations :

- (a) stocker le hash sur la Demande et créer l'utilisateur à la validation ;
- (b) ne rien stocker et envoyer un **lien d'activation** à l'ouverture.

Le handoff recommande (b), et il a raison : rien à purger, aucun secret dormant,
et une demande refusée ne laisse pas un hash derrière elle. **Si (b) est retenu,
retirer le champ du formulaire** plutôt que de le laisser décoratif — un champ
qui ne sert à rien est exactement le genre de chose qu'on retrouve trois lots
plus tard en se demandant à quoi il sert.

**2. Le verrouillage après N tentatives.** Non tranché. Recommandation du
handoff : lockout ASP.NET Identity, 5 tentatives / 15 minutes. À confirmer,
parce que ça se voit à l'écran (§7 demande un message et un délai).

**3. Douze caractères ou huit ?** La spec se contredit elle-même : la note sous
le champ annonce 12, la jauge de robustesse compte à partir de 8 (`auStrength`,
§2). Trancher — recommandé : **12 partout** — et aligner la jauge *et* la
validation serveur, sinon l'écran promet une règle que le serveur n'applique pas.

**4. Les plafonds des formules.** L'onboarding annonce **80 / 600 membres**, la
console **150 / 600** (`07`, §…). Une seule source de vérité : la table `Plans`
de la plateforme, affichée ici. **Ne pas dupliquer les chiffres dans la copie
commerciale.**

**5. Le découpage.** Ce lot est gros : sept écrans, deux plateformes, trois
marques, une entité nouvelle, des endpoints publics, un job de purge. Ma
préférence, à valider : **deux PR**, dans cet ordre.

- **PR 1 — l'entrée** : les cinq écrans d'authentification (`login`, `forgot`,
  `sent`, `reset`, `reset-done`), thémés client, desktop et mobile, avec les
  jetons Identity et l'e-mail. Ça remplace un écran existant et en crée quatre,
  et ça se vérifie de bout en bout tout seul.
- **PR 2 — la demande d'ouverture** : le formulaire public en 6 étapes,
  l'entité hors tenant, l'anti-bot, la vérification de sous-domaine, la purge.

L'inverse ne marche pas : la PR 2 dépend du `AuthShell` que la PR 1 pose.

---

## Pièges déjà payés, et un nouveau

Ceux-là sont acquis, ne les repayez pas :

- **Avant de croire à un écran vide, lire `window.innerWidth`.** Le volet
  navigateur rapporte parfois **0** ; `ResponsiveModeService` lit cette largeur,
  un 0 passe sous le seuil mobile, et la grille bureau se rend vide. Forcer
  1440 × 900, puis recharger.
- **Le volet ne transmet aucun clic** sur les formulaires. Poster en JS ; pour la
  connexion, poser `.value` **sur les `fluent-text-field` eux-mêmes** puis
  déclencher un `change`. Un `.click()` en JS sur un bouton Blazor fonctionne.
- **Les comptes de démo** : `dwayne.johnson@gymxyz.fr`,
  `aurelie.siquier@teamtrainers.fr`, `najate.amzil@leyssa-coaching.fr`, et le
  coach `marine.debord@teamtrainers.fr`. Mot de passe `GymXyz!2026!`.
- **Le tenant vient des claims dès qu'on est authentifié** : changer de client =
  se reconnecter. Pour se déconnecter depuis le volet, poster un formulaire vers
  `/account/deconnexion`.
- **La base de dev est recréée au démarrage** et déconnecte tout le monde.
- **Un seul serveur peut tenir le port 5173.**

Et celui-ci est **nouveau, et il touche le cœur du lot** :

> **En développement, il n'y a pas de sous-domaine.** La règle structurante du
> handoff — la connexion porte la marque du client — repose sur
> `ResolveSlugFromHost()`, qui découpe `teamtrainers.gymxyz.fr`. Sur
> `localhost:5173` il n'y a rien à découper : le resolver retombe sur
> `Tenant:DefaultSlug`, c'est-à-dire `gymxyz`, **toujours**. Autrement dit
> l'écran de connexion s'affichera en bleu GymXYZ quoi qu'on fasse, et les deux
> autres habillages ne seront jamais vus.
>
> Ce n'est pas un bug, c'est une configuration : il faut poser
> `Tenant:RootDomain` à `localhost` en développement et ouvrir
> `http://teamtrainers.localhost:5173`. **À régler au début du lot**, pas à la
> vérification — c'est la seule façon de voir les trois marques, et donc le seul
> moyen de tenir le critère d'acceptation.

Deux autres à connaître avant de s'y casser les dents :

- **En dev, l'e-mail ne part pas** : `LoggingEmailSender` remplace Brevo tant
  qu'aucune clé n'est configurée. Le lien de réinitialisation n'existera donc que
  dans le log du serveur — prévoir de le lire là, et ne pas conclure à une panne.
- **Il n'y a toujours pas de migrations EF** : la base est recréée à chaque
  démarrage (`ResetDatabaseOnStartup`). Ajouter `DemandeOuverture` est donc
  gratuit aujourd'hui. Ça cesse de l'être au lot suivant, où la console
  provisionne de vrais clients — c'est l'**entrée 5** du registre de dette, et
  son échéance est exactement là.

---

## Ce que la maquette ne couvre pas, et qu'il faut quand même livrer

`06` §7 les liste, et la DoD du dépôt demande de toute façon les états vide,
chargement et erreur :

| État | Où | Attendu |
|---|---|---|
| Identifiants invalides | `login` | message générique sous le formulaire — **jamais** lequel des deux est faux |
| Compte verrouillé | `login` | message + délai, selon la décision 2 |
| Espace suspendu (impayé) | `login` | message dédié |
| Lien expiré ou déjà utilisé | `reset` | « Ce lien n'est plus valable. » + renvoi |
| Validation par étape | `ob` | erreurs sous les champs, « Continuer » bloqué |
| Sous-domaine déjà pris | `ob` étape 5 | message en `--color-danger` + suggestion |
| Envoi en cours | `ob` étape 6 | bouton en chargement, **double-envoi impossible** |
| Échec d'envoi | `ob` | conserver la saisie, proposer de réessayer |

Et deux protections que le prototype ne peut pas montrer : **anti-bot** sur un
formulaire public (honeypot + limitation par IP) et **anti-spam sur le renvoi**
du lien (1 envoi / 60 s).

---

## Critère d'acceptation

- **Les trois marques sur l'écran de connexion**, réellement vues — pas déduites.
  Ce qui suppose le sous-domaine local réglé d'abord.
- **Leyssa en panneau clair**, avec la variante de marque qui va avec.
- **Aucune énumération de comptes** : « mot de passe oublié » répond pareil pour
  une adresse connue et une inconnue, et le message d'identifiants invalides ne
  dit jamais lequel des deux champs est faux.
- **Le parcours de réinitialisation marche de bout en bout**, lien compris — en
  dev, lu dans le log.
- **Les six étapes se remplissent et la demande arrive en base**, avec sa
  référence `DEM-2026-NNNN`.
- **La demande vit hors tenant**, et le contournement du filtre global est
  **nommé**, pas improvisé au fil de l'eau.
- **La purge à 3 mois existe** et n'est pas un TODO : elle est promise à
  l'utilisateur dans le texte du consentement.
- `RoutePerimeterTests` **connaît les nouvelles routes anonymes** — c'est le
  filet qui empêche une page publique d'hériter silencieusement d'un
  `[Authorize]`, ou l'inverse.
- **Desktop et mobile**, les deux, sur les sept écrans.
- `dotnet test` vert et **zéro warning** dans les projets applicatifs.

---

## Ce qui n'est pas dans ce lot

- **Le traitement de la demande** — valider, refuser, demander un complément,
  notes internes. C'est la console, point 5, doc `07`.
- **Le provisionnement d'un client** (« Valider et ouvrir l'espace » : création du
  `Tenant`, du premier compte, du seed, de l'invitation). Console également.
- **La décision sur l'impersonation**, qui engage du code déjà livré. Console.
- **L'écran de génération de l'affiche** (`08`, §6) : toujours pas maquetté. Il
  n'appartient pas à ce lot, mais c'est le seul aller-retour design encore
  ouvert sur du code livré — le bouton « Aperçu » de l'Accueil l'attend, désactivé
  avec sa raison.
- **Les comptes à casquettes multiples** (entrée 4 du registre), point 6,
  délibérément après les points 4 et 5.
- **Les migrations EF** (entrée 5), en dernier, juste avant un déploiement.

> **Conséquence à assumer et à dire** : à la fin de ce lot, une demande
> d'ouverture peut être déposée et **rien ne peut la traiter**. Les demandes
> s'empilent jusqu'à la console. C'est le bon ordre — le formulaire est la
> matière dont la console a besoin pour être conçue — mais il ne faut pas livrer
> à l'utilisateur un accusé de réception qui promette un délai que personne ne
> peut encore tenir. Relire le texte de `ob-sent` sous cet angle.

---

## Premier geste suggéré

1. **Ouvrir `design/GymXYZ Auth & Onboarding.html` dans le navigateur.** C'est la
   cible, elle est au pixel, et elle vaut tous les résumés — y compris celui-ci.
2. **Régler le sous-domaine local** (`Tenant:RootDomain` → `localhost`) et
   vérifier que `http://teamtrainers.localhost:5173` sert bien la marque Team
   Trainer's. Sans ça, rien de ce lot n'est vérifiable.
3. **Trancher les quatre décisions** : mot de passe (a) ou (b), verrouillage,
   12 ou 8 caractères, source des plafonds de formules.
4. **Trancher le découpage** : deux PR, l'entrée puis la demande.
5. **Constater l'ampleur réelle** : quatre écrans à créer, pas à remplacer.

Revenir avec un plan qui dit **le découpage retenu**, **ce que devient le mot de
passe de l'étape 3**, **comment le hors-tenant est nommé**, et **comment les
trois marques seront vues en développement**. **Attendre la validation avant
d'écrire.**
