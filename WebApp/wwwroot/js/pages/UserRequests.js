$(document).ready(function () {
    LoadCommerceRequests();
    LoadBankRequests();

    // Manejar cambio de pestañas
    $('#bancos-tab').on('shown.bs.tab', function () {
        // Reinicializar tabla de bancos cuando se muestre la pestaña
        if ($.fn.DataTable.isDataTable('#tblBancos')) {
            $('#tblBancos').DataTable().columns.adjust().responsive.recalc();
        }
    });
});

function LoadCommerceRequests() {
    const user = JSON.parse(localStorage.getItem("user"));
    if (!user) {
        console.log('No hay usuario logueado');
        return;
    }

    var ctrlActions = new ControlActions();

    // Usar GetToApiWithCustomHeaders para enviar el header LoggedUserId
    ctrlActions.GetToApiWithCustomHeaders(
        `Commerce/RetrieveByUser?userId=${user.id}`,
        function (data) {
            console.log('Comercios obtenidos:', data);
            PopulateCommerceTable(data);
        },
        function (error) {
            console.log('Error al obtener comercios:', error);
            // Si no hay comercios o hay error, mostrar tabla vacía
            PopulateCommerceTable([]);
        },
        { 'LoggedUserId': user.id }
    );
}

function LoadBankRequests() {
    const user = JSON.parse(localStorage.getItem("user"));
    if (!user) {
        console.log('No hay usuario logueado');
        return;
    }

    var ctrlActions = new ControlActions();

    // Usar GetToApiWithCustomHeaders para enviar el header LoggedUserId  
    ctrlActions.GetToApiWithCustomHeaders(
        `FinancialEntity/RetrieveByUser?userId=${user.id}`,
        function (data) {
            console.log('Entidades financieras obtenidas:', data);
            PopulateBankTable(data);
        },
        function (error) {
            console.log('Error al obtener entidades financieras:', error);
            // Si no hay entidades o hay error, mostrar tabla vacía
            PopulateBankTable([]);
        },
        { 'LoggedUserId': user.id }
    );
}

function PopulateCommerceTable(data) {
    // Destruir DataTable existente si existe
    if ($.fn.DataTable.isDataTable('#tblComercios')) {
        $('#tblComercios').DataTable().destroy();
    }

    const tableBody = $('#tblComercios tbody');
    tableBody.empty();

    if (!data || data.length === 0) {
        tableBody.append('<tr><td colspan="5" class="text-center">No tiene solicitudes de comercio</td></tr>');
    } else {
        data.forEach(function (commerce) {
            const row = `
                <tr>
                    <td>${commerce.legalId || 'N/A'}</td>
                    <td>${commerce.name || 'N/A'}</td>
                    <td>${commerce.phone || 'N/A'}</td>
                    <td>${commerce.email || 'N/A'}</td>
                    <td><span class="badge bg-${getStatusColor(commerce.status)}">${commerce.status || 'N/A'}</span></td>
                </tr>
            `;
            tableBody.append(row);
        });
    }

    // Inicializar DataTable solo si hay datos
    if (data && data.length > 0) {
        $('#tblComercios').DataTable({
            responsive: true,
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
            },
            pageLength: 10,
            lengthChange: false,
            searching: true,
            ordering: true,
            info: true,
            autoWidth: false
        });
    }
}

function PopulateBankTable(data) {
    // Destruir DataTable existente si existe
    if ($.fn.DataTable.isDataTable('#tblBancos')) {
        $('#tblBancos').DataTable().destroy();
    }

    const tableBody = $('#tblBancos tbody');
    tableBody.empty();

    if (!data || data.length === 0) {
        tableBody.append('<tr><td colspan="6" class="text-center">No tiene solicitudes de entidad financiera</td></tr>');
    } else {
        data.forEach(function (bank) {
            const row = `
                <tr>
                    <td>${bank.legalId || 'N/A'}</td>
                    <td>${bank.bankCode || 'N/A'}</td>
                    <td>${bank.name || 'N/A'}</td>
                    <td>${bank.phone || 'N/A'}</td>
                    <td>${bank.email || 'N/A'}</td>
                    <td><span class="badge bg-${getStatusColor(bank.status)}">${bank.status || 'N/A'}</span></td>
                </tr>
            `;
            tableBody.append(row);
        });
    }

    // Inicializar DataTable solo si hay datos
    if (data && data.length > 0) {
        $('#tblBancos').DataTable({
            responsive: true,
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
            },
            pageLength: 10,
            lengthChange: false,
            searching: true,
            ordering: true,
            info: true,
            autoWidth: false
        });
    }
}

function getStatusColor(status) {
    if (!status) return 'secondary';

    switch (status.toLowerCase()) {
        case 'activo':
        case 'activa':
            return 'success';
        case 'pendiente':
            return 'warning';
        case 'rechazado':
        case 'rechazada':
            return 'danger';
        default:
            return 'secondary';
    }
}