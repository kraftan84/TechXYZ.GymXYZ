/* ============================================================
   GymXYZ — Auth & Onboarding : racine du prototype
   Un seul document pour tout le parcours d'entrée :
   connexion (3 marques) · mot de passe oublié · demande
   d'ouverture d'espace · traitement côté super-admin.
   Desktop et mobile, pilotés depuis le panneau Tweaks.
   ============================================================ */
const AUTH_TWEAK_DEFAULTS = /*EDITMODE-BEGIN*/{
  "surface": "desktop",
  "theme": "techxyz",
  "anim": true
}/*EDITMODE-END*/;

const AUTH_ROUTES = [
  ['login', 'Connexion'],
  ['forgot', 'Mot de passe oublié'],
  ['sent', 'Lien envoyé'],
  ['reset', 'Nouveau mot de passe'],
  ['reset-done', 'Mot de passe modifié'],
  ['ob', 'Demande d’ouverture'],
  ['ob-sent', 'Demande envoyée'],
  ['admin', 'Super-admin · demandes'],
];
/* écrans côté GymXYZ : jamais habillés aux couleurs d'un client */
const AUTH_GYM_ONLY = ['ob', 'ob-sent', 'admin'];

function AuThemePicker({ value, onChange }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 7 }}>
      {window.GX_THEMES.map((th) => {
        const on = th.id === value;
        return (
          <button key={th.id} type="button" onClick={() => onChange(th.id)} style={{
            display: 'flex', alignItems: 'center', gap: 10, width: '100%', textAlign: 'left',
            padding: '8px 10px', borderRadius: 9, cursor: 'pointer',
            border: '1px solid ' + (on ? 'rgba(0,0,0,.8)' : 'rgba(0,0,0,.1)'),
            background: on ? 'rgba(255,255,255,.92)' : 'rgba(255,255,255,.5)',
          }}>
            <span style={{ display: 'flex', borderRadius: 5, overflow: 'hidden', flex: 'none' }}>
              {th.swatch.map((c, i) => <span key={i} style={{ width: 13, height: 26, background: c }} />)}
            </span>
            <span style={{ flex: 1, minWidth: 0 }}>
              <span style={{ display: 'block', fontWeight: 600, fontSize: 12, color: '#29261b' }}>{th.label}</span>
              <span style={{ display: 'block', fontSize: 10.5, color: 'rgba(41,38,27,.55)' }}>{th.sub}</span>
            </span>
          </button>
        );
      })}
    </div>
  );
}

/* ---------- écran mobile mis à l'échelle dans un iPhone ---------- */
function AuDeviceScale(w, h) {
  const [scale, setScale] = React.useState(1);
  React.useEffect(() => {
    const fit = () => {
      const s = Math.min((window.innerWidth - 28) / w, (window.innerHeight - 28) / h, 1);
      setScale(s > 0 ? s : 1);
    };
    fit();
    window.addEventListener('resize', fit);
    return () => window.removeEventListener('resize', fit);
  }, [w, h]);
  return scale;
}

/* ---------- effet d'une action admin sur la liste ---------- */
function auApplyAct(list, kind, d, payload) {
  const now = '6 août · ' + new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
  const me = window.GX_AUTH.admin.name;
  return list.map((x) => {
    if (x.id !== d.id) return x;
    if (kind === 'valider') return { ...x, status: 'validee', owner: me,
      activity: x.activity.concat([{ t: 'Demande validée', d: `Espace ${payload.sub}.gymxyz.fr · formule ${payload.plan}`, when: now, state: 'done' }]) };
    if (kind === 'refuser') return { ...x, status: 'refusee', owner: me,
      activity: x.activity.concat([{ t: 'Demande refusée', d: payload.motif, when: now, state: 'done' }]) };
    if (kind === 'complement') return { ...x, status: 'en-cours', owner: me,
      activity: x.activity.concat([{ t: 'Complément demandé', d: payload.msg, when: now, state: 'now' }]) };
    return x;
  });
}

function AuthApp() {
  const [t, setTweak] = useTweaks(AUTH_TWEAK_DEFAULTS);
  const [route, setRoute] = React.useState('login');
  const [email, setEmail] = React.useState('');
  const [demandes, setDemandes] = React.useState(window.GX_AUTH.demandes);
  const [focus, setFocus] = React.useState(null);
  const [sent, setSent] = React.useState(null);
  const [toast, setToast] = React.useState(null);
  const DEV_W = 402, DEV_H = 874;
  const scale = AuDeviceScale(DEV_W, DEV_H);
  const mobile = t.surface === 'mobile';

  React.useEffect(() => {
    try {
      const s = localStorage.getItem('gxa-route');
      if (s && AUTH_ROUTES.some((r) => r[0] === s)) setRoute(s);
      const th = localStorage.getItem('gxa-theme');
      if (th && window.GX_THEMES.some((x) => x.id === th)) setTweak('theme', th);
    } catch (e) {}
  }, []);
  React.useEffect(() => { try { localStorage.setItem('gxa-route', route); } catch (e) {} }, [route]);
  React.useEffect(() => { try { localStorage.setItem('gxa-theme', t.theme); } catch (e) {} }, [t.theme]);

  const gymOnly = AUTH_GYM_ONLY.indexOf(route) !== -1;
  const themeId = gymOnly ? 'techxyz' : t.theme;
  React.useEffect(() => { document.documentElement.dataset.theme = themeId; }, [themeId]);
  const theme = window.GX_THEMES.find((x) => x.id === themeId) || window.GX_THEMES[0];

  React.useEffect(() => { if (!toast) return; const x = setTimeout(() => setToast(null), 3200); return () => clearTimeout(x); }, [toast]);

  const go = (r, payload) => {
    if (payload && payload.email) setEmail(payload.email);
    setRoute(r);
    if (r !== 'admin') setFocus(null);
  };

  /* une demande envoyée arrive en tête de la file super-admin */
  const submit = (f) => {
    const ref = 'DEM-2026-0' + (149 + Math.floor(Math.random() * 9));
    const planName = (window.GX_AUTH.plans.find((p) => p.id === f.plan) || {}).name || 'Pro';
    const received = '6 août 2026 · ' + new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
    const d = {
      id: 'new-' + ref, ref, type: f.profile === 'coach' ? 'coach' : 'salle',
      name: f.name || 'Structure sans nom', city: f.profile === 'coach' ? (f.zone || 'Zone non précisée') : [f.city, f.zip ? '(' + f.zip.slice(0, 2) + ')' : ''].filter(Boolean).join(' '),
      contact: { name: [f.first, f.last].filter(Boolean).join(' ') || 'Contact', role: f.role || 'Non précisé', email: f.email || 'contact@exemple.fr', phone: f.phone || '—' },
      siret: f.siret || '—', members: f.size || 'Non précisé', disciplines: f.disciplines || 'Non précisé',
      plan: planName, received, status: 'a-traiter', owner: null, source: 'Formulaire en ligne',
      brand: { accent: f.accent, accentLabel: f.accentLabel, sub: f.sub || 'nouvelle-salle', logo: f.logo },
      message: 'Demande envoyée depuis le formulaire de démonstration.',
      activity: [{ t: 'Demande envoyée', d: 'Formulaire en 6 étapes', when: received.split(' · ')[1], state: 'done' },
                 { t: 'Accusé de réception envoyé', d: f.email || 'contact@exemple.fr', when: received.split(' · ')[1], state: 'done' }],
      notes: [],
    };
    setDemandes([d].concat(demandes));
    setSent({ ...f, ref, planName, received });
    setRoute('ob-sent');
  };

  const mAct = (kind, d, payload) => {
    setDemandes(auApplyAct(demandes, kind, d, payload));
    setToast({ valider: 'Espace ouvert. Invitation envoyée.', refuser: 'Demande refusée, contact prévenu.', complement: 'Complément demandé.' }[kind]);
    setFocus(null);
  };

  /* ---------- desktop ---------- */
  let desktop = null;
  if (route === 'login') desktop = <AuthLogin theme={theme} onRoute={go} />;
  else if (route === 'forgot') desktop = <AuthForgot theme={theme} onRoute={go} />;
  else if (route === 'sent') desktop = <AuthLinkSent theme={theme} onRoute={go} email={email} />;
  else if (route === 'reset') desktop = <AuthReset theme={theme} onRoute={go} />;
  else if (route === 'reset-done') desktop = <AuthResetDone theme={theme} onRoute={go} />;
  else if (route === 'ob') desktop = <AuthOnboarding onRoute={go} onSubmit={submit} />;
  else if (route === 'ob-sent') desktop = <AuthObSent onRoute={go} demande={sent} />;
  else desktop = <AdminConsole onRoute={go} demandes={demandes} onDemandes={setDemandes} focus={focus} onFocus={setFocus} />;

  /* ---------- mobile ---------- */
  let phone = null;
  if (route === 'login') phone = <MAuthLogin theme={theme} onRoute={go} />;
  else if (route === 'forgot') phone = <MAuthForgot theme={theme} onRoute={go} />;
  else if (route === 'sent') phone = <MAuthLinkSent theme={theme} onRoute={go} email={email} />;
  else if (route === 'reset') phone = <MAuthReset theme={theme} onRoute={go} />;
  else if (route === 'reset-done') phone = <MAuthResetDone theme={theme} onRoute={go} />;
  else if (route === 'ob') phone = <MAuthOnboarding onRoute={go} onSubmit={submit} />;
  else if (route === 'ob-sent') phone = <MAuthObSent onRoute={go} demande={sent} />;
  else phone = focus
    ? <MAdmFiche d={demandes.find((x) => x.id === focus)} onBack={() => setFocus(null)} onAct={mAct} />
    : <MAdmDemandes demandes={demandes} onOpen={setFocus} onRoute={go} />;

  return (
    <React.Fragment>
      <div className={'gx-app-root' + (t.anim ? '' : ' gx-no-anim')}>
        {mobile
          ? <div className="gx-m-stage">
              <div className="gx-m-scale" style={{ transform: 'scale(' + scale + ')' }}>
                <IOSDevice width={DEV_W} height={DEV_H}>{phone}</IOSDevice>
              </div>
            </div>
          : desktop}
        {mobile && toast ? <div className="gx-toast">{toast}</div> : null}
      </div>

      <TweaksPanel title="Tweaks">
        <TweakSection label="Parcours" />
        <div style={{ fontSize: 10.5, color: 'rgba(41,38,27,.6)', margin: '-2px 0 6px', lineHeight: 1.4 }}>
          Connexion → demande d’ouverture → traitement super-admin. Les écrans sont cliquables entre eux ; ce sélecteur sert à sauter directement.
        </div>
        <TweakSelect label="Écran" value={route} options={AUTH_ROUTES.map((r) => ({ value: r[0], label: r[1] }))} onChange={(v) => go(v)} />
        <TweakRadio label="Surface" value={t.surface} options={['desktop', 'mobile']} onChange={(v) => setTweak('surface', v)} />
        <TweakSection label="Marque — écran de connexion" />
        <div style={{ fontSize: 10.5, color: 'rgba(41,38,27,.6)', margin: '-2px 0 6px', lineHeight: 1.4 }}>
          La connexion porte la marque du client. La demande d’ouverture et la console super-admin restent chez GymXYZ.
        </div>
        <AuThemePicker value={t.theme} onChange={(v) => setTweak('theme', v)} />
        <TweakSection label="Affichage" />
        <TweakToggle label="Animations d’entrée" value={t.anim} onChange={(v) => setTweak('anim', v)} />
      </TweaksPanel>
    </React.Fragment>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(React.createElement(AuthApp));
