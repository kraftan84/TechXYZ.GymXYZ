/* ============================================================
   GymXYZ — Screen: Réglages (paramètres de la salle)
   Identité · Apparence (marque blanche) · Équipe & accès ·
   Formules & tarifs · Notifications · Facturation
   ============================================================ */
const { Button: RgBtn, Card: RgCard, Switch: RgSwitch, Input: RgInput,
        Select: RgSelect, Checkbox: RgCheck, Avatar: RgAvatar, Badge: RgBadge } = window.TechXYZDesignSystem_ff9a8f;

const RG_SECTIONS_USER = [
  { id: 'identite', label: 'Identité', icon: 'building' },
  { id: 'equipe', label: 'Équipe & accès', icon: 'users' },
  { id: 'tarifs', label: 'Formules & tarifs', icon: 'euro' },
  { id: 'notifs', label: 'Notifications', icon: 'bell' },
];

const RG_SECTIONS_ADMIN = [
  { id: 'marque', label: 'Apparence & marque', icon: 'palette' },
  { id: 'facturation', label: 'Facturation', icon: 'wallet' },
];

const RG_PAY_ICONS = ['card', 'refresh', 'euro', 'file', 'send'];

function rgInitials(name) {
  return name.split(' ').filter(Boolean).slice(0, 2).map((w) => w[0]).join('').toUpperCase();
}

/* sticky save bar shared by editable panels */
function SaveBar({ onSave, saved }) {
  return React.createElement('div', { className: 'gx-savebar' },
    saved
      ? React.createElement('span', { className: 'note', style: { color: 'var(--color-success)' } },
          React.createElement(Icon, { name: 'check', size: 14 }), 'Modifications enregistrées')
      : React.createElement('span', { className: 'note' },
          React.createElement(Icon, { name: 'history', size: 14 }), 'Dernière mise à jour il y a 6 jours'),
    React.createElement('span', { className: 'sp' }),
    React.createElement(RgBtn, { variant: 'ghost' }, 'Annuler'),
    React.createElement(RgBtn, { variant: 'primary', onClick: onSave }, 'Enregistrer'));
}

/* ============================================================
   1 · IDENTITÉ
   ============================================================ */
function SecIdentite({ theme }) {
  const D = window.GX_DATA.reglages.identite;
  const init = () => ({
    name: theme.name, baseline: D.baseline, capacity: D.capacity,
    address: D.address, zip: D.zip, city: D.city,
    email: D.email, phone: D.phone, siret: D.siret,
  });
  const [f, setF] = React.useState(init);
  const [saved, setSaved] = React.useState(false);
  React.useEffect(() => { setF(init()); setSaved(false); }, [theme.id]);
  const up = (k) => (e) => { setF((s) => ({ ...s, [k]: e.target.value })); setSaved(false); };
  const today = React.useMemo(() => new Date(), []);
  const weekDates = React.useMemo(() => Array.from({ length: 7 }, (_, i) => gxAddDays(gxMonday(today), i)), []);
  const cal = useSchoolCalendar(f.zip, weekDates);

  return React.createElement('div', { className: 'gx-set-panel' },
    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Informations générales' }),
      React.createElement('div', { className: 'gx-field-grid' },
        React.createElement('div', { className: 'full' }, React.createElement(RgInput, { label: 'Nom de la salle', value: f.name, onChange: up('name') })),
        React.createElement('div', { className: 'full' }, React.createElement(RgInput, { label: 'Baseline', value: f.baseline, onChange: up('baseline'), hint: 'Affichée sous le logo et sur les documents.' })),
        React.createElement(RgInput, { label: 'Capacité d’accueil', type: 'number', value: f.capacity, onChange: up('capacity'), hint: 'Membres maximum.' }),
        React.createElement(RgInput, { label: 'SIRET', value: f.siret, onChange: up('siret') }))),

    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Coordonnées' }),
      React.createElement('div', { className: 'gx-field-grid' },
        React.createElement('div', { className: 'full' }, React.createElement(RgInput, { label: 'Adresse', value: f.address, onChange: up('address'), iconLeft: React.createElement(Icon, { name: 'pin', size: 18 }) })),
        React.createElement(RgInput, { label: 'Code postal', value: f.zip, onChange: up('zip') }),
        React.createElement(RgInput, { label: 'Ville', value: f.city, onChange: up('city') }),
        React.createElement(RgInput, { label: 'E-mail de contact', type: 'email', value: f.email, onChange: up('email'), iconLeft: React.createElement(Icon, { name: 'mail', size: 18 }) }),
        React.createElement(RgInput, { label: 'Téléphone', type: 'tel', value: f.phone, onChange: up('phone'), iconLeft: React.createElement(Icon, { name: 'phone', size: 18 }) }))),

    React.createElement(CalendarCard, { cal, refDate: today }),

    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Horaires d’ouverture' },
        React.createElement(RgBtn, { variant: 'ghost', size: 'sm', iconLeft: React.createElement(Icon, { name: 'plus', size: 16 }) }, 'Ajouter')),
      React.createElement('div', null,
        D.hours.map((h, i) => React.createElement('div', { className: 'gx-hours-row', key: i },
          React.createElement('span', { className: 'day' }, h[0]),
          React.createElement('span', { className: 'h' }, h[1]),
          React.createElement(IconButton, { label: 'Modifier', variant: 'ghost', size: 'sm' }, React.createElement(Icon, { name: 'settings', size: 16 })))))),

    React.createElement(SaveBar, { saved, onSave: () => setSaved(true) }));
}

/* ============================================================
   2 · APPARENCE & MARQUE (white-label)
   ============================================================ */
function SecMarque({ theme, onSetTheme }) {
  return React.createElement('div', { className: 'gx-set-panel' },
    React.createElement('p', { className: 'gx-set-intro' },
      'GymXYZ va plus loin que la marque blanche : chaque thème porte sa propre ',
      React.createElement('b', null, 'atmosphère'),
      ' — accent, encre, fond, surfaces, ombres et registre de la barre latérale (claire, sombre ou douce). Logo, couleurs, typographie et nom suivent, et le panneau ',
      React.createElement('b', null, 'Tweaks'), ' bascule le tout en direct.'),

    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Thème de marque' },
        React.createElement(Chip, { tone: 'brand', icon: 'palette' }, theme.name)),
      React.createElement('div', { className: 'gx-theme-grid' },
        window.GX_THEMES.map((th) => {
          const on = th.id === theme.id;
          return React.createElement('button', {
            key: th.id, type: 'button', className: 'gx-theme-card' + (on ? ' on' : ''),
            onClick: () => onSetTheme && onSetTheme(th.id),
          },
            React.createElement('span', { className: 'sw' }, th.swatch.map((c, i) => React.createElement('i', { key: i, style: { background: c } }))),
            React.createElement('span', { className: 'tnm' }, th.label,
              on && React.createElement(Icon, { name: 'check', size: 16, style: { color: 'var(--color-primary)' } })),
            React.createElement('span', { className: 'tsub' }, th.sub));
        }))),

    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Aperçu' }),
      React.createElement('div', { style: { padding: '20px 18px', display: 'flex', alignItems: 'center', gap: 20, flexWrap: 'wrap' } },
        React.createElement(Brand, { theme, size: 'lg' }),
        React.createElement('span', { style: { width: 1, height: 38, background: 'var(--border-subtle)' } }),
        React.createElement('div', { className: 'gx-tokens', style: { padding: 0 } },
          React.createElement('div', { className: 'gx-token' },
            React.createElement('span', { className: 'dot', style: { background: 'var(--color-primary)' } }),
            React.createElement('span', null,
              React.createElement('span', { className: 'k', style: { display: 'block' } }, 'Accent'),
              React.createElement('span', { className: 'v' }, 'Primaire'))),
          React.createElement('div', { className: 'gx-token' },
            React.createElement('span', { className: 'dot', style: { background: 'var(--text-strong)' } }),
            React.createElement('span', null,
              React.createElement('span', { className: 'k', style: { display: 'block' } }, 'Encre'),
              React.createElement('span', { className: 'v' }, 'Texte fort'))),
          React.createElement('div', { className: 'gx-token' },
            React.createElement('span', { className: 'dot', style: { background: 'var(--surface-page)' } }),
            React.createElement('span', null,
              React.createElement('span', { className: 'k', style: { display: 'block' } }, 'Fond'),
              React.createElement('span', { className: 'v' }, 'Atmosphère'))),
          React.createElement('div', { className: 'gx-token' },
            React.createElement('span', { className: 'dot', style: { background: 'var(--gx-sb-bg)' } }),
            React.createElement('span', null,
              React.createElement('span', { className: 'k', style: { display: 'block' } }, 'Sidebar'),
              React.createElement('span', { className: 'v' }, 'Registre')))))),

    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Logo' }),
      React.createElement('div', { style: { padding: '18px', display: 'flex', alignItems: 'center', gap: 14 } },
        React.createElement('span', { className: 'gx-disc-ic big t-brand' }, React.createElement(Icon, { name: 'download', size: 24 })),
        React.createElement('div', { style: { flex: 1 } },
          React.createElement('div', { style: { fontSize: 'var(--text-sm)', fontWeight: 'var(--weight-bold)', color: 'var(--text-strong)' } }, 'Logo et favicon'),
          React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 2 } }, 'PNG ou SVG · fond transparent · 512 × 512 px recommandé')),
        React.createElement(RgBtn, { variant: 'outline' }, 'Remplacer'))));
}

/* ============================================================
   3 · ÉQUIPE & ACCÈS
   ============================================================ */
function TeamRow({ name, initials, role, roleTone, access, last, you }) {
  return React.createElement('div', { className: 'gx-team-row' },
    React.createElement(RgAvatar, { name, size: 'sm' }),
    React.createElement('div', { className: 'info' },
      React.createElement('div', { className: 'nm' }, name,
        you && React.createElement(RgBadge, { tone: 'neutral' }, 'Vous')),
      React.createElement('div', { className: 'ac' }, access)),
    React.createElement(RgBadge, { tone: roleTone }, role),
    React.createElement('span', { className: 'last' }, 'Vu ' + last),
    React.createElement(IconButton, { label: 'Gérer l’accès', variant: 'ghost', size: 'sm' }, React.createElement(Icon, { name: 'settings', size: 16 })));
}

function SecEquipe({ theme }) {
  const R = window.GX_DATA.reglages;
  const M = R.membres;
  const m = theme.manager;
  const [scope, setScope] = React.useState('gestion');

  const gestion = React.createElement(React.Fragment, null,
    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Équipe de gestion' },
        React.createElement(Chip, { tone: 'neutral' }, R.team.length + 1)),
      React.createElement('div', null,
        React.createElement(TeamRow, { name: m.name, initials: rgInitials(m.name), role: m.role, roleTone: 'brand', access: 'Administration complète', last: "à l’instant", you: true }),
        R.team.map((t, i) => React.createElement(TeamRow, { key: i, ...t })))),

    R.invites.length > 0 && React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Invitations en attente' },
        React.createElement(Chip, { tone: 'warning' }, R.invites.length)),
      React.createElement('div', null,
        R.invites.map((iv, i) => React.createElement('div', { className: 'gx-team-row', key: i },
          React.createElement('span', { className: 'gx-disc-ic t-neutral', style: { width: 36, height: 36 } }, React.createElement(Icon, { name: 'mail', size: 18 })),
          React.createElement('div', { className: 'info' },
            React.createElement('div', { className: 'nm' }, iv.email),
            React.createElement('div', { className: 'ac' }, 'Invité ' + iv.sent)),
          React.createElement(RgBadge, { tone: 'neutral' }, iv.role),
          React.createElement(RgBtn, { variant: 'ghost', size: 'sm' }, 'Renvoyer'))))));

  const membres = React.createElement(React.Fragment, null,
    React.createElement('div', { className: 'gx-grid3', style: { marginBottom: 4 } },
      React.createElement(Kpi, { label: 'Comptes actifs', value: String(M.withAccount), sub: 'sur ' + M.total + ' membres' }),
      React.createElement(Kpi, { label: 'Invitations en attente', value: String(M.invited), sub: 'accès non encore activé' }),
      React.createElement(Kpi, { label: 'Sans accès', value: String(M.total - M.withAccount - M.invited), sub: 'aucun compte créé' })),
    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Accès à l’espace membre' },
        React.createElement(RgBtn, { variant: 'ghost', size: 'sm', iconLeft: React.createElement(Icon, { name: 'send', size: 16 }) }, 'Inviter en masse')),
      React.createElement('div', null,
        M.list.map((u, i) => React.createElement('div', { className: 'gx-team-row', key: i },
          React.createElement(RgAvatar, { name: u.name, size: 'sm' }),
          React.createElement('div', { className: 'info' },
            React.createElement('div', { className: 'nm' }, u.name),
            React.createElement('div', { className: 'ac' }, u.email + ' · ' + u.plan)),
          React.createElement(RgBadge, { tone: u.accTone, dot: u.acc === 'Actif' }, u.acc),
          React.createElement('span', { className: 'last' }, u.acc === 'Sans accès' ? '' : 'Connecté ' + u.last),
          u.acc === 'Sans accès'
            ? React.createElement(RgBtn, { variant: 'outline', size: 'sm' }, 'Inviter')
            : React.createElement(IconButton, { label: 'Gérer le compte', variant: 'ghost', size: 'sm' }, React.createElement(Icon, { name: 'settings', size: 16 })))))));

  return React.createElement('div', { className: 'gx-set-panel' },
    React.createElement('div', { className: 'gx-pagehead', style: { marginBottom: 0, alignItems: 'center' } },
      React.createElement('p', { className: 'gx-set-intro', style: { margin: 0, flex: 1 } },
        'Gérez qui peut se connecter à ' + theme.name + ' : votre équipe côté gestion, et les membres qui accèdent à leur espace personnel.'),
      React.createElement('div', { className: 'right' },
        React.createElement(RgBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'plus', size: 18 }) }, scope === 'gestion' ? 'Inviter un collaborateur' : 'Inviter un membre'))),

    React.createElement('div', { className: 'gx-seg', style: { alignSelf: 'flex-start' } },
      React.createElement('button', { type: 'button', className: scope === 'gestion' ? 'on brand' : '', onClick: () => setScope('gestion') },
        React.createElement(Icon, { name: 'users', size: 15 }), 'Équipe de gestion'),
      React.createElement('button', { type: 'button', className: scope === 'membres' ? 'on brand' : '', onClick: () => setScope('membres') },
        React.createElement(Icon, { name: 'userCheck', size: 15 }), 'Comptes membres')),

    scope === 'gestion' ? gestion : membres);
}

/* ============================================================
   4 · FORMULES & TARIFS
   ============================================================ */
function SecTarifs({ onNavigate }) {
  const F = window.GX_DATA.abos.formules;
  const P = window.GX_DATA.reglages.paiements;
  const [moyens, setMoyens] = React.useState(P.moyens.map((x) => x[1]));
  const [devise, setDevise] = React.useState(P.devise);

  return React.createElement('div', { className: 'gx-set-panel' },
    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Formules d’abonnement' },
        React.createElement(RgBtn, { variant: 'ghost', size: 'sm', iconRight: React.createElement(Icon, { name: 'arrowR', size: 16 }), onClick: () => onNavigate && onNavigate('abos') }, 'Gérer dans Abonnements')),
      React.createElement('div', null,
        F.map((fo) => React.createElement('div', { className: 'gx-formrow', key: fo.id },
          React.createElement('span', { className: 'gx-disc-ic t-' + (fo.tone === 'brand' ? 'brand' : fo.tone), style: { width: 38, height: 38 } }, React.createElement(Icon, { name: 'card', size: 18 })),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 't' }, fo.name),
            React.createElement('div', { className: 's' }, fo.members + ' membres · ' + fo.billing)),
          React.createElement('div', { className: 'price' }, fo.price + ' ', React.createElement('span', null, fo.unit)),
          React.createElement(IconButton, { label: 'Modifier', variant: 'ghost', size: 'sm' }, React.createElement(Icon, { name: 'settings', size: 16 })))))),

    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Paiement & facturation' }),
      React.createElement('div', { className: 'gx-field-grid' },
        React.createElement(RgSelect, { label: 'Devise', value: devise, onChange: (e) => setDevise(e.target.value), options: ['Euro (€)', 'Franc suisse (CHF)', 'Dollar canadien (CA$)'] }),
        React.createElement(RgInput, { label: 'Régime de TVA', value: P.tva, onChange: () => {} })),
      React.createElement('div', { className: 'gx-seclab', style: { padding: '0 18px', margin: '4px 0 0' } }, 'Moyens de paiement acceptés'),
      React.createElement('div', null,
        P.moyens.map((mo, i) => React.createElement('div', { className: 'gx-pay-row', key: i },
          React.createElement(Icon, { name: RG_PAY_ICONS[i] || 'send', size: 18, style: { color: 'var(--text-muted)' } }),
          React.createElement('span', { className: 'nm' }, mo[0]),
          React.createElement(RgSwitch, { checked: moyens[i], onChange: (v) => setMoyens((s) => s.map((x, j) => j === i ? v : x)) }))))));
}

/* ============================================================
   5 · NOTIFICATIONS
   ============================================================ */
function SecNotifs() {
  const G = window.GX_DATA.reglages.notifs;
  const [state, setState] = React.useState(() => G.map((g) => g.items.map((it) => it.on)));
  const toggle = (gi, ii, v) => setState((s) => s.map((g, x) => x === gi ? g.map((o, y) => y === ii ? v : o) : g));

  return React.createElement('div', { className: 'gx-set-panel' },
    React.createElement('p', { className: 'gx-set-intro' },
      'Choisissez quand GymXYZ envoie un message — à vos membres ou à vous. Les canaux disponibles dépendent du forfait.'),
    G.map((g, gi) => React.createElement(RgCard, { padding: '0', key: gi },
      React.createElement(CardHead, { title: g.group }),
      React.createElement('div', null,
        g.items.map((it, ii) => React.createElement('div', { className: 'gx-notif-row', key: ii },
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 't' }, it.t),
            React.createElement('div', { className: 'd' }, it.d),
            React.createElement('div', { className: 'gx-chan' },
              it.chan.map((c, ci) => React.createElement('span', { className: 'c', key: ci }, c)))),
          React.createElement(RgSwitch, { checked: state[gi][ii], onChange: (v) => toggle(gi, ii, v) })))))));
}

/* ============================================================
   6 · FACTURATION (abonnement du studio à GymXYZ)
   ============================================================ */
function SecFacturation() {
  const B = window.GX_DATA.reglages.facturation;
  return React.createElement('div', { className: 'gx-set-panel' },
    React.createElement(RgCard, { padding: '0', accent: true },
      React.createElement(CardHead, { title: 'Votre abonnement GymXYZ' },
        React.createElement(Chip, { tone: 'success', icon: 'check' }, 'Actif')),
      React.createElement('div', { className: 'gx-plan-hero' },
        React.createElement('span', { className: 'gx-disc-ic big t-brand' }, React.createElement(Icon, { name: 'zap', size: 24 })),
        React.createElement('div', { style: { flex: 1 } },
          React.createElement('div', { style: { fontSize: 'var(--text-lg)', fontWeight: 'var(--weight-bold)', color: 'var(--text-strong)' } }, B.plan),
          React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 2 } }, B.planDesc),
          React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 8, display: 'flex', alignItems: 'center', gap: 6 } },
            React.createElement(Icon, { name: 'refresh', size: 13 }), 'Renouvellement le ' + B.renew)),
        React.createElement('div', { style: { textAlign: 'right' } },
          React.createElement('div', { className: 'price' }, B.price),
          React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)' } }, B.unit))),
      React.createElement('div', { style: { padding: '0 20px 18px' } },
        React.createElement('div', { style: { display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginBottom: 6, fontWeight: 'var(--weight-semibold)' } },
          React.createElement('span', null, 'Membres actifs'),
          React.createElement('b', { style: { color: 'var(--text-strong)' } }, B.members + ' / ' + B.membersCap)),
        React.createElement(Bar, { pct: 42 })),
      React.createElement('div', { style: { display: 'flex', gap: 10, padding: '0 20px 20px' } },
        React.createElement(RgBtn, { variant: 'outline' }, 'Changer de formule'),
        React.createElement(RgBtn, { variant: 'ghost' }, 'Voir la consommation'))),

    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Moyen de paiement' }),
      React.createElement('div', { style: { padding: '16px 18px', display: 'flex', alignItems: 'center', gap: 14 } },
        React.createElement('span', { className: 'gx-disc-ic t-neutral' }, React.createElement(Icon, { name: 'card', size: 22 })),
        React.createElement('div', { style: { flex: 1 } },
          React.createElement('div', { style: { fontSize: 'var(--text-sm)', fontWeight: 'var(--weight-bold)', color: 'var(--text-strong)' } }, B.card.brand + ' •••• ' + B.card.last),
          React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 2 } }, 'Expire ' + B.card.exp)),
        React.createElement(RgBtn, { variant: 'outline', size: 'sm' }, 'Modifier'))),

    React.createElement(RgCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Factures' }),
      React.createElement('div', { style: { padding: '4px 18px 8px' } },
        B.factures.map((fa, i) => React.createElement('div', { className: 'gx-listrow', key: i },
          React.createElement('span', { className: 'gx-disc-ic t-neutral', style: { width: 34, height: 34 } }, React.createElement(Icon, { name: 'file', size: 16 })),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 't' }, fa.amount + ' · ' + fa.date),
            React.createElement('div', { className: 's' }, fa.ref)),
          React.createElement(Chip, { tone: fa.tone }, fa.status),
          React.createElement(IconButton, { label: 'Télécharger', variant: 'ghost', size: 'sm' }, React.createElement(Icon, { name: 'download', size: 16 })))))));
}

/* ============================================================
   SHELL
   ============================================================ */
const RG_PANELS = {
  identite: SecIdentite, marque: SecMarque, equipe: SecEquipe,
  tarifs: SecTarifs, notifs: SecNotifs, facturation: SecFacturation,
};

function SettingsScreen({ sections, title, sub, crumb, onNavigate, theme, onSetTheme }) {
  const [sec, setSec] = React.useState(sections[0].id);
  const Panel = RG_PANELS[sec];

  return React.createElement('div', { className: 'gx-screen' },
    React.createElement(Crumb, { parts: crumb }),
    React.createElement(PageHead, { title, sub }),
    React.createElement('div', { className: 'gx-tabs' },
      sections.map((s) => React.createElement('button', {
        key: s.id, type: 'button', className: 'gx-tab' + (s.id === sec ? ' on' : ''),
        onClick: () => setSec(s.id),
      },
        React.createElement(Icon, { name: s.icon, size: 17 }),
        React.createElement('span', null, s.label)))),
    React.createElement('div', { key: sec },
      React.createElement(Panel, { theme, onNavigate, onSetTheme })));
}

/* User-facing settings — opened from the gear next to notifications */
function ScreenReglages({ onNavigate, theme, onSetTheme }) {
  return React.createElement(SettingsScreen, {
    sections: RG_SECTIONS_USER,
    title: 'Réglages',
    sub: 'Configuration de ' + theme.name,
    crumb: ['Accueil', 'Réglages'],
    onNavigate, theme, onSetTheme,
  });
}

/* Super-admin — opened from "Administration" in the sidebar */
function ScreenAdministration({ onNavigate, theme, onSetTheme }) {
  return React.createElement(SettingsScreen, {
    sections: RG_SECTIONS_ADMIN,
    title: 'Administration',
    sub: 'Réglages super-admin',
    crumb: ['Accueil', 'Administration'],
    onNavigate, theme, onSetTheme,
  });
}

window.ScreenReglages = ScreenReglages;
window.ScreenAdministration = ScreenAdministration;
