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
            const tipo = document.getElementById("TipoLogin").value;
            let datos = { tipo };

            if (tipo === "alumno") {
                datos.matricula = document.getElementById("matriculaLogin").value;
            } else if (tipo === "empleado") {
                datos.numEmpleado = document.getElementById("numEmpleadoLogin").value;
                datos.correo = document.getElementById("correo").value;
            } else {
                alert("Selecciona un tipo de usuario");
                return;
            }
            datos.password = document.getElementById("password").value;

            const response = await fetch('/api/Main/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(datos)
            });

            if (response.ok) {
                const data = await response.json();
                sessionStorage.setItem("logueado", data.logueado ? "1" : "0");
                sessionStorage.setItem("tipo", data.tipo);
                sessionStorage.setItem("nombre", data.nombre);
                window.location.reload();
                alert("Bienvenido");
                window.location.href = "About.aspx";
            } else {
                alert("Datos incorrectos");
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
                body: { palabra, Tipo }
            });
            if (response.ok) {
                const data = await response.json();
                console.log(data)
                // Puedes guardar el token si tu API lo devuelve
            }
        }
        function mostrarCampos() {
            const tipo = document.getElementById("TipoUsuario").value;
            document.getElementById("CamposNombre").style.display = tipo ? "block" : "none";
            document.getElementById("CamposMatricula").style.display = tipo === "alumno" ? "block" : "none";
            document.getElementById("CamposEmpleado").style.display = tipo === "empleado" ? "block" : "none";
            document.getElementById("CamposFoto").style.display = tipo ? "block" : "none";
            document.getElementById("CamposPasswordRegistro").style.display = tipo ? "block" : "none";
            document.getElementById("CamposConfirmarPassword").style.display = tipo ? "block" : "none";
        }


        // Convierte la imagen seleccionada a base64
        function getBase64FromFile(input) {
            return new Promise((resolve, reject) => {
                const file = input.files[0];
                if (!file) {
                    resolve("");
                    return;
                }
                const reader = new FileReader();
                reader.onload = function (e) {
                    resolve(e.target.result.split(',')[1]); // Solo la parte base64
                };
                reader.onerror = function (e) {
                    reject(e);
                };
                reader.readAsDataURL(file);
            });
        }

        async function registrarPersona() {
            const tipo = document.getElementById("TipoUsuario").value;
            const nombre = document.getElementById("NombreMandar").value;
            const fotoInput = document.getElementById("FotoMandar");
            const fotoBase64 = await getBase64FromFile(fotoInput);

            const password = document.getElementById("PasswordRegistro").value;
            const confirmarPassword = document.getElementById("ConfirmarPasswordRegistro").value;

            if (password !== confirmarPassword) {
                alert("Las contraseñas no coinciden");
                return;
            }

            let datos = { TipoUsuario: tipo, Nombre: nombre, Foto: fotoBase64, Password: password };

            if (tipo === "alumno") {
                datos.Matricula = document.getElementById("MatriculaMandar").value;
            } else if (tipo === "empleado") {
                datos.NumEmpleado = document.getElementById("NumEmpleadoMandar").value;
                datos.TipoEmpleado = document.getElementById("TipoEmpleadoMandar").value;
            } else {
                alert("Selecciona un tipo de usuario");
                return;
            }

            fetch('/api/Main/RegistrarPersona', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(datos)
            })
                .then(response => response.json())
                .then(data => {
                    if (data.Codigo === 200 || data === "Alumno registrado" || data === "Empleado registrado") {
                        alert("Registro exitoso");
                    } else {
                        alert(data.Mensaje || data);
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        }

        function mostrarCamposLogin() {
            const tipo = document.getElementById("TipoLogin").value;
            document.getElementById("CamposCorreo").style.display = tipo === "alumno" ? "none" : (tipo ? "block" : "none");
            document.getElementById("CamposMatriculaLogin").style.display = tipo === "alumno" ? "block" : "none";
            document.getElementById("CamposNumEmpleadoLogin").style.display = tipo === "empleado" ? "block" : "none";
            document.getElementById("CamposPassword").style.display = tipo ? "block" : "none";
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
                                        <span>¿Accederás como?</span>
                                    </div>
                                    <div class="col-md-12">
                                        <select id="TipoLogin" onchange="mostrarCamposLogin()">
                                            <option value="">Selecciona...</option>
                                            <option value="alumno">Alumno</option>
                                            <option value="empleado">Profesor</option>
                                        </select>
                                    </div>
                                    <div class="col-md-12" id="CamposCorreo" style="display: none;">
                                        <span>Correo</span>
                                        <input type="text" id="correo" />
                                    </div>
                                    <div class="col-md-12" id="CamposMatriculaLogin" style="display: none;">
                                        <span>Matrícula</span>
                                        <input type="text" id="matriculaLogin" />
                                    </div>
                                    <div class="col-md-12" id="CamposNumEmpleadoLogin" style="display: none;">
                                        <span>Número de empleado</span>
                                        <input type="text" id="numEmpleadoLogin" />
                                    </div>
                                    <div class="col-md-12" id="CamposPassword" style="display: none;">
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

                    <div class="tab-pane fade" id="pills-registrar" role="tabpanel" aria-labelledby="pills-registrar-tab">
                        <div class="row d-flex justify-content-center">
                            <div class="col-md-4 d-flex justify-content-center">
                                <div class="row">
                                    <div class="col-md-12">
                                        <span>Tipo de usuario</span>
                                    </div>
                                    <div class="col-md-12">
                                        <select id="TipoUsuario" onchange="mostrarCampos()">
                                            <option value="">Selecciona...</option>
                                            <option value="alumno">Alumno</option>
                                            <option value="empleado">Empleado</option>
                                        </select>
                                    </div>
                                    <div class="col-md-12" id="CamposNombre" style="display: none;">
                                        <span>Nombre</span>
                                        <input type="text" id="NombreMandar" />
                                    </div>
                                    <div class="col-md-12" id="CamposMatricula" style="display: none;">
                                        <span>Matrícula</span>
                                        <input type="text" id="MatriculaMandar" />
                                    </div>
                                    <div class="col-md-12" id="CamposEmpleado" style="display: none;">
                                        <span>Número de empleado</span>
                                        <input type="text" id="NumEmpleadoMandar" />
                                        <span>Tipo de empleado</span>
                                        <input type="text" id="TipoEmpleadoMandar" />
                                    </div>
                                    <div class="col-md-12" id="CamposFoto" style="display: none;">
                                        <span>Foto</span>
                                        <input type="file" id="FotoMandar" accept="image/*" />
                                    </div>
                                    <div class="col-md-12" id="CamposPasswordRegistro" style="display: none;">
                                        <span>Contraseña</span>
                                        <input type="password" id="PasswordRegistro" maxlength="50" />
                                    </div>
                                    <div class="col-md-12" id="CamposConfirmarPassword" style="display: none;">
                                        <span>Confirmar contraseña</span>
                                        <input type="password" id="ConfirmarPasswordRegistro" maxlength="50" />
                                    </div>
                                    <div class="col-md-12">
                                        <button type="button" class="btn btn-success" onclick="registrarPersona()">Registrar</button>
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
