// Turns the poster the server rendered off-screen into a PNG the manager can
// publish, without any of it leaving the browser.
//
// WHY IT IS DONE HERE AND NOT ON THE SERVER
// The app is hosted on shared hosting with 128–512 MB per account. A headless
// Chromium wants 150–300 MB before the app does, and the one public account of
// trying it on that host describes it failing silently. So the browser already
// in front of the manager does the work — which is a real browser, laying out
// the real HTML, so the result matches the design at the pixel.
//
// THE TWO TRAPS, BOTH PAID FOR BELOW
// 1 · A foreignObject is its own little document. It has no <body>, inherits
//     nothing from the page and — this is the one that bites — fetches nothing
//     from the network. Every font, every image and every rule has to travel
//     inside the SVG. A font left as a URL comes out as Times, which is the
//     whole identity of the poster gone.
// 2 · WebKit has a long history of returning a blank first render here. So the
//     poster is drawn more than once and the last one kept, and the result is
//     checked for ink before it is handed over: a blank image is still a valid
//     PNG, and publishing one would be worse than failing.

let fontFacePromise = null;

/**
 * The poster's own rules, and only those. Pulling in the whole application's
 * CSS would carry hundreds of kilobytes into every image and let a rule written
 * for a screen change how the poster prints.
 *
 * Found by the data-gx-poster marker on its <link>, not by file name:
 * MapStaticAssets fingerprints the URL, so poster.css is served as
 * poster.<hash>.css and matching on the name would quietly find nothing — and
 * a poster with no rules is a blank page, which is exactly the failure the ink
 * check exists to catch. Better not to create it.
 */
function posterRules() {
  for (const sheet of document.styleSheets) {
    if (sheet.ownerNode?.dataset?.gxPoster === undefined) {
      continue;
    }

    try {
      return [...sheet.cssRules].map(rule => rule.cssText).join('\n');
    } catch {
      // A stylesheet from another origin cannot be read. poster.css is ours,
      // so this should not happen — but returning nothing beats throwing.
      return '';
    }
  }

  return '';
}

function toBase64(bytes) {
  // Chunked: fromCharCode on a 40 KB array is enough to blow the stack.
  let binary = '';

  for (let index = 0; index < bytes.length; index += 8192) {
    binary += String.fromCharCode.apply(null, bytes.subarray(index, index + 8192));
  }

  return btoa(binary);
}

async function asDataUri(url, mime) {
  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`${url} → ${response.status}`);
  }

  const bytes = new Uint8Array(await response.arrayBuffer());

  return `data:${mime || response.headers.get('content-type') || 'application/octet-stream'};base64,${toBase64(bytes)}`;
}

/**
 * Every @font-face rule in the document, including the ones inside an @import.
 *
 * document.styleSheets lists only the sheets the document links directly. An
 * imported sheet shows up in its parent as a CSSImportRule, and its own rules
 * hang off that rule — so a loop over the top level walks straight past them.
 * The five brand fonts live in techxyz/tokens/fonts.css, which styles.css
 * @imports, which is to say: all of them.
 */
function fontFaceRules(sheet, seen) {
  let rules;

  try {
    rules = [...sheet.cssRules];
  } catch {
    // Another origin. Ours are all local, so this only ever skips somebody
    // else's stylesheet.
    return [];
  }

  const faces = [];

  for (const rule of rules) {
    if (rule instanceof CSSFontFaceRule) {
      faces.push(rule);
    } else if (rule instanceof CSSImportRule && rule.styleSheet && !seen.has(rule.styleSheet)) {
      seen.add(rule.styleSheet);
      faces.push(...fontFaceRules(rule.styleSheet, seen));
    }
  }

  return faces;
}

/**
 * Every @font-face the app declares, rebuilt with the file inlined. Read from
 * the live stylesheets rather than from a list kept here, so a font added to
 * fonts.css cannot go missing from the poster without anybody noticing.
 *
 * Fetched once per session: the files are already in the browser cache by the
 * time anybody presses the button.
 */
async function inlinedFontFaces() {
  if (fontFacePromise) {
    return fontFacePromise;
  }

  fontFacePromise = (async () => {
    const faces = [];
    const families = new Set();
    const seen = new Set();

    for (const sheet of document.styleSheets) {
      seen.add(sheet);
    }

    for (const sheet of [...seen]) {
      for (const rule of fontFaceRules(sheet, seen)) {
        const source = /url\(["']?([^"')]+)["']?\)/.exec(rule.style.getPropertyValue('src'));

        if (!source) {
          continue;
        }

        try {
          // Resolved against the stylesheet, not the page. fonts.css writes
          // ../assets/fonts/…, which is relative to /css/techxyz/tokens/ —
          // resolving it against the document gives /assets/fonts/… and a 404
          // for every one of the five.
          const base = rule.parentStyleSheet?.href || document.baseURI;
          const data = await asDataUri(new URL(source[1], base).href, 'font/woff2');
          const range = rule.style.getPropertyValue('unicode-range');

          // Quoting varies — "Dancing Script" comes back quoted, Anton does
          // not — so the name is normalised before it is compared.
          families.add(rule.style.getPropertyValue('font-family').replace(/["']/g, '').trim());

          // Rebuilt from the properties rather than by patching cssText: the
          // browser normalises quoting and format() differently across engines,
          // and a textual replace that misses leaves a URL nothing can fetch.
          faces.push(
            '@font-face{' +
            `font-family:${rule.style.getPropertyValue('font-family')};` +
            `font-style:${rule.style.getPropertyValue('font-style') || 'normal'};` +
            `font-weight:${rule.style.getPropertyValue('font-weight') || '400'};` +
            (range ? `unicode-range:${range};` : '') +
            `src:url(${data}) format("woff2")}`);
        } catch {
          // One file that will not load leaves the others in place; the count
          // check in download() decides whether what is left is still the
          // brand's poster or somebody else's.
        }
      }
    }

    return { css: faces.join('\n'), families };
  })();

  return fontFacePromise;
}

/** Swaps every image in the clone for its bytes — the Leyssa mark, today. */
async function inlineImages(root) {
  await Promise.all([...root.querySelectorAll('img')].map(async image => {
    const source = image.getAttribute('src');

    if (!source || source.startsWith('data:')) {
      return;
    }

    try {
      image.setAttribute('src', await asDataUri(source));
    } catch {
      image.remove();
    }
  }));
}

/**
 * Is there anything on this canvas, or did the engine hand back an empty
 * rectangle? Counts distinct colours across a sample: a poster has hundreds, a
 * blank or single-colour render has one or two.
 */
function hasInk(canvas) {
  const context = canvas.getContext('2d');
  const step = Math.max(1, Math.floor(canvas.width / 60));
  const shades = new Set();

  for (let x = 0; x < canvas.width; x += step) {
    for (let y = 0; y < canvas.height; y += step) {
      const [r, g, b, a] = context.getImageData(x, y, 1, 1).data;
      shades.add(`${r},${g},${b},${a}`);

      if (shades.size > 8) {
        return true;
      }
    }
  }

  return false;
}

async function draw(source, width, height, scale, css) {
  const clone = source.cloneNode(true);
  await inlineImages(clone);
  clone.setAttribute('xmlns', 'http://www.w3.org/1999/xhtml');

  const markup = new XMLSerializer().serializeToString(clone);
  const svg =
    `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}">` +
      '<foreignObject x="0" y="0" width="100%" height="100%">' +
        '<div xmlns="http://www.w3.org/1999/xhtml">' +
          `<style>${css.replace(/</g, '&lt;')}</style>${markup}` +
        '</div>' +
      '</foreignObject>' +
    '</svg>';

  const image = new Image();
  image.src = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`;
  await image.decode();

  const canvas = document.createElement('canvas');
  canvas.width = width * scale;
  canvas.height = height * scale;

  const context = canvas.getContext('2d');
  context.scale(scale, scale);
  context.drawImage(image, 0, 0);

  return canvas;
}

/**
 * Renders the poster and hands the file to the browser.
 *
 * Returns { ok, reason } rather than throwing: the caller turns a refusal into
 * a message on screen and a line in the log, and a failed generation must never
 * reach the user as a broken render.
 */
export async function download(elementId, fileName, width, height, scale, displayFont) {
  const source = document.getElementById(elementId);

  if (!source) {
    return { ok: false, reason: 'poster-not-rendered' };
  }

  try {
    // Both halves of trap 1: the poster's rules and the fonts as bytes.
    const fonts = await inlinedFontFaces();
    const css = `${fonts.css}\n${posterRules()}`;

    // The identity of this image is its typeface, and the brand's display face
    // is the whole of it — Anton for one customer, Dancing Script for another.
    // A poster that falls back to the system sans is still a full page of ink,
    // so the ink check below waves it through: that is exactly how three
    // off-brand posters were once generated and reported as a success.
    // Refusing is the honest answer — better no image than one published in the
    // wrong face under the customer's name.
    if (displayFont && !fonts.families.has(displayFont)) {
      return { ok: false, reason: `font-missing:${displayFont}` };
    }

    if (document.fonts && document.fonts.ready) {
      await document.fonts.ready;
    }

    // Trap 2: draw twice and keep the second. On WebKit the first pass through
    // a foreignObject comes back blank often enough that one attempt is a coin
    // toss; on Chromium the second costs a few dozen milliseconds.
    await draw(source, width, height, 1, css);
    const canvas = await draw(source, width, height, scale, css);

    if (!hasInk(canvas)) {
      return { ok: false, reason: 'blank-render' };
    }

    const blob = await new Promise(resolve => canvas.toBlob(resolve, 'image/png'));

    if (!blob) {
      return { ok: false, reason: 'no-blob' };
    }

    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    document.body.append(anchor);
    anchor.click();
    anchor.remove();

    // Left long enough for the download to start; revoking straight away
    // cancels it on some builds.
    setTimeout(() => URL.revokeObjectURL(url), 60_000);

    return { ok: true, bytes: blob.size };
  } catch (error) {
    return { ok: false, reason: error && error.message ? error.message : 'unknown' };
  }
}
