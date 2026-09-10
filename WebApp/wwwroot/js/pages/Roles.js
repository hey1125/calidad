function RolesController() {
    this.ApiBase = "Roles";                     
    this.RoleApi = "FinalUserCommerceRol";        
    this.commerceId = null;
    this.usuario = [];

    this.toast = (title, text, icon) =>
        (window.Swal ? Swal.fire({ title, text, icon }) : alert(`${title}\n\n${text || ""}`));

    this.ShowError = (msg, level = "error") => {
        if (window.Swal) Swal.fire("Aviso", msg, level);
        const $a = $("#alertMessage");
        if ($a.length) {
            $a.removeClass().addClass(`alert alert-${level}`).text(msg).show();
            setTimeout(() => $a.hide(), 3000);
        }
    };

    this.GetCommerceIdFromURL = () => new URLSearchParams(location.search).get("commerceId");

    // ------- CARGA DE ROLES (combo) -------
    this.LoadRoles = function (done) {
        const ctrl = new ControlActions();
        const url = `FinalUserCommerceRol/RolesByCommerce?commerceId=${this.commerceId}`;

        ctrl.GetToApi(url, (rows) => {
            const $ddl = $("#ddlIntermedio");
            const prev = $ddl.val();
            $ddl.empty().append(`<option value="" selected disabled>Seleccione un rol...</option>`);

            if (!rows || !rows.length) {
                $ddl.append(`<option value="" disabled>(Sin roles en este comercio)</option>`);
                return done && done();
            }

            const vistos = new Set();
            rows.forEach(r => {
                const id = r.RoleId ?? r.roleId ?? r.RolId;
                const name = r.RoleName ?? r.roleName ?? `Rol ${id}`;
                if (id != null && !vistos.has(id)) {
                    $ddl.append(`<option value="${id}">${name}</option>`);
                    vistos.add(id);
                }
            });

            if (prev && $ddl.find(`option[value="${prev}"]`).length) $ddl.val(prev);
            done && done();
        });
    };

    // ------- LISTA DE USUARIOS  -------
    this.LoadTable = function () {
        const ctrl = new ControlActions();
        ctrl.GetToApi(`${this.ApiBase}/List?commerceId=${this.commerceId}`, (resp) => {
            this.usuario = Array.isArray(resp) ? resp : [];
            const $tb = $("#tblRoles").empty();
            if (!this.usuario.length) {
                $tb.append(`<tr><td colspan="5" class="text-center text-muted">Sin usuario</td></tr>`);
                return;
            }
            this.usuario.forEach(c => {
                const userId = c.Id ?? c.UserId;
                $tb.append(`
          <tr>
            <td>${c.Cedula ?? ""}</td>
            <td>${c.FirstName ?? ""}</td>
            <td>${c.LastName ?? ""}</td>
            <td>${c.Email ?? ""}</td>
            <td><button class="btn btn-sm btn-danger" onclick="RolesCtrl.DeleteRoles(${userId})">Eliminar</button></td>
          </tr>
        `);
            });
        });
    };

    // ------- Asignar rol seleccionado a cédula (botón Agregar Roles) -------
   
    this.AssignSelectedRole = function () {
        const cedula = ($("#txtCedula").val() || "").trim();
        const roleId = $("#ddlIntermedio").val();

        if (!cedula) return this.ShowError("Debe ingresar una cédula.", "warning");
        if (!roleId) return this.ShowError("Seleccione un rol.", "warning");

        // no permitir agregarse a sí mismo
        try {
            const user = JSON.parse(localStorage.getItem("user"));
            if (user && String(user.nationalId) === cedula)
                return this.ShowError("No puedes agregarte a ti mismo.", "info");
        } catch { }

        const ctrl = new ControlActions();

        
        const url = `${this.ApiBase}/AddByNationalId?nationalId=${encodeURIComponent(cedula)}&commerceId=${this.commerceId}&roleId=${roleId}`;
        

        ctrl.PostToAPIWithCustomHeaders(
            url,
            {},
            () => { $("#txtCedula").val(""); this.LoadTable(); this.LoadRoles(); },
            (jq) => {
                if (jq?.status === 409)
                    this.toast("Aviso", "Ese usuario ya tiene ese rol en este comercio.", "info");
                else
                    this.ShowError("No se pudo asignar el rol.", "error");
            }
        );
    };


    // ------- Crear un rol y asignarlo (botón Crear y asignar) -------
    this.CreateAndAssignRole = function () {
        const roleName = ($("#txtNewRoleName").val() || "").trim();
        const cedula = ($("#txtNewRoleCedula").val() || "").trim();

        if (!roleName) return this.ShowError("Digite el nombre del rol.", "warning");
        if (!cedula) return this.ShowError("Digite la cédula del usuario.", "warning");

        const ctrl = new ControlActions();
        ctrl.PostToAPIWithCustomHeaders(
            `Role/CreateAndAssign?name=${encodeURIComponent(roleName)}&nationalId=${encodeURIComponent(cedula)}&commerceId=${this.commerceId}`,
            {},
            () => {
                this.toast("¡Listo!", "Rol creado y asignado.", "success");
                $("#txtNewRoleName").val(""); $("#txtNewRoleCedula").val("");
                this.LoadRoles(); this.LoadTable();
            },
            (jq) => {
                if (jq?.status === 404) return this.ShowError("No existe un usuario con esa cédula.", "warning");
                if (jq?.status === 409) return this.ShowError("Ese usuario ya tiene ese rol en este comercio.", "info");
                this.ShowError("No se pudo crear/asignar el rol.", "error");
            }
        );
    };

    // ------- Init ÚNICO -------
    this.InitView = function () {
        this.commerceId = this.GetCommerceIdFromURL();
        if (!this.commerceId) return this.ShowError("No se proporcionó el ID del comercio.", "warning");

        this.LoadRoles();
        this.LoadTable();

        // evita registrar handlers duplicados si se re-inicializa
        $("#btnAssignRole").off("click").on("click", () => this.AssignSelectedRole());
        $("#btnCreateAssignRole").off("click").on("click", () => this.CreateAndAssignRole());

        $("#ddlIntermedio").off("change").on("change", function () {
            console.log("Rol seleccionado:", $(this).val());
        });
    };

    // ------- Delete -------
    this.DeleteRoles = function (userId) {
        const doDel = () => {
            const ctrl = new ControlActions();
            const url = ctrl.GetUrlApiService(`${this.ApiBase}/Delete?userId=${userId}&commerceId=${this.commerceId}`);
            fetch(url, { method: "DELETE" })
                .then(r => { if (!r.ok) throw new Error("No se pudo eliminar"); return r.json().catch(() => ({})); })
                .then(() => { this.toast("Eliminado", "Usuario eliminado.", "success"); this.LoadTable(); this.LoadRoles(); })
                .catch(e => this.ShowError(e.message || "No se pudo eliminar el Usuario.", "error"));
        };

        if (window.Swal) {
            Swal.fire({ title: "¿Eliminar Usuario?", icon: "warning", showCancelButton: true, confirmButtonText: "Eliminar", cancelButtonText: "Cancelar" })
                .then(res => { if (res.isConfirmed) doDel(); });
        } else if (confirm("¿Eliminar Usuario?")) doDel();
    };
}


