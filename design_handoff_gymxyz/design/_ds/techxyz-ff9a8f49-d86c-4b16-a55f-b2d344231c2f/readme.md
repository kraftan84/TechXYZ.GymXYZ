# TechXYZ — Design System

> **TechXYZ · Creative Engineering**
> Logiciels sur-mesure pour les municipalités, associations et PME.

TechXYZ is an (in-formation) software studio building practical, trustworthy
applications for **local governments, non-profit associations, and small/mid
businesses** — primarily a French-speaking audience. This design system powers
two surfaces:

1. **The marketing site (vitrine)** — likely a single, dense page that explains
   what TechXYZ does and converts a visit into a conversation.
2. **The default look-and-feel of the apps TechXYZ ships** — admin panels,
   citizen/member portals, dashboards, forms. The system gives every product a
   shared, credible baseline so a one-person studio looks like a team.

The brand idea is **"Creative Engineering"**: the rigor and reliability that
public-sector and SME software demands, expressed with a spark of creativity.
The logo — a lightbulb drawn as a **network of connected nodes** — is the whole
thesis in one mark: ideas (the bulb) built from connected, engineered parts (the
graph).

---

## Sources provided

| Source | What it is | Notes |
|---|---|---|
| `uploads/TechXYZ 2026.png` | Primary logo lockup | Node-graph lightbulb + `TECH`/`XYZ` wordmark + `CREATIVE ENGINEERING` tagline. Colors sampled directly from it. Construction: **TECH** = Orbitron **Bold (700)**, **XYZ** = Orbitron **Medium (500)** superscript in azure, **tagline** = Montserrat **Light (300)**, uppercase, wide tracking (~0.26em). |
| `uploads/Orbitron-VariableFont_wght.ttf` | Display font | Official variable Orbitron. |
| `uploads/Montserrat-VariableFont_wght.ttf` + Italic | Text font | Official variable Montserrat. |

No codebase, Figma file, or existing product screens were provided. The product
UI kits in this system are therefore **conventions proposed by this design
system**, not recreations of an existing app — they establish the *default*
TechXYZ look for future products. When real product code or designs arrive,
treat them as the source of truth and reconcile.

> **Self-hosted fonts:** the official TTFs were supplied. For web performance the
> system ships the matching **woff2 latin** subsets (official Google Fonts
> production binaries, identical glyphs, full French-accent coverage) in
> `assets/fonts/`. Swap in the full TTFs if you need extended scripts.

## Production stack — .NET / Blazor + Fluent UI Blazor

The real products and site will be built in **.NET with Blazor**, using the
**[Fluent UI Blazor](https://www.fluentui-blazor.net/)** component library.
This design system stays the **brand source of truth**; Fluent UI Blazor is the
**production component layer**. They meet through theming — don't fork Fluent's
components, *re-skin* them with these tokens:

- **Accent color → azure.** Fluent themes off a single accent ramp. Set it with
  `<FluentDesignTheme>` (`CustomColor="#00ABFC"` / `AccentBaseColor`), and the
  `--accent-fill-*` design tokens cascade. Use `--azure-600` (`#0089CE`) for
  solid accent so white text passes AA, and the spark `#00ABFC` for highlights.
- **Neutrals → slate.** Map Fluent's neutral layers onto our `--neutral-*`
  ramp (`--neutral-fill-layer`, `--neutral-stroke-*`) so surfaces read cool, not
  the default Fluent gray.
- **Type.** Override Fluent's default Segoe UI: set `--body-font` /
  `--font-family` to **Montserrat**, keep **Orbitron** for the wordmark/headings
  via `.font-display`. Link `styles.css` (it loads both webfonts) in
  `index.html` / `App.razor` and you get the tokens too.
- **Shape.** Fluent's `--control-corner-radius` / `--layer-corner-radius` →
  our 10px (controls) / 14px (cards). Control height baseline 44px (`--control-md`).
- **Density & a11y.** Fluent already targets WCAG; keep the visible azure focus
  ring (`--ring-brand`) — important for RGAA/public-sector compliance.

**How to use this repo with Blazor:** treat the React components here as
**prototyping / mock fidelity only** — use them (and the UI kits) to design and
validate screens fast, then implement the approved design with Fluent UI Blazor
components themed as above. The tokens, colors, type, voice, logo and iconography
rules transfer 1:1. (A future addition could be a `theme/fluent.css` that maps
these tokens onto Fluent's design-token variables — ask if you want it scaffolded.)

---

## Brand foundations at a glance

- **Azure spark** `#00ABFC` — the energetic brand blue (sampled from the logo nodes).
- **Ink navy** `#0C2236` — the serious, structural brand dark (the wordmark color).
- **Cool slate neutrals** — everything is faintly blue-cool, never warm gray.
- **Orbitron** for display & the wordmark; **Montserrat** for everything you read.
- Geometric, lightly-rounded shapes; soft navy-tinted shadows; a restrained,
  confident use of the spark as accent and glow.

---

## CONTENT FUNDAMENTALS — how TechXYZ writes

The audience is busy public-sector and SME decision-makers who are wary of
"tech bro" hype. Copy earns trust by being **clear, concrete, and respectful**.

**Voice:** competent, warm, plain-spoken. A skilled engineer who explains things
without condescension. Confident but never boastful.

**Language:** **French-first** (the primary market is French municipalities,
associations and PME). English is secondary. Write real French — not translated
English. Use the **formal "vous"** with prospects and public-sector clients;
the studio refers to itself as **"nous"** (or "je" for the founder's personal
notes), never an anonymous "the platform".

**Tone rules**
- **Concrete over abstract.** "Gérez les inscriptions de votre association en
  ligne" beats "Optimisez vos workflows associatifs". Name the real task.
- **Benefit, then mechanism.** Lead with what the client gets ("Moins de
  paperasse"), then how ("un formulaire en ligne relié à votre tableur").
- **No hype words.** Avoid *révolutionnaire, disruptif, magique, leader,
  next-gen, propulsé par l'IA* unless literally true and useful.
- **Short sentences.** One idea each. Public-sector readers skim.
- **Honest about scope.** A small studio's superpower is candor — say what's
  included, what isn't, and what it costs to find out.

**Casing**
- **Wordmark / big display:** UPPERCASE, Orbitron — `TECH` Bold 700, `XYZ` Medium 500.
- **Tagline:** UPPERCASE Montserrat **Light 300**, very wide tracking (`CREATIVE ENGINEERING`).
- **Eyebrows / overlines:** UPPERCASE Montserrat **Semibold 600**, wide tracking (`NOS SERVICES`).
- **Headings & UI:** sentence case in French ("Prenons rendez-vous"), Title
  Case only for proper product names.
- **Buttons:** sentence-case verb phrases — "Demander un devis", "Voir nos
  réalisations". Avoid shouting.

**Emoji:** not used in the brand voice or UI. The visual energy comes from the
azure spark and the node motif, not emoji. (Icons, yes — see ICONOGRAPHY.)

**Numbers & units:** French formatting — space as thousands separator
("1 200 dossiers"), comma decimal, "€" after the amount with a thin space
("490 € / mois"), 24-hour time.

**Micro-copy examples**
- Eyebrow: `CRÉATIVE ENGINEERING` → section eyebrows like `NOS SERVICES`, `À PROPOS`.
- Hero H1: *"Des logiciels sur-mesure pour les collectivités, les associations et les PME."*
- Sub: *"Nous concevons et développons des outils simples, fiables et adaptés à votre réalité de terrain."*
- CTA primary: *"Parlons de votre projet"* · secondary: *"Voir nos réalisations"*
- Empty state: *"Aucun dossier pour l'instant. Créez le premier pour commencer."*
- Error: *"Ce champ est requis."* (plain, no blame, no humor)

---

## VISUAL FOUNDATIONS

**Color vibe.** Cool and technical, never warm. Two brand colors carry the
identity: **azure `#00ABFC`** (energy, the "idea") and **ink navy `#0C2236`**
(structure, trust). Neutrals are deliberately **cool slate** (a hint of blue),
so the whole system reads as one cool family. Azure is used as an *accent and a
spark*, not as large flat fills of pure `#00ABFC` (too vibrant to sit under
white text) — solid actions use the slightly deeper `--azure-600`. Backgrounds
are mostly white/very-light-slate; **ink navy is the dramatic alternate
ground** for heroes and footers, where the azure spark glows against it.

**Typography.** A two-font system. **Orbitron** (geometric, wide, technical) is
the *display & identity* voice — the wordmark and the occasional big statement.
It is used **sparingly and large**; never for body (it's hard to read in long
runs). The wordmark itself is **TECH in Orbitron Bold (700)** with **XYZ in
Orbitron Medium (500)**. **Montserrat** is the *workhorse* — all body, UI,
labels, buttons, tables. Headings are Montserrat **bold** by default; Orbitron
is opt-in via `.font-display` for moments that should feel like the brand. The
**tagline** (`CREATIVE ENGINEERING`) is **Montserrat Light (300)**, uppercase,
with very wide tracking — reuse it via the `.tagline` helper. **Eyebrows /
overlines** are uppercase **Montserrat Semibold (600)** with wide tracking (the
`.eyebrow` helper); they are *not* Orbitron.

**Spacing & layout.** 4px base grid. Generous vertical rhythm
(`--section-y: 96px`) on marketing; tighter, data-dense rhythm in apps.
Centered max-width containers (`--container-xl: 1240px`) with comfortable
gutters. Layouts are **structured and gridded** — the node motif rewards
alignment. Sticky top app bar; content never edge-to-edge text (line length
stays readable).

**Backgrounds.** Three registers:
1. **Light** — `--surface-page` (#F5F8FA) or white. The default.
2. **Ink** — deep navy (`--gradient-ink` / `--gradient-mesh`) for heroes,
   footers, and feature spotlights. A faint azure radial "mesh" glow sits in
   one corner, evoking the node graph without being literal.
3. **Brand-soft** — `--azure-50` panels to highlight a callout without going dark.
No photographic textures or noise by default; no busy patterns. The optional
decorative element is the **node-graph motif** (sparse connected dots) used at
low opacity — never loud.

**Borders.** Hairline `1px` cool-slate borders define cards, inputs, and table
rows (`--border-subtle` / `--border-default`). Brand or focus states switch the
border to azure. Borders do the quiet structural work; shadows are reserved for
elevation.

**Corner radii.** Lightly rounded, consistent: inputs/buttons `10px`
(`--radius-md`), cards `14px` (`--radius-lg`), panels/modals `20px`. Pills
(`999px`) for tags, badges, and avatars. Nothing sharp-cornered, nothing
bubble-round — engineered, not playful.

**Shadows (elevation).** Soft and **navy-tinted**, never neutral-gray or black.
Two-layer shadows (`--shadow-md`, `--shadow-lg`) for cards and popovers. A
special **brand glow** (`--shadow-brand`, `--glow-spark`) puts an azure halo on
the primary CTA and active/selected states — this is the literal "spark." Inset
shadow (`--shadow-inset`) for sunken/pressed fields.

**Cards.** White surface, `14px` radius, `1px --border-subtle`, `--shadow-sm` at
rest rising to `--shadow-md` on hover, with a 1–2px lift
(`translateY(-2px)`). Optional azure top-accent or left node-dot for featured
cards. Interior padding `24–32px`.

**Hover states.** Buttons darken one step (primary → `--azure-700`); links go
darker azure; cards lift + deepen shadow; ghost/secondary items get a faint
`--surface-sunken` wash. Transitions are quick (`--duration-fast` 140ms) on the
`--ease-standard` curve.

**Press / active states.** Solid buttons darken another step
(`--azure-800`) and **scale to 0.98** for a tactile "click"; inputs show the
inset shadow. No long bouncy animations on press — it should feel instant.

**Focus.** Always visible: a `3px` azure ring (`--ring-brand`,
`rgba(0,171,252,.35)`) — keyboard accessibility is non-negotiable for
public-sector clients (RGAA/WCAG).

**Transparency & blur.** Used judiciously: the sticky top bar may use a glass
effect (`--blur-glass`) over scrolled content; overlays/scrims use
`rgba(12,34,54,.5)` (navy, not pure black). No frosted-glass everywhere — it's a
focus tool, not a texture.

**Motion.** Purposeful and calm. Entrance fades + small upward translate (8–16px)
on the `--ease-out` curve; durations 140–360ms. Hover/press use `--duration-fast`.
**No infinite decorative loops**, no parallax circus. The node motif may animate
once on load (dots connecting) but then rests. Always respect
`prefers-reduced-motion`.

**Imagery.** When photography is used it should be **real, candid, French
municipal/SME contexts** (a town hall desk, an association event, a small-team
office) — warm subjects, but color-graded slightly **cool** to sit with the
palette. Avoid generic glossy stock and avoid literal "circuit board" clichés.
Screenshots of the products themselves are the preferred "imagery."

---

## ICONOGRAPHY

TechXYZ has **no custom icon font**. The system standardizes on **[Lucide](https://lucide.dev)**
— an open-source line-icon set whose **geometric, even ~2px stroke, rounded
joins, and `24×24` grid** are a near-perfect match for Montserrat's humanist-
geometric shapes and Orbitron's clean strokes. It's MIT-licensed (safe for
client products) and available via CDN.

> **Substitution flag:** no icon assets were provided with the brand, so Lucide
> is a *chosen default*, not a recreation. If TechXYZ later adopts a different
> set, document it here and swap the CDN reference.

**Usage**
- **Stroke style only** (line icons), never filled, to match the line-drawn
  node logo. Default stroke `~1.75–2px`, color inherits `currentColor`.
- **Sizes:** `16` (inline/meta), `20` (buttons/inputs), `24` (nav/features).
  Keep stroke optically consistent across sizes.
- **Color:** icons are `--text-muted` at rest, `--color-brand` when active /
  on brand surfaces, `--text-on-brand` (white) on ink.
- **Don't:** mix icon families, use duotone/filled glyphs, or use emoji as
  icons. Emoji are not part of the brand.

**The node motif as iconography.** The lightbulb-as-graph is the brand's signature
glyph. Use the real logo mark (`assets/logo/techxyz-mark.png`) for the bulb;
the sparse "connected dots" pattern may be reused decoratively (low opacity) but
should not be redrawn as new literal illustrations.

**CDN**
```html
<script src="https://unpkg.com/lucide@latest"></script>
<script>lucide.createIcons();</script>
<!-- <i data-lucide="building-2"></i>  <i data-lucide="arrow-right"></i> -->
```
Good default icons for this audience: `building-2` (mairie), `users`
(associations), `briefcase` (PME), `file-text`, `calendar-days`, `shield-check`,
`message-circle`, `arrow-right`, `check`, `sparkles`.

---

## INDEX — what's in this system

**Root**
- `styles.css` — the single entry point consumers link (imports everything below).
- `readme.md` — this guide.
- `SKILL.md` — Agent-Skill front-matter so this folder works as a Claude skill.

**Tokens** (`tokens/`, all `@import`ed by `styles.css`)
- `fonts.css` — `@font-face` for Orbitron + Montserrat (self-hosted woff2).
- `colors.css` — azure / ink / slate ramps + semantic aliases.
- `typography.css` — families, scale, weights, tracking, role aliases.
- `spacing.css` — 4px scale, containers, control sizes, z-index.
- `effects.css` — radii, shadows, motion, gradients, blur.
- `base.css` — light global resets/defaults + `.eyebrow` / `.tagline` / `.font-display` helpers.

**Foundations** (`guidelines/` — specimen cards in the Design System tab)
- Color, type, spacing, shadow, radius and brand specimen cards.

**Components** (`components/` — reusable React primitives, namespace
`window.TechXYZDesignSystem_ff9a8f`; each has `<Name>.d.ts` + `<Name>.prompt.md`
+ a directory card HTML)
- `buttons/` — **Button** (primary / secondary / outline / ghost / danger), **IconButton**.
- `forms/` — **Input**, **Select**, **Switch**, **Checkbox**.
- `display/` — **Badge**, **Avatar**, **Card**.
- Starting points: `Button` and `Card` (section "Core").

**UI kits** (`ui_kits/`)
- `vitrine/` — marketing-site default look (NavBar, Hero, Audiences, Services,
  Process, ContactCTA, Footer). Starting point + `Vitrine` card.
- `app/` — back-office default look (Sidebar, Topbar, LoginScreen, Dashboard,
  RequestsList, RequestDetail). Starting point + `Application` card.

**Guidelines** (`guidelines/`) — specimen cards: colors (brand/azure/ink/
neutrals/semantic), type (display/body/scale/eyebrow), spacing (scale/radii/
shadows), brand (logo on light & ink, voice do/don't, iconography).

**Assets** (`assets/`)
- `logo/` — lockups & marks:
  - `techxyz-logo-full.png` — full color, light bg.
  - `techxyz-logo-white.png` — full white-knockout (azure nodes kept), **dark bg**, transparent.
  - `techxyz-logo-mono-black.png` — **1-color pure black**, transparent (B&W print, stamps, fax).
  - `techxyz-logo-mono-white.png` — **1-color pure white**, transparent (single-color on dark/photo).
  - `techxyz-mark.png` — node mark, navy base (light bg) · `techxyz-mark-dark.png` — node mark, white base (dark bg) · `techxyz-mark-512.png` — square / favicon.
  - Rule: full color when possible; white-knockout on brand ink; the 1-color
    variants only when color can't be used. Navy-base mark on light, white-base on ink.
- `favicon/` — generated favicon set from the bulb mark on an ink rounded square:
  `favicon-16/32/48/192/512.png`, `apple-touch-icon.png` (180), `maskable-512.png`
  (PWA safe-area), transparent `mark-32/512.png`, and `site.webmanifest`
  (theme `#0C2236`). Wire-up snippet is in the kits' `index.html` `<head>`.
- `fonts/` — self-hosted woff2 (Orbitron + Montserrat roman & italic).

**SKILL.md** (root) — Agent-Skill front-matter for use as a downloadable Claude skill.
