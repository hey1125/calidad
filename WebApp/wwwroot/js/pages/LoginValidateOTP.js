function LoginValidateOTPController() {
    this.ApiLogin = "FinalUser/Login";
    this.ApiValidateUser = "FinalUser/ValidateUser"; 

    this.LoginValidateOTP = function () {
        var ctrlActions = new ControlActions();
        var credentials = ctrlActions.GetDataForm("frmLoginOTP");
        const stored = localStorage.getItem("user");
        const finalUser = JSON.parse(stored);
        credentials.nationalId = finalUser.nationalId; 

        const payload = {
            otp: credentials.otp,
            nationalId: credentials.nationalId
        }
        console.log(credentials);
        console.log(payload);

        ctrlActions.PostToAPI(this.ApiValidateUser, payload,
            function (response) {
                if (!response) {
                    Swal.fire({
                        icon: "error",
                        title: "Oops...",
                        text: error.responseText || "Error al validar OTP"
                    });
                } else {
                    console.log("Respuesta del login:", finalUser);
                    Swal.fire({
                        icon: "success",
                        title: `¡Bienvenido ${finalUser.firstName}!`,
                        text: "Inicio de sesión exitoso",
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {

                        // Redirigir: admin o usuario final
                        window.location.href = finalUser.email === "admin@billetico.com"
                            ? "/Admin/Dashboard"
                            : "/User/Dashboard";
                    });
                }
            },
            function (error) {
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: error.responseText || "Error al iniciar sesión"
                });
            }
        );
    };
}
