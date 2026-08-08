/* ============================================================
   Console — Facturation · Support · Formules · Santé · Référentiels
   ============================================================ */
const { Button: OBtn, Card: OCard, Avatar: OAvatar, Input: OInput } = window.TechXYZDesignSystem_ff9a8f;

/* ============================================================
   FACTURATION — suivi seulement, l'encaissement se fait ailleurs
   ============================================================ */
function ConFacturation({ onClient }) {
  const C = window.GX_CONSOLE;
  const [f, setF] = React.useState('tous');
  const filters = [{ id: 'tous', label: 'Toutes' }, { id: 'paye', label: 'Payées' }, { id: 'echec', label: 'Rejetées' }, { id: 'impaye', label: 'Impayées' }];
  const count = (id) => id === 'tous' ? C.invoices.length : C.invoices.filter((i) => i.status === id).length;
  const rows = f === 'tous' ? C.invoices : C.invoices.filter((i) => i.status === f);
  const mrr = C.clients.filter((c) => c.status === 'actif').reduce((s, c) => s + c.mrr, 0);
  return (
    <div className="gx-screen">
      <PageHead title="Facturation" sub="Ce qui est dû, ce qui est rentré, ce qui bloque. L’encaissement lui-même se fait chez votre banque.">
        <OBtn variant="outline" iconLeft={<Icon name="download" size={18} />}>Exporter le mois</OBtn>
      </PageHead>
      <div className="gx-adm-kpis">
        <Kpi label="Revenu mensuel récurrent" value={mrr + ' €'} delta="+129 € fin d’essai Atlas" deltaIcon="trend" deltaTone="var(--color-success)" spark />
        <Kpi label="Encaissé en août" value="467 €" sub="4 factures sur 5" />
        <Kpi label="En échec" value="49 €" sub="Studio Vertical · 1 août" valueColor="var(--color-danger)" />
        <Kpi label="Impayé cumulé" value="98 €" sub="2 échéances · 1 client" valueColor="var(--color-danger)" />
      </div>

      <OCard padding="0" style={{ marginBottom: 18 }}>
        <CardHead title="Action requise" />
        <div className="gxc-stack">
          <AlertRow tone="bad" icon="alert" onClick={() => onClient('vertical')}
            title="Studio Vertical — relance de niveau 2"
            detail="Prélèvement rejeté le 1 août (provision insuffisante). Espace suspendu automatiquement ce matin." />
          <AlertRow tone="warn" icon="clock" onClick={() => onClient('atlas')}
            title="Atlas Training Club — essai terminé le 21 août"
            detail="Aucun moyen de paiement enregistré. Prévoir le devis Pro avant le 18 août." />
        </div>
      </OCard>

      <div className="gx-adm-tools">
        {filters.map((x) => <button key={x.id} className={'gx-fchip' + (f === x.id ? ' on' : '')} onClick={() => setF(x.id)}>{x.label}<span className="n">{count(x.id)}</span></button>)}
      </div>

      <OCard padding="0">
        <CardHead title={rows.length + ' factures'} />
        <div className="gx-tbl-scroll">
          <table className="gx-tbl">
            <thead><tr><th>Référence</th><th>Client</th><th>Période</th><th>Émise</th><th>Échéance</th><th>Montant</th><th>Statut</th><th></th></tr></thead>
            <tbody>
              {rows.map((iv) => {
                const st = CON_PAY[iv.status];
                return (
                  <tr key={iv.ref}>
                    <td style={{ fontVariantNumeric: 'tabular-nums', fontWeight: 'var(--weight-semibold)', color: 'var(--text-strong)' }}>{iv.ref}</td>
                    <td>{iv.client}</td>
                    <td style={{ color: 'var(--text-muted)' }}>{iv.period}</td>
                    <td style={{ color: 'var(--text-muted)' }}>{iv.issued}</td>
                    <td style={{ color: 'var(--text-muted)' }}>{iv.due}</td>
                    <td style={{ fontVariantNumeric: 'tabular-nums', fontWeight: 'var(--weight-semibold)' }}>{iv.amount}</td>
                    <td><Chip tone={st.t}>{st.l}</Chip></td>
                    <td style={{ textAlign: 'right' }}><OBtn variant="ghost" size="sm" iconLeft={<Icon name="download" size={16} />}>PDF</OBtn></td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </OCard>
    </div>
  );
}

/* ============================================================
   SUPPORT — file des tickets ouverts depuis le bouton « Aide »
   ============================================================ */
function ConSupport({ tickets, onTickets, focus, onFocus, onClient }) {
  const [f, setF] = React.useState('ouverts');
  const [reply, setReply] = React.useState('');
  const filters = [{ id: 'ouverts', label: 'À traiter' }, { id: 'tous', label: 'Tous' }, { id: 'resolu', label: 'Résolus' }];
  const match = (t) => f === 'tous' ? true : f === 'resolu' ? t.status === 'resolu' : t.status !== 'resolu';
  const rows = tickets.filter(match);
  const sel = tickets.find((t) => t.id === focus) || rows[0] || tickets[0];
  const send = () => {
    if (!reply.trim()) return;
    onTickets(tickets.map((t) => t.id !== sel.id ? t : {
      ...t, status: 'en-cours',
      thread: t.thread.concat([{ who: 'Vous', when: "aujourd'hui · " + new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' }), side: 'me', txt: reply.trim() }]),
    }));
    setReply('');
  };
  const close = () => onTickets(tickets.map((t) => t.id !== sel.id ? t : { ...t, status: 'resolu' }));

  return (
    <div className="gx-screen">
      <PageHead title="Support" sub="Chaque ticket arrive du bouton « Aide » de l’app cliente, avec le contexte technique déjà joint.">
        <OBtn variant="outline" iconLeft={<Icon name="settings" size={18} />}>Réglages du bouton Aide</OBtn>
      </PageHead>
      <div className="gx-adm-kpis">
        <Kpi label="Ouverts" value={tickets.filter((t) => t.status === 'ouvert').length} sub="aucune réponse encore" spark />
        <Kpi label="En cours" value={tickets.filter((t) => t.status === 'en-cours').length} sub="réponse envoyée, en attente" />
        <Kpi label="Sans réponse &gt; 24 h" value={tickets.filter((t) => t.status === 'ouvert').length} sub="objectif : aucun" valueColor="var(--color-warning)" />
        <Kpi label="Délai moyen de réponse" value="4 h" sub="sur 30 jours" />
      </div>
      <div className="gx-adm-tools">
        {filters.map((x) => <button key={x.id} className={'gx-fchip' + (f === x.id ? ' on' : '')} onClick={() => setF(x.id)}>{x.label}
          <span className="n">{x.id === 'tous' ? tickets.length : tickets.filter((t) => x.id === 'resolu' ? t.status === 'resolu' : t.status !== 'resolu').length}</span></button>)}
      </div>

      <div className="gxc-two">
        <OCard padding="0">
          <CardHead title={rows.length + (rows.length > 1 ? ' tickets' : ' ticket')} />
          <div className="gxc-tklist">
            {rows.map((t) => (
              <button key={t.id} className={'gxc-tk' + (sel && sel.id === t.id ? ' on' : '')} onClick={() => onFocus(t.id)}>
                <span className="r1">
                  <span className={'gxc-dot ' + (t.status === 'ouvert' ? 'warn' : t.status === 'resolu' ? 'ok' : 'bad')}
                    style={t.status === 'en-cours' ? { background: 'var(--color-primary)' } : null}></span>
                  <span className="cl">{t.client}</span><span className="ag">{t.age}</span>
                </span>
                <span className="sj">{t.subject}</span>
                <span className="wh">{t.ref} · {t.who} · {t.when}</span>
              </button>
            ))}
            {rows.length === 0 ? <p style={{ padding: 18, margin: 0, fontSize: 'var(--text-sm)', color: 'var(--text-muted)' }}>Rien à traiter. Bonne journée.</p> : null}
          </div>
        </OCard>

        {sel ? <div className="gx-col">
          <OCard padding="0">
            <CardHead title={sel.subject}>
              <Chip tone={CON_TK[sel.status].t}>{CON_TK[sel.status].l}</Chip>
              {sel.priority === 'haute' ? <Chip tone="danger" icon="alert">Priorité haute</Chip> : null}
            </CardHead>
            <div className="gx-card-pad" style={{ display: 'flex', alignItems: 'center', gap: 13, borderBottom: '1px solid var(--border-subtle)' }}>
              <OAvatar name={sel.who} size="md" />
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontSize: 'var(--text-md)', fontWeight: 'var(--weight-bold)', color: 'var(--text-strong)' }}>{sel.who}</div>
                <div style={{ fontSize: 'var(--text-sm)', color: 'var(--text-muted)' }}>{sel.role} · {sel.client}</div>
              </div>
              <OBtn variant="outline" size="sm" iconLeft={<Icon name="building" size={16} />} onClick={() => onClient(sel.clientId)}>Voir la fiche client</OBtn>
            </div>
            <div className="gxc-msgs">
              {sel.thread.map((m, i) => (
                <div className={'gxc-msg ' + m.side} key={i}>
                  <span className="h"><b>{m.who}</b><span>{m.when}</span></span>{m.txt}
                </div>
              ))}
            </div>
            {sel.hint ? <div className="gxc-hint"><Icon name="alert" size={15} /><span><b>Note interne</b> — {sel.hint}</span></div> : null}
            {sel.status !== 'resolu' ? <div style={{ padding: 18, borderTop: '1px solid var(--border-subtle)', display: 'flex', flexDirection: 'column', gap: 11 }}>
              <textarea className="gx-ta" rows="3" value={reply} onChange={(e) => setReply(e.target.value)}
                placeholder={'Répondre à ' + sel.who.split(' ')[0] + '… Dites ce que vous avez compris, ce que vous faites, et quand.'}></textarea>
              <div style={{ display: 'flex', gap: 10 }}>
                <OBtn variant="primary" iconLeft={<Icon name="send" size={17} />} disabled={!reply.trim()} onClick={send}>Envoyer la réponse</OBtn>
                <OBtn variant="ghost" iconLeft={<Icon name="check" size={17} />} onClick={close}>Marquer résolu</OBtn>
              </div>
            </div> : <p className="gx-fieldnote" style={{ padding: 18, borderTop: '1px solid var(--border-subtle)' }}>Ticket résolu. Le client peut répondre pour le rouvrir.</p>}
          </OCard>

          <OCard padding="0">
            <CardHead title="Contexte joint automatiquement" />
            <div className="gx-card-pad">
              <dl className="gxc-ctx">
                <div><dt>Écran</dt><dd>{sel.ctx.screen}</dd></div>
                <div><dt>URL</dt><dd>{sel.ctx.url}</dd></div>
                <div><dt>Espace</dt><dd>{sel.ctx.tenant}</dd></div>
                <div><dt>Navigateur</dt><dd>{sel.ctx.browser}</dd></div>
                <div><dt>Système</dt><dd>{sel.ctx.os}</dd></div>
                <div><dt>Version</dt><dd>{sel.ctx.version}</dd></div>
              </dl>
              <p className="gx-fieldnote" style={{ marginTop: 12 }}>Compte à l’origine : {sel.ctx.account}. Aucune donnée d’adhérent n’est jointe au ticket.</p>
            </div>
          </OCard>
        </div> : null}
      </div>
    </div>
  );
}

/* ============================================================
   FORMULES & TARIFS
   ============================================================ */
function ConFormules() {
  const C = window.GX_CONSOLE;
  return (
    <div className="gx-screen">
      <PageHead title="Formules & tarifs" sub="Ce que vous vendez, et combien de clients sont sur chaque formule.">
        <OBtn variant="outline" iconLeft={<Icon name="plus" size={18} />}>Nouvelle formule</OBtn>
      </PageHead>
      <div className="gxc-plans">
        {C.plans.map((p) => (
          <div className={'gxc-plan' + (p.reco ? ' reco' : '')} key={p.id}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <span className="nm">{p.name}</span>
              {p.reco ? <Chip tone="brand">Le plus vendu</Chip> : null}
            </div>
            <div className="pr">{p.price} <small>{p.unit}</small></div>
            <div className="fr">{p.for}</div>
            <div className="lim">{p.limits.map((l, i) => <span key={i}>{l}</span>)}</div>
            <ul>{p.features.map((ft, i) => <li key={i}><Icon name="check" size={15} />{ft}</li>)}</ul>
            <div className="use">
              <b>{p.clients}</b> {p.clients > 1 ? 'clients' : 'client'} · <b>{p.mrr}</b> / mois
              <span style={{ marginLeft: 'auto' }}><OBtn variant="ghost" size="sm" iconRight={<Icon name="arrowR" size={16} />}>Modifier</OBtn></span>
            </div>
          </div>
        ))}
      </div>
      <p className="gx-fieldnote" style={{ marginTop: 16 }}>Un changement de tarif ne s’applique qu’aux nouveaux contrats. Les clients en cours gardent leur prix jusqu’à leur date anniversaire.</p>
    </div>
  );
}

/* ============================================================
   SANTÉ & JOURNAL
   ============================================================ */
function ConSante() {
  const C = window.GX_CONSOLE;
  return (
    <div className="gx-screen">
      <PageHead title="Santé & journal" sub="L’état des services, les incidents récents et la trace de ce qui a été fait.">
        <OBtn variant="outline" iconLeft={<Icon name="download" size={18} />}>Exporter le journal</OBtn>
      </PageHead>
      <div className="gx-grid2" style={{ gridTemplateColumns: '1fr 1fr', alignItems: 'start', marginBottom: 18 }}>
        <OCard padding="0">
          <CardHead title="Services"><Chip tone="warning" icon="alert">1 point de vigilance</Chip></CardHead>
          <div>{C.services.map((s, i) => (
            <div className="gxc-svc" key={i}>
              <span className={'gxc-dot ' + (s.state === 'ok' ? 'ok' : s.state === 'warn' ? 'warn' : 'bad')}></span>
              <span><span className="n">{s.name}</span><span className="d">{s.detail}</span></span>
              <span className="m">{s.metric}</span>
            </div>
          ))}</div>
        </OCard>
        <OCard padding="0">
          <CardHead title="Incidents récents" />
          <div className="gx-card-pad">
            <div className="gx-tl2">
              {C.incidents.map((it, i) => (
                <div className={'it' + (i === 0 ? ' now' : ' done')} key={i}>
                  <span className="dot">{i === 0 ? <Icon name="alert" size={13} /> : <Icon name="check" size={13} />}</span>
                  <span><span className="t">{it.title}</span><span className="d">{it.when} · {it.impact} · {it.state}</span></span>
                </div>
              ))}
            </div>
          </div>
        </OCard>
      </div>
      <OCard padding="0">
        <CardHead title="Journal d’audit">
          <span style={{ fontSize: 'var(--text-xs)', color: 'var(--text-muted)' }}>conservé 3 ans · exportable sur demande d’un client</span>
        </CardHead>
        <div className="gx-tbl-scroll">
          <table className="gx-tbl">
            <thead><tr><th>Horodatage</th><th>Auteur</th><th>Action</th><th>Cible</th></tr></thead>
            <tbody>
              {C.audit.map((a, i) => (
                <tr key={i}>
                  <td style={{ color: 'var(--text-muted)', whiteSpace: 'nowrap' }}>{a.when}</td>
                  <td><Chip tone={a.actor === 'Vous' ? 'brand' : 'neutral'} icon={a.actor === 'Vous' ? 'user' : 'zap'}>{a.actor}</Chip></td>
                  <td style={{ fontWeight: 'var(--weight-semibold)', color: 'var(--text-strong)' }}>{a.action}</td>
                  <td style={{ color: 'var(--text-muted)' }}>{a.target}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </OCard>
    </div>
  );
}

/* ============================================================
   RÉFÉRENTIELS — communs à tous les espaces
   ============================================================ */
function ConReferentiels() {
  const R = window.GX_CONSOLE.refs;
  const [disc, setDisc] = React.useState(R.disciplines);
  const [add, setAdd] = React.useState('');
  return (
    <div className="gx-screen">
      <PageHead title="Référentiels" sub="Les listes partagées par tous les espaces clients. Une modification ici est visible partout." />
      <div className="gx-grid2" style={{ gridTemplateColumns: '1.1fr 1fr', alignItems: 'start' }}>
        <div className="gx-col">
          <OCard padding="0">
            <CardHead title="Disciplines"><Chip tone="neutral">{disc.length}</Chip></CardHead>
            <div className="gx-card-pad" style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div className="gxc-tags">
                {disc.map((d) => (
                  <span className="gxc-tag" key={d}>{d}
                    <button type="button" title="Retirer" onClick={() => setDisc(disc.filter((x) => x !== d))}><Icon name="x" size={13} /></button>
                  </span>
                ))}
              </div>
              <div style={{ display: 'flex', gap: 10, alignItems: 'flex-end' }}>
                <div style={{ flex: 1 }}><OInput label="Ajouter une discipline" value={add} onChange={(e) => setAdd(e.target.value)} placeholder="Ex. : Aquagym" /></div>
                <OBtn variant="outline" disabled={!add.trim()} onClick={() => { setDisc(disc.concat([add.trim()])); setAdd(''); }}>Ajouter</OBtn>
              </div>
              <p className="gx-fieldnote">Retirer une discipline ne supprime rien chez les clients qui l’utilisent déjà : elle disparaît seulement des nouvelles saisies.</p>
            </div>
          </OCard>

          <OCard padding="0">
            <CardHead title="Calendrier scolaire"><Chip tone="success" icon="check">Synchronisé</Chip></CardHead>
            <div className="gx-tbl-scroll">
              <table className="gx-tbl">
                <thead><tr><th>Zone</th><th>Académies</th><th>Prochaines vacances</th></tr></thead>
                <tbody>{R.zones.map((z, i) => (
                  <tr key={i}>
                    <td style={{ fontWeight: 'var(--weight-semibold)', color: 'var(--text-strong)' }}>{z.zone}</td>
                    <td style={{ color: 'var(--text-muted)' }}>{z.dept}</td>
                    <td>{z.next}</td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
            <p className="gx-fieldnote" style={{ padding: 18 }}>Source : {R.calendarSource}. Les plannings clients grisent automatiquement les périodes de vacances de leur zone.</p>
          </OCard>
        </div>

        <OCard padding="0">
          <CardHead title="E-mails automatiques"><Chip tone="neutral">{R.mails.length} modèles</Chip></CardHead>
          <div className="gx-tbl-scroll">
            <table className="gx-tbl">
              <thead><tr><th>Modèle</th><th>Déclencheur</th><th>Envoyés 30 j</th><th></th></tr></thead>
              <tbody>{R.mails.map((m) => (
                <tr key={m.key}>
                  <td><div className="nm" style={{ fontWeight: 'var(--weight-semibold)', color: 'var(--text-strong)' }}>{m.name}</div>
                    <div className="em" style={{ fontSize: 'var(--text-xs)', color: 'var(--text-subtle)' }}>modifié le {m.edited}</div></td>
                  <td style={{ color: 'var(--text-muted)' }}>{m.trigger}</td>
                  <td style={{ fontVariantNumeric: 'tabular-nums' }}>{m.sent30.toLocaleString('fr-FR')}</td>
                  <td style={{ textAlign: 'right' }}><OBtn variant="ghost" size="sm" iconRight={<Icon name="arrowR" size={16} />}>Modifier</OBtn></td>
                </tr>
              ))}</tbody>
            </table>
          </div>
          <p className="gx-fieldnote" style={{ padding: 18 }}>Chaque modèle est envoyé au nom du client, avec son habillage. Vous éditez le texte commun ; les variables (nom du cours, date) sont remplacées à l’envoi.</p>
        </OCard>
      </div>
    </div>
  );
}

Object.assign(window, { ConFacturation, ConSupport, ConFormules, ConSante, ConReferentiels });
