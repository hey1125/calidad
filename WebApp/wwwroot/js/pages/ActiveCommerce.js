function ActiveCommerceViewController() {
    this.ViewName = "ActiveCommerce";
    this.ApiEndPointName = "Commerce/RetrieveActive";

    this.InitView = function () {
        var ctrlActions = new ControlActions();

        $('#tblComerciosActivos').DataTable({
            processing: true,
            ajax: {
                url: ctrlActions.GetUrlApiService(this.ApiEndPointName),
                dataSrc: ''
            },
            columns: [
                { data: "legalId" },
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
    var viewController = new ActiveCommerceViewController();
    viewController.InitView();
});
