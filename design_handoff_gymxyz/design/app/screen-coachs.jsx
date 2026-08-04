/* ============================================================
   GymXYZ — Screen: Coachs (team grid → fiche coach)
   ============================================================ */
const { Button: CoBtn, Card: CoCard, Avatar: CoAvatar } = window.TechXYZDesignSystem_ff9a8f;

const CO_DAYS = ['L', 'M', 'M', 'J', 'V', 'S', 'D'];

function fillTone(pct) { return pct >= 95 ? 'warn' : pct >= 80 ? '' : ''; }

/* ---------- card ---------- */
function CoachCard({ c, onOpen }) {
  return React.createElement('div', { className: 'gx-coach-card', onClick: () => onOpen(c) },
    React.createElement('div', { className: 'top' },
      React.createElement(CoAvatar, { name: c.name, size: 'lg' }),
      React.createElement('div', { style: { flex: 1, minWidth: 0 } },
        React.createElement('div', { className: 'nm' }, c.name),
        React.createElement('div', { className: 'rl' }, c.role)),
      React.createElement('span', { style: { flex: 'none' } }, React.createElement(Chip, { tone: c.tone }, c.status))),
    React.createElement('div', { className: 'gx-tags' },
      c.disciplines.map((d, i) => React.createElement('span', { key: i, className: 'gx-tag' + (i === 0 ? ' brand' : '') }, d))),
    React.createElement('div', { className: 'stats' },
      React.createElement('div', { className: 'st' },
        React.createElement('div', { className: 'v' }, c.classesWeek),
        React.createElement('div', { className: 'l' }, 'cours / sem.')),
      React.createElement('div', { className: 'st' },
        React.createElement('div', { className: 'v' }, c.fill + '%'),
        React.createElement('div', { className: 'l' }, 'remplissage')),
      React.createElement('div', { className: 'st' },
        React.createElement('div', { className: 'v' }, c.rating),
        React.createElement('div', { className: 'l' }, 'note membres'))));
}

/* ---------- list ---------- */
function CoachsList({ onOpen }) {
  const D = window.GX_DATA;
  const [filter, setFilter] = React.useState('all');
  const dispo = D.coachs.filter((c) => c.tone === 'success').length;
  const list = filter === 'dispo' ? D.coachs.filter((c) => c.tone === 'success')
    : filter === 'away' ? D.coachs.filter((c) => c.tone === 'neutral')
      : D.coachs;
  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: ['Accueil', 'Coachs'] }),
    React.createElement(PageHead, { title: 'Coachs', sub: D.coachs.length + ' coachs · ' + dispo + ' disponibles cette semaine' },
      React.createElement('span', { className: 'gx-search', style: { width: 230, height: 40 } },
        React.createElement(Icon, { name: 'search', size: 17 }), React.createElement('span', null, 'Nom, discipline…')),
      React.createElement(CoBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'plus', size: 18 }) }, 'Inviter un coach')),
    React.createElement('div', { className: 'gx-filters' },
      React.createElement('span', { className: 'gx-fchip' + (filter === 'all' ? ' on' : ''), onClick: () => setFilter('all') }, 'Tous · ' + D.coachs.length),
      React.createElement('span', { className: 'gx-fchip' + (filter === 'dispo' ? ' on' : ''), onClick: () => setFilter('dispo') }, 'Disponibles · ' + dispo),
      React.createElement('span', { className: 'gx-fchip' + (filter === 'away' ? ' on' : ''), onClick: () => setFilter('away') }, 'En congé · ' + D.coachs.filter((c) => c.tone === 'neutral').length),
      React.createElement('span', { className: 'gx-fchip', style: { marginLeft: 'auto' } }, React.createElement(Icon, { name: 'filter', size: 14 }), 'Trier : remplissage')),
    React.createElement('div', { className: 'gx-coachgrid' },
      list.map((c) => React.createElement(CoachCard, { key: c.id, c, onOpen }))));
}

/* ---------- fiche ---------- */
function SessionRow({ s }) {
  const [day, time, name, occ, cap] = s;
  const pct = Math.round(occ / cap * 100);
  return React.createElement('div', { className: 'gx-listrow' },
    React.createElement('span', { style: { width: 58, flex: 'none', fontSize: 'var(--text-2xs)', fontWeight: 700, color: 'var(--color-brand)', textTransform: 'uppercase', letterSpacing: '.03em' } }, day + ' · ' + time),
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 't' }, name)),
    React.createElement(Bar, { pct, tone: pct >= 100 ? 'bad' : '', width: 72 }),
    React.createElement('span', { style: { width: 44, flex: 'none', textAlign: 'right', fontSize: 'var(--text-2xs)', color: 'var(--text-muted)', fontVariantNumeric: 'tabular-nums' } }, occ + '/' + cap));
}

function FicheCoach({ coach, onBack }) {
  const c = coach || window.GX_DATA.coachs[0];
  const stat = (lab, val, sub) => React.createElement('div', { className: 'gx-kpi' },
    React.createElement('div', { className: 'lab' }, lab),
    React.createElement('div', { className: 'val', style: { fontSize: 'var(--text-2xl)' } }, val),
    React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 3 } }, sub));

  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: [{ label: 'Accueil' }, { label: 'Coachs', to: 'coachs' }, c.name], onNav: onBack }),
    React.createElement('div', { className: 'gx-fiche-head' },
      React.createElement(CoAvatar, { name: c.name, size: 'xl' }),
      React.createElement('div', { className: 'info' },
        React.createElement('div', { className: 'name-row' },
          React.createElement('span', { className: 'name' }, c.name),
          React.createElement(Chip, { tone: c.tone, icon: c.tone === 'success' ? 'check' : null }, c.status)),
        React.createElement('div', { className: 'contact' }, `${c.role} · ${c.email} · coach depuis ${c.since}`)),
      React.createElement(CoBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'calendar', size: 18 }) }, 'Disponibilités'),
      React.createElement(CoBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'settings', size: 18 }) }, 'Éditer')),

    React.createElement('div', { className: 'gx-grid2', style: { gridTemplateColumns: '1.3fr 1fr', alignItems: 'start' } },
      // left
      React.createElement('div', { className: 'gx-col' },
        React.createElement('div', { className: 'gx-grid3' },
          stat('Cours / sem.', c.classesWeek, 'cette semaine'),
          stat('Remplissage', c.fill + '%', 'moyenne 30 j'),
          stat('Note membres', c.rating, c.members + ' membres suivis')),
        React.createElement(CoCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Cours animés cette semaine' },
            React.createElement('span', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)' } }, c.sessions.length + ' séances')),
          React.createElement('div', { style: { padding: '4px 18px 8px' } },
            c.sessions.map((s, i) => React.createElement(SessionRow, { key: i, s })))),
        React.createElement(CoCard, { padding: '0' },
          React.createElement(CardHead, { title: 'À propos' }),
          React.createElement('div', { style: { padding: '14px 18px 16px', fontSize: 'var(--text-sm)', color: 'var(--text-body)', lineHeight: 1.55 } }, c.bio))),
      // right
      React.createElement('div', { className: 'gx-col' },
        React.createElement(CoCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Disponibilité' },
            c.away ? React.createElement(Chip, { tone: 'neutral' }, c.away) : React.createElement(Chip, { tone: 'success', icon: 'check' }, 'Cette semaine')),
          React.createElement('div', { style: { padding: '16px 18px' } },
            React.createElement('div', { className: 'gx-avail' },
              CO_DAYS.map((d, i) => React.createElement('div', { key: i, className: 'd' + (c.avail[i] ? ' on' : '') }, d))),
            React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 10, display: 'flex', alignItems: 'center', gap: 6 } },
              React.createElement(Icon, { name: 'clock', size: 14 }),
              c.avail.filter(Boolean).length + ' jours disponibles · créneaux 07 h – 21 h'))),
        React.createElement(CoCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Disciplines' }),
          React.createElement('div', { style: { padding: '14px 18px 4px' } },
            React.createElement('div', { className: 'gx-tags', style: { marginTop: 0 } },
              c.disciplines.map((d, i) => React.createElement('span', { key: i, className: 'gx-tag' + (i === 0 ? ' brand' : '') }, d)))),
          React.createElement('div', { style: { padding: '12px 18px 16px' } },
            React.createElement('div', { className: 'gx-seclab', style: { margin: '0 0 8px' } }, 'Certifications'),
            React.createElement('div', { style: { display: 'flex', flexDirection: 'column', gap: 9 } },
              c.certs.map((ct, i) => React.createElement('div', { key: i, style: { display: 'flex', alignItems: 'center', gap: 9, fontSize: 'var(--text-sm)', color: 'var(--text-body)' } },
                React.createElement('span', { style: { color: 'var(--color-success)', flex: 'none', display: 'inline-flex' } }, React.createElement(Icon, { name: 'check', size: 16 })),
                ct))))))));
}

function ScreenCoachs() {
  const [view, setView] = React.useState('list');
  const [coach, setCoach] = React.useState(null);
  return React.createElement('div', { className: 'gx-screen' },
    view === 'list'
      ? React.createElement(CoachsList, { onOpen: (c) => { setCoach(c); setView('fiche'); } })
      : React.createElement(FicheCoach, { coach, onBack: () => setView('list') }));
}

window.ScreenCoachs = ScreenCoachs;
