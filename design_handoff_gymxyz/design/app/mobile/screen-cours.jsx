/* ============================================================
   GymXYZ Mobile — Cours (catalogue de modèles + fiche)
   ============================================================ */
function mCoachById(id) { return window.GX_DATA.coachs.find((c) => c.id === id); }

function MCoursFiche({ cours, theme, onBack }) {
  const toneFor = cours.icon === 'dumbbell' ? 'brand' : cours.icon === 'sparkles' ? 'success' : cours.icon === 'user' ? 'neutral' : 'danger';
  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement('div', { className: 'gx-m-fiche-hero' },
        React.createElement(MIcTile, { name: cours.icon, tone: toneFor, lg: true }),
        React.createElement('div', null,
          React.createElement('div', { className: 'nm' }, cours.name),
          React.createElement('div', { className: 'rl' }, cours.discipline + ' · ' + cours.duration)),
        React.createElement('div', { style: { display: 'flex', gap: 8, flexWrap: 'wrap', justifyContent: 'center' } },
          React.createElement(MChip, { tone: 'neutral' }, cours.level),
          React.createElement(MChip, { tone: cours.intensity === 'Élevée' ? 'danger' : cours.intensity === 'Privé' ? 'neutral' : 'warning' }, 'Intensité ' + cours.intensity),
          React.createElement(MChip, { tone: cours.price === 'Inclus' ? 'success' : 'brand' }, cours.price))),

      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-fstats' },
          React.createElement('div', { className: 'it' }, React.createElement('div', { className: 'v' }, cours.sessionsWeek), React.createElement('div', { className: 'l' }, 'Séances / sem.')),
          React.createElement('div', { className: 'it' }, React.createElement('div', { className: 'v' }, cours.fill + '%'), React.createElement('div', { className: 'l' }, 'Remplissage')),
          React.createElement('div', { className: 'it' }, React.createElement('div', { className: 'v' }, cours.regulars), React.createElement('div', { className: 'l' }, 'Habitués')))),

      React.createElement(MSection, { title: 'Description' }),
      React.createElement(MCard, null, React.createElement('div', { className: 'gx-m-prose' }, cours.desc)),

      React.createElement(MSection, { title: 'Détails' }),
      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Durée'), React.createElement('span', { className: 'v' }, cours.duration)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Capacité'), React.createElement('span', { className: 'v' }, cours.cap + ' places')),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Studio'), React.createElement('span', { className: 'v' }, cours.studio))),

      React.createElement(MSection, { title: 'Coachs habilités' }),
      React.createElement(MCard, null,
        cours.coachs.map((id) => { const c = mCoachById(id); if (!c) return null;
          return React.createElement('div', { className: 'gx-m-row', key: id, style: { cursor: 'default' } },
            React.createElement(MAvatar, { name: c.name, size: 'md' }),
            React.createElement('div', { className: 'main' },
              React.createElement('div', { className: 'nm' }, c.name),
              React.createElement('div', { className: 'meta' }, c.role))); })),

      React.createElement(MSection, { title: 'Prochaines séances' }),
      React.createElement(MCard, null,
        cours.next.map((n, i) => {
          const pct = Math.round(n[3] / n[4] * 100);
          return React.createElement('div', { className: 'gx-m-class', key: i },
            React.createElement('div', { className: 'time', style: { width: 64 } }, n[0] + ' ' + n[1]),
            React.createElement('div', { className: 'main' },
              React.createElement('div', { className: 'nm' }, n[2]),
              React.createElement('div', { className: 'meta' }, n[3] + '/' + n[4] + ' places')),
            React.createElement('div', { className: 'occ' },
              React.createElement('div', { className: 'n' }, pct + '%'),
              React.createElement(MBar, { pct, tone: pct >= 100 ? 'bad' : '' })));
        }))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'sub', title: cours.name, onBack }),
    body);
}

function MScreenCours({ theme, onNavigate, onOpenPlus }) {
  const D = window.GX_DATA;
  const [open, setOpen] = React.useState(null);
  if (open) return React.createElement(MCoursFiche, { cours: open, theme, onBack: () => setOpen(null) });

  const toneFor = (ic) => ic === 'dumbbell' ? 'brand' : ic === 'sparkles' ? 'success' : ic === 'user' ? 'neutral' : 'danger';

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement(MPageTitle, { title: 'Cours', sub: D.cours.length + ' modèles de cours',
        action: React.createElement('button', { className: 'gx-m-iconbtn', 'aria-label': 'Nouveau cours' }, React.createElement(Icon, { name: 'plus', size: 20 })) }),
      React.createElement(MCard, { style: { marginTop: 14 } },
        D.cours.map((c) => React.createElement('button', { className: 'gx-m-tile', key: c.id, onClick: () => setOpen(c) },
          React.createElement(MIcTile, { name: c.icon, tone: toneFor(c.icon) }),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, c.name),
            React.createElement('div', { className: 'meta' },
              React.createElement('span', null, c.discipline),
              React.createElement('span', { className: 'sep' }),
              React.createElement('span', null, c.duration),
              React.createElement('span', { className: 'sep' }),
              React.createElement('span', null, c.sessionsWeek + '×/sem.'))),
          React.createElement('div', { style: { flex: 'none', display: 'flex', alignItems: 'center', gap: 8 } },
            React.createElement(MChip, { tone: c.fill >= 90 ? 'danger' : 'neutral' }, c.fill + '%'),
            React.createElement('span', { className: 'chev' }, React.createElement(Icon, { name: 'chevR', size: 18 }))))))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenCours = MScreenCours;
