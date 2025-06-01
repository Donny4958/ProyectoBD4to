<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="Laboratorio.aspx.cs" Inherits="AppWeb.Laboratorio" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<link rel="stylesheet" href="https://cdn.datatables.net/2.3.1/css/dataTables.dataTables.css" />  
<script src="https://cdn.datatables.net/2.3.1/js/dataTables.js"></script>
    <h2>Laboratorios</h2>
    <button type="button" onclick="mostrarFormularioCrear()">Crear nuevo laboratorio</button>
    <div id="formularioLaboratorio" style="display: none; margin-top: 20px;">
        <input type="hidden" id="idLaboratorio" />
        <label>Nombre:</label>
        <input type="text" id="nombreLaboratorio" maxlength="100" /><br />
        <label>Descripción:</label>
        <input type="text" id="descripcionLaboratorio" maxlength="255" /><br />
        <label>Responsable:</label>
        <select id="responsableLaboratorio"></select><br />
        <button type="button" onclick="guardarLaboratorio()">Guardar</button>
        <button type="button" onclick="cancelarEdicion()">Cancelar</button>
    </div>
    <table id="tablaLaboratorios" class="display" style="width:100%">
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
        let tabla;

        document.addEventListener("DOMContentLoaded", function () {
            cargarResponsables();
            tabla = new DataTable('#tablaLaboratorios', {
                ajax: {
                    url: '/api/Laboratorios/ObtenerLaboratorios',
                    dataSrc: ''
                },
                columns: [
                    { data: 'Id' },
                    { data: 'nombre' },
                    { data: 'descripcion' },
                    { data: 'Responsable' },
                    {
                        data: null,
                        orderable: false,
                        render: function (data, type, row) {
                            return `
                                <button type="button" class="btn btn-primary" onclick="guardarLaboratorio(${row.Id})">Editar</button>
                                <button onclick="eliminarLaboratorio('${row.Id}')">Eliminar</button>
                            `;
                        }
                    }
                ],
                language: {
                    url: '//cdn.datatables.net/plug-ins/2.3.1/i18n/es-ES.json'
                }
            });
        });

        function cargarResponsables() {
            fetch('/api/Laboratorios/ObtenerProfesoresOEmpleados')
                .then(r => r.json())
                .then(data => {
                    const select = document.getElementById("responsableLaboratorio");
                    select.innerHTML = '<option value="">Seleccione responsable</option>';
                    data.forEach(u => {
                        select.innerHTML += `<option value="${u.ID}">${u.Nombre} (${u.Rol})</option>`;
                    });
                });
        }

        function recargarTabla() {
            if (tabla) tabla.ajax.reload();
        }

        function mostrarFormularioCrear() {
            document.getElementById("idLaboratorio").value = "";
            document.getElementById("nombreLaboratorio").value = "";
            document.getElementById("descripcionLaboratorio").value = "";
            document.getElementById("responsableLaboratorio").value = "";
            cargarResponsables();
            document.getElementById("formularioLaboratorio").style.display = "block";
        }

        function cancelarEdicion() {
            document.getElementById("formularioLaboratorio").style.display = "none";
        }

        function guardarLaboratorio() {
            const id = document.getElementById("idLaboratorio").value;
            const nombre = document.getElementById("nombreLaboratorio").value;
            const descripcion = document.getElementById("descripcionLaboratorio").value;
            const responsable = document.getElementById("responsableLaboratorio").value;

            if (!nombre || !descripcion || !responsable) {
                alert("Nombre, descripción y responsable son obligatorios.");
                return;
            }

            const body = { nombre, descripcion, responsable };

            if (id) {
                body.id_laboratorio = id; // El backend espera este campo
                // Modificar laboratorio
                fetch('/api/Laboratorios/ModificarLaboratorio', {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(body)
                })
                    .then(r => r.ok ? r.text() : Promise.reject(r.text()))
                    .then(() => {
                        recargarTabla();
                        cancelarEdicion();
                    })
                    .catch(async err => alert(await err));
            } else {
                fetch('/api/Laboratorios/CrearLaboratorio', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(body)
                })
                    .then(r => r.ok ? r.text() : Promise.reject(r.text()))
                    .then(() => {
                        recargarTabla();
                        cancelarEdicion();
                    })
                    .catch(async err => alert(await err));
            }
        }



        function eliminarLaboratorio(id) {
            if (!confirm("¿Seguro que deseas eliminar este laboratorio?")) return;
            fetch(`/api/Laboratorios/EliminarLaboratorio?id=${id}`, {
                method: 'DELETE'
            })
                .then(r => r.ok ? r.text() : Promise.reject(r.text()))
                .then(() => recargarTabla())
                .catch(async err => alert(await err));
        }
    </script>
</asp:Content>
