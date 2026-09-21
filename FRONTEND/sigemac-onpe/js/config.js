/* Configuración central.
   Client-Server: el frontend solo habla HTTP/JSON con la API (nunca con la BD).
   Microservicios: cada dominio tiene su propia URL base; hoy apuntan al mismo host. */
const CONFIG = {
  USE_MOCK: true, // ← poner en false cuando la API C# .NET esté lista
  SERVICES: {
    auth: 'https://localhost:5001/api/auth',
    sessions: 'https://localhost:5001/api/sessions',
    members: 'https://localhost:5001/api/members',
    attendance: 'https://localhost:5001/api/attendance',
    materials: 'https://localhost:5001/api/materials',
    reports: 'https://localhost:5001/api/reports',
    incidents: 'https://localhost:5001/api/incidents'
  },
  ROLES: {
    coordinador: { label: 'Coordinador de Capacitación', icon: '👔' },
    capacitador: { label: 'Capacitador', icon: '🧑‍🏫' },
    logistico: { label: 'Asistente Logístico', icon: '📦' },
    miembro: { label: 'Miembro de Mesa', icon: '🗳️' }
  },
  STAFF: ['coordinador', 'capacitador', 'logistico'], // acceso por pestaña "Personal ONPE"
  MAX_ATTEMPTS: 5,   // control de intentos en cliente (el Rate Limiting real va en la API)
  LOCK_MS: 30000
};
