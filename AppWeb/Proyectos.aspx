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
        <label>Responsable:</label>
        <select id="proyecto_responsable" class="form-control"></select><br />
        <button type="button" onclick="guardarProyecto()" class="btn btn-success">Guardar</button>
        <button type="button" onclick="cerrarModalProyecto()" class="btn btn-secondary">Cancelar</button>
    </div>
</div>

<!-- Modal para usuarios del proyecto -->
<div id="modalUsuarios" class="modal" tabindex="-1" style="display:none; position:fixed; z-index:1050; left:0; top:0; width:100%; height:100%; overflow:auto; background:rgba(0,0,0,0.5);">
    <div style="background:#fff; margin:5% auto; padding:20px; border-radius:8px; width:400px; position:relative;">
        <span style="position:absolute; top:10px; right:15px; cursor:pointer; font-size:20px;" onclick="cerrarModalUsuarios()">&times;</span>
        <h3>Usuarios del Proyecto</h3>
        <table id="tablaUsuarios" class="display" style="width:100%">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Nombre</th>
                    <th>Correo</th>
                    <th>Rol</th>
                </tr>
            </thead>
            <tbody></tbody>
        </table>
    </div>
</div>

<table id="tablaProyectos" class="display" style="width:100%">
    <thead>
        <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Descripción</th>
            <th>Responsable</th>
            <th>Opciones</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<script>
    let tablaProyectos;

    document.addEventListener("DOMContentLoaded", function () {
        cargarResponsables();
        tablaProyectos = new DataTable('#tablaProyectos', {
            ajax: {
                url: '/api/Proyectos/ObtenerTodos',
                dataSrc: ''
            },
            columns: [
                { data: 'id' },
                { data: 'nombre' },
                { data: 'descripcion' },
                { data: 'responsable' },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                        <button type="button" class="btn btn-primary" onclick="editarProyecto('${row.id}')">Editar</button>
                        <button type="button" class="btn btn-danger" onclick="eliminarProyecto('${row.id}')">Eliminar</button>
                        <button type="button" class="btn btn-info" onclick="verUsuarios('${row.id}')">Usuarios</button>
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

    function cargarResponsables() {
        fetch('/api/Usuarios/ObtenerTodos')
            .then(r => r.json())
            .then(data => {
                const select = document.getElementById("proyecto_responsable");
                select.innerHTML = '<option value="">Seleccione responsable</option>';
                data.forEach(u => {
                    select.innerHTML += `<option value="${u.id_usuario}">${u.Nombre} (${u.rol})</option>`;
                });
            });
    }

    function abrirModalProyecto() {
        document.getElementById("modalProyectoTitulo").textContent = "Nuevo Proyecto";
        document.getElementById("proyecto_id").value = "";
        document.getElementById("proyecto_nombre").value = "";
        document.getElementById("proyecto_descripcion").value = "";
        document.getElementById("proyecto_responsable").value = "";
        cargarResponsables();
        document.getElementById("modalProyecto").style.display = "block";
    }

    function cerrarModalProyecto() {
        document.getElementById("modalProyecto").style.display = "none";
    }

    function guardarProyecto() {
        const id = document.getElementById("proyecto_id").value;
        const nombre = document.getElementById("proyecto_nombre").value;
        const descripcion = document.getElementById("proyecto_descripcion").value;
        const id_responsable = document.getElementById("proyecto_responsable").value;

        if (!nombre || !id_responsable) {
            alert("Todos los campos son obligatorios.");
            return;
        }

        const body = {
            nombre,
            descripcion,
            id_responsable: parseInt(id_responsable)
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
                cargarResponsables();
                setTimeout(() => {
                    document.getElementById("proyecto_responsable").value = data.id_responsable;
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

    // Usuarios por proyecto
    function verUsuarios(idProyecto) {
        fetch(`/api/Proyectos/UsuariosPorProyecto?idProyecto=${idProyecto}`)
            .then(r => r.json())
            .then(data => {
                const tbody = document.querySelector("#tablaUsuarios tbody");
                tbody.innerHTML = "";
                data.forEach(u => {
                    tbody.innerHTML += `<tr>
                        <td>${u.id_usuario}</td>
                        <td>${u.nombre}</td>
                        <td>${u.correo}</td>
                        <td>${u.rol}</td>
                    </tr>`;
                });
                document.getElementById("modalUsuarios").style.display = "block";
            });
    }

    function cerrarModalUsuarios() {
        document.getElementById("modalUsuarios").style.display = "none";
    }
</script>
</asp:Content>
