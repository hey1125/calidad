let map;
let marker;

$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const bankId = urlParams.get('id');

    if (bankId) {
        LoadBankDetails(bankId);
    }

    // Eventos
    $('#selectDecision').change(function () {
        const decision = $(this).val();
        if (decision === 'aprobar') {
            $('#comisionGroup').removeClass('d-none');
            $('#commissionRate').attr('required', true);
        } else {
            $('#comisionGroup').addClass('d-none');
            $('#commissionRate').removeAttr('required');
        }
    });

    $('#btnCancelar').click(function () {
        Swal.fire({
            title: '¿Desea cancelar?',
            text: 'Se perderán los cambios no guardados',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Sí, cancelar',
            cancelButtonText: 'Continuar editando'
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = '/Admin/PendingBanks';
            }
        });
    });

    $('#btnGuardar').click(function () {
        ProcessDecision();
    });

    $('#commissionRate').on('input', function () {
        const value = parseFloat($(this).val());
        if (value < 0.01 || value > 99.99) {
            $(this).addClass('is-invalid');
            if (!$('#commission-feedback').length) {
                $(this).after('<div id="commission-feedback" class="invalid-feedback">La comisión debe estar entre 0.01% y 99.99%</div>');
            }
        } else {
            $(this).removeClass('is-invalid').addClass('is-valid');
            $('#commission-feedback').remove();
        }
    });
});

function LoadBankDetails(bankId) {
    var ctrlActions = new ControlActions();

    ctrlActions.GetToApi(`FinancialEntity/RetrieveById?id=${bankId}`, function (data) {
        console.log('Datos del banco:', data);
        ctrlActions.BindFields('frmBanco', data);
        InitializeMap(data.latitude, data.longitude);
    });
}

function InitializeMap(lat, lng) {
    map = L.map('map').setView([lat, lng], 15);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    marker = L.marker([lat, lng]).addTo(map)
        .bindPopup(`<strong>${$('#frmBanco input[ColumnDataName="name"]').val()}</strong><br>Ubicación registrada`)
        .openPopup();
}

function ProcessDecision() {
    const decision = $('#selectDecision').val();

    if (!decision) {
        Swal.fire({
            icon: 'warning',
            title: 'Seleccione una decisión',
            text: 'Debe elegir entre aprobar o rechazar la solicitud'
        });
        return;
    }

    if (decision === 'aprobar') {
        ShowApprovalConfirmation();
    } else if (decision === 'rechazar') {
        ShowRejectionConfirmation();
    }
}

function ShowApprovalConfirmation() {
    const commissionRate = $('#commissionRate').val();
    const bankName = $('#frmBanco input[ColumnDataName="name"]').val();
    const nationalId = $('#frmBanco input[ColumnDataName="nationalId"]').val();

    if (!nationalId || nationalId.trim().length !== 9 || !/^\d+$/.test(nationalId)) {
        Swal.fire({
            icon: 'error',
            title: 'Cédula inválida',
            text: 'Debe ingresar una cédula válida para el administrador (9 dígitos numéricos).'
        });
        return;
    }

    if (!commissionRate || commissionRate <= 0 || commissionRate > 99.99) {
        Swal.fire({
            icon: 'error',
            title: 'Comisión inválida',
            text: 'Debe ingresar una comisión válida entre 0.01% y 99.99%'
        });
        $('#commissionRate').focus();
        return;
    }

    Swal.fire({
        title: '¿Aprobar esta entidad financiera?',
        html: `
            <div class="text-start">
                <p><strong>Entidad:</strong> ${bankName}</p>
                <p><strong>Cédula administrador:</strong> ${nationalId}</p>
                <p><strong>Comisión asignada:</strong> ${commissionRate}%</p>
                <hr>
                <p class="text-success"><i class="fas fa-check-circle me-2"></i>Al aprobar:</p>
                <ul class="text-muted">
                    <li>La entidad quedará <strong>ACTIVA</strong></li>
                    <li>Podrá operar en la plataforma BilleTico</li>
                    <li>Se aplicará la comisión del <strong>${commissionRate}%</strong></li>
                    <li>Se enviará notificación automática</li>
                </ul>
            </div>
        `,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-check me-1"></i>Sí, aprobar',
        cancelButtonText: '<i class="fas fa-times me-1"></i>Cancelar',
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#6c757d',
        width: '500px'
    }).then((result) => {
        if (result.isConfirmed) {
            ApproveBank();
        }
    });
}

function ShowRejectionConfirmation() {
    const bankName = $('#frmBanco input[ColumnDataName="name"]').val();

    Swal.fire({
        title: '¿Rechazar esta entidad financiera?',
        html: `
            <div class="text-start">
                <p><strong>Entidad:</strong> ${bankName}</p>
                <hr>
                <p class="text-danger"><i class="fas fa-exclamation-triangle me-2"></i>Al rechazar:</p>
                <ul class="text-muted">
                    <li>La entidad quedará <strong>RECHAZADA</strong></li>
                    <li>NO podrá operar en la plataforma</li>
                    <li>Se enviará notificación del rechazo</li>
                </ul>
            </div>
        `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-ban me-1"></i>Sí, rechazar',
        cancelButtonText: '<i class="fas fa-times me-1"></i>Cancelar',
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        width: '500px'
    }).then((result) => {
        if (result.isConfirmed) {
            RejectBank();
        }
    });
}

function ApproveBank() {
    const commissionRate = $('#commissionRate').val();
    const bankName = $('#frmBanco input[ColumnDataName="name"]').val();
    const nationalId = $('#frmBanco input[ColumnDataName="nationalId"]').val();

    var ctrlActions = new ControlActions();
    var bankData = ctrlActions.GetDataForm('frmBanco');

    bankData.commissionRate = parseFloat(commissionRate);
    bankData.nationalId = nationalId;

    ctrlActions.PutToAPI('FinancialEntity/Approve', bankData, function () {
        Swal.fire({
            icon: 'success',
            title: '¡Entidad Aprobada!',
            html: `
                <p><strong>${bankName}</strong> ha sido aprobada exitosamente.</p>
                <p>Estado: ACTIVA</p>
                <p>Comisión: ${commissionRate}%</p>
            `,
            confirmButtonText: 'Continuar'
        }).then(() => {
            window.location.href = '/Admin/PendingBanks';
        });
    });
}

function RejectBank() {
    const bankName = $('#frmBanco input[ColumnDataName="name"]').val();
    var ctrlActions = new ControlActions();
    var bankData = ctrlActions.GetDataForm('frmBanco');

    ctrlActions.PutToAPI('FinancialEntity/Reject', bankData, function () {
        Swal.fire({
            icon: 'info',
            title: 'Entidad Rechazada',
            html: `
                <p><strong>${bankName}</strong> ha sido rechazada.</p>
                <p>Estado: RECHAZADA</p>
            `,
            confirmButtonText: 'Continuar'
        }).then(() => {
            window.location.href = '/Admin/PendingBanks';
        });
    });
}
