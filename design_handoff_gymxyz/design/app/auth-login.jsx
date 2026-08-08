/* ============================================================
   GymXYZ — Auth desktop : connexion, mot de passe oublié,
   lien envoyé, réinitialisation, confirmation.
   Entièrement thémé : la même page porte les 3 marques.
   ============================================================ */
const { Button: AuBtn, Input: AuInput, Checkbox: AuCheck } = window.TechXYZDesignSystem_ff9a8f;

const AU_COPY = {
  techxyz: { h: 'Votre espace de gestion', p: 'Planning, membres, présences et abonnements au même endroit.' },
  teamtrainers: { h: 'Le club, côté coulisses', p: 'Planning, présences et abonnements : tout le club dans une seule interface.' },
  leyssa: { h: 'Votre studio, en douceur', p: 'Séances, suivi des clientes et abonnements réunis au même endroit.' },
};

function auStrength(pw) {
  let s = 0;
  if (pw.length >= 8) s++;
  if (/[A-Z]/.test(pw) && /[a-z]/.test(pw)) s++;
  if (/[0-9]/.test(pw) || /[^A-Za-z0-9]/.test(pw)) s++;
  return pw ? s : 0;
}

/* ---------- left brand panel ---------- */
function AuthPanel({ theme }) {
  const c = AU_COPY[theme.id] || AU_COPY.techxyz;
  const dark = theme.id !== 'leyssa';
  return (
    <div className="gx-auth-panel">
      <Brand theme={theme} onDark={dark} />
      <div className="gx-a-mid">
        <h1>{c.h}</h1>
        <p>{c.p}</p>
        <div className="gx-a-pills">
          <span className="gx-a-pill"><Icon name="calendar" size={16} />Planning</span>
          <span className="gx-a-pill"><Icon name="check" size={16} />Présences</span>
          <span className="gx-a-pill"><Icon name="card" size={16} />Abonnements</span>
        </div>
      </div>
      <div className="gx-a-pfoot">
        <span>Hébergé en France</span><span className="sep"></span><span>Conforme RGPD</span>
        {theme.id !== 'techxyz' ? <React.Fragment><span className="sep"></span><span>Propulsé par GymXYZ</span></React.Fragment> : null}
      </div>
    </div>
  );
}

function AuthPw({ label, value, onChange, hint, placeholder }) {
  const [show, setShow] = React.useState(false);
  return (
    <div className="gx-a-pw">
      <AuInput label={label} type={show ? 'text' : 'password'} value={value} onChange={onChange} hint={hint} placeholder={placeholder || '••••••••••'} />
      <button className="gx-a-eye" type="button" onClick={() => setShow(!show)} aria-label={show ? 'Masquer' : 'Afficher'}>
        <Icon name="eye" size={18} />
      </button>
    </div>
  );
}

/* ---------- shell : panel + right column ---------- */
function AuthShell({ theme, children }) {
  return (
    <div className="gx-auth">
      <AuthPanel theme={theme} />
      <div className="gx-auth-side">{children}</div>
    </div>
  );
}

/* ============================================================
   Connexion
   ============================================================ */
function AuthLogin({ theme, onRoute }) {
  const [email, setEmail] = React.useState('');
  const [pw, setPw] = React.useState('');
  const [stay, setStay] = React.useState(true);
  const enter = () => {
    try { localStorage.setItem('gx-theme', theme.id); } catch (e) {}
    window.location.href = 'GymXYZ Desktop.html';
  };
  return (
    <AuthShell theme={theme}>
      <div className="gx-a-form">
        <div>
          <h1 className="gx-a-h">Connexion</h1>
          <p className="gx-a-sub">Accédez à votre tableau de bord.</p>
        </div>
        <AuInput label="E-mail" type="email" value={email} placeholder="prenom@votre-salle.fr" onChange={(e) => setEmail(e.target.value)} />
        <AuthPw label="Mot de passe" value={pw} onChange={(e) => setPw(e.target.value)} />
        <div className="gx-a-row">
          <AuCheck checked={stay} onChange={setStay} label={<span style={{ fontSize: 'var(--text-sm)' }}>Rester connecté</span>} />
          <button className="gx-link" onClick={() => onRoute('forgot')}>Mot de passe oublié ?</button>
        </div>
        <AuBtn variant="primary" style={{ width: '100%' }} onClick={enter}>Se connecter</AuBtn>
        <div className="gx-a-hr">ou</div>
        {theme.id === 'techxyz'
          ? <p className="gx-a-tail">Vous découvrez GymXYZ ?<br />
              <button className="gx-link" onClick={() => onRoute('ob')}>Demander l’ouverture d’un espace</button></p>
          : <p className="gx-a-tail">Vous êtes membre de {theme.name} ?<br />
              Connectez-vous depuis l’application mobile, avec le lien reçu par e-mail.</p>}
        <p className="gx-a-tail" style={{ fontSize: 'var(--text-xs)', color: 'var(--text-subtle)' }}>
          Besoin d’aide ? <a href="mailto:support@gymxyz.fr">support@gymxyz.fr</a> · 04 50 00 00 00
        </p>
      </div>
    </AuthShell>
  );
}

/* ============================================================
   Mot de passe oublié
   ============================================================ */
function AuthForgot({ theme, onRoute }) {
  const [email, setEmail] = React.useState('');
  return (
    <AuthShell theme={theme}>
      <div className="gx-a-form">
        <button className="gx-link muted" onClick={() => onRoute('login')} style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
          <Icon name="chevL" size={16} />Revenir à la connexion
        </button>
        <div>
          <h1 className="gx-a-h">Mot de passe oublié</h1>
          <p className="gx-a-sub">Indiquez l’e-mail de votre compte : nous vous envoyons un lien pour en choisir un nouveau.</p>
        </div>
        <AuInput label="E-mail" type="email" value={email} placeholder="prenom@votre-salle.fr" onChange={(e) => setEmail(e.target.value)} />
        <AuBtn variant="primary" style={{ width: '100%' }} onClick={() => onRoute('sent', { email })}>Envoyer le lien</AuBtn>
        <p className="gx-a-tail" style={{ fontSize: 'var(--text-xs)' }}>
          Le lien est valable 30 minutes et ne peut servir qu’une fois.
        </p>
      </div>
    </AuthShell>
  );
}

/* ============================================================
   Lien envoyé
   ============================================================ */
function AuthLinkSent({ theme, onRoute, email }) {
  return (
    <AuthShell theme={theme}>
      <div className="gx-a-form gx-a-center">
        <div className="gx-a-big"><Icon name="mail" size={32} /></div>
        <div>
          <h1 className="gx-a-h">Vérifiez votre boîte</h1>
          <p className="gx-a-sub">Un lien de réinitialisation vient de partir vers <b style={{ color: 'var(--text-strong)' }}>{email || 'votre adresse'}</b>.</p>
        </div>
        <div className="gx-a-note" style={{ textAlign: 'left' }}>
          <span className="ic"><Icon name="clock" size={17} /></span>
          <span>Rien reçu au bout de deux minutes ? Regardez dans les indésirables, puis renvoyez le lien.</span>
        </div>
        <AuBtn variant="primary" style={{ width: '100%' }} onClick={() => onRoute('reset')} iconRight={<Icon name="arrowR" size={18} />}>Ouvrir le lien (démo)</AuBtn>
        <AuBtn variant="outline" style={{ width: '100%' }} onClick={() => onRoute('sent', { email })}>Renvoyer le lien</AuBtn>
        <button className="gx-link muted" onClick={() => onRoute('login')}>Revenir à la connexion</button>
      </div>
    </AuthShell>
  );
}

/* ============================================================
   Nouveau mot de passe
   ============================================================ */
function AuthReset({ theme, onRoute }) {
  const [pw, setPw] = React.useState('');
  const [pw2, setPw2] = React.useState('');
  const s = auStrength(pw);
  const mismatch = pw2.length > 0 && pw !== pw2;
  return (
    <AuthShell theme={theme}>
      <div className="gx-a-form">
        <div>
          <h1 className="gx-a-h">Nouveau mot de passe</h1>
          <p className="gx-a-sub">Choisissez un mot de passe que vous n’utilisez nulle part ailleurs.</p>
        </div>
        <div>
          <AuthPw label="Mot de passe" value={pw} onChange={(e) => setPw(e.target.value)} />
          <div className="gx-a-strength" style={{ marginTop: 8 }}>
            <i className={s >= 1 ? (s === 1 ? 'mid' : 'on') : ''}></i>
            <i className={s >= 2 ? (s === 2 ? 'mid' : 'on') : ''}></i>
            <i className={s >= 3 ? 'on' : ''}></i>
          </div>
          <p className="gx-fieldnote" style={{ marginTop: 7 }}>12 caractères minimum, avec majuscules, minuscules et un chiffre.</p>
        </div>
        <AuthPw label="Confirmation" value={pw2} onChange={(e) => setPw2(e.target.value)} hint={mismatch ? 'Les deux mots de passe ne sont pas identiques.' : null} />
        <AuBtn variant="primary" style={{ width: '100%' }} onClick={() => onRoute('reset-done')}>Enregistrer le mot de passe</AuBtn>
        <button className="gx-link muted" onClick={() => onRoute('login')}>Annuler</button>
      </div>
    </AuthShell>
  );
}

/* ============================================================
   Confirmation
   ============================================================ */
function AuthResetDone({ theme, onRoute }) {
  return (
    <AuthShell theme={theme}>
      <div className="gx-a-form gx-a-center">
        <div className="gx-a-big ok"><Icon name="check" size={34} /></div>
        <div>
          <h1 className="gx-a-h">Mot de passe modifié</h1>
          <p className="gx-a-sub">Vous pouvez vous connecter avec votre nouveau mot de passe. Les autres appareils ont été déconnectés.</p>
        </div>
        <AuBtn variant="primary" style={{ width: '100%' }} onClick={() => onRoute('login')}>Se connecter</AuBtn>
      </div>
    </AuthShell>
  );
}

Object.assign(window, { AuthLogin, AuthForgot, AuthLinkSent, AuthReset, AuthResetDone, AuthPanel, AuthShell, AuthPw, auStrength, AU_COPY });
