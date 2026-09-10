function CommercePromotionsController() {
    this.Api = "CommercePromotion";
    this.ctrlActions = new ControlActions();
    this.commerceId = null; // se define en InitView leyendo el URL

    this.InitView = function () {
        // Leer CommerceId desde querystring (?commerceId=123) o desde segmento /Promotions/123 si usas routing así
        const qs = new URLSearchParams(window.location.search);
        const qsId = parseInt(qs.get('commerceId'), 10);
        const segId = (function () {
            const parts = window.location.pathname.split('/').filter(Boolean);
            const last = parseInt(parts[parts.length - 1], 10);
            return Number.isFinite(last) ? last : null;
        })();

        this.commerceId = Number.isFinite(qsId) && qsId > 0 ? qsId : (Number.isFinite(segId) ? segId : null);

        if (!this.commerceId) {
            Swal.fire({ icon: "error", title: "CommerceId requerido", text: "Agrega ?commerceId={id} en la URL." });
            disableAll();
            return;
        }

        // Mostrarlo en la UI
        $('#lblCommerceId').text(this.commerceId);

        // Botones
        $('#btn-best').on('click', () => this.LoadBest());
        $('#btn-save').on('click', () => this.Save());
        $('#btn-reset').on('click', () => this.ResetForm());
        $('#btn-cancel-edit').on('click', () => this.CancelEdit());

        // Prefill fechas
        const nowLocal = new Date(Date.now() - (new Date()).getTimezoneOffset() * 60000)
            .toISOString().slice(0, 16);
        $('#refDateFilter').val(nowLocal);
        $('#StartDate').val(nowLocal);

        this.LoadAll();
    };

    // ================== Consultas (siempre con this.commerceId) ==================

    this.LoadBest = function () {
        if (!this.ensureCommerceId()) return;

        const { amount, refDate } = getFilters();
        if (!amount) return;

        const url = `${this.Api}/RetrieveBestByCommerce?commerceId=${this.commerceId}&amount=${amount}`
            + (refDate ? `&refDate=${encodeURIComponent(refDate)}` : '');

        this.ctrlActions.GetToApiWithCustomHeaders(
            url,
            (promo) => this.RenderTable(promo ? [promo] : [], amount),
            () => this.showNoPromosMessage(),
            {}
        );
    };

    // NUEVO: cargar todas por commerceId (desde la URL)
    this.LoadAll = function () {
        if (!this.ensureCommerceId()) return;

        // amount es opcional; solo lo usamos para calcular "Descuento Efectivo" en la tabla
        const { amount } = getFilters(true); // true: no obligar monto

        const url = `${this.Api}/RetrieveAllByCommerce?commerceId=${this.commerceId}`;

        this.ctrlActions.GetToApiWithCustomHeaders(
            url,
            (data) => this.RenderTable(data, amount || null),
            () => this.showNoPromosMessage(),
            {}
        );
    };


    // ============ Create / Update / Delete ============
    this.Save = function () {
        if (!this.ensureCommerceId()) return;

        const dto = this.ReadForm();
        if (!dto) return;

        // Forzar commerceId desde URL
        dto.commerceId = this.commerceId;

        const editing = $('#EditingId').val();
        if (editing) {
            this.ctrlActions.PutToAPI(
                `${this.Api}/Update/${editing}`,
                dto,
                () => { this.CancelEdit(); this.LoadAll(); }
            );
        } else {
            this.ctrlActions.PostToAPI(
                `${this.Api}/Create`,
                dto,
                () => { this.ResetForm(); this.LoadAll(); }
            );
        }
    };

    this.StartEdit = function (promoId) {
        if (!this.ensureCommerceId()) return;

        const url = `${this.Api}/RetrieveById/${promoId}`;

        this.ctrlActions.GetToApiWithCustomHeaders(
            url,
            (p) => {
                if (!p) return;

                $('#EditingId').val(p.id);
                $('#form-title').text('Editar promoción');

                $('#CoPromoId').val(p.coPromoId);
                $('#StartDate').val(toLocalInput(p.startDate));
                $('#EndDate').val(toLocalInput(p.endDate));
                setOrClear('#MinAmount', p.minAmount);
                setOrClear('#AmountPercentage', p.amountPercentage);
                setOrClear('#AmountDiscount', p.amountDiscount);
                setOrClear('#TotalCoupon', p.totalCoupon);
                setOrClear('#UsedCoupon', p.usedCoupon);
                $('#Status').val(p.status);
                $('#Description').val(p.description || '');

                $('#btn-cancel-edit').removeClass('d-none');
                $('#btn-save').html('<i class="fa-solid fa-floppy-disk me-1"></i> Actualizar');
            },
            (err) => {
                // NotFound u otros errores -> alerta coherente
                const msg = err?.responseText || "No se pudo cargar la promoción.";
                Swal.fire({ icon: "error", title: "Oops...", text: msg });
            },
            {}
        );
    };


    this.Delete = function (id) {
        Swal.fire({
            title: "¿Eliminar promoción?",
            text: "Esta acción no se puede deshacer.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Sí, eliminar"
        }).then((result) => {
            if (result.isConfirmed) {
                this.ctrlActions.DeleteToAPI(
                    `${this.Api}/Delete/${id}`,
                    {},
                    () => { this.LoadAll(); }
                );
            }
        });
    };

    this.ensureCommerceId = function () {
        if (!this.commerceId || this.commerceId <= 0) {
            Swal.fire({ icon: "error", title: "CommerceId requerido", text: "Proporcione ?commerceId={id} en la URL." });
            return false;
        }
        return true;
    };

    // ================== UI ==================
    this.RenderTable = function (rows, amountForEffective) {
        const tbody = $('#promos-body');
        tbody.empty();

        if (!rows || !rows.length) {
            this.showNoPromosMessage();
            return;
        }

        rows.forEach((p) => {
            const eff = computeEffectiveDiscount(p, amountForEffective);
            const coupons = `${(p.totalCoupon ?? "-")}/${(p.usedCoupon ?? 0)}`;

            const tr = `
                <tr>
                    <td>${escapeHtml(p.coPromoId)}</td>
                    <td>${fmtDt(p.startDate)}</td>
                    <td>${fmtDt(p.endDate)}</td>
                    <td>${fmtMoney(p.minAmount)}</td>
                    <td>${fmtPct(p.amountPercentage)}</td>
                    <td>${fmtMoney(p.amountDiscount)}</td>
                    <td>${coupons}</td>
                    <td><span class="badge ${p.status === 'Activa' ? 'bg-success' : 'bg-secondary'}">${p.status}</span></td>
                    <td>${fmtMoney(eff)}</td>
                    <td>
                        <div class="btn-group btn-group-sm" role="group">
                            <button class="btn btn-outline-primary" title="Editar" onclick="commercePromotionsController.StartEdit(${p.id})">
                                <i class="fa-solid fa-pen-to-square"></i>
                            </button>
                            <button class="btn btn-outline-danger" title="Eliminar" onclick="commercePromotionsController.Delete(${p.id})">
                                <i class="fa-solid fa-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>`;
            tbody.append(tr);
        });

        $('#noPromosMessage').addClass('d-none');
    };

    this.showNoPromosMessage = function () {
        $('#promos-body').empty();
        $('#noPromosMessage').removeClass('d-none');
    };

    this.ResetForm = function () {
        $('#EditingId').val('');
        $('#form-title').text('Crear promoción');
        $('#btn-cancel-edit').addClass('d-none');
        $('#btn-save').html('<i class="fa-solid fa-floppy-disk me-1"></i> Guardar');

        $('#CoPromoId').val('');
        $('#MinAmount').val('');
        $('#AmountPercentage').val('');
        $('#AmountDiscount').val('');
        $('#TotalCoupon').val('');
        $('#UsedCoupon').val('');
        $('#Status').val('Activa');
        $('#Description').val('');
        // mantiene fechas
    };

    this.CancelEdit = function () {
        this.ResetForm();
    };

    this.ReadForm = function () {
        const dto = {
            coPromoId: strVal('#CoPromoId'),
            startDate: toIso($('#StartDate').val()),
            endDate: toIso($('#EndDate').val()),
            status: $('#Status').val(),
            description: strVal('#Description', true)
        };

        addIfNotNull(dto, 'minAmount', numVal('#MinAmount', true));
        addIfNotNull(dto, 'amountPercentage', numVal('#AmountPercentage', true));
        addIfNotNull(dto, 'amountDiscount', numVal('#AmountDiscount', true));
        addIfNotNull(dto, 'totalCoupon', intVal('#TotalCoupon', true));
        addIfNotNull(dto, 'usedCoupon', intVal('#UsedCoupon', true));

        // Validaciones básicas
        if (!dto.coPromoId || !dto.startDate || !dto.endDate || !dto.status) {
            Swal.fire({ icon: "error", title: "Campos requeridos", text: "Completa CoPromoId, fechas y estado." });
            return null;
        }
        if (new Date(dto.endDate) < new Date(dto.startDate)) {
            Swal.fire({ icon: "error", title: "Rango de fechas", text: "EndDate debe ser mayor o igual a StartDate." });
            return null;
        }
        if (dto.usedCoupon != null && (dto.totalCoupon == null || dto.usedCoupon > dto.totalCoupon)) {
            Swal.fire({ icon: "error", title: "Cupones", text: "UsedCoupon requiere TotalCoupon y no puede excederlo." });
            return null;
        }
        return dto;
    };

    // ================= Helpers =================
    function getFilters(soft) {
        const amountStr = $('#amountFilter').val();
        const amount = parseFloat(amountStr);
        const refIn = $('#refDateFilter').val();
        const refDate = refIn ? toIso(refIn) : null;

        if (!soft) {
            if (!amount || amount <= 0) {
                Swal.fire({ icon: "error", title: "Monto inválido" });
                return {};
            }
        }

        return { amount: amount && amount > 0 ? amount : null, refDate };
    }

    function computeEffectiveDiscount(p, amount) {
        const pct = (p.amountPercentage ?? 0);
        const fix = (p.amountDiscount ?? 0);
        const fromPct = amount ? amount * (pct / 100.0) : 0;
        return Math.max(fromPct, fix);
    }

    function fmtMoney(v) { if (v == null || v === '') return '-'; return Number(v).toFixed(2); }
    function fmtPct(v) { if (v == null || v === '') return '-'; return Number(v).toFixed(2) + '%'; }
    function fmtDt(dt) { if (!dt) return '-'; const d = new Date(dt); return d.toLocaleString(); }
    function toIso(localVal) { if (!localVal) return null; const d = new Date(localVal); return d.toISOString(); }
    function toLocalInput(dt) { if (!dt) return ''; const d = new Date(dt); const tz = d.getTimezoneOffset() * 60000; return new Date(d - tz).toISOString().slice(0, 16); }
    function setOrClear(sel, val) { $(sel).val(val == null ? '' : val); }
    function numVal(sel, allowNull) { const v = $(sel).val(); if (v === '' || v == null) return allowNull ? null : 0; const n = parseFloat(v); return isNaN(n) ? (allowNull ? null : 0) : n; }
    function intVal(sel, allowNull) { const v = $(sel).val(); if (v === '' || v == null) return allowNull ? null : 0; const n = parseInt(v, 10); return isNaN(n) ? (allowNull ? null : 0) : n; }
    function strVal(sel, allowEmpty) { const v = ($(sel).val() || '').toString().trim(); if (!allowEmpty && !v) return ''; return v || null; }
    function escapeHtml(s) { if (s == null) return ''; return s.replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c])); }
    function addIfNotNull(obj, key, val) { if (val !== null) obj[key] = val; }
    function disableAll() { $('button, input, select, textarea').prop('disabled', true); }
}

// Instancia global
const commercePromotionsController = new CommercePromotionsController();
$(document).ready(function () {
    commercePromotionsController.InitView();
});

// SweetAlert override para parsear errores JSON
(function () {
    const originalSwalFire = Swal.fire;
    Swal.fire = function () {
        if (arguments.length === 1 && typeof arguments[0] === "object") {
            const options = arguments[0];
            if (options.html && typeof options.html === "string" && options.html.includes('"error"')) {
                try { const parsed = JSON.parse(options.html); if (parsed.error) options.html = parsed.error; } catch (e) { }
            }
            return originalSwalFire.call(this, options);
        }
        return originalSwalFire.apply(this, arguments);
    };
})();
