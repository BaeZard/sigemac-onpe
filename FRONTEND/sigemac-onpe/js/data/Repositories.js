/* Repositorios: Comunicación directa con la API C# .NET y SQL Server */

class Repository {
  constructor(service) { this.base = CONFIG.SERVICES[service]; }
  call(path, method, body) {
    return ApiClient.request(this.base + path, { method, body });
  }
}

class AuthRepository extends Repository {
  constructor() { super('auth'); }
  login(rol, dni, password) {
    return this.call('/login', 'POST', { rol, dni, password });
  }
  memberAccess(dni) {
    return this.call('/member-access', 'POST', { dni });
  }
}

class SessionRepository extends Repository {
  constructor() { super('sessions'); }
  list() { 
    return this.call('', 'GET', null).then(r => r.map(x => new Sesion(x))); 
  }
  // Coincide con SesionCapacitacionController -> [HttpPost("Editar")]
  update(p) { 
    return this.call('/Editar', 'POST', p); 
  }
  // Coincide con SesionCapacitacionController -> [HttpPost("Nuevo")]
  create(s) {
    return this.call('/Nuevo', 'POST', s);
  }
  // Coincide con SesionCapacitacionController -> [HttpDelete("Eliminar/{id:int}")]
  delete(id) {
    return this.call('/Eliminar/' + id, 'DELETE', null);
  }
}

class MemberRepository extends Repository {
  constructor() { super('members'); }
  byDni(dni) {
    return this.call('/' + dni, 'GET', null);
  }
  // Coincide con MiembroMesaController -> [Route("a")]
  list() { 
    return this.call('/a', 'GET', null); 
  }
  // Coincide con MiembroMesaController -> [Route("Nuevo")]
  create(m) {
    return this.call('/Nuevo', 'POST', m);
  }
  // Coincide con MiembroMesaController -> [Route("Editar")]
  update(p) { 
    return this.call('/Editar', 'POST', p); 
  }
  // Coincide con MiembroMesaController -> [Route("Eliminar/{dni}")]
  delete(dni) {
    return this.call('/Eliminar/' + dni, 'DELETE', null);
  }
  bySession(id) { 
    return this.call('?sesion=' + id, 'GET', null); 
  }
  assign(dni, sesionId) {
    return this.call(`/${dni}/assign`, 'POST', { sesionId: +sesionId });
  }
}

class AttendanceRepository extends Repository {
  constructor() { super('attendance'); }
  save(sesionId, registros) {
    return this.call('/' + sesionId, 'POST', { registros });
  }
}

class MaterialRepository extends Repository {
  constructor() { super('materials'); }
  list() { 
    return this.call('', 'GET', null); 
  }
  // Coincide con MaterialCapacitacionController -> [HttpPost("Editar")]
  update(p) { 
    return this.call('/Editar', 'POST', p); 
  }
  // Coincide con MaterialCapacitacionController -> [HttpPost("Nuevo")]
  create(m) {
    return this.call('/Nuevo', 'POST', m);
  }
  // Coincide con MaterialCapacitacionController -> [HttpDelete("Eliminar/{id:int}")]
  delete(id) {
    return this.call('/Eliminar/' + id, 'DELETE', null);
  }
}

class ReportRepository extends Repository {
  constructor() { super('reports'); }
  summary() {
    return this.call('/summary', 'GET', null);
  }
}

class IncidentRepository extends Repository {
  constructor() { super('incidents'); }
  list() { 
    return this.call('', 'GET', null); 
  }
  create(i) {
    return this.call('', 'POST', i);
  }
}