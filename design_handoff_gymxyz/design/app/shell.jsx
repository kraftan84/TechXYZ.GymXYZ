/* ============================================================
   GymXYZ — App shell + shared UI primitives
   Brand · Sidebar · Topbar · Chip · Bar · Ring · Kpi · PageHead
   Composes TechXYZ DS components (Button, Badge, Avatar, Card…).
   ============================================================ */
const { Button, Badge, Avatar, Card, IconButton } = window.TechXYZDesignSystem_ff9a8f;

/* ---------- Brand lockup (mark + themed wordmark) ---------- */
function Brand({ theme, size = 'md', onDark = false }) {
  const useDark = onDark && theme.markSrcDark;
  const mark = theme.markType === 'kettlebell'
    ? React.createElement(KettlebellMark, { size: size === 'lg' ? 44 : 38 })
    : React.createElement('img', { src: useDark ? theme.markSrcDark : theme.markSrc, alt: theme.name });
  const wm = theme.wordmark.full
    ? React.createElement('span', { className: 'gx-wordmark' }, theme.wordmark.full)
    : React.createElement('span', { className: 'gx-wordmark' },
        theme.wordmark.a,
        React.createElement('span', { className: 'accent' }, theme.wordmark.b));
  return React.createElement('div', { className: 'gx-brand' },
    React.createElement('span', { className: 'mark' + (theme.circle ? ' circle' : '') }, mark),
    wm);
}

/* ---------- Status chip (wraps DS Badge) ---------- */
function Chip({ tone = 'neutral', icon, solid, children }) {
  return React.createElement(Badge, {
    tone, variant: solid ? 'solid' : 'soft',
    iconLeft: icon ? React.createElement(Icon, { name: icon, size: 13 }) : null,
  }, children);
}

/* ---------- Gauge bar ---------- */
function Bar({ pct, tone, width }) {
  const cls = 'gx-bar' + (tone ? ' ' + tone : '');
  return React.createElement('div', { className: cls, style: width ? { width } : null },
    React.createElement('i', { style: { width: Math.min(pct, 100) + '%' } }));
}

/* ---------- Ring gauge ---------- */
function Ring({ pct, label, size = 84 }) {
  const r = size / 2 - 6, c = 2 * Math.PI * r, off = c * (1 - pct / 100);
  return React.createElement('div', { className: 'gx-ring', style: { width: size, height: size } },
    React.createElement('svg', { width: size, height: size },
      React.createElement('circle', { cx: size / 2, cy: size / 2, r, stroke: 'var(--surface-sunken)', strokeWidth: 6, fill: 'none' }),
      React.createElement('circle', {
        cx: size / 2, cy: size / 2, r, stroke: 'var(--color-primary)', strokeWidth: 6, fill: 'none',
        strokeLinecap: 'round', strokeDasharray: c, strokeDashoffset: off,
        transform: `rotate(-90 ${size / 2} ${size / 2})`,
      })),
    React.createElement('span', { className: 'ctr' }, label != null ? label : pct + '%'));
}

/* ---------- KPI card ---------- */
function Kpi({ label, value, sub, delta, deltaIcon, deltaTone, spark, valueColor }) {
  return React.createElement('div', { className: 'gx-kpi' + (spark ? ' spark' : '') },
    React.createElement('div', { className: 'lab' }, label),
    React.createElement('div', { className: 'val', style: valueColor ? { color: valueColor } : null }, value),
    delta
      ? React.createElement('div', { className: 'delta', style: deltaTone ? { color: deltaTone } : null },
          deltaIcon && React.createElement(Icon, { name: deltaIcon, size: 14 }), delta)
      : (sub ? React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 4 } }, sub) : null));
}

/* ---------- Page head ---------- */
function PageHead({ title, sub, children }) {
  return React.createElement('div', { className: 'gx-pagehead' },
    React.createElement('div', null,
      React.createElement('div', { className: 'h' }, title),
      sub && React.createElement('div', { className: 'sub' }, sub)),
    children && React.createElement('div', { className: 'right' }, children));
}

/* ---------- Breadcrumb ---------- */
function Crumb({ parts, onNav }) {
  return React.createElement('div', { className: 'gx-crumb' },
    React.createElement(Icon, { name: 'home', size: 14 }),
    parts.map((p, i) => {
      const last = i === parts.length - 1;
      return React.createElement(React.Fragment, { key: i },
        i > 0 && React.createElement(Icon, { name: 'chevR', size: 13, className: 'sep' }),
        last
          ? React.createElement('b', null, p.label || p)
          : React.createElement('span', {
              className: p.to ? 'nav-link' : null,
              onClick: p.to && onNav ? () => onNav(p.to) : null,
            }, p.label || p));
    }));
}

/* ---------- Card head ---------- */
function CardHead({ title, children }) {
  return React.createElement('div', { className: 'gx-card-h' },
    React.createElement('span', { className: 't' }, title),
    children && React.createElement('span', { className: 'r' }, children));
}

/* ============================================================
   SIDEBAR
   ============================================================ */
const GX_NAV = [
  { group: 'Pilotage' },
  { id: 'accueil', label: 'Accueil', icon: 'home' },
  { id: 'planning', label: 'Planning', icon: 'calendar' },
  { id: 'presences', label: 'Présences', icon: 'check', count: '3' },
  { group: 'Personnes' },
  { id: 'membres', label: 'Membres', icon: 'users' },
  { id: 'coachs', label: 'Coachs', icon: 'user' },
  { group: 'Offre & business' },
  { id: 'cours', label: 'Cours', icon: 'dumbbell' },
  { id: 'abos', label: 'Abonnements', icon: 'card', count: '6' },
  { group: 'Lieux' },
  { id: 'salles', label: 'Lieux', icon: 'pin' },
];
const GX_BUILT = ['accueil', 'planning', 'presences', 'membres', 'coachs', 'cours', 'abos', 'salles', 'reglages', 'administration'];

function Sidebar({ theme, active, onNavigate }) {
  return React.createElement('aside', { className: 'gx-sb' },
    React.createElement(Brand, { theme, onDark: true }),
    GX_NAV.map((item, i) => {
      if (item.group) return React.createElement('div', { className: 'gx-sb-group', key: 'g' + i }, item.group);
      if (item.id === 'coachs' && theme.solo) return null;
      const isActive = active === item.id;
      return React.createElement('div', {
        key: item.id,
        className: 'gx-nav' + (isActive ? ' active' : ''),
        onClick: () => onNavigate(item.id),
      },
        React.createElement(Icon, { name: item.icon, size: 19 }),
        React.createElement('span', null, item.label),
        item.count && React.createElement('span', { className: 'count' }, item.count));
    }),
    React.createElement('div', { className: 'gx-sb-spacer' }),
    React.createElement('div', { className: 'gx-sb-foot' },
      React.createElement('div', {
        className: 'gx-nav' + (active === 'administration' ? ' active' : ''),
        onClick: () => onNavigate('administration'),
      },
        React.createElement(Icon, { name: 'shield', size: 19 }),
        React.createElement('span', null, 'Administration')),
      React.createElement('div', { className: 'gx-theme-hint' },
        React.createElement(Icon, { name: 'palette', size: 14 }),
        React.createElement('span', null,
          'Habillage complet — ouvrez ', React.createElement('b', null, 'Tweaks'), ' pour changer de marque.'))));
}

/* ============================================================
   TOPBAR
   ============================================================ */
function Topbar({ theme, active, onNavigate }) {
  const m = theme.manager || window.GX_DATA.manager;
  return React.createElement('header', { className: 'gx-tb' },
    React.createElement('div', { className: 'gx-search' },
      React.createElement(Icon, { name: 'search', size: 17 }),
      React.createElement('span', null, 'Rechercher un membre, un cours…')),
    React.createElement('div', { className: 'gx-tb-sp' }),
    React.createElement('div', { className: 'gx-tb-ic' },
      React.createElement(Icon, { name: 'bell', size: 20 }),
      React.createElement('span', { className: 'dot' })),
    React.createElement('div', {
      className: 'gx-tb-ic' + (active === 'reglages' ? ' on' : ''),
      title: 'Réglages',
      onClick: onNavigate ? () => onNavigate('reglages') : null,
    },
      React.createElement(Icon, { name: 'settings', size: 20 })),
    React.createElement('span', { className: 'gx-tb-div' }),
    React.createElement('div', { className: 'gx-me' },
      React.createElement(Avatar, { name: m.name, size: 'sm' }),
      React.createElement('div', null,
        React.createElement('div', { className: 'nm' }, m.name),
        React.createElement('div', { className: 'rl' }, m.role + ' · ' + theme.name))));
}

/* ============================================================
   EMPTY STATE (sections not yet built)
   ============================================================ */
function EmptyState({ icon, title, text }) {
  return React.createElement('div', { className: 'gx-empty gx-screen' },
    React.createElement('div', { className: 'ic' }, React.createElement(Icon, { name: icon || 'sparkles', size: 30 })),
    React.createElement('h3', null, title),
    React.createElement('p', null, text),
    React.createElement(Button, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'arrowR', size: 18 }) }, 'Au programme du prochain lot'));
}

Object.assign(window, {
  Brand, Chip, Bar, Ring, Kpi, PageHead, Crumb, CardHead,
  Sidebar, Topbar, EmptyState, GX_NAV, GX_BUILT,
});
