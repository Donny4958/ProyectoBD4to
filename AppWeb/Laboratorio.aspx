<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Laboratorio.aspx.cs" Inherits="AppWeb.Laboratorio" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        </div>
    </form>
</body>
</html>
<div>
    <h2>Laboratorios</h2>
    <button type="button" onclick="mostrarFormularioCrear()">Crear nuevo laboratorio</button>
    <div id="formularioLaboratorio" style="display:none; margin-top:20px;">
        <input type="hidden" id="idLaboratorio" />
        <label>Nombre:</label>
        <input type="text" id="nombreLaboratorio" maxlength="100" /><br />
        <label>Descripción:</label>
        <input type="text" id="descripcionLaboratorio" maxlength="255" /><br />
        <button type="button" onclick="guardarLaboratorio()">Guardar</button>
        <button type="button" onclick="cancelarEdicion()">Cancelar</button>
    </div>
    <table border="1" style="margin-top:20px; width:100%;">
        <thead>
            <tr>
                <th>ID</th>
                <th>Nombre</th>
                <th>Descripción</th>
                <th>Opciones</th>
            </tr>
        </thead>
        <tbody id="tablaLaboratorios"></tbody>
    </table>
</div>
<script>
    document.addEventListener("DOMContentLoaded", cargarLaboratorios);

    function cargarLaboratorios() {
        fetch('/api/Laboratorios/ObtenerLaboratorios')
            .then(r => r.json())
            .then(data => {
                const tbody = document.getElementById("tablaLaboratorios");
                tbody.innerHTML = "";
                data.forEach(lab => {
                    tbody.innerHTML += `
                        <tr>
                            <td>${lab.id_laboratorio}</td>
                            <td>${lab.nombre}</td>
                            <td>${lab.descripcion}</td>
                            <td>
                                <button onclick="editarLaboratorio('${lab.id_laboratorio}', '${lab.nombre}', '${lab.descripcion}')">Editar</button>
                                <button onclick="eliminarLaboratorio('${lab.id_laboratorio}')">Eliminar</button>
                            </td>
                        </tr>`;
                });
            });
    }

    function mostrarFormularioCrear() {
        document.getElementById("idLaboratorio").value = "";
        document.getElementById("nombreLaboratorio").value = "";
        document.getElementById("descripcionLaboratorio").value = "";
        document.getElementById("formularioLaboratorio").style.display = "block";
    }

    function cancelarEdicion() {
        document.getElementById("formularioLaboratorio").style.display = "none";
    }

    function guardarLaboratorio() {
        const id = document.getElementById("idLaboratorio").value;
        const nombre = document.getElementById("nombreLaboratorio").value;
        const descripcion = document.getElementById("descripcionLaboratorio").value;

        if (!nombre || !descripcion) {
            alert("Nombre y descripción son obligatorios.");
            return;
        }

        if (id) {
            // Modificar laboratorio
            fetch('/api/Laboratorios/ModificarLaboratorio', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id_laboratorio: id, nombre, descripcion })
            })
            .then(r => r.ok ? r.text() : Promise.reject(r.text()))
            .then(() => {
                cargarLaboratorios();
                cancelarEdicion();
            })
            .catch(async err => alert(await err));
        } else {
            // Crear laboratorio
            fetch('/api/Laboratorios/CrearLaboratorio', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ nombre, descripcion })
            })
            .then(r => r.ok ? r.text() : Promise.reject(r.text()))
            .then(() => {
                cargarLaboratorios();
                cancelarEdicion();
            })
            .catch(async err => alert(await err));
        }
    }

    function editarLaboratorio(id, nombre, descripcion) {
        document.getElementById("idLaboratorio").value = id;
        document.getElementById("nombreLaboratorio").value = nombre;
        document.getElementById("descripcionLaboratorio").value = descripcion;
        document.getElementById("formularioLaboratorio").style.display = "block";
    }

    function eliminarLaboratorio(id) {
        if (!confirm("¿Seguro que deseas eliminar este laboratorio?")) return;
        fetch(`/api/Laboratorios/EliminarLaboratorio?id=${id}`, {
            method: 'DELETE'
        })
        .then(r => r.ok ? r.text() : Promise.reject(r.text()))
        .then(() => cargarLaboratorios())
        .catch(async err => alert(await err));
    }
</script>
