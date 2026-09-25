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

  async consult(e) { // CUS-01: consulta por DNI directo a la API
    e.preventDefault();
    const dniVal = document.getElementById('dniQ').value.trim();
    const out = document.getElementById('dniOut');
    
    if (!dniVal || dniVal.length !== 8) {
      out.innerHTML = `<div class="alert" role="alert">Por favor, ingrese un DNI válido de 8 dígitos.</div>`;
      return;
    }

    try {
      // Llamada directa a tu Web API de C# que creamos en el MiembroMesaController
      const miembroRepo = new MemberRepository();
      const m = await miembroRepo.byDni(dniVal);

      // Si la API responde con éxito, estructuramos el objeto de usuario y mostramos el resultado
      const user = { dni: m.dni, rol: 'miembro', nombre: `${m.nombres} ${m.apellidos}`, token: 'real-jwt' };
      
      out.innerHTML = `<div class="result"><p class="ok">✔ Eres miembro de mesa. Tu capacitación está asignada.</p>${Views.miembro(m)}<button class="btn" id="goApp">Ingresar a mi capacitación</button></div>`;
      
      document.getElementById('goApp').onclick = () => { 
        SessionStore.save(user); 
        location.href = 'app.html'; 
      };
    } catch (x) { 
      // Si el backend devuelve 404 o error, se muestra el mensaje de que no figura
      out.innerHTML = `<div class="alert" role="alert">Acceso denegado: el DNI no figura como miembro de mesa.</div>`; 
    }
  }
}
document.addEventListener('DOMContentLoaded', () => new AuthController().init());
