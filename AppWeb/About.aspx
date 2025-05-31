<%@ Page Title="Nosotros" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="AppWeb.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <script>
    if (!sessionStorage.getItem("logueado")) {
        window.location.href = "Default.aspx";
            }
        </script>
        
        <h2 id="title"><%: Title %>.</h2>        
        <h5>Bienvenido</h5>
    </main>
</asp:Content>
