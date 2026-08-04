/* ============================================================
   GymXYZ — data layer
   THEMES registry (white-label) + credible fictional data.
   Same data across themes — only the skin changes.
   ============================================================ */

window.GX_THEMES = [
  {
    id: 'techxyz', label: 'GymXYZ', sub: 'Défaut · base TechXYZ',
    name: 'GymXYZ', wordmark: { a: 'GYM', b: 'XYZ' },
    markType: 'kettlebell', swatch: ['#00ABFC', '#0C2236'],
    manager: { name: 'Dwayne Johnson', nick: 'The Rock', first: 'Dwayne', role: 'Gérant' },
  },
  {
    id: 'teamtrainers', label: "Team Trainer's", sub: 'Salle de sport',
    name: "Team Trainer's", wordmark: { full: "TEAM TRAINER'S" },
    markType: 'img', markSrc: 'assets/themes/teamtrainers-mark.png',
    markSrcDark: 'assets/themes/teamtrainers-white.png',
    swatch: ['#232327', '#fafafa'],
    manager: { name: 'Aurélie Siquier', nick: 'Lily', first: 'Aurélie', role: 'Gérante' },
  },
  {
    id: 'leyssa', label: 'Leyssa Coaching', sub: 'Coach indépendante',
    name: 'Leyssa Coaching', wordmark: { full: 'Leyssa Coaching' },
    markType: 'img', markSrc: 'assets/themes/leyssa-mark.png', circle: true,
    swatch: ['#CB5B74', '#EAF0E2'], solo: true,
    manager: { name: 'Najate Amzil', nick: 'Naj', first: 'Najate', role: 'Coach' },
  },
];

window.GX_DATA = {
  manager: { name: 'Aurélie Siquier', first: 'Aurélie', initials: 'AS', role: 'Gérante' },
  city: 'Lyon 3ᵉ',

  /* ---------- dashboard ---------- */
  weekRange: 'Semaine du 8 au 14 juin',
  week: [
    { d: 'Lun', n: 8, c: 4 }, { d: 'Mar', n: 9, c: 5 }, { d: 'Mer', n: 10, c: 3 },
    { d: 'Jeu', n: 11, c: 4 }, { d: 'Ven', n: 12, c: 5 }, { d: 'Sam', n: 13, c: 6 }, { d: 'Dim', n: 14, c: 1 },
  ],
  todayClasses: [
    { time: '09:00', name: 'HIIT Blast', meta: 'Studio A · Nora Lemoine', occ: 12, cap: 16 },
    { time: '11:00', name: 'Coaching Perso', meta: 'Studio C · Samir El Amrani', occ: 1, cap: 1, status: { tone: 'neutral', t: 'Privé' } },
    { time: '18:30', name: 'Power Cycle', meta: 'Studio C · Nora Lemoine', occ: 24, cap: 24, status: { tone: 'danger', t: 'Complet' } },
  ],
  alerts: [
    { ic: 'card', tone: 'warning', t: '4 abonnements expirent', d: 'Cette semaine — pensez à relancer', action: 'Relancer' },
    { ic: 'alert', tone: 'danger', t: '2 paiements en retard', d: '180 € à encaisser', action: 'Voir' },
    { ic: 'check', tone: 'brand', t: "Présences d'hier", d: '3 cours à pointer', action: 'Pointer' },
  ],

  /* ---------- planning ---------- */
  hours: [7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21],
  events: {
    0: { 8: ['Strength Foundations', 'N. Lemoine', 14, 20], 12: ['Core Express', 'S. El Amrani', 16, 18] },
    1: { 18: ['Pilates Core', 'N. Lemoine', 12, 16], 20: ['Mobility Reset', 'S. El Amrani', 8, 14] },
    2: { 19: ['Boxing Fundamentals', 'T. Garnier', 10, 16] },
    3: { 17: ['Yoga Restore', 'N. Lemoine', 14, 20], 19: ['Strength Lab', 'S. El Amrani', 18, 22] },
    4: { 9: ['HIIT Blast', 'N. Lemoine', 12, 16], 18: ['Power Cycle', 'N. Lemoine', 24, 24] },
    5: { 10: ['Open Gym', '—', 9, 30], 11: ['Coaching Perso', 'S. El Amrani', 1, 1] },
    6: { 9: ['HIIT Blast', 'N. Lemoine', 5, 16] },
  },
  daySlots: [
    { time: '09:00', name: 'HIIT Blast', meta: 'Studio A · Nora Lemoine · 60 min', occ: 12, cap: 16, done: true },
    { time: '18:00', name: 'Power Cycle', meta: 'Studio C · Nora Lemoine · 45 min', occ: 24, cap: 24, done: false },
  ],

  /* ---------- members ---------- */
  members: [
    { id: 'laetitia', name: 'Laetitia Moriceau', initials: 'LM', email: 'laetitia.moriceau@gymxyz.fr', phone: '06 12 34 56 78',
      plan: 'Illimité mensuel', planTone: 'brand', gauge: 100, gaugeLabel: '∞', presence: '92%', last: 'il y a 2 j',
      tone: 'success', status: 'Actif', since: 'mars 2024' },
    { id: 'camille', name: 'Camille Durand', initials: 'CD', email: 'camille.durand@gymxyz.fr', phone: '06 22 11 90 04',
      plan: 'Carte 10 séances', planTone: 'neutral', gauge: 30, gaugeLabel: '3/10', presence: '78%', last: 'il y a 1 j',
      tone: 'warning', status: 'Expire bientôt', since: 'janv. 2025' },
    { id: 'lucas', name: 'Lucas Martin', initials: 'LM', email: 'lucas.martin@gymxyz.fr', phone: '06 80 45 12 33',
      plan: 'Carte 10 séances', planTone: 'neutral', gauge: 60, gaugeLabel: '6/10', presence: '64%', last: 'il y a 5 j',
      tone: 'success', status: 'Actif', since: 'oct. 2024' },
    { id: 'amina', name: 'Amina Benali', initials: 'AB', email: 'amina.benali@gymxyz.fr', phone: '06 14 78 22 09',
      plan: 'Illimité mensuel', planTone: 'brand', gauge: 100, gaugeLabel: '∞', presence: '88%', last: "aujourd'hui",
      tone: 'success', status: 'Actif', since: 'févr. 2024' },
    { id: 'theo', name: 'Théo Garnier', initials: 'TG', email: 'theo.garnier@gymxyz.fr', phone: '06 55 32 87 41',
      plan: 'Carte 10 séances', planTone: 'neutral', gauge: 10, gaugeLabel: '1/10', presence: '40%', last: 'il y a 3 sem.',
      tone: 'danger', status: 'Inactif', since: 'sept. 2024' },
    { id: 'sarah', name: 'Sarah Cohen', initials: 'SC', email: 'sarah.cohen@gymxyz.fr', phone: '06 71 09 55 18',
      plan: 'Illimité mensuel', planTone: 'brand', gauge: 100, gaugeLabel: '∞', presence: '95%', last: 'il y a 1 j',
      tone: 'success', status: 'Actif', since: 'mai 2023' },
  ],

  /* ---------- fiche (Laetitia) ---------- */
  fiche: {
    payments: [
      { date: '1 juin 2026', label: 'Abonnement mensuel', amt: '49 €', tone: 'success', st: 'Payé' },
      { date: '1 mai 2026', label: 'Abonnement mensuel', amt: '49 €', tone: 'success', st: 'Payé' },
      { date: '1 avr. 2026', label: 'Abonnement mensuel', amt: '49 €', tone: 'success', st: 'Payé' },
    ],
    upcoming: [
      { name: 'HIIT Blast', when: 'lun. 9 juin · 09:00 · Studio A', tone: 'brand', st: 'Inscrite' },
      { name: 'Yoga Restore', when: 'jeu. 12 juin · 17:15 · Studio B', tone: 'brand', st: 'Inscrite' },
    ],
    recent: [
      { name: 'Power Cycle', when: 'ven. 6 juin · 18:00', tone: 'success', st: 'Présente' },
      { name: 'Core Express', when: 'mer. 4 juin · 12:30', tone: 'success', st: 'Présente' },
      { name: 'Pilates Core', when: 'lun. 2 juin · 18:00', tone: 'danger', st: 'Absente' },
    ],
  },

  /* ---------- coachs ---------- */
  coachs: [
    {
      id: 'nora', name: 'Nora Lemoine', role: 'Coach senior · co-fondatrice',
      disciplines: ['Pilates', 'Yoga', 'Mobilité', 'HIIT'],
      classesWeek: 9, fill: 88, rating: '4,9', members: 142,
      status: 'Disponible', tone: 'success', since: 'janv. 2022',
      email: 'nora.lemoine@gymxyz.fr', phone: '06 41 22 18 07',
      bio: "Pilier de la salle depuis l'ouverture. Nora alterne les cours doux du matin et les formats toniques du soir. Elle suit aussi les nouveaux membres sur leurs premières séances.",
      certs: ['BPJEPS AF — Cours collectifs', 'Pilates Mat · niveau 2', 'Yoga Alliance 200h'],
      avail: [1, 1, 1, 1, 1, 1, 0],
      sessions: [
        ['Lun', '07:15', 'Strength Foundations', 14, 20],
        ['Mar', '18:00', 'Pilates Core', 12, 16],
        ['Jeu', '17:15', 'Yoga Restore', 14, 20],
        ['Ven', '09:00', 'HIIT Blast', 12, 16],
        ['Ven', '18:00', 'Power Cycle', 24, 24],
      ],
    },
    {
      id: 'samir', name: 'Samir El Amrani', role: 'Coach renforcement',
      disciplines: ['Renforcement', 'Coaching perso', 'Core'],
      classesWeek: 7, fill: 81, rating: '4,8', members: 96,
      status: 'Disponible', tone: 'success', since: 'sept. 2022',
      email: 'samir.elamrani@gymxyz.fr', phone: '06 55 70 33 12',
      bio: 'Spécialiste du renforcement et du suivi individuel. Samir gère la majorité des coachings privés et les formats express du midi.',
      certs: ['BPJEPS AGFF — Haltérophilie & musculation', 'Préparation physique · FFHM'],
      avail: [1, 1, 1, 1, 1, 0, 0],
      sessions: [
        ['Lun', '12:30', 'Core Express', 16, 18],
        ['Mar', '20:00', 'Mobility Reset', 8, 14],
        ['Mer', '11:00', 'Coaching Perso', 1, 1],
        ['Jeu', '19:00', 'Strength Lab', 18, 22],
      ],
    },
    {
      id: 'theo', name: 'Théo Garnier', role: 'Coach boxe & cardio',
      disciplines: ['Boxe', 'Cardio', 'HIIT'],
      classesWeek: 5, fill: 74, rating: '4,7', members: 68,
      status: 'En congé', tone: 'neutral', away: "jusqu'au 15 juin", since: 'mars 2023',
      email: 'theo.garnier@gymxyz.fr', phone: '06 12 90 44 51',
      bio: 'Ancien compétiteur amateur, Théo anime les cours de boxe technique et les circuits cardio du soir. Très suivi par les habitués.',
      certs: ['BPJEPS — Boxe anglaise', 'PSC1 · premiers secours'],
      avail: [0, 0, 0, 0, 0, 1, 1],
      sessions: [
        ['Mer', '19:30', 'Boxing Fundamentals', 10, 16],
      ],
    },
    {
      id: 'lea', name: 'Léa Fontaine', role: 'Coach cycling',
      disciplines: ['Cycling', 'Cardio'],
      classesWeek: 6, fill: 93, rating: '4,9', members: 110,
      status: 'Cours pleins', tone: 'warning', since: 'oct. 2023',
      email: 'lea.fontaine@gymxyz.fr', phone: '06 88 21 67 02',
      bio: 'Ses séances de cycling affichent presque toujours complet. Léa entretient une liste d\'attente fidèle et propose des sessions supplémentaires le week-end.',
      certs: ['Indoor Cycling · Schwinn', 'BPJEPS AF — Cours collectifs'],
      avail: [1, 1, 1, 0, 1, 1, 1],
      sessions: [
        ['Lun', '18:30', 'Power Cycle', 24, 24],
        ['Mer', '12:30', 'Power Cycle', 22, 24],
        ['Sam', '10:00', 'Power Cycle', 23, 24],
      ],
    },
    {
      id: 'karim', name: 'Karim Bouaziz', role: 'Coach musculation',
      disciplines: ['Musculation', 'Cross-training'],
      classesWeek: 4, fill: 69, rating: '4,6', members: 54,
      status: 'Disponible', tone: 'success', since: 'févr. 2024',
      email: 'karim.bouaziz@gymxyz.fr', phone: '06 73 50 29 88',
      bio: 'Encadre la salle de musculation et les circuits cross-training. Karim accompagne les débutants sur la technique des mouvements de base.',
      certs: ['BPJEPS AGFF', 'Haltérophilie · initiateur'],
      avail: [1, 0, 1, 1, 1, 1, 0],
      sessions: [
        ['Lun', '17:00', 'Open Gym', 18, 30],
        ['Jeu', '18:00', 'Cross Circuit', 11, 16],
      ],
    },
    {
      id: 'ines', name: 'Inès Ravel', role: 'Coach yoga & mobilité',
      disciplines: ['Yoga', 'Mobilité', 'Pilates'],
      classesWeek: 5, fill: 84, rating: '4,8', members: 73,
      status: 'Disponible', tone: 'success', since: 'mai 2024',
      email: 'ines.ravel@gymxyz.fr', phone: '06 30 14 76 95',
      bio: 'Arrivée récemment, Inès a relancé les créneaux yoga du matin et les ateliers mobilité du week-end, déjà très appréciés.',
      certs: ['Yoga Alliance 300h', 'Mobilité fonctionnelle · FRC'],
      avail: [1, 1, 0, 1, 1, 1, 1],
      sessions: [
        ['Mar', '08:00', 'Yoga Restore', 16, 20],
        ['Jeu', '12:30', 'Mobility Reset', 11, 14],
        ['Dim', '10:30', 'Yoga Restore', 18, 20],
      ],
    },
  ],

  /* ---------- cours (catalogue de modèles) ---------- */
  cours: [
    {
      id: 'hiit', name: 'HIIT Blast', discipline: 'Cardio · HIIT', icon: 'trend',
      duration: '60 min', cap: 16, studio: 'Studio A', level: 'Tous niveaux',
      intensity: 'Élevée', price: 'Inclus', sessionsWeek: 4, fill: 84, regulars: 38,
      coachs: ['nora', 'theo'],
      desc: "Intervalles courts et intenses alternant cardio et renforcement. Format efficace en une heure, adaptable selon le niveau du groupe.",
      next: [['Ven', '09:00', 'Studio A', 12, 16], ['Sam', '09:00', 'Studio A', 5, 16]],
    },
    {
      id: 'cycle', name: 'Power Cycle', discipline: 'Cycling', icon: 'target',
      duration: '45 min', cap: 24, studio: 'Studio C', level: 'Intermédiaire',
      intensity: 'Élevée', price: 'Inclus', sessionsWeek: 5, fill: 96, regulars: 64,
      coachs: ['lea', 'nora'],
      desc: "Séance de vélo indoor rythmée par la musique. Le cours le plus demandé de la salle — liste d'attente fréquente.",
      next: [['Lun', '18:30', 'Studio C', 24, 24], ['Mer', '12:30', 'Studio C', 22, 24]],
    },
    {
      id: 'pilates', name: 'Pilates Core', discipline: 'Pilates', icon: 'target',
      duration: '50 min', cap: 16, studio: 'Studio A', level: 'Tous niveaux',
      intensity: 'Modérée', price: 'Inclus', sessionsWeek: 3, fill: 78, regulars: 29,
      coachs: ['nora', 'ines'],
      desc: 'Travail de gainage et de posture au sol. Renforce la sangle abdominale en profondeur, sans impact.',
      next: [['Mar', '18:00', 'Studio A', 12, 16]],
    },
    {
      id: 'yoga', name: 'Yoga Restore', discipline: 'Yoga', icon: 'sparkles',
      duration: '60 min', cap: 20, studio: 'Studio B', level: 'Tous niveaux',
      intensity: 'Douce', price: 'Inclus', sessionsWeek: 3, fill: 82, regulars: 34,
      coachs: ['ines', 'nora'],
      desc: 'Séance lente axée sur la respiration et les étirements. Idéale en fin de journée pour récupérer.',
      next: [['Jeu', '17:15', 'Studio B', 14, 20], ['Dim', '10:30', 'Studio B', 18, 20]],
    },
    {
      id: 'strength', name: 'Strength Foundations', discipline: 'Renforcement', icon: 'dumbbell',
      duration: '60 min', cap: 20, studio: 'Studio A', level: 'Débutant',
      intensity: 'Modérée', price: 'Inclus', sessionsWeek: 2, fill: 71, regulars: 22,
      coachs: ['nora', 'karim'],
      desc: 'Apprentissage des mouvements de base avec charges légères. Pose les fondations avant les formats plus exigeants.',
      next: [['Lun', '07:15', 'Studio A', 14, 20]],
    },
    {
      id: 'boxing', name: 'Boxing Fundamentals', discipline: 'Boxe', icon: 'target',
      duration: '60 min', cap: 16, studio: 'Studio C', level: 'Tous niveaux',
      intensity: 'Élevée', price: 'Inclus', sessionsWeek: 2, fill: 74, regulars: 19,
      coachs: ['theo'],
      desc: 'Technique de boxe anglaise, déplacements et travail au sac. Cardio complet, sans contact.',
      next: [['Mer', '19:30', 'Studio C', 10, 16]],
    },
    {
      id: 'core', name: 'Core Express', discipline: 'Renforcement', icon: 'dumbbell',
      duration: '30 min', cap: 18, studio: 'Studio B', level: 'Tous niveaux',
      intensity: 'Modérée', price: 'Inclus', sessionsWeek: 3, fill: 89, regulars: 31,
      coachs: ['samir'],
      desc: 'Format court et dense sur la pause déjeuner. Tout le travail de gainage en trente minutes.',
      next: [['Lun', '12:30', 'Studio B', 16, 18]],
    },
    {
      id: 'perso', name: 'Coaching Perso', discipline: 'Coaching individuel', icon: 'user',
      duration: '60 min', cap: 1, studio: 'Studio C', level: 'Sur-mesure',
      intensity: 'Privé', price: '45 € / séance', sessionsWeek: 6, fill: 100, regulars: 12,
      coachs: ['samir', 'karim'],
      desc: "Séance individuelle adaptée à l'objectif du membre. Réservable directement auprès du coach.",
      next: [['Mer', '11:00', 'Studio C', 1, 1]],
    },
  ],

  /* ---------- présences (pointage + assiduité) ---------- */
  presences: {
    date: 'Mardi 9 juin',
    kpis: { taux: '87 %', tauxDelta: '+4 pts ce mois', pointer: 3, presents: 52, noshow: 14 },
    sessions: [
      {
        id: 'p1', day: "Aujourd'hui", time: '09:00', course: 'HIIT Blast', icon: 'trend', itone: 'danger',
        coach: 'Nora Lemoine', studio: 'Studio A', cap: 16, status: 'todo',
        roster: [
          { name: 'Laetitia Moriceau', initials: 'LM', plan: 'Illimité', status: 'pending' },
          { name: 'Amina Benali', initials: 'AB', plan: 'Illimité', status: 'pending' },
          { name: 'Sarah Cohen', initials: 'SC', plan: 'Illimité', status: 'pending' },
          { name: 'Lucas Martin', initials: 'LM', plan: 'Carte 10', status: 'pending' },
          { name: 'Chloé Petit', initials: 'CP', plan: 'Illimité', status: 'pending' },
          { name: 'Maxime Roussel', initials: 'MR', plan: 'Carte 10', status: 'pending' },
          { name: 'Inès Faure', initials: 'IF', plan: 'Étudiant', status: 'pending' },
          { name: 'Yanis Moreau', initials: 'YM', plan: 'Illimité', status: 'pending' },
          { name: 'Manon Lefèvre', initials: 'ML', plan: 'Carte 10', status: 'pending' },
          { name: 'Hugo Bernard', initials: 'HB', plan: 'Illimité', status: 'pending' },
          { name: 'Léa Dubois', initials: 'LD', plan: 'Étudiant', status: 'pending' },
          { name: 'Adrien Fabre', initials: 'AF', plan: 'Illimité', status: 'pending' },
        ],
      },
      {
        id: 'p2', day: "Aujourd'hui", time: '12:30', course: 'Core Express', icon: 'dumbbell', itone: 'warning',
        coach: 'Samir El Amrani', studio: 'Studio B', cap: 18, status: 'todo',
        roster: [
          { name: 'Camille Durand', initials: 'CD', plan: 'Carte 10', status: 'pending' },
          { name: 'Nathan Girard', initials: 'NG', plan: 'Illimité', status: 'pending' },
          { name: 'Sofia Lopez', initials: 'SL', plan: 'Illimité', status: 'pending' },
          { name: 'Sarah Cohen', initials: 'SC', plan: 'Illimité', status: 'pending' },
          { name: 'Adrien Fabre', initials: 'AF', plan: 'Illimité', status: 'pending' },
          { name: 'Manon Lefèvre', initials: 'ML', plan: 'Carte 10', status: 'pending' },
          { name: 'Yanis Moreau', initials: 'YM', plan: 'Illimité', status: 'pending' },
          { name: 'Inès Faure', initials: 'IF', plan: 'Étudiant', status: 'pending' },
        ],
      },
      {
        id: 'p3', day: "Aujourd'hui", time: '18:30', course: 'Power Cycle', icon: 'target', itone: 'danger',
        coach: 'Léa Fontaine', studio: 'Studio C', cap: 24, status: 'live',
        roster: [
          { name: 'Amina Benali', initials: 'AB', plan: 'Illimité', status: 'present' },
          { name: 'Sarah Cohen', initials: 'SC', plan: 'Illimité', status: 'present' },
          { name: 'Hugo Bernard', initials: 'HB', plan: 'Illimité', status: 'present' },
          { name: 'Chloé Petit', initials: 'CP', plan: 'Illimité', status: 'present' },
          { name: 'Maxime Roussel', initials: 'MR', plan: 'Carte 10', status: 'late' },
          { name: 'Léa Dubois', initials: 'LD', plan: 'Étudiant', status: 'pending' },
          { name: 'Nathan Girard', initials: 'NG', plan: 'Illimité', status: 'pending' },
          { name: 'Lucas Martin', initials: 'LM', plan: 'Carte 10', status: 'pending' },
        ],
      },
      {
        id: 'p4', day: 'Hier', time: '18:00', course: 'Pilates Core', icon: 'target', itone: 'success',
        coach: 'Nora Lemoine', studio: 'Studio A', cap: 16, status: 'done',
        roster: [
          { name: 'Laetitia Moriceau', initials: 'LM', plan: 'Illimité', status: 'present' },
          { name: 'Camille Durand', initials: 'CD', plan: 'Carte 10', status: 'present' },
          { name: 'Sarah Cohen', initials: 'SC', plan: 'Illimité', status: 'present' },
          { name: 'Inès Faure', initials: 'IF', plan: 'Étudiant', status: 'late' },
          { name: 'Manon Lefèvre', initials: 'ML', plan: 'Carte 10', status: 'present' },
          { name: 'Théo Garnier', initials: 'TG', plan: 'Carte 10', status: 'absent' },
          { name: 'Adrien Fabre', initials: 'AF', plan: 'Illimité', status: 'present' },
          { name: 'Sofia Lopez', initials: 'SL', plan: 'Illimité', status: 'present' },
          { name: 'Yanis Moreau', initials: 'YM', plan: 'Illimité', status: 'present' },
          { name: 'Léa Dubois', initials: 'LD', plan: 'Étudiant', status: 'absent' },
        ],
      },
      {
        id: 'p5', day: 'Hier', time: '12:30', course: 'Core Express', icon: 'dumbbell', itone: 'warning',
        coach: 'Samir El Amrani', studio: 'Studio B', cap: 18, status: 'done',
        roster: [
          { name: 'Hugo Bernard', initials: 'HB', plan: 'Illimité', status: 'present' },
          { name: 'Chloé Petit', initials: 'CP', plan: 'Illimité', status: 'present' },
          { name: 'Nathan Girard', initials: 'NG', plan: 'Illimité', status: 'present' },
          { name: 'Maxime Roussel', initials: 'MR', plan: 'Carte 10', status: 'present' },
          { name: 'Camille Durand', initials: 'CD', plan: 'Carte 10', status: 'late' },
          { name: 'Lucas Martin', initials: 'LM', plan: 'Carte 10', status: 'present' },
          { name: 'Amina Benali', initials: 'AB', plan: 'Illimité', status: 'absent' },
        ],
      },
      {
        id: 'p6', day: 'Hier', time: '09:00', course: 'HIIT Blast', icon: 'trend', itone: 'danger',
        coach: 'Nora Lemoine', studio: 'Studio A', cap: 16, status: 'done',
        roster: [
          { name: 'Sarah Cohen', initials: 'SC', plan: 'Illimité', status: 'present' },
          { name: 'Amina Benali', initials: 'AB', plan: 'Illimité', status: 'present' },
          { name: 'Laetitia Moriceau', initials: 'LM', plan: 'Illimité', status: 'present' },
          { name: 'Yanis Moreau', initials: 'YM', plan: 'Illimité', status: 'present' },
          { name: 'Manon Lefèvre', initials: 'ML', plan: 'Carte 10', status: 'late' },
          { name: 'Adrien Fabre', initials: 'AF', plan: 'Illimité', status: 'present' },
          { name: 'Inès Faure', initials: 'IF', plan: 'Étudiant', status: 'absent' },
        ],
      },
    ],
    parCours: [
      { name: 'Power Cycle', pct: 96 },
      { name: 'Core Express', pct: 91 },
      { name: 'HIIT Blast', pct: 86 },
      { name: 'Yoga Restore', pct: 82 },
      { name: 'Pilates Core', pct: 78 },
      { name: 'Boxing Fundamentals', pct: 71 },
    ],
    absents: [
      { name: 'Théo Garnier', initials: 'TG', miss: 5, total: 6, last: 'il y a 3 sem.' },
      { name: 'Camille Durand', initials: 'CD', miss: 3, total: 8, last: 'il y a 1 sem.' },
      { name: 'Léa Dubois', initials: 'LD', miss: 3, total: 9, last: 'il y a 5 j' },
    ],
  },

  /* ---------- abonnements (formules + suivi + encaissements) ---------- */
  abos: {
    kpis: { mrr: '4 980 €', mrrDelta: '+6 % vs mai', active: 112, expiring: 6, late: 2, lateAmt: '180 €' },
    formules: [
      { id: 'illimite', name: 'Illimité mensuel', price: '49', unit: '€ / mois', members: 64, tone: 'brand', desc: 'Accès illimité à tous les cours collectifs.', billing: 'Sans engagement', featured: true },
      { id: 'carte10', name: 'Carte 10 séances', price: '120', unit: '€ / carte', members: 38, tone: 'neutral', desc: '10 entrées valables 4 mois.', billing: 'Paiement unique' },
      { id: 'etudiant', name: 'Étudiant mensuel', price: '35', unit: '€ / mois', members: 18, tone: 'success', desc: 'Tarif réduit sur justificatif de scolarité.', billing: 'Sans engagement' },
      { id: 'annuel', name: 'Illimité annuel', price: '490', unit: '€ / an', members: 12, tone: 'warning', desc: 'Deux mois offerts sur l’année.', billing: 'Engagement 12 mois' },
    ],
    subs: [
      { member: 'Laetitia Moriceau', initials: 'LM', formule: 'Illimité mensuel', renew: '30 juin', daysLeft: 18, pct: 62, amount: '49 € / mois', auto: true, status: 'Actif', tone: 'success' },
      { member: 'Sofia Lopez', initials: 'SL', formule: 'Illimité mensuel', renew: '12 juin', daysLeft: 3, pct: 12, amount: '49 € / mois', auto: true, status: 'Expire bientôt', tone: 'warning' },
      { member: 'Camille Durand', initials: 'CD', formule: 'Carte 10 séances', renew: '3 séances restantes', daysLeft: null, pct: 30, amount: '120 € / carte', auto: false, status: 'Expire bientôt', tone: 'warning' },
      { member: 'Théo Garnier', initials: 'TG', formule: 'Carte 10 séances', renew: 'Échue depuis 4 j', daysLeft: -4, pct: 0, amount: '120 € / carte', auto: false, status: 'En retard', tone: 'danger' },
      { member: 'Nathan Girard', initials: 'NG', formule: 'Illimité mensuel', renew: "Aujourd'hui", daysLeft: 0, pct: 4, amount: '49 € / mois', auto: true, status: 'Expire bientôt', tone: 'warning' },
      { member: 'Amina Benali', initials: 'AB', formule: 'Illimité annuel', renew: '14 févr. 2027', daysLeft: 250, pct: 68, amount: '490 € / an', auto: true, status: 'Actif', tone: 'success' },
      { member: 'Sarah Cohen', initials: 'SC', formule: 'Illimité mensuel', renew: '1 juil.', daysLeft: 22, pct: 74, amount: '49 € / mois', auto: true, status: 'Actif', tone: 'success' },
      { member: 'Lucas Martin', initials: 'LM', formule: 'Étudiant mensuel', renew: '28 juin', daysLeft: 16, pct: 55, amount: '35 € / mois', auto: true, status: 'Actif', tone: 'success' },
    ],
    encaissements: [
      { date: '9 juin', member: 'Amina Benali', label: 'Illimité mensuel', amount: '49 €', method: 'Prélèvement', tone: 'success', status: 'Encaissé' },
      { date: '8 juin', member: 'Sarah Cohen', label: 'Illimité mensuel', amount: '49 €', method: 'Carte', tone: 'success', status: 'Encaissé' },
      { date: '7 juin', member: 'Hugo Bernard', label: 'Carte 10 séances', amount: '120 €', method: 'Espèces', tone: 'success', status: 'Encaissé' },
      { date: '6 juin', member: 'Théo Garnier', label: 'Carte 10 séances', amount: '120 €', method: 'Prélèvement', tone: 'danger', status: 'Rejeté' },
      { date: '5 juin', member: 'Lucas Martin', label: 'Étudiant mensuel', amount: '35 €', method: 'Carte', tone: 'success', status: 'Encaissé' },
    ],
  },

  /* ---------- salles (studios) ---------- */
  salles: [
    {
      id: 'a', name: 'Studio A', type: 'Cours collectifs', icon: 'grid', tone: 'brand',
      cap: 20, area: '65 m²', floor: 'Rez-de-chaussée', occ: 78, sessionsWeek: 18,
      status: 'Disponible', statusTone: 'success',
      note: 'Grande salle polyvalente pour les formats collectifs — renforcement, HIIT, pilates.',
      equip: ['Tapis ×20', 'Steps', 'Haltères', 'Élastiques', 'Miroir mural', 'Sono'],
      today: [
        ['07:15', 'Strength Foundations', 'N. Lemoine', 14, 20],
        ['12:30', 'Core Express', 'S. El Amrani', 16, 18],
        ['18:00', 'Pilates Core', 'N. Lemoine', 12, 16],
      ],
      heat: [82, 74, 60, 88, 91, 70, 30],
    },
    {
      id: 'b', name: 'Studio B', type: 'Yoga & mobilité', icon: 'sparkles', tone: 'success',
      cap: 20, area: '48 m²', floor: '1ᵉʳ étage', occ: 66, sessionsWeek: 12,
      status: 'Disponible', statusTone: 'success',
      note: 'Ambiance calme, parquet et lumière tamisée — dédiée au yoga, au pilates doux et à la mobilité.',
      equip: ['Tapis ×20', 'Briques', 'Bolsters', 'Sangles', 'Parquet', 'Lumière tamisée'],
      today: [
        ['08:00', 'Yoga Restore', 'I. Ravel', 16, 20],
        ['17:15', 'Yoga Restore', 'N. Lemoine', 14, 20],
      ],
      heat: [54, 70, 48, 80, 62, 58, 72],
    },
    {
      id: 'c', name: 'Studio C', type: 'Cycling & cardio', icon: 'target', tone: 'danger',
      cap: 24, area: '55 m²', floor: 'Sous-sol', occ: 94, sessionsWeek: 22,
      status: 'Forte demande', statusTone: 'warning',
      note: 'Salle de cycling immersive — la plus demandée. Liste d’attente fréquente sur les créneaux du soir.',
      equip: ['24 vélos', 'Sono immersive', 'Écran LED', 'Ventilation', 'Éclairage scénique'],
      today: [
        ['11:00', 'Coaching Perso', 'S. El Amrani', 1, 1],
        ['18:30', 'Power Cycle', 'L. Fontaine', 24, 24],
      ],
      heat: [96, 88, 92, 84, 98, 90, 76],
    },
    {
      id: 'gym', name: 'Espace libre', type: 'Musculation & open gym', icon: 'dumbbell', tone: 'neutral',
      cap: 30, area: '120 m²', floor: 'Rez-de-chaussée', occ: 71, sessionsWeek: 6,
      status: 'Accès libre', statusTone: 'neutral',
      note: 'Plateau musculation en accès libre aux heures d’ouverture. Encadrement ponctuel sur les circuits.',
      equip: ['Rack squat ×2', 'Bancs', 'Haltères 2–40 kg', 'Poulies', 'Tapis de course ×4', 'Rameurs ×2'],
      today: [
        ['10:00', 'Open Gym', '—', 18, 30],
        ['17:00', 'Cross Circuit', 'K. Bouaziz', 11, 16],
      ],
      heat: [68, 72, 64, 70, 82, 88, 52],
    },
    {
      id: 'parc', name: "Parc de la Tête d'Or", type: 'Plein air · bootcamp', icon: 'tree', tone: 'success',
      kind: 'exterieur',
      cap: 20, sessionsWeek: 5,
      status: 'Beau temps', statusTone: 'success',
      address: 'Entrée Bd des Belges',
      weather: true, fallback: 'Studio A',
      lat: 45.78, lon: 4.85,
      note: "Cours en extérieur sur la grande pelouse dès les beaux jours — bootcamp, renforcement et cardio. Repli automatique en salle en cas de pluie ; les inscrits sont prévenus la veille.",
      equip: ['Matériel apporté', 'Tapis transportables', 'Kettlebells', 'Élastiques', 'Plots & cônes'],
      today: [
        ['08:30', 'Bootcamp matinal', 'K. Bouaziz', 14, 20],
        ['18:30', 'Cardio plein air', 'T. Garnier', 9, 20],
      ],
    },
    {
      id: 'domicile', name: 'À domicile', type: 'Chez le membre', icon: 'home', tone: 'neutral',
      kind: 'domicile',
      cap: 1, sessionsWeek: 6,
      status: 'Sur rendez-vous', statusTone: 'neutral',
      address: 'Chez le membre',
      note: "Séances individuelles au domicile du membre. L'adresse est renseignée sur la fiche du membre puis transmise au coach avant chaque rendez-vous — le coach apporte son matériel.",
      equip: ['Matériel apporté par le coach'],
      today: [
        ['10:00', 'Coaching à domicile', 'S. El Amrani', 1, 1],
        ['16:00', 'Remise en forme', 'N. Lemoine', 1, 1],
      ],
    },
  ],

  /* ---------- réglages (paramètres de la salle) ---------- */
  reglages: {
    identite: {
      baseline: 'Salle de sport & coaching',
      address: '14 rue de la Villette',
      zip: '69003',
      city: 'Lyon 3ᵉ',
      email: 'contact@gymxyz.fr',
      phone: '04 78 12 34 56',
      siret: '901 234 567 00018',
      capacity: '180',
      hours: [
        ['Lundi – vendredi', '06:30 – 22:00'],
        ['Samedi', '08:00 – 19:00'],
        ['Dimanche', '09:00 – 13:00'],
      ],
    },
    team: [
      { name: 'Nora Lemoine', initials: 'NL', role: 'Coach senior', roleTone: 'brand', access: 'Planning, cours & présences', last: "aujourd'hui" },
      { name: 'Samir El Amrani', initials: 'SE', role: 'Coach', roleTone: 'neutral', access: 'Planning & présences', last: 'il y a 2 h' },
      { name: 'Léa Fontaine', initials: 'LF', role: 'Coach', roleTone: 'neutral', access: 'Planning & présences', last: 'hier' },
      { name: 'Margaux Vidal', initials: 'MV', role: 'Accueil', roleTone: 'success', access: 'Membres & encaissements', last: 'il y a 3 j' },
    ],
    invites: [
      { email: 'theo.garnier@gymxyz.fr', role: 'Coach', sent: 'il y a 2 j' },
    ],
    membres: {
      total: 112, withAccount: 98, invited: 6,
      list: [
        { name: 'Laetitia Moriceau', initials: 'LM', email: 'laetitia.moriceau@gymxyz.fr', plan: 'Illimité mensuel', acc: 'Actif', accTone: 'success', last: "aujourd'hui" },
        { name: 'Amina Benali', initials: 'AB', email: 'amina.benali@gymxyz.fr', plan: 'Illimité mensuel', acc: 'Actif', accTone: 'success', last: 'il y a 2 h' },
        { name: 'Sarah Cohen', initials: 'SC', email: 'sarah.cohen@gymxyz.fr', plan: 'Illimité mensuel', acc: 'Actif', accTone: 'success', last: 'hier' },
        { name: 'Camille Durand', initials: 'CD', email: 'camille.durand@gymxyz.fr', plan: 'Carte 10 séances', acc: 'Invitation envoyée', accTone: 'warning', last: '—' },
        { name: 'Lucas Martin', initials: 'LM', email: 'lucas.martin@gymxyz.fr', plan: 'Carte 10 séances', acc: 'Actif', accTone: 'success', last: 'il y a 4 j' },
        { name: 'Théo Garnier', initials: 'TG', email: 'theo.garnier@gymxyz.fr', plan: 'Carte 10 séances', acc: 'Sans accès', accTone: 'neutral', last: 'jamais' },
      ],
    },
    paiements: {
      devise: 'Euro (€)',
      tva: 'TVA non applicable, art. 293 B du CGI',
      moyens: [
        ['Carte bancaire', true],
        ['Prélèvement SEPA', true],
        ['Espèces', true],
        ['Chèque', false],
        ['Lien de paiement', true],
      ],
    },
    notifs: [
      {
        group: 'Membres & abonnements',
        items: [
          { t: 'Relance avant échéance', d: 'Au membre, 7 jours avant la fin de son abonnement.', on: true, chan: ['Email', 'SMS'] },
          { t: 'Paiement en retard', d: 'À vous, dès qu’un prélèvement est rejeté.', on: true, chan: ['Email'] },
          { t: 'Nouvelle inscription', d: 'À vous, à chaque nouveau membre enregistré.', on: false, chan: ['Email'] },
        ],
      },
      {
        group: 'Cours & présences',
        items: [
          { t: 'Rappel de cours', d: 'Au membre, 2 heures avant un cours réservé.', on: true, chan: ['Email', 'SMS'] },
          { t: 'Place libérée', d: 'À la liste d’attente quand une place se libère.', on: true, chan: ['Email', 'SMS'] },
          { t: 'Annulation de cours', d: 'Aux inscrits si un cours est annulé.', on: true, chan: ['Email', 'SMS'] },
        ],
      },
    ],
    facturation: {
      plan: 'GymXYZ Pro',
      planDesc: 'Engagement annuel · sans frais de mise en service',
      price: '79 €',
      unit: '/ mois',
      renew: '1 janvier 2027',
      members: 112,
      membersCap: 'illimité',
      card: { brand: 'Visa', last: '4242', exp: '08 / 27' },
      factures: [
        { date: '1 janv. 2026', ref: 'GX-2026-001', amount: '948 €', status: 'Payée', tone: 'success' },
        { date: '1 janv. 2025', ref: 'GX-2025-001', amount: '948 €', status: 'Payée', tone: 'success' },
        { date: '1 janv. 2024', ref: 'GX-2024-001', amount: '790 €', status: 'Payée', tone: 'success' },
      ],
    },
  },
};
