# Lot 13 — Dette technique & finitions · registre courant

Ouvert le 2026-08-06, à la fin du lot 9. `main` est à `c64245d` (lot 9 PR 1),
la PR 2 est en attente de fusion, **465 tests au vert**.

Ce document n'est pas un brief de démarrage comme les autres : c'est un
**registre qui se remplit au fil des lots**. Chaque lot découvre des choses
qu'il aurait été malhonnête de corriger en passant — parce qu'elles dépassent
son périmètre, parce qu'elles touchent toute l'application, ou parce qu'elles
demandent une décision produit. Elles s'écrivent ici au moment où on les voit,
avec ce qu'on savait à ce moment-là.

---

## Comment s'en servir

**À l'écriture d'une entrée** (pendant un lot, jamais après coup) :

- Ce qu'on a observé, et **où** — le fichier, l'écran, la commande qui le montre.
- Pourquoi ça n'a pas été corrigé sur place.
- La piste, avec un chiffrage.
- Le risque de la corriger, pas seulement celui de la laisser.

**Ce qui n'entre pas ici.** Le registre n'est pas un tiroir. N'y vont ni les
« ce serait mieux si », ni les préférences de style, ni ce qu'un lot à venir
couvre déjà. Une entrée décrit quelque chose qui **est faux ou dangereux
aujourd'hui**, avec de quoi le vérifier.

**Ce n'est pas une file d'attente.** Une entrée porte une échéance. « Avant le
lot 10 » veut dire avant le lot 10 : une dette qui produit déjà du bruit ne se
range pas en fin de programme sous prétexte qu'elle est ancienne.

---

## Entrées ouvertes

### 3. `Connection must be valid and open` sur la requête de marque

**Ouverte le 2026-08-06**, en corrigeant l'entrée 1 — dont elle était un
morceau, à tort. **Échéance : aucune tant qu'elle reste invisible** ; c'est du
bruit de log, pas une panne.

**Observé.** `System.InvalidOperationException: Connection must be valid and
open`, levée dans `MySql.Data`, toujours sur la **même** requête : celle de
`GetTenantBrandQuery` (`SELECT ... FROM Tenants WHERE Slug = 'gymxyz'`). Aucune
autre requête ne tombe.

**Ce qui a été mesuré, et qui la sépare de l'entrée 1.** La fabrique de contexte
ne la change pas :

| Build | `A second operation was started` | `Connection must be valid and open` |
|---|---|---|
| `main` sans le correctif | absent sous cette charge | **présent** |
| avec la fabrique | absent | **présent, à l'identique** |

Même charge de part et d'autre : connexion, puis 32 chargements de page lus
jusqu'au bout sur les huit écrans, plus des navigations réelles (circuit
interactif) sur Planning, Présences, Membres et Administration. Le symptôme est
donc **antérieur et indépendant** du contexte partagé : il n'était pas causé par
ce que l'entrée 1 décrivait.

**Pourquoi ça ne casse rien aujourd'hui.** Les pages s'affichent complètes et
correctement habillées, et le log fait suivre l'échec d'un `Executed DbCommand`
réussi sur la même requête. Rien à l'écran ne le laisse voir — c'est pour ça
qu'il a pu vivre plusieurs lots dans le log sans que personne le remarque
autrement qu'en le lisant.

**Ce qui n'est pas su.** La cause. Deux pistes non départagées : le connecteur
`MySql.Data` qui rend une connexion du pool pas encore ouverte, et la double
passe de `TenantBoundary.OnInitializedAsync` (prérendu puis circuit). Ne pas
la refermer sans avoir tranché : une entrée qui dit « corrigé » sans savoir
pourquoi ne vaut rien à la personne qui reverra le message.

---

## Entrées fermées

### 2. Un `PlatformAdmin` hors impersonation lit les données de GymXYZ

> **Fermée le 2026-08-07**, dans le lot 11 et donc à l'échéance annoncée —
> corrigée en **A + B**, C écarté pour la raison donnée plus bas. La décision
> produit a été prise au moment du plan du lot, avant d'écrire du code, comme
> l'entrée le demandait.
>
> **Ce qui a été fait.** `TenantResolver` ne replie plus sur l'hôte pour un
> principal authentifié sans claim de tenant : le tenant ambiant reste 0 et le
> filtre global ne laisse rien passer. La navigation métier disparaît — Réglages
> compris, ce sont les réglages du client — et il ne reste qu'Administration.
> L'URL directe répond pour elle-même via `CustomerScope`, qui affiche
> « Aucun client sélectionné » plutôt qu'un état vide mensonger. La console porte
> désormais la marque `ConsoleBrand` (TechXYZ, tenant 0) au lieu d'emprunter
> celle de GymXYZ.
>
> **Ce qui a été vérifié**, l'entrée demandant de prouver qu'aucun autre rôle ne
> perd l'accès : l'admin sans client ne voit plus aucune donnée et garde sa
> console ; l'admin entré chez Leyssa voit les six membres, le bandeau, et la
> ligne `TenantImpersonation` est ouverte — c'est la **ligne 1**, tout ce qui
> précède l'entrée n'ayant rien lu ; le gérant GymXYZ voit ses 36 membres ; le
> coach voit tout son écran ; l'anonyme garde le repli par hôte, seul cas pour
> lequel il avait été écrit.
>
> **Ce que la fermeture ne couvre pas.** Le repli par hôte reste le chemin des
> sous-domaines clients en production ; il n'a pas été exercé ailleurs qu'en
> test, faute d'un DNS de développement.

**Échéance proposée : avant le lot 11** (marques clientes), qui multiplie les
clients et donc l'exposition. **Demande une décision produit avant de coder.**

**Observé, et c'est plus qu'un défaut d'affichage.** Connecté en
`admin@techxyz.fr`, **sans être entré chez aucun client**, `/members` affiche
les 36 membres de GymXYZ avec leurs adresses e-mail. Vérifié le 2026-08-06.

Conséquences, dans l'ordre de gravité :

1. **La trace du lot 9 a un trou.** Aucune ligne `TenantImpersonation` n'est
   ouverte, puisque l'admin n'est pas passé par `/account/client`. Des données
   client sont lues sans que rien ne l'enregistre — ce que cette entité existe
   précisément pour empêcher.
2. **Aucun bandeau**, puisqu'`IsImpersonating` est faux. L'admin n'a rien à
   l'écran qui lui dise chez qui il est.
3. GymXYZ est choisi par défaut, pas par lui.

Ce n'est pas une élévation de privilège — un `PlatformAdmin` a le droit de voir
n'importe quel client. C'est que ce droit s'exerce **en silence**.

**Pourquoi.** `GymUserClaimsPrincipalFactory` n'écrit aucun claim de tenant pour
un `PlatformAdmin` — délibérément, commentaire du lot 0 à l'appui.
`TenantResolver.SlugFromClaims` renvoie donc null, on retombe sur
`ResolveSlugFromHost()`, qui renvoie `TenantOptions.DefaultSlug` — `gymxyz` en
développement. Le repli existe depuis le lot 0 pour habiller l'écran de
connexion avant authentification ; il n'a jamais été pensé pour un compte
authentifié qui n'appartient à personne.

**La décision à prendre**, parce qu'elle change ce qu'on code :

| Option | Ce que ça donne |
|---|---|
| **A — Un admin sans client ne voit aucune donnée métier** | `TenantResolver` ne replie plus sur l'hôte pour un principal authentifié sans tenant. Le filtre global ne laisse alors rien passer (tenant 0), et les écrans métier s'affichent vides. Il faut décider ce qu'ils disent : « Entrez chez un client depuis la console » plutôt qu'un état vide trompeur. |
| **B — La navigation métier disparaît pour lui** | La barre latérale ne montre que l'Administration tant qu'aucun client n'est choisi. Plus lisible, mais l'URL directe reste à traiter — donc A quand même, en dessous. |
| **C — Entrer chez un client devient obligatoire** | La console redirige vers `/account/client` dès qu'un écran métier est demandé. Le plus strict, et le seul qui garantisse une ligne de trace pour toute lecture. |

Penchant à défendre au moment du plan : **A + B**, avec C écarté pour une
raison précise — obliger l'impersonation pour lire une page ferait ouvrir une
ligne de trace à chaque coup d'œil, et une trace qui enregistre tout
n'enregistre plus rien.

**Taille.** Petit en code (une condition dans `TenantResolver`, un filtre de
navigation), moyen en vérification : c'est un changement d'autorisation de fait,
et il faut prouver qu'aucun autre rôle ne perd l'accès au passage.

---

### 1. Un seul `DbContext` par circuit, partagé par tout l'écran

**Fermée le 2026-08-06**, avant le lot 10 comme prévu — corrigée dans
`TechXyz.GymXyz.Persistence/Extensions/IServiceCollectionExtensions.cs` :
`IGymDbContext` est enregistré `Transient` depuis un
`IDbContextFactory<GymDbContext>` lui-même `Scoped`. Les 77 fichiers qui le
prennent n'ont pas changé, comme l'entrée l'annonçait.

**Ce qui a été vérifié.** 465 tests au vert, puis passage navigateur sur les
quatre écrans les plus bavards en circuit interactif : plus aucun
`A second operation was started on this context instance`, et la console charge
ses trois clients sans le toast rouge.

**Ce que la fermeture ne couvre pas.** L'entrée annonçait « la disparition des
**deux** messages » comme critère. Un seul a disparu : `Connection must be valid
and open` se reproduit à l'identique sur `main` sans le correctif, donc il ne
venait pas de là. Il part vivre sa propre vie en **entrée 3**.

**Reste ouvert par choix.** Un contexte servi `Transient` est tenu par le
conteneur jusqu'à la fin du scope, et le scope d'un circuit Blazor dure ce que
dure l'onglet : les instances s'accumulent au fil d'une longue session. Elles
sont petites et ne retiennent pas de connexion — les connexions retournent au
pool après chaque requête — mais ce n'est pas nul. Si ça devient mesurable, le
remède est connu et plus gros : les handlers prennent la fabrique et disposent
eux-mêmes leur contexte.

---

<!-- Ancienne description de l'entrée 1, gardée telle qu'écrite : -->

**Échéance proposée : avant le lot 10.** C'est la seule entrée urgente du
registre.

**Observé.** `Connection must be valid and open` et
`A second operation was started on this context instance` dans le log de
développement, sur plusieurs écrans. Vérifié au lot 9 PR 1 sur `/members` avec
le gérant GymXYZ — donc sur un chemin qu'aucun lot récent n'a touché : c'est
antérieur, pas une régression.

Le lot 9 PR 2 l'a rendu **fatal** pour la première fois : la console tire cinq
requêtes par chargement et perdait la course assez souvent pour finir sur un
toast rouge « Impossible de charger les clients ». Contourné en donnant à cet
écran son propre scope DI (`Administration.razor`, `InConsoleScopeAsync`) — un
contournement local et assumé, pas le remède.

**Pourquoi ça arrive.** `GymDbContext` est enregistré `Scoped`, donc partagé par
tous les composants d'un circuit Blazor. EF interdit deux opérations
simultanées sur une instance. `TenantResolver` porte déjà un `SemaphoreSlim`
pour exactement ça, avec le commentaire qui le dit : « la mise en page et la
frontière demandent toutes deux pendant la même passe de rendu ».

**La piste, et sa vraie taille.** Elle est plus petite qu'elle n'en a l'air :

- **Les 77 fichiers de `Application` qui prennent `IGymDbContext` ne changent
  pas.** Seul l'enregistrement change : `IGymDbContext` servi depuis un
  `IDbContextFactory<GymDbContext>`, donc une instance par handler.
- **`GymDbContext` reste `Scoped`** : `AddEntityFrameworkStores<GymDbContext>()`
  en a besoin, et `DbInitializer` le prend directement.
- Le filtre global lit `ITenantContext` **à travers l'instance de contexte**, et
  `ITenantContext` reste `Scoped` : chaque nouveau contexte lit donc toujours le
  tenant ambiant du circuit. À vérifier, c'est le point qui casserait tout.

**Ce qui rend ça sûr — vérifié, pas supposé.** Le changement fait perdre le
suivi d'entités partagé entre deux handlers d'une même requête. Deux recherches
disent que personne n'en dépend :

- aucun handler n'envoie de requête MediatR imbriquée (`ISender` / `IMediator`
  n'apparaît dans aucun `*.Handler.cs`) ;
- aucune transaction explicite (`BeginTransaction`) nulle part.

Autrement dit, **un handler est déjà une unité de travail**. C'est ce qui fait
la différence entre un changement d'enregistrement et une réécriture.

**Risque à ne pas sous-estimer.** Le rayon d'action est l'application entière,
et les symptômes actuels sont intermittents — donc « ça marche chez moi » ne
prouve rien. Ce lot demande la suite complète **plus** un passage navigateur sur
les écrans les plus bavards (Planning, Présences, Membres, Administration), et
la disparition des deux messages du log est le critère d'acceptation, pas
l'absence de plainte.

**À faire au passage** : retirer `InConsoleScopeAsync` d'`Administration.razor`
une fois la fabrique en place — sauf pour sa **première** raison d'être, qui
survit au correctif : la console est servie comme aucun client, et un scope
neuf ne résout aucun tenant. Garder le scope, réécrire le commentaire pour
qu'il ne parle plus d'une course qui n'existera plus.

---

## Ce que ce registre ne remplace pas

`01-LOTS.md` reste la source des lots métier et de leurs décisions attendues.
Ce document est l'inverse : ce que la **construction** a appris, que le hand-off
ne pouvait pas prévoir. Les deux se lisent avant de planifier un lot.
