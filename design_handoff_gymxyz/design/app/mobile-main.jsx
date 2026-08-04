/* ============================================================
   GymXYZ Mobile — App root
   Theme state · tab routing · Plus / theme sheets · Tweaks ·
   iPhone frame + responsive scaling.
   ============================================================ */

const M_TWEAK_DEFAULTS = /*EDITMODE-BEGIN*/{
  "theme": "techxyz",
  "anim": true
}/*EDITMODE-END*/;

const M_SCREENS = {
  accueil: window.MScreenAccueil,
  planning: window.MScreenPlanning,
  presences: window.MScreenPresences,
  membres: window.MScreenMembres,
  coachs: window.MScreenCoachs,
  cours: window.MScreenCours,
  abos: window.MScreenAbos,
  salles: window.MScreenSalles,
  reglages: window.MScreenReglages,
  administration: window.MScreenAdministration,
};
const M_ROOT_TABS = ['accueil', 'planning', 'presences', 'membres'];

/* ---------- Tweaks theme picker (mirrors desktop) ---------- */
function MTweakThemePicker({ value, onChange }) {
  return React.createElement('div', { style: { display: 'flex', flexDirection: 'column', gap: 7 } },
    window.GX_THEMES.map((th) => {
      const on = th.id === value;
      return React.createElement('button', {
        key: th.id, type: 'button', onClick: () => onChange(th.id),
        style: {
          display: 'flex', alignItems: 'center', gap: 10, width: '100%', textAlign: 'left',
          padding: '8px 10px', borderRadius: 9, cursor: 'pointer',
          border: '1px solid ' + (on ? 'rgba(0,0,0,.8)' : 'rgba(0,0,0,.1)'),
          background: on ? 'rgba(255,255,255,.92)' : 'rgba(255,255,255,.5)',
          boxShadow: on ? '0 0 0 1px rgba(0,0,0,.7)' : 'none',
        },
      },
        React.createElement('span', { style: { display: 'flex', borderRadius: 5, overflow: 'hidden', flex: 'none', boxShadow: '0 0 0 .5px rgba(0,0,0,.15)' } },
          th.swatch.map((c, i) => React.createElement('span', { key: i, style: { width: 13, height: 26, background: c } }))),
        React.createElement('span', { style: { flex: 1, minWidth: 0 } },
          React.createElement('span', { style: { display: 'block', fontWeight: 600, fontSize: 12, color: '#29261b' } }, th.label),
          React.createElement('span', { style: { display: 'block', fontSize: 10.5, color: 'rgba(41,38,27,.55)' } }, th.sub)),
        on && React.createElement('span', { style: { flex: 'none', color: '#29261b' } },
          React.createElement('svg', { width: 14, height: 14, viewBox: '0 0 14 14' },
            React.createElement('path', { d: 'M3 7.2 5.8 10 11 4.2', fill: 'none', stroke: 'currentColor', strokeWidth: 2.2, strokeLinecap: 'round', strokeLinejoin: 'round' }))));
    }));
}

/* ---------- responsive scale hook ---------- */
function useDeviceScale(w, h) {
  const [scale, setScale] = React.useState(1);
  React.useEffect(() => {
    const fit = () => {
      const m = 28;
      const s = Math.min((window.innerWidth - m) / w, (window.innerHeight - m) / h, 1);
      setScale(s > 0 ? s : 1);
    };
    fit();
    window.addEventListener('resize', fit);
    return () => window.removeEventListener('resize', fit);
  }, [w, h]);
  return scale;
}

function MApp() {
  const [t, setTweak] = useTweaks(M_TWEAK_DEFAULTS);
  const [nav, setNav] = React.useState('accueil');
  const [plus, setPlus] = React.useState(false);
  const DEV_W = 402, DEV_H = 874;
  const scale = useDeviceScale(DEV_W, DEV_H);

  // restore last theme/screen for demo continuity
  React.useEffect(() => {
    try {
      const saved = localStorage.getItem('gxm-theme');
      if (saved && saved !== t.theme && window.GX_THEMES.some((x) => x.id === saved)) setTweak('theme', saved);
      const sc = localStorage.getItem('gxm-nav');
      if (sc && M_SCREENS[sc]) setNav(sc);
    } catch (e) {}
  }, []);
  React.useEffect(() => {
    document.documentElement.dataset.theme = t.theme;
    try { localStorage.setItem('gxm-theme', t.theme); } catch (e) {}
  }, [t.theme]);
  React.useEffect(() => { try { localStorage.setItem('gxm-nav', nav); } catch (e) {} }, [nav]);

  const theme = window.GX_THEMES.find((x) => x.id === t.theme) || window.GX_THEMES[0];
  // Coach indépendante (solo) : pas de section Coachs
  React.useEffect(() => { if (theme.solo && nav === 'coachs') setNav('accueil'); }, [theme.solo, nav]);

  const Screen = M_SCREENS[nav] || window.MScreenAccueil;
  const counts = { presences: window.GX_DATA.presences.kpis.pointer };
  const tabActive = M_ROOT_TABS.indexOf(nav) !== -1 ? nav : null;

  const goto = (id) => { setNav(id); setPlus(false); };

  const app = React.createElement('div', { className: 'gx-m-app' + (t.anim ? '' : ' gx-no-anim') },
    React.createElement(Screen, {
      key: nav, theme, onNavigate: setNav, onOpenPlus: () => setPlus(true),
    }),
    React.createElement(MTabBar, {
      active: tabActive, onNavigate: goto, onPlus: () => setPlus(true),
      plusActive: plus || M_ROOT_TABS.indexOf(nav) === -1, counts,
    }),
    plus && React.createElement(MPlusSheet, {
      theme, onClose: () => setPlus(false), onNavigate: setNav,
    }));

  return React.createElement(React.Fragment, null,
    React.createElement('div', { className: 'gx-m-stage' },
      React.createElement('div', { className: 'gx-m-scale', style: { transform: 'scale(' + scale + ')' } },
        React.createElement(IOSDevice, { width: DEV_W, height: DEV_H }, app))),

    React.createElement(TweaksPanel, { title: 'Tweaks' },
      React.createElement(TweakSection, { label: 'Thème — marque blanche' }),
      React.createElement('div', { style: { fontSize: 10.5, color: 'rgba(41,38,27,.6)', margin: '-2px 0 6px', lineHeight: 1.4 } },
        'Basculez l\'habillage en direct devant un prospect. Tout l\'app suit : couleurs, logo, typo, nom.'),
      React.createElement(MTweakThemePicker, { value: t.theme, onChange: (v) => setTweak('theme', v) }),
      React.createElement(TweakSection, { label: 'Affichage' }),
      React.createElement(TweakToggle, { label: "Animations d'entrée", value: t.anim, onChange: (v) => setTweak('anim', v) })));
}

ReactDOM.createRoot(document.getElementById('root')).render(React.createElement(MApp));
