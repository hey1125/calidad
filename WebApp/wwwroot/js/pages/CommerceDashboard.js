$(document).ready(function () {
    LoadCommerceDashboard();

    // Event listeners para filtros
    $('#btnApplyFilter').click(function () {
        LoadPayments();
    });

    $('#btnClearFilter').click(function () {
        ClearFilters();
    });

    $('#btnRefresh').click(function () {
        LoadCommerceDashboard();
    });
});

let currentCommerceId = null;

function LoadCommerceDashboard() {
    // Obtener el commerceId del query string
    const urlParams = new URLSearchParams(window.location.search);
    currentCommerceId = urlParams.get('commerceId');

    if (!currentCommerceId) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'No se especificó el ID del comercio',
            confirmButtonText: 'OK'
        }).then(() => {
            window.location.href = '/User/ActiveCommerce';
        });
        return;
    }

    // Cargar datos del comercio y sus pagos
    LoadCommerceDetails(currentCommerceId);
    LoadPayments();
}

function LoadCommerceDetails(commerceId) {
    var ctrlActions = new ControlActions();

    ctrlActions.GetToApi(`Commerce/RetrieveById?id=${commerceId}`, function (data) {
        if (data) {
            PopulateCommerceInfo(data);
        } else {
            ShowError('No se pudo cargar la información del comercio');
        }
    }, function (error) {
        console.log('Error al obtener detalles del comercio:', error);
        ShowError('Error al cargar los datos del comercio');
    });
}

function LoadPayments() {
    if (!currentCommerceId) return;

    var ctrlActions = new ControlActions();

    // Obtener filtros
    const startDate = $('#startDate').val();
    const endDate = $('#endDate').val();
    const status = $('#statusFilter').val();

    // Construir URL con parámetros
    let url = `Payment/GetCommercePayments?commerceId=${currentCommerceId}`;
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
        console.log('Error al obtener pagos:', error);
        ShowNoPayments();
    });
}

function PopulateCommerceInfo(commerce) {
    // Actualizar el título de la página
    document.title = `${commerce.name} - Dashboard`;

    // Llenar los campos de información
    $('#commerceName').text(commerce.name);
    $('#commerceEmail').text(commerce.email);
    $('#commerceStatus').text(commerce.status);
}

function PopulatePaymentsTable(payments) {
    const tableBody = $('#paymentsTableBody');
    tableBody.empty();

    payments.forEach(function (payment) {
        const row = `
            <tr>
                <td>${formatDate(payment.paymentDate)}</td>
                <td>${payment.payerName || 'Cliente'}</td>
                <td>${payment.description || '-'}</td>
                <td class="text-end">₡${formatCurrency(payment.grossAmount)}</td>
                <td class="text-end">${payment.amountWDiscount ? '₡' + formatCurrency(payment.amountWDiscount) : '-'}</td>
                <td class="text-end fw-bold">₡${formatCurrency(payment.netAmount)}</td>
                <td class="text-end text-warning">₡${formatCurrency(payment.coCommisionAmount || 0)}</td>
                <td class="text-end text-success fw-bold">₡${formatCurrency(payment.commerceProfit)}</td>
                <td>${getStatusBadge(payment.status)}</td>
            </tr>
        `;
        tableBody.append(row);
    });
}

function CalculateStatistics(payments) {
    let totalReceived = 0;
    let totalCommissions = 0;
    let totalProfit = 0;
    let totalTransactions = payments.length;

    payments.forEach(function (payment) {
        if (payment.status === 'Pagado') {
            totalReceived += payment.netAmount || 0;
            totalCommissions += payment.coCommisionAmount || 0;
            totalProfit += payment.commerceProfit || 0;
        }
    });

    // Actualizar las estadísticas en las tarjetas
    $('#totalReceived').text('₡' + formatCurrency(totalReceived));
    $('#totalTransactions').text(totalTransactions);
    $('#totalCommissions').text('₡' + formatCurrency(totalCommissions));
    $('#netProfit').text('₡' + formatCurrency(totalProfit));
}

function ShowNoPayments() {
    $('#paymentsTableBody').empty();
    $('#paymentsTable').addClass('d-none');
    $('#noPaymentsMessage').removeClass('d-none');

    // Resetear estadísticas
    $('#totalReceived').text('₡0.00');
    $('#totalTransactions').text('0');
    $('#totalCommissions').text('₡0.00');
    $('#netProfit').text('₡0.00');
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
        window.location.href = '/User/ActiveCommerce';
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
        case 'Cancelado':
            return '<span class="badge bg-danger">Cancelado</span>';
        default:
            return '<span class="badge bg-secondary">' + status + '</span>';
    }
}