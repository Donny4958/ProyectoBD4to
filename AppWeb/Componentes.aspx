<%@ Page Title="Dispositivos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<h2>Dispositivos</h2>

<!-- Formulario para agregar dispositivo -->
<div>
    <h4>Agregar nuevo dispositivo</h4>
    <form id="formNuevoDispositivo" onsubmit="return crearDispositivo();">
        <label>RFID: <input type="text" id="rfid" required /></label><br />
        <label>Nombre: <input type="text" id="nombre" required /></label><br />
        <label>Descripción: <input type="text" id="descripcion" /></label><br />
        <label>Foto: <input type="file" id="foto" accept="image/*" onchange="convertirFotoBase64(event)" /></label><br />
        <input type="hidden" id="fotoBase64" />
        <label>Clave Producto: <input type="text" id="claveproducto" /></label><br />
        <label>Fecha Adquisición: <input type="date" id="fechaadquisicion" /></label><br />
        <label>Costo: <input type="number" id="costo" step="0.01" /></label><br />
        <label>Donado: <input type="checkbox" id="donado" /></label><br />
        <label>Proveedor: <input type="text" id="proveedor" /></label><br />
        <label>ID Laboratorio: <input type="number" id="id_laboratorio" /></label><br />
        <button type="submit">Crear</button>
    </form>
</div>

<hr />

<!-- Tabla de dispositivos -->
<div>
    <h4>Lista de dispositivos</h4>
    <table border="1" id="tablaDispositivos">
        <thead>
            <tr>
                <th>ID</th>
                <th>RFID</th>
                <th>Nombre</th>
                <th>Descripción</th>
                <th>Foto</th>
                <th>Clave Producto</th>
                <th>Fecha Adquisición</th>
                <th>Costo</th>
                <th>Donado</th>
                <th>Proveedor</th>
                <th>ID Laboratorio</th>
            </tr>
        </thead>
        <tbody>
        </tbody>
    </table>
</div>

<script type="text/javascript">
// Cargar dispositivos al iniciar
    window.onload = function () {

    cargarDispositivos();
};

function cargarDispositivos() {
    fetch('/api/Dispositivos/ObtenerTodos')
        .then(response => response.json())
        .then(data => {
            const tbody = document.getElementById('tablaDispositivos').getElementsByTagName('tbody')[0];
            tbody.innerHTML = '';
            data.forEach(d => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${d.id_dispositivo}</td>
                    <td>${d.rfid}</td>
                    <td>${d.nombre}</td>
                    <td>${d.descripcion || ''}</td>
                    <td>${d.foto ? `<img src="data:image/png;base64,${d.foto}" style="max-width:80px;max-height:80px;" />` : ''}</td>
                    <td>${d.claveproducto || ''}</td>
                    <td>${d.fechaadquisicion || ''}</td>
                    <td>${d.costo != null ? d.costo : ''}</td>
                    <td>${d.donado === true ? 'Sí' : d.donado === false ? 'No' : ''}</td>
                    <td>${d.proveedor || ''}</td>
                    <td>${d.id_laboratorio != null ? d.id_laboratorio : ''}</td>
                `;
                tbody.appendChild(tr);
            });
        });
}

// Convertir imagen a base64
function convertirFotoBase64(event) {
    const file = event.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = function(e) {
        // Quitar el prefijo data:image/...;base64,
        const base64 = e.target.result.split(',')[1];
        document.getElementById('fotoBase64').value = base64;
    };
    reader.readAsDataURL(file);
}

// Crear dispositivo
function crearDispositivo() {
    const datos = {
        rfid: document.getElementById('rfid').value,
        nombre: document.getElementById('nombre').value,
        descripcion: document.getElementById('descripcion').value,
        foto: document.getElementById('fotoBase64').value,
        claveproducto: document.getElementById('claveproducto').value,
        fechaadquisicion: document.getElementById('fechaadquisicion').value,
        costo: document.getElementById('costo').value ? parseFloat(document.getElementById('costo').value) : null,
        donado: document.getElementById('donado').checked,
        proveedor: document.getElementById('proveedor').value,
        id_laboratorio: document.getElementById('id_laboratorio').value ? parseInt(document.getElementById('id_laboratorio').value) : null
    };
    fetch('/api/Dispositivos/Crear', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(datos)
    })
    .then(response => response.text())
    .then(msg => {
        alert(msg);
        cargarDispositivos();
        document.getElementById('formNuevoDispositivo').reset();
        document.getElementById('fotoBase64').value = '';
    });
    return false; // Evita recarga de página
}
</script>
</asp:Content>
