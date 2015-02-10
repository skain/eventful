(function (eventful) {
	eventful.simpleTableFilter = eventful.simpleTableFilter || {}; //initialize the module namespace

	eventful.simpleTableFilter.registerFilterTable = function (filterTBSelector, tableSelector, filterFieldIndex) {
		function doFilter() {
			var filterBy = $(filterTBSelector).val().toLowerCase();
			if (!filterBy || filterBy == '') {
				$(tableSelector + ' tr:not(:first)').show();
				return;
			}

			var $nameTDs = $(tableSelector + ' tr:not(:first) td:nth-child(' + filterFieldIndex + ')');
			$.each($nameTDs, function (index, value) {
				var $td = $(value);
				var $tr = $td.parents('tr');
				var name = $td.text().trim().toLowerCase();
				if (name.indexOf(filterBy) < 0) {
					$tr.hide();
				} else {
					$tr.show();
				}
			});
		}

		$('body').on('keyup', filterTBSelector, doFilter);
	};
})(eventful);
