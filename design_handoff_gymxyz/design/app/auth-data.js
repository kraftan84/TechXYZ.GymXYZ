/* ============================================================
   GymXYZ — Auth & Onboarding demo data
   Demandes d'ouverture d'espace · clients (tenants) · formules.
   Données de démonstration, cohérentes mais fictives.
   ============================================================ */
window.GX_AUTH = {

  /* ---------- formules proposées à la demande ---------- */
  plans: [
    { id: 'essentiel', name: 'Essentiel', price: '49 €', unit: '/ mois', for: 'Coach indépendant ou petite structure, jusqu’à 80 membres.',
      features: ['Planning & réservations', 'Fiches membres', 'Abonnements et relances'] },
    { id: 'pro', name: 'Pro', price: '129 €', unit: '/ mois', reco: 'Le plus demandé',
      for: 'Salle avec équipe de coachs, jusqu’à 600 membres.',
      features: ['Tout Essentiel', 'Multi-lieux & multi-coachs', 'Présences, QR code, statistiques', 'Marque blanche complète'] },
    { id: 'surmesure', name: 'Sur-mesure', price: 'Sur devis', unit: '', for: 'Réseau, franchise ou besoin métier spécifique.',
      features: ['Tout Pro', 'Développements dédiés', 'Reprise de vos données', 'Interlocuteur unique'] },
  ],

  /* ---------- couleurs d'accent proposées (marque blanche) ---------- */
  accents: [
    { id: 'azure', label: 'Azure', hex: '#00ABFC' },
    { id: 'graphite', label: 'Graphite', hex: '#232327' },
    { id: 'rose', label: 'Rose', hex: '#CB5B74' },
    { id: 'sauge', label: 'Sauge', hex: '#7E8E64' },
    { id: 'ambre', label: 'Ambre', hex: '#D08A2C' },
    { id: 'indigo', label: 'Indigo', hex: '#4C5BD4' },
  ],

  /* ---------- tailles de structure ---------- */
  sizes: ['Moins de 50 membres', '50 à 150 membres', '150 à 400 membres', '400 à 800 membres', 'Plus de 800 membres'],
  sizesSolo: ['Moins de 20 clients', '20 à 50 clients', '50 à 120 clients', 'Plus de 120 clients'],
  roles: ['Gérant·e', 'Responsable administratif', 'Coach & gérant·e', 'Président·e d’association', 'Autre'],

  /* ---------- ce qui se passe après l'envoi ---------- */
  next: [
    { t: 'Demande reçue', d: 'Nous avons votre dossier. Un accusé de réception part par e-mail.', state: 'done' },
    { t: 'Vérification', d: 'Nous contrôlons les informations de la structure. Un jour ouvré en moyenne.', state: 'now' },
    { t: 'Échange de 20 minutes', d: 'Un appel pour cadrer vos besoins et votre planning de démarrage.' },
    { t: 'Devis puis ouverture de l’espace', d: 'À la signature, votre espace est ouvert et vos comptes créés.' },
  ],

  /* ---------- demandes d'ouverture (super-admin) ---------- */
  demandes: [
    {
      id: 'd1', ref: 'DEM-2026-0148', type: 'salle', name: 'Atlas Training Club', city: 'Annecy (74)',
      contact: { name: 'Camille Fournier', role: 'Gérante', email: 'camille@atlas-training.fr', phone: '06 42 18 77 05' },
      siret: '918 402 551 00019', members: '150 à 400 membres', disciplines: 'Musculation, cross-training, cours collectifs',
      plan: 'Pro', received: '6 août 2026 · 09:12', status: 'a-traiter', owner: null, source: 'Site vitrine',
      brand: { accent: '#232327', accentLabel: 'Graphite', sub: 'atlas-training', logo: 'logo-atlas.png' },
      message: 'Nous ouvrons une deuxième salle en septembre et nous gérons encore le planning sur un tableur partagé.',
      activity: [
        { t: 'Demande envoyée', d: 'Formulaire en 6 étapes · IP FR', when: '6 août · 09:12', state: 'done' },
        { t: 'Accusé de réception envoyé', d: 'camille@atlas-training.fr', when: '6 août · 09:12', state: 'done' },
      ],
      notes: [],
    },
    {
      id: 'd2', ref: 'DEM-2026-0147', type: 'coach', name: 'Naj Coaching', city: 'Thonon-les-Bains (74)',
      contact: { name: 'Najate Amzil', role: 'Coach & gérante', email: 'contact@najcoaching.fr', phone: '06 71 30 45 12' },
      siret: '902 771 118 00027', members: '20 à 50 clients', disciplines: 'Renforcement, pré/post-natal, coaching à domicile',
      plan: 'Essentiel', received: '5 août 2026 · 18:40', status: 'a-traiter', owner: null, source: 'Recommandation cliente',
      brand: { accent: '#CB5B74', accentLabel: 'Rose', sub: 'naj-coaching', logo: null },
      message: 'Je suis seule, je veux surtout arrêter les allers-retours par SMS pour les créneaux.',
      activity: [
        { t: 'Demande envoyée', d: 'Formulaire en 6 étapes · mobile', when: '5 août · 18:40', state: 'done' },
        { t: 'Accusé de réception envoyé', d: 'contact@najcoaching.fr', when: '5 août · 18:41', state: 'done' },
      ],
      notes: [],
    },
    {
      id: 'd3', ref: 'DEM-2026-0146', type: 'salle', name: 'Halle des Sports — Ville de Sciez', city: 'Sciez (74)',
      contact: { name: 'Marc Belhadj', role: 'Responsable des équipements', email: 'm.belhadj@sciez.fr', phone: '04 50 72 61 30' },
      siret: '217 402 641 00013', members: '400 à 800 membres', disciplines: 'Associations sportives, créneaux scolaires',
      plan: 'Sur-mesure', received: '5 août 2026 · 11:05', status: 'a-traiter', owner: null, source: 'Marché public',
      brand: { accent: '#4C5BD4', accentLabel: 'Indigo', sub: 'sciez-sports', logo: 'blason-sciez.png' },
      message: 'Nous cherchons à remplacer le planning papier des créneaux associatifs. Marché sous 40 000 €.',
      activity: [
        { t: 'Demande envoyée', d: 'Formulaire en 6 étapes', when: '5 août · 11:05', state: 'done' },
        { t: 'Accusé de réception envoyé', d: 'm.belhadj@sciez.fr', when: '5 août · 11:06', state: 'done' },
      ],
      notes: [],
    },
    {
      id: 'd4', ref: 'DEM-2026-0145', type: 'salle', name: 'Studio Vertika', city: 'Genève / Ferney (74)',
      contact: { name: 'Lise Marchand', role: 'Gérante', email: 'lise@vertika.studio', phone: '06 08 55 41 90' },
      siret: '934 118 002 00011', members: '50 à 150 membres', disciplines: 'Pilates, yoga, mobilité',
      plan: 'Pro', received: '4 août 2026 · 15:22', status: 'en-cours', owner: 'Julien Roux', source: 'Site vitrine',
      brand: { accent: '#7E8E64', accentLabel: 'Sauge', sub: 'vertika', logo: 'vertika-mark.svg' },
      message: 'Deux studios, 9 créneaux par jour, beaucoup de cartes de 10 séances à suivre.',
      activity: [
        { t: 'Demande envoyée', d: 'Formulaire en 6 étapes', when: '4 août · 15:22', state: 'done' },
        { t: 'Prise en charge', d: 'Assignée à Julien Roux', when: '4 août · 16:05', state: 'done' },
        { t: 'Appel de cadrage planifié', d: 'Jeudi 7 août · 14:00 (20 min)', when: '5 août · 09:30', state: 'now' },
      ],
      notes: [
        { who: 'Julien Roux', when: '5 août · 09:32', txt: 'Besoin réel : cartes de séances + rappels. Formule Pro cohérente. Vérifier la reprise de leur fichier Excel (1 100 lignes).' },
      ],
    },
    {
      id: 'd5', ref: 'DEM-2026-0143', type: 'coach', name: 'Yohan Prieur Performance', city: 'Évian-les-Bains (74)',
      contact: { name: 'Yohan Prieur', role: 'Coach indépendant', email: 'yohan@ypperf.fr', phone: '07 62 14 88 03' },
      siret: '—', members: 'Moins de 20 clients', disciplines: 'Préparation physique, trail',
      plan: 'Essentiel', received: '2 août 2026 · 20:14', status: 'en-cours', owner: 'Julien Roux', source: 'Instagram',
      brand: { accent: '#D08A2C', accentLabel: 'Ambre', sub: 'yp-perf', logo: null },
      message: 'Je démarre mon activité en septembre, je n’ai pas encore de SIRET.',
      activity: [
        { t: 'Demande envoyée', d: 'Formulaire en 6 étapes · mobile', when: '2 août · 20:14', state: 'done' },
        { t: 'Complément demandé', d: 'SIRET ou récépissé d’immatriculation', when: '3 août · 08:50', state: 'now' },
      ],
      notes: [
        { who: 'Julien Roux', when: '3 août · 08:51', txt: 'En attente du SIRET. Relancer le 10 août si pas de réponse.' },
      ],
    },
    {
      id: 'd6', ref: 'DEM-2026-0139', type: 'salle', name: "Team Trainer's", city: 'Thonon-les-Bains (74)',
      contact: { name: 'Kevin Tissot', role: 'Gérant', email: 'kevin@teamtrainers.fr', phone: '06 33 90 12 47' },
      siret: '889 204 771 00024', members: '150 à 400 membres', disciplines: 'Cross-training, boxe, musculation',
      plan: 'Pro', received: '21 juillet 2026 · 10:02', status: 'validee', owner: 'Julien Roux', source: 'Site vitrine',
      brand: { accent: '#232327', accentLabel: 'Graphite', sub: 'teamtrainers', logo: 'teamtrainers-mark.png' },
      message: 'On veut un espace à notre marque, noir et blanc, sans logo GymXYZ visible côté membres.',
      activity: [
        { t: 'Demande envoyée', d: 'Formulaire en 6 étapes', when: '21 juil. · 10:02', state: 'done' },
        { t: 'Appel de cadrage', d: '25 min · besoins validés', when: '22 juil. · 11:00', state: 'done' },
        { t: 'Devis signé', d: 'Formule Pro · 129 € / mois', when: '24 juil. · 16:20', state: 'done' },
        { t: 'Espace ouvert', d: 'teamtrainers.gymxyz.fr · thème Team Trainer’s', when: '25 juil. · 09:00', state: 'done' },
      ],
      notes: [
        { who: 'Julien Roux', when: '25 juil. · 09:04', txt: 'Espace provisionné, thème monochrome appliqué. Kevin a importé 287 membres lui-même.' },
      ],
    },
    {
      id: 'd7', ref: 'DEM-2026-0138', type: 'coach', name: 'Leyssa Coaching', city: 'Thonon (74)',
      contact: { name: 'Najate Amzil', role: 'Coach indépendante', email: 'najate@leyssa-coaching.fr', phone: '06 71 30 45 12' },
      siret: '921 660 338 00012', members: '50 à 120 clients', disciplines: 'Coaching féminin, remise en forme',
      plan: 'Essentiel', received: '18 juillet 2026 · 14:35', status: 'validee', owner: 'Julien Roux', source: 'Recommandation',
      brand: { accent: '#CB5B74', accentLabel: 'Rose', sub: 'leyssa', logo: 'leyssa-mark.png' },
      message: 'Je veux quelque chose de doux, à mon image, avec « Révélez-vous ».',
      activity: [
        { t: 'Demande envoyée', d: 'Formulaire en 6 étapes', when: '18 juil. · 14:35', state: 'done' },
        { t: 'Devis signé', d: 'Formule Essentiel · 49 € / mois', when: '20 juil. · 09:10', state: 'done' },
        { t: 'Espace ouvert', d: 'leyssa.gymxyz.fr · thème Leyssa Coaching', when: '20 juil. · 15:00', state: 'done' },
      ],
      notes: [],
    },
    {
      id: 'd8', ref: 'DEM-2026-0131', type: 'salle', name: 'FitZone 24/7', city: 'Lyon (69)',
      contact: { name: 'Damien Costa', role: 'Franchisé', email: 'd.costa@fitzone-247.com', phone: '06 12 44 09 81' },
      siret: '—', members: 'Plus de 800 membres', disciplines: 'Salle en libre accès',
      plan: 'Sur-mesure', received: '9 juillet 2026 · 22:48', status: 'refusee', owner: 'Julien Roux', source: 'Formulaire',
      brand: { accent: '#00ABFC', accentLabel: 'Azure', sub: 'fitzone', logo: null },
      message: 'Besoin d’un contrôle d’accès par tourniquet connecté sur 14 sites.',
      activity: [
        { t: 'Demande envoyée', d: 'Formulaire en 6 étapes', when: '9 juil. · 22:48', state: 'done' },
        { t: 'Demande refusée', d: 'Contrôle d’accès matériel hors périmètre', when: '11 juil. · 10:15', state: 'done' },
      ],
      notes: [
        { who: 'Julien Roux', when: '11 juil. · 10:16', txt: 'Refus poli : le pilotage de tourniquets sur 14 sites n’est pas notre métier. Deux intégrateurs conseillés en réponse.' },
      ],
    },
  ],

  refusMotifs: [
    'Besoin hors périmètre du produit',
    'Structure hors zone d’intervention',
    'Informations invérifiables',
    'Projet non abouti / sans budget',
    'Doublon d’une demande existante',
  ],

  /* ---------- clients (tenants) déjà ouverts ---------- */
  clients: [
    { name: "Team Trainer's", sub: 'teamtrainers', type: 'Salle de sport', plan: 'Pro', members: 287, mrr: '129 €', status: 'actif', since: 'juil. 2026', last: 'il y a 12 min' },
    { name: 'Leyssa Coaching', sub: 'leyssa', type: 'Coach indépendante', plan: 'Essentiel', members: 64, mrr: '49 €', status: 'actif', since: 'juil. 2026', last: 'il y a 2 h' },
    { name: 'Studio Kinéo', sub: 'kineo', type: 'Studio Pilates', plan: 'Pro', members: 143, mrr: '129 €', status: 'actif', since: 'mai 2026', last: 'hier' },
    { name: 'ASM Gym Annemasse', sub: 'asm-gym', type: 'Association', plan: 'Essentiel', members: 210, mrr: '49 €', status: 'actif', since: 'mars 2026', last: 'il y a 4 h' },
    { name: 'Boxe Club Léman', sub: 'bc-leman', type: 'Association', plan: 'Essentiel', members: 88, mrr: '0 €', status: 'essai', since: 'août 2026', last: 'il y a 35 min' },
    { name: 'Vertika (pré-ouverture)', sub: 'vertika', type: 'Studio', plan: 'Pro', members: 0, mrr: '0 €', status: 'essai', since: 'août 2026', last: '—' },
    { name: 'CrossPoint Chablais', sub: 'crosspoint', type: 'Salle de sport', plan: 'Pro', members: 176, mrr: '129 €', status: 'suspendu', since: 'févr. 2026', last: 'il y a 21 j' },
  ],

  admin: { name: 'Julien Roux', role: 'Super-admin' },
};
