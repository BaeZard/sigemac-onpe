/* MVC · Controlador del panel: menú por rol, navegación, pestañas, formularios y exportación */
class AppController {
  // [ruta, icono, etiqueta, [[subopción, índice de pestaña]]]
  static MENU = {
    coordinador: [['inicio', '🏠', 'Inicio / Dashboard'],
      ['miembros', '👥', 'Miembros de Mesa', [['Registrar miembro', 0], ['Consultar miembros', 1], ['Actualizar información', 2], ['Historial de capacitación', 3]]],
      ['capacitaciones', '📅', 'Capacitaciones', [['Planificar capacitación', 0], ['Crear sesiones', 0], ['Asignar miembros', 1], ['Asignar capacitadores', 2], ['Consultar programación', 3]]],
      ['materiales', '📚', 'Materiales', [['Gestionar materiales', 0], ['Ver disponibilidad', 1], ['Actualizar manuales', 1]]],
      ['seguimiento', '📊', 'Seguimiento', [['Progreso', 0], ['Asistencia', 1], ['Cumplimiento', 2], ['Pendientes', 3]]],
      ['reportes', '📑', 'Reportes', [['Reporte de asistencia', 0], ['Reporte de cumplimiento', 1], ['Exportar Excel/PDF', 0]]],
      ['usuarios', '👤', 'Usuarios y roles'], ['config', '⚙️', 'Configuración']],
    capacitador: [['inicio', '🏠', 'Inicio'],
      ['mis', '📅', 'Mis capacitaciones', [['Ver sesiones asignadas', 0], ['Lista de miembros', 1]]],
      ['asistencia', '📝', 'Asistencia', [['Registrar asistencia', 0], ['Buscar por DNI', 1], ['Ver asistentes/ausentes', 2]]],
      ['materiales', '📚', 'Materiales', [['Manuales', 0], ['Guías', 1], ['Videos', 2]]],
      ['participantes', '👥', 'Participantes'], ['seguimiento', '📊', 'Mi seguimiento'], ['notif', '🔔', 'Notificaciones'], ['perfil', '👤', 'Mi perfil']],
    logistico: [['inicio', '🏠', 'Inicio'],
      ['materiales', '📚', 'Materiales', [['Registrar / consultar', 0], ['Actualizar / disponibilidad', 1]]],
      ['distribucion', '📦', 'Distribución', [['Asignar material', 0], ['Material por sesión', 1], ['Material por capacitador', 2], ['Estado de distribución', 3]]],
      ['sesiones', '📅', 'Sesiones'],
      ['alertas', '⚠️', 'Alertas / Incidencias', [['Material faltante / pendiente', 0], ['Registrar incidencia', 1]]],
      ['reportes', '📊', 'Reportes', [['Inventario', 0], ['Distribución', 1]]], ['perfil', '👤', 'Mi perfil']],
    miembro: [['inicio', '🏠', 'Inicio'], ['mis', '📅', 'Mis capacitaciones'],
      ['materiales', '📚', 'Materiales de capacitación', [['Manuales PDF', 0], ['Guías', 1], ['Videos', 2]]],
      ['progreso', '📈', 'Mi progreso'], ['historial', '📜', 'Historial'], ['notif', '🔔', 'Notificaciones'], ['perfil', '👤', 'Mi perfil']]
  };

  init() {
    this.user = SessionStore.get();
    if (!this.user) { location.replace('index.html'); return; } // la seguridad real la aplica la API (JWT)
    this.menu = AppController.MENU[this.user.rol];
    const rol = CONFIG.ROLES[this.user.rol], nav = document.getElementById('nav');
    nav.innerHTML = this.menu.map(([k, i, t, subs = []]) =>
      `<a class="nv" data-r="${k}"><span class="ic">${i}</span><span class="lbl">${t}</span>${subs.length ? '<i>▸</i>' : ''}</a>` +
      (subs.length ? `<div class="sub" data-s="${k}">${subs.map(([l, ti]) => `<a data-r="${k}" data-tab="${ti}">${l}</a>`).join('')}</div>` : '')).join('');
    nav.addEventListener('click', e => {
      const a = e.target.closest('a'); if (!a) return;
      this.go(a.dataset.r, a.dataset.tab === undefined ? undefined : +a.dataset.tab);
    });
    document.getElementById('pill').textContent = `${rol.icon} ${rol.label}`;
    document.getElementById('whoName').textContent = rol.label;
    document.getElementById('toggle').onclick = () => document.getElementById('app').classList.toggle('collapsed');
    document.getElementById('logout').onclick = () => { SessionStore.clear(); location.href = 'index.html'; };
    document.addEventListener('submit', e => this.submit(e));
    document.addEventListener('change', e => this.change(e));
    document.addEventListener('click', e => this.click(e));
    this.go(this.menu[0][0]);
  }

  hl() { // resalta la subopción activa
    document.querySelectorAll('.sub a').forEach(a => a.classList.toggle('on', a.dataset.r === this.route && +a.dataset.tab === Views.tab));
  }

  async go(r, tab) {
    if (r !== this.route) Views.tab = 0;
    if (tab !== undefined) Views.tab = tab;
    this.route = r;
    const titulo = this.menu.find(m => m[0] === r)[2];
    document.querySelectorAll('.nv[data-r]').forEach(a => a.classList.toggle('on', a.dataset.r === r));
    document.querySelectorAll('.sub').forEach(x => x.classList.toggle('open', x.dataset.s === r));
    document.getElementById('crumb').textContent = titulo;
    const c = document.getElementById('content');
    try {
      this.d = await DataService.bundle(this.user);
      c.innerHTML = (r === 'inicio' ? '' : `<h2>${esc(titulo)}</h2>`) + Views.page(r, this.d);
    } catch (e) { c.innerHTML = `<p class="error">${esc(e.message)}</p>`; }
    this.hl();
  }

  click(e) {
    const t = e.target.closest('[data-t]');
    if (t) { // cambio de pestaña interna
      Views.tab = +t.dataset.t;
      document.querySelectorAll('.tabs button').forEach(b => b.classList.toggle('on', b === t));
      document.querySelectorAll('.tp').forEach(p => p.hidden = +p.dataset.p !== Views.tab);
      this.hl();
    }
    const x = e.target.closest('[data-export]'); if (x) this.exportar(x.dataset.export);
    if (e.target.closest('[data-print]')) window.print();
  }

  async submit(e) {
    const f = e.target.closest('form[data-form]'); if (!f) return;
    e.preventDefault();
    const d = Object.fromEntries(new FormData(f)), dni = (d.dni || '').trim();
    const acciones = {
      sesion: () => SessionService.create(d),
      asignar: () => MemberService.assign(dni, d.sesion),
      asigCap: () => SessionService.setTrainer(d.sesion, dni),
      miembro: () => MemberService.create(d),
      miembroUpd: () => MemberService.update(d),
      material: () => MaterialService.create(d),
      matEstado: () => MaterialService.setEstado(d),
      distrib: () => SessionService.addMaterial(d.sesion, d.material),
      incidencia: () => IncidentService.create(d),
      asisDni: () => AttendanceService.markDni(d.sesion, dni),
      asistencia: () => AttendanceService.save(d.sesion,
        [...f.querySelectorAll('[data-dni]')].map(i => ({ dni: i.dataset.dni, asistio: i.checked })))
    };
    try {
      await acciones[f.dataset.form]();
      this.toast('Guardado correctamente');
      if (f.dataset.form !== 'asistencia') await this.go(this.route);
    } catch (x) { this.toast(x.message, true); }
  }

  async change(e) { // asistencia: cargar participantes de la sesión elegida
    if (e.target.id !== 'selSesion') return;
    document.getElementById('lista').innerHTML = e.target.value ? Views.lista(await MemberService.bySession(e.target.value)) : '';
  }

  exportar(k) { // Excel = CSV (UTF-8); PDF = imprimir/guardar como PDF
    const { mie, ses, mat, res } = this.d, s = id => ses.find(x => x.id === id);
    const t = {
      asistencia: [['DNI', 'Nombre', 'Sesión', 'Estado'], ...mie.map(m => [m.dni, m.nombre, s(m.sesionId)?.etiqueta || 'Sin sesión', Views.estado(m)])],
      cumplimiento: [['Región', '% cumplimiento'], ['Nacional', res.cumplimiento], ...res.regiones.map(r => [r.n, r.p])],
      inventario: [['Material', 'Tipo', 'Disponibilidad'], ...mat.map(m => [m.titulo, m.tipo, m.estado])],
      distribucion: [['Sesión', 'Materiales'], ...ses.map(x => [x.etiqueta, x.materialIds.map(id => mat.find(m => m.id === id)?.titulo).join(' | ')])]
    }[k];
    const cel = c => { c = String(c ?? ''); if (/^[=+\-@]/.test(c)) c = "'" + c; return `"${c.replace(/"/g, '""')}"`; };
    const a = Object.assign(document.createElement('a'), {
      href: URL.createObjectURL(new Blob(['\ufeff' + t.map(r => r.map(cel).join(',')).join('\n')], { type: 'text/csv;charset=utf-8' })),
      download: `reporte-${k}.csv`
    });
    a.click(); URL.revokeObjectURL(a.href);
  }

  toast(msg, err = false) {
    const t = document.getElementById('toast');
    t.textContent = msg; t.className = 'show' + (err ? ' err' : '');
    setTimeout(() => t.className = '', 3000);
  }
}
document.addEventListener('DOMContentLoaded', () => new AppController().init());
