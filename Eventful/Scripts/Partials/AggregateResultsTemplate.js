(function (partials) {
	partials.aggregateResultsTemplate = partials.aggregateResultsTemplate || {};
	var self = partials.aggregateResultsTemplate;
	self.renderResults = function (response, templateDiv, statusText, xhr) {
		response.StartTime = new Date(parseInt(response.StartTime.substr(6)));
		response.EndTime = new Date(parseInt(response.EndTime.substr(6)));
		response.TotalItemsFound = self.calulateTotalItemsFound(response);
		response.BaseCriteriaQueryString = self.buildCriteriaQueryStringFromResponse(response);
		ko.applyBindings(response, templateDiv);
		self.renderGraph(response, templateDiv);
	};
	self.renderGraph = function (response, resultDiv) {
		var groupBy = response.GroupByFieldName || response.GroupName;
		//var groupBy = $('#GroupBy').val();
		var data = [];
		var categories = [];

		for (var groupIndex = 0, groupLen = response.QueryResults.length; groupIndex < groupLen; groupIndex++) {
			var group = response.QueryResults[groupIndex];
			console.log(group);
			data.push(group.AggregateValue);
			categories.push(group.GroupName == 'Aggregate' ? response.AggregateOperator : group.GroupName.replace(/\./g, '. ')); //we stick a space in between dots for better chart formatting and then have to remove it later for linking
		}

		$('.graphDiv', resultDiv).highcharts({
			xAxis: { categories: categories },
			series: [{ name: '{e}vents', color: '#4A89DC', data: data }],
			chart: {
				type: 'bar',
				style: { fontFamily: "'Open Sans', sans-serif" }
			},
			title: {
				text: groupBy ? response.TotalItemsFound + ' {e}vents grouped by ' + groupBy : response.TotalItemsFound + ' ' + response.AggregateOperator
			},
			yAxis: {
				allowDecimals: false,
				title: {
					text: '{e}vents'
				}
			},
			plotOptions: {
				series: {
					cursor: 'pointer',
					point: {
						events: {
							click: function () {
								response.EqlQuery = (response.EqlQuery ? response.EqlQuery + ' && ' : '') + self.formatCriteriaToAddToQuery(groupBy, this.category.replace(/\. /g, '.'));
								window.location = self.buildQueryPageLinkFromResponse(response);
							}
						}
					}
				}
			}
		});
	};
	self.formatCriteriaToAddToQuery = function (criteriaName, criteriaValue) {
		if (criteriaValue == 'NULL') {
			return criteriaName + ' !exists ';
		} else {
			return criteriaName + ' == \'' + criteriaValue + '\'';
		}
	};

	self.calulateTotalItemsFound = function(response) {
		var total = 0;
		for (var i = 0, len = response.QueryResults.length; i < len; i++) {
			total += response.QueryResults[i].AggregateValue;
		}

		return total;
	};
	self.buildCriteriaQueryStringFromResponse = function(response) {		
		var enc = {
			startTime: encodeURIComponent(eventful.utils.formatDateForCSharp(response.StartTime)),
			endTime: encodeURIComponent(eventful.utils.formatDateForCSharp(response.EndTime)),
			queryCriteria: encodeURIComponent(response.EqlQuery)
		};
		return 'RequestedStartTime=' + enc.startTime + '&RequestedEndTime=' + enc.endTime + '&EqlQuery=' + enc.queryCriteria;
	}
	self.buildQueryPageLinkFromResponse = function (response) {
		return '/query?' + self.buildCriteriaQueryStringFromResponse(response);
	};	
})(eventful.partials);
