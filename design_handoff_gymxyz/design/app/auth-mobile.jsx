/* ============================================================
   GymXYZ Mobile — Auth & onboarding
   Connexion (3 marques) · mot de passe oublié · réinitialisation
   Demande d'ouverture en 6 étapes · fiche demande côté admin.
   ============================================================ */
const { Button: MaBtn, Input: MaInput, Select: MaSelect, Checkbox: MaCheck, Avatar: MaAvatar } = window.TechXYZDesignSystem_ff9a8f;

/* wordmark seul — le hero est coloré, la marque image ne s'y pose pas bien */
function MaWordmark({ theme }) {
  return theme.wordmark.full
    ? <span className="gx-m-wordmark">{theme.wordmark.full}</span>
    : <span className="gx-m-wordmark">{theme.wordmark.a}<span className="accent">{theme.wordmark.b}</span></span>;
}

function MaHero({ theme, title, text }) {
  return (
    <div className="gx-ma-hero">
      <MaWordmark theme={theme} />
      <h1>{title}</h1>
      <p>{text}</p>
    </div>
  );
}

function MaPw({ label, value, onChange, hint }) {
  const [show, setShow] = React.useState(false);
  return (
    <div className="gx-a-pw">
      <MaInput label={label} type={show ? 'text' : 'password'} value={value} onChange={onChange} hint={hint} placeholder="••••••••••" />
      <button className="gx-a-eye" type="button" onClick={() => setShow(!show)} aria-label={show ? 'Masquer' : 'Afficher'}><Icon name="eye" size={18} /></button>
    </div>
  );
}

/* ============================================================
   Connexion
   ============================================================ */
function MAuthLogin({ theme, onRoute }) {
  const [email, setEmail] = React.useState('');
  const [pw, setPw] = React.useState('');
  const [stay, setStay] = React.useState(true);
  const c = window.AU_COPY[theme.id] || window.AU_COPY.techxyz;
  const enter = () => {
    try { localStorage.setItem('gxm-theme', theme.id); } catch (e) {}
    window.location.href = 'GymXYZ Mobile.html';
  };
  return (
    <div className="gx-ma">
      <MaHero theme={theme} title={c.h} text={c.p} />
      <div className="gx-ma-body">
        <MaInput label="E-mail" type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="prenom@votre-salle.fr" />
        <MaPw label="Mot de passe" value={pw} onChange={(e) => setPw(e.target.value)} />
        <div className="gx-a-row">
          <MaCheck checked={stay} onChange={setStay} label={<span style={{ fontSize: 'var(--text-sm)' }}>Rester connecté</span>} />
          <button className="gx-link" onClick={() => onRoute('forgot')}>Oublié ?</button>
        </div>
        <MaBtn variant="primary" style={{ width: '100%' }} onClick={enter}>Se connecter</MaBtn>
        {theme.id === 'techxyz'
          ? <p className="gx-a-tail">Pas encore d’espace ?<br /><button className="gx-link" onClick={() => onRoute('ob')}>Demander l’ouverture d’un espace</button></p>
          : <p className="gx-a-tail">Membre de {theme.name} ? Utilisez le lien reçu par e-mail pour activer votre accès.</p>}
        <p className="gx-a-tail" style={{ fontSize: 'var(--text-xs)', color: 'var(--text-subtle)' }}>Hébergé en France · Conforme RGPD</p>
      </div>
    </div>
  );
}

/* ============================================================
   Mot de passe oublié → lien envoyé → nouveau mot de passe
   ============================================================ */
function MAuthForgot({ theme, onRoute }) {
  const [email, setEmail] = React.useState('');
  return (
    <div className="gx-ma">
      <MHead theme={theme} mode="sub" title="Mot de passe oublié" onBack={() => onRoute('login')} />
      <div className="gx-ma-body">
        <p className="gx-ma-sub" style={{ marginTop: 0 }}>Indiquez l’e-mail de votre compte : nous vous envoyons un lien pour en choisir un nouveau.</p>
        <MaInput label="E-mail" type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="prenom@votre-salle.fr" />
        <MaBtn variant="primary" style={{ width: '100%' }} onClick={() => onRoute('sent', { email })}>Envoyer le lien</MaBtn>
        <p className="gx-fieldnote">Lien valable 30 minutes, utilisable une seule fois.</p>
      </div>
    </div>
  );
}

function MAuthLinkSent({ theme, onRoute, email }) {
  return (
    <div className="gx-ma">
      <MHead theme={theme} mode="sub" title="Lien envoyé" onBack={() => onRoute('forgot')} />
      <div className="gx-ma-body gx-a-center">
        <div className="gx-a-big"><Icon name="mail" size={30} /></div>
        <h2 className="gx-ma-title">Vérifiez votre boîte</h2>
        <p className="gx-ma-sub" style={{ marginTop: 0 }}>Le lien part vers <b>{email || 'votre adresse'}</b>. Regardez dans les indésirables si rien n’arrive.</p>
        <MaBtn variant="primary" style={{ width: '100%' }} onClick={() => onRoute('reset')} iconRight={<Icon name="arrowR" size={18} />}>Ouvrir le lien (démo)</MaBtn>
        <MaBtn variant="outline" style={{ width: '100%' }} onClick={() => onRoute('sent', { email })}>Renvoyer le lien</MaBtn>
        <button className="gx-link muted" onClick={() => onRoute('login')}>Revenir à la connexion</button>
      </div>
    </div>
  );
}

function MAuthReset({ theme, onRoute }) {
  const [pw, setPw] = React.useState('');
  const [pw2, setPw2] = React.useState('');
  const s = window.auStrength(pw);
  return (
    <div className="gx-ma">
      <MHead theme={theme} mode="sub" title="Nouveau mot de passe" onBack={() => onRoute('login')} />
      <div className="gx-ma-body">
        <MaPw label="Mot de passe" value={pw} onChange={(e) => setPw(e.target.value)} />
        <div className="gx-a-strength">
          <i className={s >= 1 ? (s === 1 ? 'mid' : 'on') : ''}></i>
          <i className={s >= 2 ? (s === 2 ? 'mid' : 'on') : ''}></i>
          <i className={s >= 3 ? 'on' : ''}></i>
        </div>
        <MaPw label="Confirmation" value={pw2} onChange={(e) => setPw2(e.target.value)}
          hint={pw2 && pw !== pw2 ? 'Les deux mots de passe ne sont pas identiques.' : '12 caractères minimum.'} />
        <MaBtn variant="primary" style={{ width: '100%' }} onClick={() => onRoute('reset-done')}>Enregistrer</MaBtn>
      </div>
    </div>
  );
}

function MAuthResetDone({ theme, onRoute }) {
  return (
    <div className="gx-ma">
      <MHead theme={theme} mode="sub" title="Mot de passe" onBack={() => onRoute('login')} />
      <div className="gx-ma-body gx-a-center">
        <div className="gx-a-big ok"><Icon name="check" size={32} /></div>
        <h2 className="gx-ma-title">Mot de passe modifié</h2>
        <p className="gx-ma-sub" style={{ marginTop: 0 }}>Les autres appareils ont été déconnectés.</p>
        <MaBtn variant="primary" style={{ width: '100%' }} onClick={() => onRoute('login')}>Se connecter</MaBtn>
      </div>
    </div>
  );
}

/* ============================================================
   Onboarding mobile — 6 étapes
   ============================================================ */
function MAuthOnboarding({ onRoute, onSubmit }) {
  const A = window.GX_AUTH;
  const gym = window.GX_THEMES[0];
  const [step, setStep] = React.useState(0);
  const [f, setF] = React.useState({
    profile: 'salle', name: '', siret: '', size: '', address: '', zip: '', city: '', zone: '', disciplines: '',
    first: '', last: '', role: '', email: '', phone: '', pw: '',
    plan: 'pro', accent: '#00ABFC', accentLabel: 'Azure', sub: '', logo: null, cgu: false, rgpd: false, news: false,
  });
  const up = (k) => (e) => setF((s) => ({ ...s, [k]: e.target.value }));
  const set = (k, v) => setF((s) => ({ ...s, [k]: v }));
  const solo = f.profile === 'coach';
  const plan = A.plans.find((p) => p.id === f.plan) || A.plans[1];
  const titles = ['Vous gérez quoi ?', solo ? 'Votre activité' : 'Votre structure', 'Vous contacter', 'Votre formule', 'Votre marque', 'On relit ensemble'];
  const subs = [
    'La suite s’adapte à votre réponse.',
    'Pour vérifier la demande et préparer l’espace.',
    'C’est ici que nous répondons.',
    'Rien n’est facturé aujourd’hui.',
    'GymXYZ disparaît derrière votre identité.',
    'Vérifiez, cochez, envoyez.',
  ];

  const bodies = [
    <div className="gx-ma-choice" key="p0">
      <ObChoice on={!solo} onClick={() => set('profile', 'salle')} icon="building" title="Salle de sport ou club"
        desc="Une équipe, des cours collectifs." bullets={['Plusieurs coachs et lieux', 'Présences et abonnements']} />
      <ObChoice on={solo} onClick={() => set('profile', 'coach')} icon="user" title="Coach indépendant·e"
        desc="Seul·e, en studio, à domicile ou dehors." bullets={['Séances individuelles', 'Cartes de séances']} />
    </div>,
    <React.Fragment key="p1">
      <MaInput label={solo ? 'Nom de votre activité' : 'Nom de la structure'} required value={f.name} onChange={up('name')} placeholder={solo ? 'Naj Coaching' : 'Atlas Training Club'} />
      <MaInput label="SIRET" value={f.siret} onChange={up('siret')} placeholder="918 402 551 00019" />
      <MaSelect label={solo ? 'Client·es suivi·es' : 'Nombre de membres'} value={f.size} onChange={up('size')} options={solo ? A.sizesSolo : A.sizes} placeholder="Choisissez" />
      {solo
        ? <MaInput label="Zone d’intervention" value={f.zone} onChange={up('zone')} placeholder="Thonon et 30 km alentour" />
        : <React.Fragment>
            <MaInput label="Adresse" value={f.address} onChange={up('address')} placeholder="12 avenue des Sports" />
            <MaInput label="Code postal" value={f.zip} onChange={up('zip')} placeholder="74200" />
            <MaInput label="Ville" value={f.city} onChange={up('city')} placeholder="Thonon-les-Bains" />
          </React.Fragment>}
      <MaInput label={solo ? 'Spécialités' : 'Disciplines'} value={f.disciplines} onChange={up('disciplines')} placeholder="Musculation, cross-training" />
    </React.Fragment>,
    <React.Fragment key="p2">
      <MaInput label="Prénom" required value={f.first} onChange={up('first')} />
      <MaInput label="Nom" required value={f.last} onChange={up('last')} />
      <MaSelect label="Votre rôle" value={f.role} onChange={up('role')} options={A.roles} placeholder="Choisissez" />
      <MaInput label="E-mail professionnel" required type="email" value={f.email} onChange={up('email')} placeholder="prenom@votre-salle.fr" />
      <MaInput label="Téléphone" type="tel" value={f.phone} onChange={up('phone')} placeholder="06 12 34 56 78" />
      <MaPw label="Mot de passe" value={f.pw} onChange={up('pw')} hint="12 caractères minimum." />
    </React.Fragment>,
    <div className="gx-ma-plans" key="p3">
      {A.plans.map((p) => (
        <button key={p.id} type="button" className={'gx-plan' + (f.plan === p.id ? ' on' : '')} onClick={() => set('plan', p.id)}>
          {p.reco ? <span className="gx-ribbon">{p.reco}</span> : null}
          <span className="nm">{p.name}</span>
          <span className="pr">{p.price} <small>{p.unit}</small></span>
          <span className="for">{p.for}</span>
        </button>
      ))}
      <div className="gx-a-note"><span className="ic"><Icon name="wallet" size={17} /></span>
        <span>Aucun paiement maintenant. Devis après un échange de 20 minutes.</span></div>
    </div>,
    <React.Fragment key="p4">
      <div>
        <span className="gx-lab">Votre logo</span>
        <button type="button" className="gx-drop" style={{ width: '100%', textAlign: 'left', fontFamily: 'inherit', cursor: 'pointer' }}
          onClick={() => set('logo', f.logo ? null : 'logo-structure.png')}>
          <span className="ic"><Icon name={f.logo ? 'check' : 'download'} size={20} /></span>
          <span style={{ flex: 1 }}>
            <span className="t">{f.logo || 'Ajouter un logo'}</span>
            <span className="d">{f.logo ? 'PNG · 84 Ko' : 'PNG ou SVG, fond transparent'}</span>
          </span>
        </button>
      </div>
      <div>
        <span className="gx-lab">Couleur d’accent</span>
        <div className="gx-sws">
          {A.accents.map((a) => (
            <button key={a.id} type="button" className={'gx-sw' + (f.accent === a.hex ? ' on' : '')} style={{ background: a.hex }} aria-label={a.label}
              onClick={() => setF((s) => ({ ...s, accent: a.hex, accentLabel: a.label }))}>
              {f.accent === a.hex ? <Icon name="check" size={15} /> : null}
            </button>
          ))}
        </div>
      </div>
      <div>
        <span className="gx-lab">Adresse de votre espace</span>
        <div className="gx-dom">
          <MaInput value={f.sub} onChange={(e) => set('sub', e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, ''))} placeholder="votre-salle" />
          <span className="sfx">.gymxyz.fr</span>
        </div>
        <p className="gx-fieldnote" style={{ marginTop: 8, color: f.sub ? 'var(--color-success)' : null }}>
          {f.sub ? f.sub + '.gymxyz.fr est disponible.' : 'Lettres, chiffres et tirets.'}</p>
      </div>
    </React.Fragment>,
    <React.Fragment key="p5">
      <MCard>
        <MCardHead title={solo ? 'Activité' : 'Structure'} right={<button className="gx-link" onClick={() => setStep(1)}>Modifier</button>} />
        <div style={{ padding: '12px 16px 14px' }}><ObKv rows={[
          ['Nom', f.name || 'Non renseigné'], ['Profil', solo ? 'Coach' : 'Salle'],
          [solo ? 'Zone' : 'Ville', solo ? f.zone : f.city], ['Taille', f.size],
        ]} /></div>
      </MCard>
      <MCard>
        <MCardHead title="Contact" right={<button className="gx-link" onClick={() => setStep(2)}>Modifier</button>} />
        <div style={{ padding: '12px 16px 14px' }}><ObKv rows={[
          ['Personne', [f.first, f.last].filter(Boolean).join(' ')], ['E-mail', f.email], ['Téléphone', f.phone],
        ]} /></div>
      </MCard>
      <MCard>
        <MCardHead title="Formule & marque" right={<button className="gx-link" onClick={() => setStep(3)}>Modifier</button>} />
        <div style={{ padding: '12px 16px 14px' }}><ObKv rows={[
          ['Formule', plan.name], ['Accent', f.accentLabel], ['Espace', (f.sub || 'votre-salle') + '.gymxyz.fr'],
        ]} /></div>
      </MCard>
      <div className="gx-consent">
        <MaCheck checked={f.cgu} onChange={(v) => set('cgu', v)} style={{ fontSize: 'var(--text-sm)' }}
          label={<span>J’accepte les conditions générales et je peux engager la structure.</span>} />
        <MaCheck checked={f.rgpd} onChange={(v) => set('rgpd', v)} style={{ fontSize: 'var(--text-sm)' }}
          label={<span>J’autorise GymXYZ à traiter ces informations pour étudier ma demande.</span>} />
      </div>
    </React.Fragment>,
  ];

  return (
    <div className="gx-ma">
      <MHead theme={gym} mode="sub" title="Créer votre espace" onBack={() => step === 0 ? onRoute('login') : setStep(step - 1)} />
      <div className="gx-ma-prog"><i style={{ width: Math.round((step / 6) * 100) + '%' }}></i></div>
      <div className="gx-ma-body">
        <div>
          <span className="gx-ma-step">ÉTAPE {step + 1} / 6</span>
          <h2 className="gx-ma-title" style={{ marginTop: 6 }}>{titles[step]}</h2>
          <p className="gx-ma-sub">{subs[step]}</p>
        </div>
        {bodies[step]}
      </div>
      <div className="gx-ma-bar">
        {step < 5
          ? <MaBtn variant="primary" style={{ width: '100%' }} onClick={() => setStep(step + 1)} iconRight={<Icon name="arrowR" size={18} />}>Continuer</MaBtn>
          : <MaBtn variant="primary" style={{ width: '100%' }} disabled={!(f.cgu && f.rgpd)} onClick={() => onSubmit(f)} iconRight={<Icon name="send" size={17} />}>Envoyer ma demande</MaBtn>}
      </div>
    </div>
  );
}

function MAuthObSent({ onRoute, demande }) {
  const gym = window.GX_THEMES[0];
  const d = demande || {};
  return (
    <div className="gx-ma">
      <MHead theme={gym} mode="sub" title="Demande envoyée" onBack={() => onRoute('login')} />
      <div className="gx-ma-body">
        <div className="gx-a-center">
          <div className="gx-a-big ok"><Icon name="check" size={32} /></div>
          <h2 className="gx-ma-title">Demande envoyée</h2>
          <p className="gx-ma-sub">Votre demande pour <b>{d.name || 'votre structure'}</b> attend la validation de l’équipe GymXYZ.</p>
          <div style={{ marginTop: 14 }}><span className="gx-ref"><Icon name="file" size={14} />{d.ref || 'DEM-2026-0149'}</span></div>
        </div>
        <MCard>
          <MCardHead title="Ce qui se passe ensuite" />
          <div style={{ padding: '14px 16px 16px' }}>
            <div className="gx-tl2">
              {window.GX_AUTH.next.map((n, i) => (
                <div className={'it' + (n.state ? ' ' + n.state : '')} key={i}>
                  <span className="dot">{n.state === 'done' ? <Icon name="check" size={12} /> : i + 1}</span>
                  <span><span className="t">{n.t}</span><span className="d">{n.d}</span></span>
                </div>
              ))}
            </div>
          </div>
        </MCard>
        <MaBtn variant="primary" style={{ width: '100%' }} onClick={() => onRoute('login')}>Aller à la connexion</MaBtn>
        <MaBtn variant="ghost" style={{ width: '100%' }} onClick={() => onRoute('admin')} iconLeft={<Icon name="shield" size={17} />}>Vue super-admin (démo)</MaBtn>
      </div>
    </div>
  );
}

/* ============================================================
   Console super-admin mobile — demandes + fiche
   ============================================================ */
function MAdmDemandes({ demandes, onOpen, onRoute }) {
  const gym = window.GX_THEMES[0];
  const [filter, setFilter] = React.useState('a-traiter');
  const rows = filter === 'tous' ? demandes : demandes.filter((d) => d.status === filter);
  return (
    <div className="gx-ma">
      <MHead theme={gym} mode="sub" title="Demandes" onBack={() => onRoute('login')} />
      <div className="gx-ma-body">
        <div style={{ display: 'flex', gap: 8, overflowX: 'auto', paddingBottom: 2 }}>
          {window.AD_FILTERS_M.map((f) => (
            <button key={f.id} className={'gx-fchip' + (filter === f.id ? ' on' : '')} onClick={() => setFilter(f.id)} style={{ flex: 'none' }}>
              {f.label}<span className="n">{f.id === 'tous' ? demandes.length : demandes.filter((d) => d.status === f.id).length}</span>
            </button>
          ))}
        </div>
        <MCard>
          <div className="gx-m-rows">
            {rows.map((d) => {
              const st = window.AD_STATUS[d.status];
              return (
                <button className="gx-m-row" key={d.id} onClick={() => onOpen(d.id)}>
                  <MaAvatar name={d.name.slice(0, 2).toUpperCase()} size="sm" />
                  <span className="main">
                    <span className="nm"><span className="truncate">{d.name}</span></span>
                    <span className="meta">{d.city} · {d.plan} · {d.received.split(' · ')[0]}</span>
                  </span>
                  <span className="trail"><MChip tone={st.tone}>{st.label}</MChip></span>
                  <span className="chev"><Icon name="chevR" /></span>
                </button>
              );
            })}
            {rows.length === 0 ? <p style={{ padding: '18px 16px', margin: 0, fontSize: 'var(--text-sm)', color: 'var(--text-muted)' }}>Aucune demande dans ce filtre.</p> : null}
          </div>
        </MCard>
      </div>
    </div>
  );
}

function MAdmFiche({ d, onBack, onAct }) {
  const gym = window.GX_THEMES[0];
  const st = window.AD_STATUS[d.status];
  const open = d.status === 'a-traiter' || d.status === 'en-cours';
  const solo = d.type === 'coach';
  return (
    <div className="gx-ma">
      <MHead theme={gym} mode="sub" title={d.ref} onBack={onBack} />
      <div className="gx-ma-body">
        <div>
          <h2 className="gx-ma-title">{d.name}</h2>
          <p className="gx-ma-sub">{d.city} · reçue le {d.received}</p>
          <div style={{ display: 'flex', gap: 8, marginTop: 10 }}>
            <MChip tone={st.tone} icon={st.icon}>{st.label}</MChip>
            <MChip tone="neutral" icon={solo ? 'user' : 'building'}>{solo ? 'Coach' : 'Salle'}</MChip>
            <MChip tone="brand" icon="card">{d.plan}</MChip>
          </div>
        </div>
        <MCard>
          <MCardHead title={solo ? 'Activité' : 'Structure'} />
          <div style={{ padding: '12px 16px 14px' }}><ObKv rows={[
            ['SIRET', d.siret], [solo ? 'Client·es' : 'Membres', d.members], ['Disciplines', d.disciplines],
            ['Espace souhaité', d.brand.sub + '.gymxyz.fr'], ['Accent', d.brand.accentLabel],
          ]} /></div>
        </MCard>
        <MCard>
          <MCardHead title="Contact" />
          <div className="gx-m-rows">
            <div className="gx-m-row" style={{ cursor: 'default' }}>
              <MaAvatar name={d.contact.name} size="sm" />
              <span className="main">
                <span className="nm"><span className="truncate">{d.contact.name}</span></span>
                <span className="meta">{d.contact.role} · {d.contact.email}</span>
              </span>
            </div>
            <a className="gx-m-row" href={'tel:' + d.contact.phone.replace(/\s/g, '')} style={{ textDecoration: 'none' }}>
              <span className="main"><span className="nm">Appeler</span><span className="meta">{d.contact.phone}</span></span>
              <span className="chev"><Icon name="phone" /></span>
            </a>
          </div>
        </MCard>
        <MCard>
          <MCardHead title="Message" />
          <div style={{ padding: '12px 16px 14px', fontSize: 'var(--text-sm)', color: 'var(--text-body)', lineHeight: 1.55, fontStyle: 'italic' }}>« {d.message} »</div>
        </MCard>
        <MCard>
          <MCardHead title="Activité" />
          <div style={{ padding: '14px 16px 16px' }}>
            <div className="gx-tl2">
              {d.activity.map((a, i) => (
                <div className={'it' + (a.state ? ' ' + a.state : '')} key={i}>
                  <span className="dot">{a.state === 'done' ? <Icon name="check" size={12} /> : i + 1}</span>
                  <span><span className="t">{a.t}</span><span className="d">{a.d} · {a.when}</span></span>
                </div>
              ))}
            </div>
          </div>
        </MCard>
        {open ? <React.Fragment>
          <MaBtn variant="primary" style={{ width: '100%' }} iconLeft={<Icon name="check" size={18} />}
            onClick={() => onAct('valider', d, { sub: d.brand.sub, plan: d.plan, invite: true })}>Valider et ouvrir l’espace</MaBtn>
          <MaBtn variant="outline" style={{ width: '100%' }} iconLeft={<Icon name="mail" size={18} />}
            onClick={() => onAct('complement', d, { msg: 'Informations complémentaires demandées' })}>Demander un complément</MaBtn>
          <MaBtn variant="ghost" style={{ width: '100%' }}
            onClick={() => onAct('refuser', d, { motif: window.GX_AUTH.refusMotifs[0] })}>Refuser</MaBtn>
        </React.Fragment> : null}
      </div>
    </div>
  );
}

window.AD_FILTERS_M = [
  { id: 'a-traiter', label: 'À traiter' }, { id: 'en-cours', label: 'En cours' },
  { id: 'validee', label: 'Validées' }, { id: 'refusee', label: 'Refusées' }, { id: 'tous', label: 'Toutes' },
];

Object.assign(window, {
  MAuthLogin, MAuthForgot, MAuthLinkSent, MAuthReset, MAuthResetDone,
  MAuthOnboarding, MAuthObSent, MAdmDemandes, MAdmFiche, MaHero, MaWordmark, MaPw,
});
