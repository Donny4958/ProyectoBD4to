<%@ Page Title="Dispositivos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<link rel="stylesheet" href="https://cdn.datatables.net/2.3.1/css/dataTables.dataTables.css" />
<script src="https://cdn.datatables.net/2.3.1/js/dataTables.js"></script>
<h2>Dispositivos</h2>

<!-- Filtros -->
<div style="margin-bottom:15px;">
    <label>Laboratorio:
        <select id="filtroLaboratorio" style="min-width:200px;">
            <option value="">Todos</option>
        </select>
    </label>
    <button type="button" onclick="abrirModalDispositivo()" style="margin-left:20px;">Agregar nuevo dispositivo</button>
</div>

<!-- Modal para crear dispositivo -->
<div id="modalDispositivo" class="modal" tabindex="-1" style="display:none; position:fixed; z-index:1050; left:0; top:0; width:100%; height:100%; overflow:auto; background:rgba(0,0,0,0.5);">
    <div style="background:#fff; margin:5% auto; padding:20px; border-radius:8px; width:400px; position:relative;">
        <span style="position:absolute; top:10px; right:15px; cursor:pointer; font-size:20px;" onclick="cerrarModalDispositivo()">&times;</span>
        <h3>Nuevo Dispositivo</h3>
        <!-- Formulario sin etiqueta <form> -->
        <label>RFID: <input type="text" id="rfid" required class="form-control" /></label><br />
        <label>Nombre: <input type="text" id="nombre" required class="form-control" /></label><br />
        <label>Descripción: <input type="text" id="descripcion" class="form-control" /></label><br />
        <label>Foto: <input type="file" id="foto" accept="image/*" onchange="convertirFotoBase64(event)" class="form-control" /></label><br />
        <input type="hidden" id="fotoBase64" />
        <label>Clave Producto: <input type="text" id="claveproducto" class="form-control" /></label><br />
        <label>Fecha Adquisición: <input type="date" id="fechaadquisicion" class="form-control" /></label><br />
        <label>Costo: <input type="number" id="costo" step="0.01" class="form-control" /></label><br />
        <label>Donado: <input type="checkbox" id="donado" /></label><br />
        <label>Proveedor: <input type="text" id="proveedor" class="form-control" /></label><br />
        <label>Laboratorio:
            <select id="id_laboratorio" required class="form-control"></select>
        </label><br />
        <button type="button" class="btn btn-success" onclick="crearDispositivo();">Crear</button>
        <button type="button" class="btn btn-secondary" data-dismiss="modal" onclick="cerrarModalDispositivo();">Close</button>
    </div>
</div>

    <div id="modalImagen" style="display:none; position:fixed; z-index:2000; left:0; top:0; width:100vw; height:100vh; background:rgba(0,0,0,0.7); align-items:center; justify-content:center;">
    <span style="position:absolute; top:20px; right:40px; color:#fff; font-size:40px; cursor:pointer;" onclick="cerrarModalImagen()">&times;</span>
    <img id="imgModalGrande" src="" style="max-width:90vw; max-height:90vh; display:block; margin:auto;" />
</div>

<table id="tablaDispositivos" class="display" style="width:100%">
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
            <th>Laboratorio</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<div id="modalEditar" class="modal" tabindex="-1" style="display:none; position:fixed; z-index:1050; left:0; top:0; width:100%; height:100%; overflow:auto; background:rgba(0,0,0,0.5);">
    <div style="background:#fff; margin:5% auto; padding:20px; border-radius:8px; width:400px; position:relative;">
        <span style="position:absolute; top:10px; right:15px; cursor:pointer; font-size:20px;" onclick="cerrarModalEditar()">&times;</span>
        <h3>Editar Componente</h3>
        <input type="hidden" id="edit_id_dispositivo" />
        <label>Descripción: <input type="text" id="edit_descripcion" class="form-control" /></label><br />
        <label>Foto: <input type="file" id="edit_foto" accept="image/*" onchange="convertirFotoBase64Editar(event)" class="form-control" /></label><br />
        <input type="hidden" id="edit_fotoBase64" />
        <label>Costo: <input type="number" id="edit_costo" step="0.01" class="form-control" /></label><br />
        <button type="button" class="btn btn-primary" onclick="guardarEdicion();">Guardar</button>
        <button type="button" class="btn btn-secondary" onclick="cerrarModalEditar();">Cancelar</button>
    </div>
</div>

<script>
    let tablaDispositivos;
    let laboratorios = [];
    let fotoBase64 = "";
    function mostrarModalImagen(base64) {
        document.getElementById('imgModalGrande').src = 'data:image/png;base64,' + base64;
        document.getElementById('modalImagen').style.display = 'flex';
    }
    function cerrarModalImagen() {
        document.getElementById('modalImagen').style.display = 'none';
        document.getElementById('imgModalGrande').src = '';
    }

    document.addEventListener("DOMContentLoaded", function () {
        cargarLaboratorios();
        inicializarTabla();
        document.getElementById("filtroLaboratorio").addEventListener("change", function () {
            tablaDispositivos.ajax.reload();
        });
    });

    function cargarLaboratorios() {
        fetch('/api/Laboratorios/ObtenerLaboratorios')
            .then(r => r.json())
            .then(data => {
                laboratorios = data;
                const selectFiltro = document.getElementById("filtroLaboratorio");
                const selectModal = document.getElementById("id_laboratorio");
                selectFiltro.innerHTML = '<option value="">Todos</option>';
                selectModal.innerHTML = '<option value="">Seleccione laboratorio</option>';
                data.forEach(lab => {
                    selectFiltro.innerHTML += `<option value="${lab.Id}">${lab.nombre}</option>`;
                    selectModal.innerHTML += `<option value="${lab.Id}">${lab.nombre}</option>`;
                });
            });
    }



    function inicializarTabla() {
        tablaDispositivos = new DataTable('#tablaDispositivos', {
            ajax: {
                url: '/api/Dispositivos/ObtenerTodos',
                dataSrc: function (json) {
                    const filtro = document.getElementById("filtroLaboratorio").value;
                    if (!filtro) return json;
                    return json.filter(d => d.id_laboratorio == filtro);
                }
            },
            columns: [
                { data: 'id_dispositivo' },
                { data: 'rfid' },
                { data: 'nombre' },
                { data: 'descripcion' },
                {
                    data: 'foto',
                    orderable: false,
                    render: function (data) {
                        if (data)
                            return `<img src="data:image/png;base64,${data}" style="max-width:50px;max-height:50px;cursor:pointer;" onclick="mostrarModalImagen('${data}')" />`;
                        return '';
                    }
                },
                { data: 'clave_producto' },
                { data: 'fecha_adquisicion' },
                { data: 'costo' },
                {
                    data: 'donado',
                    render: function (data) {
                        return data === true ? 'Sí' : data === false ? 'No' : '';
                    }
                },
                { data: 'proveedor' },
                {
                    data: 'laboratorio',
                    render: function (data, type, row) {
                        if (data && data.nombre)
                            return `<span title="${data.descripcion || ''}">${data.nombre}</span>`;
                        return '';
                    }
                },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        return `<button class="btn btn-sm btn-warning" onclick="abrirModalEditar(${row.id_dispositivo}, '${encodeURIComponent(row.descripcion || '')}', '${row.foto || ''}', '${row.costo || ''}')">Editar</button>`;
                    }
                }
            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/2.3.1/i18n/es-ES.json'
            }
        });
    }

    // Modal editar
    function abrirModalEditar(id, descripcion, foto, costo) {
        document.getElementById("edit_id_dispositivo").value = id;
        document.getElementById("edit_descripcion").value = decodeURIComponent(descripcion || "");
        document.getElementById("edit_foto").value = "";
        document.getElementById("edit_fotoBase64").value = "";
        document.getElementById("edit_costo").value = costo || "";
        document.getElementById("modalEditar").style.display = "block";
    }
    function cerrarModalEditar() {
        document.getElementById("modalEditar").style.display = "none";
    }
    function convertirFotoBase64Editar(event) {
        const file = event.target.files[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = function (e) {
            const base64 = e.target.result.split(',')[1];
            document.getElementById('edit_fotoBase64').value = base64;
        };
        reader.readAsDataURL(file);
    }
    function guardarEdicion() {
        const id = document.getElementById("edit_id_dispositivo").value;
        const descripcion = document.getElementById("edit_descripcion").value;
        const foto = document.getElementById("edit_fotoBase64").value;
        const costo = document.getElementById("edit_costo").value;

        // Solo enviar los campos que se modifican
        const datos = { id_dispositivo: parseInt(id) };
        if (descripcion !== undefined) datos.descripcion = descripcion;
        if (foto) datos.foto =foto  ;
        if (costo !== "") datos.costo = parseFloat(costo);

        fetch('/api/Dispositivos/ModificarParcial', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(datos)
        })
            .then(response => response.text())
            .then(msg => {
                alert(msg);
                cerrarModalEditar();
                recargarTablaDispositivos();
            });
    }
</script>
</asp:Content>
