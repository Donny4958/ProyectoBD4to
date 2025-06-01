<%@ Page Title="Nosotros" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="AppWeb.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
    <script>
        if (!sessionStorage.getItem("logueado")) {
            window.location.href = "Default.aspx";
        }
        $(document).ready(function () {
            if (!sessionStorage.getItem("logueado")) {
                window.location.href = "Default.aspx";
            }
            
            buscarInfo();
        });
        async function buscarInfo() {
            const tipo = sessionStorage.getItem("tipo");
            const id = sessionStorage.getItem("ID");

            fetch('/api/Main/ObtenerUsuarioInfo', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ tipo, id })
            })
                .then(response => response.json())
                .then(data => {
                    // Aquí muestras los datos en la página
                    // Ejemplo:
                    if (tipo === "alumno") {
                        document.getElementById("infoUsuario").innerHTML =
                            `<p><b>Nombre:</b> ${data.nombre}</p>
             <p><b>Matrícula:</b> ${data.matricula}</p>
             <img src="data:image/png;base64,${data.foto}" style="max-width:200px;max-height:200px;" />`;
                    } else if (tipo === "empleado") {
                        document.getElementById("infoUsuario").innerHTML =
                            `<p><b>Nombre:</b> ${data.nombre}</p>
             <p><b>Número de empleado:</b> ${data.num_empleado}</p>
             <p><b>Tipo de empleado:</b> ${data.tipo_empleado}</p>
             <img src="data:image/png;base64,${data.foto}" style="max-width:200px;max-height:200px;" />
             <a></a> />
             `;
                    }
                });

        }
        window.onload = async function () {
            
        };
    </script>
    <h2 id="title"><%: Title %>.</h2>
    <h5>Bienvenido</h5>
    <div id="infoUsuario"></div>
</main>

</asp:Content>
