/* ============================================================
   GymXYZ — Screen: Lieux (lieux → fiche lieu)
   catalogue de lieux : salle (intérieur), extérieur, à domicile.
   Le modèle s'adapte au type : un studio a une surface et un taux
   d'occupation ; un spot extérieur a un point de RDV + une météo ;
   le domicile prend l'adresse du membre.
   ============================================================ */
const { Button: SlBtn, Card: SlCard } = window.TechXYZDesignSystem_ff9a8f;

function slKind(s) { return s.kind || 'interieur'; }
function slIndoor(s) { return slKind(s) === 'interieur'; }

const SL_KIND_LABEL = { interieur: 'Salle', exterieur: 'Extérieur', domicile: 'À domicile' };

/* ============================================================
   MÉTÉO EN DIRECT — Open-Meteo (gratuit, sans clé, CORS ouvert)
   Pilote l'état "repli" des lieux extérieurs selon la prévision.
   ============================================================ */
const GX_WMO = {
  0: ['Ciel clair', 'sun'], 1: ['Plutôt clair', 'sun'], 2: ['Éclaircies', 'cloud'], 3: ['Couvert', 'cloud'],
  45: ['Brouillard', 'cloud'], 48: ['Brouillard givrant', 'cloud'],
  51: ['Bruine', 'cloud'], 53: ['Bruine', 'cloud'], 55: ['Bruine dense', 'cloud'],
  56: ['Bruine verglaçante', 'cloud'], 57: ['Bruine verglaçante', 'cloud'],
  61: ['Pluie faible', 'cloud'], 63: ['Pluie', 'cloud'], 65: ['Pluie forte', 'cloud'],
  66: ['Pluie verglaçante', 'cloud'], 67: ['Pluie verglaçante', 'cloud'],
  71: ['Neige faible', 'cloud'], 73: ['Neige', 'cloud'], 75: ['Neige forte', 'cloud'], 77: ['Neige', 'cloud'],
  80: ['Averses', 'cloud'], 81: ['Averses', 'cloud'], 82: ['Averses fortes', 'cloud'],
  85: ['Averses de neige', 'cloud'], 86: ['Averses de neige', 'cloud'],
  95: ['Orage', 'cloud'], 96: ['Orage', 'cloud'], 99: ['Orage', 'cloud'],
};
const GX_WEATHER_CACHE = {};

function gxVerdict(d) {
  const rainy = (d.code >= 51) || (d.precipProb >= 60) || (d.currentPrecip > 0.2);
  if (rainy) return { tone: 'danger', repli: true, label: 'Repli conseillé' };
  if (d.precipProb >= 30) return { tone: 'warning', repli: false, label: 'À surveiller' };
  return { tone: 'success', repli: false, label: 'Conditions OK' };
}

function useWeather(lat, lon) {
  const key = lat != null ? lat + ',' + lon : null;
  const [state, setState] = React.useState(
    key && GX_WEATHER_CACHE[key] ? { loading: false, data: GX_WEATHER_CACHE[key] } : { loading: key != null });
  React.useEffect(() => {
    if (key == null) { setState({ loading: false, error: true }); return; }
    if (GX_WEATHER_CACHE[key]) { setState({ loading: false, data: GX_WEATHER_CACHE[key] }); return; }
    let alive = true;
    const url = 'https://api.open-meteo.com/v1/forecast?latitude=' + lat + '&longitude=' + lon +
      '&current=temperature_2m,precipitation,weather_code&daily=precipitation_probability_max,temperature_2m_max&timezone=auto&forecast_days=1';
    fetch(url).then((r) => r.json()).then((j) => {
      if (!alive) return;
      const cur = j.current || {}, day = j.daily || {};
      const data = {
        temp: Math.round(cur.temperature_2m),
        tempMax: Math.round((day.temperature_2m_max || [])[0]),
        code: cur.weather_code,
        precipProb: (day.precipitation_probability_max || [])[0] != null ? (day.precipitation_probability_max || [])[0] : 0,
        currentPrecip: cur.precipitation != null ? cur.precipitation : 0,
      };
      GX_WEATHER_CACHE[key] = data;
      setState({ loading: false, data });
    }).catch(() => { if (alive) setState({ loading: false, error: true }); });
    return () => { alive = false; };
  }, [key]);
  return state;
}

/* sessions/week counter (shared by card foots) */
function SessionsCount({ n }) {
  return React.createElement('div', { style: { textAlign: 'right', flex: 'none' } },
    React.createElement('div', { style: { fontSize: 'var(--text-lg)', fontWeight: 'var(--weight-bold)', color: 'var(--text-strong)', lineHeight: 1, fontVariantNumeric: 'tabular-nums' } }, n),
    React.createElement('div', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)', marginTop: 3 } }, 'cours / sem.'));
}

/* live-weather foot for outdoor lieux */
function ExterieurFoot({ s }) {
  const w = useWeather(s.lat, s.lon);
  let chip, repliActive = false;
  if (w.loading) chip = React.createElement(Chip, { tone: 'neutral', icon: 'cloud' }, 'Météo…');
  else if (w.data) {
    const v = gxVerdict(w.data); repliActive = v.repli;
    const cd = GX_WMO[w.data.code] || ['Météo', 'cloud'];
    chip = React.createElement(Chip, { tone: v.tone, icon: cd[1] }, cd[0] + ' · ' + w.data.temp + '°');
  } else chip = React.createElement(Chip, { tone: 'success', icon: 'sun' }, 'Météo-dépendant');
  const left = React.createElement('div', { className: 'gx-foot-info' },
    React.createElement('div', { className: 'lab' }, w.data ? 'Météo · maintenant' : 'Conditions'),
    React.createElement('div', { className: 'row' },
      chip,
      s.fallback && React.createElement('span', { className: 'repli' + (repliActive ? ' on' : '') },
        repliActive ? '→ Repli ' : 'Repli ', React.createElement('b', null, s.fallback))));
  return React.createElement('div', { className: 'gx-salle-foot' }, left, React.createElement(SessionsCount, { n: s.sessionsWeek }));
}

function heatColor(pct) {
  if (pct >= 90) return 'var(--color-danger)';
  if (pct >= 70) return 'var(--color-primary)';
  return 'var(--azure-300)';
}

/* ---------- weekly occupancy heat strip ---------- */
function HeatStrip({ heat, days, showPct }) {
  const D = days || ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim'];
  return React.createElement('div', { className: 'gx-heat' },
    heat.map((pct, i) => React.createElement('div', { className: 'd', key: i },
      showPct && React.createElement('span', { className: 'pct' }, pct + '%'),
      React.createElement('div', { className: 'bar' },
        React.createElement('i', { style: { height: pct + '%', background: heatColor(pct) } })),
      React.createElement('span', { className: 'lab' }, D[i]))));
}

/* ---------- card meta line (adapts to kind) ---------- */
function SalleMeta({ s }) {
  const it = (icon, txt) => React.createElement('span', { className: 'it', key: icon + txt },
    React.createElement(Icon, { name: icon, size: 14 }), txt);
  const sep = (k) => React.createElement('span', { className: 'sep', key: 'sep' + k });
  let items;
  if (slKind(s) === 'exterieur') items = [it('users', s.cap + ' places'), sep(1), it('pin', s.address)];
  else if (slKind(s) === 'domicile') items = [it('user', 'Séance privée'), sep(1), it('home', s.address)];
  else items = [it('users', s.cap + ' places'), sep(1), it('maximize', s.area), sep(2), it('building', s.floor)];
  return React.createElement('div', { className: 'gx-salle-meta' }, items);
}

/* ---------- card foot (occupation / météo / adresse) ---------- */
function SalleFoot({ s }) {
  if (slKind(s) === 'exterieur') return React.createElement(ExterieurFoot, { s });
  const sessions = React.createElement(SessionsCount, { n: s.sessionsWeek });

  let left;
  if (slKind(s) === 'domicile') {
    left = React.createElement('div', { className: 'gx-foot-info' },
      React.createElement('div', { className: 'lab' }, 'Lieu de la séance'),
      React.createElement('div', { className: 'row' },
        React.createElement(Chip, { tone: 'neutral', icon: 'pin' }, 'Adresse du membre')));
  } else {
    left = React.createElement('div', { className: 'occ' },
      React.createElement('div', { className: 'lab' },
        React.createElement('span', null, 'Occupation moyenne'),
        React.createElement('b', null, s.occ + '%')),
      React.createElement(Bar, { pct: s.occ, tone: s.occ >= 90 ? 'warn' : '' }));
  }
  return React.createElement('div', { className: 'gx-salle-foot' }, left, sessions);
}

/* ---------- studio / lieu card ---------- */
function SalleCard({ s, onOpen }) {
  const shown = s.equip.slice(0, 4);
  const extra = s.equip.length - shown.length;
  return React.createElement('div', { className: 'gx-salle-card', onClick: () => onOpen(s) },
    React.createElement('div', { className: 'gx-salle-hd t-' + s.tone },
      React.createElement('span', { className: 'ic' }, React.createElement(Icon, { name: s.icon, size: 26 })),
      React.createElement('div', { className: 'tt' },
        React.createElement('div', { className: 'nm' }, s.name),
        React.createElement('div', { className: 'ty' }, s.type)),
      React.createElement('span', { className: 'chip-tr' }, React.createElement(Chip, { tone: s.statusTone }, s.status))),
    React.createElement('div', { className: 'gx-salle-bd' },
      React.createElement(SalleMeta, { s }),
      React.createElement('div', { className: 'gx-tags', style: { marginTop: 12 } },
        shown.map((e, i) => React.createElement('span', { className: 'gx-tag', key: i }, e)),
        extra > 0 && React.createElement('span', { className: 'gx-tag', key: 'x' }, '+' + extra)),
      React.createElement(SalleFoot, { s })));
}

/* ---------- list ---------- */
function SallesList({ onOpen }) {
  const D = window.GX_DATA.salles;
  const indoor = D.filter(slIndoor);
  const avgOcc = Math.round(indoor.reduce((n, s) => n + s.occ, 0) / indoor.length);
  const totalSlots = D.reduce((n, s) => n + s.sessionsWeek, 0);
  const totalCap = D.filter((s) => slKind(s) !== 'domicile').reduce((n, s) => n + s.cap, 0);
  const byKind = { interieur: 0, exterieur: 0, domicile: 0 };
  D.forEach((s) => { byKind[slKind(s)]++; });
  const espacesSub = [
    byKind.interieur && byKind.interieur + ' en salle',
    byKind.exterieur && byKind.exterieur + ' extérieur',
    byKind.domicile && byKind.domicile + ' à domicile',
  ].filter(Boolean).join(' · ');

  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: ['Accueil', 'Lieux'] }),
    React.createElement(PageHead, { title: 'Lieux', sub: D.length + ' lieux · ' + totalSlots + ' créneaux par semaine' },
      React.createElement(SlBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'plus', size: 18 }) }, 'Nouveau lieu')),
    React.createElement('div', { className: 'gx-grid4', style: { marginBottom: 18 } },
      React.createElement(Kpi, { label: 'Lieux', value: String(D.length), sub: espacesSub }),
      React.createElement(Kpi, { label: 'Occupation salles', value: avgOcc + '%', delta: '+3 pts ce mois', deltaIcon: 'trend', spark: true }),
      React.createElement(Kpi, { label: 'Créneaux / semaine', value: String(totalSlots), sub: 'tous lieux confondus' }),
      React.createElement(Kpi, { label: 'Capacité totale', value: String(totalCap), sub: 'places en simultané' })),
    React.createElement('div', { className: 'gx-salles' },
      D.map((s) => React.createElement(SalleCard, { key: s.id, s, onOpen }))));
}

/* ---------- fiche lieu ---------- */
function SalleTodayRow({ r }) {
  const time = r[0], name = r[1], coach = r[2], occ = r[3], cap = r[4];
  const pct = Math.round((occ / cap) * 100);
  return React.createElement('div', { className: 'gx-listrow' },
    React.createElement('span', { style: { width: 52, flex: 'none', fontSize: 'var(--text-sm)', fontWeight: 'var(--weight-bold)', color: 'var(--color-brand)' } }, time),
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 't' }, name),
      React.createElement('div', { className: 's' }, coach === '—' ? 'Accès libre' : coach)),
    React.createElement(Chip, { tone: pct >= 100 ? 'danger' : 'brand' }, cap === 1 ? 'Privé' : occ + '/' + cap));
}

/* point-de-RDV / adresse card (extérieur & domicile) */
function RdvCard({ s, weather }) {
  const ext = slKind(s) === 'exterieur';
  const w = weather || {};
  let banner = null, repliLine;
  if (ext) {
    if (w.data) {
      const v = gxVerdict(w.data);
      const cd = GX_WMO[w.data.code] || ['Météo', 'cloud'];
      banner = React.createElement('div', { className: 'gx-wx' },
        React.createElement('span', { className: 'ic' }, React.createElement(Icon, { name: cd[1], size: 22 })),
        React.createElement('div', { className: 'm' },
          React.createElement('div', { className: 't' }, cd[0] + ' · ' + w.data.temp + '°'),
          React.createElement('div', { className: 's' }, 'Max ' + w.data.tempMax + '° · pluie ' + w.data.precipProb + '%')),
        React.createElement(Chip, { tone: v.tone }, v.label));
      repliLine = v.repli
        ? React.createElement('div', { className: 'gx-rdv-repli on' },
            React.createElement(Icon, { name: 'cloud', size: 16 }),
            React.createElement('span', null, 'Pluie attendue aujourd\u2019hui — repli conseillé en ', React.createElement('b', null, s.fallback)))
        : React.createElement('div', { className: 'gx-rdv-repli' },
            React.createElement(Icon, { name: 'sun', size: 16 }),
            React.createElement('span', null, 'Conditions favorables — séance maintenue au parc'));
    } else {
      repliLine = s.fallback && React.createElement('div', { className: 'gx-rdv-repli' },
        React.createElement(Icon, { name: 'cloud', size: 16 }),
        React.createElement('span', null, 'En cas de pluie, repli en ', React.createElement('b', null, s.fallback)));
    }
  } else {
    repliLine = React.createElement('div', { className: 'gx-rdv-repli' },
      React.createElement(Icon, { name: 'send', size: 16 }),
      React.createElement('span', null, 'Transmise au coach avant chaque rendez-vous'));
  }
  return React.createElement(SlCard, { padding: '0' },
    React.createElement(CardHead, { title: ext ? 'Point de rendez-vous' : 'Adresse de la séance' }),
    banner,
    React.createElement('div', { className: 'gx-rdv' },
      React.createElement('span', { className: 'pin' }, React.createElement(Icon, { name: ext ? 'pin' : 'home', size: 20 })),
      React.createElement('div', null,
        React.createElement('div', { className: 'addr' }, ext ? s.address : 'Au domicile du membre'),
        React.createElement('div', { className: 'sub' }, ext ? s.name : 'Renseignée sur la fiche du membre'))),
    repliLine);
}

function FicheSalle({ salle, onBack }) {
  const s = salle || window.GX_DATA.salles[0];
  const indoor = slIndoor(s);
  const weather = useWeather(s.lat, s.lon);
  const wVerdict = (slKind(s) === 'exterieur' && weather.data) ? gxVerdict(weather.data) : null;
  const stat = (lab, val, sub) => React.createElement('div', { className: 'gx-kpi', key: lab },
    React.createElement('div', { className: 'lab' }, lab),
    React.createElement('div', { className: 'val', style: { fontSize: 'var(--text-2xl)' } }, val),
    React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 3 } }, sub));
  const kv = (k, v) => React.createElement('div', { className: 'gx-kv', key: k },
    React.createElement('span', { className: 'k' }, k), React.createElement('span', { className: 'v' }, v));

  // contact line under the name
  let contact;
  if (slKind(s) === 'exterieur') contact = s.type + ' · ' + s.address;
  else if (slKind(s) === 'domicile') contact = s.type + ' · séance individuelle';
  else contact = s.type + ' · ' + s.area + ' · ' + s.floor;

  // top stat triplet
  let stats;
  if (slKind(s) === 'exterieur') stats = [
    ['Créneaux / sem.', String(s.sessionsWeek), 'au planning'],
    ['Capacité', String(s.cap), 'places (souple)'],
    ['Repli', s.fallback || '—', wVerdict ? (wVerdict.repli ? 'conseillé aujourd\u2019hui' : 'non requis aujourd\u2019hui') : 'en cas de pluie'],
  ];
  else if (slKind(s) === 'domicile') stats = [
    ['Créneaux / sem.', String(s.sessionsWeek), 'au planning'],
    ['Capacité', '1', 'place / séance'],
    ['Déplacement', 'Mobile', 'chez le membre'],
  ];
  else stats = [
    ['Occupation', s.occ + '%', 'moyenne 30 j'],
    ['Créneaux / sem.', String(s.sessionsWeek), 'au planning'],
    ['Capacité', String(s.cap), 'places'],
  ];

  // info key/values
  let infos;
  if (slKind(s) === 'exterieur') infos = [
    ['Type', s.type], ['Point de RDV', s.address], ['Capacité', s.cap + ' places'],
    ['Météo', 'Dépendant'], ['Repli', s.fallback || '—'],
  ];
  else if (slKind(s) === 'domicile') infos = [
    ['Type', s.type], ['Adresse', 'Chez le membre'], ['Capacité', '1 place'],
    ['Déplacement', 'Coach se déplace'],
  ];
  else infos = [
    ['Type', s.type], ['Surface', s.area], ['Étage', s.floor],
    ['Capacité', s.cap + ' places'], ['Accès', s.status],
  ];

  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: [{ label: 'Accueil' }, { label: 'Lieux', to: 'salles' }, s.name], onNav: onBack }),
    React.createElement('div', { className: 'gx-fiche-head' },
      React.createElement('span', { className: 'gx-disc-ic big t-' + (s.tone === 'brand' ? 'brand' : s.tone) }, React.createElement(Icon, { name: s.icon, size: 28 })),
      React.createElement('div', { className: 'info' },
        React.createElement('div', { className: 'name-row' },
          React.createElement('span', { className: 'name' }, s.name),
          React.createElement(Chip, { tone: 'neutral' }, SL_KIND_LABEL[slKind(s)]),
          wVerdict
            ? React.createElement(Chip, { tone: wVerdict.tone }, (GX_WMO[weather.data.code] || ['Météo'])[0] + ' · ' + weather.data.temp + '°')
            : React.createElement(Chip, { tone: s.statusTone }, s.status)),
        React.createElement('div', { className: 'contact' }, contact)),
      React.createElement(SlBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'calendar', size: 18 }) }, 'Planning'),
      React.createElement(SlBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'settings', size: 18 }) }, 'Éditer')),

    React.createElement('div', { className: 'gx-grid2', style: { gridTemplateColumns: '1.3fr 1fr', alignItems: 'start' } },
      // left
      React.createElement('div', { className: 'gx-col' },
        React.createElement('div', { className: 'gx-grid3' },
          stats.map((t) => stat(t[0], t[1], t[2]))),
        React.createElement(SlCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Planning du jour' },
            React.createElement(SlBtn, { variant: 'ghost', size: 'sm', iconLeft: React.createElement(Icon, { name: 'calendar', size: 16 }) }, 'Semaine')),
          React.createElement('div', { style: { padding: '4px 18px 8px' } },
            s.today.map((r, i) => React.createElement(SalleTodayRow, { key: i, r })))),
        indoor
          ? React.createElement(SlCard, { padding: '0' },
              React.createElement(CardHead, { title: 'Occupation de la semaine' },
                React.createElement('span', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)' } }, 'moyenne par jour')),
              React.createElement('div', { style: { padding: '16px 18px 16px' } },
                React.createElement(HeatStrip, { heat: s.heat, showPct: true })))
          : React.createElement(RdvCard, { s, weather })),
      // right
      React.createElement('div', { className: 'gx-col' },
        React.createElement(SlCard, { padding: '0' },
          React.createElement(CardHead, { title: 'À propos' }),
          React.createElement('div', { style: { padding: '14px 18px 16px', fontSize: 'var(--text-sm)', color: 'var(--text-body)', lineHeight: 1.55 } }, s.note)),
        React.createElement(SlCard, { padding: '0' },
          React.createElement(CardHead, { title: indoor ? 'Équipement' : 'Matériel' },
            React.createElement(Chip, { tone: 'neutral' }, s.equip.length)),
          React.createElement('div', { style: { padding: '14px 18px 18px' } },
            React.createElement('div', { className: 'gx-tags' },
              s.equip.map((e, i) => React.createElement('span', { className: 'gx-tag brand', key: i }, e))))),
        React.createElement(SlCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Informations' }),
          React.createElement('div', { style: { padding: '4px 18px 10px' } },
            infos.map((p) => kv(p[0], p[1])))))));
}

function ScreenSalles() {
  const [view, setView] = React.useState('list');
  const [salle, setSalle] = React.useState(null);
  return React.createElement('div', { className: 'gx-screen' },
    view === 'list'
      ? React.createElement(SallesList, { onOpen: (s) => { setSalle(s); setView('fiche'); } })
      : React.createElement(FicheSalle, { salle, onBack: () => setView('list') }));
}

window.ScreenSalles = ScreenSalles;
