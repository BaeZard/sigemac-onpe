/* Capa de lógica de negocio (front): validaciones y reglas.
   La API C# .NET debe repetir estas reglas: el servidor es la fuente de verdad. */
class Validator {
  static dni(v) { return /^\d{8}$/.test(v); }
  static required(o, fields) {
    for (const f of fields) if (!String(o[f] || '').trim()) throw new Error(`El campo "${f}" es obligatorio.`);
  }
}

class AuthService {
  static repo = new AuthRepository();
  static fails = 0;
  static lockedUntil = 0;
  static checkLock() {
    const wait = Math.ceil((this.lockedUntil - Date.now()) / 1000);
    if (wait > 0) throw new Error(`Demasiados intentos. Espera ${wait} s.`);
  }
  static fail() { // anti fuerza bruta / scraping del padrón (la API aplica el Rate Limiting real)
    if (++this.fails >= CONFIG.MAX_ATTEMPTS) { this.lockedUntil = Date.now() + CONFIG.LOCK_MS; this.fails = 0; }
  }
  // Personal administrativo (coordinador, capacitador, asistente logístico)
  static async login(rol, dni, password) {
    this.checkLock();
    if (!CONFIG.STAFF.includes(rol)) throw new Error('Selecciona tu rol.');
    if (!Validator.dni(dni)) throw new Error('El DNI debe tener 8 dígitos numéricos.');
    if (password.length < 4) throw new Error('Contraseña inválida (mínimo 4 caracteres).');
    try { const u = await this.repo.login(rol, dni, password); SessionStore.save(u); this.fails = 0; return u; }
    catch (e) { this.fail(); throw e; }
  }
  // Miembro de mesa (CUS-01): valida DNI en la BD; si no figura, se deniega el acceso
  static async memberAccess(dni) {
    this.checkLock();
    if (!Validator.dni(dni)) throw new Error('El DNI debe tener 8 dígitos numéricos.');
    try { const u = await this.repo.memberAccess(dni); this.fails = 0; return u; }
    catch (e) {
      this.fail();
      throw e.status === 404 ? new Error('Acceso denegado: el DNI no figura como miembro de mesa.') : e;
    }
  }
}

class SessionService {
  static repo = new SessionRepository();
  static async setTrainer(id, dni) {
    if (!id) throw new Error('Selecciona una sesión.');
    if (!Validator.dni(dni)) throw new Error('El DNI del capacitador debe tener 8 dígitos.');
    return this.repo.update(id, { capacitador: dni });
  }
  static async addMaterial(id, matId) {
    if (!id || !matId) throw new Error('Selecciona sesión y material.');
    const s = (await this.list()).find(x => x.id === +id);
    if (s.materialIds.includes(+matId)) throw new Error('El material ya está asignado a la sesión.');
    return this.repo.update(id, { materialIds: [...s.materialIds, +matId] });
  }
  static list() { return this.repo.list(); }
  static create(d) {
    Validator.required(d, ['fecha', 'hora', 'sede', 'modalidad']);
    if (d.fecha < new Date().toISOString().slice(0, 10)) throw new Error('La fecha no puede ser anterior a hoy.');
    return this.repo.create(d);
  }
}

class MemberService {
  static repo = new MemberRepository();
  static list() { return this.repo.list(); }
  static create(d) {
    Validator.required(d, ['dni', 'nombre', 'cargo', 'local', 'region']);
    if (!Validator.dni(d.dni.trim())) throw new Error('El DNI debe tener 8 dígitos numéricos.');
    return this.repo.create({ ...d, dni: d.dni.trim(), sesionId: null, asistio: null });
  }
  static async update(d) {
    const dni = (d.dni || '').trim(); await this.consult(dni);
    const p = Object.fromEntries(['cargo', 'local', 'region'].filter(k => (d[k] || '').trim()).map(k => [k, d[k].trim()]));
    if (!Object.keys(p).length) throw new Error('Indica al menos un dato a actualizar.');
    return this.repo.update(dni, p);
  }
  static async consult(dni) {
    if (!Validator.dni(dni)) throw new Error('El DNI debe tener 8 dígitos numéricos.');
    try { return await this.repo.byDni(dni); }
    catch (e) { throw e.status === 404 ? new Error('El DNI no figura como miembro de mesa.') : e; }
  }
  static bySession(id) { return this.repo.bySession(id); }
  static async assign(dni, sesionId) {
    if (!sesionId) throw new Error('Selecciona una sesión.');
    const m = await this.consult(dni);
    if (String(m.sesionId) === String(sesionId)) throw new Error('El miembro ya está asignado a esta sesión.');
    return this.repo.assign(dni, sesionId);
  }
}

class AttendanceService {
  static repo = new AttendanceRepository();
  static async markDni(sesionId, dni) {
    if (!sesionId) throw new Error('Selecciona una sesión.');
    if (!Validator.dni(dni)) throw new Error('El DNI debe tener 8 dígitos numéricos.');
    if (!(await MemberService.bySession(sesionId)).some(m => m.dni === dni)) throw new Error('El DNI no está asignado a esta sesión.');
    return this.repo.save(sesionId, [{ dni, asistio: true }]);
  }
  static save(sesionId, registros) {
    if (!sesionId) throw new Error('Selecciona una sesión.');
    if (!registros.length) throw new Error('No hay participantes asignados a la sesión.');
    return this.repo.save(sesionId, registros);
  }
}

class MaterialService {
  static repo = new MaterialRepository();
  static setEstado(d) { Validator.required(d, ['id', 'estado']); return this.repo.update(d.id, { estado: d.estado }); }
  static list() { return this.repo.list(); }
  static create(d) {
    Validator.required(d, ['titulo', 'tipo', 'url']);
    if (!/^https?:\/\//i.test(d.url)) throw new Error('La URL debe iniciar con http:// o https://');
    return this.repo.create(d);
  }
}

class ReportService {
  static repo = new ReportRepository();
  static summary() { return this.repo.summary(); }
}

class IncidentService {
  static repo = new IncidentRepository();
  static list() { return this.repo.list(); }
  static create(d) {
    Validator.required(d, ['tipo', 'detalle']);
    return this.repo.create({ ...d, fecha: new Date().toISOString().slice(0, 10), estado: 'Abierta' });
  }
}

/* Reúne los datos que necesita el panel. La API debe filtrar por rol/token:
   un miembro de mesa solo recibe sus propios datos (Ley N° 29733). */
class DataService {
  static async bundle(u) {
    const m = u.rol === 'miembro';
    const [ses, mie, mat, inc, res, me] = await Promise.all([
      SessionService.list(), m ? [] : MemberService.list(), MaterialService.list(),
      IncidentService.list(), ReportService.summary(), m ? MemberService.consult(u.dni) : null
    ]);
    return { ses, mie, mat, inc, res, me, u };
  }
}
