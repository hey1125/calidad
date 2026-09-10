function RegisterController() {
    this.ApiCreate = "FinalUser/Create";
    this.ApiUploadFile = "Upload/file";

    this.InitView = function () {
        var ctrlActions = new ControlActions();
        var self = this; // Guardamos el contexto
        var finalUser = {
            id: 0,
            created: new Date(),
            updated: new Date(),
            nationalId: "",
            firstName: "",
            lastName1: "",
            lastName2: "",
            passwordHash: "",
            phone: "",
            email: "",
            dateOfBirth: "",
            locationLat: 0,
            locationLng: 0,
            selfieUrl: "",
            facialVerificationPassed: false,
            profilePhotoUrl: "",
            idCardFrontPhotoUrl: "",
            idCardBackPhotoUrl: "",
            status: "Pending"
        };

        startMap();

        $("#btnSiguiente").on("click", function () {
            let isValid = true;

            if (!$("#Cedula").val() || !$("#Cedula").val().trim()) {
                Swal.fire("Error", "La cédula es obligatoria", "error");
                isValid = false;
                return;
            }
            if (!/^\d{9}$/.test($("#Cedula").val())) {
                Swal.fire("Error", "La cédula debe tener 9 dígitos numéricos", "error");
                isValid = false;
                return;
            }
            if (!$("#Nombre").val() || !$("#Nombre").val().trim()) {
                Swal.fire("Error", "El nombre es obligatorio", "error");
                isValid = false;
                return;
            }
            if ($("#Nombre").val().length > 40) {
                Swal.fire("Error", "El nombre no debe superar los 40 caracteres", "error");
                isValid = false;
                return;
            }
            if (!$("#Apellido1").val() || !$("#Apellido1").val().trim()) {
                Swal.fire("Error", "El primer apellido es obligatorio", "error");
                isValid = false
                return;
            }
            if ($("#Apellido1").val().length > 40) {
                Swal.fire("Error", "El primer apellido no debe superar los 40 caracteres", "error");
                isValid = false
                return;
            }
            if (!$("#Apellido2").val() || !$("#Apellido2").val().trim()) {
                Swal.fire("Error", "El segundo apellido es obligatorio", "error");
                isValid = false
                return;
            }
            if ($("#Apellido2").val().length > 40) {
                Swal.fire("Error", "El segundo apellido no debe superar los 40 caracteres", "error");
                isValid = false
                return;
            }
            if (!$("#Telefono").val() || !$("#Telefono").val().trim()) {
                Swal.fire("Error", "El teléfono es obligatorio", "error");
                isValid = false
                return;
            }
            if (!/^\d{8}$/.test($("#Telefono").val())) {
                Swal.fire("Error", "El teléfono debe tener exactamente 8 dígitos", "error");
                isValid = false
                return;
            }
            if (!$("#Email").val() || !$("#Email").val().trim()) {
                Swal.fire("Error", "El correo electrónico es obligatorio", "error");
                isValid = false
                return;
            }
            if ($("#Email").val().length > 250) {
                Swal.fire("Error", "El correo no debe superar los 250 caracteres", "error");
                isValid = false
                return;
            }
            const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!pattern.test($("#Email").val())) {
                Swal.fire("Error", "Ingrese un correo electrónico válido (ejemplo@dominio.com)", "error");
                isValid = false
                return;
            }
            if (!$("#Password").val()) {
                Swal.fire("Error", "La contraseña es obligatoria", "error");
                isValid = false
                return;
            }
            if ($("#Password").val().length < 8) {
                Swal.fire("Error", "La contraseña debe tener al menos 8 caracteres", "error");
                isValid = false
                return;
            }
            const patternPassword = /^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$/;
            if (!patternPassword.test($("#Password").val())) {
                Swal.fire("Error", "Debe contener letras, números y al menos un carácter especial", "error");
                isValid = false
                return;
            }
            if (!$("#FechaNacimiento").val() || !$("#FechaNacimiento").val().trim()) {
                Swal.fire("Error", "La fecha de nacimiento es obligatoria", "error");
                isValid = false
                return;
            }
            const dob = new Date($("#FechaNacimiento").val());
            if (isNaN(dob.getTime())) {
                Swal.fire("Error", "Ingrese una fecha de nacimiento válida", "error");
                isValid = false
                return;
            }
            const today = new Date();
            if (dob > today) {
                Swal.fire("Error", "Ingrese una fecha de nacimiento en el pasado", "error");
                isValid = false
                return;
            }
            let age = today.getFullYear() - dob.getFullYear();
            const m = today.getMonth() - dob.getMonth();
            if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) {
                age--;
            }
            if (age < 18) {
                Swal.fire("Error", "Debe ser mayor de edad", "error");
                isValid = false
                return;
            }
            if (!$("#txtLatitude").val() || !$("#txtLongitude").val()) {
                Swal.fire("Ubicación requerida", "Debe seleccionar la ubicación en el mapa", "warning");
                isValid = false;
                return;
            }
            finalUser.nationalId = $("#Cedula").val();
            finalUser.firstName = $("#Nombre").val();
            finalUser.lastName1 = $("#Apellido1").val();
            finalUser.lastName2 = $("#Apellido2").val();
            finalUser.passwordHash = $("#Password").val();
            finalUser.phone = $("#Telefono").val();
            finalUser.email = $("#Email").val();
            finalUser.dateOfBirth = $("#FechaNacimiento").val();
            finalUser.locationLat = $("#txtLatitude").val();
            finalUser.locationLng = $("#txtLongitude").val();


            if (isValid) {
                ctrlActions.PostToAPIWithCustomHeaders(
                    self.ApiCreate, // usamos self aquí, no this
                    finalUser,
                    function (resp) {
                        sessionStorage.setItem("FinalUser", JSON.stringify(resp));
                        Swal.fire("¡Éxito!", "Usuario registrado correctamente", "success")
                            .then(() => window.location.href = `/Public/RegisterOTPValidation?nationalId=${finalUser.nationalId}`);
                    },
                    function (error) {
                        Swal.fire("Error", error.responseJSON || error.responseText, "error");
                    },
                );
            }

        });

        $("#btnValidate").on("click", function () {
            let isValid = true;

            if (!$("#OTP").val() || !$("#OTP").val().trim()) {
                Swal.fire("Error", "El OTP es obligatorio", "error");
                isValid = false;
                return;
            }

            const stored = sessionStorage.getItem("FinalUser");

            const payload = {
                nationalId: stored.nationalId,
                otp: $("#OTP").val()
            }
            console.log(payload);

            if (isValid) {
                //ctrlActions.PostToAPIWithCustomHeaders(
                //    self.ApiCreate, // usamos self aquí, no this
                //    finalUser,
                //    function (resp) {
                //        sessionStorage.setItem("FinalUser", JSON.stringify(resp));
                //        Swal.fire("¡Éxito!", "Usuario registrado correctamente.", "success")
                //            .then(() => window.location.href = "/Public/RegisterOTPValidation");
                //    },
                //    function (error) {
                //        Swal.fire("Error", error.responseJSON || error.responseText, "error");
                //    },
                //);
            }

        });
    };

}

function startMap() {
    var map = L.map("map").setView([9.7489, -83.7534], 7); // Centro de CR

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap"
    }).addTo(map);

    var marker;

    map.on("click", function (e) {
        var lat = e.latlng.lat.toFixed(8);
        var lng = e.latlng.lng.toFixed(8);

        $("#txtLatitude").val(lat);
        $("#txtLongitude").val(lng);

        if (marker) {
            marker.setLatLng(e.latlng);
        } else {
            marker = L.marker(e.latlng).addTo(map);
        }
    });
}


// Instanciar
$(document).ready(function () {
    var view = new RegisterController();
    view.InitView();
});
