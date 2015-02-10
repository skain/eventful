(function (eventful) {
	eventful.dataModels = eventful.dataModels || {}; //initialize the module namespace
	eventful.dataModels.addModel = function (modelName, model) {
		if (eventful.dataModels[modelName]) {
			throw "Error. DataModel: '" + modelName + "' has already been defined.";
		}

		eventful.dataModels[modelName] = model;
	};
	eventful.dataModels.addObservableModel = function (modelName, model, excludeFields) {
		var observable = model;
		if (model) {
			observable = new Object();
			excludeFields = excludeFields || [];
			for (var prop in model) {
				if (excludeFields.indexOf(prop) > -1) {
					observable[prop] = model[prop];
				} else {
					if (eventful.dataModels.isArray(model[prop])) {
						console.log('observable array:' + prop);
						observable[prop] = ko.observableArray(model[prop]);
					} else {
						observable[prop] = ko.observable(model[prop]);
					}
				}
			}

		}

		eventful.dataModels.addModel(modelName, observable);
	}
	eventful.dataModels.isArray = function (obj) {
		if (!obj) {
			return false;
		}

		var arrayToString = Object.prototype.toString.call([]);
		return Object.prototype.toString.call(obj) === arrayToString;
	}
})(eventful);
