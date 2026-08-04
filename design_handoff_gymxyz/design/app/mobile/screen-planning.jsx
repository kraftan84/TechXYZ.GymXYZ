/* ============================================================
   GymXYZ Mobile — Planning (agenda by day)
   Date selector (week) → vertical agenda of that day's classes.
   ============================================================ */
function MScreenPlanning({ theme, onNavigate, onOpenPlus }) {
  const D = window.GX_DATA;
  const weekDates = React.useMemo(() => D.week.map((x) => new Date(2026, 5, x.n)), []);
  const zip = (D.reglages && D.reglages.identite && D.reglages.identite.zip) || '69003';
  const cal = useSchoolCalendar(zip, weekDates);
  const [sel, setSel] = React.useState(1); // Tue 9 (today in data)

  // build agenda for selected day from D.events
  const dayEvents = D.events[sel] || {};
  const slots = Object.keys(dayEvents).map(Number).sort((a, b) => a - b).map((h) => {
    const [name, coach, occ, cap] = dayEvents[h];
    return { time: String(h).padStart(2, '0') + ':00', name, coach, occ, cap };
  });
  const info = gxDayInfo(cal, weekDates[sel]);
  const dayName = ['Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi', 'Samedi', 'Dimanche'][sel];

  const body = React.createElement('div', { className: 'gx-m-body flush' },
    React.createElement('div', { className: 'gx-m-screen' },
      React.createElement('div', { style: { padding: '0 16px' } },
        React.createElement(MPageTitle, { title: 'Planning', sub: D.weekRange,
          action: React.createElement('button', { className: 'gx-m-iconbtn', onClick: () => onNavigate('planning'), 'aria-label': 'Ajouter un cours' }, React.createElement(Icon, { name: 'plus', size: 20 })) })),

      // date selector
      React.createElement('div', { className: 'gx-m-datebar gx-m-week7', style: { marginTop: 14 } },
        D.week.map((d, i) =>
          React.createElement('div', { key: i, className: 'gx-m-datechip' + (i === sel ? ' on' : ''), onClick: () => setSel(i) },
            React.createElement('div', { className: 'dn' }, d.d),
            React.createElement('div', { className: 'nn' }, d.n),
            d.c ? React.createElement('div', { className: 'dot' }) : null))),

      // school-calendar banner
      info && React.createElement('div', { className: 'gx-m-calbar', style: { marginTop: 12 } },
        React.createElement('span', { className: 'zone' }, React.createElement(Icon, { name: 'pin', size: 13 }), 'Zone ' + cal.zone),
        React.createElement('span', { className: 'gx-m-calpill ' + info.type },
          React.createElement(Icon, { name: info.type === 'ferie' ? 'star' : 'sun', size: 11 }), info.label)),

      // day header
      React.createElement('div', { className: 'gx-m-sec', style: { padding: '0 16px', margin: '18px 0 6px' } },
        React.createElement('span', { className: 't' }, dayName + ' ' + D.week[sel].n + ' juin · ' + slots.length + ' cours')),

      // agenda
      slots.length
        ? React.createElement('div', { className: 'gx-m-agenda' },
            slots.map((s, i) => {
              const pct = Math.round(s.occ / s.cap * 100);
              const priv = s.cap === 1;
              const full = pct >= 100 && !priv;
              const cls = 'gx-m-slot' + (full ? ' full' : priv ? ' priv' : '');
              return React.createElement('div', { className: cls, key: i },
                React.createElement('div', { className: 'when' }, s.time),
                React.createElement('div', { className: 'track' },
                  React.createElement('div', { className: 'gx-m-evcard' + (full ? ' full' : priv ? ' priv' : ''), onClick: () => onNavigate('presences') },
                    React.createElement('div', { className: 'top' },
                      React.createElement('span', { className: 'nm' }, s.name),
                      priv ? React.createElement(MChip, { tone: 'neutral' }, 'Privé')
                        : full ? React.createElement(MChip, { tone: 'danger' }, 'Complet') : null),
                    React.createElement('div', { className: 'meta' }, s.coach + (s.coach !== '—' ? ' · ' : '') + (priv ? 'Sur RDV' : s.occ + '/' + s.cap + ' places')),
                    !priv && React.createElement('div', { className: 'foot' },
                      React.createElement(MBar, { pct, tone: full ? 'bad' : '' }),
                      React.createElement('span', { className: 'cap' }, pct + '%')))));
            }))
        : React.createElement(MEmpty, { icon: 'calendar', title: 'Aucun cours', text: 'Pas de cours programmé ce jour.' })));

  return React.createElement(React.Fragment, null,
    React.createElement(MHead, { theme, mode: 'root', action: React.createElement(MRootActions, { theme, onOpenPlus }) }),
    body);
}
window.MScreenPlanning = MScreenPlanning;
