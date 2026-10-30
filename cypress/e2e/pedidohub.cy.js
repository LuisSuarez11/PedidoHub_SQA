// Pruebas E2E con Cypress. Escenarios en estilo BDD (Dado / Cuando / Entonces) y datos únicos
// para poder ejecutarse más de una vez contra la misma base de datos.
const unico = (prefijo) => `${prefijo} ${Math.random().toString(36).slice(2, 8)}`;
// Sesión por rol: se inicia una vez y se reutiliza (cy.session guarda las cookies).
const ingresar = (usuario = 'admin') =>
  cy.session(usuario, () => {
    cy.visit('/Cuenta/Ingresar');
    cy.get('input[name=NombreUsuario]').type(usuario);
    cy.get('input[name=Clave]').type(Cypress.env('CLAVE') || 'PedidoHub2026!', { log: false });
    cy.get('.login-caja button[type=submit]').click();
    cy.location('pathname').should('eq', '/Home/Servicio');
  });


// Evidencia: captura del estado final de cada escenario.
afterEach(function () {
  cy.screenshot(this.currentTest.title.replace(/[^\wáéíóúñÁÉÍÓÚÑ ]/g, '').slice(0, 90), { capture: 'viewport', overwrite: true });
});

describe('Navegación y búsqueda', () => {
  beforeEach(() => ingresar());
  it('Dado el inicio, Cuando carga, Entonces muestra el plano de mesas y la navegación', () => {
    cy.visit('/Home/Servicio');
    cy.title().should('contain', 'PedidoHub');
    cy.contains('main', 'Mesa 1');
    cy.get('nav a.side-link').should('have.length.at.least', 8);
  });

  it('Dada la paleta (Ctrl+K), Cuando se busca "Silpancho", Entonces lo encuentra', () => {
    cy.visit('/Home/Servicio');
    cy.get('body').type('{ctrl}k');
    cy.get('#paleta-entrada').type('Silpancho');
    cy.get('#paleta-lista').should('contain', 'Silpancho');
  });

  it('Dado el menú, Cuando se visita cada pantalla del catálogo, Entonces responde sin error', () => {
    ['/Clientes', '/Mesas', '/Platos', '/Empleados', '/Reservas', '/Pedidos', '/Facturas'].forEach((ruta) => {
      cy.request(ruta).its('status').should('eq', 200);
    });
  });

  it('Dado el endpoint de salud, Cuando se consulta, Entonces responde Healthy', () => {
    cy.request('/health').its('body').should('eq', 'Healthy');
  });
});

describe('Clientes y platos', () => {
  beforeEach(() => ingresar());
  it('Dado un cliente válido, Cuando se registra, Entonces aparece en el listado', () => {
    const nombre = unico('Cliente Cypress');
    cy.visit('/Clientes/Create');
    cy.get('input[name=Nombre]').type(nombre);
    cy.get('input[name=Telefono]').type('71230000');
    cy.get('form.form-card button[type=submit]').click();
    cy.get('.toast-ph').should('contain', 'registrado');
    cy.visit('/Clientes');
    cy.contains('main', nombre);
  });

  it('Dado un cliente sin teléfono, Cuando se intenta guardar, Entonces se muestra la validación', () => {
    cy.visit('/Clientes/Create');
    cy.get('input[name=Nombre]').type(unico('Sin telefono'));
    cy.get('form.form-card button[type=submit]').click();
    cy.get('.text-danger').invoke('text').should('match', /\S/);
    cy.location('pathname').should('eq', '/Clientes/Create');
  });

  it('Dado un plato con nombre repetido, Cuando se crea, Entonces se rechaza', () => {
    cy.visit('/Platos/Create');
    cy.get('input[name=Nombre]').type('Silpancho');
    cy.get('input[name=Precio]').type('30');
    cy.get('form.form-card button[type=submit]').click();
    cy.contains('Ya existe un plato con ese nombre.');
  });
});

describe('Reservas', () => {
  beforeEach(() => ingresar());
  const completar = (fecha, personas) => {
    cy.visit('/Reservas/Create');
    cy.get('select[name=IdCliente]').select('Ana Torres');
    cy.get('select[name=NumMesa]').select('Mesa 2 (capacidad 4)');
    cy.get('input[name=FechaHora]').invoke('val', fecha).trigger('change');
    cy.get('input[name=CantPersonas]').clear().type(String(personas));
    cy.get('form.form-card button[type=submit]').click();
  };

  it('Dada una mesa de 4, Cuando se reservan 5 personas, Entonces se rechaza por capacidad', () => {
    completar('2027-05-20T20:00', 5);
    cy.contains('capacidad para 4 personas');
  });

  it('Dada una fecha pasada, Cuando se crea la reserva, Entonces se rechaza', () => {
    completar('2026-01-10T20:00', 2);
    cy.contains('no puede estar en el pasado');
  });
});

describe('Reportes', () => {
  beforeEach(() => ingresar());
  it('Dado el reporte de ventas, Cuando las fechas están invertidas, Entonces informa el error', () => {
    cy.visit('/Reportes/Ventas?Desde=2026-10-20&Hasta=2026-10-01');
    cy.contains('La fecha final no puede ser anterior a la inicial.');
  });

  it('Dado el reporte de platos más vendidos, Cuando se abre, Entonces lista Refresco', () => {
    cy.visit('/Reportes/PlatosMasVendidos');
    cy.contains('main', 'Refresco');
  });
});

describe('Seguridad básica', () => {
  beforeEach(() => ingresar());
  it('Dado un POST sin token antiforgery, Cuando se envía, Entonces es rechazado', () => {
    cy.request({ method: 'POST', url: '/Platos/Create', form: true, body: { Nombre: 'X', Precio: '5' }, failOnStatusCode: false })
      .its('status').should('eq', 400);
  });

  it('Dadas las respuestas HTML, Cuando se piden, Entonces incluyen cabeceras de seguridad', () => {
    cy.request('/').then((r) => {
      expect(r.headers['x-content-type-options']).to.eq('nosniff');
      expect(r.headers['x-frame-options']).to.eq('DENY');
    });
  });
});

describe('Acceso, landing y roles', () => {
  it('Dado un visitante, Cuando abre la raíz, Entonces ve la landing con indicadores de calidad', () => {
    cy.visit('/');
    cy.contains('calidad medible');
    cy.contains('pruebas automatizadas');
    cy.contains('a', 'Ingresar').should('have.attr', 'href').and('include', '/Cuenta/Ingresar');
  });

  it('Dado un visitante, Cuando abre una pantalla interna, Entonces es enviado al login', () => {
    cy.visit('/Clientes');
    cy.location('pathname').should('eq', '/Cuenta/Ingresar');
  });

  it('Dada una clave incorrecta, Cuando se ingresa, Entonces se muestra el error', () => {
    cy.visit('/Cuenta/Ingresar');
    cy.get('input[name=NombreUsuario]').type('admin');
    cy.get('input[name=Clave]').type('incorrecta');
    cy.get('.login-caja button[type=submit]').click();
    cy.contains('Usuario o contraseña incorrectos.');
  });

  it('Dado el administrador, Cuando abre el panel de calidad, Entonces ve las métricas', () => {
    ingresar('admin');
    cy.visit('/Calidad');
    cy.contains('Panel de calidad');
    cy.contains('Definition of Done');
    cy.contains('Pirámide de pruebas');
  });

  it('Dado un mesero, Cuando abre Empleados, Entonces ve acceso denegado', () => {
    ingresar('mesero');
    cy.visit('/Empleados');
    cy.location('pathname').should('eq', '/Cuenta/Denegado');
    cy.contains('Acceso denegado');
  });

  it('Dado el cajero, Cuando abre ventas, Entonces puede exportar el CSV', () => {
    ingresar('cajero');
    cy.visit('/Reportes/Ventas');
    cy.contains('Exportar CSV');
    cy.request('/Reportes/VentasCsv').its('headers.content-type').should('contain', 'text/csv');
  });
});