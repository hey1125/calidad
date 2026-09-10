let map;
let marker;

$(document).ready(function () {
    InitializeMap();
    InitializeInputLimits();

    $('#btnGuardar').click(function () {
        CreateCommerce();
    });
});

function InitializeInputLimits() {
    // Cédula jurídica: solo números, máximo 10
    $('#txtLegalId').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');
        if (value.length > 10) {
            value = value.substring(0, 10);
        }
        $(this).val(value);
    });

    // Teléfono: solo números, máximo 8
    $('#txtPhone').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');
        if (value.length > 8) {
            value = value.substring(0, 8);
        }
        $(this).val(value);
    });

    // Nombre: límite razonable de caracteres
    $('#txtName').on('input', function () {
        let value = $(this).val();
        if (value.length > 100) {
            $(this).val(value.substring(0, 100));
        }
    });

    // Email: límite razonable de caracteres
    $('#txtEmail').on('input', function () {
        let value = $(this).val();
        if (value.length > 100) {
            $(this).val(value.substring(0, 100));
        }
    });

    // IBAN: comienza con CR y 20 dígitos
    $('#txtIBAN').on('input', function () {
        let value = $(this).val().toUpperCase().replace(/[^A-Z0-9]/g, '');
        if (!value.startsWith("CR")) {
            value = "CR" + value.replace(/^CR/, '');
        }
        if (value.length > 22) {
            value = value.substring(0, 22);
        }
        $(this).val(value);
    });
}

function InitializeMap() {
    const defaultLat = 9.9281;
    const defaultLng = -84.0907;

    map = L.map('map').setView([defaultLat, defaultLng], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    map.on('click', function (e) {
        if (marker) {
            map.removeLayer(marker);
        }

        marker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(map);

        $('#txtLatitude').val(e.latlng.lat);
        $('#txtLongitude').val(e.latlng.lng);
    });
}

function CreateCommerce() {
    const user = JSON.parse(localStorage.getItem("user"));

    const legalId = $('#txtLegalId').val().trim();
    const name = $('#txtName').val().trim();
    const phone = $('#txtPhone').val().trim();
    const email = $('#txtEmail').val().trim();
    const latitude = $('#txtLatitude').val();
    const longitude = $('#txtLongitude').val();
    const iban = $('#txtIBAN').val().trim();

    // Validar campos vacíos
    if (!legalId) {
        Swal.fire('Campo requerido', 'Debe ingresar la cédula jurídica', 'warning');
        $('#txtLegalId').focus();
        return;
    }

    if (!name) {
        Swal.fire('Campo requerido', 'Debe ingresar el nombre del comercio', 'warning');
        $('#txtName').focus();
        return;
    }

    if (!phone) {
        Swal.fire('Campo requerido', 'Debe ingresar el número de teléfono', 'warning');
        $('#txtPhone').focus();
        return;
    }

    if (!email) {
        Swal.fire('Campo requerido', 'Debe ingresar el correo electrónico', 'warning');
        $('#txtEmail').focus();
        return;
    }

    if (!iban) {
        Swal.fire('Campo requerido', 'Debe ingresar el IBAN', 'warning');
        $('#txtIBAN').focus();
        return;
    }

    if (!latitude || !longitude) {
        Swal.fire('Ubicación requerida', 'Debe seleccionar la ubicación en el mapa', 'warning');
        return;
    }

    // Validar formatos
    if (legalId.length !== 10) {
        Swal.fire('Formato incorrecto', 'La cédula jurídica debe tener exactamente 10 dígitos', 'error');
        $('#txtLegalId').focus();
        return;
    }

    if (phone.length !== 8) {
        Swal.fire('Formato incorrecto', 'El teléfono debe tener exactamente 8 dígitos', 'error');
        $('#txtPhone').focus();
        return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        Swal.fire('Formato incorrecto', 'Ingrese un correo electrónico válido (ejemplo@dominio.com)', 'error');
        $('#txtEmail').focus();
        return;
    }

    if (!/^CR\d{20}$/.test(iban)) {
        Swal.fire('Formato de IBAN inválido', "Asegúrese de ingresar un valor que comience con 'CR' seguido de 20 dígitos", 'error');
        $('#txtIBAN').focus();
        return;
    }

    const commerceData = {
        legalId: legalId,
        name: name,
        phone: phone,
        email: email,
        latitude: parseFloat(latitude),
        longitude: parseFloat(longitude),
        status: "Pendiente",
        iban: iban
    };

    console.log('Datos a enviar:', commerceData);

    var ctrlActions = new ControlActions();

    ctrlActions.PostToAPIWithCustomHeaders(
        'Commerce/Create',
        commerceData,
        function (response) {
            console.log('Respuesta del servidor:', response);
            Swal.fire({
                icon: 'success',
                title: '¡Solicitud enviada!',
                text: 'Hemos recibido su información. Le notificaremos por correo cuando sea revisada por nuestro equipo.'
            }).then(() => {
                // Limpiar formulario
                $('#frmCreateCommerce')[0].reset();
                $('#txtLatitude').val('');
                $('#txtLongitude').val('');
                if (marker) {
                    map.removeLayer(marker);
                    marker = null;
                }

                // Redirigir al dashboard
                window.location.href = '/User/Dashboard';
            });
        },
        function (error) {
            console.error('Error del servidor:', error);

            let errorMessage = 'Error al procesar la solicitud';

            if (error.responseText) {
                const errorText = error.responseText.toLowerCase();

                if (errorText.includes('cédula')) {
                    errorMessage = 'Esta cédula jurídica ya está registrada.';
                } else if (errorText.includes('iban')) {
                    errorMessage = 'Este IBAN ya está registrado.';
                } else {
                    errorMessage = error.responseText;
                }
            }

            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: errorMessage
            });
        },
        { 'LoggedUserId': user.id }
    );
}
