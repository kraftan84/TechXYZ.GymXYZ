/* ============================================================
   GymXYZ — Screen: Membres (list → fiche, + create/edit drawer)
   ============================================================ */
const { Button: MbBtn, Card: MbCard, Input: MbInput, Select: MbSelect, Switch: MbSwitch, Avatar: MbAvatar } = window.TechXYZDesignSystem_ff9a8f;

function ScreenMembres() {
  const [view, setView] = React.useState('list');
  const [member, setMember] = React.useState(null);
  const [drawer, setDrawer] = React.useState(null); // 'new' | 'edit' | null

  return React.createElement('div', { className: 'gx-screen' },
    view === 'list'
      ? React.createElement(MembresList, { onOpen: (m) => { setMember(m); setView('fiche'); }, onNew: () => setDrawer('new') })
      : React.createElement(FicheMembre, { member, onBack: () => setView('list'), onEdit: () => setDrawer('edit') }),
    drawer === 'new' && React.createElement(MemberDrawer, { mode: 'new', onClose: () => setDrawer(null) }),
    drawer === 'edit' && React.createElement(MemberDrawer, { mode: 'edit', member, onClose: () => setDrawer(null) }));
}

/* ---------- List ---------- */
function MembresList({ onOpen, onNew }) {
  const D = window.GX_DATA;
  const [filter, setFilter] = React.useState('all');
  const counts = { all: 128, active: 112, expire: 6, inactive: 10 };
  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: ['Accueil', 'Membres'] }),
    React.createElement(PageHead, { title: 'Membres', sub: '128 membres · 6 abonnements expirent cette semaine' },
      React.createElement('span', { className: 'gx-search', style: { width: 240, height: 40 } },
        React.createElement(Icon, { name: 'search', size: 17 }), React.createElement('span', null, 'Nom, e-mail, téléphone…')),
      React.createElement(MbBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'plus', size: 18 }), onClick: onNew }, 'Nouveau membre')),
    React.createElement('div', { className: 'gx-filters' },
      React.createElement('span', { className: 'gx-fchip' + (filter === 'all' ? ' on' : ''), onClick: () => setFilter('all') }, 'Tous · ' + counts.all),
      React.createElement('span', { className: 'gx-fchip' + (filter === 'active' ? ' on' : ''), onClick: () => setFilter('active') }, 'Actifs · ' + counts.active),
      React.createElement('span', { className: 'gx-fchip warn' + (filter === 'expire' ? ' on' : ''), onClick: () => setFilter('expire') }, 'Expire bientôt · ' + counts.expire),
      React.createElement('span', { className: 'gx-fchip' + (filter === 'inactive' ? ' on' : ''), onClick: () => setFilter('inactive') }, 'Inactifs · ' + counts.inactive),
      React.createElement('span', { className: 'gx-fchip', style: { marginLeft: 'auto' } }, React.createElement(Icon, { name: 'filter', size: 14 }), 'Trier : dernière visite')),
    React.createElement(MbCard, { padding: '6px 10px 2px' },
      React.createElement('table', { className: 'gx-tbl' },
        React.createElement('thead', null, React.createElement('tr', null,
          ['Membre', 'Abonnement', 'Présence', 'Dernière visite', 'Statut', ''].map((h, i) => React.createElement('th', { key: i }, h)))),
        React.createElement('tbody', null,
          D.members.map((m) => React.createElement('tr', { key: m.id, onClick: () => onOpen(m) },
            React.createElement('td', null, React.createElement('div', { className: 'u' },
              React.createElement(MbAvatar, { name: m.name, size: 'md' }),
              React.createElement('div', null,
                React.createElement('div', { className: 'nm' }, m.name),
                React.createElement('div', { className: 'em' }, m.email)))),
            React.createElement('td', null,
              React.createElement('div', { style: { fontSize: 'var(--text-sm)', color: 'var(--text-body)' } }, m.plan),
              React.createElement('div', { style: { display: 'flex', alignItems: 'center', gap: 8, marginTop: 5 } },
                React.createElement(Bar, { pct: m.gauge, tone: m.gauge <= 30 ? 'warn' : '', width: 90 }),
                React.createElement('span', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)' } }, m.gaugeLabel))),
            React.createElement('td', null, React.createElement('b', { style: { color: 'var(--text-strong)' } }, m.presence)),
            React.createElement('td', { style: { color: 'var(--text-muted)' } }, m.last),
            React.createElement('td', null, React.createElement(Chip, { tone: m.tone }, m.status)),
            React.createElement('td', { style: { textAlign: 'right', color: 'var(--text-subtle)' } }, React.createElement(Icon, { name: 'chevR', size: 16 }))))))));
}

/* ---------- Fiche ---------- */
function PersonRow({ name, sub, tone, st }) {
  return React.createElement('div', { className: 'gx-listrow' },
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 't' }, name),
      React.createElement('div', { className: 's' }, sub)),
    React.createElement(Chip, { tone }, st));
}

function FicheMembre({ member, onBack, onEdit }) {
  const m = member || window.GX_DATA.members[0];
  const F = window.GX_DATA.fiche;
  const stat = (lab, val, sub) => React.createElement('div', { className: 'gx-kpi' },
    React.createElement('div', { className: 'lab' }, lab),
    React.createElement('div', { className: 'val', style: { fontSize: 'var(--text-2xl)' } }, val),
    React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 3 } }, sub));

  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: [{ label: 'Accueil' }, { label: 'Membres', to: 'membres' }, m.name], onNav: onBack }),
    React.createElement('div', { className: 'gx-fiche-head' },
      React.createElement(MbAvatar, { name: m.name, size: 'xl' }),
      React.createElement('div', { className: 'info' },
        React.createElement('div', { className: 'name-row' },
          React.createElement('span', { className: 'name' }, m.name),
          React.createElement(Chip, { tone: m.tone, icon: m.tone === 'success' ? 'check' : null }, m.status)),
        React.createElement('div', { className: 'contact' }, `${m.email} · ${m.phone} · membre depuis ${m.since}`)),
      React.createElement(MbBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'mail', size: 18 }) }, 'Message'),
      React.createElement(MbBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'settings', size: 18 }), onClick: onEdit }, 'Éditer')),

    React.createElement('div', { className: 'gx-grid2', style: { gridTemplateColumns: '1fr 1fr', alignItems: 'start' } },
      // left column
      React.createElement('div', { className: 'gx-col' },
        React.createElement(MbCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Abonnement' }, React.createElement(Chip, { tone: m.planTone }, m.plan)),
          React.createElement('div', { style: { padding: 18, display: 'flex', alignItems: 'center', gap: 18 } },
            React.createElement(Ring, { pct: 64, label: '18 j', size: 88 }),
            React.createElement('div', { style: { flex: 1 } },
              React.createElement('div', { style: { fontSize: 'var(--text-sm)', color: 'var(--text-body)', marginBottom: 8 } }, 'Renouvelé le 1ᵉʳ juin · ', React.createElement('b', null, 'expire le 30 juin')),
              React.createElement('div', { style: { display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-xs)', color: 'var(--text-muted)' } },
                React.createElement('span', null, 'Accès illimité'), React.createElement('span', null, '49 €/mois')),
              React.createElement('div', { style: { marginTop: 12, display: 'flex', gap: 8 } },
                React.createElement(MbBtn, { variant: 'outline', size: 'sm' }, 'Renouveler'),
                React.createElement(MbBtn, { variant: 'ghost', size: 'sm' }, 'Historique'))))),
        React.createElement(MbCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Paiements' }),
          React.createElement('div', { style: { padding: '4px 18px 12px' } },
            F.payments.map((p, i) => React.createElement('div', { className: 'gx-listrow', key: i },
              React.createElement('div', { className: 'main' },
                React.createElement('div', { className: 't' }, p.label),
                React.createElement('div', { className: 's' }, p.date)),
              React.createElement('b', { className: 'amt' }, p.amt),
              React.createElement(Chip, { tone: p.tone }, p.st)))))),
      // right column
      React.createElement('div', { className: 'gx-col' },
        React.createElement('div', { className: 'gx-grid3' },
          stat('Séances', '142', "depuis l'inscription"),
          stat('Présence', m.presence, '+5 pts ce mois'),
          stat('Visite', '2 j', 'dernière venue')),
        React.createElement(MbCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Prochains cours' },
            React.createElement(MbBtn, { variant: 'outline', size: 'sm', iconLeft: React.createElement(Icon, { name: 'plus', size: 16 }) }, 'Inscrire')),
          React.createElement('div', { style: { padding: '4px 18px 6px' } },
            F.upcoming.map((c, i) => React.createElement(PersonRow, { key: i, name: c.name, sub: c.when, tone: c.tone, st: c.st })))),
        React.createElement(MbCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Présences récentes' }),
          React.createElement('div', { style: { padding: '4px 18px 6px' } },
            F.recent.map((c, i) => React.createElement(PersonRow, { key: i, name: c.name, sub: c.when, tone: c.tone, st: c.st })))))));
}

/* ---------- Create / edit drawer ---------- */
function Field({ label, value, icon, placeholder, options, onChange, full }) {
  const common = { label, value, onChange: onChange || (() => {}), style: { width: '100%' } };
  if (options) return React.createElement(MbSelect, { ...common, options });
  return React.createElement(MbInput, { ...common, placeholder, iconLeft: icon ? React.createElement(Icon, { name: icon, size: 18 }) : null });
}
function FieldRow({ children }) {
  return React.createElement('div', { style: { display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 14, marginBottom: 14 } }, children);
}

function MemberDrawer({ mode, member, onClose }) {
  const isNew = mode === 'new';
  const m = member || {};
  const [f, setF] = React.useState({
    first: isNew ? '' : m.name.split(' ')[0], last: isNew ? '' : m.name.split(' ').slice(1).join(' '),
    email: isNew ? '' : m.email, phone: isNew ? '' : m.phone, welcome: true, status: 'active',
    plan: '', note: 'Préfère les cours du matin. Vient surtout en début de semaine.',
  });
  const set = (k) => (v) => setF({ ...f, [k]: v });

  return React.createElement(React.Fragment, null,
    React.createElement('div', { className: 'gx-scrim', onClick: onClose }),
    React.createElement('aside', { className: 'gx-drawer' },
      React.createElement('div', { className: 'gx-drawer-h' },
        React.createElement('div', { className: 'ic' }, React.createElement(Icon, { name: isNew ? 'user' : 'settings', size: 20 })),
        React.createElement('div', { className: 'tt' },
          React.createElement('div', { className: 't' }, isNew ? 'Nouveau membre' : 'Modifier la fiche'),
          React.createElement('div', { className: 's' }, isNew ? 'Ajouter une personne au fichier' : `${m.name} · membre depuis ${m.since}`)),
        React.createElement(IconButton, { label: 'Fermer', variant: 'ghost', onClick: onClose }, React.createElement(Icon, { name: 'x', size: 20 }))),
      React.createElement('div', { className: 'gx-drawer-b' },
        React.createElement('div', { className: 'gx-seclab' }, 'Identité'),
        React.createElement(FieldRow, null,
          React.createElement(Field, { label: 'Prénom', value: f.first, onChange: set('first'), placeholder: 'Camille' }),
          React.createElement(Field, { label: 'Nom', value: f.last, onChange: set('last'), placeholder: 'Durand' })),
        React.createElement('div', { style: { marginBottom: 14 } }, React.createElement(Field, { label: 'Adresse e-mail', value: f.email, onChange: set('email'), icon: 'mail', placeholder: 'camille.durand@email.fr' })),
        React.createElement(FieldRow, null,
          React.createElement(Field, { label: 'Téléphone', value: f.phone, onChange: set('phone'), icon: 'phone', placeholder: '06 12 34 56 78' }),
          React.createElement(Field, { label: 'Naissance', value: '', onChange: () => {}, icon: 'calendar', placeholder: 'jj/mm/aaaa' })),
        isNew
          ? React.createElement(React.Fragment, null,
              React.createElement('div', { className: 'gx-seclab' }, 'Abonnement'),
              React.createElement('div', { style: { marginBottom: 14 } },
                React.createElement(Field, { label: 'Formule', value: f.plan, onChange: set('plan'), options: [{ value: '', label: 'Choisir une formule…' }, { value: 'illimite', label: 'Illimité mensuel' }, { value: 'carte', label: 'Carte 10 séances' }, { value: 'etudiant', label: 'Étudiant mensuel' }] })),
              React.createElement('div', { style: { padding: '6px 0' } },
                React.createElement(MbSwitch, { checked: f.welcome, onChange: set('welcome'), label: 'Envoyer un e-mail de bienvenue avec les identifiants' })))
          : React.createElement(React.Fragment, null,
              React.createElement('div', { className: 'gx-seclab' }, 'Note interne'),
              React.createElement(MbInput, { label: '', value: f.note, onChange: set('note'), style: { width: '100%' } }))),
      React.createElement('div', { className: 'gx-drawer-f' },
        !isNew && React.createElement(MbBtn, { variant: 'danger', iconLeft: React.createElement(Icon, { name: 'trash', size: 18 }) }, 'Supprimer'),
        React.createElement('span', { style: { marginLeft: 'auto', display: 'flex', gap: 10 } },
          React.createElement(MbBtn, { variant: 'ghost', onClick: onClose }, 'Annuler'),
          React.createElement(MbBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: isNew ? 'plus' : 'check', size: 18 }), onClick: onClose }, isNew ? 'Créer le membre' : 'Enregistrer')))));
}

window.ScreenMembres = ScreenMembres;
