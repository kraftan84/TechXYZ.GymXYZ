/* ============================================================
   GymXYZ Mobile — Coachs (list + fiche)
   ============================================================ */
const MJDAYS = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim'];

function MCoachFiche({ coach, theme, onBack }) {
  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement('div', { className: 'gx-m-fiche-hero' },
        React.createElement(MAvatar, { name: coach.name, size: 'xl' }),
        React.createElement('div', null,
          React.createElement('div', { className: 'nm' }, coach.name),
          React.createElement('div', { className: 'rl' }, coach.role)),
        React.createElement('div', { style: { display: 'flex', gap: 8, flexWrap: 'wrap', justifyContent: 'center' } },
          React.createElement(MChip, { tone: coach.tone, icon: coach.tone === 'success' ? 'check' : null }, coach.status + (coach.away ? ' · ' + coach.away : '')),
          React.createElement(MChip, { tone: 'neutral', icon: 'star' }, coach.rating)),
        React.createElement('div', { className: 'gx-m-tags', style: { justifyContent: 'center', marginTop: 2 } },
          coach.disciplines.map((d, i) => React.createElement('span', { className: 'gx-m-tag brand', key: i }, d)))),

      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-fstats' },
          React.createElement('div', { className: 'it' }, React.createElement('div', { className: 'v' }, coach.classesWeek), React.createElement('div', { className: 'l' }, 'Cours / sem.')),
          React.createElement('div', { className: 'it' }, React.createElement('div', { className: 'v' }, coach.fill + '%'), React.createElement('div', { className: 'l' }, 'Remplissage')),
          React.createElement('div', { className: 'it' }, React.createElement('div', { className: 'v' }, coach.members), React.createElement('div', { className: 'l' }, 'Membres')))),

      React.createElement(MSection, { title: 'Disponibilités' }),
      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-avail' },
          coach.avail.map((v, i) => React.createElement('div', { key: i, className: 'd' + (v ? ' on' : '') }, MJDAYS[i])))),

      React.createElement(MSection, { title: 'À propos' }),
      React.createElement(MCard, null, React.createElement('div', { className: 'gx-m-prose' }, coach.bio)),

      React.createElement(MSection, { title: 'Cours de la semaine' }),
      React.createElement(MCard, null,
        coach.sessions.map((s, i) => {
          const pct = Math.round(s[3] / s[4] * 100);
          return React.createElement('div', { className: 'gx-m-class', key: i },
            React.createElement('div', { className: 'time', style: { width: 38 } }, s[0]),
            React.createElement('div', { className: 'main' },
              React.createElement('div', { className: 'nm' }, s[2]),
              React.createElement('div', { className: 'meta' }, s[1] + ' · ' + s[3] + '/' + s[4])),
            React.createElement('div', { className: 'occ' },
              React.createElement('div', { className: 'n' }, pct + '%'),
              React.createElement(MBar, { pct, tone: pct >= 100 ? 'bad' : '' })));
        })),

      React.createElement(MSection, { title: 'Certifications' }),
      React.createElement(MCard, null,
        coach.certs.map((c, i) => React.createElement('div', { className: 'gx-m-row', key: i, style: { cursor: 'default' } },
          React.createElement('span', { className: 'gx-m-ic t-success', style: { width: 34, height: 34 } }, React.createElement(Icon, { name: 'shield', size: 17 })),
          React.createElement('div', { className: 'main' }, React.createElement('div', { className: 'nm', style: { fontWeight: 600 } }, c)))))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'sub', title: coach.name.split(' ')[0], onBack }),
    body);
}

function MScreenCoachs({ theme, onNavigate, onOpenPlus }) {
  const D = window.GX_DATA;
  const [open, setOpen] = React.useState(null);
  if (open) return React.createElement(MCoachFiche, { coach: open, theme, onBack: () => setOpen(null) });

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement(MPageTitle, { title: 'Coachs', sub: D.coachs.length + ' coachs · planning partagé',
        action: React.createElement('button', { className: 'gx-m-iconbtn', 'aria-label': 'Inviter un coach' }, React.createElement(Icon, { name: 'plus', size: 20 })) }),
      React.createElement(MCard, { style: { marginTop: 14 } },
        D.coachs.map((c) => React.createElement('button', { className: 'gx-m-tile', key: c.id, onClick: () => setOpen(c) },
          React.createElement(MAvatar, { name: c.name, size: 'lg' }),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, c.name),
            React.createElement('div', { className: 'meta' },
              React.createElement('span', null, c.classesWeek + ' cours'),
              React.createElement('span', { className: 'sep' }),
              React.createElement('span', null, c.fill + '% rempli'),
              React.createElement('span', { className: 'sep' }),
              React.createElement('span', null, '★ ' + c.rating))),
          React.createElement('div', { style: { flex: 'none', display: 'flex', alignItems: 'center', gap: 8 } },
            React.createElement(MChip, { tone: c.tone }, c.status),
            React.createElement('span', { className: 'chev' }, React.createElement(Icon, { name: 'chevR', size: 18 }))))))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenCoachs = MScreenCoachs;
