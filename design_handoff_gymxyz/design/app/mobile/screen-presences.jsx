/* ============================================================
   GymXYZ Mobile — Présences (pointage + assiduité)
   List of sessions → tap → roster with segmented check-in.
   ============================================================ */
const MPRES_STATUS = [
  { id: 'present', icon: 'check', cls: 'present' },
  { id: 'late', icon: 'clock', cls: 'late' },
  { id: 'absent', icon: 'x', cls: 'absent' },
];

function MRosterRow({ person }) {
  const [st, setSt] = React.useState(person.status === 'pending' ? null : person.status);
  return React.createElement('div', { className: 'gx-m-roster' + (st === 'absent' ? ' absent' : '') },
    React.createElement(MAvatarMini, { name: person.name }),
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 'nm' }, person.name),
      React.createElement('div', { className: 'pl' }, person.plan)),
    React.createElement('div', { className: 'gx-m-seg' },
      MPRES_STATUS.map((o) =>
        React.createElement('button', { key: o.id, className: (st === o.id ? 'on ' + o.cls : ''), onClick: () => setSt(o.id), 'aria-label': o.id },
          React.createElement(Icon, { name: o.icon, size: 17 })))));
}

function MAvatarMini({ name }) {
  const { Avatar } = window.TechXYZDesignSystem_ff9a8f;
  return React.createElement(Avatar, { name, size: 'sm' });
}

function MPresRoster({ session, theme, onBack }) {
  const present = session.roster.filter((r) => r.status === 'present' || r.status === 'late').length;
  const body = React.createElement('div', { className: 'gx-m-body', style: { paddingBottom: 92 } },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement('div', { className: 'gx-m-flex', style: { gap: 13, padding: '4px 2px 14px' } },
        React.createElement(MIcTile, { name: session.icon, tone: session.itone, lg: true }),
        React.createElement('div', { className: 'gx-m-grow' },
          React.createElement('div', { style: { fontSize: 'var(--text-lg)', fontWeight: 700, color: 'var(--text-strong)' } }, session.course),
          React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 2 } },
            session.day + ' · ' + session.time + ' · ' + session.studio),
          React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 1 } }, session.coach))),
      React.createElement(MCard, null,
        React.createElement(MCardHead, { title: 'Feuille de présence', right: session.roster.length + ' inscrits' }),
        session.roster.map((p, i) => React.createElement(MRosterRow, { key: i, person: p }))),
      React.createElement('div', { className: 'gx-m-note' },
        'Pointez chaque membre : présent, en retard ou absent. Les absences répétées remontent dans l\'assiduité.')),
    // sticky validate footer
    React.createElement('div', { className: 'gx-m-stickyfoot' },
      React.createElement('div', { className: 'info' },
        React.createElement('b', null, present + '/' + session.roster.length), ' pointés'),
      React.createElement('button', { className: 'gx-m-cta', style: { width: 'auto', padding: '0 22px', height: 46 }, onClick: onBack },
        React.createElement(Icon, { name: 'check', size: 18 }), 'Valider')));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'sub', title: session.course, onBack }),
    body);
}

function MScreenPresences({ theme, onNavigate, onOpenPlus }) {
  const P = window.GX_DATA.presences;
  const [open, setOpen] = React.useState(null);
  if (open) return React.createElement(MPresRoster, { session: open, theme, onBack: () => setOpen(null) });

  const todo = P.sessions.filter((s) => s.status === 'todo' || s.status === 'live');
  const done = P.sessions.filter((s) => s.status === 'done');

  const sessRow = (s) => {
    const present = s.roster.filter((r) => r.status === 'present' || r.status === 'late').length;
    const tag = s.status === 'live' ? React.createElement(MChip, { tone: 'danger', solid: true }, 'En cours')
      : s.status === 'todo' ? React.createElement(MChip, { tone: 'warning' }, 'À pointer')
      : React.createElement(MChip, { tone: 'success', icon: 'check' }, present + '/' + s.roster.length);
    return React.createElement('div', { className: 'gx-m-sess', key: s.id, onClick: () => setOpen(s) },
      React.createElement(MIcTile, { name: s.icon, tone: s.itone }),
      React.createElement('div', { className: 'main' },
        React.createElement('div', { className: 'nm' }, s.course),
        React.createElement('div', { className: 'meta' }, s.time + ' · ' + s.studio + ' · ' + s.coach)),
      React.createElement('div', { style: { flex: 'none' } }, tag));
  };

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement(MPageTitle, { title: 'Présences', sub: P.date }),

      React.createElement('div', { className: 'gx-m-pres-stat', style: { marginTop: 16 } },
        React.createElement('div', { className: 'it' }, React.createElement('div', { className: 'v' }, P.kpis.taux), React.createElement('div', { className: 'l' }, 'Assiduité')),
        React.createElement('div', { className: 'it' }, React.createElement('div', { className: 'v', style: { color: 'var(--color-warning)' } }, P.kpis.pointer), React.createElement('div', { className: 'l' }, 'À pointer')),
        React.createElement('div', { className: 'it' }, React.createElement('div', { className: 'v' }, P.kpis.presents), React.createElement('div', { className: 'l' }, 'Présents')))),

    React.createElement(MSection, { title: "À pointer" }),
    React.createElement(MCard, null, todo.map(sessRow)),

    React.createElement(MSection, { title: 'Taux par cours' }),
    React.createElement(MCard, null,
      P.parCours.map((c, i) => React.createElement('div', { className: 'gx-m-statbar', key: i },
        React.createElement('span', { className: 'nm' }, c.name),
        React.createElement(MBar, { pct: c.pct, tone: c.pct >= 90 ? 'ok' : '' }),
        React.createElement('span', { className: 'pct' }, c.pct + '%')))),

    React.createElement(MSection, { title: 'Déjà pointé' }),
    React.createElement(MCard, null, done.map(sessRow)),

    React.createElement(MSection, { title: 'Assiduité faible' }),
    React.createElement(MCard, null,
      P.absents.map((a, i) => React.createElement('div', { className: 'gx-m-row', key: i },
        React.createElement(MAvatarMini, { name: a.name }),
        React.createElement('div', { className: 'main' },
          React.createElement('div', { className: 'nm' }, a.name),
          React.createElement('div', { className: 'meta' }, a.miss + ' absences / ' + a.total + ' · dernière venue ' + a.last)),
        React.createElement('div', { className: 'trail' }, React.createElement(MChip, { tone: 'danger' }, a.miss + ' abs.'))))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenPresences = MScreenPresences;
window.MAvatarMini = MAvatarMini;
