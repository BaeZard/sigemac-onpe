/* MVC · Vistas: generan HTML a partir de los datos (sin lógica de negocio) */
const esc = s => String(s ?? '').replace(/[&<>"']/g, c => `&#${c.charCodeAt(0)};`);
const raw = h => ({ raw: h });
const hoy = () => new Date().toISOString().slice(0, 10);
const MES = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];

class Views {
  static tab = 0;

  /* ---------- componentes ---------- */
  static opt(o) { return Array.isArray(o) ? `<option value="${esc(o[0])}">${esc(o[1])}</option>` : `<option>${esc(o)}</option>`; }
  static sel(ss) { return [['', '— Selecciona —'], ...ss.map(s => [s.id, s.etiqueta])]; }
  static estado(m) { return m.asistio === true ? 'Asistió' : m.asistio === false ? 'No asistió' : 'Pendiente'; }
  static tag(t) {
    const c = { 'Asistió': 'g', Completo: 'g', Disponible: 'g', Presencial: 'g', 'No asistió': 'r', Agotado: 'r', 'Sin material': 'r', Pendiente: 'y', Virtual: 'b' }[t] || '';
    return raw(`<span class="tag ${c}">${esc(t)}</span>`);
  }
  static link(u) { return raw(`<a href="${esc(u)}" target="_blank" rel="noopener">Abrir</a>`); }
  static panel(t, h) { return `<div class="panel">${t ? `<h3>${t}</h3>` : ''}${h}</div>`; }
  static kpis(l) { return `<div class="grid">${l.map(([n, t]) => `<div class="panel kpi"><b>${esc(n)}</b><span>${t}</span></div>`).join('')}</div>`; }
  static bars(l) { return l.map(([n, p]) => `<p class="muted">${esc(n)} · ${+p}%</p><div class="bar"><i style="width:${+p}%"></i></div>`).join(''); }
  static table(h, rows) {
    const c = x => x && x.raw !== undefined ? x.raw : esc(x);
    return rows.length ? `<div style="overflow-x:auto"><table><tr>${h.map(x => `<th>${x}</th>`).join('')}</tr>${rows.map(r => `<tr>${r.map(x => `<td>${c(x)}</td>`).join('')}</tr>`).join('')}</table></div>` : '<p class="muted">Sin registros.</p>';
  }
  static form(name, fields, btn) {
    return `<form class="form" data-form="${name}">${fields.map(([n, l, t = 'text']) => `<div><label>${l}</label>${Array.isArray(t) ? `<select name="${n}">${t.map(this.opt).join('')}</select>` : `<input name="${n}" type="${t}">`}</div>`).join('')}<button class="btn">${btn}</button></form>`;
  }
  static tabs(o) {
    const k = Object.keys(o);
    return `<div class="tabs">${k.map((t, i) => `<button type="button" data-t="${i}" class="${i === this.tab ? 'on' : ''}">${t}</button>`).join('')}</div>` +
      k.map((t, i) => `<div class="tp" data-p="${i}" ${i === this.tab ? '' : 'hidden'}>${o[t]}</div>`).join('');
  }
  static exportar(kind) {
    return `<p style="margin-top:1rem"><button type="button" class="btn sec" style="width:auto;margin-right:.5rem" data-export="${kind}">⬇ Exportar Excel (CSV)</button><button type="button" class="btn sec" style="width:auto" data-print>🖨 Exportar PDF</button></p>`;
  }

  /* ---------- pantalla pública (consulta por DNI) ---------- */
  static miembro(m) {
    const s = m.sesion;
    return `<div class="panel"><h3>${esc(m.nombre)}</h3>
      <p class="muted">${esc(m.cargo)} · ${esc(m.local)}</p>
      <p style="margin:.8rem 0">${s ? `Capacitación: <b>${esc(s.fecha)} ${esc(s.hora)}</b> (${esc(s.modalidad)}) — ${esc(s.sede)}` : 'Aún sin sesión asignada.'}</p>
      <span class="tag">${this.estado(m)}</span></div>`;
  }

  static lista(ms) {
    return `<table style="margin-top:1rem"><tr><th>DNI</th><th>Nombre</th><th>Asistió</th></tr>
      ${ms.map(m => `<tr><td>${esc(m.dni)}</td><td>${esc(m.nombre)}</td><td><input type="checkbox" data-dni="${esc(m.dni)}" ${m.asistio ? 'checked' : ''}></td></tr>`).join('')}</table>`;
  }

  static asistencia(ss) {
    return this.panel('Registrar asistencia', `<form data-form="asistencia"><label>Sesión</label>
      <select name="sesion" id="selSesion">${this.sel(ss).map(this.opt).join('')}</select><div id="lista"></div>
      <button class="btn" style="width:auto;margin-top:1rem">Guardar asistencia</button></form>`);
  }

  static mats(mat, edit) {
    const lista = this.panel('', this.table(['Título', 'Tipo', 'Disponibilidad', 'Enlace'], mat.map(m => [m.titulo, this.tag(m.tipo), this.tag(m.estado), this.link(m.url)])));
    return edit ? this.tabs({
      'Gestionar': this.panel('Registrar material', this.form('material', [['titulo', 'Título'], ['tipo', 'Tipo', ['Manual', 'Guía', 'Video']], ['url', 'URL', 'url']], 'Guardar material')) + lista,
      'Disponibilidad / actualizar': this.panel('Actualizar material', this.form('matEstado', [['id', 'Material', mat.map(m => [m.id, m.titulo])], ['estado', 'Disponibilidad', ['Disponible', 'Pendiente', 'Agotado']]], 'Actualizar')) + lista
    }) : this.matsTipo(mat);
  }

  /* ---------- estilo del prototipo ---------- */
  static stat(t, v, sub, c, i) { return `<div class="stat"><div class="ico" style="background:${c}22">${i}</div><div><b>${esc(v)}</b><span>${t}</span><small style="color:${c}">${sub}</small></div></div>`; }
  static stats(l) { return `<div class="stats">${l.map(x => this.stat(...x)).join('')}</div>`; }
  static head(t, s) { return `<div class="head"><h1>${t}</h1><p>${esc(s)}</p></div>`; }
  static row2(a, b) { return `<div class="row2">${a}${b}</div>`; }
  static sesItem(s, n) {
    const [, mo, dd] = s.fecha.split('-');
    return `<div class="sitem"><div class="dbox"><b>${dd}</b><small>${MES[+mo - 1]}</small></div><div><div class="t">${esc(s.sede)}</div><small>${esc(s.hora)} hrs${n != null ? ` · ${n} pers.` : ''}</small></div>${this.tag(s.modalidad).raw}</div>`;
  }
  static person(m) {
    return `<div class="sitem"><div class="av">${esc((m.nombre || '?')[0])}</div><div><div class="t">${esc(m.nombre)}</div><small>DNI: ${esc(m.dni)}</small></div>${this.tag(this.estado(m)).raw}</div>`;
  }
  static matsTipo(mat) {
    const it = m => `<div class="sitem"><span>${m.tipo === 'Video' ? '▶️' : m.tipo === 'Guía' ? '📋' : '📄'}</span><div><div class="t">${esc(m.titulo)}</div><small>${esc(m.estado)}</small></div><a class="btnsm" href="${esc(m.url)}" target="_blank" rel="noopener">${m.tipo === 'Video' ? 'Ver' : 'Descargar'}</a></div>`;
    const l = t => mat.filter(m => m.tipo === t).map(it).join('') || '<p class="muted">Sin material disponible.</p>';
    return this.tabs({ 'Manuales': l('Manual'), 'Guías': l('Guía'), 'Videos': l('Video') });
  }

  /* ---------- enrutador por rol ---------- */
  static page(r, d) {
    if (r === 'notif') return this.notif(d);
    if (r === 'perfil') return this.perfil(d);
    return this[{ coordinador: 'coord', capacitador: 'cap', logistico: 'log', miembro: 'mem' }[d.u.rol]](r, d);
  }

  static notif(d) {
    const l = d.u.rol === 'miembro' ? (d.me.sesion ? [d.me.sesion] : []) : d.u.rol === 'capacitador' ? d.ses.filter(s => s.capacitador === d.u.dni) : d.ses;
    const n = l.filter(s => s.fecha >= hoy()).map(s => `Recordatorio: sesión el ${s.fecha} a las ${s.hora} — ${s.sede}`);
    return this.panel('Recordatorios', n.length ? `<ul>${n.map(x => `<li>${esc(x)}</li>`).join('')}</ul>` : '<p class="muted">No tienes notificaciones.</p>');
  }

  static perfil(d) {
    return this.panel('Mi perfil', this.table(['Campo', 'Dato'], [['Nombre', d.u.nombre], ['DNI', d.u.dni], ['Rol', CONFIG.ROLES[d.u.rol].label]]));
  }

  /* ---------- Coordinador ---------- */
  static coord(r, d) {
    const { ses, mie, mat, res } = d, selSes = this.sel(ses), h = hoy();
    const pres = mie.filter(m => m.asistio === true).length, marc = mie.filter(m => m.asistio !== null).length;
    const pend = mie.filter(m => m.asistio === null), reg = res.regiones.map(x => [x.n, x.p]);
    const sesDe = m => ses.find(s => s.id === m.sesionId)?.etiqueta || 'Sin sesión';
    const filas = ms => ms.map(m => [m.dni, m.nombre, sesDe(m), this.tag(this.estado(m))]);
    const cumpl = ses.map(s => { const a = mie.filter(m => m.sesionId === s.id); return [s.sede, a.length ? Math.round(a.filter(m => m.asistio).length * 100 / a.length) : 0]; });
    return {
      inicio: () => this.head('Panel del Coordinador', `Proceso Electoral 2026 · Actualizado: ${h.split('-').reverse().join('/')}`) +
        this.stats([['Miembros capacitados', res.capacitados.toLocaleString('es-PE'), 'Acumulado del proceso', GREEN, '✅'], ['Sesiones programadas', res.sesiones, `${ses.filter(s => s.fecha >= h).length} próximas`, RED, '📅'], ['Asistencia promedio', res.asistencia + '%', 'Meta: 90%', GOLD, '📊'], ['Miembros pendientes', res.pendientes, 'Requieren capacitación', INDIGO, '⏳']]) +
        this.row2(this.panel('% Capacitados por ODPE / Región', Charts.bars(res.regiones.map(x => ({ n: x.n, cap: x.p, pend: x.pend })), ['cap', 'pend'], [GREEN, RED]) + Charts.legend([['Capacitados', GREEN], ['Pendientes', RED]])),
          this.panel('Estado general', Charts.donut([['Capacitados', res.capacitados, GREEN], ['En proceso', res.proceso, GOLD], ['Pendientes', res.pendientes, RED]]))) +
        this.row2(this.panel('Tendencia de asistencia mensual (%)', Charts.line(res.tendencia)),
          this.panel('Próximas sesiones', ses.filter(s => s.fecha >= h).sort((a, b) => a.fecha.localeCompare(b.fecha)).slice(0, 4).map(s => this.sesItem(s, mie.filter(m => m.sesionId === s.id).length)).join('') || '<p class="muted">Sin sesiones próximas.</p>')),
      miembros: () => this.tabs({
        'Registrar': this.panel('Registrar miembro', this.form('miembro', [['dni', 'DNI'], ['nombre', 'Nombre completo'], ['cargo', 'Cargo', ['Presidente', 'Secretario', 'Tercer miembro', 'Suplente']], ['local', 'Local de votación'], ['region', 'Región / ODPE']], 'Registrar')),
        'Consultar': this.panel('', this.table(['DNI', 'Nombre', 'Cargo', 'Local', 'Región', 'Estado'], mie.map(m => [m.dni, m.nombre, m.cargo, m.local, m.region, this.tag(this.estado(m))]))),
        'Actualizar': this.panel('Actualizar información', this.form('miembroUpd', [['dni', 'DNI del miembro'], ['cargo', 'Nuevo cargo', [['', '— sin cambio —'], 'Presidente', 'Secretario', 'Tercer miembro', 'Suplente']], ['local', 'Nuevo local'], ['region', 'Nueva región']], 'Actualizar')),
        'Historial': this.panel('Historial de capacitación', this.table(['DNI', 'Nombre', 'Sesión', 'Estado'], filas(mie)))
      }),
      capacitaciones: () => this.tabs({
        'Planificar / crear sesión': this.panel('Nueva sesión', this.form('sesion', [['fecha', 'Fecha', 'date'], ['hora', 'Hora', 'time'], ['sede', 'Sede / enlace'], ['modalidad', 'Modalidad', ['Presencial', 'Virtual']]], 'Registrar sesión')),
        'Asignar miembros': this.panel('Asignar miembro de mesa', this.form('asignar', [['sesion', 'Sesión', selSes], ['dni', 'DNI del miembro']], 'Asignar')),
        'Asignar capacitadores': this.panel('Asignar capacitador', this.form('asigCap', [['sesion', 'Sesión', selSes], ['dni', 'DNI del capacitador']], 'Asignar')),
        'Programación': this.panel('', this.table(['Fecha', 'Hora', 'Sede', 'Modalidad', 'Capacitador', 'Participantes'], ses.map(s => [s.fecha, s.hora, s.sede, this.tag(s.modalidad), s.capacitador || 'Sin asignar', mie.filter(m => m.sesionId === s.id).length])))
      }),
      materiales: () => this.mats(mat, true),
      seguimiento: () => this.tabs({
        'Progreso': this.panel('Progreso por región / ODPE', this.bars(reg)),
        'Asistencia': this.panel('', this.table(['DNI', 'Nombre', 'Sesión', 'Estado'], filas(mie.filter(m => m.asistio !== null)))),
        'Cumplimiento': this.panel('Cumplimiento por sesión', this.bars(cumpl)),
        'Pendientes': this.panel('', this.table(['DNI', 'Nombre', 'Sesión', 'Estado'], filas(pend)))
      }),
      reportes: () => this.tabs({
        'Asistencia': this.panel('Reporte de asistencia', this.table(['DNI', 'Nombre', 'Sesión', 'Estado'], filas(mie)) + this.exportar('asistencia')),
        'Cumplimiento': this.panel('Reporte de cumplimiento', this.bars([['Nacional', res.cumplimiento], ...reg]) + this.exportar('cumplimiento'))
      }),
      usuarios: () => this.panel('Roles del sistema', this.table(['Rol', 'Alcance'], [
        ['Coordinador de Capacitación', 'Visión general, sesiones, asignaciones, reportes'], ['Capacitador', 'Sesiones asignadas y registro de asistencia'],
        ['Asistente Logístico', 'Materiales, distribución e incidencias'], ['Miembro de Mesa', 'Su capacitación y materiales']]) + '<p class="muted" style="margin-top:1rem">La gestión de cuentas (CUS-11) se conectará a la API.</p>'),
      config: () => this.panel('Parámetros', this.table(['Parámetro', 'Valor'], [['Intentos máximos de acceso', CONFIG.MAX_ATTEMPTS], ['Bloqueo temporal (s)', CONFIG.LOCK_MS / 1000], ['Origen de datos', CONFIG.USE_MOCK ? 'Demo (mock)' : 'API C# .NET']]))
    }[r]();
  }

  /* ---------- Capacitador ---------- */
  static cap(r, d) {
    const mine = d.ses.filter(s => s.capacitador === d.u.dni), ids = mine.map(s => s.id), h = hoy();
    const ms = d.mie.filter(m => ids.includes(m.sesionId)), prox = mine.filter(s => s.fecha >= h);
    const H = ['Fecha', 'Hora', 'Lugar / enlace', 'Modalidad', 'Participantes'];
    const fila = s => [s.fecha, s.hora, s.sede, this.tag(s.modalidad), ms.filter(m => m.sesionId === s.id).length];
    const est = l => this.table(['DNI', 'Nombre', 'Estado'], l.map(m => [m.dni, m.nombre, this.tag(this.estado(m))]));
    const marc = ms.filter(m => m.asistio !== null).length, pres = ms.filter(m => m.asistio === true).length;
    return {
      inicio: () => {
        const hoyS = mine.filter(s => s.fecha === h), past = mine.filter(s => s.fecha < h);
        const sp = s => { const a = ms.filter(m => m.sesionId === s.id); return a.length ? Math.round(a.filter(m => m.asistio).length * 100 / a.length) : 0; };
        return this.head('Panel del Capacitador', `Capacitador: ${d.u.nombre} · DNI ${d.u.dni}`) +
          this.stats([['Sesiones del día', hoyS.length, `${prox.length} próximas`, RED, '📅'], ['Participantes', ms.length, `${mine.length} sesiones asignadas`, NAVY, '👥'], ['Asistencia registrada', (marc ? Math.round(pres * 100 / marc) : 0) + '%', `${pres} de ${ms.length}`, GOLD, '✅'], ['Sesiones realizadas', past.length, 'Este proceso electoral', GREEN, '🏆']]) +
          this.row2(this.panel('Mis sesiones asignadas', mine.slice(0, 4).map(s => `${this.sesItem(s, ms.filter(m => m.sesionId === s.id).length)}<div class="bar"><i style="width:${sp(s)}%;background:${GREEN}"></i></div>`).join('') || '<p class="muted">No tienes sesiones asignadas.</p>'),
            this.panel('Registrar asistencia por DNI', this.form('asisDni', [['sesion', 'Sesión', this.sel(mine)], ['dni', 'DNI del miembro']], 'Registrar') + `<div style="margin-top:1rem">${ms.slice(0, 5).map(m => this.person(m)).join('')}</div>`)) +
          this.panel('Mi seguimiento — Proceso Electoral 2026', Charts.bars(d.res.tendencia, ['v'], [NAVY]));
      },
      mis: () => this.tabs({
        'Sesiones asignadas': this.panel('', this.table(H, mine.map(fila))),
        'Lista de miembros': this.panel('', this.table(['DNI', 'Nombre', 'Sesión'], ms.map(m => [m.dni, m.nombre, mine.find(s => s.id === m.sesionId).etiqueta])))
      }),
      asistencia: () => this.tabs({
        'Por sesión': this.asistencia(mine),
        'Buscar por DNI': this.panel('Registrar asistencia por DNI', this.form('asisDni', [['sesion', 'Sesión', this.sel(mine)], ['dni', 'DNI del miembro']], 'Marcar asistencia')),
        'Asistentes / ausentes': this.panel('', est(ms))
      }),
      materiales: () => this.mats(d.mat, false),
      participantes: () => this.panel('', this.table(['DNI', 'Nombre', 'Cargo', 'Estado'], ms.map(m => [m.dni, m.nombre, m.cargo, this.tag(this.estado(m))]))),
      seguimiento: () => this.kpis([[mine.filter(s => s.fecha < h).length, 'Sesiones realizadas'], [marc, 'Participantes atendidos'], [(marc ? Math.round(pres * 100 / marc) : 0) + '%', 'Asistencia']])
    }[r]();
  }

  /* ---------- Asistente Logístico ---------- */
  static log(r, d) {
    const { mat, ses, inc } = d, h = hoy(), selSes = this.sel(ses);
    const noDisp = mat.filter(m => m.estado !== 'Disponible'), tit = id => mat.find(m => m.id === id)?.titulo || '—';
    const estD = s => !s.materialIds.length ? 'Sin material' : s.materialIds.every(id => mat.find(m => m.id === id)?.estado === 'Disponible') ? 'Completo' : 'Pendiente';
    const dist = s => [s.etiqueta, s.materialIds.map(tit).join(', ') || 'Sin material', this.tag(estD(s))];
    const caps = [...new Set(ses.map(s => s.capacitador || 'Sin asignar'))];
    const porCap = caps.map(c => { const ss = ses.filter(s => (s.capacitador || 'Sin asignar') === c); return [c, ss.length, [...new Set(ss.flatMap(s => s.materialIds))].map(tit).join(', ') || '—']; });
    const invent = this.table(['Material', 'Tipo', 'Disponibilidad'], mat.map(m => [m.titulo, this.tag(m.tipo), this.tag(m.estado)]));
    return {
      inicio: () => {
        const st = d.res.stock, sum = k => st.reduce((a, x) => a + x[k], 0), mx = Math.max(...st.map(x => x.disp));
        const alertas = [...noDisp.map(m => [m.estado === 'Agotado' ? '🔴' : '🟠', 'Material ' + (m.estado === 'Agotado' ? 'faltante' : 'pendiente'), m.titulo]), ...inc.filter(i => i.estado === 'Abierta').map(i => ['🟡', i.tipo, i.detalle])];
        return this.head('Panel Logístico', `Asistente: ${d.u.nombre} · DNI ${d.u.dni}`) +
          this.stats([['Materiales disponibles', sum('disp').toLocaleString('es-PE'), 'Inventario activo', GREEN, '📦'], ['Materiales distribuidos', sum('dist'), Math.round(sum('dist') * 100 / sum('disp')) + '% del total', NAVY, '✅'], ['Materiales pendientes', sum('pend'), 'Requieren distribución', GOLD, '⏳'], ['Alertas activas', alertas.length, 'Material faltante / pendiente', RED, '⚠️']]) +
          this.row2(this.panel('Disponibilidad por tipo de material', st.map(x => `<div class="hb"><span>${esc(x.n)}</span><div class="stack"><i style="width:${x.dist * 100 / mx}%;background:${GREEN}"></i><i style="width:${x.pend * 100 / mx}%;background:${GOLD}"></i></div><small>${x.dist}/${x.disp}</small></div>`).join('') + Charts.legend([['Distribuido', GREEN], ['Pendiente', GOLD]])),
            this.panel('Material asignado por sesión', ses.map(s => `<div class="sitem"><div><div class="t">${esc(s.sede)}</div><small>${esc(s.fecha)} · ${esc(s.materialIds.map(tit).join(', ') || 'Sin material')}</small></div>${this.tag(estD(s)).raw}</div>`).join('') || '<p class="muted">Sin sesiones.</p>')) +
          this.panel('⚠️ Alertas e incidencias activas', alertas.map(([e, t, x]) => `<div class="sitem"><span>${e}</span><div><div class="t">${esc(t)}</div><small>${esc(x)}</small></div></div>`).join('') || '<p class="muted">Sin alertas activas.</p>');
      },
      materiales: () => this.mats(mat, true),
      distribucion: () => this.tabs({
        'Asignar material': this.panel('Asignar material a una sesión', this.form('distrib', [['sesion', 'Sesión', selSes], ['material', 'Material', mat.map(m => [m.id, m.titulo])]], 'Asignar')),
        'Por sesión': this.panel('', this.table(['Sesión', 'Material', 'Estado'], ses.map(dist))),
        'Por capacitador': this.panel('', this.table(['Capacitador (DNI)', 'Sesiones', 'Materiales'], porCap)),
        'Estado': this.kpis(['Completo', 'Pendiente', 'Sin material'].map(e => [ses.filter(s => estD(s) === e).length, e]))
      }),
      sesiones: () => this.panel('Sesiones y materiales requeridos', this.table(['Fecha', 'Sede', 'Materiales requeridos', 'Estado'], ses.map(s => [s.fecha, s.sede, s.materialIds.map(tit).join(', ') || 'Sin material', this.tag(estD(s))]))),
      alertas: () => this.tabs({
        'Material faltante / pendiente': this.panel('', this.table(['Material', 'Estado'], noDisp.map(m => [m.titulo, this.tag(m.estado)]))) +
          this.panel('Incidencias', this.table(['Fecha', 'Tipo', 'Detalle', 'Estado'], inc.map(i => [i.fecha, i.tipo, i.detalle, this.tag(i.estado)]))),
        'Registrar incidencia': this.panel('Nueva incidencia', this.form('incidencia', [['tipo', 'Tipo', ['Material faltante', 'Material pendiente']], ['detalle', 'Detalle']], 'Registrar'))
      }),
      reportes: () => this.tabs({
        'Inventario / disponibilidad': this.panel('', invent + this.exportar('inventario')),
        'Distribución de materiales': this.panel('', this.table(['Sesión', 'Material', 'Estado'], ses.map(dist)) + this.exportar('distribucion'))
      })
    }[r]();
  }

  /* ---------- Miembro de Mesa ---------- */
  static mem(r, d) {
    const m = d.me, s = m.sesion, est = this.estado(m), av = m.asistio === true ? 100 : s ? 50 : 0;
    return {
      inicio: () => {
        const done = m.asistio === true ? 1 : 0;
        return this.head('Mi Panel de Capacitación', `${m.nombre} · DNI: ${m.dni} · ${m.local}`) +
          this.panel('Mi estado de capacitación', `<div class="bar"><i style="width:${av}%;background:${GREEN}"></i></div><p><b>${av}%</b> · ${done} de ${s ? 1 : 0} sesiones completadas</p>`) +
          this.row2(this.panel('📅 Mis capacitaciones', s ? this.sesItem(s, null) : '<p class="muted">Aún sin sesión asignada.</p>'),
            this.panel('📚 Materiales de capacitación', this.matsTipo(d.mat.filter(x => x.estado === 'Disponible')))) +
          this.panel('📈 Mi historial de participación', this.table(['Sesión', 'Fecha', 'Participación'], s ? [[s.sede, s.fecha, this.tag(est)]] : []));
      },
      mis: () => this.panel('', this.table(['Fecha', 'Hora', 'Modalidad', 'Lugar / enlace'], s ? [[s.fecha, s.hora, this.tag(s.modalidad), s.sede]] : [])),
      materiales: () => this.mats(d.mat.filter(x => x.estado === 'Disponible'), false),
      progreso: () => this.kpis([[m.asistio === true ? 1 : 0, 'Capacitaciones realizadas'], [est, 'Asistencia'], [av + '%', 'Cumplimiento']]) + this.panel('', this.bars([['Cumplimiento', av]])),
      historial: () => this.panel('Sesiones anteriores', this.table(['Fecha', 'Sede', 'Participación'], s && s.fecha < hoy() ? [[s.fecha, s.sede, this.tag(est)]] : []))
    }[r]();
  }
}
