<%@ Page Title="Equipos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<link rel="stylesheet" href="https://cdn.datatables.net/2.3.1/css/dataTables.dataTables.css" />  
<script src="https://cdn.datatables.net/2.3.1/js/dataTables.js"></script>
<h2>Equipos</h2>
<button type="button" onclick="abrirModalEquipo()">Crear nuevo equipo</button>

<!-- Modal para crear/editar equipo -->
<div id="modalEquipo" class="modal" tabindex="-1" style="display:none; position:fixed; z-index:1050; left:0; top:0; width:100%; height:100%; overflow:auto; background:rgba(0,0,0,0.5);">
    <div style="background:#fff; margin:5% auto; padding:20px; border-radius:8px; width:400px; position:relative;">
        <span style="position:absolute; top:10px; right:15px; cursor:pointer; font-size:20px;" onclick="cerrarModalEquipo()">&times;</span>
        <h3 id="modalEquipoTitulo">Nuevo Equipo</h3>
        <input type="hidden" id="equipo_id" />
        <label>Nombre:</label>
        <input type="text" id="equipo_nombre" maxlength="255" class="form-control" /><br />
        <label>Laboratorio:</label>
        <select id="equipo_laboratorio" class="form-control"></select><br />
        <label>Responsable:</label>
        <select id="equipo_responsable" class="form-control"></select><br />
        <button type="button" onclick="guardarEquipo()" class="btn btn-success">Guardar</button>
        <button type="button" onclick="cerrarModalEquipo()" class="btn btn-secondary">Cancelar</button>
    </div>
</div>

<!-- Modal para ver detalles de equipo (incluye proyectos) -->
<div id="modalDetalleEquipo" class="modal" tabindex="-1" style="display:none; position:fixed; z-index:1050; left:0; top:0; width:100%; height:100%; overflow:auto; background:rgba(0,0,0,0.5);">
    <div style="background:#fff; margin:5% auto; padding:20px; border-radius:8px; width:400px; position:relative;">
        <span style="position:absolute; top:10px; right:15px; cursor:pointer; font-size:20px;" onclick="cerrarModalDetalleEquipo()">&times;</span>
        <h3>Detalle de Equipo</h3>
        <div id="detalleEquipoContenido"></div>
    </div>
</div>

<table id="tablaEquipos" class="display" style="width:100%">
    <thead>
        <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Laboratorio</th>
            <th>Responsable</th>
            <th>Proyectos</th>
            <th>Opciones</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<script>
    let tablaEquipos;

    document.addEventListener("DOMContentLoaded", function () {
        cargarLaboratorios();
        cargarResponsables();
        tablaEquipos = new DataTable('#tablaEquipos', {
            ajax: {
                url: '/api/Equipos/ObtenerTodos',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'nombre' },
                { data: 'laboratorio' },
                { data: 'responsable' },
                {
                    data: null,
                    render: function (data, type, row) {
                        // Si la API de ObtenerTodos no trae proyectos, puedes dejarlo vacío o mostrar un botón para ver detalles
                        return `<button type="button" class="btn btn-info btn-sm" onclick="verDetalleEquipo('${row.id}')">Ver</button>`;
                    }
                },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                        <button type="button" class="btn btn-primary" onclick="editarEquipo('${row.id}')">Editar</button>
                        <button type="button" class="btn btn-danger" onclick="eliminarEquipo('${row.id}')">Eliminar</button>
                    `;
                    }
                }
            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/2.3.1/i18n/es-ES.json'
            }
        });
    });

    function recargarTablaEquipos() {
        if (tablaEquipos) tablaEquipos.ajax.reload();
    }

    function cargarLaboratorios() {
        fetch('/api/Laboratorios/ObtenerLaboratorios')
            .then(r => r.json())
            .then(data => {
                const select = document.getElementById("equipo_laboratorio");
                select.innerHTML = '<option value="">Sin laboratorio</option>';
                data.forEach(lab => {
                    select.innerHTML += `<option value="${lab.Id}">${lab.nombre}</option>`;
                });
            });
    }

    function cargarResponsables() {
        fetch('/api/Laboratorios/ObtenerProfesoresOEmpleados')
            .then(r => r.json())
            .then(data => {
                const select = document.getElementById("equipo_responsable");
                select.innerHTML = '<option value="">Seleccione responsable</option>';
                data.forEach(u => {
                    select.innerHTML += `<option value="${u.ID}">${u.Nombre} (${u.Rol})</option>`;
                });
            });
    }

    function abrirModalEquipo() {
        document.getElementById("modalEquipoTitulo").textContent = "Nuevo Equipo";
        document.getElementById("equipo_id").value = "";
        document.getElementById("equipo_nombre").value = "";
        document.getElementById("equipo_laboratorio").value = "";
        document.getElementById("equipo_responsable").value = "";
        cargarLaboratorios();
        cargarResponsables();
        document.getElementById("modalEquipo").style.display = "block";
    }

    function cerrarModalEquipo() {
        document.getElementById("modalEquipo").style.display = "none";
    }

    function guardarEquipo() {
        const id = document.getElementById("equipo_id").value;
        const nombre = document.getElementById("equipo_nombre").value;
        const laboratorioId = document.getElementById("equipo_laboratorio").value;
        const responsableId = document.getElementById("equipo_responsable").value;

        if (!nombre || !responsableId) {
            alert("El nombre y el responsable son obligatorios.");
            return;
        }

        const body = {
            nombre,
            responsableId: parseInt(responsableId)
        };
        if (laboratorioId) body.laboratorioId = parseInt(laboratorioId);

        if (id) {
            body.id = parseInt(id);
            fetch('/api/Equipos/Modificar', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            })
                .then(r => r.ok ? r.text() : Promise.reject(r.text()))
                .then(() => {
                    recargarTablaEquipos();
                    cerrarModalEquipo();
                })
                .catch(async err => alert(await err));
        } else {
            fetch('/api/Equipos/Crear', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            })
                .then(r => r.ok ? r.text() : Promise.reject(r.text()))
                .then(() => {
                    recargarTablaEquipos();
                    cerrarModalEquipo();
                })
                .catch(async err => alert(await err));
        }
    }

    function editarEquipo(id) {
        fetch(`/api/Equipos/ObtenerPorId?id=${id}`)
            .then(r => r.json())
            .then(data => {
                document.getElementById("modalEquipoTitulo").textContent = "Editar Equipo";
                document.getElementById("equipo_id").value = data.id;
                document.getElementById("equipo_nombre").value = data.nombre;
                cargarLaboratorios();
                cargarResponsables();
                setTimeout(() => {
                    document.getElementById("equipo_laboratorio").value = data.laboratorioId || "";
                    document.getElementById("equipo_responsable").value = data.responsableId || "";
                }, 300);
                document.getElementById("modalEquipo").style.display = "block";
            });
    }

    function eliminarEquipo(id) {
        if (!confirm("¿Seguro que deseas eliminar este equipo?")) return;
        fetch(`/api/Equipos/Eliminar?id=${id}`, {
            method: 'DELETE'
        })
            .then(r => r.ok ? r.text() : Promise.reject(r.text()))
            .then(() => recargarTablaEquipos())
            .catch(async err => alert(await err));
    }

    function verDetalleEquipo(id) {
        fetch(`/api/Equipos/ObtenerPorId?id=${id}`)
            .then(r => r.json())
            .then(data => {
                let html = `
                <strong>Nombre:</strong> ${data.nombre}<br/>
                <strong>Laboratorio:</strong> ${data.laboratorio || 'Sin laboratorio'}<br/>
                <strong>Responsable:</strong> ${data.responsable}<br/>
                <strong>Proyectos:</strong> ${data.Proyectos ? data.Proyectos : 'Sin proyectos'}<br/>
            `;
                document.getElementById("detalleEquipoContenido").innerHTML = html;
                document.getElementById("modalDetalleEquipo").style.display = "block";
            });
    }

    function cerrarModalDetalleEquipo() {
        document.getElementById("modalDetalleEquipo").style.display = "none";
    }
</script>
</asp:Content>
