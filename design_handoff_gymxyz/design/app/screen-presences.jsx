/* ============================================================
   GymXYZ — Screen: Présences (pointage + assiduité)
   list (séances à pointer / récentes + assiduité) → feuille de présence
   ============================================================ */
const { Button: PrBtn, Card: PrCard, Avatar: PrAvatar } = window.TechXYZDesignSystem_ff9a8f;

const PR_STATUS = {
  present: { label: 'Présent', icon: 'check', tone: 'success' },
  late: { label: 'Retard', icon: 'clock', tone: 'warning' },
  absent: { label: 'Absent', icon: 'minus', tone: 'danger' },
  pending: { label: 'En attente', icon: 'user', tone: 'neutral' },
};

function prTally(roster) {
  const t = { present: 0, late: 0, absent: 0, pending: 0 };
  roster.forEach((r) => { t[r.status] = (t[r.status] || 0) + 1; });
  return t;
}
function prTaux(roster) {
  const t = prTally(roster);
  const counted = roster.length - t.pending;
  if (!counted) return 0;
  return Math.round(((t.present + t.late) / counted) * 100);
}

/* ---------- session row (in cards) ---------- */
function PresSessionRow({ s, onOpen }) {
  const enr = s.roster.length;
  const t = prTally(s.roster);
  const taux = prTaux(s.roster);
  let right;
  if (s.status === 'done') {
    right = React.createElement('div', { style: { display: 'flex', alignItems: 'center', gap: 12 } },
      React.createElement('div', { style: { textAlign: 'right' } },
        React.createElement('div', { style: { fontSize: 'var(--text-sm)', fontWeight: 'var(--weight-bold)', color: 'var(--text-strong)', fontVariantNumeric: 'tabular-nums' } }, taux + '%'),
        React.createElement('div', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)' } }, t.present + t.late + '/' + enr + ' présents')),
      React.createElement(Chip, { tone: 'success', icon: 'check' }, 'Pointé'));
  } else if (s.status === 'live') {
    right = React.createElement('div', { style: { display: 'flex', alignItems: 'center', gap: 12 } },
      React.createElement('span', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)' } }, t.present + t.late + '/' + enr + ' arrivés'),
      React.createElement(Chip, { tone: 'warning', icon: 'clock' }, 'En cours'));
  } else {
    right = React.createElement('div', { style: { display: 'flex', alignItems: 'center', gap: 12 } },
      React.createElement('span', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)' } }, enr + ' inscrits'),
      React.createElement(Chip, { tone: 'brand' }, 'À pointer'));
  }
  return React.createElement('div', { className: 'gx-classrow click', onClick: () => onOpen(s) },
    React.createElement('span', { className: 'time' }, s.time),
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 'nm' }, s.course),
      React.createElement('div', { className: 'meta' }, s.studio + ' · ' + s.coach + (s.day !== "Aujourd'hui" ? ' · ' + s.day : ''))),
    right,
    React.createElement(Icon, { name: 'chevR', size: 16, style: { color: 'var(--text-subtle)', flex: 'none' } }));
}

/* ---------- list ---------- */
function PresencesList({ onOpen }) {
  const P = window.GX_DATA.presences;
  const [filter, setFilter] = React.useState('today');
  const todo = P.sessions.filter((s) => s.status === 'todo' || s.status === 'live');
  const done = P.sessions.filter((s) => s.status === 'done');

  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: ['Accueil', 'Présences'] }),
    React.createElement(PageHead, { title: 'Présences', sub: P.date + ' · ' + todo.length + ' séances à pointer' },
      React.createElement('span', { className: 'gx-search', style: { width: 230, height: 40 } },
        React.createElement(Icon, { name: 'search', size: 17 }), React.createElement('span', null, 'Membre, cours…')),
      React.createElement(PrBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'qr', size: 18 }) }, 'Borne de pointage')),

    React.createElement('div', { className: 'gx-grid4', style: { marginBottom: 18 } },
      React.createElement(Kpi, { label: 'Taux de présence', value: P.kpis.taux, delta: P.kpis.tauxDelta, deltaIcon: 'trend', spark: true }),
      React.createElement(Kpi, { label: 'Séances à pointer', value: String(P.kpis.pointer), sub: "aujourd'hui", valueColor: 'var(--color-primary)' }),
      React.createElement(Kpi, { label: 'Présents du jour', value: String(P.kpis.presents), sub: 'sur 6 séances' }),
      React.createElement(Kpi, { label: 'No-shows', value: String(P.kpis.noshow), sub: 'cette semaine', valueColor: 'var(--color-danger)' })),

    React.createElement('div', { className: 'gx-grid2', style: { gridTemplateColumns: '1.5fr 1fr', alignItems: 'start' } },
      // left — sessions
      React.createElement('div', { className: 'gx-col' },
        React.createElement(PrCard, { padding: '0' },
          React.createElement(CardHead, { title: 'À pointer · ' + window.GX_DATA.presences.date },
            React.createElement(Chip, { tone: 'brand' }, todo.length + ' séances')),
          React.createElement('div', { style: { padding: '2px 18px 8px' } },
            todo.map((s) => React.createElement(PresSessionRow, { key: s.id, s, onOpen })))),
        React.createElement(PrCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Séances récentes' },
            React.createElement(PrBtn, { variant: 'ghost', size: 'sm', iconLeft: React.createElement(Icon, { name: 'history', size: 16 }) }, 'Historique')),
          React.createElement('div', { style: { padding: '2px 18px 8px' } },
            done.map((s) => React.createElement(PresSessionRow, { key: s.id, s, onOpen }))))),
      // right — assiduité
      React.createElement('div', { className: 'gx-col' },
        React.createElement(PrCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Taux par cours' },
            React.createElement('span', { style: { fontSize: 'var(--text-2xs)', color: 'var(--text-muted)' } }, '30 derniers jours')),
          React.createElement('div', { style: { padding: '8px 18px 14px' } },
            P.parCours.map((c, i) => React.createElement('div', { className: 'gx-statbar', key: i },
              React.createElement('span', { className: 'nm' }, c.name),
              React.createElement('div', { style: { flex: 1 } }, React.createElement(Bar, { pct: c.pct, tone: c.pct < 75 ? 'warn' : '' })),
              React.createElement('span', { className: 'pct' }, c.pct + '%'))))),
        React.createElement(PrCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Absents à relancer' },
            React.createElement(Chip, { tone: 'danger' }, P.absents.length)),
          React.createElement('div', { style: { padding: '4px 18px 10px' } },
            P.absents.map((a, i) => React.createElement('div', { className: 'gx-listrow', key: i },
              React.createElement(PrAvatar, { name: a.name, size: 'md' }),
              React.createElement('div', { className: 'main' },
                React.createElement('div', { className: 't' }, a.name),
                React.createElement('div', { className: 's' }, a.miss + ' absences / ' + a.total + ' · ' + a.last)),
              React.createElement(IconButton, { label: 'Relancer', variant: 'ghost', size: 'sm' }, React.createElement(Icon, { name: 'mail', size: 17 }))))),
          React.createElement('div', { style: { padding: '0 18px 16px' } },
            React.createElement(PrBtn, { variant: 'outline', size: 'sm', style: { width: '100%' }, iconLeft: React.createElement(Icon, { name: 'send', size: 16 }) }, 'Relancer tout le monde'))))));
}

/* ---------- segmented present / late / absent ---------- */
function PresenceSeg({ value, onChange }) {
  const opt = (key) => {
    const m = PR_STATUS[key];
    return React.createElement('button', {
      type: 'button', className: value === key ? 'on ' + key : '', onClick: () => onChange(key),
    }, React.createElement(Icon, { name: m.icon, size: 15 }), m.label);
  };
  return React.createElement('div', { className: 'gx-seg' }, opt('present'), opt('late'), opt('absent'));
}

/* ---------- feuille de présence ---------- */
function FeuillePresence({ session, onBack }) {
  const s = session || window.GX_DATA.presences.sessions[0];
  const [roster, setRoster] = React.useState(() => s.roster.map((r) => ({ ...r })));
  const t = prTally(roster);
  const taux = prTaux(roster);
  const setStatus = (i, status) => setRoster((prev) => prev.map((r, j) => (j === i ? { ...r, status } : r)));
  const allPresent = () => setRoster((prev) => prev.map((r) => ({ ...r, status: 'present' })));

  const sum = (icon, kind, val, label) => React.createElement('div', { className: 'gx-pres-stat' },
    React.createElement('div', { className: 'ic ' + kind }, React.createElement(Icon, { name: icon, size: 20 })),
    React.createElement('div', null,
      React.createElement('div', { className: 'v' }, val),
      React.createElement('div', { className: 'l' }, label)));

  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: [{ label: 'Accueil' }, { label: 'Présences', to: 'presences' }, s.course], onNav: onBack }),
    React.createElement('div', { className: 'gx-fiche-head' },
      React.createElement('span', { className: 'gx-disc-ic big t-' + s.itone }, React.createElement(Icon, { name: s.icon, size: 28 })),
      React.createElement('div', { className: 'info' },
        React.createElement('div', { className: 'name-row' },
          React.createElement('span', { className: 'name' }, s.course),
          s.status === 'live'
            ? React.createElement(Chip, { tone: 'warning', icon: 'clock' }, 'En cours')
            : React.createElement(Chip, { tone: s.status === 'done' ? 'success' : 'brand' }, s.status === 'done' ? 'Pointé' : 'À pointer')),
        React.createElement('div', { className: 'contact' }, s.day + ' · ' + s.time + ' · ' + s.studio + ' · ' + s.coach + ' · ' + roster.length + ' inscrits')),
      React.createElement(PrBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'userCheck', size: 18 }), onClick: allPresent }, 'Tout présent'),
      React.createElement(PrBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'check', size: 18 }), onClick: onBack }, 'Valider la feuille')),

    React.createElement('div', { className: 'gx-pres-summary' },
      sum('check', 'present', String(t.present), 'Présents'),
      sum('clock', 'late', String(t.late), 'Retards'),
      sum('minus', 'absent', String(t.absent), 'Absents'),
      sum('percent', 'pending', taux + '%', 'Taux de présence')),

    React.createElement(PrCard, { padding: '0' },
      React.createElement(CardHead, { title: 'Feuille de présence' },
        t.pending > 0
          ? React.createElement('span', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)' } }, t.pending + ' à pointer')
          : React.createElement(Chip, { tone: 'success', icon: 'check' }, 'Complète')),
      React.createElement('div', null,
        roster.map((r, i) => React.createElement('div', { className: 'gx-roster' + (r.status === 'absent' ? ' is-absent' : ''), key: i },
          React.createElement(PrAvatar, { name: r.name, size: 'md' }),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, r.name),
            React.createElement('div', { className: 'pl' }, r.plan)),
          React.createElement(PresenceSeg, { value: r.status, onChange: (v) => setStatus(i, v) }))))),
    React.createElement('div', { className: 'gx-diffuse-foot' },
      React.createElement(Icon, { name: 'qr', size: 15 }),
      React.createElement('span', null, 'Astuce — les membres peuvent aussi pointer eux-mêmes via la ', React.createElement('b', null, 'borne QR'), " à l'entrée du studio.")));
}

function ScreenPresences() {
  const [view, setView] = React.useState('list');
  const [session, setSession] = React.useState(null);
  return React.createElement('div', { className: 'gx-screen' },
    view === 'list'
      ? React.createElement(PresencesList, { onOpen: (s) => { setSession(s); setView('sheet'); } })
      : React.createElement(FeuillePresence, { session, onBack: () => setView('list') }));
}

window.ScreenPresences = ScreenPresences;
