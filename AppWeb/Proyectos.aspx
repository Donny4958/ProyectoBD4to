<%@ Page Title="Proyectos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<link rel="stylesheet" href="https://cdn.datatables.net/2.3.1/css/dataTables.dataTables.css" />  
<script src="https://cdn.datatables.net/2.3.1/js/dataTables.js"></script>
<h2>Proyectos</h2>
<button type="button" onclick="abrirModalProyecto()">Crear nuevo proyecto</button>

<!-- Modal para crear/editar proyecto -->
<div id="modalProyecto" class="modal" tabindex="-1" style="display:none; position:fixed; z-index:1050; left:0; top:0; width:100%; height:100%; overflow:auto; background:rgba(0,0,0,0.5);">
    <div style="background:#fff; margin:5% auto; padding:20px; border-radius:8px; width:400px; position:relative;">
        <span style="position:absolute; top:10px; right:15px; cursor:pointer; font-size:20px;" onclick="cerrarModalProyecto()">&times;</span>
        <h3 id="modalProyectoTitulo">Nuevo Proyecto</h3>
        <input type="hidden" id="proyecto_id" />
        <label>Nombre:</label>
        <input type="text" id="proyecto_nombre" maxlength="255" class="form-control" /><br />
        <label>Descripción:</label>
        <input type="text" id="proyecto_descripcion" maxlength="255" class="form-control" /><br />
        <label>Laboratorio:</label>
        <select id="proyecto_laboratorio" class="form-control"></select><br />
        <label>Responsable:</label>
        <select id="proyecto_responsable" class="form-control"></select><br />
        <label>Equipo:</label>
        <select id="proyecto_equipo" class="form-control"></select><br />
        <button type="button" onclick="guardarProyecto()" class="btn btn-success">Guardar</button>
        <button type="button" onclick="cerrarModalProyecto()" class="btn btn-secondary">Cancelar</button>
    </div>
</div>

<table id="tablaProyectos" class="display" style="width:100%">
    <thead>
        <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Descripción</th>
            <th>Laboratorio</th>
            <th>Responsable</th>
            <th>Equipo</th>
            <th>Opciones</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<script>
    let tablaProyectos;

    document.addEventListener("DOMContentLoaded", function () {
        cargarLaboratorios();
        cargarResponsables();
        cargarEquipos();
        tablaProyectos = new DataTable('#tablaProyectos', {
            ajax: {
                url: '/api/Proyectos/ObtenerTodos',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'nombre' },
                { data: 'descripcion' },
                { data: 'laboratorio' },
                { data: 'responsable' },
                { data: 'equipo' },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                        <button type="button" class="btn btn-primary" onclick="editarProyecto('${row.id}')">Editar</button>
                        <button type="button" class="btn btn-danger" onclick="eliminarProyecto('${row.id}')">Eliminar</button>
                    `;
                    }
                }
            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/2.3.1/i18n/es-ES.json'
            }
        });
    });

    function recargarTablaProyectos() {
        if (tablaProyectos) tablaProyectos.ajax.reload();
    }

    function cargarLaboratorios() {
        fetch('/api/Laboratorios/ObtenerLaboratorios')
            .then(r => r.json())
            .then(data => {
                const select = document.getElementById("proyecto_laboratorio");
                select.innerHTML = '<option value="">Seleccione laboratorio</option>';
                data.forEach(lab => {
                    select.innerHTML += `<option value="${lab.Id}">${lab.nombre}</option>`;
                });
            });
    }

    function cargarResponsables() {
        fetch('/api/Laboratorios/ObtenerProfesoresOEmpleados')
            .then(r => r.json())
            .then(data => {
                const select = document.getElementById("proyecto_responsable");
                select.innerHTML = '<option value="">Seleccione responsable</option>';
                data.forEach(u => {
                    select.innerHTML += `<option value="${u.ID}">${u.Nombre} (${u.Rol})</option>`;
                });
            });
    }

    function cargarEquipos() {
        fetch('/api/Equipos/ObtenerTodos')
            .then(r => r.json())
            .then(data => {
                const select = document.getElementById("proyecto_equipo");
                select.innerHTML = '<option value="">Seleccione equipo</option>';
                data.forEach(eq => {
                    select.innerHTML += `<option value="${eq.id}">${eq.nombre}</option>`;
                });
            });
    }

    function abrirModalProyecto() {
        document.getElementById("modalProyectoTitulo").textContent = "Nuevo Proyecto";
        document.getElementById("proyecto_id").value = "";
        document.getElementById("proyecto_nombre").value = "";
        document.getElementById("proyecto_descripcion").value = "";
        document.getElementById("proyecto_laboratorio").value = "";
        document.getElementById("proyecto_responsable").value = "";
        document.getElementById("proyecto_equipo").value = "";
        cargarLaboratorios();
        cargarResponsables();
        cargarEquipos();
        document.getElementById("modalProyecto").style.display = "block";
    }

    function cerrarModalProyecto() {
        document.getElementById("modalProyecto").style.display = "none";
    }

    function guardarProyecto() {
        const id = document.getElementById("proyecto_id").value;
        const nombre = document.getElementById("proyecto_nombre").value;
        const descripcion = document.getElementById("proyecto_descripcion").value;
        const laboratorioId = document.getElementById("proyecto_laboratorio").value;
        const responsableId = document.getElementById("proyecto_responsable").value;
        const equipoId = document.getElementById("proyecto_equipo").value;

        if (!nombre || !descripcion || !laboratorioId || !responsableId || !equipoId) {
            alert("Todos los campos son obligatorios.");
            return;
        }

        const body = {
            nombre,
            descripcion,
            laboratorioId: parseInt(laboratorioId),
            responsableId: parseInt(responsableId),
            equipoId: parseInt(equipoId)
        };

        if (id) {
            body.id = parseInt(id);
            fetch('/api/Proyectos/Modificar', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            })
                .then(r => r.ok ? r.text() : Promise.reject(r.text()))
                .then(() => {
                    recargarTablaProyectos();
                    cerrarModalProyecto();
                })
                .catch(async err => alert(await err));
        } else {
            fetch('/api/Proyectos/Crear', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            })
                .then(r => r.ok ? r.text() : Promise.reject(r.text()))
                .then(() => {
                    recargarTablaProyectos();
                    cerrarModalProyecto();
                })
                .catch(async err => alert(await err));
        }
    }

    function editarProyecto(id) {
        fetch(`/api/Proyectos/ObtenerPorId?id=${id}`)
            .then(r => r.json())
            .then(data => {
                document.getElementById("modalProyectoTitulo").textContent = "Editar Proyecto";
                document.getElementById("proyecto_id").value = data.id;
                document.getElementById("proyecto_nombre").value = data.nombre;
                document.getElementById("proyecto_descripcion").value = data.descripcion;
                cargarLaboratorios();
                cargarResponsables();
                cargarEquipos();
                setTimeout(() => {
                    document.getElementById("proyecto_laboratorio").value = data.laboratorioId;
                    document.getElementById("proyecto_responsable").value = data.responsableId;
                    document.getElementById("proyecto_equipo").value = data.equipoId;
                }, 300);
                document.getElementById("modalProyecto").style.display = "block";
            });
    }

    function eliminarProyecto(id) {
        if (!confirm("¿Seguro que deseas eliminar este proyecto?")) return;
        fetch(`/api/Proyectos/Eliminar?id=${id}`, {
            method: 'DELETE'
        })
            .then(r => r.ok ? r.text() : Promise.reject(r.text()))
            .then(() => recargarTablaProyectos())
            .catch(async err => alert(await err));
    }
</script>
</asp:Content>
