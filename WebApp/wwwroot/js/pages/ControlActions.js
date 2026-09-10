function ControlActions() {
	//Ruta base del API
	this.URL_API = "https://localhost:7163/api/";
	//this.URL_API = "https://billetico-api-baeecxbkd0asb4fb.centralus-01.azurewebsites.net/api/";

	this.GetUrlApiService = function (service) {
		return this.URL_API + service;
	}

	this.GetTableColumsDataName = function (tableId) {
		var val = $('#' + tableId).attr("ColumnsDataName");

		return val;
	}

	this.FillTable = function (service, tableId, refresh) {

		if (!refresh) {
			columns = this.GetTableColumsDataName(tableId).split(',');
			var arrayColumnsData = [];


			$.each(columns, function (index, value) {
				var obj = {};
				obj.data = value;
				arrayColumnsData.push(obj);
			});
			//Esto es la inicializacion de la tabla de data tables segun la documentacion de 
			// datatables.net, carga la data usando un request async al API
			$('#' + tableId).DataTable({
				"processing": true,
				"ajax": {
					"url": this.GetUrlApiService(service),
					dataSrc: ''
				},
				"columns": arrayColumnsData
			});
		} else {
			//RECARGA LA TABLA
			$('#' + tableId).DataTable().ajax.reload();
		}

	}

	this.GetSelectedRow = function () {
		var data = sessionStorage.getItem(tableId + '_selected');

		return data;
	};

	this.BindFields = function (formId, data) {
		console.log(data);
		$('#' + formId + ' *').filter(':input').each(function (input) {
			var columnDataName = $(this).attr("ColumnDataName");
			this.value = data[columnDataName];
		});
	}

	this.GetDataForm = function (formId) {
		var data = {};

		$('#' + formId + ' *').filter(':input').each(function (input) {
			var columnDataName = $(this).attr("ColumnDataName");
			if (columnDataName) data[columnDataName] = this.value;
		});

		return data;
	}


	/* ACCIONES VIA AJAX, O ACCIONES ASINCRONAS*/

	this.PostToAPI = function (service, data, callBackFunction) {
		$.ajax({
			type: "POST",
			url: this.GetUrlApiService(service),
			data: JSON.stringify(data),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: function (data) {
				if (callBackFunction) {
					Swal.fire(
						'Éxito',
						'La operación se realizó correctamente.',
						'success'
					);
					callBackFunction(data);
				}
			},
			error: function (jqXHR) {
				var responseJson = jqXHR.responseJSON;
				var message = jqXHR.responseText || "Ha ocurrido un error inesperado.";

				if (responseJson) {
					var errors = responseJson.errors;
					if (errors) {
						var errorMessages = Object.values(errors).flat();
						message = errorMessages.join("<br/> ");
					} else if (responseJson.message) {
						message = responseJson.message;
					}
				}

				Swal.fire({
					icon: 'error',
					title: 'Oops...',
					html: message,
					footer: 'BilleTico'
				});
			}
		});
	};


	this.PutToAPI = function (service, data, callBackFunction) {
		var jqxhr = $.put(this.GetUrlApiService(service), data, function (response) {
			var ctrlActions = new ControlActions();

			Swal.fire(
				'Éxito',
				'La información fue actualizada correctamente.',
				'success'
			);

			if (callBackFunction) {
				callBackFunction(response);
			}
		})
			.fail(function (response) {
				var data = response.responseJSON;
				var message = "Ha ocurrido un error inesperado.";

				if (data) {
					var errors = data.errors;
					if (errors) {
						var errorMessages = Object.values(errors).flat();
						message = errorMessages.join("<br/> ");
					} else if (data.message) {
						message = data.message;
					}
				}

				Swal.fire({
					icon: 'error',
					title: 'Oops...',
					html: message,
					footer: 'BilleTico'
				});
			});
	};

	this.DeleteToAPI = function (service, data, callBackFunction) {
		var jqxhr = $.delete(this.GetUrlApiService(service), data, function (response) {
			var ctrlActions = new ControlActions();

			Swal.fire(
				'Éxito',
				'El elemento fue eliminado correctamente.',
				'success'
			);

			if (callBackFunction) {
				callBackFunction(response);
			}
		})
			.fail(function (response) {
				var data = response.responseJSON;
				var message = "Ha ocurrido un error inesperado.";

				if (data) {
					var errors = data.errors;
					if (errors) {
						var errorMessages = Object.values(errors).flat();
						message = errorMessages.join("<br/> ");
					} else if (data.message) {
						message = data.message;
					}
				}

				Swal.fire({
					icon: 'error',
					title: 'Oops...',
					html: message,
					footer: 'BilleTico'
				});
			});
	};

	this.GetToApi = function (service, callBackFunction) {
		var jqxhr = $.get(this.GetUrlApiService(service), function (response) {
			console.log("Response " + response);
			if (callBackFunction) {
				callBackFunction(response);
			}

		});
	}
}

//Custom jquery actions
$.put = function (url, data, callback) {
	if ($.isFunction(data)) {
		type = type || callback,
			callback = data,
			data = {}
	}
	return $.ajax({
		url: url,
		type: 'PUT',
		success: callback,
		data: JSON.stringify(data),
		contentType: 'application/json'
	});
}

$.delete = function (url, data, callback) {
	if ($.isFunction(data)) {
		type = type || callback,
			callback = data,
			data = {}
	}
	return $.ajax({
		url: url,
		type: 'DELETE',
		success: callback,
		data: JSON.stringify(data),
		contentType: 'application/json'
	});
}

// MÉTODO EXTENDIDO: Post con headers personalizados
ControlActions.prototype.PostToAPIWithCustomHeaders = function (service, data, callBackFunction, errorCallback, customHeaders) {
	$.ajax({
		type: "POST",
		url: this.GetUrlApiService(service),
		data: JSON.stringify(data),
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		headers: customHeaders || {},
		success: function (data) {
			if (callBackFunction) {
				Swal.fire("Éxito", "La operación se realizó correctamente.", "success");
				callBackFunction(data);
			}
		},
		error: function (jqXHR) {
			var responseJson = jqXHR.responseJSON;
			var message = jqXHR.responseText || "Ha ocurrido un error inesperado.";

			if (responseJson) {
				var errors = responseJson.errors;
				if (errors) {
					var errorMessages = Object.values(errors).flat();
					message = errorMessages.join("<br/> ");
				} else if (responseJson.message) {
					message = responseJson.message;
				}
			}

			Swal.fire({
				icon: 'error',
				title: 'Oops...',
				html: message,
				footer: 'BilleTico'
			});

			if (errorCallback) {
				errorCallback(jqXHR);
			}
		}
	});
};

ControlActions.prototype.PostToAPIWithCustomContentTypeAndHeaders = function (
	service,
	data,
	callBackFunction,
	errorCallback,
	contentType,
	customHeaders
) {
	const isFormData = data instanceof FormData;
	const ajaxOptions = {
		type: "POST",
		url: this.GetUrlApiService(service),
		data: isFormData ? data : JSON.stringify(data),
		dataType: "json",
		headers: customHeaders || {},
		success: function (data) {
			if (callBackFunction) {
				callBackFunction(data);
				Swal.fire("Éxito", "La operación se realizó correctamente.", "success");
			}
		},
		error: function (jqXHR) {
			/* tu manejo de errores… */
			if (errorCallback) errorCallback(jqXHR);
		}
	};

	if (isFormData) {
		// si es FormData, delega al navegador el contentType y no procese los datos
		ajaxOptions.processData = false;
		ajaxOptions.contentType = false;
	} else {
		// JSON normal
		ajaxOptions.contentType = contentType;
	}

	$.ajax(ajaxOptions);
};

ControlActions.prototype.GetToApiWithCustomHeaders = function (service, callBackFunction, errorCallback, customHeaders) {
	$.ajax({
		type: "GET",
		url: this.GetUrlApiService(service),
		headers: customHeaders || {},
		success: function (response) {
			if (callBackFunction) {
				callBackFunction(response);
			}
		},
		error: function (jqXHR) {
			if (errorCallback) {
				errorCallback(jqXHR);
			} else {
				var message = jqXHR.responseText || "Error al obtener los datos.";
				Swal.fire({
					icon: "error",
					title: "Oops...",
					text: message
				});
			}
		}
	});
};

