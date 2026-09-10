function CommerceRequestController() {
    this.ApiRetrieve = "Commerce/RetrieveById";
    this.ApiApprove = "Commerce/Approve";
    this.ApiReject = "Commerce/Reject";
    this.id = new URLSearchParams(window.location.search).get("id");

    this.InitView = function () {
        var ctrlActions = new ControlActions();

        // Carga datos del comercio
        ctrlActions.GetToApi(this.ApiRetrieve + "?id=" + this.id, function (data) {
            ctrlActions.BindFields("frmComercio", data);
            initMap(data.latitude, data.longitude);
        });

        // Mostrar/ocultar campo de comisión
        $("#selectDecision").on("change", function () {
            if ($(this).val() === "aprobar") {
                $("#comisionGroup").removeClass("d-none");
                $("#commissionRate").attr("required", true);
            } else {
                $("#comisionGroup").addClass("d-none");
                $("#commissionRate").removeAttr("required");
            }
        });

        // Validación visual del campo de comisión
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

        // Cancelar
        $("#btnCancelar").on("click", function () {
            Swal.fire({
                title: '¿Desea cancelar?',
                text: 'Se perderán los cambios no guardados',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Sí, cancelar',
                cancelButtonText: 'Continuar editando'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = "/Admin/PendingCommerce";
                }
            });
        });

        // Guardar
        $("#btnGuardar").on("click", () => {
            const decision = $("#selectDecision").val();
            const data = ctrlActions.GetDataForm("frmComercio");
            console.log(data);

            const nationalId = data.nationalId;

            if (!nationalId || !/^\d{9}$/.test(nationalId)) {
                Swal.fire("Cédula inválida", "Debe incluir una cédula válida del administrador (9 dígitos)", "error");
                return;
            }

            if (!decision) {
                Swal.fire("Seleccione una decisión", "Debe elegir entre aprobar o rechazar la solicitud", "warning");
                return;
            }

            if (!data.name || !data.email || !data.phone) {
                Swal.fire("Faltan datos", "Por favor revise que todos los campos estén presentes.", "warning");
                return;
            }

            if (decision === "aprobar") {
                const commission = parseFloat(data.commissionRate);

                if (isNaN(commission) || commission < 0.01 || commission > 99.99) {
                    Swal.fire("Comisión inválida", "Debe ingresar una comisión válida entre 0.01% y 99.99%", "error");
                    return;
                }

                Swal.fire({
                    title: '¿Aprobar este comercio?',
                    html: `
                        <div class="text-start">
                            <p><strong>Comercio:</strong> ${data.name}</p>
                            <p><strong>Cédula administrador:</strong> ${data.nationalId}</p>
                            <p><strong>Comisión asignada:</strong> ${commission}%</p>
                            <hr>
                            <p class="text-success"><i class="fas fa-check-circle me-2"></i>Al aprobar:</p>
                            <ul class="text-muted">
                                <li>El comercio quedará <strong>ACTIVO</strong></li>
                                <li>Podrá operar en la plataforma</li>
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
                        data.commissionRate = commission;
                        data.status = "Activo";

                        ctrlActions.PutToAPI(this.ApiApprove, data,
                            function () {
                                Swal.fire({
                                    icon: 'success',
                                    title: '¡Comercio aprobado!',
                                    html: `
                                        <p><strong>${data.name}</strong> ha sido aprobado exitosamente.</p>
                                        <p>Estado: ACTIVO</p>
                                        <p>Comisión: ${commission}%</p>
                                    `,
                                    confirmButtonText: 'Continuar'
                                }).then(() => {
                                    window.location.href = "/Admin/PendingCommerce";
                                });
                            },
                            function (error) {
                                const msg = error.responseText || "Ocurrió un error inesperado al aprobar el comercio.";
                                Swal.fire("No se pudo aprobar", msg, "error");
                            }
                        );
                    }
                });
            }

            if (decision === "rechazar") {
                Swal.fire({
                    title: '¿Rechazar este comercio?',
                    html: `
                        <div class="text-start">
                            <p><strong>Comercio:</strong> ${data.name}</p>
                            <hr>
                            <p class="text-danger"><i class="fas fa-exclamation-triangle me-2"></i>Al rechazar:</p>
                            <ul class="text-muted">
                                <li>El comercio quedará <strong>RECHAZADO</strong></li>
                                <li>No podrá operar en la plataforma</li>
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
                        data.commissionRate = 0;
                        data.status = "Rechazado";

                        ctrlActions.PutToAPI(this.ApiReject, data,
                            function () {
                                Swal.fire({
                                    icon: 'info',
                                    title: 'Comercio rechazado',
                                    html: `
                                        <p><strong>${data.name}</strong> ha sido rechazado correctamente.</p>
                                        <p>Estado: RECHAZADO</p>
                                    `,
                                    confirmButtonText: 'Continuar'
                                }).then(() => {
                                    window.location.href = "/Admin/PendingCommerce";
                                });
                            },
                            function (error) {
                                const msg = error.responseText || "Error al rechazar el comercio.";
                                Swal.fire("No se pudo rechazar", msg, "error");
                            }
                        );
                    }
                });
            }
        });
    };
}

// Mapa con Leaflet
function initMap(lat, lng) {
    const map = L.map('map').setView([lat, lng], 15);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    L.marker([lat, lng]).addTo(map)
        .bindPopup(`<strong>${$('#frmComercio input[ColumnDataName="name"]').val()}</strong><br>Ubicación registrada`)
        .openPopup();
}

$(document).ready(function () {
    const view = new CommerceRequestController();
    view.InitView();
});
