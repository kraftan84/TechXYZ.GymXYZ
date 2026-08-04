/* ============================================================
   GymXYZ — Screen: Accueil (planning-first dashboard)
   Home → click a day → day detail.
   ============================================================ */
const { Button: AccBtn, Card: AccCard } = window.TechXYZDesignSystem_ff9a8f;

function ClassRow({ time, name, meta, occ, cap, status }) {
  const pct = Math.round(occ / cap * 100);
  const tone = pct >= 100 ? 'bad' : '';
  return React.createElement('div', { className: 'gx-classrow' },
    React.createElement('div', { className: 'time' }, time),
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 'nm' }, name),
      React.createElement('div', { className: 'meta' }, meta)),
    React.createElement('div', { className: 'occ' },
      React.createElement('div', { className: 'lab' },
        React.createElement('span', null, occ + '/' + cap + ' places'),
        React.createElement('span', { style: { color: 'var(--text-muted)' } }, pct + '%')),
      React.createElement(Bar, { pct, tone })),
    React.createElement('div', { style: { width: 78, flex: 'none', display: 'flex', justifyContent: 'flex-end' } },
      status ? React.createElement(Chip, { tone: status.tone }, status.t) : null));
}

function AlertRow({ ic, tone, t, d, action, onAction }) {
  const palette = {
    warning: ['var(--warning-50)', 'var(--warning-600)'],
    danger: ['var(--danger-50)', 'var(--color-danger)'],
    brand: ['var(--azure-50)', 'var(--color-primary)'],
  }[tone];
  return React.createElement('div', { className: 'gx-alert' },
    React.createElement('span', { className: 'ic', style: { background: palette[0], color: palette[1] } },
      React.createElement(Icon, { name: ic, size: 18 })),
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 't' }, t),
      React.createElement('div', { className: 'd' }, d)),
    React.createElement(AccBtn, { variant: 'outline', size: 'sm', onClick: onAction }, action));
}

function WeekStrip({ data, weekDates, cal, onPick }) {
  return React.createElement('div', { className: 'gx-weekstrip' },
    data.map((d, i) => {
      const info = weekDates ? gxDayInfo(cal, weekDates[i]) : null;
      return React.createElement('div', { className: 'gx-day' + (info ? ' ' + info.type : ''), key: i, onClick: () => onPick(d) },
        React.createElement('div', { className: 'dn' }, d.d),
        React.createElement('div', { className: 'nm' }, d.n),
        React.createElement('div', { className: 'ct' }, d.c + ' cours'),
        info && React.createElement('span', { className: 'gx-day-mark ' + info.type, title: info.label },
          React.createElement(Icon, { name: info.type === 'ferie' ? 'star' : 'sun', size: 11 })));
    }));
}

function ScreenAccueil({ onNavigate, theme }) {
  const D = window.GX_DATA;
  const manager = (theme && theme.manager) || D.manager;
  const solo = theme && theme.solo;
  const weekDates = React.useMemo(() => D.week.map((x) => new Date(2026, 5, x.n)), []);
  const zip = (D.reglages && D.reglages.identite && D.reglages.identite.zip) || '69003';
  const cal = useSchoolCalendar(zip, weekDates);
  const [view, setView] = React.useState('home');
  const [day, setDay] = React.useState(null);

  if (view === 'day') return React.createElement(AccueilDay, { day, onBack: () => setView('home'), onNavigate });

  return React.createElement('div', { className: 'gx-screen' },
    React.createElement(PageHead, { title: 'Bonjour ' + (manager.nick || manager.first), sub: 'Préparez la semaine et diffusez-la à vos membres.' },
      React.createElement(AccBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'eye', size: 18 }) }, 'Aperçu'),
      React.createElement(AccBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'share', size: 18 }), onClick: () => onNavigate('planning') }, 'Diffuser le planning')),

    // Week card
    React.createElement(AccCard, { padding: '0', style: { marginBottom: 18 } },
      React.createElement(CardHead, { title: D.weekRange },
        React.createElement(Chip, { tone: 'success', icon: 'check' }, 'Prêt à diffuser'),
        React.createElement('span', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)' } }, solo ? '28 cours' : '28 cours · 6 coachs')),
      React.createElement('div', { className: 'gx-card-pad' },
        React.createElement(WeekStrip, { data: D.week, weekDates, cal, onPick: (d) => { setDay(d); setView('day'); } }),
        React.createElement('div', { className: 'gx-diffuse-foot' },
          React.createElement(Icon, { name: 'send', size: 15 }),
          React.createElement('span', null, 'Dernière diffusion : dimanche dernier · ',
            React.createElement('b', null, 'vos membres notifiés'), ' par e-mail + app.'),
          React.createElement('span', { style: { marginLeft: 'auto' } },
            React.createElement(AccBtn, { variant: 'ghost', size: 'sm', iconLeft: React.createElement(Icon, { name: 'calendar', size: 16 }), onClick: () => onNavigate('planning') }, 'Ouvrir le planning'))))),

    // Two columns
    React.createElement('div', { className: 'gx-grid2', style: { gridTemplateColumns: '1.4fr 1fr' } },
      React.createElement(AccCard, { padding: '0' },
        React.createElement(CardHead, { title: "Aujourd'hui · 3 cours" },
          React.createElement('span', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)' } }, 'dim. 8 juin')),
        React.createElement('div', { style: { padding: '4px 18px 12px' } },
          D.todayClasses.map((c, i) => React.createElement(ClassRow, { key: i, ...c })))),
      React.createElement(AccCard, { padding: '0' },
        React.createElement(CardHead, { title: 'À surveiller' }),
        React.createElement('div', { style: { padding: '2px 18px 12px' } },
          D.alerts.map((a, i) => React.createElement(AlertRow, {
            key: i, ...a,
            onAction: () => { if (a.ic === 'card') onNavigate('abos'); else if (a.ic === 'check') onNavigate('presences'); },
          }))))));
}

/* ---------- Day detail ---------- */
function AccueilDay({ day, onBack, onNavigate }) {
  const slots = [
    { time: '07:15', name: 'Strength Foundations', meta: 'Studio A · Nora Lemoine · 60 min', occ: 14, cap: 20 },
    { time: '12:30', name: 'Core Express', meta: 'Studio B · Samir El Amrani · 30 min', occ: 16, cap: 18 },
    { time: '18:00', name: 'Pilates Core', meta: 'Studio A · Nora Lemoine · 50 min', occ: 12, cap: 16 },
    { time: '20:00', name: 'Mobility Reset', meta: 'Studio C · Samir El Amrani · 45 min', occ: 8, cap: 14 },
  ];
  const dl = day ? `${day.d.toLowerCase()}. ${day.n} juin` : 'Lun. 9 juin';
  return React.createElement('div', { className: 'gx-screen' },
    React.createElement(Crumb, { parts: [{ label: 'Accueil', to: 'accueil' }, dl], onNav: () => onBack() }),
    React.createElement(PageHead, { title: `${day ? day.d === 'Lun' ? 'Lundi' : day.d : 'Lundi'} ${day ? day.n : 9} juin`, sub: '4 cours programmés · 62 participants attendus' },
      React.createElement(AccBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'chevL', size: 18 }), onClick: onBack }, 'Accueil'),
      React.createElement(AccBtn, { variant: 'outline', iconRight: React.createElement(Icon, { name: 'chevR', size: 18 }) }, 'Jour suivant'),
      React.createElement(AccBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'plus', size: 18 }) }, 'Cours')),
    React.createElement('div', { className: 'gx-grid2', style: { gridTemplateColumns: '1.5fr 1fr', alignItems: 'start' } },
      React.createElement(AccCard, { style: { padding: '18px 20px' } },
        React.createElement('div', { style: { fontSize: 'var(--text-sm)', fontWeight: 700, color: 'var(--text-strong)', marginBottom: 14 } }, 'Déroulé de la journée'),
        slots.map((s, i) => {
          const pct = Math.round(s.occ / s.cap * 100);
          return React.createElement('div', { className: 'gx-tl', key: i },
            React.createElement('div', { className: 'when' }, s.time),
            React.createElement('div', { className: 'body' },
              React.createElement('div', { className: 'card' },
                React.createElement('div', { className: 'top' },
                  React.createElement('span', { className: 'nm' }, s.name),
                  React.createElement('span', { className: 'cap' }, s.occ + '/' + s.cap)),
                React.createElement('div', { className: 'meta' }, s.meta),
                React.createElement(Bar, { pct, tone: pct >= 100 ? 'bad' : '' }))));
        })),
      React.createElement('div', { className: 'gx-col' },
        React.createElement(AccCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Cette semaine' },
            React.createElement(AccBtn, { variant: 'outline', size: 'sm', iconLeft: React.createElement(Icon, { name: 'share', size: 16 }), onClick: () => onNavigate('planning') }, 'Diffuser')),
          React.createElement('div', { style: { padding: '4px 18px 10px' } },
            [['Lun', 'Strength Foundations', '07:15'], ['Mar', 'Pilates Core', '18:00'], ['Mer', 'Boxing Fundamentals', '19:30'], ['Jeu', 'Yoga Restore', '17:15']].map((r, i) =>
              React.createElement('div', { key: i, style: { display: 'flex', alignItems: 'center', gap: 10, padding: '10px 0', borderBottom: '1px solid var(--border-subtle)' } },
                React.createElement('span', { style: { width: 34, fontSize: 'var(--text-2xs)', fontWeight: 700, color: 'var(--text-muted)', textTransform: 'uppercase' } }, r[0]),
                React.createElement('span', { style: { flex: 1, fontSize: 'var(--text-sm)', fontWeight: 700, color: 'var(--text-strong)' } }, r[1]),
                React.createElement('span', { style: { fontSize: 'var(--text-xs)', color: 'var(--color-brand)', fontWeight: 700 } }, r[2]))))),
        React.createElement(AccCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Relances' }, React.createElement(Chip, { tone: 'warning' }, '6')),
          React.createElement('div', { style: { padding: '14px 18px 16px', display: 'flex', flexDirection: 'column', gap: 11 } },
            React.createElement('div', { style: { display: 'flex', alignItems: 'center', gap: 10, fontSize: 'var(--text-sm)' } },
              React.createElement('span', { style: { color: 'var(--color-warning)' } }, React.createElement(Icon, { name: 'card', size: 16 })),
              React.createElement('span', { style: { color: 'var(--text-body)' } }, '4 abonnements expirent'),
              React.createElement('span', { style: { marginLeft: 'auto' } }, React.createElement(AccBtn, { variant: 'ghost', size: 'sm', onClick: () => onNavigate('abos') }, 'Relancer'))),
            React.createElement('div', { style: { display: 'flex', alignItems: 'center', gap: 10, fontSize: 'var(--text-sm)' } },
              React.createElement('span', { style: { color: 'var(--color-danger)' } }, React.createElement(Icon, { name: 'alert', size: 16 })),
              React.createElement('span', { style: { color: 'var(--text-body)' } }, '2 paiements en retard'),
              React.createElement('span', { style: { marginLeft: 'auto' } }, React.createElement(AccBtn, { variant: 'ghost', size: 'sm', onClick: () => onNavigate('abos') }, 'Voir'))))))));
}

window.ScreenAccueil = ScreenAccueil;
