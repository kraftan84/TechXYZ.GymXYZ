/* ============================================================
   GymXYZ Mobile — Lieux (studios + extérieur + domicile)
   ============================================================ */
function MSalleFiche({ salle, theme, onBack }) {
  const HEATD = ['L', 'M', 'M', 'J', 'V', 'S', 'D'];
  const heatColor = (v) => v >= 90 ? 'var(--color-danger)' : v >= 70 ? 'var(--color-primary)' : 'var(--azure-300)';

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement('div', { className: 'gx-m-fiche-hero' },
        React.createElement(MIcTile, { name: salle.icon, tone: salle.tone, lg: true }),
        React.createElement('div', null,
          React.createElement('div', { className: 'nm' }, salle.name),
          React.createElement('div', { className: 'rl' }, salle.type)),
        React.createElement('div', { style: { display: 'flex', gap: 8 } },
          React.createElement(MChip, { tone: salle.statusTone }, salle.status),
          React.createElement(MChip, { tone: 'neutral' }, 'Cap. ' + salle.cap))),

      salle.area && React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Surface'), React.createElement('span', { className: 'v' }, salle.area)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Niveau'), React.createElement('span', { className: 'v' }, salle.floor)),
        React.createElement('div', { className: 'gx-m-kv' }, React.createElement('span', { className: 'k' }, 'Cours / sem.'), React.createElement('span', { className: 'v' }, salle.sessionsWeek))),

      salle.address && React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-flex', style: { padding: 16, gap: 13 } },
          React.createElement('span', { className: 'gx-m-ic t-brand' }, React.createElement(Icon, { name: 'pin', size: 21 })),
          React.createElement('div', { className: 'gx-m-grow' },
            React.createElement('div', { style: { fontSize: 'var(--text-md)', fontWeight: 700, color: 'var(--text-strong)' } }, salle.address),
            salle.fallback && React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 2 } }, 'Repli : ' + salle.fallback + ' en cas de pluie')))),

      React.createElement(MSection, { title: 'À propos' }),
      React.createElement(MCard, null, React.createElement('div', { className: 'gx-m-prose' }, salle.note)),

      salle.heat && React.createElement(React.Fragment, null,
        React.createElement(MSection, { title: 'Occupation de la semaine' }),
        React.createElement(MCard, null,
          React.createElement('div', { className: 'gx-m-heat' },
            salle.heat.map((v, i) => React.createElement('div', { className: 'd', key: i },
              React.createElement('div', { className: 'bar' }, React.createElement('i', { style: { height: v + '%', background: heatColor(v) } })),
              React.createElement('div', { className: 'lab' }, HEATD[i])))))),

      React.createElement(MSection, { title: "Aujourd'hui" }),
      React.createElement(MCard, null,
        salle.today.map((t, i) => {
          const priv = t[4] === 1;
          const pct = Math.round(t[3] / t[4] * 100);
          return React.createElement('div', { className: 'gx-m-class', key: i },
            React.createElement('div', { className: 'time' }, t[0]),
            React.createElement('div', { className: 'main' },
              React.createElement('div', { className: 'nm' }, t[1]),
              React.createElement('div', { className: 'meta' }, t[2])),
            priv ? React.createElement('div', { style: { flex: 'none' } }, React.createElement(MChip, { tone: 'neutral' }, 'Privé'))
              : React.createElement('div', { className: 'occ' },
                  React.createElement('div', { className: 'n' }, t[3] + '/' + t[4]),
                  React.createElement(MBar, { pct, tone: pct >= 100 ? 'bad' : '' })));
        })),

      React.createElement(MSection, { title: 'Équipement' }),
      React.createElement(MCard, null,
        React.createElement('div', { className: 'gx-m-tags', style: { padding: 16 } },
          salle.equip.map((e, i) => React.createElement('span', { className: 'gx-m-tag', key: i }, e))))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'sub', title: salle.name, onBack }),
    body);
}

function MScreenSalles({ theme, onNavigate, onOpenPlus }) {
  const D = window.GX_DATA;
  const [open, setOpen] = React.useState(null);
  if (open) return React.createElement(MSalleFiche, { salle: open, theme, onBack: () => setOpen(null) });

  const body = React.createElement('div', { className: 'gx-m-body' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement(MPageTitle, { title: 'Lieux', sub: D.salles.length + ' lieux d\'entraînement',
        action: React.createElement('button', { className: 'gx-m-iconbtn', 'aria-label': 'Ajouter un lieu' }, React.createElement(Icon, { name: 'plus', size: 20 })) }),
      React.createElement(MCard, { style: { marginTop: 14 } },
        D.salles.map((s) => React.createElement('button', { className: 'gx-m-tile', key: s.id, onClick: () => setOpen(s) },
          React.createElement(MIcTile, { name: s.icon, tone: s.tone, lg: true }),
          React.createElement('div', { className: 'main' },
            React.createElement('div', { className: 'nm' }, s.name),
            React.createElement('div', { className: 'meta' },
              React.createElement('span', null, s.type),
              React.createElement('span', { className: 'sep' }),
              React.createElement('span', null, 'Cap. ' + s.cap))),
          React.createElement('div', { style: { flex: 'none', display: 'flex', alignItems: 'center', gap: 8 } },
            React.createElement(MChip, { tone: s.statusTone }, s.status),
            React.createElement('span', { className: 'chev' }, React.createElement(Icon, { name: 'chevR', size: 18 }))))))));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenSalles = MScreenSalles;
