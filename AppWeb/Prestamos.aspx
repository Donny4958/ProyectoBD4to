<%@ Page Title="Préstamos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<link rel="stylesheet" href="https://cdn.datatables.net/2.3.1/css/dataTables.dataTables.css" />
<script src="https://cdn.datatables.net/2.3.1/js/dataTables.js"></script>
<h2>Préstamos</h2>

<!-- Filtros con checkbox -->
<div style="margin-bottom:15px;">
    <label><input type="checkbox" id="chkPendiente" checked /> Pendientes</label>
    <label style="margin-left:10px;"><input type="checkbox" id="chkExpirado" /> Expirados</label>
    <label style="margin-left:20px;"><input type="checkbox" id="chkInterno" checked /> Interno</label>
    <label style="margin-left:10px;"><input type="checkbox" id="chkExterno" checked /> Externo</label>
    <button type="button" onclick="abrirModalPrestamo()" style="margin-left:20px;">Pedir préstamo</button>
</div>

<!-- Modal para pedir préstamo -->
<div id="modalPrestamo" class="modal" tabindex="-1" style="display:none; position:fixed; z-index:1050; left:0; top:0; width:100%; height:100%; overflow:auto; background:rgba(0,0,0,0.5);">
    <div style="background:#fff; margin:5% auto; padding:20px; border-radius:8px; width:400px; position:relative;">
        <span style="position:absolute; top:10px; right:15px; cursor:pointer; font-size:20px;" onclick="cerrarModalPrestamo()">&times;</span>
        <h3>Nuevo Préstamo</h3>
        <label>Usuario:
            <select id="id_usuario_prestamo" class="form-control" required></select>
        </label><br />
        <label>Dispositivo:
            <select id="id_dispositivo" class="form-control" required></select>
        </label><br />
        <label>Tipo:
            <select id="tipo_prestamo" class="form-control">
                <option value="interno">Interno</option>
                <option value="externo">Externo</option>
            </select>
        </label><br />
        <label>Fecha estimada regreso: <input type="date" id="fecha_estimada" class="form-control" required /></label><br />
        <button type="button" class="btn btn-success" onclick="crearPrestamo();">Pedir</button>
        <button type="button" class="btn btn-secondary" onclick="cerrarModalPrestamo();">Cancelar</button>
    </div>
</div>

<!-- Modal para aceptar préstamo -->
<div id="modalAceptar" class="modal" tabindex="-1" style="display:none; position:fixed; z-index:1050; left:0; top:0; width:100%; height:100%; overflow:auto; background:rgba(0,0,0,0.5);">
    <div style="background:#fff; margin:5% auto; padding:20px; border-radius:8px; width:400px; position:relative;">
        <span style="position:absolute; top:10px; right:15px; cursor:pointer; font-size:20px;" onclick="cerrarModalAceptar()">&times;</span>
        <h3>Aceptar Préstamo</h3>
        <input type="hidden" id="aceptar_id_prestamo" />
        <label>Usuario autorizador:
            <select id="id_autorizador" class="form-control" required></select>
        </label><br />
        <button type="button" class="btn btn-primary" onclick="aceptarPrestamo();">Aceptar</button>
        <button type="button" class="btn btn-secondary" onclick="cerrarModalAceptar();">Cancelar</button>
    </div>
</div>

<div id="modalImagen" style="display:none; position:fixed; z-index:2000; left:0; top:0; width:100vw; height:100vh; background:rgba(0,0,0,0.7); align-items:center; justify-content:center;">
    <span style="position:absolute; top:20px; right:40px; color:#fff; font-size:40px; cursor:pointer;" onclick="cerrarModalImagen()">&times;</span>
    <img id="imgModalGrande" src="" style="max-width:90vw; max-height:90vh; display:block; margin:auto;" />
</div>

<table id="tablaPrestamos" class="display" style="width:100%">
    <thead>
        <tr>
            <th>ID</th>
            <th>Usuario</th>
            <th>Dispositivo</th>
            <th>Tipo</th>
            <th>Fecha salida</th>
            <th>Fecha estimada</th>
            <th>Fecha regreso</th>
            <th>Acciones</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<script>
    let tablaPrestamos;
    let usuarios = [];
    let dispositivos = [];

    document.addEventListener("DOMContentLoaded", function () {
        cargarUsuariosYDispositivos();
        inicializarTabla();
        document.getElementById("chkPendiente").addEventListener("change", recargarTablaPrestamos);
        document.getElementById("chkExpirado").addEventListener("change", recargarTablaPrestamos);
        document.getElementById("chkInterno").addEventListener("change", recargarTablaPrestamos);
        document.getElementById("chkExterno").addEventListener("change", recargarTablaPrestamos);
    });

    function cargarUsuariosYDispositivos() {
        // Cargar usuarios
        fetch('/api/Usuarios/ObtenerTodos')
            .then(r => r.json())
            .then(data => {
                usuarios = data;
                let userSelects = [document.getElementById("id_usuario_prestamo"), document.getElementById("id_autorizador")];
                userSelects.forEach(select => {
                    select.innerHTML = '<option value="">Seleccione usuario</option>';
                    data.forEach(u => {
                        select.innerHTML += `<option value="${u.id_usuario}">${u.nombre}</option>`;
                    });
                });
            });
        // Cargar dispositivos
        fetch('/api/Dispositivos/ObtenerTodos')
            .then(r => r.json())
            .then(data => {
                dispositivos = data;
                let select = document.getElementById("id_dispositivo");
                select.innerHTML = '<option value="">Seleccione dispositivo</option>';
                data.forEach(d => {
                    select.innerHTML += `<option value="${d.id_dispositivo}">${d.nombre}</option>`;
                });
            });
    }

    function inicializarTabla() {
        tablaPrestamos = new DataTable('#tablaPrestamos', {
            ajax: {
                url: '/api/Prestamos/Revisar',
                dataSrc: function (json) { return json; },
                data: function (d) {
                    // Estado
                    const pendiente = document.getElementById("chkPendiente").checked;
                    const expirado = document.getElementById("chkExpirado").checked;
                    if (pendiente && !expirado) d.estado = "activo";
                    else if (!pendiente && !expirado) d.estado = "inactivo";
                    else if (!pendiente && expirado) d.Expirado = "1";
                    else if (pendiente && expirado) {
                        d.estado = "activo";
                        d.Expirado = "1";
                    }
                    // Tipo
                    const interno = document.getElementById("chkInterno").checked;
                    const externo = document.getElementById("chkExterno").checked;
                    if (interno && !externo) d.tipo = "interno";
                    else if (!interno && externo) d.tipo = "externo";
                }
            },
            columns: [
                { data: 'id_prestamo' },
                { data: 'usuario' },
                { data: 'dispositivo' },
                { data: 'tipo_prestamo' },
                { data: 'fecha_salida' },
                { data: 'fecha_estimada' },
                { data: 'fecha_regreso' },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        let acciones = '';
                        if (!row.fecha_regreso) {
                            acciones += `<button class="btn btn-sm btn-success" onclick="regresarComponente(${row.id_prestamo})">Regresar</button> `;
                            if ((!row.id_login_autorizado || row.id_login_autorizado === 0) && sessionStorage.getItem('tipo')) {
                                acciones += `<button class="btn btn-sm btn-primary" onclick="abrirModalAceptar(${row.id_prestamo})">Aceptar</button>`;
                            }
                        }
                        return acciones;
                    }
                }
            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/2.3.1/i18n/es-ES.json'
            }
        });
    }

    function recargarTablaPrestamos() {
        tablaPrestamos.ajax.reload();
    }

    function abrirModalPrestamo() {
        document.getElementById("modalPrestamo").style.display = "block";
    }
    function cerrarModalPrestamo() {
        document.getElementById("modalPrestamo").style.display = "none";
        document.getElementById("id_usuario_prestamo").value = "";
        document.getElementById("id_dispositivo").value = "";
        document.getElementById("tipo_prestamo").value = "interno";
        document.getElementById("fecha_estimada").value = "";
    }

    function crearPrestamo() {
        const id_usuario_prestamo = parseInt(document.getElementById('id_usuario_prestamo').value);
        const id_dispositivo = parseInt(document.getElementById('id_dispositivo').value);
        const tipo_prestamo = document.getElementById('tipo_prestamo').value;
        const fecha_estimada = document.getElementById('fecha_estimada').value;

        if (!id_usuario_prestamo || !id_dispositivo || !fecha_estimada) {
            alert('Todos los campos son obligatorios.');
            return;
        }

        fetch('/api/Prestamos/Pedir', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                id_usuario_prestamo,
                id_dispositivo,
                tipo_prestamo,
                fecha_estimada
            })
        })
            .then(response => response.ok ? response.text() : response.text().then(msg => { throw new Error(msg); }))
            .then(msg => {
                alert(msg);
                cerrarModalPrestamo();
                recargarTablaPrestamos();
            })
            .catch(error => alert(error.message));
    }

    // Aceptar préstamo
    function abrirModalAceptar(id_prestamo) {
        document.getElementById("aceptar_id_prestamo").value = id_prestamo;
        document.getElementById("modalAceptar").style.display = "block";
    }
    function cerrarModalAceptar() {
        document.getElementById("modalAceptar").style.display = "none";
        document.getElementById("aceptar_id_prestamo").value = "";
        document.getElementById("id_autorizador").value = "";
    }
    function aceptarPrestamo() {
        const id_prestamo = document.getElementById("aceptar_id_prestamo").value;
        const id_autorizador = document.getElementById("id_autorizador").value;
        if (!id_prestamo || !id_autorizador) {
            alert("Selecciona el usuario autorizador.");
            return;
        }
        fetch('/api/Prestamos/Aceptar/' + id_prestamo, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id_autorizador: parseInt(id_autorizador) })
        })
            .then(response => response.ok ? response.text() : response.text().then(msg => { throw new Error(msg); }))
            .then(msg => {
                alert(msg);
                cerrarModalAceptar();
                recargarTablaPrestamos();
            })
            .catch(error => alert(error.message));
    }

    function regresarComponente(id_prestamo) {
        if (!confirm('¿Seguro que deseas marcar este componente como regresado?')) return;
        fetch('/api/Prestamos/Regresar/' + id_prestamo, {
            method: 'PUT'
        })
            .then(response => response.ok ? response.text() : response.text().then(msg => { throw new Error(msg); }))
            .then(msg => {
                alert(msg);
                recargarTablaPrestamos();
            })
            .catch(error => alert(error.message));
    }

    // Modal imagen (si lo necesitas)
    function mostrarModalImagen(base64) {
        document.getElementById('imgModalGrande').src = 'data:image/png;base64,' + base64;
        document.getElementById('modalImagen').style.display = 'flex';
    }
    function cerrarModalImagen() {
        document.getElementById('modalImagen').style.display = 'none';
        document.getElementById('imgModalGrande').src = '';
    }
</script>
</asp:Content>
