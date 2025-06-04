<%@ Page Title="Alertas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
<link rel="stylesheet" href="https://cdn.datatables.net/2.3.1/css/dataTables.dataTables.css" />
<script src="https://cdn.datatables.net/2.3.1/js/dataTables.js"></script>

<h2>Alertas</h2>

<!-- Selector de tipo de alerta -->
<select id="tipoAlerta" onchange="cargarAlertas()">
    <option value="Prestamos">Alertas de Préstamos</option>
    <option value="SinAutorizacion">Alertas sin Autorización</option>
</select>

<!-- Tabla de resultados -->
<table id="tablaResultados" class="display" style="width:100%">
    <thead>
        <tr id="theadResultados"></tr>
    </thead>
    <tbody></tbody>
</table>
<div id="modalImagen" style="display:none; position:fixed; z-index:2000; left:0; top:0; width:100vw; height:100vh; background:rgba(0,0,0,0.7); align-items:center; justify-content:center;">
    <span style="position:absolute; top:20px; right:40px; color:#fff; font-size:40px; cursor:pointer;" onclick="cerrarModalImagen()">&times;</span>
    <img id="imgModalGrande" src="" style="max-width:90vw; max-height:90vh; display:block; margin:auto;" />
</div>
<script>
    let tablaResultados;
    function mostrarModalImagen(base64) {
        document.getElementById('imgModalGrande').src = 'data:image/png;base64,' + base64;
        document.getElementById('modalImagen').style.display = 'flex';
    }
    function cerrarModalImagen() {
        document.getElementById('modalImagen').style.display = 'none';
        document.getElementById('imgModalGrande').src = '';
    }
    document.addEventListener("DOMContentLoaded", function () {
        cargarAlertas();
    });

    function inicializarTabla(columnas, data) {
        if (tablaResultados) {
            tablaResultados.destroy();
            document.querySelector("#tablaResultados tbody").innerHTML = "";
        }
        // Encabezados
        const thead = document.getElementById("theadResultados");
        thead.innerHTML = "";
        columnas.forEach(col => {
            const th = document.createElement("th");
            th.textContent = col.title;
            thead.appendChild(th);
        });
        // DataTable
        tablaResultados = new DataTable('#tablaResultados', {
            data: data,
            columns: columnas,
            language: { url: '//cdn.datatables.net/plug-ins/2.3.1/i18n/es-ES.json' }
        });
    }

    function cargarAlertas() {
        const tipo = document.getElementById('tipoAlerta').value;
        let url = '/api/Alertas/' + tipo;
        fetch(url)
            .then(r => r.json())
            .then(data => {
                // Ajusta las columnas según el endpoint
                let columnas = [
                    { data: 'id_alerta', title: 'ID Alerta' },
                    { data: 'momento', title: 'Fecha-Hora' },
                    { data: 'ubicacion', title: 'Ubicación' },
                    { data: 'rfid', title: 'RFID' },
                    { data: 'dispositivo.nombre', title: 'Nombre Dispositivo' },
                    { data: 'dispositivo.descripcion', title: 'Descripción' },
                    {
                        data: 'dispositivo.foto', title: 'Foto', render: function (data) {
                            if (data)
                                return `<img src="data:image/png;base64,${data}" style="max-width:50px;max-height:50px;cursor:pointer;" onclick="mostrarModalImagen('${data}')" />`;
                            return '';
                        }
                    },
                    { data: 'dispositivo.clave_producto', title: 'Clave Producto' },
                    { data: 'dispositivo.fecha_adquisicion', title: 'Fecha Adquisición' },
                    { data: 'dispositivo.costo', title: 'Costo' },
                    { data: 'dispositivo.donado', title: 'Donado' },
                    { data: 'dispositivo.proveedor', title: 'Proveedor' },
                    { data: 'dispositivo.id_laboratorio', title: 'ID Laboratorio' }
                ];
                // Si es SinAutorizacion, el nombre del dispositivo es 'nombre' directamente
                if (tipo === "SinAutorizacion") {
                    columnas[4] = { data: 'dispositivo.nombre', title: 'Nombre Dispositivo' };
                }
                inicializarTabla(columnas, data);
            });
    }
</script>
</asp:Content>
