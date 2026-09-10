let map;
let marker;

$(document).ready(function () {
    InitializeMap();
    InitializeInputLimits();

    $('#btnGuardarCo').click(function () {
        CreateCommerce();
    });
});

function InitializeInputLimits() {
    // Cédula jurídica: solo números, máximo 10
    $('#coLegalId').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');
        if (value.length > 10) {
            value = value.substring(0, 10);
        }
        $(this).val(value);
    });
    // Teléfono: solo números, máximo 8
    $('#coPhone').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');
        if (value.length > 8) {
            value = value.substring(0, 8);
        }
        $(this).val(value);
    });
    // Nombre: límite razonable de caracteres
    $('#coName').on('input', function () {
        let value = $(this).val();
        if (value.length > 100) {
            $(this).val(value.substring(0, 100));
        }
    });
    // Email: límite razonable de caracteres
    $('#coEmail').on('input', function () {
        let value = $(this).val();
        if (value.length > 100) {
            $(this).val(value.substring(0, 100));
        }
    });
    // IBAN: comienza con CR y 20 dígitos
    $('#coIBAN').on('input', function () {
        let value = $(this).val().toUpperCase().replace(/[^A-Z0-9]/g, '');
        if (!value.startsWith("CR")) {
            value = "CR" + value.replace(/^CR/, '');
        }
        if (value.length > 22) {
            value = value.substring(0, 22);
        }
        $(this).val(value);
    });
    // Cédula nacional: solo números, máximo 9
    $('#coNationalId').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');
        if (value.length > 9) {
            value = value.substring(0, 9);
        }
        $(this).val(value);
    });
}

function InitializeMap() {

    map = L.map('coMap').setView([9.7489, -83.7534], 7);
    window["coMap_leaflet"] = map;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    map.on('click', function (e) {
        if (marker) {
            map.removeLayer(marker);
        }

        marker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(map);

        $('#coLatitude').val(e.latlng.lat);
        $('#coLongitude').val(e.latlng.lng);
    });
}

function CreateCommerce() {

    const legalId = $('#coLegalId').val().trim();
    const name = $('#coName').val().trim();
    const phone = $('#coPhone').val().trim();
    const email = $('#coEmail').val().trim();
    const iban = $('#coIBAN').val().trim();
    const nationalId = $('#coNationalId').val().trim();
    const commissionRate = parseFloat($('#coCommission').val());
    const latitude = $('#coLatitude').val();
    const longitude = $('#coLongitude').val();

    // Validar campos vacíos
    if (!legalId) {
        Swal.fire('Campo requerido', 'Debe ingresar la cédula jurídica', 'warning');
        $('#coLegalId').focus();
        return;
    }

    if (legalId.length !== 10) {
        Swal.fire('Formato incorrecto', 'La cédula jurídica debe tener exactamente 10 dígitos', 'error');
        $('#coLegalId').focus();
        return;
    }

    if (!name) {
        Swal.fire('Campo requerido', 'Debe ingresar el nombre del comercio', 'warning');
        $('#coName').focus();
        return;
    }

    if (!phone) {
        Swal.fire('Campo requerido', 'Debe ingresar el número de teléfono', 'warning');
        $('#coPhone').focus();
        return;
    }

    if (phone.length !== 8) {
        Swal.fire('Formato incorrecto', 'El teléfono debe tener exactamente 8 dígitos', 'error');
        $('#coPhone').focus();
        return;
    }

    if (!email) {
        Swal.fire('Campo requerido', 'Debe ingresar el correo electrónico', 'warning');
        $('#coEmail').focus();
        return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        Swal.fire('Formato incorrecto', 'Ingrese un correo electrónico válido (ejemplo@dominio.com)', 'error');
        $('#coEmail').focus();
        return;
    }

    if (!iban) {
        Swal.fire('Campo requerido', 'Debe ingresar el IBAN', 'warning');
        $('#coIBAN').focus();
        return;
    }

    if (!/^CR\d{20}$/.test(iban)) {
        Swal.fire('Formato de IBAN inválido', "Asegúrese de ingresar un valor que comience con 'CR' seguido de 20 dígitos", 'error');
        $('#coIBAN').focus();
        return;
    }

    if (!nationalId) {
        Swal.fire('Campo requerido', 'Debe ingresar la cédula del administrador', 'warning');
        $('#coNationalId').focus();
        return;
    }

    if (nationalId.length !== 9) {
        Swal.fire('Formato incorrecto', 'La cédula nacional debe tener exactamente 9 dígitos', 'error');
        $('#coNationalId').focus();
        return;
    }

    if (!latitude || !longitude) {
        Swal.fire('Ubicación requerida', 'Debe seleccionar la ubicación en el mapa', 'warning');
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
        iban: iban,
        nationalId: nationalId
    };

    console.log('Datos a enviar:', commerceData);

    const ctrlActions = new ControlActions();

    ctrlActions.PostToAPI(
        'Commerce/Create',
        commerceData,
        function () {
            Swal.fire({
                icon: 'success',
                title: '¡Solicitud enviada!',
                text: 'Hemos recibido su información. Le notificaremos por correo cuando sea revisada por nuestro equipo.'
            }).then(() => {
                $('#commerceForm')[0].reset();
                $('#coLatitude').val('');
                $('#coLongitude').val('');
                if (marker) {
                    map.removeLayer(marker);
                    marker = null;
                }

                window.location.href = '/Public/Index';
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
        }
    );
}
