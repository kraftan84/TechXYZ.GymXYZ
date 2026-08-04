/* ============================================================
   GymXYZ Mobile — Administration (super-admin)
   The studio's own GymXYZ subscription, payment method & invoices.
   Mirrors the desktop "Administration" screen (white-label theme
   lives in Tweaks, not surfaced here).
   ============================================================ */
function MScreenAdministration({ theme, onNavigate, onOpenPlus }) {
  const f = window.GX_DATA.reglages.facturation;

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement(MPageTitle, { title: 'Administration', sub: 'Votre compte ' + theme.name + ' chez GymXYZ' }),

      // subscription hero
      React.createElement(MSection, { title: 'Abonnement GymXYZ' }),
      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-flex', style: { padding: 16, gap: 14, alignItems: 'flex-start' } },
          React.createElement('span', { className: 'gx-m-ic t-brand lg' }, React.createElement(Icon, { name: 'zap', size: 26 })),
          React.createElement('div', { className: 'gx-m-grow' },
            React.createElement('div', { style: { display: 'flex', alignItems: 'center', gap: 8 } },
              React.createElement('span', { style: { fontSize: 'var(--text-md)', fontWeight: 700, color: 'var(--text-strong)' } }, f.plan),
              React.createElement(MChip, { tone: 'success', icon: 'check' }, 'Actif')),
            React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 3 } }, f.planDesc)),
          React.createElement('div', { style: { flex: 'none', textAlign: 'right' } },
            React.createElement('div', { style: { fontSize: 'var(--text-2xl)', fontWeight: 700, color: 'var(--text-strong)', lineHeight: 1 } }, f.price),
            React.createElement('div', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)' } }, f.unit))),
        React.createElement('div', { style: { padding: '0 16px 16px' } },
          React.createElement('div', { style: { display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginBottom: 6, fontWeight: 600 } },
            React.createElement('span', null, 'Membres actifs'),
            React.createElement('b', { style: { color: 'var(--text-strong)' } }, f.members + ' / ' + f.membersCap)),
          React.createElement(MBar, { pct: 42 }),
          React.createElement('div', { style: { display: 'flex', alignItems: 'center', gap: 6, marginTop: 12, fontSize: 'var(--text-xs)', color: 'var(--text-muted)' } },
            React.createElement(Icon, { name: 'refresh', size: 13 }), 'Renouvellement le ' + f.renew))),

      React.createElement('button', { className: 'gx-m-cta ghost', style: { marginTop: 12 } },
        React.createElement(Icon, { name: 'layers', size: 18 }), 'Changer de formule'),

      // payment method
      React.createElement(MSection, { title: 'Moyen de paiement' }),
      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-flex', style: { padding: 16, gap: 14 } },
          React.createElement('span', { className: 'gx-m-ic t-neutral' }, React.createElement(Icon, { name: 'card', size: 21 })),
          React.createElement('div', { className: 'gx-m-grow' },
            React.createElement('div', { style: { fontSize: 'var(--text-sm)', fontWeight: 700, color: 'var(--text-strong)' } }, f.card.brand + ' ···· ' + f.card.last),
            React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 2 } }, 'Expire ' + f.card.exp)),
          React.createElement('span', { className: 'gx-m-tag' }, 'Modifier'))),

      // invoices
      React.createElement(MSection, { title: 'Factures' }),
      React.createElement(MCard, null,
        f.factures.map((fa, i) => React.createElement('div', { className: 'gx-m-row', key: i, style: { cursor: 'default' } },
          React.createElement('span', { className: 'gx-m-ic t-neutral', style: { width: 36, height: 36 } }, React.createElement(Icon, { name: 'file', size: 17 })),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, fa.ref),
            React.createElement('div', { className: 'meta' }, fa.date)),
          React.createElement('div', { className: 'trail' },
            React.createElement('span', { className: 'amt' }, fa.amount),
            React.createElement(MChip, { tone: fa.tone }, fa.status)),
          React.createElement('span', { className: 'chev' }, React.createElement(Icon, { name: 'download', size: 18 }))))),

      React.createElement('div', { className: 'gx-m-note', style: { textAlign: 'center', marginTop: 8 } }, 'GymXYZ · version mobile · démo')));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenAdministration = MScreenAdministration;
