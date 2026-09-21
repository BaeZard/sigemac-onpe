/* Capa de acceso a datos · cliente HTTP (Fetch API + JWT, API stateless) */
class SessionStore {
  static save(u) { sessionStorage.setItem('user', JSON.stringify(u)); }
  static get() { const u = sessionStorage.getItem('user'); return u ? new Usuario(JSON.parse(u)) : null; }
  static clear() { sessionStorage.removeItem('user'); }
}

class ApiClient {
  static async request(url, { method = 'GET', body } = {}) {
    const u = SessionStore.get();
    const res = await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json', ...(u && { Authorization: `Bearer ${u.token}` }) },
      body: body && JSON.stringify(body)
    });
    if (!res.ok) {
      const msg = { 401: 'Credenciales incorrectas.', 403: 'No tienes permisos.', 404: 'No se encontró el registro.' }[res.status] || 'Error del servidor. Intenta nuevamente.';
      throw Object.assign(new Error(msg), { status: res.status });
    }
    return res.json();
  }
}
