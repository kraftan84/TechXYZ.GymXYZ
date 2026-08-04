/* ============================================================
   GymXYZ — Calendrier scolaire & jours fériés (données réelles)
   Sources publiques, sans clé :
     • Jours fériés  : calendrier.api.gouv.fr (métropole)
     • Vacances      : data.education.gouv.fr (fr-en-calendrier-scolaire)
   La ZONE (A/B/C) est déduite du code postal de l'adresse de la
   salle / du coach. Tout est mis en cache au niveau module.
   ============================================================ */

/* département → zone (métropole) */
const GX_ZONE_DEPTS = {
  A: '01 03 07 15 16 17 19 21 23 24 25 26 33 38 39 40 42 43 47 58 63 64 69 70 71 73 74 79 86 87 89 90',
  B: '02 04 05 06 08 10 13 14 18 22 27 28 29 35 36 37 41 44 45 49 50 51 52 53 54 55 56 57 59 60 61 62 67 68 72 76 80 83 84 85 88',
  C: '09 11 12 30 31 32 34 46 48 65 66 75 77 78 81 82 91 92 93 94 95',
};

function gxZoneForZip(zip) {
  const d = String(zip || '').trim().slice(0, 2);
  for (const z of ['A', 'B', 'C']) {
    if (GX_ZONE_DEPTS[z].split(' ').indexOf(d) !== -1) return z;
  }
  return 'A';
}

/* ---- date helpers (calendar-date strings, no time) ---- */
function gxIso(d) { return d.toLocaleDateString('en-CA'); }            // local YYYY-MM-DD
function gxParisDay(iso) { return new Date(iso).toLocaleDateString('en-CA', { timeZone: 'Europe/Paris' }); }
function gxAddDaysIso(iso, n) {
  const [y, m, d] = iso.split('-').map(Number);
  const dt = new Date(y, m - 1, d + n);
  return gxIso(dt);
}
const GX_MONTHS = ['janv.', 'févr.', 'mars', 'avril', 'mai', 'juin', 'juill.', 'août', 'sept.', 'oct.', 'nov.', 'déc.'];
function gxPrettyIso(iso) { const [y, m, d] = iso.split('-').map(Number); return d + ' ' + GX_MONTHS[m - 1]; }
function gxAddDays(d, n) { return new Date(d.getFullYear(), d.getMonth(), d.getDate() + n); }
function gxMonday(d) { const x = new Date(d.getFullYear(), d.getMonth(), d.getDate()); return gxAddDays(x, -((x.getDay() + 6) % 7)); }

/* ---- fetch + cache ---- */
const GX_CAL_CACHE = { feries: {}, vac: {} };

async function gxLoadFeries(year) {
  if (GX_CAL_CACHE.feries[year]) return GX_CAL_CACHE.feries[year];
  const r = await fetch('https://calendrier.api.gouv.fr/jours-feries/metropole/' + year + '.json');
  const j = await r.json();
  GX_CAL_CACHE.feries[year] = j;
  return j;
}

async function gxLoadVacances(zone, fromISO, toISO) {
  const key = zone + '|' + fromISO + '|' + toISO;
  if (GX_CAL_CACHE.vac[key]) return GX_CAL_CACHE.vac[key];
  const where = encodeURIComponent('zones="Zone ' + zone + '" and end_date>="' + fromISO + '" and start_date<="' + toISO + '"');
  const url = 'https://data.education.gouv.fr/api/explore/v2.1/catalog/datasets/fr-en-calendrier-scolaire/records?where=' + where + '&order_by=start_date&limit=100';
  const r = await fetch(url);
  const j = await r.json();
  const seen = {}, list = [];
  (j.results || []).forEach((x) => {
    if (x.population === 'Enseignants') return;
    const k = x.description + '|' + x.start_date;
    if (seen[k]) return; seen[k] = 1;
    list.push({
      desc: x.description,
      start: gxParisDay(x.start_date),                 // 1er jour de congé
      end: gxAddDaysIso(gxParisDay(x.end_date), -1),   // dernier jour avant rentrée
    });
  });
  GX_CAL_CACHE.vac[key] = list;
  return list;
}

/* ---- hook : charge fériés (années visibles) + vacances (zone) ---- */
function useSchoolCalendar(zip, weekDates) {
  const zone = gxZoneForZip(zip);
  const anchor = weekDates && weekDates[0] ? weekDates[0].getTime() : 0;
  const [state, setState] = React.useState({ loading: true, zone });
  React.useEffect(() => {
    if (!weekDates || !weekDates.length) return;
    let alive = true;
    (async () => {
      try {
        const years = Array.from(new Set(weekDates.map((d) => d.getFullYear())));
        const objs = await Promise.all(years.map(gxLoadFeries));
        const feries = {}; objs.forEach((o) => Object.assign(feries, o));
        const fromISO = Math.min.apply(null, years) + '-01-01';
        const toISO = (Math.max.apply(null, years) + 1) + '-02-28';
        const vac = await gxLoadVacances(zone, fromISO, toISO);
        if (alive) setState({ loading: false, zone, feries, vac });
      } catch (e) {
        if (alive) setState({ loading: false, zone, error: true });
      }
    })();
    return () => { alive = false; };
  }, [zone, anchor]);
  return state;
}

/* ---- lookup : qu'y a-t-il ce jour ? ---- */
function gxDayInfo(cal, date) {
  if (!cal || cal.loading || cal.error) return null;
  const iso = gxIso(date);
  if (cal.feries && cal.feries[iso]) return { type: 'ferie', label: cal.feries[iso] };
  if (cal.vac) {
    for (const v of cal.vac) {
      if (iso >= v.start && iso <= v.end) return { type: 'vac', label: v.desc };
    }
  }
  return null;
}

/* prochain jour férié / vacances à partir d'une date (pour le bandeau) */
function gxNextEvents(cal, fromDate) {
  if (!cal || cal.loading || cal.error) return {};
  const today = gxIso(fromDate);
  let nextFerie = null;
  Object.keys(cal.feries || {}).sort().forEach((iso) => {
    if (!nextFerie && iso >= today) nextFerie = { iso, label: cal.feries[iso] };
  });
  let curVac = null, nextVac = null;
  (cal.vac || []).slice().sort((a, b) => a.start.localeCompare(b.start)).forEach((v) => {
    if (today >= v.start && today <= v.end) { if (!curVac) curVac = v; }
    else if (v.start > today && !nextVac) nextVac = v;
  });
  return { nextFerie, curVac, nextVac };
}

/* ---- bandeau calendrier (zone + ce qui concerne la semaine) ---- */
function CalendarBanner({ cal, weekDates, refDate }) {
  const zone = cal ? cal.zone : '—';
  // évènements tombant dans la semaine affichée
  const inWeek = [];
  if (cal && !cal.loading && weekDates) {
    const seen = {};
    weekDates.forEach((d) => {
      const info = gxDayInfo(cal, d);
      if (info && !seen[info.type + info.label]) { seen[info.type + info.label] = 1; inWeek.push(info); }
    });
  }
  let detail;
  if (!cal || cal.loading) {
    detail = React.createElement('span', { className: 'muted' }, 'Chargement du calendrier…');
  } else if (cal.error) {
    detail = React.createElement('span', { className: 'muted' }, 'Calendrier indisponible');
  } else if (inWeek.length) {
    detail = React.createElement('span', { className: 'items' },
      inWeek.map((it, i) => React.createElement('span', { key: i, className: 'gx-cal-pill ' + it.type },
        React.createElement(Icon, { name: it.type === 'ferie' ? 'star' : 'sun', size: 13 }), it.label)));
  } else {
    const nx = gxNextEvents(cal, refDate || new Date());
    const bits = [];
    if (nx.curVac) bits.push('en cours : ' + nx.curVac.desc.toLowerCase());
    if (nx.nextFerie) bits.push('prochain férié : ' + nx.nextFerie.label + ' (' + gxPrettyIso(nx.nextFerie.iso) + ')');
    if (!nx.curVac && nx.nextVac) bits.push('prochaines vacances : ' + nx.nextVac.desc.toLowerCase() + ' dès le ' + gxPrettyIso(nx.nextVac.start));
    detail = React.createElement('span', { className: 'muted' },
      'Rien cette semaine — ' + (bits.join(' · ') || 'aucun évènement à venir'));
  }
  return React.createElement('div', { className: 'gx-calbar' },
    React.createElement('span', { className: 'gx-cal-zone' },
      React.createElement(Icon, { name: 'pin', size: 13 }), 'Zone ' + zone),
    detail);
}

/* ---- carte "Calendrier scolaire" pour les Réglages (Identité) ---- */
function CalendarCard({ cal, refDate }) {
  const { Card } = window.TechXYZDesignSystem_ff9a8f;
  const zone = cal ? cal.zone : '—';
  const loading = !cal || cal.loading;
  const error = cal && cal.error;
  const nx = (!loading && !error) ? gxNextEvents(cal, refDate || new Date()) : {};
  let ferieVal = '—', vacVal = '—';
  if (!loading && !error) {
    if (nx.nextFerie) ferieVal = nx.nextFerie.label + ' · ' + gxPrettyIso(nx.nextFerie.iso);
    if (nx.curVac) vacVal = 'En cours : ' + nx.curVac.desc + ' (jusqu’au ' + gxPrettyIso(nx.curVac.end) + ')';
    else if (nx.nextVac) vacVal = nx.nextVac.desc + ' dès le ' + gxPrettyIso(nx.nextVac.start);
  }
  const kv = (k, v) => React.createElement('div', { className: 'gx-kv', key: k },
    React.createElement('span', { className: 'k' }, k),
    React.createElement('span', { className: 'v', style: { textAlign: 'right', maxWidth: '62%' } }, v));
  return React.createElement(Card, { padding: '0' },
    React.createElement(CardHead, { title: 'Calendrier scolaire' },
      React.createElement(Chip, { tone: 'brand', icon: 'pin' }, 'Zone ' + zone)),
    React.createElement('div', { style: { padding: '12px 18px 6px' } },
      React.createElement('p', { style: { margin: '0 0 4px', fontSize: 'var(--text-xs)', color: 'var(--text-muted)', lineHeight: 1.5 } },
        'Zone déduite du code postal ci-dessus. Elle sert à signaler les jours fériés et les vacances scolaires sur le planning et la vue semaine.'),
      loading
        ? kv('Calendrier', 'Chargement…')
        : error
          ? kv('Calendrier', 'Indisponible')
          : React.createElement(React.Fragment, null,
              kv('Prochain jour férié', ferieVal),
              kv('Vacances scolaires', vacVal))));
}

Object.assign(window, {
  gxZoneForZip, useSchoolCalendar, gxDayInfo, gxNextEvents, gxIso, gxPrettyIso, gxAddDays, gxMonday, CalendarBanner, CalendarCard,
});
