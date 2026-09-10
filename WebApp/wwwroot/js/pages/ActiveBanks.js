function ActiveBanksViewController() {
    this.ViewName = "ActiveBanks";
    this.ApiEndPointName = "FinancialEntity/RetrieveActive";

    this.InitView = function () {
        var ctrlActions = new ControlActions();

        $('#tblBancosActivos').DataTable({
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
                    data: "commissionRate",
                    render: function (data) {
                        return data + '%';
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
    var viewController = new ActiveBanksViewController();
    viewController.InitView();
});