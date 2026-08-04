/* ============================================================
   GymXYZ Mobile — Membres (list + fiche)
   ============================================================ */
function MMemberFiche({ member, theme, onBack, onNavigate }) {
  const F = window.GX_DATA.fiche;
  const planTone = member.planTone === 'brand' ? 'brand' : 'neutral';
  const actRow = (item) => React.createElement('div', { className: 'gx-m-row', key: item.name + item.when, style: { cursor: 'default' } },
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 'nm' }, item.name),
      React.createElement('div', { className: 'meta' }, item.when)),
    React.createElement('div', { className: 'trail' }, React.createElement(MChip, { tone: item.tone }, item.st)));

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement('div', { className: 'gx-m-fiche-hero' },
        React.createElement(MAvatar, { name: member.name, size: 'xl' }),
        React.createElement('div', null,
          React.createElement('div', { className: 'nm' }, member.name),
          React.createElement('div', { className: 'rl' }, member.plan + ' · depuis ' + member.since)),
        React.createElement('div', { style: { display: 'flex', gap: 8 } },
          React.createElement(MChip, { tone: member.tone, solid: member.tone === 'success' ? false : true }, member.status),
          React.createElement(MChip, { tone: 'neutral' }, 'Assiduité ' + member.presence)),
        React.createElement('div', { className: 'gx-m-fiche-actions' },
          [['phone', 'Appeler'], ['mail', 'E-mail'], ['card', 'Abonnement'], ['history', 'Historique']].map((a) =>
            React.createElement('div', { key: a[0], className: 'gx-m-circbtn', onClick: () => a[0] === 'card' ? onNavigate('abos') : null },
              React.createElement('span', { className: 'b' }, React.createElement(Icon, { name: a[0], size: 20 })),
              React.createElement('span', { className: 'cl' }, a[1]))))),

      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Téléphone'), React.createElement('span', { className: 'v' }, member.phone)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'E-mail'), React.createElement('span', { className: 'v', style: { fontWeight: 600, fontSize: 'var(--text-xs)' } }, member.email)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Formule'),
          React.createElement('span', { className: 'v' }, React.createElement(MChip, { tone: planTone }, member.gaugeLabel + (member.gaugeLabel === '∞' ? '' : ' restantes'))))),

      React.createElement(MSection, { title: 'Prochains cours' }),
      React.createElement(MCard, null, F.upcoming.map(actRow)),

      React.createElement(MSection, { title: 'Derniers passages' }),
      React.createElement(MCard, null, F.recent.map(actRow)),

      React.createElement(MSection, { title: 'Paiements' }),
      React.createElement(MCard, null,
        F.payments.map((p, i) => React.createElement('div', { className: 'gx-m-row', key: i, style: { cursor: 'default' } },
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, p.label),
            React.createElement('div', { className: 'meta' }, p.date)),
          React.createElement('div', { className: 'trail' },
            React.createElement('span', { className: 'amt' }, p.amt),
            React.createElement(MChip, { tone: p.tone }, p.st)))))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'sub', title: member.name.split(' ')[0], onBack }),
    body);
}

function MScreenMembres({ theme, onNavigate, onOpenPlus }) {
  const D = window.GX_DATA;
  const [open, setOpen] = React.useState(null);
  const [filter, setFilter] = React.useState('tous');
  if (open) return React.createElement(MMemberFiche, { member: open, theme, onBack: () => setOpen(null), onNavigate });

  const filters = [
    { id: 'tous', label: 'Tous', count: D.members.length },
    { id: 'success', label: 'Actifs' },
    { id: 'warning', label: 'À relancer' },
    { id: 'danger', label: 'Inactifs' },
  ];
  const list = D.members.filter((m) => filter === 'tous' || m.tone === filter);

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement(MPageTitle, { title: 'Membres', sub: D.members.length + ' membres · ' + D.city,
        action: React.createElement('button', { className: 'gx-m-iconbtn', 'aria-label': 'Ajouter un membre' }, React.createElement(Icon, { name: 'plus', size: 20 })) }),
      React.createElement('div', { className: 'gx-m-search' }, React.createElement(Icon, { name: 'search', size: 18 }), 'Rechercher un membre…'),

      React.createElement('div', { className: 'gx-m-datebar', style: { padding: '12px 0 2px', gap: 8 } },
        filters.map((f) => React.createElement('button', {
          key: f.id, onClick: () => setFilter(f.id),
          className: 'gx-m-tag' + (filter === f.id ? ' brand' : ''),
          style: { padding: '7px 13px', fontSize: 'var(--text-xs)', cursor: 'pointer', flex: 'none', fontWeight: 700,
            border: '1px solid ' + (filter === f.id ? 'var(--azure-200)' : 'var(--border-subtle)'),
            background: filter === f.id ? 'var(--azure-50)' : 'var(--surface-card)',
            color: filter === f.id ? 'var(--azure-800)' : 'var(--text-body)' },
        }, f.label + (f.count ? ' · ' + f.count : '')))),

      React.createElement(MCard, { style: { marginTop: 12 } },
        list.map((m) => React.createElement('button', { className: 'gx-m-row', key: m.id, onClick: () => setOpen(m) },
          React.createElement(MAvatar, { name: m.name, size: 'md' }),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, React.createElement('span', { className: 'truncate' }, m.name)),
            React.createElement('div', { className: 'meta' }, m.plan + ' · vu ' + m.last)),
          React.createElement('div', { className: 'trail' },
            React.createElement(MChip, { tone: m.tone }, m.status)),
          React.createElement('span', { className: 'chev' }, React.createElement(Icon, { name: 'chevR', size: 18 })))))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenMembres = MScreenMembres;
