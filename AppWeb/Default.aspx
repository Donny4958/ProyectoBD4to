<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AppWeb._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Bootstrap 5 CSS (si no está en el MasterPage) -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" 
          rel="stylesheet" 
          integrity="sha384-ENjdO4Dr2bkBIFxQpeo8X3eNLzw4Oebt7awO2RDlV/n6xzK1Qe16HRZZ+8NHWojF" 
          crossorigin="anonymous" />
    <!-- Bootstrap Icons (opcional, para íconos en los inputs) -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css" 
          rel="stylesheet" />

    <!-- jQuery (ya lo usas para probar conexión, etc.) -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"
            integrity="sha256-/xUj+3OJ+Y4t/6U9yqCWR1TI4FJDrTzvZ9Gq0UI5Koc="
            crossorigin="anonymous"></script>
    <!-- Bootstrap 5 JS (opcional, si usas componentes JS de Bootstrap) -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js" 
            integrity="sha384-qWvrDdpvY0EyvMtyM2Q0ywIsTjU8E+vbPHjhy3f9Mj1mFRTsBIZOi27IcX8rl9cN" 
            crossorigin="anonymous"></script>

    <!-- ============================= -->
    <!-- SCRIPT de lógica de Login -->
    <!-- ============================= -->
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            // Si ya está logueado, redirigir a About.aspx
            if (sessionStorage.getItem("logueado") === "1") {
                window.location.href = "About.aspx";
                return;
            }

            // Referencias a elementos del DOM
            const inputCorreo = document.getElementById("correo");
            const inputPassword = document.getElementById("password");
            const btnLogin = document.getElementById("btnLogin");

            // Validar inputs en tiempo real
            function validarCampos() {
                const correoValido = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(inputCorreo.value.trim());
                const passValida = inputPassword.value.trim().length >= 6;

                // Aplica clases de validación
                if (correoValido) {
                    inputCorreo.classList.remove("is-invalid");
                    inputCorreo.classList.add("is-valid");
                } else {
                    inputCorreo.classList.remove("is-valid");
                    if (inputCorreo.value.trim() !== "") {
                        inputCorreo.classList.add("is-invalid");
                    } else {
                        inputCorreo.classList.remove("is-invalid");
                    }
                }

                if (passValida) {
                    inputPassword.classList.remove("is-invalid");
                    inputPassword.classList.add("is-valid");
                } else {
                    inputPassword.classList.remove("is-valid");
                    if (inputPassword.value.trim() !== "") {
                        inputPassword.classList.add("is-invalid");
                    } else {
                        inputPassword.classList.remove("is-invalid");
                    }
                }

                // Habilitar sólo si ambos son válidos
                btnLogin.disabled = !(correoValido && passValida);
            }

            inputCorreo.addEventListener("input", validarCampos);
            inputPassword.addEventListener("input", validarCampos);

            // Función login() llamando a la API
            window.login = async function () {
                const datos = {
                    Correo: inputCorreo.value.trim(),
                    Password: inputPassword.value.trim()
                };

                try {
                    btnLogin.disabled = true;
                    btnLogin.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Iniciando...`;

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

                        // Mostrar breve mensaje y redirigir
                        alert("Bienvenido, " + data.nombre + "!");
                        window.location.href = "About.aspx";
                    } else {
                        alert("Correo o contraseña incorrectos.");
                        inputPassword.value = "";
                        inputPassword.focus();
                    }
                } catch (error) {
                    console.error("Error al llamar la API:", error);
                    alert("Ocurrió un error de conexión. Intenta de nuevo.");
                } finally {
                    btnLogin.disabled = false;
                    btnLogin.textContent = "Iniciar Sesión";
                }
            };
        });
    </script>

                <main class="container py-5">
              <div class="row justify-content-center">
                <div class="col-12 col-sm-10 col-md-8 col-lg-6">

                  <!-- Logo centrado encima de la tarjeta -->
                  <div class="text-center mb-4">
                    <asp:Image
                      ID="Image1"
                      runat="server"
                      ImageUrl="~/Images/CEID.jpg"
                      CssClass="img-fluid logo-custom"
                      AlternateText="Logo CEID"
                    />
                  </div>

                  <!-- Card de Login centrado -->
                  <div class="card shadow border-0 rounded-4">
                    <!-- Header con fondo primario y texto centrado -->
                    <div class="card-header bg-primary text-white text-center py-3">
                      <h3 class="mb-0 fw-bold text-uppercase">
                        <i class="bi bi-shield-lock-fill me-2"></i>
                        Sistema de Seguridad y Control de Equipamiento CEID
                      </h3>
                    </div>
                    <div class="card-body p-5">
                      <!-- Instrucción breve (opcional) -->
                      <p class="text-center text-muted mb-4">
                        Ingresa tus credenciales para acceder al sistema
                      </p>

                      <!-- Formulario de Login -->
                      <form id="formLogin" novalidate>
                        <!-- Campo Correo -->
                        <div class="mb-3">
                          <label for="correo" class="form-label">Correo electrónico</label>
                          <div class="input-group">
                            <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                            <input
                              type="email"
                              class="form-control"
                              id="correo"
                              placeholder="ejemplo@dominio.com"
                              required
                            />
                            <div class="invalid-feedback">
                              Ingresa un correo válido.
                            </div>
                          </div>
                        </div>

                        <!-- Campo Contraseña -->
                        <div class="mb-4">
                          <label for="password" class="form-label">Contraseña</label>
                          <div class="input-group">
                            <span class="input-group-text"><i class="bi bi-lock"></i></span>
                            <input
                              type="password"
                              class="form-control"
                              id="password"
                              placeholder="Mínimo 6 caracteres"
                              minlength="6"
                              required
                            />
                            <div class="invalid-feedback">
                              La contraseña debe tener al menos 6 caracteres.
                            </div>
                          </div>
                        </div>

                        <!-- Botón de Inicio de Sesión -->
                        <div class="d-grid mb-3">
                          <button
                            type="button"
                            class="btn btn-primary"
                            id="btnLogin"
                            disabled
                            onclick="login()"
                          >
                            Iniciar Sesión
                          </button>
                        </div>

                        <!-- Enlace para Registrarse (opcional) -->
                        <p class="text-center mb-0">
                          ¿No tienes cuenta?
                          <a href="Registro.aspx" class="link-primary">Regístrate aquí</a>
                        </p>
                      </form>
                    </div>
                  </div>
             </div>
             </div>
        
    </main>

</asp:Content>
