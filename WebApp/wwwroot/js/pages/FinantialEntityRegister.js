let bankMap;
let bankMarker;

$(document).ready(function () {
    InitializeBankMap();
    InitializeBankInputLimits();

    $('#btnGuardarBank').click(function () {
        CreateFinancialEntity();
    });
});

function InitializeBankInputLimits() {
    // Cédula jurídica: solo números, máximo 10
    $('#baLegalId').on('input', function () {
        let value = $(this).val().replace(/\D/g, ''); // Solo números
        if (value.length > 10) {
            value = value.substring(0, 10);
        }
        $(this).val(value);
    });

    // Teléfono: solo números, máximo 8
    $('#baPhone').on('input', function () {
        let value = $(this).val().replace(/\D/g, ''); // Solo números
        if (value.length > 8) {
            value = value.substring(0, 8);
        }
        $(this).val(value);
    });

    // Nombre: límite razonable de caracteres
    $('#baName').on('input', function () {
        let value = $(this).val();
        if (value.length > 100) {
            $(this).val(value.substring(0, 100));
        }
    });

    // Email: límite razonable de caracteres
    $('#baEmail').on('input', function () {
        let value = $(this).val();
        if (value.length > 100) {
            $(this).val(value.substring(0, 100));
        }
    });

    // Código bancario: solo números, máximo 3
    $('#baBankCode').on('input', function () {
        let value = $(this).val().replace(/\D/g, ''); // Solo números
        if (value.length > 3) {
            value = value.substring(0, 3);
        }
        $(this).val(value);
    });

    // Cédula nacional: solo números, máximo 9
    $('#baNationalId').on('input', function () {
        let value = $(this).val().replace(/\D/g, '');
        if (value.length > 9) {
            value = value.substring(0, 9);
        }
        $(this).val(value);
    });
}

function InitializeBankMap() {
    bankMap = L.map('baMap').setView([9.7489, -83.7534], 7);
    window["baMap_leaflet"] = bankMap;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(bankMap);

    bankMap.on('click', function (e) {
        if (bankMarker) {
            bankMap.removeLayer(bankMarker);
        }

        bankMarker = L.marker([e.latlng.lat, e.latlng.lng]).addTo(bankMap);

        $('#baLatitude').val(e.latlng.lat);
        $('#baLongitude').val(e.latlng.lng);
    });
}

function CreateFinancialEntity() {

    const legalId = $('#baLegalId').val().trim();
    const bankCode = $('#baBankCode').val().trim();
    const name = $('#baName').val().trim();
    const phone = $('#baPhone').val().trim();
    const email = $('#baEmail').val().trim();
    const nationalId = $('#baNationalId').val().trim();
    const latitude = $('#baLatitude').val();
    const longitude = $('#baLongitude').val();

    // Validar campos vacíos primero
    if (!legalId) {
        Swal.fire('Campo requerido', 'Debe ingresar la cédula jurídica', 'warning');
        $('#baLegalId').focus();
        return;
    }

    if (legalId.length !== 10) {
        Swal.fire('Formato incorrecto', 'La cédula jurídica debe tener exactamente 10 dígitos', 'error');
        $('#baLegalId').focus();
        return;
    }

    if (!bankCode) {
        Swal.fire('Campo requerido', 'Debe ingresar el código bancario', 'warning');
        $('#baBankCode').focus();
        return;
    }

    if (bankCode.length !== 3) {
        Swal.fire('Formato incorrecto', 'El código bancario debe tener exactamente 3 dígitos', 'error');
        $('#baBankCode').focus();
        return;
    }

    if (!name) {
        Swal.fire('Campo requerido', 'Debe ingresar el nombre de la entidad financiera', 'warning');
        $('#baName').focus();
        return;
    }

    if (!email) {
        Swal.fire('Campo requerido', 'Debe ingresar el correo electrónico', 'warning');
        $('#baEmail').focus();
        return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        Swal.fire('Formato incorrecto', 'Ingrese un correo electrónico válido (ejemplo@dominio.com)', 'error');
        $('#baEmail').focus();
        return;
    }

    if (!phone) {
        Swal.fire('Campo requerido', 'Debe ingresar el número de teléfono', 'warning');
        $('#baPhone').focus();
        return;
    }

    if (phone.length !== 8) {
        Swal.fire('Formato incorrecto', 'El teléfono debe tener exactamente 8 dígitos', 'error');
        $('#baPhone').focus();
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

    // Si llegamos aquí, todo está correcto
    const entityData = {
        legalId: legalId,
        bankCode: bankCode,
        name: name,
        email: email,
        phone: phone,
        latitude: parseFloat(latitude),
        longitude: parseFloat(longitude),
        status: "Pendiente",
        nationalId: nationalId
    };

    console.log('Datos a enviar:', entityData);

    const ctrlActions = new ControlActions();

    ctrlActions.PostToAPI(
        'FinancialEntity/Create',
        entityData,
        function () {
            Swal.fire({
                icon: 'success',
                title: '¡Solicitud enviada!',
                text: 'Hemos recibido su información. Le notificaremos por correo cuando sea revisada por nuestro equipo.'
            }).then(() => {
                $('#bankForm')[0].reset();
                $('#baLatitude').val('');
                $('#baLongitude').val('');
                if (bankMarker) {
                    bankMap.removeLayer(bankMarker);
                    bankMarker = null;
                }

                window.location.href = '/Public/Index';
            });
        },
        function (error) {
            console.error('Error del servidor:', error);

            let errorMessage = 'Error al procesar la solicitud';

            if (error.responseText) {
                const errorText = error.responseText.toLowerCase();

                if (errorText.includes('ya está asociado')) {
                    errorMessage = 'Ya está asociado a esta entidad financiera.';
                } else if (errorText.includes('código bancario')) {
                    errorMessage = 'Este código bancario ya está registrado.';
                } else if (errorText.includes('cédula jurídica')) {
                    errorMessage = 'Esta cédula jurídica ya está registrada.';
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
