/* ============================================================
   GymXYZ Mobile — Abonnements (formules + suivi + encaissements)
   ============================================================ */
function MScreenAbos({ theme, onNavigate, onOpenPlus }) {
  const A = window.GX_DATA.abos;
  const [tab, setTab] = React.useState('suivi');

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement(MPageTitle, { title: 'Abonnements', sub: A.kpis.active + ' abonnements actifs' }),

      // KPIs
      React.createElement('div', { className: 'gx-m-kpis', style: { marginTop: 16 } },
        React.createElement(MKpi, { label: 'Revenu mensuel', value: A.kpis.mrr, delta: A.kpis.mrrDelta, deltaIcon: 'trend', spark: true }),
        React.createElement(MKpi, { label: 'Actifs', value: A.kpis.active, sub: A.kpis.expiring + ' expirent bientôt' })),
      React.createElement('div', { className: 'gx-m-kpis', style: { marginTop: 10 } },
        React.createElement(MKpi, { label: 'En retard', value: A.kpis.late, sub: A.kpis.lateAmt + ' à encaisser' }),
        React.createElement(MKpi, { label: 'Expirent', value: A.kpis.expiring, sub: 'cette semaine' })),

      // segmented tabs
      React.createElement('div', { className: 'gx-m-datebar', style: { padding: '14px 0 2px', gap: 8 } },
        [['suivi', 'Suivi'], ['formules', 'Formules'], ['encaiss', 'Encaissements']].map((t) =>
          React.createElement('button', { key: t[0], onClick: () => setTab(t[0]),
            style: { flex: 'none', padding: '8px 14px', borderRadius: 'var(--radius-pill)', fontFamily: 'inherit', cursor: 'pointer', fontSize: 'var(--text-xs)', fontWeight: 700,
              border: '1px solid ' + (tab === t[0] ? 'var(--azure-200)' : 'var(--border-subtle)'),
              background: tab === t[0] ? 'var(--azure-50)' : 'var(--surface-card)',
              color: tab === t[0] ? 'var(--azure-800)' : 'var(--text-body)' } }, t[1]))),

      tab === 'suivi' && React.createElement(MCard, { style: { marginTop: 12 } },
        A.subs.map((s, i) => React.createElement('div', { className: 'gx-m-row', key: i, style: { cursor: 'default' } },
          React.createElement(MAvatar, { name: s.member, size: 'md' }),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, React.createElement('span', { className: 'truncate' }, s.member)),
            React.createElement('div', { className: 'meta' }, s.formule + ' · ' + s.renew)),
          React.createElement('div', { className: 'trail' },
            React.createElement(MChip, { tone: s.tone }, s.status),
            s.auto ? React.createElement('span', { className: 'gx-m-tag', style: { fontSize: 9 } }, 'Auto') : null)))),

      tab === 'formules' && React.createElement(MCard, { style: { marginTop: 12 } },
        A.formules.map((f, i) => React.createElement('div', { className: 'gx-m-formule' + (f.featured ? ' featured' : ''), key: i },
          React.createElement('div', { className: 'top' },
            React.createElement('span', { className: 'nm' }, f.name),
            React.createElement('span', { className: 'price' }, f.price, React.createElement('span', null, ' ' + f.unit))),
          React.createElement('div', { className: 'desc' }, f.desc),
          React.createElement('div', { className: 'foot' },
            React.createElement(Icon, { name: 'refresh', size: 14 }), f.billing,
            React.createElement('span', { className: 'members' }, f.members + ' membres'))))),

      tab === 'encaiss' && React.createElement(MCard, { style: { marginTop: 12 } },
        A.encaissements.map((e, i) => React.createElement('div', { className: 'gx-m-row', key: i, style: { cursor: 'default' } },
          React.createElement('span', { className: 'gx-m-ic ' + (e.tone === 'danger' ? 't-danger' : 't-success'), style: { width: 36, height: 36 } },
            React.createElement(Icon, { name: e.tone === 'danger' ? 'alert' : 'check', size: 17 })),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, React.createElement('span', { className: 'truncate' }, e.member)),
            React.createElement('div', { className: 'meta' }, e.date + ' · ' + e.method)),
          React.createElement('div', { className: 'trail' },
            React.createElement('span', { className: 'amt' }, e.amount),
            React.createElement(MChip, { tone: e.tone }, e.status)))))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenAbos = MScreenAbos;
