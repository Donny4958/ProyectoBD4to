<%@ Page Title="Usuarios" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<link rel="stylesheet" href="https://cdn.datatables.net/2.3.1/css/dataTables.dataTables.css" />  
<script src="https://cdn.datatables.net/2.3.1/js/dataTables.js"></script>
<h2>Usuarios</h2>
<button type="button" onclick="abrirModalUsuario()">Crear nuevo usuario</button>

<!-- Modal para crear/editar usuario -->
<div id="modalUsuario" class="modal" tabindex="-1" style="display:none; position:fixed; z-index:1050; left:0; top:0; width:100%; height:100%; overflow:auto; background:rgba(0,0,0,0.5);">
    <div style="background:#fff; margin:5% auto; padding:20px; border-radius:8px; width:400px; position:relative;">
        <span style="position:absolute; top:10px; right:15px; cursor:pointer; font-size:20px;" onclick="cerrarModalUsuario()">&times;</span>
        <h3 id="modalUsuarioTitulo">Nuevo Usuario</h3>
        <input type="hidden" id="usuario_id" />
        <label>Correo:</label>
        <input type="email" id="usuario_correo" maxlength="255" class="form-control" /><br />
        <label>Rol:</label>
        <input type="text" id="usuario_rol" maxlength="50" class="form-control" /><br />
        <label>Tipo Empleado:</label>
        <select id="usuario_tipoempleado" class="form-control">
            <option value="">Seleccione tipo</option>
            <option value="planta">Planta</option>
            <option value="media_planta">Media Planta</option>
            <option value="auxiliar">Auxiliar</option>
            <option value="otro">Otro</option>
            <option value="alumno">Alumno</option>
        </select><br />
        <label>Matrícula:</label>
        <input type="text" id="usuario_matricula" maxlength="50" class="form-control" /><br />
        <label>Password:</label>
        <input type="password" id="usuario_password" maxlength="255" class="form-control" /><br />
        <label>Puede Autorizar:</label>
        <input type="checkbox" id="usuario_puede_autorizar" /><br />
        <label>Nombre:</label>
        <input type="text" id="usuario_nombre" maxlength="100" class="form-control" /><br />
        <label>Foto:</label>
        <input type="file" id="usuario_foto" accept="image/*" onchange="cargarFotoBase64(event)" /><br />
        <img id="preview_foto" src="" alt="Foto" style="max-width:100px;max-height:100px;display:none;margin-top:5px;" /><br />
        <button type="button" onclick="guardarUsuario()" class="btn btn-success">Guardar</button>
        <button type="button" onclick="cerrarModalUsuario()" class="btn btn-secondary">Cancelar</button>
    </div>
</div>

<table id="tablaUsuarios" class="display" style="width:100%">
    <thead>
        <tr>
            <th>ID</th>
            <th>Correo</th>
            <th>Rol</th>
            <th>Tipo</th>
            <th>Matrícula</th>
            <th>Puede Autorizar</th>
            <th>Nombre</th>
            <th>Foto</th>
            <th>Opciones</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<script>
    let tablaUsuarios;
    let fotoBase64 = "";

    document.addEventListener("DOMContentLoaded", function () {
        tablaUsuarios = new DataTable('#tablaUsuarios', {
            ajax: {
                url: '/api/Usuarios/ObtenerTodos',
                dataSrc: ''
            },
            columns: [
                { data: 'id_usuario' },
                { data: 'correo' },
                { data: 'rol' },
                { data: 'tipoempleado' },
                { data: 'matricula' },
                { 
                    data: 'puede_autorizar',
                    render: function (data) { return data ? "Sí" : "No"; }
                },
                { data: 'Nombre' },
                { 
                    data: 'foto',
                    orderable: false,
                    render: function (data) {
                        if (data)
                            return `<img src="data:image/*;base64,${data}" style="max-width:50px;max-height:50px;" />`;
                        return '';
                    }
                },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        return `
                        <button type="button" class="btn btn-primary" onclick="editarUsuario('${row.id_usuario}')">Editar</button>
                        <button type="button" class="btn btn-danger" onclick="eliminarUsuario('${row.id_usuario}')">Eliminar</button>
                    `;
                    }
                }
            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/2.3.1/i18n/es-ES.json'
            }
        });
    });

    function recargarTablaUsuarios() {
        if (tablaUsuarios) tablaUsuarios.ajax.reload();
    }

    function abrirModalUsuario() {
        document.getElementById("modalUsuarioTitulo").textContent = "Nuevo Usuario";
        document.getElementById("usuario_id").value = "";
        document.getElementById("usuario_correo").value = "";
        document.getElementById("usuario_rol").value = "";
        document.getElementById("usuario_tipoempleado").value = "";
        document.getElementById("usuario_matricula").value = "";
        document.getElementById("usuario_password").value = "";
        document.getElementById("usuario_puede_autorizar").checked = false;
        document.getElementById("usuario_nombre").value = "";
        document.getElementById("usuario_foto").value = "";
        fotoBase64 = "";
        document.getElementById("preview_foto").src = "";
        document.getElementById("preview_foto").style.display = "none";
        document.getElementById("modalUsuario").style.display = "block";
    }

    function cerrarModalUsuario() {
        document.getElementById("modalUsuario").style.display = "none";
    }

    function cargarFotoBase64(event) {
        const file = event.target.files[0];
        if (!file) {
            fotoBase64 = "";
            document.getElementById("preview_foto").src = "";
            document.getElementById("preview_foto").style.display = "none";
            return;
        }
        const reader = new FileReader();
        reader.onload = function (e) {
            const base64 = e.target.result.split(',')[1];
            fotoBase64 = base64;
            document.getElementById("preview_foto").src = e.target.result;
            document.getElementById("preview_foto").style.display = "block";
        };
        reader.readAsDataURL(file);
    }

    function guardarUsuario() {
        const id = document.getElementById("usuario_id").value;
        const correo = document.getElementById("usuario_correo").value;
        const rol = document.getElementById("usuario_rol").value;
        const tipoempleado = document.getElementById("usuario_tipoempleado").value;
        const matricula = document.getElementById("usuario_matricula").value;
        const password = document.getElementById("usuario_password").value;
        const puede_autorizar = document.getElementById("usuario_puede_autorizar").checked;
        const nombre = document.getElementById("usuario_nombre").value;
        const foto = fotoBase64;

        if (!correo || !tipoempleado || (!id && !password)) {
            alert("Correo, tipo de empleado y contraseña (al crear) son obligatorios.");
            return;
        }

        const body = {
            correo,
            rol,
            tipoempleado,
            matricula,
            password,
            puede_autorizar,
            Nombre: nombre,
            foto
        };

        if (id) {
            body.id_usuario = parseInt(id);
            fetch('/api/Usuarios/Modificar', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            })
                .then(r => r.ok ? r.text() : Promise.reject(r.text()))
                .then(() => {
                    recargarTablaUsuarios();
                    cerrarModalUsuario();
                })
                .catch(async err => alert(await err));
        } else {
            fetch('/api/Usuarios/Crear', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            })
                .then(r => r.ok ? r.text() : Promise.reject(r.text()))
                .then(() => {
                    recargarTablaUsuarios();
                    cerrarModalUsuario();
                })
                .catch(async err => alert(await err));
        }
    }

    function editarUsuario(id) {
        fetch(`/api/Usuarios/ObtenerPorId?id=${id}`)
            .then(r => r.json())
            .then(data => {
                document.getElementById("modalUsuarioTitulo").textContent = "Editar Usuario";
                document.getElementById("usuario_id").value = data.id_usuario;
                document.getElementById("usuario_correo").value = data.correo;
                document.getElementById("usuario_rol").value = data.rol;
                document.getElementById("usuario_tipoempleado").value = data.tipoempleado;
                document.getElementById("usuario_matricula").value = data.matricula;
                document.getElementById("usuario_password").value = "";
                document.getElementById("usuario_puede_autorizar").checked = data.puede_autorizar;
                document.getElementById("usuario_nombre").value = data.Nombre;
                fotoBase64 = data.foto || "";
                if (fotoBase64) {
                    document.getElementById("preview_foto").src = "data:image/*;base64," + fotoBase64;
                    document.getElementById("preview_foto").style.display = "block";
                } else {
                    document.getElementById("preview_foto").src = "";
                    document.getElementById("preview_foto").style.display = "none";
                }
                document.getElementById("modalUsuario").style.display = "block";
            });
    }

    function eliminarUsuario(id) {
        if (!confirm("¿Seguro que deseas eliminar este usuario?")) return;
        fetch(`/api/Usuarios/Eliminar?id=${id}`, {
            method: 'DELETE'
        })
            .then(r => r.ok ? r.text() : Promise.reject(r.text()))
            .then(() => recargarTablaUsuarios())
            .catch(async err => alert(await err));
    }
</script>
</asp:Content>
