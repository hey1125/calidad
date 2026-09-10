function PendingCommerceViewController() {
    this.ViewName = "PendingCommerce";
    this.ApiEndPointName = "Commerce/RetrievePending";

    this.InitView = function () {
        var ctrlActions = new ControlActions();

        $('#tblComerciosPendientes').DataTable({
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
                    data: "id",
                    render: function (data) {
                        return `<a href="/Admin/CommerceRequest?id=${data}" class="btn btn-sm btn-outline-primary">Revisar</a>`;
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
    var viewController = new PendingCommerceViewController();
    viewController.InitView();
});
