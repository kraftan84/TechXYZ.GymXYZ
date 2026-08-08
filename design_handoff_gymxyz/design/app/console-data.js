/* ============================================================
   GymXYZ — Console plateforme (super-admin TechXYZ)
   Données de démonstration. Le super-admin n'entre JAMAIS dans
   l'espace d'un client : tout ce qui suit est agrégé côté
   plateforme (compteurs, comptes gestionnaires, facturation).
   ============================================================ */
window.GX_CONSOLE = {
  today: 'jeudi 6 août 2026',

  clients: [
    {
      id: 'teamtrainers', name: "Team Trainer's", sub: 'teamtrainers', type: 'Salle de sport', city: 'Thonon-les-Bains (74)',
      plan: 'Pro', mrr: 129, status: 'actif', since: '12 juillet 2026', ref: 'DEM-2026-0121',
      theme: { label: "Team Trainer's — rock monochrome", swatch: ['#232327', '#fafafa'], delivered: '14 juillet 2026', kind: 'Sur-mesure' },
      usage: { membres: 287, coursSem: 34, resa30: 1412, conn30: 214, serie: [58, 62, 71, 66, 74, 80, 77, 84, 88, 91, 86, 94] },
      health: { label: 'Usage régulier', tone: 'success' },
      comptes: [
        { name: 'Aurélie Siquier', role: 'Gérante', email: 'aurelie@teamtrainers.fr', last: "aujourd'hui · 08:12", state: 'Actif' },
        { name: 'Bruno Siquier', role: 'Gérant', email: 'bruno@teamtrainers.fr', last: 'hier · 19:40', state: 'Actif' },
        { name: 'Sofia Marchetti', role: 'Accueil', email: 'accueil@teamtrainers.fr', last: 'il y a 3 j', state: 'Actif' },
      ],
      billing: { next: '1 septembre 2026', method: 'Prélèvement SEPA', state: 'ok', stateLabel: 'À jour', lastPaid: '1 août 2026 · 129 €' },
      contact: { name: 'Aurélie Siquier', email: 'aurelie@teamtrainers.fr', phone: '06 88 21 40 77' },
    },
    {
      id: 'leyssa', name: 'Leyssa Coaching', sub: 'leyssa', type: 'Coach indépendante', city: 'Thonon (74)',
      plan: 'Essentiel', mrr: 49, status: 'actif', since: '18 juillet 2026', ref: 'DEM-2026-0129',
      theme: { label: 'Leyssa — rose & sauge', swatch: ['#CB5B74', '#EAF0E2'], delivered: '21 juillet 2026', kind: 'Sur-mesure' },
      usage: { membres: 64, coursSem: 11, resa30: 238, conn30: 41, serie: [12, 18, 22, 26, 31, 29, 35, 38, 36, 41, 44, 42] },
      health: { label: 'Usage régulier', tone: 'success' },
      comptes: [
        { name: 'Najate Amzil', role: 'Coach & gérante', email: 'contact@najcoaching.fr', last: "aujourd'hui · 07:05", state: 'Actif' },
      ],
      billing: { next: '1 septembre 2026', method: 'Prélèvement SEPA', state: 'ok', stateLabel: 'À jour', lastPaid: '1 août 2026 · 49 €' },
      contact: { name: 'Najate Amzil', email: 'contact@najcoaching.fr', phone: '06 71 30 45 12' },
    },
    {
      id: 'atlas', name: 'Atlas Training Club', sub: 'atlas-training', type: 'Salle de sport', city: 'Annecy (74)',
      plan: 'Pro', mrr: 129, status: 'essai', since: '2 août 2026', ref: 'DEM-2026-0148',
      theme: { label: 'Graphite (thème standard)', swatch: ['#232327', '#f4f6f8'], delivered: '2 août 2026', kind: 'Standard' },
      usage: { membres: 96, coursSem: 18, resa30: 187, conn30: 33, serie: [0, 0, 0, 0, 0, 0, 0, 0, 12, 26, 38, 44] },
      health: { label: 'Essai — 15 jours restants', tone: 'brand' },
      comptes: [
        { name: 'Camille Fournier', role: 'Gérante', email: 'camille@atlas-training.fr', last: 'hier · 21:14', state: 'Actif' },
        { name: 'Yanis Roche', role: 'Coach référent', email: 'yanis@atlas-training.fr', last: 'il y a 5 j', state: 'Invitation en attente' },
      ],
      billing: { next: '21 août 2026 — fin d’essai', method: 'Aucun moyen enregistré', state: 'trial', stateLabel: 'Essai gratuit', lastPaid: '—' },
      contact: { name: 'Camille Fournier', email: 'camille@atlas-training.fr', phone: '06 42 18 77 05' },
    },
    {
      id: 'sciez', name: 'Halle des Sports — Ville de Sciez', sub: 'sciez-sports', type: 'Collectivité', city: 'Sciez (74)',
      plan: 'Sur-mesure', mrr: 240, status: 'actif', since: '4 juin 2026', ref: 'DEM-2026-0092',
      theme: { label: 'Blason Sciez — indigo', swatch: ['#4C5BD4', '#eef0fb'], delivered: '11 juin 2026', kind: 'Sur-mesure' },
      usage: { membres: 612, coursSem: 47, resa30: 2038, conn30: 128, serie: [70, 82, 88, 95, 91, 104, 110, 98, 86, 61, 44, 38] },
      health: { label: 'Usage en baisse — vacances scolaires', tone: 'warning' },
      comptes: [
        { name: 'Marc Belhadj', role: 'Responsable des équipements', email: 'm.belhadj@sciez.fr', last: 'il y a 2 j', state: 'Actif' },
        { name: 'Sandrine Perret', role: 'Agent administratif', email: 's.perret@sciez.fr', last: 'il y a 9 j', state: 'Actif' },
        { name: 'Compte associations', role: 'Lecture seule', email: 'assos@sciez.fr', last: 'il y a 1 mois', state: 'Actif' },
      ],
      billing: { next: '1 septembre 2026', method: 'Virement · mandat administratif', state: 'ok', stateLabel: 'À jour', lastPaid: '28 juillet 2026 · 240 €' },
      contact: { name: 'Marc Belhadj', email: 'm.belhadj@sciez.fr', phone: '04 50 72 61 30' },
    },
    {
      id: 'vertical', name: 'Studio Vertical', sub: 'studio-vertical', type: 'Salle d’escalade', city: 'Annemasse (74)',
      plan: 'Essentiel', mrr: 49, status: 'suspendu', since: '3 mai 2026', ref: 'DEM-2026-0071',
      theme: { label: 'Ambre (thème standard)', swatch: ['#D08A2C', '#fbf5ea'], delivered: '5 mai 2026', kind: 'Standard' },
      usage: { membres: 118, coursSem: 6, resa30: 74, conn30: 4, serie: [44, 46, 41, 38, 30, 26, 19, 14, 11, 8, 5, 2] },
      health: { label: 'Aucune connexion depuis 16 jours', tone: 'danger' },
      comptes: [
        { name: 'Hugo Delattre', role: 'Gérant', email: 'hugo@studiovertical.fr', last: 'il y a 16 j', state: 'Actif' },
      ],
      billing: { next: 'Prélèvement rejeté le 1 août', method: 'Prélèvement SEPA', state: 'late', stateLabel: '2 échéances impayées', lastPaid: '1 juin 2026 · 49 €' },
      contact: { name: 'Hugo Delattre', email: 'hugo@studiovertical.fr', phone: '06 09 55 12 88' },
    },
    {
      id: 'elan', name: 'Élan Gym Évian', sub: 'elan-gym', type: 'Association', city: 'Évian-les-Bains (74)',
      plan: 'Essentiel', mrr: 49, status: 'actif', since: '20 juin 2026', ref: 'DEM-2026-0104',
      theme: { label: 'Azure (thème par défaut)', swatch: ['#00ABFC', '#0C2236'], delivered: '20 juin 2026', kind: 'Défaut' },
      usage: { membres: 143, coursSem: 14, resa30: 402, conn30: 58, serie: [20, 28, 34, 40, 46, 52, 49, 55, 61, 58, 63, 66] },
      health: { label: 'Usage régulier', tone: 'success' },
      comptes: [
        { name: 'Patricia Nguyen', role: 'Présidente', email: 'presidence@elangym.fr', last: 'hier · 18:02', state: 'Actif' },
        { name: 'Kevin Ottavi', role: 'Secrétaire', email: 'secretariat@elangym.fr', last: 'il y a 4 j', state: 'Actif' },
      ],
      billing: { next: '1 septembre 2026', method: 'Prélèvement SEPA', state: 'ok', stateLabel: 'À jour', lastPaid: '1 août 2026 · 49 €' },
      contact: { name: 'Patricia Nguyen', email: 'presidence@elangym.fr', phone: '04 50 26 71 09' },
    },
  ],

  /* ---------- facturation : suivi seulement ---------- */
  invoices: [
    { ref: 'F-2026-0214', client: 'Halle des Sports — Ville de Sciez', period: 'Août 2026', amount: '240 €', issued: '28 juil. 2026', due: '27 août 2026', status: 'paye' },
    { ref: 'F-2026-0213', client: "Team Trainer's", period: 'Août 2026', amount: '129 €', issued: '1 août 2026', due: '1 août 2026', status: 'paye' },
    { ref: 'F-2026-0212', client: 'Leyssa Coaching', period: 'Août 2026', amount: '49 €', issued: '1 août 2026', due: '1 août 2026', status: 'paye' },
    { ref: 'F-2026-0211', client: 'Élan Gym Évian', period: 'Août 2026', amount: '49 €', issued: '1 août 2026', due: '1 août 2026', status: 'paye' },
    { ref: 'F-2026-0210', client: 'Studio Vertical', period: 'Août 2026', amount: '49 €', issued: '1 août 2026', due: '1 août 2026', status: 'echec' },
    { ref: 'F-2026-0198', client: 'Studio Vertical', period: 'Juillet 2026', amount: '49 €', issued: '1 juil. 2026', due: '1 juil. 2026', status: 'impaye' },
    { ref: 'F-2026-0197', client: "Team Trainer's", period: 'Juillet 2026', amount: '129 €', issued: '1 juil. 2026', due: '1 juil. 2026', status: 'paye' },
    { ref: 'F-2026-0196', client: 'Halle des Sports — Ville de Sciez', period: 'Juillet 2026', amount: '240 €', issued: '28 juin 2026', due: '28 juil. 2026', status: 'paye' },
  ],

  /* ---------- support : tickets ouverts depuis le bouton « Aide » ---------- */
  tickets: [
    {
      id: 't1', ref: 'SUP-0312', client: 'Studio Vertical', clientId: 'vertical',
      who: 'Hugo Delattre', role: 'Gérant', subject: 'Je n’arrive plus à me connecter',
      when: '4 août · 09:30', age: '2 j', status: 'ouvert', priority: 'haute',
      ctx: { screen: 'Connexion', url: '/connexion', browser: 'Safari 18.2', os: 'macOS 15.4', version: 'GymXYZ 1.9.2', account: 'hugo@studiovertical.fr', tenant: 'studio-vertical' },
      thread: [
        { who: 'Hugo Delattre', when: '4 août · 09:30', side: 'them', txt: 'Bonjour, depuis lundi je tombe sur « espace suspendu » quand je me connecte. Mes coachs aussi. Qu’est-ce qui se passe ?' },
      ],
      hint: 'Espace suspendu automatiquement après 2 échéances impayées. Régler la facturation avant de répondre.',
    },
    {
      id: 't2', ref: 'SUP-0311', client: 'Halle des Sports — Ville de Sciez', clientId: 'sciez',
      who: 'Sandrine Perret', role: 'Agent administratif', subject: 'Export des créneaux associatifs en PDF',
      when: '4 août · 15:12', age: '1 j', status: 'ouvert', priority: 'normale',
      ctx: { screen: 'Planning', url: '/planning?vue=semaine', browser: 'Edge 128', os: 'Windows 11', version: 'GymXYZ 1.9.2', account: 's.perret@sciez.fr', tenant: 'sciez-sports' },
      thread: [
        { who: 'Sandrine Perret', when: '4 août · 15:12', side: 'them', txt: 'Le service juridique demande le planning de la saison en PDF pour les conventions. Je ne trouve que l’export tableur.' },
      ],
      hint: 'Export PDF du planning : pas encore développé. Candidat pour le lot 8.',
    },
    {
      id: 't3', ref: 'SUP-0310', client: 'Atlas Training Club', clientId: 'atlas',
      who: 'Camille Fournier', role: 'Gérante', subject: 'Invitation coach jamais reçue',
      when: '3 août · 11:48', age: '3 j', status: 'en-cours', priority: 'normale',
      ctx: { screen: 'Coachs', url: '/coachs', browser: 'Chrome 129', os: 'Windows 11', version: 'GymXYZ 1.9.2', account: 'camille@atlas-training.fr', tenant: 'atlas-training' },
      thread: [
        { who: 'Camille Fournier', when: '3 août · 11:48', side: 'them', txt: 'J’ai invité Yanis trois fois, il ne reçoit rien. Il utilise une adresse Orange.' },
        { who: 'Vous', when: '3 août · 14:02', side: 'me', txt: 'Bonjour Camille, l’e-mail est bien parti trois fois. Chez Orange il finit souvent en indésirables — demandez à Yanis de vérifier, je relance en parallèle depuis un autre expéditeur.' },
      ],
      hint: 'Délivrabilité Orange : 3 rebonds mous ce mois-ci, tous clients confondus.',
    },
    {
      id: 't4', ref: 'SUP-0309', client: "Team Trainer's", clientId: 'teamtrainers',
      who: 'Sofia Marchetti', role: 'Accueil', subject: 'Le QR code de présence ne scanne pas sur un iPhone',
      when: '2 août · 18:24', age: '4 j', status: 'en-cours', priority: 'haute',
      ctx: { screen: 'Présences', url: '/presences/qr', browser: 'Safari iOS 18.2', os: 'iOS 18.2', version: 'GymXYZ 1.9.2', account: 'accueil@teamtrainers.fr', tenant: 'teamtrainers' },
      thread: [
        { who: 'Sofia Marchetti', when: '2 août · 18:24', side: 'them', txt: 'Sur le cours de 18h30, la moitié des membres n’arrive pas à scanner. Ça tourne puis ça revient à l’écran d’accueil.' },
        { who: 'Vous', when: '2 août · 19:10', side: 'me', txt: 'Merci Sofia. Je reproduis le problème sur iOS 18.2 — correctif prévu dans la 1.9.3, en attendant utilisez le pointage manuel depuis la liste du cours.' },
      ],
      hint: 'Reproduit. Corrigé dans la 1.9.3 (livraison prévue le 12 août).',
    },
    {
      id: 't5', ref: 'SUP-0308', client: 'Élan Gym Évian', clientId: 'elan',
      who: 'Kevin Ottavi', role: 'Secrétaire', subject: 'Ajouter une discipline « gym douce séniors »',
      when: '31 juil. · 10:02', age: '6 j', status: 'resolu', priority: 'basse',
      ctx: { screen: 'Cours', url: '/cours/nouveau', browser: 'Firefox 130', os: 'Windows 10', version: 'GymXYZ 1.9.1', account: 'secretariat@elangym.fr', tenant: 'elan-gym' },
      thread: [
        { who: 'Kevin Ottavi', when: '31 juil. · 10:02', side: 'them', txt: 'La liste des disciplines ne propose pas la gym douce séniors, c’est notre créneau le plus rempli.' },
        { who: 'Vous', when: '31 juil. · 11:20', side: 'me', txt: 'Ajoutée au référentiel commun, elle apparaît chez vous après rechargement. Merci pour le signalement.' },
      ],
      hint: null,
    },
  ],

  /* ---------- formules plateforme ---------- */
  plans: [
    { id: 'essentiel', name: 'Essentiel', price: '49 €', unit: '/ mois', clients: 3, mrr: '147 €',
      for: 'Coach indépendant, association, petite structure.',
      limits: ['Jusqu’à 150 membres', '1 lieu', '2 comptes gestionnaires'],
      features: ['Planning & réservations', 'Fiches membres', 'Abonnements et relances', 'Thème standard'] },
    { id: 'pro', name: 'Pro', price: '129 €', unit: '/ mois', clients: 2, mrr: '258 €', reco: true,
      for: 'Salle avec équipe de coachs.',
      limits: ['Jusqu’à 600 membres', '3 lieux', '10 comptes gestionnaires'],
      features: ['Tout Essentiel', 'Multi-lieux & multi-coachs', 'Présences, QR code, statistiques', 'Marque blanche complète'] },
    { id: 'surmesure', name: 'Sur-mesure', price: 'Sur devis', unit: '', clients: 1, mrr: '240 €',
      for: 'Collectivité, réseau, franchise.',
      limits: ['Membres illimités', 'Lieux illimités', 'Comptes illimités'],
      features: ['Tout Pro', 'Développements dédiés', 'Reprise de vos données', 'Interlocuteur unique'] },
  ],

  /* ---------- santé technique ---------- */
  services: [
    { name: 'Application', detail: 'app.gymxyz.fr · Blazor Server', state: 'ok', metric: '99,98 % sur 30 j' },
    { name: 'Base de données', detail: 'MySQL · 6 tenants', state: 'ok', metric: '412 Mo · latence 18 ms' },
    { name: 'Envoi d’e-mails', detail: 'Transactionnel', state: 'warn', metric: '3 rebonds mous (Orange)' },
    { name: 'Sauvegardes', detail: 'Quotidienne · 03:00', state: 'ok', metric: 'Dernière : ce matin 03:04' },
  ],
  incidents: [
    { when: '2 août · 18:20 → 19:40', title: 'Scan QR inopérant sur iOS 18.2', impact: '1 client · 2 cours', state: 'Correctif dans la 1.9.3' },
    { when: '24 juillet · 02:10 → 02:26', title: 'Redémarrage après mise à jour serveur', impact: 'Tous les clients · 16 min', state: 'Clos' },
  ],
  audit: [
    { when: "aujourd'hui · 08:41", actor: 'Vous', action: 'Consultation fiche client', target: 'Studio Vertical' },
    { when: "aujourd'hui · 08:39", actor: 'Système', action: 'Espace suspendu (2 impayés)', target: 'Studio Vertical' },
    { when: 'hier · 17:02', actor: 'Vous', action: 'Demande validée · espace ouvert', target: 'atlas-training.gymxyz.fr' },
    { when: 'hier · 16:58', actor: 'Vous', action: 'Formule modifiée · Essentiel → Pro', target: 'Atlas Training Club' },
    { when: '4 août · 11:15', actor: 'Système', action: 'Prélèvement rejeté', target: 'Studio Vertical · 49 €' },
    { when: '3 août · 14:02', actor: 'Vous', action: 'Réponse au ticket SUP-0310', target: 'Atlas Training Club' },
    { when: '31 juil. · 11:18', actor: 'Vous', action: 'Référentiel disciplines modifié', target: '+ Gym douce séniors' },
    { when: '28 juil. · 09:00', actor: 'Système', action: 'Facture émise', target: 'Ville de Sciez · 240 €' },
  ],

  /* ---------- référentiels communs à tous les espaces ---------- */
  refs: {
    disciplines: ['Musculation', 'Cross-training', 'HIIT', 'Cardio', 'Cycling', 'Yoga', 'Pilates', 'Mobilité', 'Renforcement', 'Boxe', 'Escalade', 'Gym douce séniors', 'Pré/post-natal', 'Coaching individuel'],
    zones: [
      { zone: 'Zone A', dept: 'Ain, Isère, Savoie…', next: 'Toussaint : 17 oct. → 2 nov. 2026' },
      { zone: 'Zone B', dept: 'Haute-Savoie (nos clients)', next: 'Toussaint : 17 oct. → 2 nov. 2026' },
      { zone: 'Zone C', dept: 'Paris, Créteil, Versailles', next: 'Toussaint : 17 oct. → 2 nov. 2026' },
    ],
    calendarSource: 'data.education.gouv.fr — synchronisé le 1 août 2026',
    mails: [
      { key: 'invitation', name: 'Invitation d’un gestionnaire', trigger: 'Ouverture d’espace ou ajout de compte', edited: '21 juil. 2026', sent30: 7 },
      { key: 'reservation', name: 'Confirmation de réservation', trigger: 'Membre réserve un cours', edited: '4 juin 2026', sent30: 1148 },
      { key: 'rappel', name: 'Rappel de séance (J-1)', trigger: 'Veille du cours à 18:00', edited: '4 juin 2026', sent30: 986 },
      { key: 'echeance', name: 'Abonnement bientôt échu', trigger: 'J-7 avant échéance', edited: '12 juil. 2026', sent30: 63 },
      { key: 'impaye', name: 'Prélèvement rejeté', trigger: 'Échec de paiement', edited: '12 juil. 2026', sent30: 2 },
    ],
  },
};
