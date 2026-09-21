/* Repositorios: uno por microservicio. Con USE_MOCK=true usan MockDB;
   con false llaman a la API C# .NET (que consultará SQL Server). */
class MockDB {
  static data = JSON.parse(localStorage.getItem('sigemac_mock2') || 'null') || {
    sesiones: [
      { id: 1, fecha: '2026-10-05', hora: '09:00', sede: 'Lima – I.E. Ricardo Palma', modalidad: 'Presencial', capacitador: '12345678', materialIds: [1] },
      { id: 2, fecha: '2026-10-07', hora: '15:00', sede: 'Arequipa – ODPE Arequipa', modalidad: 'Virtual', capacitador: '', materialIds: [] }
    ],
    miembros: [
      { dni: '72710878', nombre: 'Ana Torres Quispe', cargo: 'Presidente', local: 'I.E. Ricardo Palma', region: 'Lima', sesionId: 1, asistio: null },
      { dni: '45871236', nombre: 'Luis Ramos Díaz', cargo: 'Secretario', local: 'I.E. Ricardo Palma', region: 'Lima', sesionId: 1, asistio: null },
      { dni: '40125896', nombre: 'Rosa Huamán Cruz', cargo: 'Tercer miembro', local: 'ODPE Arequipa', region: 'Arequipa', sesionId: 2, asistio: null },
      { dni: '71234567', nombre: 'Carlos Vega Rojas', cargo: 'Suplente', local: 'I.E. Ricardo Palma', region: 'Lima', sesionId: null, asistio: null }
    ],
    materiales: [
      { id: 1, titulo: 'Manual del Miembro de Mesa', tipo: 'Manual', url: 'https://www.onpe.gob.pe', estado: 'Disponible' },
      { id: 2, titulo: 'Guía de instalación de la mesa', tipo: 'Guía', url: 'https://www.onpe.gob.pe', estado: 'Disponible' },
      { id: 3, titulo: 'Video: proceso de votación', tipo: 'Video', url: 'https://www.onpe.gob.pe', estado: 'Pendiente' }
    ],
    incidencias: []
  };
  static save() { localStorage.setItem('sigemac_mock2', JSON.stringify(this.data)); }
  static nextId(list) { return Math.max(0, ...list.map(x => x.id)) + 1; }
}

class Repository {
  constructor(service) { this.base = CONFIG.SERVICES[service]; }
  call(path, method, body, mock) {
    if (!CONFIG.USE_MOCK) return ApiClient.request(this.base + path, { method, body });
    return new Promise((ok, no) => setTimeout(() => { try { ok(mock()); } catch (e) { no(e); } }, 150));
  }
}

class AuthRepository extends Repository {
  constructor() { super('auth'); }
  login(rol, dni, password) {
    return this.call('/login', 'POST', { rol, dni, password }, () => ({ dni, rol, nombre: 'Usuario ' + dni, token: 'mock-jwt' }));
  }
  // Miembro de mesa: la API valida el DNI en el padrón (SQL Server), asigna la capacitación y devuelve el token. 404 = no figura.
  memberAccess(dni) {
    return this.call('/member-access', 'POST', { dni }, () => {
      const m = MockDB.data.miembros.find(x => x.dni === dni);
      if (!m) throw Object.assign(new Error('No encontrado'), { status: 404 });
      if (!m.sesionId) m.sesionId = MockDB.data.sesiones[0].id; // asignación automática (CUN-02)
      MockDB.save();
      return { dni, rol: 'miembro', nombre: m.nombre, token: 'mock-jwt' };
    });
  }
}

class SessionRepository extends Repository {
  constructor() { super('sessions'); }
  list() { return this.call('', 'GET', null, () => MockDB.data.sesiones).then(r => r.map(x => new Sesion(x))); }
  update(id, p) { return this.call('/' + id, 'PATCH', p, () => { Object.assign(MockDB.data.sesiones.find(s => s.id === +id), p); MockDB.save(); return { ok: true }; }); }
  create(s) {
    return this.call('', 'POST', s, () => {
      const n = { id: MockDB.nextId(MockDB.data.sesiones), capacitador: '', materialIds: [], ...s };
      MockDB.data.sesiones.push(n); MockDB.save(); return n;
    });
  }
}

class MemberRepository extends Repository {
  constructor() { super('members'); }
  byDni(dni) {
    return this.call('/' + dni, 'GET', null, () => {
      const m = MockDB.data.miembros.find(x => x.dni === dni);
      if (!m) throw Object.assign(new Error('No encontrado'), { status: 404 });
      return { ...m, sesion: MockDB.data.sesiones.find(s => s.id === m.sesionId) };
    });
  }
  list() { return this.call('', 'GET', null, () => MockDB.data.miembros); }
  create(m) {
    return this.call('', 'POST', m, () => {
      if (MockDB.data.miembros.some(x => x.dni === m.dni)) throw new Error('El DNI ya está registrado.');
      MockDB.data.miembros.push(m); MockDB.save(); return m;
    });
  }
  update(dni, p) { return this.call('/' + dni, 'PATCH', p, () => { Object.assign(MockDB.data.miembros.find(x => x.dni === dni), p); MockDB.save(); return { ok: true }; }); }
  bySession(id) { return this.call('?sesion=' + id, 'GET', null, () => MockDB.data.miembros.filter(m => String(m.sesionId) === String(id))); }
  assign(dni, sesionId) {
    return this.call(`/${dni}/assign`, 'POST', { sesionId: +sesionId }, () => {
      const m = MockDB.data.miembros.find(x => x.dni === dni);
      m.sesionId = +sesionId; m.asistio = null; MockDB.save(); return { ok: true };
    });
  }
}

class AttendanceRepository extends Repository {
  constructor() { super('attendance'); }
  save(sesionId, registros) {
    return this.call('/' + sesionId, 'POST', { registros }, () => {
      registros.forEach(r => { const m = MockDB.data.miembros.find(x => x.dni === r.dni); if (m) m.asistio = r.asistio; });
      MockDB.save(); return { ok: true };
    });
  }
}

class MaterialRepository extends Repository {
  constructor() { super('materials'); }
  list() { return this.call('', 'GET', null, () => MockDB.data.materiales); }
  update(id, p) { return this.call('/' + id, 'PATCH', p, () => { Object.assign(MockDB.data.materiales.find(m => m.id === +id), p); MockDB.save(); return { ok: true }; }); }
  create(m) {
    return this.call('', 'POST', m, () => {
      const n = { id: MockDB.nextId(MockDB.data.materiales), estado: 'Disponible', ...m };
      MockDB.data.materiales.push(n); MockDB.save(); return n;
    });
  }
}

class ReportRepository extends Repository {
  constructor() { super('reports'); }
  summary() {
    return this.call('/summary', 'GET', null, () => ({
      miembros: 4847 + MockDB.data.miembros.length,
      sesiones: 125 + MockDB.data.sesiones.length,
      cumplimiento: 76, capacitados: 3420, proceso: 890, pendientes: 540, asistencia: 88.4,
      regiones: [{ n: 'Lima', p: 87, pend: 13 }, { n: 'Arequipa', p: 72, pend: 28 }, { n: 'Cusco', p: 65, pend: 35 }, { n: 'La Libertad', p: 91, pend: 9 }, { n: 'Piura', p: 58, pend: 42 }, { n: 'Junín', p: 78, pend: 22 }],
      tendencia: [{ n: 'Jun', v: 82 }, { n: 'Jul', v: 88 }, { n: 'Ago', v: 75 }, { n: 'Sep', v: 92 }, { n: 'Oct', v: 85 }, { n: 'Nov', v: 94 }],
      stock: [{ n: 'Manuales', disp: 340, dist: 280, pend: 60 }, { n: 'Guías', disp: 520, dist: 410, pend: 110 }, { n: 'Kits', disp: 180, dist: 150, pend: 30 }, { n: 'Videos', disp: 95, dist: 95, pend: 0 }]
    }));
  }
}

class IncidentRepository extends Repository {
  constructor() { super('incidents'); }
  list() { return this.call('', 'GET', null, () => MockDB.data.incidencias); }
  create(i) {
    return this.call('', 'POST', i, () => {
      const n = { id: MockDB.nextId(MockDB.data.incidencias), ...i };
      MockDB.data.incidencias.push(n); MockDB.save(); return n;
    });
  }
}
