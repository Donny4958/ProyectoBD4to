<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AppWeb._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
        integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="
        crossorigin="" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
        integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="
        crossorigin=""></script>

    <script>             
        $(document).ready(function () {

        });

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
            const correo = document.getElementById("correo").value;
            const password = document.getElementById("password").value;

            const response = await fetch('/api/Main/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ correo, password })
            });

            if (response.ok) {
                const data = await response.json();
                console.log(response)
                alert("Bienvenido");
                // Puedes guardar el token si tu API lo devuelve
            } else {
                alert("Correo o contraseña incorrectos");
            }
        }

        async function registrar() {
            const Nombre = document.getElementById("NombreMandar").value;
            const Correo = document.getElementById("CorreoMandar").value;
            const Contrasena = document.getElementById("ContrasenaMandar").value;
            const Moneda = document.getElementById("MonedaMandar").value;
            const Lenguaje = "ESP";
            const response = await fetch('/api/Main/Registrar', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Nombre, Correo, Contrasena, Moneda, Lenguaje })
            }).then(response => response.json())
                .then(data => {
                    console.log(data)
                    if (data.Codigo === 200) {
                        alert("Registro exitoso");
                    } else {
                        alert(data);
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        }
        async function regex(palabra, Tipo) {
            palabra = palabra.toString(),
                Tipo = Number(Tipo);
            const response = await fetch('/api/Main/VerificarRegex', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: {palabra,Tipo  }
            });
            if (response.ok) {
                const data = await response.json();
                console.log(data)
                // Puedes guardar el token si tu API lo devuelve
            }
        }

    </script>
    <main>
        <div class="row d-flex justify-content-center">
            <div class="col-md-12">
                <ul class="nav nav-pills mb-3" id="pills-tab" role="tablist">
                    <li class="nav-item" role="presentation">
                        <button class="nav-link active" id="pills-login-tab" data-bs-toggle="pill" data-bs-target="#pills-login" type="button" role="tab" aria-controls="pills-login" aria-selected="true">Login</button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="pills-registrar-tab" data-bs-toggle="pill" data-bs-target="#pills-registrar" type="button" role="tab" aria-controls="pills-profile" aria-selected="false">Registrar</button>
                    </li>
                </ul>

            </div>
            <div class="row">
                <div class="tab-content" id="pills-tabContent">
                    <div class="tab-pane fade show active" id="pills-login" role="tabpanel" aria-labelledby="pills-login-tab">
                        <div class="row d-flex justify-content-center">
                            <div class="col-md-4 d-flex justify-content-center">
                                <div class="row">
                                    <div class="col-md-12">
                                        <span>Usuario</span>
                                    </div>
                                    <div class="col-md-12">
                                        <input type="text" id="correo" />
                                    </div>
                                    <div class="col-md-12">
                                        <span>Contraseña</span>
                                    </div>
                                    <div class="col-md-12">
                                        <input type="text" id="password" />
                                    </div>
                                    <div class="col-md-12">
                                        <button type="button" class="btn btn-primary" onclick="login()">Login</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="tab-pane fade" id="pills-registrar" role="tabpanel" aria-labelledby="pills-registrar-tab">
                        <div class="row d-flex justify-content-center">
                            <div class="col-md-4 d-flex justify-content-center">
                                <div class="row">
                                    <div class="col-md-12">
                                        <span>Usuario</span>
                                    </div>
                                    <div class="col-md-12">
                                        <input type="text" id="NombreMandar" />
                                    </div>
                                    <div class="col-md-12">
                                        <span>Correo</span>
                                    </div>
                                    <div class="col-md-12">
                                        <input type="text" id="CorreoMandar" />
                                    </div>
                                    <div class="col-md-12">
                                        <span>Contrasena</span>
                                    </div>
                                    <div class="col-md-12">
                                        <input type="text" id="ContrasenaMandar" />
                                    </div>
                                    <div class="col-md-12">
                                        <span>Moneda</span>
                                    </div>
                                    <div class="col-md-12">
                                        <input type="text" id="MonedaMandar" />
                                    </div>
                                    <div class="col-md-12">
                                        <button type="button" class="btn btn-success" onclick="registrar()">Registrar</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</asp:Content>
