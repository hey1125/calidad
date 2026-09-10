// Variable global - SOLO UNA DECLARACIÓN
let currentBankId = null;

$(document).ready(function () {
    LoadBankDashboard();

    // Event listeners para filtros
    $('#btnApplyFilter').click(function () {
        LoadPayments();
    });

    $('#btnClearFilter').click(function () {
        ClearFilters();
    });

    $('#btnRefresh').click(function () {
        LoadBankDashboard();
    });
});

function LoadBankDashboard() {
    // Obtener el bankId del query string
    const urlParams = new URLSearchParams(window.location.search);
    currentBankId = urlParams.get('bankId'); // NO redeclarar aquí

    if (!currentBankId) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se especificó el ID del banco',
            confirmButtonText: 'OK'
        }).then(() => {
            window.location.href = '/User/ActiveBanks';
        });
        return;
    }

    // Cargar datos del banco y sus pagos
    LoadBankDetails(currentBankId);
    LoadPayments();
}

function LoadBankDetails(bankId) {
    var ctrlActions = new ControlActions();

    ctrlActions.GetToApi(`FinancialEntity/RetrieveById?id=${bankId}`, function (data) {
        if (data) {
            PopulateBankInfo(data);
        } else {
            ShowError('No se pudo cargar la información del banco');
        }
    }, function (error) {
        console.log('Error al obtener detalles del banco:', error);
        ShowError('Error al cargar los datos del banco');
    });
}

function LoadPayments() {
    if (!currentBankId) return;

    var ctrlActions = new ControlActions();

    // Obtener filtros
    const startDate = $('#startDate').val();
    const endDate = $('#endDate').val();
    const status = $('#statusFilter').val();

    // Primero necesitamos obtener el bankCode del banco
    ctrlActions.GetToApi(`FinancialEntity/RetrieveById?id=${currentBankId}`, function (bankData) {
        if (bankData && bankData.bankCode) {
            // Construir URL con parámetros para pagos del banco
            let url = `Payment/GetBankPayments?bankCode=${bankData.bankCode}`;
            if (startDate) url += `&startDate=${startDate}`;
            if (endDate) url += `&endDate=${endDate}`;
            if (status) url += `&status=${status}`;

            ctrlActions.GetToApi(url, function (data) {
                if (data && data.length > 0) {
                    PopulatePaymentsTable(data);
                    CalculateStatistics(data);
                    $('#noPaymentsMessage').addClass('d-none');
                    $('#paymentsTable').removeClass('d-none');
                } else {
                    ShowNoPayments();
                }
            }, function (error) {
                console.log('Error al obtener pagos del banco:', error);
                ShowNoPayments();
            });
        }
    });
}

function PopulateBankInfo(bank) {
    // Actualizar el título de la página
    document.title = `${bank.name} - Dashboard`;

    // Llenar los campos de información
    $('#bankName').text(bank.name);
    $('#bankEmail').text(bank.email);
    $('#bankStatus').text(bank.status);
}

function PopulatePaymentsTable(payments) {
    const tableBody = $('#paymentsTableBody');
    tableBody.empty();

    payments.forEach(function (payment) {
        // TABLA CORREGIDA SEGÚN HU 7.3: SIN COLUMNA DE DESCUENTO
        const row = `
            <tr>
                <td>${formatDate(payment.paymentDate)}</td>
                <td>${payment.commerceName || 'Comercio'}</td>
                <td>${payment.description || '-'}</td>
                <td class="text-end">₡${formatCurrency(payment.grossAmount)}</td>
                <td class="text-end text-warning fw-bold">₡${formatCurrency(payment.feCommisionAmount || 0)}</td>
                <td>${getStatusBadge(payment.status)}</td>
            </tr>
        `;
        tableBody.append(row);
    });
}

function CalculateStatistics(payments) {
    let totalProcessed = 0;
    let totalCommissions = 0; // Comisiones que el banco PAGA a la plataforma
    let totalTransactions = payments.length;

    payments.forEach(function (payment) {
        if (payment.status === 'Pagado') {
            totalProcessed += payment.grossAmount || 0;
            totalCommissions += payment.feCommisionAmount || 0; // Comisión que paga el banco
        }
    });

    // Actualizar las estadísticas en las tarjetas (SIN comercios activos)
    $('#totalProcessed').text('₡' + formatCurrency(totalProcessed));
    $('#totalTransactions').text(totalTransactions);
    $('#totalCommissions').text('₡' + formatCurrency(totalCommissions));
}

function ShowNoPayments() {
    $('#paymentsTableBody').empty();
    $('#paymentsTable').addClass('d-none');
    $('#noPaymentsMessage').removeClass('d-none');

    // Resetear estadísticas (SIN comercios activos)
    $('#totalProcessed').text('₡0.00');
    $('#totalTransactions').text('0');
    $('#totalCommissions').text('₡0.00');
}

function ClearFilters() {
    $('#startDate').val('');
    $('#endDate').val('');
    $('#statusFilter').val('');
    LoadPayments();
}

function ShowError(message) {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: message,
        confirmButtonText: 'OK'
    }).then(() => {
        window.location.href = '/User/ActiveBanks';
    });
}

// Funciones de utilidad
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-CR', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('es-CR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(amount);
}

function getStatusBadge(status) {
    switch (status) {
        case 'Pagado':
            return '<span class="badge bg-success">Pagado</span>';
        case 'Pendiente':
            return '<span class="badge bg-warning">Pendiente</span>';
        default:
            return '<span class="badge bg-secondary">' + status + '</span>';
    }
}