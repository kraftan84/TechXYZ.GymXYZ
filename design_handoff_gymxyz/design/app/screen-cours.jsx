/* ============================================================
   GymXYZ — Screen: Cours (catalogue de modèles → fiche cours)
   ============================================================ */
const { Button: CrBtn, Card: CrCard, Avatar: CrAvatar } = window.TechXYZDesignSystem_ff9a8f;

const CR_INTENSITY = { 'Élevée': 'danger', 'Modérée': 'warning', 'Douce': 'success', 'Privé': 'neutral' };

function coachById(id) { return window.GX_DATA.coachs.find((c) => c.id === id); }

/* ---------- list (table) ---------- */
function CoursList({ onOpen, theme }) {
  const D = window.GX_DATA;
  const solo = theme && theme.solo;
  const meName = (theme && theme.manager && theme.manager.name) || 'Vous';
  const [filter, setFilter] = React.useState('all');
  const list = filter === 'collectif' ? D.cours.filter((c) => c.cap > 1)
    : filter === 'prive' ? D.cours.filter((c) => c.cap === 1)
      : D.cours;
  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: ['Accueil', 'Cours'] }),
    React.createElement(PageHead, { title: 'Cours', sub: D.cours.length + ' modèles de cours · réutilisés dans le planning' },
      React.createElement('span', { className: 'gx-search', style: { width: 230, height: 40 } },
        React.createElement(Icon, { name: 'search', size: 17 }), React.createElement('span', null, 'Nom, discipline…')),
      React.createElement(CrBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'plus', size: 18 }) }, 'Nouveau cours')),
    React.createElement('div', { className: 'gx-filters' },
      React.createElement('span', { className: 'gx-fchip' + (filter === 'all' ? ' on' : ''), onClick: () => setFilter('all') }, 'Tous · ' + D.cours.length),
      React.createElement('span', { className: 'gx-fchip' + (filter === 'collectif' ? ' on' : ''), onClick: () => setFilter('collectif') }, 'Collectifs · ' + D.cours.filter((c) => c.cap > 1).length),
      React.createElement('span', { className: 'gx-fchip' + (filter === 'prive' ? ' on' : ''), onClick: () => setFilter('prive') }, 'Privés · ' + D.cours.filter((c) => c.cap === 1).length),
      React.createElement('span', { className: 'gx-fchip', style: { marginLeft: 'auto' } }, React.createElement(Icon, { name: 'filter', size: 14 }), 'Trier : popularité')),
    React.createElement(CrCard, { padding: '6px 10px 2px' },
      React.createElement('table', { className: 'gx-tbl' },
        React.createElement('thead', null, React.createElement('tr', null,
          ['Cours', 'Durée', 'Capacité', solo ? 'Coach' : 'Coachs', 'Remplissage moyen', ''].map((h, i) => React.createElement('th', { key: i }, h)))),
        React.createElement('tbody', null,
          list.map((c) => {
            const itone = CR_INTENSITY[c.intensity] || 'neutral';
            return React.createElement('tr', { key: c.id, onClick: () => onOpen(c) },
              React.createElement('td', null, React.createElement('div', { className: 'u' },
                React.createElement('span', { className: 'gx-disc-ic t-' + itone }, React.createElement(Icon, { name: c.icon, size: 20 })),
                React.createElement('div', null,
                  React.createElement('div', { className: 'nm' }, c.name),
                  React.createElement('div', { className: 'em' }, c.discipline + ' · ' + c.intensity)))),
              React.createElement('td', { style: { color: 'var(--text-body)' } }, c.duration),
              React.createElement('td', { style: { color: 'var(--text-body)' } }, c.cap === 1 ? 'Privé (1)' : c.cap + ' places'),
              React.createElement('td', null,
                React.createElement('div', { className: 'gx-avstack' },
                  solo
                    ? React.createElement(CrAvatar, { name: meName, size: 'sm' })
                    : c.coachs.map((id) => { const co = coachById(id); return React.createElement(CrAvatar, { key: id, name: co ? co.name : '?', size: 'sm' }); }))),
              React.createElement('td', null,
                React.createElement('div', { style: { display: 'flex', alignItems: 'center', gap: 9 } },
                  React.createElement(Bar, { pct: c.fill, tone: c.fill >= 95 ? 'warn' : '', width: 96 }),
                  React.createElement('b', { style: { color: 'var(--text-strong)', fontSize: 'var(--text-xs)', fontVariantNumeric: 'tabular-nums' } }, c.fill + '%'))),
              React.createElement('td', { style: { textAlign: 'right', color: 'var(--text-subtle)' } }, React.createElement(Icon, { name: 'chevR', size: 16 })));
          })))));
}

/* ---------- fiche ---------- */
function NextRow({ n }) {
  const [day, time, studio, occ, cap] = n;
  const pct = Math.round(occ / cap * 100);
  return React.createElement('div', { className: 'gx-listrow' },
    React.createElement('span', { style: { width: 62, flex: 'none', fontSize: 'var(--text-2xs)', fontWeight: 700, color: 'var(--color-brand)', textTransform: 'uppercase', letterSpacing: '.03em' } }, day + ' · ' + time),
    React.createElement('div', { className: 'main' },
      React.createElement('div', { className: 't' }, studio)),
    React.createElement(Chip, { tone: pct >= 100 ? 'danger' : 'brand' }, cap === 1 ? 'Privé' : occ + '/' + cap));
}

function FicheCours({ cours, onBack, theme }) {
  const c = cours || window.GX_DATA.cours[0];
  const solo = theme && theme.solo;
  const meName = (theme && theme.manager && theme.manager.name) || 'Vous';
  const itone = CR_INTENSITY[c.intensity] || 'neutral';
  const stat = (lab, val, sub) => React.createElement('div', { className: 'gx-kpi' },
    React.createElement('div', { className: 'lab' }, lab),
    React.createElement('div', { className: 'val', style: { fontSize: 'var(--text-2xl)' } }, val),
    React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 3 } }, sub));
  const kv = (k, v) => React.createElement('div', { className: 'gx-kv' },
    React.createElement('span', { className: 'k' }, k), React.createElement('span', { className: 'v' }, v));

  return React.createElement(React.Fragment, null,
    React.createElement(Crumb, { parts: [{ label: 'Accueil' }, { label: 'Cours', to: 'cours' }, c.name], onNav: onBack }),
    React.createElement('div', { className: 'gx-fiche-head' },
      React.createElement('span', { className: 'gx-disc-ic big t-' + itone }, React.createElement(Icon, { name: c.icon, size: 28 })),
      React.createElement('div', { className: 'info' },
        React.createElement('div', { className: 'name-row' },
          React.createElement('span', { className: 'name' }, c.name),
          React.createElement(Chip, { tone: 'neutral' }, c.level)),
        React.createElement('div', { className: 'contact' }, `${c.discipline} · ${c.duration} · intensité ${c.intensity.toLowerCase()}`)),
      React.createElement(CrBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'copy', size: 18 }) }, 'Dupliquer'),
      React.createElement(CrBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'settings', size: 18 }) }, 'Éditer')),

    React.createElement('div', { className: 'gx-grid2', style: { gridTemplateColumns: '1.3fr 1fr', alignItems: 'start' } },
      // left
      React.createElement('div', { className: 'gx-col' },
        React.createElement('div', { className: 'gx-grid3' },
          stat('Séances / sem.', c.sessionsWeek, 'au planning'),
          stat('Remplissage', c.fill + '%', 'moyenne 30 j'),
          stat('Habitués', c.regulars, 'inscrits réguliers')),
        React.createElement(CrCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Description' }),
          React.createElement('div', { style: { padding: '14px 18px 16px', fontSize: 'var(--text-sm)', color: 'var(--text-body)', lineHeight: 1.55 } }, c.desc)),
        React.createElement(CrCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Prochaines séances' },
            React.createElement(CrBtn, { variant: 'ghost', size: 'sm', iconLeft: React.createElement(Icon, { name: 'calendar', size: 16 }) }, 'Planning')),
          React.createElement('div', { style: { padding: '4px 18px 8px' } },
            c.next.map((n, i) => React.createElement(NextRow, { key: i, n }))))),
      // right
      React.createElement('div', { className: 'gx-col' },
        React.createElement(CrCard, { padding: '0' },
          React.createElement(CardHead, { title: 'Paramètres par défaut' }),
          React.createElement('div', { style: { padding: '4px 18px 10px' } },
            kv('Durée', c.duration),
            kv('Capacité', c.cap === 1 ? '1 (privé)' : c.cap + ' places'),
            kv('Studio', c.studio),
            kv('Niveau', c.level),
            kv('Intensité', c.intensity),
            kv('Tarif', c.price))),
        React.createElement(CrCard, { padding: '0' },
          React.createElement(CardHead, { title: solo ? 'Encadré par' : 'Coachs habilités' }),
          solo
            ? React.createElement('div', { style: { padding: '6px 18px 8px' } },
                React.createElement('div', { className: 'gx-listrow' },
                  React.createElement(CrAvatar, { name: meName, size: 'md' }),
                  React.createElement('div', { className: 'main' },
                    React.createElement('div', { className: 't' }, meName),
                    React.createElement('div', { className: 's' }, (theme && theme.manager && theme.manager.role) || 'Coach')),
                  React.createElement(Chip, { tone: 'success' }, 'Vous')))
            : React.createElement('div', { style: { padding: '6px 18px 8px' } },
                c.coachs.map((id) => {
                  const co = coachById(id);
                  if (!co) return null;
                  return React.createElement('div', { className: 'gx-listrow', key: id },
                    React.createElement(CrAvatar, { name: co.name, size: 'md' }),
                    React.createElement('div', { className: 'main' },
                      React.createElement('div', { className: 't' }, co.name),
                      React.createElement('div', { className: 's' }, co.role)),
                    React.createElement(Chip, { tone: co.tone }, co.status));
                }))))));
}

function ScreenCours({ theme }) {
  const [view, setView] = React.useState('list');
  const [cours, setCours] = React.useState(null);
  return React.createElement('div', { className: 'gx-screen' },
    view === 'list'
      ? React.createElement(CoursList, { theme, onOpen: (c) => { setCours(c); setView('fiche'); } })
      : React.createElement(FicheCours, { cours, theme, onBack: () => setView('list') }));
}

window.ScreenCours = ScreenCours;
