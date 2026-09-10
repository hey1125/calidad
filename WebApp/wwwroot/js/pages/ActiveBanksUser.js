$(document).ready(function () {
    LoadActiveBanks();
});

function LoadActiveBanks() {
    const user = JSON.parse(localStorage.getItem("user"));
    if (!user) {
        console.log('No hay usuario logueado');
        showNoBanksMessage();
        return;
    }

    var ctrlActions = new ControlActions();

    ctrlActions.GetToApiWithCustomHeaders(
        `FinancialEntity/RetrieveActiveByUser?userId=${user.id}`,
        function (data) {
            console.log('Bancos activos obtenidos:', data);
            if (data && data.length > 0) {
                PopulateBankCards(data);
            } else {
                showNoBanksMessage();
            }
        },
        function (error) {
            console.log('Error al obtener bancos activos:', error);
            showNoBanksMessage();
        },
        { 'LoggedUserId': user.id }
    );
}

function PopulateBankCards(banks) {
    const container = $('#banksContainer');
    container.empty();

    banks.forEach(function (bank) {
        const cardHtml = `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="card h-100 shadow-sm" style="border-radius: 15px;">
                    <div class="card-header bg-primary text-white" style="border-radius: 15px 15px 0 0;">
                        <h6 class="mb-0 fw-bold">
                            <i class="fas fa-university me-2"></i>
                            ${bank.name}
                        </h6>
                    </div>
                    <div class="card-body">
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Código</small>
                            </div>
                            <div class="col-7">
                                <small class="fw-bold">${bank.bankCode}</small>
                            </div>
                        </div>
                        
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Cédula Jurídica</small>
                            </div>
                            <div class="col-7">
                                <small class="fw-bold">${formatLegalId(bank.legalId)}</small>
                            </div>
                        </div>
                        
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Email</small>
                            </div>
                            <div class="col-7">
                                <small class="fw-bold">${bank.email}</small>
                            </div>
                        </div>
                        
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Teléfono</small>
                            </div>
                            <div class="col-7">
                                <small class="fw-bold">${formatPhone(bank.phone)}</small>
                            </div>
                        </div>
                        
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Estado</small>
                            </div>
                            <div class="col-7">
                                <span class="badge bg-success">${bank.status}</span>
                            </div>
                        </div>
                        
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Comisión</small>
                            </div>
                            <div class="col-7">
                                <small class="fw-bold text-success">${bank.commissionRate}%</small>
                            </div>
                        </div>
                    </div>
                    
                    <div class="card-footer bg-light" style="border-radius: 0 0 15px 15px;">
                        <div class="d-grid gap-2">
                            <button class="btn btn-outline-primary btn-sm" onclick="viewBankDetails(${bank.id})">
                                <i class="fas fa-eye me-1"></i>Ver Detalles
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        container.append(cardHtml);
    });

    // Ocultar mensaje de no hay bancos
    $('#noBanksMessage').addClass('d-none');
}

function showNoBanksMessage() {
    $('#banksContainer').empty();
    $('#noBanksMessage').removeClass('d-none');
}

function formatLegalId(legalId) {
    if (!legalId) return 'N/A';
    // Formato: 3-101-123456
    return legalId.replace(/(\d{1})(\d{3})(\d{6})/, '$1-$2-$3');
}

function formatPhone(phone) {
    if (!phone) return 'N/A';
    // Formato: 2222-2222
    return phone.replace(/(\d{4})(\d{4})/, '$1-$2');
}

function viewBankDetails(bankId) {
    window.location.href = `/Bank/Dashboard?bankId=${bankId}`;
}