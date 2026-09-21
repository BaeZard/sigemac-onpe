/* MVC · Controlador de la portada: acceso de miembro de mesa (principal) y personal (pestaña) */
class AuthController {
  rol = 'coordinador';

  init() {
    if (SessionStore.get()) { location.replace('app.html'); return; }
    const dlg = document.getElementById('staffDlg'), box = document.getElementById('roles');
    box.innerHTML = CONFIG.STAFF.map(k => `<button type="button" data-rol="${k}" class="${k === this.rol ? 'on' : ''}">${CONFIG.ROLES[k].icon} ${CONFIG.ROLES[k].label}</button>`).join('');
    box.addEventListener('click', e => {
      const b = e.target.closest('button'); if (!b) return;
      this.rol = b.dataset.rol;
      box.querySelectorAll('button').forEach(x => x.classList.toggle('on', x === b));
    });
    document.getElementById('openStaff').onclick = () => dlg.showModal();
    document.getElementById('closeStaff').onclick = () => dlg.close();
    document.getElementById('loginForm').addEventListener('submit', e => this.login(e));
    document.getElementById('dniForm').addEventListener('submit', e => this.consult(e));
  }

  async login(e) { // personal administrativo
    e.preventDefault();
    const err = document.getElementById('err'); err.textContent = '';
    try {
      await AuthService.login(this.rol, document.getElementById('dni').value.trim(), document.getElementById('pwd').value);
      location.href = 'app.html';
    } catch (x) { err.textContent = x.message; }
  }

  async consult(e) { // CUS-01: consulta por DNI → asignación de capacitación o rechazo
    e.preventDefault();
    const out = document.getElementById('dniOut');
    try {
      const user = await AuthService.memberAccess(document.getElementById('dniQ').value.trim());
      const m = await MemberService.consult(user.dni);
      out.innerHTML = `<div class="result"><p class="ok">✔ Eres miembro de mesa. Tu capacitación está asignada.</p>${Views.miembro(m)}<button class="btn" id="goApp">Ingresar a mi capacitación</button></div>`;
      document.getElementById('goApp').onclick = () => { SessionStore.save(user); location.href = 'app.html'; };
    } catch (x) { out.innerHTML = `<div class="alert" role="alert">${esc(x.message)}</div>`; }
  }
}
document.addEventListener('DOMContentLoaded', () => new AuthController().init());
