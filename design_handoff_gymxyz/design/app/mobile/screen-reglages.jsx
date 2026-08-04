/* ============================================================
   GymXYZ Mobile — Réglages (grouped settings + sub-panels)
   ============================================================ */
const { Switch: MRSwitch } = window.TechXYZDesignSystem_ff9a8f;

function MReglagesPanel({ panel, theme, onBack }) {
  const R = window.GX_DATA.reglages;
  let title = '', content = null;

  if (panel === 'identite') {
    title = 'Identité de la salle';
    const id = R.identite;
    content = React.createElement(React.Fragment, null,
      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Adresse'), React.createElement('span', { className: 'v' }, id.address)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Ville'), React.createElement('span', { className: 'v' }, id.zip + ' ' + id.city)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Téléphone'), React.createElement('span', { className: 'v' }, id.phone)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'E-mail'), React.createElement('span', { className: 'v', style: { fontSize: 'var(--text-xs)' } }, id.email)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'SIRET'), React.createElement('span', { className: 'v' }, id.siret))),
      React.createElement(MSection, { title: 'Horaires d\'ouverture' }),
      React.createElement(MCard, null,
        id.hours.map((h, i) => React.createElement('div', { className: 'gx-m-kv', key: i },
          React.createElement('span', { className: 'k', style: { textTransform: 'none', letterSpacing: 0, fontSize: 'var(--text-sm)', color: 'var(--text-strong)' } }, h[0]),
          React.createElement('span', { className: 'v', style: { fontVariantNumeric: 'tabular-nums' } }, h[1])))));
  } else if (panel === 'equipe') {
    title = 'Équipe & accès';
    content = React.createElement(React.Fragment, null,
      React.createElement(MCard, null,
        R.team.map((t, i) => React.createElement('div', { className: 'gx-m-row', key: i, style: { cursor: 'default' } },
          React.createElement(MAvatar, { name: t.name, size: 'md' }),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, t.name),
            React.createElement('div', { className: 'meta' }, t.access + ' · ' + t.last)),
          React.createElement('div', { className: 'trail' }, React.createElement(MChip, { tone: t.roleTone }, t.role))))),
      R.invites.length ? React.createElement(React.Fragment, null,
        React.createElement(MSection, { title: 'Invitations en attente' }),
        React.createElement(MCard, null, R.invites.map((iv, i) => React.createElement('div', { className: 'gx-m-row', key: i, style: { cursor: 'default' } },
          React.createElement('span', { className: 'gx-m-ic t-warning', style: { width: 36, height: 36 } }, React.createElement(Icon, { name: 'mail', size: 17 })),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm', style: { fontSize: 'var(--text-xs)', fontWeight: 600 } }, iv.email),
            React.createElement('div', { className: 'meta' }, iv.role + ' · ' + iv.sent)),
          React.createElement('div', { className: 'trail' }, React.createElement(MChip, { tone: 'warning' }, 'Envoyée')))))) : null);
  } else if (panel === 'tarifs') {
    title = 'Formules & tarifs';
    const F = window.GX_DATA.abos.formules;
    content = React.createElement(React.Fragment, null,
      React.createElement(MSection, { title: 'Formules d\'abonnement' }),
      React.createElement(MCard, null,
        F.map((fo, i) => React.createElement('div', { className: 'gx-m-row', key: i, style: { cursor: 'default' } },
          React.createElement('span', { className: 'gx-m-ic t-' + (fo.tone === 'brand' ? 'brand' : fo.tone), style: { width: 38, height: 38 } }, React.createElement(Icon, { name: 'card', size: 18 })),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, fo.name),
            React.createElement('div', { className: 'meta' }, fo.members + ' membres · ' + fo.billing)),
          React.createElement('div', { className: 'trail' }, React.createElement('span', { className: 'amt' }, fo.price + ' ', React.createElement('span', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)', fontWeight: 600 } }, fo.unit)))))),
      React.createElement(MSection, { title: 'Paiement & facturation' }),
      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Devise'), React.createElement('span', { className: 'v' }, R.paiements.devise)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k', style: { flex: 'none' } }, 'TVA'), React.createElement('span', { className: 'v', style: { fontSize: 'var(--text-2xs)', maxWidth: '64%' } }, R.paiements.tva))),
      React.createElement(MSection, { title: 'Moyens de paiement' }),
      React.createElement(MCard, null,
        R.paiements.moyens.map((m, i) => React.createElement(MPayRow, { key: i, label: m[0], on: m[1] }))));
  } else if (panel === 'notifs') {
    title = 'Notifications';
    content = R.notifs.map((g, gi) => React.createElement(React.Fragment, { key: gi },
      React.createElement(MSection, { title: g.group }),
      React.createElement(MCard, null,
        g.items.map((it, i) => React.createElement(MNotifRow, { key: i, item: it })))));
  }

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'sub', title, onBack }),
    React.createElement('div', { className: 'gx-m-body' }, React.createElement('div', { className: 'gx-m-screen' }, content)));
}

function MPayRow({ label, on }) {
  const [v, setV] = React.useState(on);
  return React.createElement('div', { className: 'gx-m-flex', style: { padding: '13px 16px', borderBottom: '1px solid var(--border-subtle)' } },
    React.createElement('span', { className: 'gx-m-grow', style: { fontSize: 'var(--text-sm)', fontWeight: 600, color: 'var(--text-strong)' } }, label),
    React.createElement(MRSwitch, { checked: v, onChange: setV }));
}
function MNotifRow({ item }) {
  const [v, setV] = React.useState(item.on);
  return React.createElement('div', { className: 'gx-m-notif' },
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 't' }, item.t),
      React.createElement('div', { className: 'd' }, item.d),
      React.createElement('div', { className: 'gx-m-chan' }, item.chan.map((c, i) => React.createElement('span', { className: 'c', key: i }, c)))),
    React.createElement('div', { style: { flex: 'none', paddingTop: 2 } }, React.createElement(MRSwitch, { checked: v, onChange: setV })));
}

function MScreenReglages({ theme, onNavigate, onOpenPlus }) {
  const [panel, setPanel] = React.useState(null);
  if (panel) return React.createElement(MReglagesPanel, { panel, theme, onBack: () => setPanel(null) });
  const R = window.GX_DATA.reglages;
  const m = theme.manager || window.GX_DATA.manager;

  const rows = [
    { id: 'identite', icon: 'building', t: 'Identité de la salle', d: R.identite.address + ' · ' + R.identite.city },
    { id: 'equipe', icon: 'users', t: 'Équipe & accès', d: R.team.length + ' membres de l\'équipe' },
    { id: 'tarifs', icon: 'euro', t: 'Formules & tarifs', d: window.GX_DATA.abos.formules.length + ' formules · ' + R.paiements.devise },
    { id: 'notifs', icon: 'bell', t: 'Notifications', d: 'Relances, rappels, alertes' },
  ];

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement(MPageTitle, { title: 'Réglages' }),

      // profile
      React.createElement(MCard, { style: { marginTop: 14 } },
        React.createElement('div', { className: 'gx-m-flex', style: { padding: 16, gap: 13 } },
          React.createElement(MAvatar, { name: m.name, size: 'lg' }),
          React.createElement('div', { className: 'gx-m-grow' },
            React.createElement('div', { style: { fontSize: 'var(--text-md)', fontWeight: 700, color: 'var(--text-strong)' } }, m.name),
            React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 1 } }, m.role + ' · ' + theme.name)),
          React.createElement('span', { className: 'gx-m-tag brand' }, 'Compte'))),

      // settings groups
      React.createElement(MSection, { title: 'Configuration' }),
      React.createElement(MCard, null,
        rows.map((r) => React.createElement('button', { className: 'gx-m-setrow', key: r.id, onClick: () => setPanel(r.id) },
          React.createElement('span', { className: 'si' }, React.createElement(Icon, { name: r.icon, size: 18 })),
          React.createElement('span', { className: 'main' },
            React.createElement('span', { className: 't' }, r.t),
            React.createElement('span', { className: 'd' }, r.d)),
          React.createElement('span', { className: 'chev' }, React.createElement(Icon, { name: 'chevR', size: 18 }))))),

      React.createElement('div', { className: 'gx-m-note', style: { textAlign: 'center', marginTop: 8 } }, 'GymXYZ · version mobile · démo')));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenReglages = MScreenReglages;
