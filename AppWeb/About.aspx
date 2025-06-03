<%@ Page Title="Menu" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="AppWeb.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
    <script>
        $(document).ready(function () {
            if (!sessionStorage.getItem("logueado")) {
                window.location.href = "Default.aspx";
            } else {
                buscarInfo();
            }
        });

        async function buscarInfo() {
            const id = sessionStorage.getItem("ID");
            if (!id) return;
            try {
                const resp = await fetch(`/api/Usuarios/ObtenerPorId?id=${id}`);
                if (!resp.ok) {
                    $("#infoUsuario").html("<div class='alert alert-danger'>No se pudo obtener la información del usuario.</div>");
                    return;
                }
                const data = await resp.json();
                let html = `
                <div class="card" style="max-width:90%;">
                    <div class="card-body">
                        <h4 class="card-title">${data.Nombre || ""}</h4>
                        <p><strong>Correo:</strong> ${data.correo || ""}</p>
                        <p><strong>Rol:</strong> ${data.rol || ""}</p>
                        <p><strong>Tipo empleado:</strong> ${data.tipoempleado || ""}</p>
                        <p><strong>Matrícula:</strong> ${data.matricula || ""}</p>
                        <p><strong>Puede autorizar:</strong> ${data.puede_autorizar ? "Sí" : "No"}</p>
                        ${data.foto ? `<img src="data:image/*;base64,${data.foto}" alt="Foto" style="max-width:500px;max-height:500px;border-radius:8px;" />` : ""}
                    </div>
                </div>
            `;
                $("#infoUsuario").html(html);
            } catch (e) {
                $("#infoUsuario").html("<div class='alert alert-danger'>Error al obtener los datos del usuario.</div>");
            }
        }
</script>

    <h2 id="title"><%: Title %>.</h2>
    <h5>Bienvenido</h5>
    <div id="infoUsuario"></div>
</main>

</asp:Content>
