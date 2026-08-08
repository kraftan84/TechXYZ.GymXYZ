/* ============================================================
   Console — Vue d'ensemble · Clients · Fiche client
   ============================================================ */
const { Button: VBtn, Card: VCard, Avatar: VAvatar } = window.TechXYZDesignSystem_ff9a8f;

/* ============================================================
   VUE D'ENSEMBLE — la file du matin d'abord, les chiffres ensuite
   ============================================================ */
function ConVue({ onNav, onClient, demandes, tickets }) {
  const C = window.GX_CONSOLE;
  const pend = demandes.filter((d) => d.status === 'a-traiter');
  const late = tickets.filter((t) => t.status !== 'resolu' && parseInt(t.age, 10) >= 1 && t.status === 'ouvert');
  const failed = C.invoices.filter((i) => i.status === 'echec' || i.status === 'impaye');
  const mrr = C.clients.filter((c) => c.status === 'actif').reduce((s, c) => s + c.mrr, 0);
  const membres = C.clients.reduce((s, c) => s + c.usage.membres, 0);
  const open = tickets.filter((t) => t.status !== 'resolu').length;

  return (
    <div className="gx-screen">
      <PageHead title={'Ce matin, ' + C.today} sub="Trois choses vous attendent. Le reste tourne.">
        <VBtn variant="outline" iconLeft={<Icon name="refresh" size={18} />}>Actualiser</VBtn>
      </PageHead>

      <VCard padding="0" style={{ marginBottom: 18 }}>
        <CardHead title="À traiter"><Chip tone="warning">{(pend.length ? 1 : 0) + (late.length ? 1 : 0) + (failed.length ? 1 : 0)} sujets</Chip></CardHead>
        <div className="gxc-stack">
          {pend.length ? <AlertRow tone="brand" icon="file" onClick={() => onNav('demandes')}
            title={pend.length + (pend.length > 1 ? ' demandes d’ouverture attendent' : ' demande d’ouverture attend')}
            detail={'La plus ancienne : ' + pend[pend.length - 1].name + ' · reçue le ' + pend[pend.length - 1].received.split('·')[0].trim()} /> : null}
          {late.length ? <AlertRow tone="warn" icon="clock" onClick={() => onNav('support')}
            title={late.length + (late.length > 1 ? ' tickets sans réponse depuis plus de 24 h' : ' ticket sans réponse depuis plus de 24 h')}
            detail={late.map((t) => t.client).join(' · ')} /> : null}
          {failed.length ? <AlertRow tone="bad" icon="euro" onClick={() => onNav('facturation')}
            title="Studio Vertical — 2 échéances impayées, 98 €"
            detail="Prélèvement rejeté le 1 août. L’espace est suspendu depuis ce matin." /> : null}
        </div>
      </VCard>

      <div className="gx-adm-kpis">
        <Kpi label="Revenu mensuel récurrent" value={mrr + ' €'} delta="+129 € à la fin de l’essai Atlas" deltaIcon="trend" deltaTone="var(--color-success)" spark />
        <Kpi label="Clients actifs" value={C.clients.filter((c) => c.status === 'actif').length} sub="1 en essai · 1 suspendu" />
        <Kpi label="Membres gérés" value={membres.toLocaleString('fr-FR')} sub="tous espaces confondus" />
        <Kpi label="Tickets ouverts" value={open} sub="délai de réponse moyen : 4 h" />
      </div>

      <div className="gx-grid2" style={{ gridTemplateColumns: '1.35fr 1fr', alignItems: 'start' }}>
        <VCard padding="0">
          <CardHead title="Activité des espaces — 30 derniers jours">
            <span style={{ fontSize: 'var(--text-xs)', color: 'var(--text-muted)' }}>connexions gestionnaires</span>
          </CardHead>
          <div className="gx-tbl-scroll">
            <table className="gx-tbl">
              <thead><tr><th>Client</th><th>Formule</th><th>Membres</th><th>Connexions</th><th>Signal</th><th></th></tr></thead>
              <tbody>
                {C.clients.map((c) => (
                  <tr key={c.id} onClick={() => onClient(c.id)}>
                    <td><div className="u"><VAvatar name={conInitials(c.name)} size="sm" />
                      <div><div className="nm">{c.name}</div><div className="em">{c.sub}.gymxyz.fr</div></div></div></td>
                    <td style={{ fontWeight: 'var(--weight-semibold)', color: 'var(--text-strong)' }}>{c.plan}</td>
                    <td style={{ fontVariantNumeric: 'tabular-nums' }}>{c.usage.membres}</td>
                    <td style={{ fontVariantNumeric: 'tabular-nums' }}>{c.usage.conn30}</td>
                    <td><Chip tone={c.health.tone}>{c.health.label}</Chip></td>
                    <td style={{ textAlign: 'right', color: 'var(--text-subtle)' }}><Icon name="chevR" size={17} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </VCard>

        <div className="gx-col">
          <VCard padding="0">
            <CardHead title="État de la plateforme"><Chip tone="success" icon="check">Tout tourne</Chip></CardHead>
            <div>{C.services.map((s, i) => (
              <div className="gxc-svc" key={i}>
                <span className={'gxc-dot ' + (s.state === 'ok' ? 'ok' : s.state === 'warn' ? 'warn' : 'bad')}></span>
                <span><span className="n">{s.name}</span><span className="d">{s.detail}</span></span>
                <span className="m">{s.metric}</span>
              </div>
            ))}</div>
          </VCard>
          <VCard padding="0">
            <CardHead title="Derniers événements"><span style={{ fontSize: 'var(--text-xs)', color: 'var(--text-muted)' }}>journal</span></CardHead>
            <div className="gx-card-pad">
              <div className="gx-tl2">
                {C.audit.slice(0, 5).map((a, i) => (
                  <div className="it" key={i}>
                    <span className="dot">{a.actor === 'Vous' ? <Icon name="user" size={13} /> : <Icon name="zap" size={13} />}</span>
                    <span><span className="t">{a.action}</span><span className="d">{a.target} · {a.when}</span></span>
                  </div>
                ))}
              </div>
            </div>
          </VCard>
        </div>
      </div>
    </div>
  );
}

/* ============================================================
   CLIENTS — liste
   ============================================================ */
function ConClients({ onClient }) {
  const C = window.GX_CONSOLE.clients;
  const [f, setF] = React.useState('tous');
  const filters = [{ id: 'tous', label: 'Tous' }, { id: 'actif', label: 'Actifs' }, { id: 'essai', label: 'En essai' }, { id: 'suspendu', label: 'Suspendus' }];
  const rows = f === 'tous' ? C : C.filter((c) => c.status === f);
  const count = (id) => id === 'tous' ? C.length : C.filter((c) => c.status === id).length;
  return (
    <div className="gx-screen">
      <PageHead title="Clients" sub="Les espaces ouverts : leur formule, leur usage, l’état de leur paiement.">
        <VBtn variant="outline" iconLeft={<Icon name="download" size={18} />}>Exporter</VBtn>
      </PageHead>
      <div className="gx-adm-tools">
        {filters.map((x) => <button key={x.id} className={'gx-fchip' + (f === x.id ? ' on' : '')} onClick={() => setF(x.id)}>{x.label}<span className="n">{count(x.id)}</span></button>)}
      </div>
      <VCard padding="0">
        <CardHead title={rows.length + ' espaces'}>
          <span style={{ fontSize: 'var(--text-xs)', color: 'var(--text-muted)' }}>Cliquez une ligne pour ouvrir la fiche</span>
        </CardHead>
        <div className="gx-tbl-scroll">
          <table className="gx-tbl">
            <thead><tr><th>Client</th><th>Type</th><th>Formule</th><th>Membres</th><th>Mensuel</th><th>Paiement</th><th>Statut</th><th></th></tr></thead>
            <tbody>
              {C.filter((c) => f === 'tous' || c.status === f).map((c) => (
                <tr key={c.id} onClick={() => onClient(c.id)}>
                  <td><div className="u"><VAvatar name={conInitials(c.name)} size="sm" />
                    <div><div className="nm">{c.name}</div><div className="em">{c.sub}.gymxyz.fr</div></div></div></td>
                  <td style={{ color: 'var(--text-muted)' }}>{c.type}</td>
                  <td style={{ fontWeight: 'var(--weight-semibold)', color: 'var(--text-strong)' }}>{c.plan}</td>
                  <td style={{ fontVariantNumeric: 'tabular-nums' }}>{c.usage.membres}</td>
                  <td style={{ fontVariantNumeric: 'tabular-nums' }}>{c.status === 'essai' ? '—' : c.mrr + ' €'}</td>
                  <td><Chip tone={c.billing.state === 'ok' ? 'success' : c.billing.state === 'late' ? 'danger' : 'neutral'}>{c.billing.stateLabel}</Chip></td>
                  <td><Chip tone={CON_TONE[c.status]}>{CON_STATUS[c.status]}</Chip></td>
                  <td style={{ textAlign: 'right', color: 'var(--text-subtle)' }}><Icon name="chevR" size={17} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </VCard>
      <div style={{ marginTop: 14 }}>
        <NoAccessNote>La console n’affiche que des <b>compteurs agrégés</b> et les comptes gestionnaires. Les fiches membres, plannings et paiements des adhérents restent dans l’espace du client — vous n’y avez pas accès.</NoAccessNote>
      </div>
    </div>
  );
}

/* ============================================================
   FICHE CLIENT
   ============================================================ */
function ConFiche({ id, onBack, onNav, tickets, imp }) {
  const c = window.GX_CONSOLE.clients.find((x) => x.id === id);
  const tk = tickets.filter((t) => t.clientId === id);
  if (!c) return null;
  return (
    <div className="gx-screen">
      <Crumb parts={[{ label: 'Clients', to: 'clients' }, c.name]} onNav={onBack} />
      <PageHead title={c.name} sub={c.sub + '.gymxyz.fr · ' + c.type + ' · ' + c.city + ' · client depuis le ' + c.since}>
        <Chip tone={CON_TONE[c.status]}>{CON_STATUS[c.status]}</Chip>
        <VBtn variant="outline" iconLeft={<Icon name="mail" size={18} />}>Écrire au contact</VBtn>
        <VBtn variant="outline" iconLeft={<Icon name="layers" size={18} />}>Changer de formule</VBtn>
        {imp ? <VBtn variant="danger" iconLeft={<Icon name="eye" size={18} />}>Ouvrir l’espace</VBtn> : null}
      </PageHead>

      <div className="gx-grid2" style={{ gridTemplateColumns: '1.5fr 1fr', alignItems: 'start' }}>
        <div className="gx-col">
          <VCard padding="0">
            <CardHead title="Usage"><Chip tone={c.health.tone}>{c.health.label}</Chip></CardHead>
            <div className="gxc-minis">
              <MiniKpi label="Membres" value={c.usage.membres} sub={'plafond ' + (c.plan === 'Essentiel' ? '150' : c.plan === 'Pro' ? '600' : 'illimité')} />
              <MiniKpi label="Cours / semaine" value={c.usage.coursSem} />
              <MiniKpi label="Réservations 30 j" value={c.usage.resa30.toLocaleString('fr-FR')} />
              <MiniKpi label="Connexions 30 j" value={c.usage.conn30} sub="comptes gestionnaires" />
            </div>
            <div className="gx-card-pad">
              <Spark data={c.usage.serie} />
              <p className="gx-fieldnote" style={{ marginTop: 10 }}>Réservations par semaine sur douze semaines. Aucune donnée nominative n’est remontée à la console.</p>
            </div>
          </VCard>

          <VCard padding="0">
            <CardHead title="Comptes gestionnaires"><Chip tone="neutral">{c.comptes.length}</Chip></CardHead>
            <div className="gx-tbl-scroll">
              <table className="gx-tbl">
                <thead><tr><th>Personne</th><th>Rôle</th><th>Dernière connexion</th><th>État</th></tr></thead>
                <tbody>
                  {c.comptes.map((p, i) => (
                    <tr key={i}>
                      <td><div className="u"><VAvatar name={p.name} size="sm" />
                        <div><div className="nm">{p.name}</div><div className="em">{p.email}</div></div></div></td>
                      <td style={{ color: 'var(--text-muted)' }}>{p.role}</td>
                      <td style={{ color: 'var(--text-muted)' }}>{p.last}</td>
                      <td><Chip tone={p.state === 'Actif' ? 'success' : 'warning'}>{p.state}</Chip></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </VCard>

          <VCard padding="0">
            <CardHead title="Tickets de ce client"><Chip tone="neutral">{tk.length}</Chip></CardHead>
            {tk.length ? <div>
              {tk.map((t) => (
                <div className="gxc-alert" key={t.id} onClick={() => onNav('support', t.id)}>
                  <span className={'ic ' + (t.status === 'ouvert' ? 'warn' : t.status === 'resolu' ? 'brand' : 'brand')}><Icon name="mail" size={18} /></span>
                  <span className="tx"><span className="t" style={{ fontSize: 'var(--text-sm)' }}>{t.subject}</span>
                    <span className="d">{t.ref} · {t.who} · {t.when}</span></span>
                  <Chip tone={CON_TK[t.status].t}>{CON_TK[t.status].l}</Chip>
                </div>
              ))}
            </div> : <p style={{ padding: 18, margin: 0, fontSize: 'var(--text-sm)', color: 'var(--text-muted)' }}>Aucun ticket. Ce client ne vous a jamais écrit.</p>}
          </VCard>
        </div>

        <div className="gx-col">
          <VCard padding="0">
            <CardHead title="Abonnement">
              <Chip tone={c.billing.state === 'ok' ? 'success' : c.billing.state === 'late' ? 'danger' : 'brand'}>{c.billing.stateLabel}</Chip>
            </CardHead>
            <div className="gx-card-pad">
              <ObKv rows={[
                ['Formule', c.plan], ['Montant', c.status === 'essai' ? 'Gratuit pendant l’essai' : c.mrr + ' € / mois'],
                ['Prochaine échéance', c.billing.next], ['Moyen de paiement', c.billing.method], ['Dernier règlement', c.billing.lastPaid],
              ]} />
            </div>
          </VCard>

          <VCard padding="0">
            <CardHead title="Habillage appliqué"><Chip tone="neutral" icon="palette">{c.theme.kind}</Chip></CardHead>
            <div className="gx-card-pad" style={{ display: 'flex', alignItems: 'center', gap: 13 }}>
              <span className="gxc-sw">{c.theme.swatch.map((s, i) => <i key={i} style={{ background: s }}></i>)}</span>
              <div style={{ minWidth: 0 }}>
                <div style={{ fontSize: 'var(--text-sm)', fontWeight: 'var(--weight-bold)', color: 'var(--text-strong)' }}>{c.theme.label}</div>
                <div style={{ fontSize: 'var(--text-xs)', color: 'var(--text-muted)', marginTop: 2 }}>Livré le {c.theme.delivered}</div>
              </div>
            </div>
            <p className="gx-fieldnote" style={{ padding: '0 18px 18px' }}>Les thèmes sur-mesure sont produits hors application. La console affiche celui qui est en ligne.</p>
          </VCard>

          <VCard padding="0">
            <CardHead title="Dossier" />
            <div className="gx-card-pad">
              <ObKv rows={[
                ['Adresse', c.sub + '.gymxyz.fr'], ['Ouvert le', c.since], ['Demande d’origine', c.ref],
                ['Contact', c.contact.name], ['E-mail', c.contact.email], ['Téléphone', c.contact.phone],
              ]} />
            </div>
          </VCard>

          {imp
            ? <NoAccessNote>Ouvrir cet espace vous connecte <b>sous l’identité du gestionnaire</b>. Un bandeau rouge reste affiché pendant toute la session et l’entrée est inscrite au journal d’audit.</NoAccessNote>
            : <NoAccessNote>Vous ne pouvez pas ouvrir cet espace ni consulter ses membres. Pour dépanner, appelez le contact ou demandez un partage d’écran.</NoAccessNote>}
        </div>
      </div>
    </div>
  );
}

Object.assign(window, { ConVue, ConClients, ConFiche });
