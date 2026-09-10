function PendingBanksViewController() {
    this.ViewName = "PendingBanks";
    this.ApiEndPointName = "FinancialEntity/RetrievePending";
    this.InitView = function () {
        var ctrlActions = new ControlActions();
        $('#tblBancosPendientes').DataTable({
            processing: true,
            ajax: {
                url: ctrlActions.GetUrlApiService(this.ApiEndPointName),
                dataSrc: ''
            },
            columns: [
                { data: "legalId" },
                { data: "bankCode" },
                { data: "name" },
                { data: "status" },
                {
                    data: "id",
                    render: function (data) {
                        return `<a href="/Admin/BankRequest?id=${data}" class="btn btn-sm btn-outline-primary">Revisar</a>`;
                    }
                }
            ],
            responsive: true,
            scrollX: true,
            language: {
                url: "//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json"
            },
            pagingType: "simple",
            info: false,
            lengthChange: false
        });
    };
}

$(document).ready(function () {
    var viewController = new PendingBanksViewController();
    viewController.InitView();
});