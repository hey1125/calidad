function LoginController() {
    this.ApiLogin = "FinalUser/Login";
    this.ApiRecoverPassword = "FinalUser/RecoverPassword";
    const ctrlActions = new ControlActions();

    this._recoverInFlight = false;
    this._loginInFlight = false;

    this.ToggleRecover = function () {
        const panel = document.getElementById("recoverContainer");
        panel.classList.toggle("d-none");
    };

    this.RecoverPassword = function () {
        if (this._recoverInFlight) return;
        const btn = document.getElementById("btnRecoverPassword");

        const emailInput = document.getElementById("RecoverEmail");
        const email = (emailInput?.value || "").trim();
        if (!email) { Swal.fire({ icon: "error", title: "Error", text: "El correo es obligatorio." }); return; }
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) { Swal.fire({ icon: "error", title: "Error", text: "Ingresa un correo válido." }); return; }

        this._recoverInFlight = true; btn?.setAttribute("disabled", "disabled");

        $.ajax({
            type: "POST",
            url: ctrlActions.GetUrlApiService(this.ApiRecoverPassword),
            data: JSON.stringify(email), 
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        })
            .done(() => {
                Swal.fire({ icon: "success", title: "Solicitud enviada", text: "Se enviaron instrucciones al correo registrado" });
            })
            .fail((jqXHR) => {
                const msg = jqXHR.responseJSON?.message || jqXHR.responseText || "No se pudo procesar la solicitud.";
                Swal.fire({ icon: "error", title: "Oops...", text: msg });
            })
            .always(() => {
                this._recoverInFlight = false; btn?.removeAttribute("disabled");
            });
    };

    this.Login = function () {
        if (this._loginInFlight) return;
        const btn = document.getElementById("btnLogin");

        var credentials = ctrlActions.GetDataForm("frmLogin");
        credentials.twoFactorCode = "";
        credentials.twoFactorRecoveryCode = "";

        this._loginInFlight = true; btn?.setAttribute("disabled", "disabled");

        $.ajax({
            type: "POST",
            url: ctrlActions.GetUrlApiService(this.ApiLogin),
            data: JSON.stringify(credentials),
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        })
            .done((response) => {
                Swal.fire({
                    icon: "success",
                    title: `¡Hola ${response.firstName}!`,
                    text: "Inicio de sesión exitoso",
                    timer: 1500,
                    showConfirmButton: false
                }).then(() => {
                    localStorage.setItem("user", JSON.stringify(response));
                    sessionStorage.setItem("userId", response.id);
                    window.location.href = response.email === "admin@billetico.com"
                        ? "/Admin/Dashboard"
                        : "/User/Dashboard";
                });
            })
            .fail((jqXHR) => {
                const msg = jqXHR.responseJSON?.message || jqXHR.responseText || "Error al iniciar sesión";
                Swal.fire({ icon: "error", title: "Oops...", text: msg });
            })
            .always(() => {
                this._loginInFlight = false; btn?.removeAttribute("disabled");
            });
    };
}

document.addEventListener("DOMContentLoaded", () => {
    window.loginCtrl = new LoginController();

    $(document).on("click", "#btnToggleRecover", () => window.loginCtrl.ToggleRecover());
    $(document).on("click", "#btnRecoverPassword", () => window.loginCtrl.RecoverPassword());
    $(document).on("click", "#btnLogin", () => window.loginCtrl.Login());
});
