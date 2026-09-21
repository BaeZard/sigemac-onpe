/* MVC · Modelos (reflejan las entidades de SQL Server) */
class Usuario { constructor({ dni, nombre, rol, token }) { Object.assign(this, { dni, nombre, rol, token }); } }
class Sesion {
  constructor(o) { Object.assign(this, o); }
  get etiqueta() { return `${this.fecha} ${this.hora} · ${this.sede} (${this.modalidad})`; }
}
class Miembro { constructor(o) { Object.assign(this, o); } }
class Material { constructor(o) { Object.assign(this, o); } }
