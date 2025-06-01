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
            const tipo = sessionStorage.getItem("tipo");
            const id = sessionStorage.getItem("ID");
        }
        
    </script>
    <h2 id="title"><%: Title %>.</h2>
    <h5>Bienvenido</h5>
    <div id="infoUsuario"></div>
</main>

</asp:Content>
