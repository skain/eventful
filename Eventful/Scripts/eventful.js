(function (window) {
	var eventful = window.eventful || {};
	window.eventful = eventful;
})(window);

//eventful.utils
(function (eventful) {
	eventful.utils = eventful.utils || {};
	var self = eventful.utils;
	self.setSelectionRange = function(input, selectionStart, selectionEnd) {
		if (input.setSelectionRange) {
			input.focus();
			input.setSelectionRange(selectionStart, selectionEnd);
		}
		else if (input.createTextRange) {
			var range = input.createTextRange();
			range.collapse(true);
			range.moveEnd('character', selectionEnd);
			range.moveStart('character', selectionStart);
			range.select();
		}
	}

	self.setCaretToPos = function(input, pos) {
		self.setSelectionRange(input, pos, pos);
	}
	self.endsWith = function (str, suffix) {
		return str.indexOf(suffix, str.length - suffix.length) !== -1;
	};
	self.getQSFormFields = function (formSelector) {
		return $(formSelector + ' input,' + formSelector + ' textarea');
	}
	self.getQSNameFromFormField = function ($formField) {
		return $formField.attr('data-qsname') || $formField.attr('name') || $formField.attr('id');
	}
	self.buildQueryStringFromForm = function (formSelector) {
		var qs = [];
		var $qsFields = self.getQSFormFields(formSelector)
		$qsFields.each(function () {
			var $this = $(this);
			var qsName = self.getQSNameFromFormField($this);
			if (qsName) {
				var val = $this.val();
				qs.push(qsName + '=' + encodeURIComponent(val));
			}
		});

		var joined = qs.join('&');
		return joined;
	};
	self.getQuerystringValues = function () {
		var result = {}, keyValuePairs = location.search.slice(1).split('&');

		keyValuePairs.forEach(function (keyValuePair) {
			keyValuePair = keyValuePair.split('=');
			result[keyValuePair[0]] = keyValuePair[1] ? decodeURIComponent(keyValuePair[1]) : '';
		});

		return result;
	};
	self.loadQuerystringValues = function (formSelector) {
		var qs = self.getQuerystringValues();
		var $qsFields = self.getQSFormFields(formSelector);
		$qsFields.each(function () {
			var $this = $(this);
			var qsName = self.getQSNameFromFormField($this);
			if (qs[qsName]) {
				$this.val(qs[qsName]);
			}
		});
	};
	self.addToCriteriaInput = function (valueToAdd, $input) {
		var curCriteria = $input.val();
		var and = curCriteria.length > 0 ? '\r\n&& ' : '';
		$input.val(curCriteria + and + valueToAdd + ' ');
		self.setCaretToPos($input[0], $input.val().length);
	};
	self.formatDateForCSharp = function (date) {
		var d = (date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear() + ' ' + date.getHours() + ':' + date.getMinutes() + ':' + date.getSeconds();
		return d;
	}
})(eventful);

//namespace for partials scripts
(function (eventful) {
	eventful.partials = eventful.partials || {};
})(eventful);

(function (eventful) {
	eventful.errorAlert = eventful.errorAlert || {};
	var self = eventful.errorAlert;
	self.showAlert = function (msg) {
		$('#PageAlertBody').html(msg);
		$('#PageAlert').show();
	};
	self.hookupAlert = function () {
		$('.alert').alert();
		$('#PageAlert button').click(function () {
			$('#PageAlertBody').html('');
			$('#PageAlert').hide();
		})
	};
})(eventful);

//eventful.templates
//(function (eventful) {
//	eventful.templates = eventful.templates || {};
//	eventful.templates.loadedTemplates = {};
//	eventful.templates.getTemplate = function (templatePath) {
//		if (!eventful.templates.loadedTemplates[templatePath]) {
//			$.ajax({ url: templatePath, async: false }).done(function (content) {
//				eventful.templates.loadedTemplates[templatePath] = content;
//			});
//		}
//		return eventful.templates.loadedTemplates[templatePath];
//	}
//})(eventful);

//eventful.eButton
(function (eventful) {
	eventful.eButton = eventful.eButton || {};
	eventful.eButton.handleFieldPickerLinkClick = function () {
		var $this = $(this);
		var field = $this.text();
		var $inputContainer = $this.closest('div.input-group');
		var $queryCriteria = $inputContainer.children('#QueryCriteria');
		if ($queryCriteria.length < 1) {
			$queryCriteria = $inputContainer.children('#EqlQuery');
		}
		if ($queryCriteria.length < 1) {
			$queryCriteria = $inputContainer.children('.queryTB');
		}
		if ($queryCriteria.length == 1) {
			eventful.utils.addToCriteriaInput(field, $queryCriteria);
		} else {
			var $groupBy = $inputContainer.children('#GroupBy');
			if ($groupBy.length < 1) {
				$groupBy = $inputContainer.children('.groupByTB');
			}

			if ($groupBy.length < 1) {
				$groupBy = $inputContainer.children('#GroupByFieldName');
			}

			if ($groupBy.length < 1) {
				throw "Could not find element to copy field to."
			}
			$groupBy.val(field);
		}
		
		$this.parents('.popover').siblings('.fieldPicker.eButton').popover('toggle');
	}
})(eventful);

//eventful.modal
(function (eventful) {
	eventful.modal = eventful.modal || {};
	//this is the old modal code
	eventful.modal.show = function (modalTitle, modalBody) {

		var modalTemplate = $('#BSModalTemplate').text();
		var $modalTemplateContainer = $('#BSModalTemplateContainer');
		$modalTemplateContainer.empty();
		$modalTemplateContainer.append(modalTemplate);
		ko.applyBindings({ modalTitle: modalTitle, modalBody: modalBody }, $modalTemplateContainer[0]);
		$('div.modal', $modalTemplateContainer).modal();
	};

	//new modal code starts here
	var self = eventful.modal;
	function baseModalModel(templateName, title) {
		var self = {};
		self.templateName = ko.observable(templateName || 'AlertModalTemplate');
		self.title = ko.observable(title || 'Unassigned');
		self.modalContent = ko.observable({
			body: 'Unassigned',
			onOKClick: null
		});
		return self;
	};

	self.showConfirmModal = function (title, body, onOKClick, onCancelClick) {
		var model = self.modalModel;
		model.title(title);
		model.modalContent({
			body: body,
			onOKClick: onOKClick,
			onCancelClick: onCancelClick
		});
		model.templateName('ConfirmModalTemplate');
		$('#PageModal').modal();
	};

	self.showAlertModal = function (title, body, onOKClick) {
		var model = self.modalModel;
		model.title(title);
		model.modalContent({
			body: body,
			onOKClick: onOKClick
		});
		model.templateName('AlertModalTemplate');

		$('#PageModal').modal();
	}
	
	$(document).ready(function () {
		self.modalModel = baseModalModel();
		ko.applyBindings(self.modalModel, $('#PageModal')[0])
	});
})(eventful);

$(document).ready(function () {
	$('body').popover({
		selector: 'button.eButton.fieldPicker, button.eButton.datePicker',
		html: true,
		title: '{e}ventful',
		content: function () { var $this = $(this); return $this.hasClass('fieldPicker') ? $('#FieldPickerPopupTemplate').text() : $('#DatePickerPopupTemplate').text(); }
	});
	eventful.errorAlert.hookupAlert();

	$('body').on('click', '.deleteButton', function () {
		return confirm('Are you sure you want to delete?');
	})
});