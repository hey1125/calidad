// ENDPOINTS

const BANK_ACCOUNTS_ENDPOINT = "BankAccount/GetAccountsByUser"; // ?userId=123
// Alineados con los controllers
const CO_PROMOS_ENDPOINT = "CommercePromotion/RetrieveActiveByCommerce";    // ?commerceId=1&amount=1000
const FE_PROMOS_ENDPOINT = "FinancialEntityPromotion/RetrieveActiveByIBAN"; // ?iban=CR..&amount=1000

// Keys en Storage
const SESSION_USER_ID_KEYS = ["userId", "LoggedUserId", "UserId", "id"];
const LOCAL_USER_KEYS = ["LoggedUser", "FinalUser", "user", "loggedUser"];

let dt;
let ctrl = new ControlActions();
let currentUser = null;
let currentNationalId = null;
let userBankAccounts = []; // {iban, alias?, bankName?}

$(document).ready(function () {
    currentUser = getUserFromLocalStorage();
    currentNationalId = extractNationalId(currentUser);

    if (!currentNationalId) {
        Swal.fire("Sesión inválida", "No se pudo obtener tu cédula. Inicia sesión nuevamente.", "error");
        return;
    }

    showUserInfo(currentUser, currentNationalId);
    initTable();
    loadPendingPayments(currentNationalId);

    $("#btnRefrescar").on("click", function () {
        loadPendingPayments(currentNationalId, true);
    });

    preloadUserBankAccounts();
});

/* ---------- Helpers de sesión ---------- */
function getUserFromLocalStorage() {
    for (const key of LOCAL_USER_KEYS) {
        const raw = localStorage.getItem(key);
        if (!raw) continue;
        try {
            const obj = JSON.parse(raw);
            if (obj && typeof obj === "object") return obj;
        } catch (_) { }
    }
    return null;
}

function extractNationalId(user) {
    if (!user) return null;
    return user.nationalId || user.NationalId || null;
}

function getSessionUserId() {
    for (const k of SESSION_USER_ID_KEYS) {
        const v = sessionStorage.getItem(k);
        if (v) return v;
    }
    return currentUser?.id || currentUser?.Id || null;
}

function showUserInfo(user, nationalId) {
    const el = $("#userInfo");
    const name = [user?.firstName, user?.lastName1, user?.lastName2].filter(Boolean).join(" ");
    el.html(`<i class="fa-solid fa-user me-2"></i> Sesión: <b>${name}</b> — Cédula: <b>${nationalId}</b>`);
    el.show();
}

function preloadUserBankAccounts() {
    const userId = getSessionUserId();
    if (!userId) return;

    const url = `${BANK_ACCOUNTS_ENDPOINT}?userId=${encodeURIComponent(userId)}`;
    ctrl.GetToApi(url, function (data) {
        userBankAccounts = (Array.isArray(data) ? data : []).map(x => ({
            iban: x.iban || x.IBAN || "",
            alias: x.alias || x.Alias || "",
            bankName: x.bankName || x.BankName || "",
        })).filter(x => x.iban);
    }, function (err) {
        console.error("Error loading accounts:", err);
        Swal.fire("Error", "No se pudieron cargar tus cuentas IBAN.", "error");
    });
}

/* ---------- IBAN utils ---------- */
function isCRIBAN(iban) {
    return /^CR\d{20}$/.test((iban || "").toUpperCase().replace(/\s+/g, ""));
}

/* ---------- DataTable ---------- */
function initTable() {
    dt = $("#tblPayments").DataTable({
        responsive: true,
        paging: true,
        searching: true,
        info: true,
        order: [[0, "desc"]],
        columns: [
            { data: "id" },
            { data: "commerceName", defaultContent: "—" },
            { data: "netAmount", render: (v) => `₡${Number(v || 0).toFixed(2)}` },
            { data: "status" },
            {
                data: "created",
                render: (v, t, row) => {
                    const val = v || row.Created || row.createdAt || row.CreatedAt;
                    return val ? new Date(val).toLocaleString() : "—";
                }
            },
            {
                data: null,
                orderable: false,
                render: (_, __, row) => {
                    const status = String(row.status ?? '').toLowerCase();
                    if (status === 'pendiente') {
                        return `<button class="btn btn-sm btn-success" onclick="payPrompt(${row.id}, ${Number(row.netAmount)}, ${row.commerceId})">
                                    <i class="fa-solid fa-credit-card"></i> Pagar
                                </button>`;
                    }
                    return `<span class="text-muted">—</span>`;
                }
            }
        ],
        language: { url: "https://cdn.datatables.net/plug-ins/2.0.8/i18n/es-ES.json" }
    });
}


/* ---------- Carga de pagos ---------- */
function loadPendingPayments(nationalId, showToast) {
    if (showToast) {
        Swal.fire({ title: "Actualizando...", didOpen: () => Swal.showLoading(), allowOutsideClick: false });
    }
    ctrl.GetToApi(`Payment/GetUserHistoryPayment?nationalId=${encodeURIComponent(nationalId)}`,
        function (data) {
            const pendientes = (Array.isArray(data) ? data : []).filter(x => String(x.status || "").toLowerCase() === "pendiente");
            pendientes.forEach(p => {
                p.commerceName = p.commerceName || p.commerce?.name || "—";
                p.created = p.created || p.Created || null;
            });
            dt.clear().rows.add(pendientes).draw();
            if (showToast) Swal.close();
        },
        function () {
            if (showToast) Swal.close();
            Swal.fire("Error", "No se pudo cargar la lista de pagos.", "error");
        }
    );
}

/* ---------- Helper robusto para GET (no bloquea en 404) ---------- */
async function getJsonSafe(url) {
    return await new Promise((resolve) => {
        let settled = false;
        const done = (data) => { if (!settled) { settled = true; resolve(Array.isArray(data) ? data : []); } };
        try {
            ctrl.GetToApi(url, d => done(d), _ => done([]));
        } catch (_) { done([]); }
        // Fallback por si el helper no dispara onError
        setTimeout(() => done([]), 1500);
    });
}

/* ---------- Modal de pago ---------- */
async function payPrompt(paymentId, netAmount, commerceId) {
    const hasAccounts = userBankAccounts && userBankAccounts.length > 0;
    const options = hasAccounts
        ? userBankAccounts.map(a => {
            const short = a.iban.slice(0, 8) + "…" + a.iban.slice(-4);
            const label = [a.alias, a.bankName, short].filter(Boolean).join(" — ");
            return `<option value="${a.iban}">${label}</option>`;
        }).join("")
        : "";

    let promosLoaded = false;

    const { value: form } = await Swal.fire({
        title: 'Procesar pago',
        html: `
            <div class="text-start">
                <div class="small text-muted mb-2">Neto a pagar: <b>₡${Number(netAmount).toFixed(2)}</b></div>
                ${hasAccounts ?
                `<label class="form-label">Selecciona tu cuenta IBAN</label>
                 <select id="swalIbanSelect" class="form-select">${options}</select>` :
                `<label class="form-label">IBAN</label>
                 <input id="swalIBAN" class="form-control" placeholder="CR00000000000000000000" />`
            }
                <div id="promoList" class="mt-3 small text-muted">Ingrese un IBAN válido para ver promociones.</div>
            </div>
        `,
        didOpen: async () => {
            const promoDiv = document.getElementById("promoList");

            const loadPromos = async (iban) => {
                promosLoaded = false;

                if (!iban || !isCRIBAN(iban)) {
                    promoDiv.innerHTML = "Ingrese un IBAN CR válido para ver promociones.";
                    promosLoaded = true;
                    return;
                }

                promoDiv.innerHTML = "Cargando promociones…";

                const commerceUrl = `${CO_PROMOS_ENDPOINT}?commerceId=${commerceId}&amount=${netAmount}`;
                const bankUrl = `${FE_PROMOS_ENDPOINT}?iban=${encodeURIComponent(iban)}&amount=${netAmount}`;

                // SIEMPRE resuelve (aunque haya 404)
                const [co, fe] = await Promise.all([
                    getJsonSafe(commerceUrl),
                    getJsonSafe(bankUrl)
                ]);

                let items = `
                  <div class="form-check mb-1">
                    <input class="form-check-input" type="radio" name="promoChoice" value="none" checked>
                    <label class="form-check-label">Sin promoción</label>
                  </div>`;

                co.forEach((p) => {
                    const code = p.coPromoId || p.CoPromoId;
                    const desc = p.description || p.Description || code;
                    items += `
                      <div class="form-check mb-1">
                        <input class="form-check-input" type="radio" name="promoChoice" value="co|${code}">
                        <label class="form-check-label">Comercio: ${desc}</label>
                      </div>`;
                });

                fe.forEach((p) => {
                    const code = p.fePromoId || p.FePromoId;
                    const desc = p.description || p.Description || code;
                    items += `
                      <div class="form-check mb-1">
                        <input class="form-check-input" type="radio" name="promoChoice" value="fe|${code}">
                        <label class="form-check-label">Banco: ${desc}</label>
                      </div>`;
                });

                promoDiv.innerHTML = items;
                promosLoaded = true; // ← clave: nunca bloqueamos el “Continuar”
            };

            if (hasAccounts) {
                const sel = document.getElementById('swalIbanSelect');
                await loadPromos(sel.value);
                sel.addEventListener('change', () => loadPromos(sel.value));
            } else {
                const inp = document.getElementById('swalIBAN');
                inp.addEventListener('input', () => loadPromos(inp.value));
            }
        },
        showCancelButton: true,
        confirmButtonText: 'Continuar',
        preConfirm: () => {
            const iban = hasAccounts
                ? document.getElementById('swalIbanSelect').value
                : document.getElementById('swalIBAN').value.trim();
            if (!isCRIBAN(iban)) {
                Swal.showValidationMessage('IBAN inválido.');
                return false;
            }
            if (!promosLoaded) {
                Swal.showValidationMessage('Espera a que carguen las promociones.');
                return false;
            }
            const promoChoice = document.querySelector('input[name="promoChoice"]:checked')?.value || 'none';
            let coPromoId = null, fePromoId = null;
            if (promoChoice.startsWith('co|')) coPromoId = promoChoice.split('|')[1];
            if (promoChoice.startsWith('fe|')) fePromoId = promoChoice.split('|')[1];
            return { id: paymentId, iban, coPromoId, fePromoId };
        }
    });

    if (!form) return;
    await confirmPayment(form);
}

async function confirmPayment(form) {
    const payload = {
        Id: Number(form.id),
        IBAN: String(form.iban).trim().toUpperCase(),
        CoPromoId: form.coPromoId ? String(form.coPromoId).trim() : null,
        FePromoId: form.fePromoId ? String(form.fePromoId).trim() : null
    };

    Swal.fire({ title: "Procesando pago...", didOpen: () => Swal.showLoading(), allowOutsideClick: false });

    ctrl.PutToAPI("Payment/Confirm", payload,
        function (res) {
            Swal.close();

            // Normaliza nombres (camel / Pascal)
            const p = res || {};
            const gross = Number(p.grossAmount ?? p.GrossAmount ?? 0);
            const disc = Number(p.amountWDiscount ?? p.AmountWDiscount ?? 0);
            const net = Number(p.netAmount ?? p.NetAmount ?? (gross - disc));
            const coCom = Number(p.coCommisionAmount ?? p.CoCommisionAmount ?? 0);
            const feCom = Number(p.feCommisionAmount ?? p.FeCommisionAmount ?? 0);
            const profit = Number(p.commerceProfit ?? p.CommerceProfit ?? (net - coCom));
            const coCode = p.coPromoId ?? p.CoPromoId ?? null;
            const feCode = p.fePromoId ?? p.FePromoId ?? null;
            const iban = p.iban ?? p.IBAN ?? payload.IBAN;

            const fmt = (n) => `₡${n.toLocaleString('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;

            Swal.fire({
                icon: "success",
                title: "Pago confirmado",
                html: `
        <div class="text-start small">
          <div><b>IBAN:</b> ${iban}</div>
          <hr class="my-2"/>
          <div><b>Monto bruto:</b> ${fmt(gross)}</div>
          <div><b>Descuento aplicado:</b> ${fmt(disc)} ${coCode ? `(Comercio: ${coCode})` : feCode ? `(Banco: ${feCode})` : `(Sin promo)`}</div>
          <hr class="my-2"/>
          <div><b>Neto a cobrar:</b> ${fmt(net)}</div>
        </div>
      `,
                confirmButtonText: "OK"
            });

            loadPendingPayments(currentNationalId);
        },
        function (err) {
            Swal.close();
            const msg = (err && (err.responseJSON || err.responseText)) || "No se pudo confirmar el pago.";
            Swal.fire("Error", msg, "error");
        }
    );

    /*ctrl.PutToAPI("Payment/Confirm", payload,
        function () {
            Swal.close();
            Swal.fire("¡Listo!", "Pago aplicado correctamente.", "success");
            loadPendingPayments(currentNationalId);
        },
        function (err) {
            Swal.close();
            const msg = (err && (err.responseJSON || err.responseText)) || "No se pudo confirmar el pago.";
            Swal.fire("Error", msg, "error");
        }
    );*/
}
