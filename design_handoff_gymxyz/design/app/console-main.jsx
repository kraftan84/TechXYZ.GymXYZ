/* ============================================================
   GymXYZ — Console plateforme : racine, routage, Tweaks
   ============================================================ */
const CONSOLE_TWEAKS = /*EDITMODE-BEGIN*/{
  "density": "standard",
  "anim": true,
  "impersonation": false
}/*EDITMODE-END*/;

function ConsoleApp() {
  const [t, setTweak] = useTweaks(CONSOLE_TWEAKS);
  const [nav, setNav] = React.useState('vue');
  const [client, setClient] = React.useState(null);
  const [ticket, setTicket] = React.useState(null);
  const [demandes, setDemandes] = React.useState(window.GX_AUTH.demandes);
  const [demFocus, setDemFocus] = React.useState(null);
  const [tickets, setTickets] = React.useState(window.GX_CONSOLE.tickets);
  const [toast, setToast] = React.useState(null);

  React.useEffect(() => { document.documentElement.dataset.theme = 'techxyz'; }, []);
  React.useEffect(() => { if (!toast) return; const x = setTimeout(() => setToast(null), 3400); return () => clearTimeout(x); }, [toast]);

  const go = (id, arg) => {
    setNav(id);
    if (id !== 'clients') setClient(null);
    if (id === 'support' && arg) setTicket(arg);
    if (id !== 'demandes') setDemFocus(null);
  };
  const openClient = (id) => { setNav('clients'); setClient(id); };

  /* actions sur les demandes d'ouverture (réutilise la fiche existante) */
  const actDemande = (kind, d, payload) => {
    setDemandes(demandes.map((x) => {
      if (x.id !== d.id) return x;
      const now = '6 août · ' + new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
      if (kind === 'note') return { ...x, notes: x.notes.concat([{ who: window.GX_AUTH.admin.name, when: now, txt: payload.txt }]) };
      if (kind === 'valider') return { ...x, status: 'validee', owner: window.GX_AUTH.admin.name,
        activity: x.activity.concat([{ t: 'Demande validée', d: `Espace ${payload.sub}.gymxyz.fr · formule ${payload.plan}` + (payload.invite ? ' · invitation envoyée' : ''), when: now, state: 'done' }]) };
      if (kind === 'refuser') return { ...x, status: 'refusee', owner: window.GX_AUTH.admin.name,
        activity: x.activity.concat([{ t: 'Demande refusée', d: payload.motif, when: now, state: 'done' }]) };
      if (kind === 'complement') return { ...x, status: 'en-cours', owner: window.GX_AUTH.admin.name,
        activity: x.activity.concat([{ t: 'Complément demandé', d: payload.msg || 'Informations manquantes', when: now, state: 'now' }]) };
      return x;
    }));
    setToast({
      note: 'Note ajoutée à la fiche.',
      valider: `Espace ouvert pour ${d.name}. Invitation en route.`,
      refuser: `Demande de ${d.name} refusée. Le contact est prévenu.`,
      complement: `Complément demandé à ${d.contact.name}.`,
    }[kind]);
  };

  const counts = {
    demandes: demandes.filter((d) => d.status === 'a-traiter').length,
    tickets: tickets.filter((x) => x.status !== 'resolu').length,
    impayes: 2,
  };

  let screen = null;
  if (nav === 'vue') screen = <ConVue onNav={go} onClient={openClient} demandes={demandes} tickets={tickets} />;
  else if (nav === 'demandes') screen = demFocus
    ? <AdmFiche d={demandes.find((d) => d.id === demFocus)} onBack={() => setDemFocus(null)} onAct={actDemande} />
    : <AdmDemandes demandes={demandes} onOpen={setDemFocus} />;
  else if (nav === 'clients') screen = client
    ? <ConFiche id={client} imp={t.impersonation} onBack={() => setClient(null)} onNav={go} tickets={tickets} />
    : <ConClients onClient={setClient} />;
  else if (nav === 'facturation') screen = <ConFacturation onClient={openClient} />;
  else if (nav === 'support') screen = <ConSupport tickets={tickets} onTickets={setTickets} focus={ticket} onFocus={setTicket} onClient={openClient} />;
  else if (nav === 'formules') screen = <ConFormules />;
  else if (nav === 'sante') screen = <ConSante />;
  else if (nav === 'referentiels') screen = <ConReferentiels />;

  return (
    <div className={'gx-density-' + t.density + (t.anim ? '' : ' gx-no-anim')}>
      <ConsoleShell nav={nav} onNav={go} counts={counts}>{screen}</ConsoleShell>
      {toast ? <div className="gx-toast"><Icon name="check" size={17} />{toast}</div> : null}
      <TweaksPanel title="Tweaks">
        <TweakSection label="Accès aux espaces clients" />
        <div style={{ fontSize: 10.5, color: 'rgba(41,38,27,.6)', margin: '-2px 0 6px', lineHeight: 1.4 }}>
          Arbitrage retenu : le super-admin n’entre jamais dans un espace client. Activez pour voir la variante avec impersonation journalisée.
        </div>
        <TweakToggle label="Autoriser l’impersonation" value={t.impersonation} onChange={(v) => setTweak('impersonation', v)} />
        <TweakSection label="Affichage" />
        <TweakRadio label="Densité" value={t.density} options={['compact', 'standard', 'confort']} onChange={(v) => setTweak('density', v)} />
        <TweakToggle label="Animations d’entrée" value={t.anim} onChange={(v) => setTweak('anim', v)} />
      </TweaksPanel>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<ConsoleApp />);
