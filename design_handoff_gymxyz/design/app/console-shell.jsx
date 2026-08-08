/* ============================================================
   GymXYZ — Console plateforme : shell + primitives partagées
   ConsoleShell · ObKv · Spark · MiniKpi · AlertRow · NoAccessNote
   ============================================================ */
const { Button: CBtn, Card: CCard, Avatar: CAvatar, Input: CInput, Select: CSelect } = window.TechXYZDesignSystem_ff9a8f;

/* ObKv est défini par l'onboarding ; la console est autonome. */
function ObKv({ rows }) {
  return <dl className="gx-kv2">{rows.map((r, i) => <React.Fragment key={i}><dt>{r[0]}</dt><dd>{r[1] || '—'}</dd></React.Fragment>)}</dl>;
}

const CON_NAV = [
  { group: 'Ce matin' },
  { id: 'vue', label: 'Vue d’ensemble', icon: 'home' },
  { group: 'Clients' },
  { id: 'demandes', label: 'Demandes', icon: 'file', countKey: 'demandes' },
  { id: 'clients', label: 'Clients', icon: 'building' },
  { id: 'facturation', label: 'Facturation', icon: 'euro', countKey: 'impayes', tone: 'bad' },
  { group: 'Assistance' },
  { id: 'support', label: 'Support', icon: 'mail', countKey: 'tickets' },
  { id: 'sante', label: 'Santé & journal', icon: 'cloud' },
  { group: 'Produit' },
  { id: 'formules', label: 'Formules & tarifs', icon: 'layers' },
  { id: 'referentiels', label: 'Référentiels', icon: 'grid' },
];

function ConsoleShell({ nav, onNav, counts, children }) {
  const me = window.GX_AUTH.admin;
  return (
    <div className="gxc-root">
      <div className="gxc-top">
        <span className="wm">TECH<i>XYZ</i></span>
        <span className="sepv"></span>
        <span>Console plateforme · <b>GymXYZ</b></span>
        <span className="sp"></span>
        <span className="env"><i></i>Production · v1.9.2</span>
        <span className="sepv"></span>
        <span>6 clients · 1 322 membres gérés</span>
      </div>
      <div className="gx-app">
        <aside className="gx-sb">
          <div className="gxc-brand">
            <span className="l1">GYM<i>XYZ</i></span>
            <span className="l2">Console plateforme</span>
          </div>
          {CON_NAV.map((it, i) => {
            if (it.group) return <div className="gx-sb-group" key={'g' + i}>{it.group}</div>;
            const n = it.countKey ? counts[it.countKey] : 0;
            return (
              <div key={it.id} className={'gx-nav' + (nav === it.id ? ' active' : '')} onClick={() => onNav(it.id)}>
                <Icon name={it.icon} size={19} /><span>{it.label}</span>
                {n ? <span className="count">{n}</span> : null}
              </div>
            );
          })}
          <div className="gx-sb-spacer"></div>
          <div className="gx-sb-foot">
            <a className="gx-nav" href="GymXYZ Desktop.html" style={{ textDecoration: 'none' }}>
              <Icon name="eye" size={19} /><span>Démo commerciale</span>
            </a>
            <div className="gx-theme-hint">
              <Icon name="shield" size={14} />
              <span>Vous ne voyez jamais les données d’un client. Chaque consultation est <b>journalisée</b>.</span>
            </div>
          </div>
        </aside>
        <div className="gx-main">
          <header className="gx-tb">
            <div className="gx-search"><Icon name="search" size={17} /><span>Rechercher un client, un ticket, une facture…</span></div>
            <div className="gx-tb-sp"></div>
            <span className="gx-adm-badge"><Icon name="shield" size={13} />Super-admin</span>
            <div className="gx-tb-ic"><Icon name="bell" size={20} /><span className="dot"></span></div>
            <span className="gx-tb-div"></span>
            <div className="gx-me">
              <CAvatar name={me.name} size="sm" />
              <div><div className="nm">{me.name}</div><div className="rl">{me.role} · TechXYZ</div></div>
            </div>
          </header>
          <div className="gx-content"><div className="gx-wrap">{children}</div></div>
        </div>
      </div>
    </div>
  );
}

/* ---------- histogramme 12 semaines ---------- */
function Spark({ data, labels }) {
  const max = Math.max.apply(null, data) || 1;
  return (
    <div>
      <div className="gxc-spark">
        {data.map((v, i) => <i key={i} className={i === data.length - 1 ? 'on' : ''} style={{ height: Math.max(2, Math.round(v / max * 78)) + 'px' }}></i>)}
      </div>
      <div className="gxc-axis">{(labels || ['il y a 12 semaines', 'cette semaine']).map((l, i) => <span key={i}>{l}</span>)}</div>
    </div>
  );
}

/* ---------- mini KPI en bandeau ---------- */
function MiniKpi({ label, value, sub }) {
  return <div className="gxc-mini"><div className="l">{label}</div><div className="v">{value}</div>{sub ? <div className="s">{sub}</div> : null}</div>;
}

/* ---------- ligne de la file « à traiter » ---------- */
function AlertRow({ tone, icon, title, detail, onClick }) {
  return (
    <div className="gxc-alert" onClick={onClick}>
      <span className={'ic ' + tone}><Icon name={icon} size={19} /></span>
      <span className="tx"><span className="t">{title}</span><span className="d">{detail}</span></span>
      <Icon name="chevR" size={18} className="go" />
    </div>
  );
}

function NoAccessNote({ children }) {
  return <div className="gxc-note"><Icon name="shield" size={15} /><span>{children}</span></div>;
}

const CON_TONE = { actif: 'success', essai: 'brand', suspendu: 'danger' };
const CON_STATUS = { actif: 'Actif', essai: 'Essai', suspendu: 'Suspendu' };
const CON_PAY = { paye: { l: 'Payée', t: 'success' }, echec: { l: 'Prélèvement rejeté', t: 'danger' }, impaye: { l: 'Impayée', t: 'danger' }, attente: { l: 'En attente', t: 'warning' } };
const CON_TK = { ouvert: { l: 'Ouvert', t: 'warning' }, 'en-cours': { l: 'En cours', t: 'brand' }, resolu: { l: 'Résolu', t: 'success' } };
function conInitials(n) { return n.split(/[\s—-]+/).filter(Boolean).slice(0, 2).map((w) => w[0]).join('').toUpperCase(); }

Object.assign(window, { ConsoleShell, ObKv, Spark, MiniKpi, AlertRow, NoAccessNote, CON_TONE, CON_STATUS, CON_PAY, CON_TK, conInitials, CON_NAV });
