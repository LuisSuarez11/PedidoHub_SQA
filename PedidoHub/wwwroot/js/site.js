// PedidoHub - comportamiento del navegador.
// Todo es mejora progresiva: sin JavaScript la aplicación funciona igual (enlaces y formularios normales).
(() => {
    'use strict';

    const reducirMovimiento = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const todos = (selector, raiz = document) => [...raiz.querySelectorAll(selector)];
    const sinAcentos = (texto) => texto.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();
    const dos = (n) => String(n).padStart(2, '0');

    // ---------- Reloj y tiempo transcurrido de cada pedido ----------
    function actualizarTiempos() {
        const ahora = new Date();
        todos('[data-reloj]').forEach((e) => { e.textContent = `${dos(ahora.getHours())}:${dos(ahora.getMinutes())}`; });
        todos('[data-desde]').forEach((e) => {
            const minutos = Math.floor((ahora - new Date(e.dataset.desde)) / 60000);
            if (Number.isNaN(minutos) || minutos < 0) { return; } // hora futura (datos de ejemplo): queda el texto fijo
            const horas = Math.floor(minutos / 60);
            e.textContent = `lleva ${horas > 0 ? `${horas} h ` : ''}${minutos % 60} min`;
        });
    }
    actualizarTiempos();
    setInterval(actualizarTiempos, 30000);

    // ---------- Cifras que cuentan hasta su valor (una sola vez, al cargar) ----------
    todos('[data-contar]').forEach((e) => {
        const final = parseFloat(e.dataset.contar);
        const decimales = parseInt(e.dataset.decimales || '0', 10);
        if (Number.isNaN(final) || reducirMovimiento) { return; }
        const formato = (v) => v.toLocaleString('en-US', { minimumFractionDigits: decimales, maximumFractionDigits: decimales });
        const inicio = performance.now();
        const duracion = 700;
        const paso = (ahora) => {
            const avance = Math.min(1, (ahora - inicio) / duracion);
            e.textContent = formato(avance >= 1 ? final : final * (1 - Math.pow(2, -10 * avance)));
            if (avance < 1) { requestAnimationFrame(paso); }
        };
        e.textContent = formato(0);
        requestAnimationFrame(paso);
    });

    // ---------- Avisos (toasts) ----------
    todos('[data-toast]').forEach((aviso) => {
        const cerrar = () => {
            aviso.classList.add('is-saliendo');
            setTimeout(() => aviso.remove(), reducirMovimiento ? 0 : 220);
        };
        aviso.querySelector('[data-toast-cerrar]')?.addEventListener('click', cerrar);
        if (!aviso.hasAttribute('data-toast-fijo')) { setTimeout(cerrar, 4500); }
    });

    // ---------- Filtro instantáneo de listas ----------
    todos('[data-filtro]').forEach((entrada) => {
        const tabla = document.querySelector(entrada.dataset.filtro);
        if (!tabla) { return; }
        const filas = todos('tbody tr', tabla);
        const cuenta = document.querySelector(entrada.dataset.cuenta || '');
        const vacio = document.querySelector(entrada.dataset.vacio || '');
        entrada.addEventListener('input', () => {
            const consulta = sinAcentos(entrada.value.trim());
            let visibles = 0;
            filas.forEach((fila) => {
                const coincide = !consulta || sinAcentos(fila.textContent).includes(consulta);
                fila.hidden = !coincide;
                if (coincide) { visibles++; }
            });
            if (cuenta) { cuenta.textContent = `${visibles} ${visibles === 1 ? entrada.dataset.singular : entrada.dataset.plural}`; }
            if (vacio) { vacio.hidden = visibles !== 0; }
        });
    });

    // ---------- Botones de envío ocupados ----------
    document.addEventListener('submit', (evento) => {
        const formulario = evento.target;
        if (!(formulario instanceof HTMLFormElement) || formulario.method !== 'post') { return; }
        setTimeout(() => {
            if (evento.defaultPrevented) { return; } // la validación del formulario lo detuvo
            const boton = formulario.querySelector('button[type="submit"]');
            if (boton) { boton.classList.add('is-ocupado'); boton.setAttribute('aria-busy', 'true'); }
        }, 0);
    });
    window.addEventListener('pageshow', () => {
        todos('.is-ocupado').forEach((b) => { b.classList.remove('is-ocupado'); b.removeAttribute('aria-busy'); });
    });

    // ---------- Cajón lateral de cada mesa ----------
    const cajon = document.getElementById('panelLateral');
    if (cajon && window.bootstrap) {
        const titulo = document.getElementById('panelLateralTitulo');
        const cuerpo = document.getElementById('panelLateralCuerpo');
        const instancia = window.bootstrap.Offcanvas.getOrCreateInstance(cajon);

        const cargar = async (url, nombre) => {
            titulo.textContent = nombre;
            cuerpo.innerHTML = '<div class="esqueleto"><i></i><i></i><i></i></div>';
            instancia.show();
            try {
                const respuesta = await fetch(url, { headers: { 'X-Requested-With': 'fetch' } });
                if (!respuesta.ok) { throw new Error(respuesta.statusText); }
                cuerpo.innerHTML = await respuesta.text();
            } catch {
                cuerpo.innerHTML = '<p class="text-muted">No se pudo cargar la mesa.</p><button type="button" class="btn btn-secondary" data-reintentar>Reintentar</button>';
                cuerpo.querySelector('[data-reintentar]').addEventListener('click', () => cargar(url, nombre));
            }
        };

        document.addEventListener('click', (evento) => {
            const enlace = evento.target.closest('[data-panel]');
            if (!enlace || evento.button !== 0 || evento.metaKey || evento.ctrlKey || evento.shiftKey) { return; }
            evento.preventDefault();
            cargar(enlace.dataset.panel, enlace.dataset.titulo || 'Mesa');
        });
    }

    // ---------- Paleta de comandos (Ctrl+K) ----------
    const paleta = document.getElementById('paleta');
    const entrada = document.getElementById('paleta-entrada');
    const lista = document.getElementById('paleta-lista');
    if (!paleta || !entrada || !lista) { return; }

    const config = window.PedidoHub || { destinos: [], buscar: '' };
    let resultados = [];
    let remotos = [];
    let activo = 0;
    let anterior = null;
    let temporizador;
    let peticion = 0;

    const pintar = () => {
        lista.replaceChildren();
        if (!resultados.length) {
            const vacio = document.createElement('li');
            vacio.className = 'paleta-vacio';
            vacio.textContent = 'Sin resultados.';
            lista.append(vacio);
            entrada.removeAttribute('aria-activedescendant');
            return;
        }
        resultados.forEach((r, i) => {
            const item = document.createElement('li');
            item.id = `paleta-${i}`;
            item.setAttribute('role', 'option');
            item.className = `paleta-item${i === activo ? ' is-activo' : ''}`;
            item.setAttribute('aria-selected', String(i === activo));
            for (const [clase, texto] of [['paleta-tipo', r.tipo], ['paleta-titulo', r.titulo], ['paleta-detalle', r.detalle]]) {
                const celda = document.createElement('span');
                celda.className = clase;
                celda.textContent = texto;
                item.append(celda);
            }
            item.addEventListener('mousemove', () => { if (activo !== i) { activo = i; marcar(); } });
            item.addEventListener('click', () => { window.location.href = r.url; });
            lista.append(item);
        });
        entrada.setAttribute('aria-activedescendant', `paleta-${activo}`);
    };

    const marcar = () => {
        todos('.paleta-item', lista).forEach((item, i) => {
            item.classList.toggle('is-activo', i === activo);
            item.setAttribute('aria-selected', String(i === activo));
        });
        entrada.setAttribute('aria-activedescendant', `paleta-${activo}`);
        lista.querySelector('.is-activo')?.scrollIntoView({ block: 'nearest' });
    };

    const combinar = () => {
        const consulta = sinAcentos(entrada.value.trim());
        const locales = config.destinos.filter((d) => !consulta ||
            sinAcentos(`${d.tipo} ${d.titulo} ${d.detalle}`).includes(consulta));
        resultados = [...remotos, ...locales].slice(0, 12);
        activo = 0;
        pintar();
    };

    const buscarRemoto = () => {
        clearTimeout(temporizador);
        const consulta = entrada.value.trim();
        if (consulta.length < 2 || !config.buscar) { remotos = []; combinar(); return; }
        temporizador = setTimeout(async () => {
            const actual = ++peticion;
            try {
                const respuesta = await fetch(`${config.buscar}?q=${encodeURIComponent(consulta)}`);
                if (!respuesta.ok || actual !== peticion) { return; }
                remotos = await respuesta.json();
            } catch { remotos = []; }
            if (actual === peticion) { combinar(); }
        }, 180);
        combinar();
    };

    const abrir = () => {
        anterior = document.activeElement;
        paleta.hidden = false;
        document.body.classList.add('paleta-abierta');
        entrada.value = '';
        remotos = [];
        combinar();
        entrada.focus();
    };

    const cerrar = () => {
        paleta.hidden = true;
        document.body.classList.remove('paleta-abierta');
        if (anterior instanceof HTMLElement) { anterior.focus(); }
    };

    entrada.addEventListener('input', buscarRemoto);
    entrada.addEventListener('keydown', (evento) => {
        if (evento.key === 'ArrowDown' && resultados.length) { evento.preventDefault(); activo = (activo + 1) % resultados.length; marcar(); }
        else if (evento.key === 'ArrowUp' && resultados.length) { evento.preventDefault(); activo = (activo - 1 + resultados.length) % resultados.length; marcar(); }
        else if (evento.key === 'Enter' && resultados[activo]) { evento.preventDefault(); window.location.href = resultados[activo].url; }
        else if (evento.key === 'Tab') { evento.preventDefault(); }
    });

    document.addEventListener('keydown', (evento) => {
        if ((evento.ctrlKey || evento.metaKey) && evento.key.toLowerCase() === 'k') {
            evento.preventDefault();
            if (paleta.hidden) { abrir(); } else { cerrar(); }
        } else if (evento.key === 'Escape' && !paleta.hidden) {
            cerrar();
        }
    });
    todos('[data-paleta-abrir]').forEach((b) => b.addEventListener('click', abrir));
    todos('[data-paleta-cerrar]').forEach((b) => b.addEventListener('click', cerrar));
})();
