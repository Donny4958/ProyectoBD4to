<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AppWeb._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    
    <script>             
        if (sessionStorage.getItem("logueado")) {
            window.location.href = "About.aspx";
        }

        const url = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;
        function probarConexion() {
            $.ajax({
                url: `/api/Main/probarConexion`,
                type: 'GET',
                success: function (data) {
                    data = JSON.parse(data);
                    console.log("Respuesta de la API:", data);
                },
                error: function (xhr, status, error) {
                    console.error("Error al llamar a la API:", error);
                    alert("Ocurrió un error al llamar a la API.");
                }
            });
        }

        async function login() {
            //  
            let datos = {}
            datos.Correo = document.getElementById("correo").value;            
            datos.Password = document.getElementById("password").value;

            const response = await fetch('/api/Main/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(datos)
            });

            if (response.ok) {
                const data = await response.json();
                sessionStorage.setItem("logueado", data.logueado ? "1" : "0");
                sessionStorage.setItem("tipo", data.tipo);
                sessionStorage.setItem("ID", data.ID);
                sessionStorage.setItem("nombre", data.nombre);
                window.location.reload();
                alert("Bienvenido");
                window.location.href = "About.aspx";
            } else {
                alert("Datos incorrectos");
            }

        }


    </script>
    <main>
        <div class="row d-flex justify-content-center">
            <div class="row">
                        <div class="row d-flex justify-content-center">
                            <div class="col-md-4 d-flex justify-content-center">
                                <div class="row">
                                    <div class="col-md-12" id="CamposCorreo" ">
                                        <span>Correo</span>
                                        <input type="text" id="correo" />
                                    </div>
                                    <div class="col-md-12" id="CamposPassword" ">
                                        <span>Contraseña</span>
                                        <input type="password" id="password" />
                                    </div>
                                    <div class="col-md-12">
                                        <button type="button" class="btn btn-primary" onclick="login()">Login</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                </div>
            </div>
    </main>
</asp:Content>
