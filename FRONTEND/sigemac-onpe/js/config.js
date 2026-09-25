const CONFIG = {
  USE_MOCK: false, // ¡Conectado a tu backend real!
  SERVICES: {
    auth: 'http://localhost:5130/api/Auth',
    sessions: 'http://localhost:5130/api/SesionCapacitacion',
    members: 'http://localhost:5130/api/MiembroMesa',
    attendance: 'http://localhost:5130/api/Asistencia',
    materials: 'http://localhost:5130/api/MaterialCapacitacion',
    reports: 'http://localhost:5130/api/Reports',
    incidents: 'http://localhost:5130/api/Incidents'
  },
  ROLES: {
    coordinador: { label: 'Coordinador de Capacitación', icon: '👔' },
    capacitador: { label: 'Capacitador', icon: '🧑‍🏫' },
    logistico: { label: 'Asistente Logístico', icon: '📦' },
    miembro: { label: 'Miembro de Mesa', icon: '🗳️' }
  },
  STAFF: ['coordinador', 'capacitador', 'logistico'],
  MAX_ATTEMPTS: 5,
  LOCK_MS: 30000
};