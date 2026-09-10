function PaymentHistoryController() {
    this.ApiGetUserHistory = "Payment/GetUserHistory";
    this.currentUser = null;
    this.payments = [];
    this.filteredPayments = [];

    const self = this; // para clausuras

    this.InitView = function () {
        this.LoadCurrentUser();

        if (!this.currentUser) {
            this.ShowError("Por favor, inicie sesión para ver su historial de pagos.", "warning");
            return;
        }

        // Cargar historial inicial (sin filtros)
        this.LoadPaymentHistory();

        // Eventos
        $("#btnFilter").on("click", function () {
            self.ApplyFilters();
        });

        $("#btnClear").on("click", function () {
            self.ClearFilters();
        });

        // Validar rango al cambiar cualquiera
        $("#startDate").on("change", function () {
            self.ValidateDateRange();
        });
        $("#endDate").on("change", function () {
            self.ValidateDateRange();
        });
    };

    this.LoadCurrentUser = function () {
        try {
            // Busca en localStorage con la clave habitual "user"
            const userStr = localStorage.getItem("user");
            if (!userStr) {
                console.warn("No hay usuario en localStorage key 'user'.");
                this.currentUser = null;
                return;
            }

            const u = JSON.parse(userStr);
            // Normaliza posibles nombres de propiedades
            this.currentUser = {
                firstName: u.firstName ?? u.FirstName ?? "",
                lastName1: u.lastName1 ?? u.LastName1 ?? u.lastName ?? u.LastName ?? "",
                nationalId: u.nationalId ?? u.NationalId ?? u.cedula ?? u.Cedula ?? ""
            };

            // Mensaje de bienvenida
            this.UpdateWelcomeMessage();
        } catch (error) {
            console.error("Error cargando usuario:", error);
            this.currentUser = null;
        }
    };

    this.UpdateWelcomeMessage = function () {
        if (this.currentUser?.firstName) {
            const fullName = `${this.currentUser.firstName} ${this.currentUser.lastName1}`.trim();
            const $title = $("#welcomeTitle");
            if ($title.length) $title.text(`¡Hola ${fullName}!`);
        }
    };

    this.LoadPaymentHistory = function (startDate = null, endDate = null) {
        const ctrlActions = new ControlActions();

        const nationalId = this.currentUser?.nationalId;
        if (!nationalId) {
            this.ShowError("No se pudo identificar al usuario.", "danger");
            return;
        }

        this.ShowLoading(true);

        // Construye query de forma segura
        const qs = new URLSearchParams();
        qs.set("nationalId", nationalId);

        if (startDate) qs.set("startDate", this.toIsoDate(startDate));
        if (endDate) qs.set("endDate", this.toIsoDate(endDate));

        ctrlActions.GetToApi(
            `${this.ApiGetUserHistory}?${qs.toString()}`,
            function (response) {
                // Esperamos un array de pagos
                self.payments = Array.isArray(response) ? response : [];
                self.filteredPayments = [...self.payments];
                self.RenderPayments();
                self.UpdateStatistics();
                self.HideError();
                self.ShowLoading(false);
            },
            function (error) {
                console.error("Error cargando pagos:", error);
                self.ShowError("Error al cargar el historial de pagos. Intente nuevamente.", "danger");
                self.ShowLoading(false);
            }
        );
    };

    this.RenderPayments = function () {
        const $tbody = $("#paymentsTableBody");
        const $tableContainer = $("#paymentsTableContainer");
        const $noPayments = $("#noPaymentsMessage");

        $tbody.empty();

        if (!this.filteredPayments.length) {
            $tableContainer.hide();
            $noPayments.show();
            $("#recordCount").text(0);
            $("#summaryTotal").text("₡0.00");
            return;
        }

        $tableContainer.show();
        $noPayments.hide();

        this.filteredPayments.forEach(p => {
            // Propiedades con fallbacks seguros
            const paymentDate = p.paymentDate ?? p.created ?? null;
            const gross = Number(p.grossAmount ?? 0);
            const net = Number(p.netAmount ?? 0);
            const amountWDiscount = Number(p.amountWDiscount ?? 0);

            const row = `
                <tr>
                    <td>${this.FormatDate(paymentDate)}</td>
                    <td>₡${this.FormatMoney(gross)}</td>
                    <td><span class="fw-bold text-success">₡${this.FormatMoney(net)}</span></td>
                    <td>${amountWDiscount > 0 ? `<span class="badge bg-warning">₡${this.FormatMoney(amountWDiscount)}</span>` : '<span class="text-muted">N/A</span>'}</td>
                    <td>${p.commerceName ?? 'Comercio'}</td>
                    <td>${p.description ?? '—'}</td>
                </tr>
            `;
            $tbody.append(row);
        });

        $("#recordCount").text(this.filteredPayments.length);
        $("#summaryTotal").text(`₡${this.FormatMoney(this.GetTotalAmount())}`);
    };

    this.UpdateStatistics = function () {
        const totalPaid = this.GetTotalAmount();
        const totalPayments = this.filteredPayments.length;
        const totalDiscounts = this.GetTotalDiscounts();

        $("#totalPaid").text(`₡${this.FormatMoney(totalPaid)}`);
        $("#totalPayments").text(totalPayments);
        $("#totalDiscounts").text(`₡${this.FormatMoney(totalDiscounts)}`);
    };

    this.ApplyFilters = function () {
        if (!this.ValidateDateRange()) return;

        const startDate = $("#startDate").val() || null;
        const endDate = $("#endDate").val() || null;

        this.ShowLoading(true);
        this.LoadPaymentHistory(startDate, endDate);
    };

    this.ClearFilters = function () {
        $("#startDate").val('');
        $("#endDate").val('');
        this.LoadPaymentHistory();
    };

    this.ValidateDateRange = function () {
        const s = $("#startDate").val();
        const e = $("#endDate").val();

        if (s && e) {
            const sd = new Date(s);
            const ed = new Date(e);
            if (isNaN(sd.getTime()) || isNaN(ed.getTime())) return true;
            if (sd > ed) {
                Swal.fire({ icon: "warning", title: "Fechas inválidas", text: "La fecha inicial no puede ser mayor que la fecha final" });
                $("#startDate").val('');
                return false;
            }
        }
        return true;
    };

    // ===== CORRECCIÓN: Calcular descuentos correctamente =====
    this.GetTotalDiscounts = function () {
        return this.filteredPayments.reduce((total, p) => {
            // AmountWDiscount ES el descuento aplicado
            const discount = Number(p.amountWDiscount ?? 0);
            return total + discount;
        }, 0);
    };

    // Suma el neto (solo pagos completados aportan)
    this.GetTotalAmount = function () {
        return this.filteredPayments.reduce((total, p) => {
            const net = Number(p.netAmount ?? 0);
            return total + net;
        }, 0);
    };

    this.ShowLoading = function (show) {
        if (show) {
            $("#loadingIndicator").show();
            $("#paymentsTableContainer").hide();
            $("#noPaymentsMessage").hide();
        } else {
            $("#loadingIndicator").hide();
        }
    };

    this.ShowError = function (message, type = "danger") {
        const $alertContainer = $("#alertContainer");
        const $alertMessage = $("#alertMessage");

        $alertMessage.removeClass().addClass(`alert alert-${type}`);
        $alertMessage.text(message);
        $alertContainer.show();
    };

    this.HideError = function () {
        $("#alertContainer").hide();
    };

    // ===== Utilidades =====
    this.FormatDate = function (dateLike) {
        if (!dateLike) return "-";
        const d = new Date(dateLike);
        if (isNaN(d.getTime())) return "-";
        return d.toLocaleDateString('es-CR', { day: '2-digit', month: '2-digit', year: 'numeric' });
    };

    this.FormatMoney = function (amount) {
        const n = Number(amount || 0);
        return new Intl.NumberFormat('es-CR').format(n);
    };

    this.toIsoDate = function (val) {
        // acepta "YYYY-MM-DD" o Date; devuelve "YYYY-MM-DD"
        if (!val) return "";
        if (val instanceof Date) {
            if (isNaN(val.getTime())) return "";
            return val.toISOString().slice(0, 10);
        }
        // si ya viene "YYYY-MM-DD", lo regresamos igual
        return String(val).slice(0, 10);
    };
}

/*function PaymentHistoryController() {
    // API endpoint siguiendo el patrón de tu proyecto
    this.ApiGetUserHistory = "Payment/GetUserHistory";

    // Variables de estado
    this.currentUser = null;
    this.payments = [];
    this.filteredPayments = [];

    this.InitView = function () {
        var ctrlActions = new ControlActions();
        var self = this;

        // Verificar usuario logueado
        this.LoadCurrentUser();

        if (!this.currentUser) {
            this.ShowError("Por favor, inicie sesión para ver su historial de pagos.", "warning");
            return;
        }

        // Cargar historial inicial
        this.LoadPaymentHistory();

        // Event listeners para filtros
        $("#btnFilter").on("click", function () {
            self.ApplyFilters();
        });

        $("#btnClear").on("click", function () {
            self.ClearFilters();
        });

        // Validación de fechas en tiempo real
        $("#startDate").on("change", function () {
            self.ValidateDateRange();
        });

        $("#endDate").on("change", function () {
            self.ValidateDateRange();
        });
    };

    this.LoadCurrentUser = function () {
        try {
            const userStr = localStorage.getItem("user");
            if (userStr) {
                this.currentUser = JSON.parse(userStr);
                console.log("Usuario cargado:", this.currentUser);

                // Actualizar el título con el nombre completo del usuario
                this.UpdateWelcomeMessage();

            } else {
                console.log("No hay usuario en localStorage");
            }
        } catch (error) {
            console.error("Error cargando usuario:", error);
            this.currentUser = null;
        }
    };

    this.UpdateWelcomeMessage = function () {
        console.log("Actualizando mensaje de bienvenida...");
        console.log("Usuario actual:", this.currentUser);

        if (this.currentUser && this.currentUser.firstName && this.currentUser.lastName1) {
            const fullName = `${this.currentUser.firstName} ${this.currentUser.lastName1}`;
            console.log("Nombre completo:", fullName);

            // Verificar que el elemento existe
            const titleElement = $("#welcomeTitle");
            console.log("Elemento encontrado:", titleElement.length);

            if (titleElement.length > 0) {
                titleElement.text(`¡Hola ${fullName}!`);
                console.log("Título actualizado a:", `¡Hola ${fullName}!`);
            } else {
                console.log("ERROR: No se encontró el elemento #welcomeTitle");
            }
        } else {
            console.log("Faltan datos del usuario:", {
                firstName: this.currentUser?.firstName,
                lastName1: this.currentUser?.lastName1
            });
        }
    };

    this.LoadPaymentHistory = function (startDate = null, endDate = null) {
        var ctrlActions = new ControlActions();
        var self = this;

        if (!this.currentUser || !this.currentUser.nationalId) {
            this.ShowError("No se pudo identificar al usuario", "danger");
            return;
        }

        // Mostrar indicador de carga
        this.ShowLoading(true);

        // Construir parámetros de la URL
        var params = `nationalId=${this.currentUser.nationalId}`;
        if (startDate) params += `&startDate=${startDate}`;
        if (endDate) params += `&endDate=${endDate}`;

        // Llamada al API - CORREGIDO: GetToApi (no GetToAPI)
        ctrlActions.GetToApi(
            `${this.ApiGetUserHistory}?${params}`,
            function (response) {
                console.log("Pagos recibidos:", response);
                self.payments = response || [];
                self.filteredPayments = [...self.payments];
                self.RenderPayments();
                self.UpdateStatistics();
                self.ShowLoading(false);
                self.HideError();
            },
            function (error) {
                console.error("Error cargando pagos:", error);
                self.ShowError("Error al cargar el historial de pagos. Intente nuevamente.", "danger");
                self.ShowLoading(false);
            }
        );
    };

    this.RenderPayments = function () {
        const tableBody = $("#paymentsTableBody");
        const tableContainer = $("#paymentsTableContainer");
        const noPaymentsMessage = $("#noPaymentsMessage");
        const self = this;

        // Limpiar tabla
        tableBody.empty();

        if (this.filteredPayments.length === 0) {
            // Mostrar mensaje de sin pagos
            tableContainer.hide();
            noPaymentsMessage.show();
            return;
        }

        // Mostrar tabla
        tableContainer.show();
        noPaymentsMessage.hide();

        // Llenar tabla
        this.filteredPayments.forEach(function (payment) {
            const row = `
                <tr>
                    <td>${self.FormatDate(payment.paymentDate)}</td>
                    <td>₡${self.FormatMoney(payment.grossAmount)}</td>
                    <td>
                        <span class="fw-bold text-success">
                            ₡${self.FormatMoney(payment.netAmount)}
                        </span>
                    </td>
                    <td>${self.FormatDiscount(payment)}</td>
                    <td>${payment.commerceName || 'Comercio'}</td>
                    <td>${payment.description || 'Descripción del pago'}</td>
                </tr>
            `;
            tableBody.append(row);
        });

        // Actualizar contador de registros
        $("#recordCount").text(this.filteredPayments.length);
        $("#summaryTotal").text(`₡${this.FormatMoney(this.GetTotalAmount())}`);
    };

    this.UpdateStatistics = function () {
        const totalPaid = this.GetTotalAmount();
        const totalPayments = this.filteredPayments.length;
        const totalDiscounts = this.GetTotalDiscounts();

        $("#totalPaid").text(`₡${this.FormatMoney(totalPaid)}`);
        $("#totalPayments").text(totalPayments);
        $("#totalDiscounts").text(`₡${this.FormatMoney(totalDiscounts)}`);
    };

    this.ApplyFilters = function () {
        const startDate = $("#startDate").val();
        const endDate = $("#endDate").val();

        if (!this.ValidateDateRange()) {
            return;
        }

        // Mostrar loading
        this.ShowLoading(true);

        // Cargar con filtros
        this.LoadPaymentHistory(startDate, endDate);
    };

    this.ClearFilters = function () {
        $("#startDate").val('');
        $("#endDate").val('');

        // Recargar todos los datos
        this.LoadPaymentHistory();
    };

    this.ValidateDateRange = function () {
        const startDate = $("#startDate").val();
        const endDate = $("#endDate").val();

        if (startDate && endDate && startDate > endDate) {
            Swal.fire({
                icon: "warning",
                title: "Fechas inválidas",
                text: "La fecha inicial no puede ser mayor que la fecha final"
            });
            $("#startDate").val('');
            return false;
        }
        return true;
    };

    // Funciones de utilidad
    this.FormatDate = function (dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('es-CR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    };

    this.FormatMoney = function (amount) {
        return new Intl.NumberFormat('es-CR').format(amount);
    };

    this.FormatDiscount = function (payment) {
        if (payment.amountWDiscount && payment.amountWDiscount > 0) {
            const discount = payment.amountWDiscount;
            return `<span class="badge bg-warning">₡${this.FormatMoney(discount)}</span>`;
        }
        return '<span class="text-muted">N/A</span>';
    };

    this.GetTotalAmount = function () {
        return this.filteredPayments.reduce((total, payment) => total + (payment.netAmount || 0), 0);
    };

    this.GetTotalDiscounts = function () {
        return this.filteredPayments.reduce((total, payment) => {
            if (payment.amountWDiscount && payment.amountWDiscount > 0) {
                return total + payment.amountWDiscount;
            }
            return total;
        }, 0);
    };

    this.ShowLoading = function (show) {
        if (show) {
            $("#loadingIndicator").show();
            $("#paymentsTableContainer").hide();
            $("#noPaymentsMessage").hide();
        } else {
            $("#loadingIndicator").hide();
        }
    };

    this.ShowError = function (message, type = "danger") {
        const alertContainer = $("#alertContainer");
        const alertMessage = $("#alertMessage");

        alertMessage.removeClass().addClass(`alert alert-${type}`);
        alertMessage.text(message);
        alertContainer.show();
    };

    this.HideError = function () {
        $("#alertContainer").hide();
    };

    // Variable para acceso global (siguiendo tu patrón)
    var self = this;
}*/