/* ============================================================
   GymXYZ — Onboarding : demande d'ouverture d'un espace
   Parcours en 6 étapes (marque GymXYZ, jamais thémé client) +
   écran de confirmation « demande en attente de validation ».
   ============================================================ */
const { Button: ObBtn, Input: ObInput, Select: ObSelect, Checkbox: ObCheck } = window.TechXYZDesignSystem_ff9a8f;

const OB_STEPS = [
  { id: 'profil', label: 'Profil' },
  { id: 'structure', label: 'Structure' },
  { id: 'contact', label: 'Contact' },
  { id: 'formule', label: 'Formule' },
  { id: 'marque', label: 'Marque' },
  { id: 'recap', label: 'Récapitulatif' },
];

function ObTop({ onRoute, right }) {
  const gym = window.GX_THEMES[0];
  return (
    <div className="gx-ob-top">
      <div className="gx-ob-topin">
        <Brand theme={gym} />
        <span style={{ marginLeft: 'auto', fontSize: 'var(--text-sm)', color: 'var(--text-muted)' }}>
          {right || <React.Fragment>Déjà un espace ? <button className="gx-link" onClick={() => onRoute('login')}>Se connecter</button></React.Fragment>}
        </span>
      </div>
    </div>
  );
}

function ObStepper({ step }) {
  return (
    <div className="gx-ob-steps" style={{ margin: '0 auto 4px', maxWidth: 780, width: '100%', justifyContent: 'space-between' }}>
      {OB_STEPS.map((s, i) => (
        <React.Fragment key={s.id}>
          {i > 0 ? <span className="gx-ob-sep"></span> : null}
          <span className={'gx-ob-step' + (i === step ? ' on' : i < step ? ' done' : '')}>
            <span className="n">{i < step ? <Icon name="check" size={12} /> : i + 1}</span>{s.label}
          </span>
        </React.Fragment>
      ))}
    </div>
  );
}

function ObChoice({ on, onClick, icon, title, desc, bullets }) {
  return (
    <button type="button" className={'gx-choice-card' + (on ? ' on' : '')} onClick={onClick}>
      {on ? <span className="tick"><Icon name="check" size={13} /></span> : null}
      <span className="ic"><Icon name={icon} size={22} /></span>
      <span className="t">{title}</span>
      <span className="d">{desc}</span>
      <ul>{bullets.map((b, i) => <li key={i}><Icon name="check" size={13} />{b}</li>)}</ul>
    </button>
  );
}

function ObRecapCard({ title, onEdit, children }) {
  return (
    <div className="gx-recap">
      <div className="gx-recap-h">{title}
        <span className="r"><button className="gx-link" onClick={onEdit}>Modifier</button></span>
      </div>
      <div className="gx-recap-b">{children}</div>
    </div>
  );
}

function ObKv({ rows }) {
  return <dl className="gx-kv2">{rows.map((r, i) => <React.Fragment key={i}><dt>{r[0]}</dt><dd>{r[1] || '—'}</dd></React.Fragment>)}</dl>;
}

/* ============================================================
   Le formulaire
   ============================================================ */
function AuthOnboarding({ onRoute, onSubmit }) {
  const A = window.GX_AUTH;
  const [step, setStep] = React.useState(0);
  const [f, setF] = React.useState({
    profile: 'salle', name: '', siret: '', size: '', address: '', zip: '', city: '', zone: '', disciplines: '',
    first: '', last: '', role: '', email: '', phone: '', pw: '',
    plan: 'pro', accent: '#00ABFC', accentLabel: 'Azure', sub: '', logo: null,
    cgu: false, rgpd: false, news: false,
  });
  const up = (k) => (e) => setF((s) => ({ ...s, [k]: e.target ? e.target.value : e }));
  const set = (k, v) => setF((s) => ({ ...s, [k]: v }));
  const solo = f.profile === 'coach';
  const plan = A.plans.find((p) => p.id === f.plan) || A.plans[1];
  const canSubmit = f.cgu && f.rgpd;

  const next = () => { if (step < 5) setStep(step + 1); };
  const back = () => { if (step === 0) onRoute('login'); else setStep(step - 1); };

  const head = [
    ['Bienvenue', 'Vous gérez quoi, exactement ?', 'La suite du formulaire s’adapte à votre réponse. Deux minutes, pas plus.'],
    ['Étape 2', solo ? 'Votre activité' : 'Votre structure', 'Ces informations servent à vérifier la demande et à préparer votre espace.'],
    ['Étape 3', 'Vous contacter', 'Nous répondons à cette adresse, et c’est elle qui recevra vos identifiants.'],
    ['Étape 4', 'La formule qui vous ressemble', 'Rien n’est facturé aujourd’hui. Vous pourrez changer d’avis après notre échange.'],
    ['Étape 5', 'Votre marque dans l’application', 'GymXYZ disparaît derrière votre identité : logo, couleur, adresse web.'],
    ['Dernière étape', 'On relit ensemble', 'Vérifiez, cochez les consentements, et la demande partira côté GymXYZ.'],
  ][step];

  const body = [
    /* 1 — profil */
    <div className="gx-ob-body" key="s0">
      <div className="gx-choice">
        <ObChoice on={!solo} onClick={() => set('profile', 'salle')} icon="building" title="Salle de sport ou club"
          desc="Une équipe, des cours collectifs, plusieurs créneaux par jour."
          bullets={['Plusieurs coachs et lieux', 'Cours collectifs et présences', 'Abonnements et relances']} />
        <ObChoice on={solo} onClick={() => set('profile', 'coach')} icon="user" title="Coach indépendant·e"
          desc="Vous travaillez seul·e, en studio, à domicile ou en extérieur."
          bullets={['Séances individuelles et petits groupes', 'Suivi de vos client·es', 'Cartes de séances et paiements']} />
      </div>
      <p className="gx-fieldnote full">Vous gérez un réseau de plusieurs salles ? Choisissez « Salle de sport » : nous ajouterons les autres lieux avec vous.</p>
    </div>,

    /* 2 — structure */
    <div className="gx-ob-body" key="s1">
      <div className="full"><ObInput label={solo ? 'Nom de votre activité' : 'Nom de la structure'} required value={f.name} onChange={up('name')} placeholder={solo ? 'Naj Coaching' : 'Atlas Training Club'} /></div>
      <ObInput label="SIRET" value={f.siret} onChange={up('siret')} placeholder="918 402 551 00019" hint={solo ? 'Laissez vide si votre immatriculation est en cours.' : null} />
      <ObSelect label={solo ? 'Client·es suivi·es' : 'Nombre de membres'} value={f.size} onChange={up('size')} options={solo ? A.sizesSolo : A.sizes} placeholder="Choisissez une fourchette" />
      {solo
        ? <div className="full"><ObInput label="Zone d’intervention" value={f.zone} onChange={up('zone')} placeholder="Thonon-les-Bains et 30 km alentour" iconLeft={<Icon name="pin" size={17} />} /></div>
        : <React.Fragment>
            <div className="full"><ObInput label="Adresse" value={f.address} onChange={up('address')} placeholder="12 avenue des Sports" iconLeft={<Icon name="pin" size={17} />} /></div>
            <ObInput label="Code postal" value={f.zip} onChange={up('zip')} placeholder="74200" />
            <ObInput label="Ville" value={f.city} onChange={up('city')} placeholder="Thonon-les-Bains" />
          </React.Fragment>}
      <div className="full"><ObInput label={solo ? 'Spécialités' : 'Disciplines proposées'} value={f.disciplines} onChange={up('disciplines')}
        placeholder={solo ? 'Renforcement, pré/post-natal' : 'Musculation, cross-training, cours collectifs'}
        hint="Séparez par des virgules. Cela nous sert à préparer vos modèles de cours." /></div>
    </div>,

    /* 3 — contact & compte */
    <div className="gx-ob-body" key="s2">
      <ObInput label="Prénom" required value={f.first} onChange={up('first')} />
      <ObInput label="Nom" required value={f.last} onChange={up('last')} />
      <ObSelect label="Votre rôle" value={f.role} onChange={up('role')} options={A.roles} placeholder="Choisissez" />
      <ObInput label="Téléphone" type="tel" value={f.phone} onChange={up('phone')} placeholder="06 12 34 56 78" iconLeft={<Icon name="phone" size={17} />} />
      <div className="full"><ObInput label="E-mail professionnel" required type="email" value={f.email} onChange={up('email')} placeholder="prenom@votre-salle.fr" iconLeft={<Icon name="mail" size={17} />} hint="Nous y envoyons l’accusé de réception, puis vos identifiants." /></div>
      <div className="full"><AuthPw label="Mot de passe" value={f.pw} onChange={up('pw')} hint="12 caractères minimum. Il sera actif à l’ouverture de votre espace." /></div>
      <div className="full gx-a-strength">
        <i className={auStrength(f.pw) >= 1 ? (auStrength(f.pw) === 1 ? 'mid' : 'on') : ''}></i>
        <i className={auStrength(f.pw) >= 2 ? (auStrength(f.pw) === 2 ? 'mid' : 'on') : ''}></i>
        <i className={auStrength(f.pw) >= 3 ? 'on' : ''}></i>
      </div>
    </div>,

    /* 4 — formule */
    <div className="gx-ob-body" key="s3">
      <div className="gx-plans">
        {A.plans.map((p) => (
          <button key={p.id} type="button" className={'gx-plan' + (f.plan === p.id ? ' on' : '')} onClick={() => set('plan', p.id)}>
            {p.reco ? <span className="gx-ribbon">{p.reco}</span> : null}
            <span className="nm">{p.name}</span>
            <span className="pr">{p.price} <small>{p.unit}</small></span>
            <span className="for">{p.for}</span>
            <ul>{p.features.map((x, i) => <li key={i}><Icon name="check" size={13} />{x}</li>)}</ul>
          </button>
        ))}
      </div>
      <div className="gx-a-note full">
        <span className="ic"><Icon name="wallet" size={18} /></span>
        <span>Aucun paiement à cette étape, et pas de carte à saisir. Après validation de votre demande, nous convenons d’un échange de 20 minutes puis vous recevez un devis.</span>
      </div>
    </div>,

    /* 5 — marque blanche */
    <div className="gx-ob-body" key="s4">
      <div className="stack">
        <div>
          <span className="gx-lab">Votre logo</span>
          {f.logo
            ? <div className="gx-drop" style={{ borderStyle: 'solid', background: 'var(--azure-50)' }}>
                <span className="ic" style={{ color: 'var(--color-primary)' }}><Icon name="check" size={22} /></span>
                <span style={{ flex: 1 }}><span className="t">{f.logo}</span><span className="d">PNG · 84 Ko · fond transparent</span></span>
                <ObBtn variant="ghost" size="sm" onClick={() => set('logo', null)}>Retirer</ObBtn>
              </div>
            : <button type="button" className="gx-drop" style={{ width: '100%', textAlign: 'left', cursor: 'pointer', fontFamily: 'inherit' }} onClick={() => set('logo', 'logo-structure.png')}>
                <span className="ic"><Icon name="download" size={22} /></span>
                <span style={{ flex: 1 }}><span className="t">Déposez votre logo ici</span><span className="d">PNG ou SVG, fond transparent de préférence. Vous pourrez le changer plus tard.</span></span>
                <ObBtn variant="outline" size="sm">Parcourir</ObBtn>
              </button>}
        </div>
        <div>
          <span className="gx-lab">Couleur d’accent</span>
          <div className="gx-sws">
            {A.accents.map((a) => (
              <button key={a.id} type="button" className={'gx-sw' + (f.accent === a.hex ? ' on' : '')} style={{ background: a.hex }}
                title={a.label} aria-label={a.label} onClick={() => setF((s) => ({ ...s, accent: a.hex, accentLabel: a.label }))}>
                {f.accent === a.hex ? <Icon name="check" size={16} /> : null}
              </button>
            ))}
          </div>
          <p className="gx-fieldnote" style={{ marginTop: 8 }}>Sélection : <b>{f.accentLabel}</b>. Nous ajustons ensuite les contrastes pour rester lisible et accessible.</p>
        </div>
        <div>
          <span className="gx-lab">Adresse de votre espace</span>
          <div className="gx-dom">
            <ObInput value={f.sub} onChange={(e) => set('sub', e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, ''))} placeholder="votre-salle" />
            <span className="sfx">.gymxyz.fr</span>
          </div>
          <p className="gx-fieldnote" style={{ marginTop: 8, color: f.sub ? 'var(--color-success)' : null }}>
            {f.sub ? f.sub + '.gymxyz.fr est disponible.' : 'Lettres, chiffres et tirets. Un nom de domaine à vous est possible plus tard.'}
          </p>
        </div>
        <div>
          <span className="gx-lab">Aperçu</span>
          <div className="gx-prev">
            <div className="sb" style={{ background: '#12181F', color: '#fff' }}>
              <b>{f.name ? f.name.slice(0, 11) : 'Votre marque'}</b>
              <i className="on" style={{ background: f.accent, opacity: 1 }}></i><i></i><i></i><i></i><i></i>
            </div>
            <div className="bd">
              <span className="t">{f.name || 'Votre structure'} · Accueil</span>
              <span className="card"></span>
              <span className="card"></span>
              <span className="btn" style={{ background: f.accent }}>Diffuser le planning</span>
            </div>
          </div>
        </div>
      </div>
    </div>,

    /* 6 — récapitulatif */
    <div className="gx-ob-body" key="s5">
      <div className="stack">
        <ObRecapCard title={solo ? 'Activité' : 'Structure'} onEdit={() => setStep(1)}>
          <ObKv rows={[
            [solo ? 'Nom de l’activité' : 'Nom', f.name || 'Non renseigné'],
            ['Profil', solo ? 'Coach indépendant·e' : 'Salle de sport ou club'],
            [solo ? 'Zone' : 'Adresse', solo ? f.zone : [f.address, f.zip, f.city].filter(Boolean).join(', ')],
            ['SIRET', f.siret],
            [solo ? 'Client·es' : 'Membres', f.size],
            ['Disciplines', f.disciplines],
          ]} />
        </ObRecapCard>
        <ObRecapCard title="Contact" onEdit={() => setStep(2)}>
          <ObKv rows={[
            ['Personne', [f.first, f.last].filter(Boolean).join(' ')],
            ['Rôle', f.role],
            ['E-mail', f.email],
            ['Téléphone', f.phone],
          ]} />
        </ObRecapCard>
        <ObRecapCard title="Formule & marque" onEdit={() => setStep(3)}>
          <ObKv rows={[
            ['Formule souhaitée', plan.name + (plan.unit ? ' · ' + plan.price + ' ' + plan.unit : ' · ' + plan.price)],
            ['Couleur d’accent', f.accentLabel],
            ['Logo', f.logo || 'À fournir plus tard'],
            ['Adresse de l’espace', (f.sub || 'votre-salle') + '.gymxyz.fr'],
          ]} />
        </ObRecapCard>
        <div className="gx-consent">
          <ObCheck checked={f.cgu} onChange={(v) => set('cgu', v)} style={{ fontSize: 'var(--text-sm)' }}
            label={<span>J’accepte les <a href="#cgu" onClick={(e) => e.preventDefault()}>conditions générales</a> de GymXYZ et je confirme être habilité·e à engager la structure.</span>} />
          <ObCheck checked={f.rgpd} onChange={(v) => set('rgpd', v)} style={{ fontSize: 'var(--text-sm)' }}
            label={<span>J’autorise GymXYZ à traiter ces informations pour étudier ma demande. Données hébergées en France, supprimées sous 3 mois en cas de refus. <a href="#rgpd" onClick={(e) => e.preventDefault()}>Politique de confidentialité</a>.</span>} />
          <ObCheck checked={f.news} onChange={(v) => set('news', v)} style={{ fontSize: 'var(--text-sm)' }}
            label={<span>Je veux recevoir les nouveautés produit, environ une fois par trimestre. <span style={{ color: 'var(--text-subtle)' }}>(facultatif)</span></span>} />
        </div>
      </div>
    </div>,
  ][step];

  return (
    <div className="gx-ob">
      <ObTop onRoute={onRoute} />
      <div className="gx-ob-prog"><i style={{ width: Math.round((step / 6) * 100) + '%' }}></i></div>
      <div className="gx-ob-main">
        <ObStepper step={step} />
        <div className="gx-ob-card" key={step}>
          <div className="gx-ob-h">
            <span className="eyebrow">{head[0]}</span>
            <h2>{head[1]}</h2>
            <p>{head[2]}</p>
          </div>
          {body}
          <div className="gx-ob-foot">
            <span className="gx-ob-count">Étape {step + 1} sur 6</span>
            <ObBtn variant="ghost" onClick={back} iconLeft={<Icon name="chevL" size={17} />}>{step === 0 ? 'Connexion' : 'Retour'}</ObBtn>
            {step < 5
              ? <ObBtn variant="primary" onClick={next} iconRight={<Icon name="arrowR" size={18} />}>Continuer</ObBtn>
              : <ObBtn variant="primary" disabled={!canSubmit} onClick={() => onSubmit(f)} iconRight={<Icon name="send" size={17} />}>Envoyer ma demande</ObBtn>}
          </div>
        </div>
        <div className="gx-ob-reassure">
          <span><Icon name="check" size={15} />Sans engagement</span>
          <span><Icon name="check" size={15} />Aucun paiement à cette étape</span>
          <span><Icon name="check" size={15} />Réponse sous 1 jour ouvré</span>
          <span><Icon name="check" size={15} />Données hébergées en France</span>
        </div>
      </div>
    </div>
  );
}

/* ============================================================
   Demande envoyée — en attente de validation
   ============================================================ */
function AuthObSent({ onRoute, demande }) {
  const A = window.GX_AUTH;
  const d = demande || {};
  return (
    <div className="gx-ob">
      <ObTop onRoute={onRoute} right={<React.Fragment>Une question ? <a href="mailto:bonjour@gymxyz.fr">bonjour@gymxyz.fr</a></React.Fragment>} />
      <div className="gx-ob-main">
        <div className="gx-ob-card">
          <div className="gx-ob-h gx-a-center" style={{ paddingBottom: 4 }}>
            <div className="gx-a-big ok" style={{ marginBottom: 14 }}><Icon name="check" size={34} /></div>
            <h2 style={{ marginTop: 0 }}>Demande envoyée</h2>
            <p style={{ margin: '10px auto 0' }}>
              Merci {d.first || ''} — votre demande pour <b style={{ color: 'var(--text-strong)' }}>{d.name || 'votre structure'}</b> est enregistrée
              et attend la validation de l’équipe GymXYZ.
            </p>
            <div style={{ marginTop: 16 }}><span className="gx-ref"><Icon name="file" size={15} />Référence <b>{d.ref || 'DEM-2026-0149'}</b></span></div>
          </div>
          <div className="gx-ob-body">
            <div className="stack">
              <div className="gx-recap">
                <div className="gx-recap-h"><Icon name="mail" size={15} />Accusé de réception envoyé à {d.email || 'votre adresse'}</div>
                <div className="gx-recap-b"><ObKv rows={[
                  ['Formule souhaitée', d.planName || 'Pro'],
                  ['Adresse de l’espace', (d.sub || 'votre-salle') + '.gymxyz.fr'],
                  ['Reçue le', d.received || 'aujourd’hui'],
                  ['Statut', 'En attente de validation'],
                ]} /></div>
              </div>
              <div>
                <span className="gx-lab">Ce qui se passe ensuite</span>
                <div className="gx-tl2">
                  {A.next.map((n, i) => (
                    <div className={'it' + (n.state ? ' ' + n.state : '')} key={i}>
                      <span className="dot">{n.state === 'done' ? <Icon name="check" size={13} /> : i + 1}</span>
                      <span><span className="t">{n.t}</span><span className="d">{n.d}</span></span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
          <div className="gx-ob-foot">
            <span className="gx-ob-count">Vous pouvez fermer cette page : tout est dans l’e-mail.</span>
            <ObBtn variant="ghost" onClick={() => onRoute('admin')} iconLeft={<Icon name="shield" size={17} />}>Vue super-admin (démo)</ObBtn>
            <ObBtn variant="primary" onClick={() => onRoute('login')}>Aller à la connexion</ObBtn>
          </div>
        </div>
      </div>
    </div>
  );
}

Object.assign(window, { AuthOnboarding, AuthObSent, ObTop, ObStepper, ObKv, ObRecapCard, ObChoice, OB_STEPS });
