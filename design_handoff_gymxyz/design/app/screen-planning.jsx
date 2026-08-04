/* ============================================================
   GymXYZ — Screen: Planning (week grid + day view + diffusion)
   ============================================================ */
const { Button: PlBtn, Card: PlCard, Switch: PlSwitch } = window.TechXYZDesignSystem_ff9a8f;

const GX_DAYS = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim'];

function EvBlock({ ev, onClick }) {
  const [name, meta, occ, cap] = ev;
  const full = occ >= cap && cap > 1;
  const priv = cap === 1;
  const cls = 'gx-ev' + (full ? ' full' : priv ? ' priv' : '');
  return React.createElement('div', { className: cls, onClick },
    React.createElement('div', { className: 't' }, name),
    React.createElement('div', { className: 'm' }, cap > 1 ? `${meta} · ${occ}/${cap}` : `${meta} · privé`));
}

function ScreenPlanning({ theme }) {
  const D = window.GX_DATA;
  const [view, setView] = React.useState('week');
  const [diffuse, setDiffuse] = React.useState(false);

  return React.createElement('div', { className: 'gx-screen' },
    view === 'week'
      ? React.createElement(PlanningWeek, { theme, onOpenDay: () => setView('day'), onDiffuse: () => setDiffuse(true) })
      : React.createElement(PlanningDay, { onBack: () => setView('week'), onDiffuse: () => setDiffuse(true) }),
    diffuse && React.createElement(DiffusionModal, { onClose: () => setDiffuse(false) }));
}

function PlanningWeek({ theme, onOpenDay, onDiffuse }) {
  const D = window.GX_DATA;
  const today = React.useMemo(() => new Date(), []);
  const [weekStart, setWeekStart] = React.useState(() => gxMonday(today));
  const weekDates = React.useMemo(() => Array.from({ length: 7 }, (_, i) => gxAddDays(weekStart, i)), [weekStart.getTime()]);
  const zip = (D.reglages && D.reglages.identite && D.reglages.identite.zip) || '69003';
  const cal = useSchoolCalendar(zip, weekDates);
  const wEnd = weekDates[6];
  const sub = 'Semaine du ' + gxPrettyIso(gxIso(weekStart)) + ' au ' + gxPrettyIso(gxIso(wEnd)) + ' ' + wEnd.getFullYear();
  const solo = theme && theme.solo;

  return React.createElement(React.Fragment, null,
    React.createElement(PageHead, { title: 'Planning', sub },
      React.createElement(PlBtn, { variant: 'outline', size: 'sm', onClick: () => setWeekStart(gxAddDays(weekStart, -7)) }, React.createElement(Icon, { name: 'chevL', size: 16 })),
      React.createElement(PlBtn, { variant: 'outline', size: 'sm', onClick: () => setWeekStart(gxMonday(new Date())) }, "Aujourd'hui"),
      React.createElement(PlBtn, { variant: 'outline', size: 'sm', onClick: () => setWeekStart(gxAddDays(weekStart, 7)) }, React.createElement(Icon, { name: 'chevR', size: 16 })),
      React.createElement(PlBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'share', size: 18 }), onClick: onDiffuse }, 'Diffuser')),
    React.createElement('div', { className: 'gx-filters' },
      !solo && React.createElement('span', { className: 'gx-fchip on' }, React.createElement(Icon, { name: 'filter', size: 14 }), 'Tous les coachs'),
      React.createElement('span', { className: 'gx-fchip' }, 'Tous les lieux'),
      React.createElement('span', { className: 'gx-fchip' }, 'Collectif'),
      React.createElement('span', { className: 'gx-fchip' }, 'Privé'),
      React.createElement('span', { style: { marginLeft: 'auto', fontSize: 'var(--text-xs)', color: 'var(--text-muted)', fontWeight: 600, display: 'inline-flex', alignItems: 'center', gap: 6 } },
        React.createElement(Icon, { name: 'arrowR', size: 13 }), "Cliquez l'en-tête d'un jour pour le détail")),
    React.createElement(PlCard, { padding: '0', style: { marginTop: 12 } }, React.createElement(WeekCalendar, { weekDates, cal, onOpenDay })));
}

function WeekCalendar({ weekDates, cal, onOpenDay }) {
  const D = window.GX_DATA;
  const scrollRef = React.useRef(null);
  const [now, setNow] = React.useState(null);
  const todayIso = gxIso(new Date());
  const todayIdx = weekDates.findIndex((d) => gxIso(d) === todayIso);
  const dayInfos = weekDates.map((d) => gxDayInfo(cal, d));
  const anchor = weekDates[0] && weekDates[0].getTime();

  React.useEffect(() => {
    const sc = scrollRef.current; if (!sc) return;
    const grid = sc.querySelector('.gx-cal');
    const cells = grid.querySelectorAll('.cell');
    const hds = grid.querySelectorAll('.hd');
    if (!cells.length) return;
    const rowH = cells[0].offsetHeight || 52;
    const headerH = cells[0].offsetTop;
    if (todayIdx < 0) { setNow(null); sc.scrollTop = Math.max(0, 3 * rowH); return; }
    const d = new Date();
    let h = d.getHours() + d.getMinutes() / 60;
    if (h < 7) h = 7; if (h > 21) h = 21;
    const top = headerH + (h - 7) * rowH;
    const hd = hds[todayIdx];
    setNow({ top, dotLeft: hd ? hd.offsetLeft + hd.offsetWidth / 2 - 4 : 0 });
    sc.scrollTop = Math.max(0, top - headerH - 60);
  }, [anchor]);

  const rows = [];
  D.hours.forEach((h, hi) => {
    const prev = D.hours[hi - 1];
    if (prev != null && h - prev > 1) {
      rows.push(React.createElement('div', { key: 'gap' + h, style: { gridColumn: '1 / -1', borderBottom: '1px dashed var(--border-subtle)', background: 'var(--surface-sunken)', height: 12, textAlign: 'center', color: 'var(--text-subtle)', fontSize: 10 } }, '· · ·'));
    }
    rows.push(React.createElement('div', { key: 'g' + h, className: 'gut' }, h + ':00'));
    for (let dd = 0; dd < 7; dd++) {
      const ev = D.events[dd] && D.events[dd][h];
      const mk = dayInfos[dd] ? ' ' + dayInfos[dd].type : '';
      rows.push(React.createElement('div', { key: h + '-' + dd, className: 'cell' + mk },
        ev ? React.createElement(EvBlock, { ev, onClick: () => onOpenDay(dd) }) : null));
    }
  });

  return React.createElement('div', { className: 'gx-cal-scroll', ref: scrollRef },
    React.createElement('div', { className: 'gx-cal' },
      React.createElement('div', { className: 'corner' }),
      weekDates.map((d, i) => {
        const info = dayInfos[i];
        return React.createElement('div', {
          key: i, className: 'hd' + (i === todayIdx ? ' today' : '') + (info ? ' ' + info.type : ''), onClick: () => onOpenDay(i),
        },
          React.createElement('div', { className: 'd' }, GX_DAYS[i]),
          React.createElement('div', { className: 'n' }, d.getDate() + '/' + String(d.getMonth() + 1).padStart(2, '0')),
          info && React.createElement('div', { className: 'gx-hd-mark ' + info.type, title: info.label },
            React.createElement(Icon, { name: info.type === 'ferie' ? 'star' : 'sun', size: 11 }),
            React.createElement('span', null, info.label)));
      }),
      rows),
    now && React.createElement('div', { className: 'gx-now-line', style: { top: now.top } }),
    now && React.createElement('div', { className: 'gx-now-dot', style: { top: now.top - 4, left: now.dotLeft } }));
}

function PlanningDay({ onBack, onDiffuse }) {
  const D = window.GX_DATA;
  const [checked, setChecked] = React.useState({ 0: true });
  const slots = [
    { time: '09:00', name: 'HIIT Blast', meta: 'Studio A · Nora Lemoine · 60 min', occ: 12, cap: 16 },
    { time: '18:00', name: 'Power Cycle', meta: 'Studio C · Nora Lemoine · 45 min', occ: 24, cap: 24 },
  ];
  return React.createElement(React.Fragment, null,
    React.createElement(PageHead, { title: 'Vendredi 12 juin', sub: '2 cours · 36 participants attendus' },
      React.createElement(PlBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'chevL', size: 18 }), onClick: onBack }, 'Semaine'),
      React.createElement(PlBtn, { variant: 'outline', iconRight: React.createElement(Icon, { name: 'chevR', size: 18 }) }, 'Jour suivant'),
      React.createElement(PlBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'plus', size: 18 }) }, 'Cours')),
    React.createElement('div', { className: 'gx-grid2', style: { gridTemplateColumns: '1.6fr 1fr', alignItems: 'start' } },
      React.createElement(PlCard, null,
        React.createElement('div', { style: { fontSize: 'var(--text-sm)', fontWeight: 700, color: 'var(--text-strong)', marginBottom: 16 } }, 'Déroulé & présences'),
        slots.map((s, i) => {
          const pct = Math.round(s.occ / s.cap * 100);
          const done = checked[i];
          return React.createElement('div', { className: 'gx-slot', key: i },
            React.createElement('div', { className: 'time' }, s.time),
            React.createElement('div', { className: 'card', style: pct >= 100 ? { borderLeftColor: 'var(--color-danger)' } : null },
              React.createElement('div', { className: 'top' },
                React.createElement('span', { className: 'nm' }, s.name),
                React.createElement(Chip, { tone: pct >= 100 ? 'danger' : 'brand' }, s.occ + '/' + s.cap),
                React.createElement('span', { style: { marginLeft: 'auto' } },
                  done
                    ? React.createElement(Chip, { tone: 'success', icon: 'check' }, 'Pointé')
                    : React.createElement(PlBtn, { variant: 'outline', size: 'sm', iconLeft: React.createElement(Icon, { name: 'qr', size: 16 }), onClick: () => setChecked({ ...checked, [i]: true }) }, 'Pointer'))),
              React.createElement('div', { className: 'meta' }, s.meta),
              React.createElement(Bar, { pct, tone: pct >= 100 ? 'bad' : '' })));
        })),
      React.createElement(PlCard, { padding: '0' },
        React.createElement(CardHead, { title: 'Résumé du jour' }),
        React.createElement('div', { style: { padding: '16px 18px', display: 'flex', flexDirection: 'column', gap: 14 } },
          summaryRow('Taux de remplissage', '90%'),
          React.createElement(Bar, { pct: 90 }),
          summaryRow('Présences pointées', Object.keys(checked).length + ' / 2 cours'),
          summaryRow('Liste d\'attente', React.createElement('b', { style: { color: 'var(--color-brand)' } }, '3 membres')),
          React.createElement('div', { style: { height: 1, background: 'var(--border-subtle)' } }),
          React.createElement(PlBtn, { variant: 'outline', iconLeft: React.createElement(Icon, { name: 'eye', size: 18 }), style: { justifyContent: 'center' } }, 'Aperçu membre du jour')))));
}

function summaryRow(label, val) {
  return React.createElement('div', { style: { display: 'flex', justifyContent: 'space-between', fontSize: 'var(--text-sm)' } },
    React.createElement('span', { style: { color: 'var(--text-muted)' } }, label),
    React.createElement('b', null, val));
}

/* ---------- Diffusion modal ---------- */
function ChannelRow({ ic, name, sub, on, onToggle }) {
  return React.createElement('label', {
    style: {
      display: 'flex', alignItems: 'center', gap: 12, padding: '12px 14px',
      border: '1px solid ' + (on ? 'var(--color-primary)' : 'var(--border-subtle)'),
      background: on ? 'var(--azure-50)' : 'var(--surface-card)', borderRadius: 'var(--radius-md)', cursor: 'pointer',
    },
  },
    React.createElement('span', { style: { width: 32, height: 32, borderRadius: 'var(--radius-sm)', flex: 'none', display: 'grid', placeItems: 'center', background: on ? 'var(--color-primary)' : 'var(--surface-sunken)', color: on ? '#fff' : 'var(--text-muted)' } },
      React.createElement(Icon, { name: ic, size: 18 })),
    React.createElement('span', { style: { flex: 1 } },
      React.createElement('span', { style: { display: 'block', fontSize: 'var(--text-sm)', fontWeight: 700, color: 'var(--text-strong)' } }, name),
      React.createElement('span', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)' } }, sub)),
    React.createElement(PlSwitch, { checked: on, onChange: onToggle }));
}

function DiffusionModal({ onClose }) {
  const [ch, setCh] = React.useState({ app: true, mail: true, link: true });
  const t = (k) => setCh({ ...ch, [k]: !ch[k] });
  return React.createElement('div', { className: 'gx-scrim', onClick: onClose, style: { display: 'flex', alignItems: 'flex-start', justifyContent: 'center', paddingTop: 60 } },
    React.createElement('div', {
      onClick: (e) => e.stopPropagation(),
      style: { width: 560, maxWidth: '94vw', background: 'var(--surface-card)', borderRadius: 'var(--radius-xl)', boxShadow: 'var(--shadow-xl)', overflow: 'hidden', animation: 'gx-rise .24s var(--ease-out) both' },
    },
      React.createElement('div', { style: { padding: '20px 22px', borderBottom: '1px solid var(--border-subtle)', display: 'flex', alignItems: 'center', gap: 12 } },
        React.createElement('span', { style: { width: 40, height: 40, borderRadius: 'var(--radius-md)', background: 'var(--azure-50)', color: 'var(--color-primary)', display: 'grid', placeItems: 'center', flex: 'none' } }, React.createElement(Icon, { name: 'share', size: 20 })),
        React.createElement('div', { style: { flex: 1 } },
          React.createElement('div', { style: { fontSize: 'var(--text-lg)', fontWeight: 700, color: 'var(--text-strong)' } }, 'Diffuser le planning de la semaine'),
          React.createElement('div', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)' } }, '8 au 14 juin · 28 cours · visible par 128 membres')),
        React.createElement(IconButton, { label: 'Fermer', variant: 'ghost', onClick: onClose }, React.createElement(Icon, { name: 'x', size: 20 }))),
      React.createElement('div', { style: { padding: '20px 22px', display: 'flex', flexDirection: 'column', gap: 11 } },
        React.createElement('div', { style: { fontSize: 'var(--text-2xs)', fontWeight: 700, letterSpacing: 'var(--tracking-wider)', textTransform: 'uppercase', color: 'var(--text-muted)' } }, 'Canaux'),
        React.createElement(ChannelRow, { ic: 'bell', name: "Notification dans l'app", sub: "Push aux membres avec l'app installée", on: ch.app, onToggle: () => t('app') }),
        React.createElement(ChannelRow, { ic: 'mail', name: 'E-mail récapitulatif', sub: 'Vos membres abonnés aux e-mails', on: ch.mail, onToggle: () => t('mail') }),
        React.createElement(ChannelRow, { ic: 'share', name: 'Lien public partageable', sub: 'À coller sur Instagram / WhatsApp', on: ch.link, onToggle: () => t('link') }),
        React.createElement('div', { style: { marginTop: 6, background: 'var(--surface-sunken)', border: '1px dashed var(--border-default)', borderRadius: 'var(--radius-md)', padding: '12px 14px' } },
          React.createElement('div', { style: { fontSize: 'var(--text-2xs)', fontWeight: 700, letterSpacing: '.06em', textTransform: 'uppercase', color: 'var(--text-muted)', marginBottom: 6 } }, 'Message (optionnel)'),
          React.createElement('div', { style: { fontSize: 'var(--text-sm)', color: 'var(--text-body)' } }, '« Voici le programme de la semaine — réservez vite, le Power Cycle de vendredi affiche déjà complet ! »'))),
      React.createElement('div', { style: { padding: '16px 22px', borderTop: '1px solid var(--border-subtle)', display: 'flex', alignItems: 'center', gap: 10 } },
        React.createElement('span', { style: { fontSize: 'var(--text-xs)', color: 'var(--text-muted)', display: 'inline-flex', alignItems: 'center', gap: 6 } },
          React.createElement(Icon, { name: 'clock', size: 14 }), 'Programmer plus tard'),
        React.createElement('span', { style: { marginLeft: 'auto', display: 'flex', gap: 10 } },
          React.createElement(PlBtn, { variant: 'ghost', onClick: onClose }, 'Annuler'),
          React.createElement(PlBtn, { variant: 'primary', iconLeft: React.createElement(Icon, { name: 'send', size: 18 }), onClick: onClose }, 'Diffuser maintenant')))));
}

window.ScreenPlanning = ScreenPlanning;
