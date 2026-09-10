$(document).ready(function () {
    LoadActiveCommerces();
});

function LoadActiveCommerces() {
    const user = JSON.parse(localStorage.getItem("user"));
    if (!user) {
        console.log('No hay usuario logueado');
        showNoCommerceMessage();
        return;
    }

    var ctrlActions = new ControlActions();

    ctrlActions.GetToApiWithCustomHeaders(
        `Commerce/RetrieveActiveByUser?userId=${user.id}`,
        function (data) {
            console.log('Comercios activos obtenidos:', data);
            if (data && data.length > 0) {
                PopulateCommerceCards(data);
            } else {
                showNoCommerceMessage();
            }
        },
        function (error) {
            console.log('Error al obtener comercios activos:', error);
            showNoCommerceMessage();
        },
        { 'LoggedUserId': user.id }
    );
}


function PopulateCommerceCards(commerces) {
    const container = $('#commerceContainer');
    container.empty();

    commerces.forEach(function (commerces) {
        const cardHtml = `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="card h-100 shadow-sm commerce-card">
                    <div class="card-header bg-primary text-white">
                        <h6 class="mb-0 fw-bold">${commerces.name}</h6>
                    </div>
                    
                    <div class="card-body">
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Cédula Jurídica</small>
                            </div>
                            <div class="col-7">
                                <small class="fw-bold">${formatLegalId(commerces.legalId)}</small>
                            </div>
                        </div>
                        
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Email</small>
                            </div>
                            <div class="col-7">
                                <small class="fw-bold">${commerces.email}</small>
                            </div>
                        </div>
                        
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Teléfono</small>
                            </div>
                            <div class="col-7">
                                <small class="fw-bold">${formatPhone(commerces.phone)}</small>
                            </div>
                        </div>
                        
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Estado</small>
                            </div>
                            <div class="col-7">
                                <span class="badge bg-success">${commerces.status}</span>
                            </div>
                        </div>
                        
                        <div class="row mb-2">
                            <div class="col-5">
                                <small class="text-muted fw-bold">Comisión</small>
                            </div>
                            <div class="col-7">
                                <small class="fw-bold text-success">${commerces.commissionRate}%</small>
                            </div>
                        </div>
                    </div>
                    
                    <div class="card-footer bg-light" style="border-radius: 0 0 15px 15px;">
                        <div class="d-grid gap-2">
                            <button class="btn btn-outline-primary btn-sm" onclick="viewCommerceDetails(${commerces.id})">
                                <i class="fas fa-eye me-1"></i>Ver Detalles
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        container.append(cardHtml);
    });

    // Ocultar mensaje de no hay comercios
    $('#noCommercesMessage').addClass('d-none');
}

function showNoCommerceMessage() {
    $('#commerceContainer').empty();
    $('#noCommercesMessage').removeClass('d-none');
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

function viewCommerceDetails(commerceId) {
    // Redirigir al dashboard del comercio específico pasando el ID como parámetro
    window.location.href = `/Commerce/Dashboard?commerceId=${commerceId}`;
}
