/* ============================================================
   GymXYZ — Console super-admin (côté TechXYZ)
   Demandes d'ouverture : liste, fiche, validation / refus /
   complément · Clients (tenants) déjà ouverts.
   ============================================================ */
const { Button: AdBtn, Card: AdCard, Input: AdInput, Select: AdSelect,
        Checkbox: AdCheck, Avatar: AdAvatar } = window.TechXYZDesignSystem_ff9a8f;

const AD_STATUS = {
  'a-traiter': { label: 'À traiter', tone: 'warning', icon: 'alert' },
  'en-cours': { label: 'En cours', tone: 'brand', icon: 'clock' },
  'validee': { label: 'Validée', tone: 'success', icon: 'check' },
  'refusee': { label: 'Refusée', tone: 'neutral', icon: 'x' },
};
const AD_CLIENT_STATUS = {
  actif: { label: 'Actif', tone: 'success' },
  essai: { label: 'Essai', tone: 'brand' },
  suspendu: { label: 'Suspendu', tone: 'warning' },
};
const AD_FILTERS = [
  { id: 'tous', label: 'Toutes' },
  { id: 'a-traiter', label: 'À traiter' },
  { id: 'en-cours', label: 'En cours' },
  { id: 'validee', label: 'Validées' },
  { id: 'refusee', label: 'Refusées' },
];

function adInitials(n) { return n.split(/[\s—-]+/).filter(Boolean).slice(0, 2).map((w) => w[0]).join('').toUpperCase(); }

/* ---------- console shell : sidebar + topbar ---------- */
function AdmShell({ nav, onNav, pending, onRoute, children }) {
  const gym = window.GX_THEMES[0];
  const me = window.GX_AUTH.admin;
  const items = [
    { id: 'demandes', label: 'Demandes', icon: 'file', count: pending ? String(pending) : null },
    { id: 'clients', label: 'Clients', icon: 'building' },
  ];
  return (
    <div className="gx-app">
      <aside className="gx-sb">
        <Brand theme={gym} />
        <div className="gx-sb-group">Console plateforme</div>
        {items.map((it) => (
          <div key={it.id} className={'gx-nav' + (nav === it.id ? ' active' : '')} onClick={() => onNav(it.id)}>
            <Icon name={it.icon} size={19} /><span>{it.label}</span>
            {it.count ? <span className="count">{it.count}</span> : null}
          </div>
        ))}
        <div className="gx-sb-spacer"></div>
        <div className="gx-sb-foot">
          <div className="gx-nav" onClick={() => onRoute('login')}>
            <Icon name="chevL" size={19} /><span>Quitter la console</span>
          </div>
          <div className="gx-theme-hint">
            <Icon name="shield" size={14} />
            <span>Accès réservé à l’équipe <b>TechXYZ</b>. Chaque action est journalisée.</span>
          </div>
        </div>
      </aside>
      <div className="gx-main">
        <header className="gx-tb">
          <div className="gx-search"><Icon name="search" size={17} /><span>Rechercher une demande, un client…</span></div>
          <div className="gx-tb-sp"></div>
          <span className="gx-adm-badge"><Icon name="shield" size={13} />Super-admin</span>
          <div className="gx-tb-ic"><Icon name="bell" size={20} /><span className="dot"></span></div>
          <span className="gx-tb-div"></span>
          <div className="gx-me">
            <AdAvatar name={me.name} size="sm" />
            <div><div className="nm">{me.name}</div><div className="rl">{me.role} · TechXYZ</div></div>
          </div>
        </header>
        <div className="gx-content"><div className="gx-wrap">{children}</div></div>
      </div>
    </div>
  );
}

/* ---------- modal ---------- */
function AdModal({ title, sub, children, onClose, actions }) {
  return (
    <div className="gx-modal-scrim" onClick={onClose}>
      <div className="gx-modal" onClick={(e) => e.stopPropagation()}>
        <div className="gx-modal-h"><h3>{title}</h3>{sub ? <p>{sub}</p> : null}</div>
        <div className="gx-modal-b">{children}</div>
        <div className="gx-modal-f">{actions}</div>
      </div>
    </div>
  );
}

/* ============================================================
   Liste des demandes
   ============================================================ */
function AdmDemandes({ demandes, onOpen }) {
  const [filter, setFilter] = React.useState('tous');
  const count = (id) => id === 'tous' ? demandes.length : demandes.filter((d) => d.status === id).length;
  const rows = filter === 'tous' ? demandes : demandes.filter((d) => d.status === filter);
  return (
    <div className="gx-screen">
      <PageHead title="Demandes d’ouverture" sub="Les structures qui veulent un espace GymXYZ. À traiter dans l’ordre d’arrivée.">
        <AdBtn variant="outline" iconLeft={<Icon name="download" size={18} />}>Exporter</AdBtn>
      </PageHead>
      <div className="gx-adm-kpis">
        <Kpi label="À traiter" value={count('a-traiter')} sub="dont 2 reçues aujourd’hui" spark />
        <Kpi label="En cours" value={count('en-cours')} sub="échange ou complément attendu" />
        <Kpi label="Validées ce mois" value={count('validee')} delta="+2 vs juillet" deltaIcon="trend" deltaTone="var(--color-success)" />
        <Kpi label="Délai moyen de réponse" value="1,4 j" sub="objectif : 1 jour ouvré" />
      </div>
      <div className="gx-adm-tools">
        {AD_FILTERS.map((f) => (
          <button key={f.id} className={'gx-fchip' + (filter === f.id ? ' on' : '')} onClick={() => setFilter(f.id)}>
            {f.label}<span className="n">{count(f.id)}</span>
          </button>
        ))}
      </div>
      <AdCard padding="0">
        <CardHead title={rows.length + (rows.length > 1 ? ' demandes' : ' demande')}>
          <span style={{ fontSize: 'var(--text-xs)', color: 'var(--text-muted)' }}>Cliquez une ligne pour ouvrir la fiche</span>
        </CardHead>
        <div className="gx-tbl-scroll">
          <table className="gx-tbl">
            <thead><tr>
              <th>Structure</th><th>Profil</th><th>Formule</th><th>Taille</th><th>Reçue</th><th>Statut</th><th></th>
            </tr></thead>
            <tbody>
              {rows.map((d) => {
                const st = AD_STATUS[d.status];
                return (
                  <tr key={d.id} onClick={() => onOpen(d.id)}>
                    <td><div className="u">
                      <AdAvatar name={adInitials(d.name)} size="sm" />
                      <div><div className="nm">{d.name}</div><div className="em">{d.city} · {d.ref}</div></div>
                    </div></td>
                    <td><Chip tone="neutral" icon={d.type === 'coach' ? 'user' : 'building'}>{d.type === 'coach' ? 'Coach' : 'Salle'}</Chip></td>
                    <td style={{ fontWeight: 'var(--weight-semibold)', color: 'var(--text-strong)' }}>{d.plan}</td>
                    <td style={{ color: 'var(--text-muted)' }}>{d.members}</td>
                    <td style={{ color: 'var(--text-muted)' }}>{d.received}</td>
                    <td><Chip tone={st.tone} icon={st.icon}>{st.label}</Chip></td>
                    <td style={{ textAlign: 'right', color: 'var(--text-subtle)' }}><Icon name="chevR" size={17} /></td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </AdCard>
    </div>
  );
}

/* ============================================================
   Fiche demande
   ============================================================ */
function AdmFiche({ d, onBack, onAct }) {
  const [modal, setModal] = React.useState(null);
  const [note, setNote] = React.useState('');
  const [sub, setSub] = React.useState(d.brand.sub);
  const [theme, setTheme] = React.useState('Créer un thème sur-mesure');
  const [plan, setPlan] = React.useState(d.plan);
  const [invite, setInvite] = React.useState(true);
  const [motif, setMotif] = React.useState(window.GX_AUTH.refusMotifs[0]);
  const [msg, setMsg] = React.useState('');
  const st = AD_STATUS[d.status];
  const open = d.status === 'a-traiter' || d.status === 'en-cours';
  const solo = d.type === 'coach';

  return (
    <div className="gx-screen">
      <Crumb parts={[{ label: 'Demandes', to: 'demandes' }, d.name]} onNav={onBack} />
      <PageHead title={d.name} sub={`${d.ref} · reçue le ${d.received} · ${d.source}`}>
        {open ? <React.Fragment>
          <AdBtn variant="ghost" onClick={() => setModal('refus')}>Refuser</AdBtn>
          <AdBtn variant="outline" iconLeft={<Icon name="mail" size={18} />} onClick={() => setModal('complement')}>Demander un complément</AdBtn>
          <AdBtn variant="primary" iconLeft={<Icon name="check" size={18} />} onClick={() => setModal('valider')}>Valider et ouvrir l’espace</AdBtn>
        </React.Fragment> : <AdBtn variant="outline" iconLeft={<Icon name="chevL" size={18} />} onClick={onBack}>Retour aux demandes</AdBtn>}
      </PageHead>

      <div className="gx-grid2" style={{ gridTemplateColumns: '1.45fr 1fr', alignItems: 'start' }}>
        <div className="gx-col">
          <AdCard padding="0">
            <CardHead title={solo ? 'Activité' : 'Structure'}>
              <Chip tone="neutral" icon={solo ? 'user' : 'building'}>{solo ? 'Coach indépendant' : 'Salle de sport'}</Chip>
            </CardHead>
            <div className="gx-card-pad"><ObKv rows={[
              ['Nom', d.name], ['Localisation', d.city], ['SIRET', d.siret],
              [solo ? 'Client·es suivi·es' : 'Membres estimés', d.members], ['Disciplines', d.disciplines],
            ]} /></div>
          </AdCard>

          <AdCard padding="0">
            <CardHead title="Contact" />
            <div className="gx-card-pad" style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
              <AdAvatar name={d.contact.name} size="md" />
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontSize: 'var(--text-md)', fontWeight: 'var(--weight-bold)', color: 'var(--text-strong)' }}>{d.contact.name}</div>
                <div style={{ fontSize: 'var(--text-sm)', color: 'var(--text-muted)' }}>{d.contact.role}</div>
              </div>
              <div style={{ display: 'flex', gap: 8 }}>
                <AdBtn variant="outline" size="sm" iconLeft={<Icon name="mail" size={16} />}>{d.contact.email}</AdBtn>
                <AdBtn variant="outline" size="sm" iconLeft={<Icon name="phone" size={16} />}>{d.contact.phone}</AdBtn>
              </div>
            </div>
          </AdCard>

          <AdCard padding="0">
            <CardHead title="Formule & marque demandées">
              <Chip tone="brand" icon="card">{d.plan}</Chip>
            </CardHead>
            <div className="gx-card-pad" style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <ObKv rows={[
                ['Adresse souhaitée', d.brand.sub + '.gymxyz.fr'],
                ['Logo fourni', d.brand.logo || 'Non — à demander'],
              ]} />
              <div style={{ display: 'flex', alignItems: 'center', gap: 12, paddingTop: 12, borderTop: '1px solid var(--border-subtle)' }}>
                <span style={{ width: 34, height: 34, borderRadius: '50%', background: d.brand.accent, flex: 'none', boxShadow: '0 0 0 1px var(--border-default) inset' }}></span>
                <div>
                  <div style={{ fontSize: 'var(--text-sm)', fontWeight: 'var(--weight-bold)', color: 'var(--text-strong)' }}>Accent {d.brand.accentLabel}</div>
                  <div style={{ fontSize: 'var(--text-xs)', color: 'var(--text-muted)' }}>{d.brand.accent} · contrastes à vérifier avant ouverture</div>
                </div>
              </div>
            </div>
          </AdCard>

          <AdCard padding="0">
            <CardHead title="Message de la structure" />
            <div className="gx-card-pad">
              <p style={{ margin: 0, fontSize: 'var(--text-md)', color: 'var(--text-body)', lineHeight: 1.6, fontStyle: 'italic' }}>« {d.message} »</p>
            </div>
          </AdCard>
        </div>

        <div className="gx-col">
          <AdCard padding="0">
            <CardHead title="Suivi"><Chip tone={st.tone} icon={st.icon}>{st.label}</Chip></CardHead>
            <div className="gx-card-pad"><ObKv rows={[
              ['Référence', d.ref], ['Assignée à', d.owner || 'Personne'], ['Origine', d.source],
            ]} /></div>
          </AdCard>

          <AdCard padding="0">
            <CardHead title="Activité" />
            <div className="gx-card-pad">
              <div className="gx-tl2">
                {d.activity.map((a, i) => (
                  <div className={'it' + (a.state ? ' ' + a.state : '')} key={i}>
                    <span className="dot">{a.state === 'done' ? <Icon name="check" size={13} /> : i + 1}</span>
                    <span><span className="t">{a.t}</span><span className="d">{a.d} · {a.when}</span></span>
                  </div>
                ))}
              </div>
            </div>
          </AdCard>

          <AdCard padding="0">
            <CardHead title="Notes internes"><Chip tone="neutral">{d.notes.length}</Chip></CardHead>
            {d.notes.length ? <div className="gx-notes">
              {d.notes.map((n, i) => (
                <div className="gx-note-it" key={i}>
                  <AdAvatar name={n.who} size="sm" />
                  <div>
                    <div><span className="who">{n.who}</span> <span className="when">· {n.when}</span></div>
                    <div className="txt">{n.txt}</div>
                  </div>
                </div>
              ))}
            </div> : <p style={{ padding: '16px 18px 0', margin: 0, fontSize: 'var(--text-sm)', color: 'var(--text-muted)' }}>Aucune note. Écrivez ce que vous voulez retrouver dans six mois.</p>}
            <div style={{ padding: 18, display: 'flex', flexDirection: 'column', gap: 10 }}>
              <textarea className="gx-ta" value={note} onChange={(e) => setNote(e.target.value)} placeholder="Ce qu’il faut savoir sur cette demande…"></textarea>
              <AdBtn variant="outline" size="sm" disabled={!note.trim()} style={{ alignSelf: 'flex-start' }}
                onClick={() => { onAct('note', d, { txt: note.trim() }); setNote(''); }}>Ajouter la note</AdBtn>
            </div>
          </AdCard>
        </div>
      </div>

      {modal === 'valider' ? <AdModal title="Valider et ouvrir l’espace"
        sub={`${d.name} passera en client actif. L’espace est créé avec la formule choisie, puis l’invitation part au contact.`}
        onClose={() => setModal(null)}
        actions={<React.Fragment>
          <AdBtn variant="ghost" onClick={() => setModal(null)}>Annuler</AdBtn>
          <AdBtn variant="primary" iconLeft={<Icon name="check" size={18} />}
            onClick={() => { setModal(null); onAct('valider', d, { sub, theme, plan, invite }); }}>Valider la demande</AdBtn>
        </React.Fragment>}>
        <div className="gx-dom">
          <AdInput label="Adresse de l’espace" value={sub} onChange={(e) => setSub(e.target.value)} />
          <span className="sfx" style={{ alignSelf: 'flex-end' }}>.gymxyz.fr</span>
        </div>
        <AdSelect label="Habillage" value={theme} onChange={(e) => setTheme(e.target.value)}
          options={['Créer un thème sur-mesure'].concat(window.GX_THEMES.map((t) => t.label))}
          hint="Un thème sur-mesure est livré sous 3 jours ouvrés." />
        <AdSelect label="Formule" value={plan} onChange={(e) => setPlan(e.target.value)} options={['Essentiel', 'Pro', 'Sur-mesure']} />
        <AdCheck checked={invite} onChange={setInvite} style={{ fontSize: 'var(--text-sm)' }}
          label={<span>Envoyer l’invitation à <b>{d.contact.email}</b> tout de suite</span>} />
      </AdModal> : null}

      {modal === 'refus' ? <AdModal title="Refuser la demande"
        sub="Le contact reçoit un e-mail avec le motif. Les données sont supprimées sous 3 mois."
        onClose={() => setModal(null)}
        actions={<React.Fragment>
          <AdBtn variant="ghost" onClick={() => setModal(null)}>Annuler</AdBtn>
          <AdBtn variant="danger" onClick={() => { setModal(null); onAct('refuser', d, { motif, msg }); }}>Refuser la demande</AdBtn>
        </React.Fragment>}>
        <AdSelect label="Motif" value={motif} onChange={(e) => setMotif(e.target.value)} options={window.GX_AUTH.refusMotifs} />
        <div>
          <span className="gx-lab">Message au contact</span>
          <textarea className="gx-ta" value={msg} onChange={(e) => setMsg(e.target.value)}
            placeholder="Expliquez simplement pourquoi, et orientez vers une solution si vous en connaissez une."></textarea>
        </div>
      </AdModal> : null}

      {modal === 'complement' ? <AdModal title="Demander un complément"
        sub="La demande passe « en cours » en attendant la réponse."
        onClose={() => setModal(null)}
        actions={<React.Fragment>
          <AdBtn variant="ghost" onClick={() => setModal(null)}>Annuler</AdBtn>
          <AdBtn variant="primary" iconLeft={<Icon name="send" size={17} />}
            onClick={() => { setModal(null); onAct('complement', d, { msg }); }}>Envoyer la demande</AdBtn>
        </React.Fragment>}>
        <div>
          <span className="gx-lab">Ce qu’il manque</span>
          <textarea className="gx-ta" value={msg} onChange={(e) => setMsg(e.target.value)}
            placeholder="Ex. : le SIRET ou le récépissé d’immatriculation, et le logo en PNG transparent."></textarea>
        </div>
        <p className="gx-fieldnote">Relance automatique après 7 jours sans réponse.</p>
      </AdModal> : null}
    </div>
  );
}

/* ============================================================
   Clients (tenants ouverts)
   ============================================================ */
function AdmClients() {
  const C = window.GX_AUTH.clients;
  const actifs = C.filter((c) => c.status === 'actif').length;
  const essais = C.filter((c) => c.status === 'essai').length;
  const membres = C.reduce((s, c) => s + c.members, 0);
  const openSpace = (c) => {
    const t = window.GX_THEMES.find((x) => x.id === c.sub);
    try { localStorage.setItem('gx-theme', t ? t.id : 'techxyz'); } catch (e) {}
    window.location.href = 'GymXYZ Desktop.html';
  };
  return (
    <div className="gx-screen">
      <PageHead title="Clients" sub="Les espaces ouverts, leur formule et leur activité.">
        <AdBtn variant="outline" iconLeft={<Icon name="download" size={18} />}>Exporter</AdBtn>
      </PageHead>
      <div className="gx-adm-kpis">
        <Kpi label="Clients actifs" value={actifs} sub="facturés ce mois" spark />
        <Kpi label="En essai" value={essais} sub="30 jours, sans carte" />
        <Kpi label="Revenu mensuel" value="534 €" delta="+129 € en juillet" deltaIcon="trend" deltaTone="var(--color-success)" />
        <Kpi label="Membres gérés" value={membres.toLocaleString('fr-FR')} sub="tous clients confondus" />
      </div>
      <AdCard padding="0">
        <CardHead title={C.length + ' espaces'} />
        <div className="gx-tbl-scroll">
          <table className="gx-tbl">
            <thead><tr>
              <th>Client</th><th>Type</th><th>Formule</th><th>Membres</th><th>Mensuel</th><th>Statut</th><th>Activité</th><th></th>
            </tr></thead>
            <tbody>
              {C.map((c, i) => {
                const st = AD_CLIENT_STATUS[c.status];
                return (
                  <tr key={i} onClick={() => openSpace(c)}>
                    <td><div className="u">
                      <AdAvatar name={adInitials(c.name)} size="sm" />
                      <div><div className="nm">{c.name}</div><div className="em">{c.sub}.gymxyz.fr</div></div>
                    </div></td>
                    <td style={{ color: 'var(--text-muted)' }}>{c.type}</td>
                    <td style={{ fontWeight: 'var(--weight-semibold)', color: 'var(--text-strong)' }}>{c.plan}</td>
                    <td style={{ fontVariantNumeric: 'tabular-nums' }}>{c.members || '—'}</td>
                    <td style={{ fontVariantNumeric: 'tabular-nums' }}>{c.mrr}</td>
                    <td><Chip tone={st.tone}>{st.label}</Chip></td>
                    <td style={{ color: 'var(--text-muted)' }}>{c.last}</td>
                    <td style={{ textAlign: 'right' }}><AdBtn variant="ghost" size="sm" iconRight={<Icon name="arrowR" size={16} />}>Ouvrir</AdBtn></td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </AdCard>
      <p className="gx-fieldnote" style={{ marginTop: 14 }}>Ouvrir un espace vous connecte en tant que gestionnaire (impersonation) — action journalisée.</p>
    </div>
  );
}

/* ============================================================
   Console : routage interne + actions sur les demandes
   ============================================================ */
function AdminConsole({ onRoute, demandes, onDemandes, focus, onFocus }) {
  const [nav, setNav] = React.useState('demandes');
  const [toast, setToast] = React.useState(null);
  React.useEffect(() => { if (!toast) return; const t = setTimeout(() => setToast(null), 3400); return () => clearTimeout(t); }, [toast]);
  const sel = focus ? demandes.find((d) => d.id === focus) : null;
  const pending = demandes.filter((d) => d.status === 'a-traiter').length;

  const act = (kind, d, payload) => {
    onDemandes(demandes.map((x) => {
      if (x.id !== d.id) return x;
      const now = '6 août · ' + new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
      if (kind === 'note') return { ...x, notes: x.notes.concat([{ who: window.GX_AUTH.admin.name, when: now, txt: payload.txt }]) };
      if (kind === 'valider') return {
        ...x, status: 'validee', owner: window.GX_AUTH.admin.name,
        activity: x.activity.concat([{ t: 'Demande validée', d: `Espace ${payload.sub}.gymxyz.fr · formule ${payload.plan}` + (payload.invite ? ' · invitation envoyée' : ''), when: now, state: 'done' }]),
      };
      if (kind === 'refuser') return {
        ...x, status: 'refusee', owner: window.GX_AUTH.admin.name,
        activity: x.activity.concat([{ t: 'Demande refusée', d: payload.motif, when: now, state: 'done' }]),
      };
      if (kind === 'complement') return {
        ...x, status: 'en-cours', owner: window.GX_AUTH.admin.name,
        activity: x.activity.concat([{ t: 'Complément demandé', d: payload.msg || 'Informations manquantes', when: now, state: 'now' }]),
      };
      return x;
    }));
    setToast({
      note: 'Note ajoutée à la fiche.',
      valider: `Espace ouvert pour ${d.name}. Invitation en route.`,
      refuser: `Demande de ${d.name} refusée. Le contact est prévenu.`,
      complement: `Complément demandé à ${d.contact.name}.`,
    }[kind]);
  };

  return (
    <AdmShell nav={nav} onNav={(id) => { setNav(id); onFocus(null); }} pending={pending} onRoute={onRoute}>
      {nav === 'clients'
        ? <AdmClients />
        : sel
          ? <AdmFiche d={sel} onBack={() => onFocus(null)} onAct={act} />
          : <AdmDemandes demandes={demandes} onOpen={onFocus} />}
      {toast ? <div className="gx-toast"><Icon name="check" size={17} />{toast}</div> : null}
    </AdmShell>
  );
}

Object.assign(window, { AdminConsole, AdmShell, AdmDemandes, AdmFiche, AdmClients, AdModal, AD_STATUS, AD_CLIENT_STATUS });
