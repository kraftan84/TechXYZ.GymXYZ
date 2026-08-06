# Self-hosted webfonts

Every font the product uses is served from this folder. **No stylesheet may
reach out to a font CDN** — an `@import` from `fonts.googleapis.com` sends each
visitor's IP address to Google before anything is displayed and without any
consent being asked, which is the GDPR problem the Munich court decision made
concrete. `NoExternalStylesheetImportsTests` enforces this.

All four families are latin subsets of the official Google Fonts production
files, with the accent coverage French needs (`unicode-range` in
`../../tokens/fonts.css`).

| File | Family | Version | Weights | Declared in |
|---|---|---|---|---|
| `Montserrat-latin.woff2` | Montserrat | lot 0 | 100–900 variable | `--font-sans`, body/UI everywhere |
| `Montserrat-Italic-latin.woff2` | Montserrat Italic | lot 0 | 100–900 variable | body italics |
| `Orbitron-latin.woff2` | Orbitron | lot 0 | 400–900 variable | `--font-accent`/`--font-display`, `techxyz` theme |
| `Anton-latin.woff2` | Anton | v27 | 400 (single) | `--font-accent`/`--font-display`, `teamtrainers` theme |
| `DancingScript-latin.woff2` | Dancing Script | v29 | 400–700 variable | `--font-accent`/`--font-display`, `leyssa` theme |

The three lot 0 files carry no recorded version: they predate this README and
the `/vNN/` segment of the URL they came from was not kept. The two added at
lot 11 do, because it is in the path they were fetched from.

Dancing Script is used at 600 **and** 700 by the Leyssa theme, and one file
covers both: Google already served the variable face for a `wght@600;700`
request, so self-hosting it added a `@font-face`, not a second download.

## Licence

All five files are licensed under the **SIL Open Font License, Version 1.1**,
which permits self-hosting and redistribution as part of this application. The
licence text is in `OFL.txt` — it is identical for all four families; only the
copyright line differs, so the per-family lines are kept here:

- **Montserrat** — Copyright 2024 The Montserrat.Git Project Authors
  (https://github.com/JulietaUla/Montserrat.git)
- **Orbitron** — Copyright 2018 The Orbitron Project Authors
  (https://github.com/theleagueof/orbitron), with Reserved Font Name: "Orbitron"
- **Anton** — Copyright 2020 The Anton Project Authors
  (https://github.com/googlefonts/AntonFont.git)
- **Dancing Script** — Copyright 2016 The Dancing Script Project Authors
  (https://github.com/googlefonts/DancingScript), with Reserved Font Name
  'Dancing Script'

## Replacing or adding a family

Fetch the `css2` API with a current browser user agent (an old one is served
`ttf` instead of `woff2`), take the `/* latin */` block's URL, and keep the
`unicode-range` that came with it:

```bash
curl -A 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36' 'https://fonts.googleapis.com/css2?family=Anton&display=swap'
```

Then add the `@font-face` to `../../tokens/fonts.css`, add the file and its
copyright line above, and point the theme's `--font-display` at it.
