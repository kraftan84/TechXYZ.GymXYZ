/* ============================================================
   GymXYZ — App root: theme state, routing, Tweaks panel
   ============================================================ */
const { Button: AppBtn } = window.TechXYZDesignSystem_ff9a8f;

const TWEAK_DEFAULTS = /*EDITMODE-BEGIN*/{
  "theme": "techxyz",
  "density": "standard",
  "anim": true
}/*EDITMODE-END*/;

const GX_SCREENS = {
  accueil: window.ScreenAccueil,
  planning: window.ScreenPlanning,
  presences: window.ScreenPresences,
  membres: window.ScreenMembres,
  coachs: window.ScreenCoachs,
  cours: window.ScreenCours,
  abos: window.ScreenAbos,
  salles: window.ScreenSalles,
  reglages: window.ScreenReglages,
  administration: window.ScreenAdministration,
};
const GX_PLACEHOLDERS = {};

function ThemePicker({ value, onChange }) {
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

function App() {
  const [t, setTweak] = useTweaks(TWEAK_DEFAULTS);
  const [nav, setNav] = React.useState('accueil');

  // restore last theme/screen for demo continuity
  React.useEffect(() => {
    try {
      const saved = localStorage.getItem('gx-theme');
      if (saved && saved !== t.theme && window.GX_THEMES.some((x) => x.id === saved)) setTweak('theme', saved);
      const sc = localStorage.getItem('gx-nav');
      if (sc && (GX_SCREENS[sc] || GX_PLACEHOLDERS[sc])) setNav(sc);
    } catch (e) {}
  }, []);

  React.useEffect(() => {
    document.documentElement.dataset.theme = t.theme;
    try { localStorage.setItem('gx-theme', t.theme); } catch (e) {}
  }, [t.theme]);
  React.useEffect(() => { try { localStorage.setItem('gx-nav', nav); } catch (e) {} }, [nav]);

  const theme = window.GX_THEMES.find((x) => x.id === t.theme) || window.GX_THEMES[0];
  // Coach indépendant : pas de section "Coachs" — on redirige si besoin
  React.useEffect(() => { if (theme.solo && nav === 'coachs') setNav('accueil'); }, [theme.solo, nav]);
  const Screen = GX_SCREENS[nav];
  const ph = GX_PLACEHOLDERS[nav];

  return React.createElement('div', {
    className: 'gx-app gx-density-' + t.density + (t.anim ? '' : ' gx-no-anim'),
  },
    React.createElement(Sidebar, { theme, active: nav, onNavigate: setNav }),
    React.createElement('div', { className: 'gx-main' },
      React.createElement(Topbar, { theme, active: nav, onNavigate: setNav }),
      React.createElement('div', { className: 'gx-content' },
        React.createElement('div', { className: 'gx-wrap' },
          Screen
            ? React.createElement(Screen, { onNavigate: setNav, theme, onSetTheme: (id) => setTweak('theme', id) })
            : React.createElement(EmptyState, { icon: ph[0], title: ph[1], text: ph[2] })))),

    React.createElement(TweaksPanel, { title: 'Tweaks' },
      React.createElement(TweakSection, { label: 'Thème — marque blanche' }),
      React.createElement('div', { style: { fontSize: 10.5, color: 'rgba(41,38,27,.6)', margin: '-2px 0 6px', lineHeight: 1.4 } },
        'Basculez l\'habillage en direct devant un prospect. Tout l\'app suit : couleurs, logo, typo, nom.'),
      React.createElement(ThemePicker, { value: t.theme, onChange: (v) => setTweak('theme', v) }),
      React.createElement(TweakSection, { label: 'Affichage' }),
      React.createElement(TweakRadio, {
        label: 'Densité', value: t.density, options: ['compact', 'standard', 'confort'],
        onChange: (v) => setTweak('density', v),
      }),
      React.createElement(TweakToggle, { label: "Animations d'entrée", value: t.anim, onChange: (v) => setTweak('anim', v) })));
}

ReactDOM.createRoot(document.getElementById('root')).render(React.createElement(App));
