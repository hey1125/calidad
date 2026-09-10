let commerceId;
let commissionRate = 0;
let ctrlActions = new ControlActions();

$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    commerceId = urlParams.get("commerceId");

    if (!commerceId) {
        Swal.fire("Error", "No se pudo obtener el ID del comercio desde la URL.", "error");
        return;
    }

    // Cargar comisión del comercio
    ctrlActions.GetToApi(
        `Commerce/RetrieveById?id=${commerceId}`,
        function (data) {
            commissionRate = parseFloat(data.commissionRate);
            if (isNaN(commissionRate)) commissionRate = 0; // CAMBIO: fallback
            $("#txtCommission").val("₡0.00");
        },
        function (xhr) { // CAMBIO: manejo de error
            console.error("Error cargando comercio:", xhr);
            commissionRate = 0;
            $("#txtCommission").val("₡0.00");
            // No bloqueamos la pantalla, se puede cobrar con comisión 0 si es necesario
        }
    );

    // Calcular comisión mientras escribe
    $("#txtAmount").on("input", function () {
        const amount = parseFloat($(this).val());
        if (!isNaN(amount) && commissionRate > 0) {
            const calculated = (amount * (commissionRate / 100)).toFixed(2);
            $("#txtCommission").val(`₡${calculated}`);
        } else {
            $("#txtCommission").val("₡0.00");
        }
    });

    // Validar cédula y autocompletar nombre
    $("#txtNationalId").on("blur", function () {
        const cedula = $(this).val().trim();
        if (!/^\d{9}$/.test(cedula)) { $("#txtFullName").val("Cédula inválida"); return; }

        $("#txtFullName").val("Verificando...");

        $.ajax({
            url: `https://billetico-api-baeecxbkd0asb4fb.centralus-01.azurewebsites.net/api/FinalUser/RetrieveByNationalId?nationalId=${encodeURIComponent(cedula)}`,
            method: "GET",
            success: function (user) {
                const fullName = `${user.firstName} ${user.lastName1 ?? ""} ${user.lastName2 ?? ""}`.trim();
                $("#txtFullName").val(fullName || "Usuario no encontrado"); // CAMBIO: fallback si faltan datos
            },
            error: function (xhr) {
                if (xhr.status === 404) $("#txtFullName").val("Usuario no encontrado");
                else { $("#txtFullName").val("Error al buscar usuario"); console.error(xhr); }
            }
        });
    });

    // Submit
    $("#frmCobro").submit(function (e) {
        e.preventDefault();

        const nationalId = $("#txtNationalId").val().trim();
        const grossAmount = parseFloat($("#txtAmount").val());
        const description = $("#txtDescription").val().trim();
        const fullNameText = $("#txtFullName").val().trim();

        if (!nationalId || isNaN(grossAmount) || grossAmount <= 0) {
            Swal.fire("Campos inválidos", "Por favor complete correctamente todos los campos.", "warning");
            return;
        }

        // CAMBIO: bloquear estados no válidos del nombre
        const invalidNameStates = ["", "usuario no encontrado", "error al buscar usuario", "cédula inválida", "verificando..."];
        if (invalidNameStates.includes(fullNameText.toLowerCase())) {
            Swal.fire("Validación requerida", "Debe ingresar una cédula válida antes de continuar.", "warning");
            return;
        }

        const coCommisionAmount = parseFloat((grossAmount * (commissionRate / 100)).toFixed(2)) || 0;

        // Solo enviamos datos requeridos para registrar el cobro pendiente
        const payment = {
            nationalId: nationalId,
            commerceId: parseInt(commerceId),
            grossAmount: grossAmount,
            amountWDiscount: 0,               // SIN promo -> 0 de DESCUENTO
            coCommisionAmount: coCommisionAmount,
            description: description
        };

        // CAMBIO: deshabilitar botón para evitar doble submit
        const $btn = $("#btnSubmitCobro");
        $btn.prop("disabled", true);

        ctrlActions.PostToAPI("Payment/Create", payment, function () {
            $("#frmCobro")[0].reset();
            $("#txtFullName").val("");
            $("#txtCommission").val("₡0.00");
            loadPayments();
            Swal.fire("Éxito", "Cobro registrado.", "success");
            $btn.prop("disabled", false);
        }, function (xhr) {
            $btn.prop("disabled", false);
            console.error("Error creando cobro:", xhr);
            Swal.fire("Error", "No se pudo registrar el cobro. Intenta de nuevo.", "error");
        });
    });

    $("#btnRefresh").on("click", function () {
        loadPayments();
    });

    loadPayments();
});

function loadPayments() {
    let ctrlActions = new ControlActions();

    ctrlActions.GetToApi(`Payment/GetCommercePayments?commerceId=${commerceId}`, function (data) {
        const $tbody = $("#tblCobrosBody");
        const $container = $("#tblCobrosContainer");
        $tbody.empty();

        if (!data || data.length === 0) {
            $("#noCobrosMessage").removeClass("d-none");
            if ($container.length) $container.addClass("d-none");
            return;
        }

        $("#noCobrosMessage").addClass("d-none");
        if ($container.length) $container.removeClass("d-none");

        data.forEach(p => {
            const gross = Number(p.grossAmount ?? 0);
            const coCom = Number(p.coCommisionAmount ?? 0);
            const profit = (p.commerceProfit != null) ? Number(p.commerceProfit) : null;
            const created = p.created ? new Date(p.created).toLocaleString() : "-";

            const row = `
                <tr>
                    <td>${p.nationalId ?? "-"}</td>
                    <td>₡${gross.toFixed(2)}</td>
                    <td>₡${coCom.toFixed(2)}</td>
                    <td>${profit != null ? "₡" + profit.toFixed(2) : "-"}</td>
                    <td>${p.status ?? "-"}</td>
                    <td>${created}</td>
                </tr>
            `;
            $tbody.append(row);
        });
    }, function (xhr) {
        console.error("Error cargando cobros:", xhr); // CAMBIO: log de error
        $("#noCobrosMessage").removeClass("d-none");
        $("#tblCobrosContainer").addClass("d-none");
    });
}
