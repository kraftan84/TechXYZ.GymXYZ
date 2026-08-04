/* ============================================================
   GymXYZ Mobile — Accueil (dashboard, planning-first)
   Home → tap a class / alert → relevant screen.
   ============================================================ */
function MScreenAccueil({ theme, onNavigate, onOpenPlus }) {
  const D = window.GX_DATA;
  const manager = (theme && theme.manager) || D.manager;
  const solo = theme && theme.solo;
  const weekDates = React.useMemo(() => D.week.map((x) => new Date(2026, 5, x.n)), []);
  const zip = (D.reglages && D.reglages.identite && D.reglages.identite.zip) || '69003';
  const cal = useSchoolCalendar(zip, weekDates);
  const todayIdx = 1; // Mardi 9 — "aujourd'hui" dans les données (cohérent avec le Planning)
  const stripRef = React.useRef(null);
  React.useEffect(() => {
    const el = stripRef.current; if (!el) return;
    const active = el.children[todayIdx]; if (!active) return;
    el.scrollLeft = active.offsetLeft - (el.clientWidth - active.clientWidth) / 2;
  }, []);

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement(MPageTitle, {
        title: 'Bonjour ' + (manager.nick || manager.first),
        sub: 'Préparez la semaine et diffusez-la à vos membres.',
      }),

      // primary action
      React.createElement('button', { className: 'gx-m-cta', style: { marginTop: 16 }, onClick: () => onNavigate('planning') },
        React.createElement(Icon, { name: 'share', size: 20 }), 'Diffuser le planning'),

      // week card
      React.createElement(MSection, { title: D.weekRange }),
      React.createElement(MCard, null,
        React.createElement('div', { style: { padding: '12px 12px 4px' } },
          React.createElement('div', { className: 'gx-m-weekstrip', ref: stripRef },
            D.week.map((d, i) => {
              const info = gxDayInfo(cal, weekDates[i]);
              return React.createElement('div', { className: 'gx-m-day' + (i === todayIdx ? ' on' : ''), key: i, onClick: () => onNavigate('planning') },
                React.createElement('div', { className: 'dn' }, d.d),
                React.createElement('div', { className: 'nm' }, d.n),
                React.createElement('div', { className: 'ct' }, d.c + ' c.'),
                info && React.createElement('span', { className: 'mk ' + info.type, title: info.label },
                  React.createElement(Icon, { name: info.type === 'ferie' ? 'star' : 'sun', size: 10 })));
            }))),
        React.createElement('div', { className: 'gx-m-diffuse' },
          React.createElement(Icon, { name: 'send', size: 16 }),
          React.createElement('span', null, 'Dernière diffusion dimanche · ', React.createElement('b', null, 'vos membres notifiés')))),

      // today
      React.createElement(MSection, { title: "Aujourd'hui · 3 cours", more: 'Planning', onMore: () => onNavigate('planning') }),
      React.createElement(MCard, null,
        D.todayClasses.map((c, i) => {
          const pct = Math.round(c.occ / c.cap * 100);
          return React.createElement('div', { className: 'gx-m-class', key: i, onClick: () => onNavigate('presences') },
            React.createElement('div', { className: 'time' }, c.time),
            React.createElement('div', { className: 'main' },
              React.createElement('div', { className: 'nm' }, c.name),
              React.createElement('div', { className: 'meta' }, c.meta)),
            c.status
              ? React.createElement('div', { style: { flex: 'none' } }, React.createElement(MChip, { tone: c.status.tone }, c.status.t))
              : React.createElement('div', { className: 'occ' },
                  React.createElement('div', { className: 'n' }, c.occ + '/' + c.cap),
                  React.createElement(MBar, { pct, tone: pct >= 100 ? 'bad' : '' })));
        })),

      // alerts
      React.createElement(MSection, { title: 'À surveiller' }),
      React.createElement(MCard, null,
        D.alerts.map((a, i) => {
          const pal = { warning: ['var(--warning-50)', 'var(--warning-600)'], danger: ['var(--danger-50)', 'var(--color-danger)'], brand: ['var(--azure-50)', 'var(--color-primary)'] }[a.tone];
          const go = a.ic === 'card' ? 'abos' : a.ic === 'check' ? 'presences' : 'abos';
          return React.createElement('div', { className: 'gx-m-alert', key: i, onClick: () => onNavigate(go) },
            React.createElement('span', { className: 'ic', style: { background: pal[0], color: pal[1] } }, React.createElement(Icon, { name: a.ic, size: 18 })),
            React.createElement('div', { className: 'main' },
              React.createElement('div', { className: 't' }, a.t),
              React.createElement('div', { className: 'd' }, a.d)),
            React.createElement('span', { className: 'chev' }, React.createElement(Icon, { name: 'chevR', size: 18 })));
        }))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenAccueil = MScreenAccueil;
