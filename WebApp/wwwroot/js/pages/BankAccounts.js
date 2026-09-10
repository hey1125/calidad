function BankAccountsController() {
    this.ApiBankAccount = "BankAccount";
    this.ApiFinancialEntity = "FinancialEntity";
    this.ctrlActions = new ControlActions();

    this.InitView = function () {
        this.LoadBankAccounts();

        $('#btn-agregar').on('click', () => {
            this.CreateBankAccount();
        });
    };

    this.LoadBankAccounts = function () {
        const user = JSON.parse(localStorage.getItem("user"));
        if (!user) {
            console.log('No hay usuario logueado');
            this.showNoBankAccountsMessage();
            return;
        }

        this.ctrlActions.GetToApiWithCustomHeaders(
            `${this.ApiBankAccount}/GetAccountsByUser?userId=${user.id}`,
            (data) => {
                if (data && data.length > 0) {
                    this.RenderTable(data, user);
                } else {
                    this.showNoBankAccountsMessage();
                }
            },
            (error) => {
                console.error('Error al obtener cuentas:', error);
                $('#cuentas-body').empty();
                $('#noBankAccountsMessage').addClass('d-none');
                Swal.fire({
                    icon: "error",
                    title: "No se pudieron cargar las cuentas",
                    text: "Intenta nuevamente. Si el problema continúa, contacta al administrador."
                });
            },
            { 'LoggedUserId': user.id }
        );
    };

    this.RenderTable = function (accounts, user) {
        const tbody = $('#cuentas-body');
        tbody.empty();

        accounts.forEach((account) => {
            const ultimos4 = account.iban.slice(-4);
            const row = `
                <tr>
                    <td>${user.firstName} ${user.lastName1}</td>
                    <td>****${ultimos4}</td>
                    <td>
                        <button class="btn btn-sm btn-danger" onclick="bankAccountsController.DeleteBankAccount(${account.id})">
                            <i class="fas fa-trash-alt me-1"></i>Eliminar
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });

        $('#noBankAccountsMessage').addClass('d-none');
    };

    this.CreateBankAccount = function () {
        const user = JSON.parse(localStorage.getItem("user"));
        const iban = $('#iban').val().trim();

        if (!iban.startsWith("CR") || iban.length !== 22) {
            Swal.fire({
                icon: "error",
                title: "IBAN inválido",
                text: "Debe iniciar con 'CR' y contener 22 caracteres."
            });
            return;
        }

        const bankCode = iban.substring(5, 8);

        this.ctrlActions.GetToApiWithCustomHeaders(
            `${this.ApiFinancialEntity}/RetrieveByBankCode?bankCode=${bankCode}`,
            (entidad) => {
                const payload = {
                    userId: user.id,
                    bankId: entidad.id,
                    iban: iban
                };

                this.ctrlActions.PostToAPI(
                    `${this.ApiBankAccount}/Create`,
                    payload,
                    () => {
                        Swal.fire({
                            icon: "success",
                            title: "Cuenta registrada",
                            text: "Se agregó correctamente la cuenta bancaria.",
                            timer: 1500,
                            showConfirmButton: false
                        });

                        $('#iban').val('');
                        this.LoadBankAccounts();
                    }
                );
            },
            () => {
                Swal.fire({
                    icon: "error",
                    title: "Entidad no encontrada",
                    text: `No se encontró entidad para el código: ${bankCode}`
                });
            },
            { 'LoggedUserId': user.id }
        );
    };

    this.DeleteBankAccount = function (accountId) {
        Swal.fire({
            title: "¿Eliminar cuenta?",
            text: "Esta acción no se puede deshacer.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Sí, eliminar"
        }).then((result) => {
            if (result.isConfirmed) {
                this.ctrlActions.DeleteToAPI(
                    `${this.ApiBankAccount}/Delete?id=${accountId}`,
                    {},
                    () => {
                        Swal.fire({
                            icon: "success",
                            title: "Cuenta eliminada",
                            text: "La cuenta bancaria se eliminó correctamente.",
                            timer: 1500,
                            showConfirmButton: false
                        });
                        this.LoadBankAccounts();
                    }
                );
            }
        });
    };

    this.showNoBankAccountsMessage = function () {
        $('#cuentas-body').empty();
        $('#noBankAccountsMessage').removeClass('d-none');
    };
}


const bankAccountsController = new BankAccountsController();
$(document).ready(function () {
    bankAccountsController.InitView();
});
(function () {
    const originalSwalFire = Swal.fire;

    Swal.fire = function () {
        if (arguments.length === 1 && typeof arguments[0] === "object") {
            const options = arguments[0];

            if (options.html && typeof options.html === "string" && options.html.includes('"error"')) {
                try {
                    const parsed = JSON.parse(options.html);
                    if (parsed.error) {
                        options.html = parsed.error;
                    }
                } catch (e) {
                    console.warn("No se pudo parsear el mensaje de error:", e);
                }
            }

            return originalSwalFire.call(this, options);
        }

        // fallback para otros usos
        return originalSwalFire.apply(this, arguments);
    };
})();
