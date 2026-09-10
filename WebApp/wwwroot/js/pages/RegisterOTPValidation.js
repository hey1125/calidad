function RegisterOTPValidationController() {
    this.ApiValidateUser = "FinalUser/ValidateUser";
    this.ApiGetUserByNationalId = "FinalUser/RetrieveByNationalId";
    this.ApiUpdateUser = "FinalUser/Update";
    this.ApiUploadFile = "Upload/file";

    let currentNationalId = null;
    let finalUser = null;

    this.InitView = function () {
        const ctrlActions = new ControlActions();
        const self = this;
        const urlParams = new URLSearchParams(window.location.search);
        currentNationalId = urlParams.get('nationalId');
        const savedUser = sessionStorage.getItem("FinalUser");
        finalUser = savedUser ? JSON.parse(savedUser) : null;
        $("#selfieContainer").toggleClass("d-none", !finalUser?.idCardFrontPhotoUrl);

        // 1) Load user by nationalId
        if (currentNationalId) {
            const url = `${self.ApiGetUserByNationalId}?nationalId=${encodeURIComponent(currentNationalId)}`;
            ctrlActions.GetToApiWithCustomHeaders(
                url,
                function (resp) {
                    finalUser = resp;
                    $("#selfieContainer").toggleClass("d-none", !finalUser.idCardFrontPhotoUrl);
                    sessionStorage.setItem("FinalUser", JSON.stringify(resp));
                },
                function (err) {
                    const msg = err.responseJSON?.message || err.responseText || "Error al obtener el usuario";
                    Swal.fire("Error", msg, "error");
                }
            );
        }

        // 2) Uploads selfie and updates user
        $("#inputSelfie").on("change", function () {
            const file = this.files[0];
            if (!file) return;

            const fd = new FormData();
            fd.append("File", file);
            fd.append("FileName", file.name);

            ctrlActions.PostToAPIWithCustomContentTypeAndHeaders(
                self.ApiUploadFile,
                fd,
                function (resp) {
                    // Make sure we have an user loaded
                    if (!finalUser) {
                        const stored = sessionStorage.getItem("FinalUser");
                        if (stored) finalUser = JSON.parse(stored);
                    }
                    if (!finalUser || !finalUser.nationalId) {
                        Swal.fire("Error", "Aún se está cargando el usuario. Intenta de nuevo.", "error");
                        return;
                    }

                    // Saves elfie URL to user
                    finalUser.selfieUrl = resp.url; 

                    ctrlActions.PutToAPI(self.ApiUpdateUser, finalUser, function (updated) {
                        finalUser = updated || finalUser;
                        sessionStorage.setItem("FinalUser", JSON.stringify(finalUser));
                    });
                },
                function (error) {
                    Swal.fire("Error", error.responseJSON || error.responseText || "Error subiendo la selfie", "error");
                }
            );
        });

        // 3) OTP Validation
        $("#btnValidateOTP").on("click", function () {
            const otp = $("#OTP").val()?.trim();
            if (!otp) {
                Swal.fire("Error", "El OTP es obligatorio", "error");
                return;
            }

            // Releer usuario “source of truth”
            const stored = sessionStorage.getItem("FinalUser");
            const storedUser = stored ? JSON.parse(stored) : finalUser;

            if (!storedUser) {
                Swal.fire("Error", "No se encontró el usuario registrado.", "error");
                return;
            }
            if (storedUser.idCardFrontPhotoUrl && !storedUser.selfieUrl) {
                Swal.fire("Error", "Debes cargar una selfie antes de validar el usuario.", "error");
                return;
            }

            const payload = {
                nationalId: storedUser.nationalId,
                otp: otp
            };

            ctrlActions.PostToAPIWithCustomHeaders(
                self.ApiValidateUser,
                payload,
                function (resp) {
                    if (!resp) {
                        Swal.fire("¡Error!", "Usuario no pudo ser validado", "error");
                        return;
                    }
                    Swal.fire("¡Éxito!", "Usuario validado correctamente.", "success")
                        .then(() => window.location.href = "/Public/Login");
                },
                function (error) {
                    Swal.fire("Error", error.responseJSON || error.responseText || "No se pudo validar el usuario", "error");
                }
            );
        });
    };
}

$(document).ready(function () {
    const view = new RegisterOTPValidationController();
    view.InitView();
});
