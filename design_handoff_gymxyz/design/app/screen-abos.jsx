/* ============================================================
   GymXYZ — Screen: Abonnements (formules + suivi + encaissements)
   ============================================================ */
const { Button: AbBtn, Card: AbCard, Input: AbInput, Select: AbSelect, Switch: AbSwitch, Avatar: AbAvatar } = window.TechXYZDesignSystem_ff9a8f;

/* ---------- formule tile ---------- */
function FormuleTile({ f, onOpen }) {
  return React.createElement('div', { className: 'gx-formule' + (f.featured ? ' featured' : ''), onClick: () => onOpen(f) },
    React.createElement('div', { className: 'nm' }, f.name),
    React.createElement('div', { className: 'price' },
      React.createElement('span', { className: 'amt' }, f.price),
      React.createElement('span', { className: 'unit' }, f.unit)),
    React.createElement('div', { className: 'desc' }, f.desc),
    React.createElement('div', { className: 'foot' },
      React.createElement(Icon, { name: 'users', size: 15 }),
      React.createElement('b', null, f.members + ' membres'),
      React.createElement('span', { className: 'sep' }, f.billing)));
}

/* ---------- subscription row ---------- */
function SubRow({ s, onAction }) {
  const barTone = s.tone === 'danger' ? 'bad' : s.tone === 'warning' ? 'warn' : '';
  let echeance;
  if (s.daysLeft === null) echeance = React.createElement('span', { className: 'gx-mini' }, React.createElement(Icon, { name: 'card', size: 13 }), s.renew);
  else if (s.daysLeft < 0) echeance = React.createElement('span', { className: 'gx-mini', style: { color: 'var(--danger-700)' } }, React.createElement(Icon, { name: 'alert', size: 13 }), s.renew);
  else echeance = React.createElement('span', { className: 'gx-mini' }, React.createElement(Icon, { name: 'calendar', size: 13 }), s.renew + (s.daysLeft <= 30 && s.daysLeft >= 0 ? ' · J-' + s.daysLeft : ''));

  let action;
  if (s.tone === 'danger') action = React.createElement(AbBtn, { variant: 'primary', size: 'sm', iconLeft: React.createElement(Icon, { name: 'euro', size: 15 }), onClick: (e) => { e.stopPropagation(); onAction('pay', s); } }, 'Encaisser');
  else if (s.tone === 'warning') action = React.createElement(AbBtn, { variant: 'outline', size: 'sm', iconLeft: React.createElement(Icon, { name: 'refresh', size: 15 }), onClick: (e) => { e.stopPropagation(); onAction('pay', s); } }, 'Renouveler');
  else action = React.createElement(IconButton, { label: 'Options', variant: 'ghost', size: 'sm', onClick: (e) => { e.stopPropagation(); onAction('pay', s); } }, React.createElement(Icon, { name: 'settings', size: 17 }));

  return React.createElement('tr', { onClick: () => onAction('pay', s) },
    React.createElement('td', null, React.createElement('div', { className: 'u' },
      React.createElement(AbAvatar, { name: s.member, size: 'md' }),
      React.createElement('div', null, React.createElement('div', { className: 'nm' }, s.member)))),
    React.createElement('td', null,
      React.createElement('div', { style: { fontSize: 'var(--text-sm)', color: 'var(--text-body)' } }, s.formule),
      s.auto && React.createElement('div', { className: 'gx-mini', style: { marginTop: 3 } }, React.createElement(Icon, { name: 'refresh', size: 12 }), 'Renouvellement auto')),
    React.createElement('td', null,
      echeance,
      React.createElement('div', { style: { marginTop: 6 } }, React.createElement(Bar, { pct: s.pct, tone: barTone, width: 110 }))),
    React.createElement('td', null, React.createElement('b', { style: { color: 'var(--text-strong)', fontVariantNumeric: 'tabular-nums' } }, s.amount)),
    React.createElement('td', null, React.createElement(Chip, { tone: s.tone, icon: s.tone === 'success' ? 'check' : null }, s.status)),
    React.createElement('td', { style: { textAlign: 'right' } }, action));
}

/* ---------- list ---------- */
function AbosList({ onAction }) {
  const A = window.GX_DATA.abos;
  const [filter, setFilter] = React.useState('all');
  const counts = {
    all: A.subs.length,
    active: A.subs.filter((s) => s.tone === 'success').length,
    expire: A.subs.filter((s) => s.tone === 'warning').length,
    late: A.subs.filter((s) => s.tone === 'danger').length,
  };
  const list = filter === 'active' ? A.subs.filter((s) => s.tone === 'success')
    : filter === 'expire' ? A.subs.filter((s) => s.tone === 'warning')
      : filter === 'late' ? A.subs.filter((s) => s.tone === 'danger')
        : A.subs;

  const totalMembers = A.formules.reduce((n, f) => n + f.members, 0);

  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: ['Accueil', 'Abonnements'] }),
    React.createElement(PageHead, { title: 'Abonnements', sub: A.kpis.active + ' abonnements actifs · ' + A.kpis.expiring + ' expirent cette semaine' },
      React.createElement(AbBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'download', size: 18 }) }, 'Exporter'),
      React.createElement(AbBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'plus', size: 18 }), onClick: () => onAction('formule', null) }, 'Nouvelle formule')),

    React.createElement('div', { className: 'gx-grid4', style: { marginBottom: 18 } },
      React.createElement(Kpi, { label: 'Revenu mensuel récurrent', value: A.kpis.mrr, delta: A.kpis.mrrDelta, deltaIcon: 'trend', spark: true }),
      React.createElement(Kpi, { label: 'Abonnements actifs', value: String(A.kpis.active), sub: 'sur 128 membres' }),
      React.createElement(Kpi, { label: 'Expirent (7 j)', value: String(A.kpis.expiring), sub: 'à relancer', valueColor: 'var(--warning-600)' }),
      React.createElement(Kpi, { label: 'Paiements en retard', value: String(A.kpis.late), sub: A.kpis.lateAmt + ' à encaisser', valueColor: 'var(--color-danger)' })),

    // formules
    React.createElement(AbCard, { padding: '0', style: { marginBottom: 18 } },
      React.createElement(CardHead, { title: 'Formules' },
        React.createElement(AbBtn, { variant: 'ghost', size: 'sm', iconLeft: React.createElement(Icon, { name: 'plus', size: 16 }), onClick: () => onAction('formule', null) }, 'Ajouter')),
      React.createElement('div', { style: { padding: 18 } },
        React.createElement('div', { className: 'gx-formules' },
          A.formules.map((f) => React.createElement(FormuleTile, { key: f.id, f, onOpen: (x) => onAction('formule', x) }))))),

    // filters
    React.createElement('div', { className: 'gx-filters' },
      React.createElement('span', { className: 'gx-fchip' + (filter === 'all' ? ' on' : ''), onClick: () => setFilter('all') }, 'Tous · ' + counts.all),
      React.createElement('span', { className: 'gx-fchip' + (filter === 'active' ? ' on' : ''), onClick: () => setFilter('active') }, 'Actifs · ' + counts.active),
      React.createElement('span', { className: 'gx-fchip warn' + (filter === 'expire' ? ' on' : ''), onClick: () => setFilter('expire') }, 'Expire bientôt · ' + counts.expire),
      React.createElement('span', { className: 'gx-fchip' + (filter === 'late' ? ' on' : ''), onClick: () => setFilter('late') }, 'En retard · ' + counts.late),
      React.createElement('span', { className: 'gx-fchip', style: { marginLeft: 'auto' } }, React.createElement(Icon, { name: 'filter', size: 14 }), 'Trier : échéance')),

    // subscriptions table — full width so the action column never clips
    React.createElement(AbCard, { padding: '6px 10px 2px', style: { marginBottom: 18 } },
      React.createElement('div', { className: 'gx-tbl-scroll' },
        React.createElement('table', { className: 'gx-tbl gx-tbl-abos' },
          React.createElement('thead', null, React.createElement('tr', null,
            ['Membre', 'Formule', 'Échéance', 'Montant', 'Statut', ''].map((h, i) => React.createElement('th', { key: i }, h)))),
          React.createElement('tbody', null,
            list.map((s, i) => React.createElement(SubRow, { key: i, s, onAction })))))),

    React.createElement('div', { className: 'gx-grid2', style: { alignItems: 'start' } },
      // encaissements + répartition, side by side below the table
      React.createElement('div', { className: 'gx-col' },
        React.createElement(AbCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Encaissements récents' },
            React.createElement('span', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)' } }, '7 derniers jours')),
          React.createElement('div', { style: { padding: '4px 18px 10px' } },
            A.encaissements.map((e, i) => React.createElement('div', { className: 'gx-listrow', key: i },
              React.createElement('div', { className: 'main' },
                React.createElement('div', { className: 't' }, e.member),
                React.createElement('div', { className: 's' }, e.date + ' · ' + e.label + ' · ' + e.method)),
              React.createElement('b', { className: 'amt', style: e.tone === 'danger' ? { color: 'var(--danger-600)' } : null }, e.amount),
              React.createElement(Chip, { tone: e.tone }, e.status)))))),
      React.createElement('div', { className: 'gx-col' },
        React.createElement(AbCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Répartition des membres' }),
          React.createElement('div', { style: { padding: '8px 18px 16px' } },
            A.formules.map((f, i) => React.createElement('div', { className: 'gx-statbar', key: i },
              React.createElement('span', { className: 'nm' }, f.name),
              React.createElement('div', { style: { flex: 1 } }, React.createElement(Bar, { pct: Math.round((f.members / totalMembers) * 100) })),
              React.createElement('span', { className: 'pct' }, f.members))),
            React.createElement('div', { className: 'gx-diffuse-foot', style: { marginTop: 8 } },
              React.createElement(Icon, { name: 'wallet', size: 15 }),
              React.createElement('span', null, 'Panier moyen ', React.createElement('b', null, '44 € / mois'), ' par abonné actif.')))))));
}

/* ---------- drawer (encaisser / formule) ---------- */
function AbField({ label, value, icon, placeholder, options, onChange }) {
  const common = { label, value, onChange: onChange || (() => {}), style: { width: '100%' } };
  if (options) return React.createElement(AbSelect, { ...common, options });
  return React.createElement(AbInput, { ...common, placeholder, iconLeft: icon ? React.createElement(Icon, { name: icon, size: 18 }) : null });
}
function AbFieldRow({ children }) {
  return React.createElement('div', { style: { display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 14, marginBottom: 14 } }, children);
}

function AboDrawer({ mode, data, onClose }) {
  const isPay = mode === 'pay';
  const isNewFormule = mode === 'formule' && !data;
  const [f, setF] = React.useState(isPay
    ? { member: data ? data.member : '', formule: data ? data.formule : 'Illimité mensuel', amount: data ? data.amount.split(' ')[0] : '49', method: 'cb', date: "9 juin 2026", receipt: true }
    : { name: data ? data.name : '', price: data ? data.price : '', period: 'mois', desc: data ? data.desc : '', commit: data ? data.billing !== 'Sans engagement' : false });
  const set = (k) => (v) => setF({ ...f, [k]: v });

  const title = isPay ? 'Encaisser un paiement' : (isNewFormule ? 'Nouvelle formule' : 'Modifier la formule');
  const sub = isPay ? (data ? data.member + ' · ' + data.formule : 'Enregistrer un encaissement') : (isNewFormule ? 'Créer une offre vendable' : data.name);

  return React.createElement(React.Fragment, null,
    React.createElement('div', { className: 'gx-scrim', onClick: onClose }),
    React.createElement('aside', { className: 'gx-drawer' },
      React.createElement('div', { className: 'gx-drawer-h' },
        React.createElement('div', { className: 'ic' }, React.createElement(Icon, { name: isPay ? 'euro' : 'card', size: 20 })),
        React.createElement('div', { className: 'tt' },
          React.createElement('div', { className: 't' }, title),
          React.createElement('div', { className: 's' }, sub)),
        React.createElement(IconButton, { label: 'Fermer', variant: 'ghost', onClick: onClose }, React.createElement(Icon, { name: 'x', size: 20 }))),
      React.createElement('div', { className: 'gx-drawer-b' },
        isPay
          ? React.createElement(React.Fragment, null,
              React.createElement('div', { className: 'gx-seclab' }, 'Paiement'),
              React.createElement('div', { style: { marginBottom: 14 } }, React.createElement(AbField, { label: 'Membre', value: f.member, onChange: set('member'), icon: 'user', placeholder: 'Nom du membre' })),
              React.createElement('div', { style: { marginBottom: 14 } }, React.createElement(AbField, { label: 'Formule', value: f.formule, onChange: set('formule'), options: window.GX_DATA.abos.formules.map((x) => ({ value: x.name, label: x.name + ' — ' + x.price + ' ' + x.unit })) })),
              React.createElement(AbFieldRow, null,
                React.createElement(AbField, { label: 'Montant (€)', value: f.amount, onChange: set('amount'), icon: 'euro' }),
                React.createElement(AbField, { label: 'Date', value: f.date, onChange: set('date'), icon: 'calendar' })),
              React.createElement('div', { style: { marginBottom: 14 } }, React.createElement(AbField, {
                label: 'Mode de paiement', value: f.method, onChange: set('method'),
                options: [{ value: 'cb', label: 'Carte bancaire' }, { value: 'prelev', label: 'Prélèvement SEPA' }, { value: 'especes', label: 'Espèces' }, { value: 'cheque', label: 'Chèque' }],
              })),
              React.createElement('div', { style: { padding: '6px 0' } }, React.createElement(AbSwitch, { checked: f.receipt, onChange: set('receipt'), label: 'Envoyer le reçu par e-mail' })))
          : React.createElement(React.Fragment, null,
              React.createElement('div', { className: 'gx-seclab' }, 'Offre' ),
              React.createElement('div', { style: { marginBottom: 14 } }, React.createElement(AbField, { label: 'Nom de la formule', value: f.name, onChange: set('name'), placeholder: 'Illimité mensuel' })),
              React.createElement(AbFieldRow, null,
                React.createElement(AbField, { label: 'Prix (€)', value: f.price, onChange: set('price'), icon: 'euro', placeholder: '49' }),
                React.createElement(AbField, { label: 'Période', value: f.period, onChange: set('period'), options: [{ value: 'mois', label: 'par mois' }, { value: 'an', label: 'par an' }, { value: 'carte', label: 'par carte' }, { value: 'seance', label: 'par séance' }] })),
              React.createElement('div', { style: { marginBottom: 14 } }, React.createElement(AbField, { label: 'Description', value: f.desc, onChange: set('desc'), placeholder: 'Ce que la formule inclut…' })),
              React.createElement('div', { style: { padding: '6px 0' } }, React.createElement(AbSwitch, { checked: f.commit, onChange: set('commit'), label: 'Engagement sur la durée' })))),
      React.createElement('div', { className: 'gx-drawer-f' },
        !isPay && !isNewFormule && React.createElement(AbBtn, { variant: 'danger', iconLeft: React.createElement(Icon, { name: 'trash', size: 18 }) }, 'Supprimer'),
        React.createElement('span', { style: { marginLeft: 'auto', display: 'flex', gap: 10 } },
          React.createElement(AbBtn, { variant: 'ghost', onClick: onClose }, 'Annuler'),
          React.createElement(AbBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'check', size: 18 }), onClick: onClose }, isPay ? 'Encaisser' : (isNewFormule ? 'Créer la formule' : 'Enregistrer'))))));
}

function ScreenAbos() {
  const [drawer, setDrawer] = React.useState(null); // { mode, data } | null
  return React.createElement('div', { className: 'gx-screen' },
    React.createElement(AbosList, { onAction: (mode, data) => setDrawer({ mode, data }) }),
    drawer && React.createElement(AboDrawer, { mode: drawer.mode, data: drawer.data, onClose: () => setDrawer(null) }));
}

window.ScreenAbos = ScreenAbos;
