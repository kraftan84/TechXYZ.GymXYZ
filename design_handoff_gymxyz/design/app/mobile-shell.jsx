/* ============================================================
   GymXYZ Mobile — shell + shared primitives
   Header · Brand · TabBar · Plus sheet · Theme sheet · Card ·
   Section · Kpi · Bar · Ring · Avatar helpers.
   Composes TechXYZ DS components (Badge, Avatar, Switch…).
   ============================================================ */
const { Badge: MBadge, Avatar: MAvatar, Switch: MSwitch } = window.TechXYZDesignSystem_ff9a8f;

/* ---------- Brand lockup (mark + themed wordmark) ---------- */
function MBrand({ theme }) {
  const mark = theme.markType === 'kettlebell'
    ? React.createElement(KettlebellMark, { size: 34 })
    : React.createElement('img', { src: theme.markSrc, alt: theme.name });
  const wm = theme.wordmark.full
    ? React.createElement('span', { className: 'gx-m-wordmark' }, theme.wordmark.full)
    : React.createElement('span', { className: 'gx-m-wordmark' },
        theme.wordmark.a, React.createElement('span', { className: 'accent' }, theme.wordmark.b));
  return React.createElement('div', { className: 'gx-m-brand' },
    React.createElement('span', { className: 'mark' + (theme.circle ? ' circle' : '') }, mark), wm);
}

/* ---------- Chip (wraps DS Badge) ---------- */
function MChip({ tone = 'neutral', icon, solid, children }) {
  return React.createElement(MBadge, {
    tone, variant: solid ? 'solid' : 'soft',
    iconLeft: icon ? React.createElement(Icon, { name: icon, size: 12 }) : null,
  }, children);
}

/* ---------- Bar / Ring ---------- */
function MBar({ pct, tone }) {
  return React.createElement('div', { className: 'gx-m-bar' + (tone ? ' ' + tone : '') },
    React.createElement('i', { style: { width: Math.min(pct, 100) + '%' } }));
}
function MRing({ pct, label, size = 72 }) {
  const r = size / 2 - 5, c = 2 * Math.PI * r, off = c * (1 - pct / 100);
  return React.createElement('div', { className: 'gx-m-ring', style: { width: size, height: size } },
    React.createElement('svg', { width: size, height: size },
      React.createElement('circle', { cx: size / 2, cy: size / 2, r, stroke: 'var(--surface-sunken)', strokeWidth: 5, fill: 'none' }),
      React.createElement('circle', { cx: size / 2, cy: size / 2, r, stroke: 'var(--color-primary)', strokeWidth: 5, fill: 'none', strokeLinecap: 'round', strokeDasharray: c, strokeDashoffset: off, transform: `rotate(-90 ${size / 2} ${size / 2})` })),
    React.createElement('span', { className: 'ctr' }, label != null ? label : pct + '%'));
}

/* ---------- KPI tile ---------- */
function MKpi({ label, value, delta, deltaIcon, deltaTone, sub, spark, three }) {
  return React.createElement('div', { className: 'gx-m-kpi' + (spark ? ' spark' : '') + (three ? ' three' : '') },
    React.createElement('div', { className: 'lab' }, label),
    React.createElement('div', { className: 'val' }, value),
    delta ? React.createElement('div', { className: 'delta', style: deltaTone ? { color: deltaTone } : null },
      deltaIcon && React.createElement(Icon, { name: deltaIcon, size: 13 }), delta)
      : (sub ? React.createElement('div', { className: 'sub' }, sub) : null));
}

/* ---------- Card / Card head / Section ---------- */
function MCard({ children, style, flush }) {
  return React.createElement('div', { className: 'gx-m-card', style }, children);
}
function MCardHead({ title, right }) {
  return React.createElement('div', { className: 'gx-m-card-h' },
    React.createElement('span', { className: 't' }, title),
    right && React.createElement('span', { className: 'r' }, right));
}
function MSection({ title, more, onMore }) {
  return React.createElement('div', { className: 'gx-m-sec' },
    React.createElement('span', { className: 't' }, title),
    more && React.createElement('span', { className: 'more', onClick: onMore }, more, React.createElement(Icon, { name: 'chevR', size: 14 })));
}

/* ---------- Discipline / studio icon tile ---------- */
function MIcTile({ name, tone = 'brand', lg }) {
  return React.createElement('span', { className: 'gx-m-ic t-' + tone + (lg ? ' lg' : '') },
    React.createElement(Icon, { name, size: lg ? 26 : 21 }));
}

/* ---------- Empty state ---------- */
function MEmpty({ icon, title, text }) {
  return React.createElement('div', { className: 'gx-m-empty gx-m-screen' },
    React.createElement('div', { className: 'ic' }, React.createElement(Icon, { name: icon || 'sparkles', size: 28 })),
    React.createElement('h3', null, title),
    React.createElement('p', null, text));
}

/* ============================================================
   HEADER — two modes: brand (root screens) / title+back (sub)
   ============================================================ */
function MHead({ theme, mode, title, onBack, action, children }) {
  return React.createElement('header', { className: 'gx-m-head' },
    React.createElement('div', { className: 'gx-m-head-top' },
      mode === 'sub'
        ? React.createElement(React.Fragment, null,
            React.createElement('button', { className: 'gx-m-back', onClick: onBack, 'aria-label': 'Retour' },
              React.createElement(Icon, { name: 'chevL', size: 24 })),
            React.createElement('div', { className: 'gx-m-htitle' }, title))
        : React.createElement('div', { className: 'lead' }, React.createElement(MBrand, { theme })),
      action),
    children);
}

/* ---------- Header round icon button ---------- */
function MHeadBtn({ icon, dot, onClick, label }) {
  return React.createElement('button', { className: 'gx-m-iconbtn', onClick, 'aria-label': label || icon },
    React.createElement(Icon, { name: icon, size: 20 }),
    dot && React.createElement('span', { className: 'dot' }));
}

/* ============================================================
   BOTTOM TAB BAR
   ============================================================ */
function MTabBar({ active, onNavigate, onPlus, plusActive, counts }) {
  const TABS = [
    { id: 'accueil', label: 'Accueil', icon: 'home' },
    { id: 'planning', label: 'Planning', icon: 'calendar' },
    { id: 'presences', label: 'Présences', icon: 'check', count: counts && counts.presences },
    { id: 'membres', label: 'Membres', icon: 'users' },
    { id: '__plus', label: 'Plus', icon: 'grid' },
  ];
  return React.createElement('nav', { className: 'gx-m-tabbar' },
    TABS.map((t) => {
      const isPlus = t.id === '__plus';
      const on = isPlus ? plusActive : active === t.id;
      return React.createElement('button', {
        key: t.id, className: 'gx-m-tab' + (on ? ' on' : ''),
        onClick: () => isPlus ? onPlus() : onNavigate(t.id),
      },
        React.createElement('span', { className: 'ti' },
          React.createElement(Icon, { name: t.icon, size: 24 }),
          t.count ? React.createElement('span', { className: 'badge' }, t.count) : null),
        React.createElement('span', { className: 'lbl' }, t.label));
    }));
}

/* ============================================================
   SHEET shell
   ============================================================ */
function MSheet({ title, onClose, children }) {
  return React.createElement(React.Fragment, null,
    React.createElement('div', { className: 'gx-m-sheet-scrim', onClick: onClose }),
    React.createElement('div', { className: 'gx-m-sheet' },
      React.createElement('div', { className: 'grab' }),
      React.createElement('div', { className: 'gx-m-sheet-h' },
        React.createElement('span', { className: 't' }, title),
        React.createElement('button', { className: 'x', onClick: onClose, 'aria-label': 'Fermer' },
          React.createElement(Icon, { name: 'x', size: 18 }))),
      React.createElement('div', { className: 'gx-m-sheet-body' }, children)));
}

/* ---------- Plus sheet : the rest of the nav + theme switch ---------- */
function MPlusSheet({ theme, onClose, onNavigate }) {
  const m = theme.manager || window.GX_DATA.manager;
  const D = window.GX_DATA;
  const items = [
    { id: 'coachs', t: 'Coachs', c: D.coachs.length + ' coachs', icon: 'user', solo: true },
    { id: 'cours', t: 'Cours', c: D.cours.length + ' modèles', icon: 'dumbbell' },
    { id: 'abos', t: 'Abonnements', c: D.abos.kpis.active + ' actifs', icon: 'card' },
    { id: 'salles', t: 'Lieux', c: D.salles.length + ' lieux', icon: 'pin' },
    { id: 'reglages', t: 'Réglages', c: 'Salle & équipe', icon: 'settings' },
    { id: 'administration', t: 'Administration', c: 'Compte & facturation', icon: 'shield' },
  ];
  return React.createElement(MSheet, { title: 'Plus', onClose },
    // profile
    React.createElement('div', { className: 'gx-m-profile' },
      React.createElement(MAvatar, { name: m.name, size: 'md' }),
      React.createElement('div', { className: 'main' },
        React.createElement('div', { className: 'nm' }, m.name),
        React.createElement('div', { className: 'rl' }, m.role + ' · ' + theme.name))),
    React.createElement('div', { className: 'gx-m-plusgrid' },
      items.filter((it) => !(it.solo && theme.solo)).map((it) =>
        React.createElement('button', { key: it.id, className: 'gx-m-plusitem', onClick: () => { onNavigate(it.id); onClose(); } },
          React.createElement('span', { className: 'si' }, React.createElement(Icon, { name: it.icon, size: 20 })),
          React.createElement('span', { className: 'tt' },
            React.createElement('span', { className: 't' }, it.t),
            React.createElement('span', { className: 'c' }, it.c))))));
}

/* ---------- Root header actions (bell + avatar→Plus) ---------- */
function MRootActions({ theme, onOpenPlus }) {
  const m = theme.manager || window.GX_DATA.manager;
  return React.createElement('div', { className: 'gx-m-flex', style: { gap: 8 } },
    React.createElement(MHeadBtn, { icon: 'bell', dot: true, label: 'Notifications' }),
    React.createElement('button', {
      className: 'gx-m-iconbtn', onClick: onOpenPlus, 'aria-label': 'Profil & plus',
      style: { padding: 0, overflow: 'hidden' },
    }, React.createElement(MAvatar, { name: m.name, size: 'sm' })));
}

/* ---------- Large page title (iOS-style, lives in scroll body) ---------- */
function MPageTitle({ title, sub, action }) {
  return React.createElement('div', { className: 'gx-m-greet', style: { display: 'flex', alignItems: 'flex-start', gap: 12 } },
    React.createElement('div', { style: { flex: 1, minWidth: 0 } },
      React.createElement('div', { className: 'hi' }, title),
      sub && React.createElement('div', { className: 'sub' }, sub)),
    action && React.createElement('div', { style: { flex: 'none', paddingTop: 4 } }, action));
}

Object.assign(window, {
  MBrand, MChip, MBar, MRing, MKpi, MCard, MCardHead, MSection, MIcTile, MEmpty,
  MHead, MHeadBtn, MTabBar, MSheet, MPlusSheet, MRootActions, MPageTitle,
});
