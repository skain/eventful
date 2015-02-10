
var editData = window.jsonEditData;

function aggregateRequestModel(item) {
	var self = this;
	if (item==null) {
		self.RequestedStartTime = ko.observable('now - 15 minutes');
		self.RequestedEndTime = ko.observable('now');
		self.EqlQuery = ko.observable('');
		self.GroupByFieldName = ko.observable('');
		self.Title = ko.observable('');
		self.MaxResultsBeforeOutliers = ko.observable('5');
		self.AggregateOperator = ko.observable('Count');
		self.AggregateFieldName = ko.observable('_id');
	}
	else {
		self.RequestedStartTime = ko.observable(item.RequestedStartTime);
		self.RequestedEndTime = ko.observable(item.RequestedEndTime);
		self.EqlQuery = ko.observable(item.EqlQuery);
		self.GroupByFieldName = ko.observable(item.GroupByFieldName);
		self.Title = ko.observable(item.Title);
		self.MaxResultsBeforeOutliers = ko.observable(item.MaxResultsBeforeOutliers);
		self.AggregateOperator = ko.observable(item.AggregateOperator);
		self.AggregateFieldName = ko.observable(item.AggregateFieldName);
	}

};

function reportModel() {
	var self = this;
	if (editData!=null) {
		self.Id = editData.Id
		self.Title = ko.observable(editData.Title);
		self.AggregateRequests = ko.observableArray($.map(editData.AggregateRequests, function (item) { return new aggregateRequestModel(item) }));
	}
	else {
		self.Title = ko.observable('');
		self.AggregateRequests = ko.observableArray([new aggregateRequestModel()]);
	}
	self.addNewAggregateRequest = function () {
		self.AggregateRequests.push(new aggregateRequestModel(null));
	};
	self.deleteAggregateRequest = function (aggregateRequestModel) {
		self.AggregateRequests.remove(aggregateRequestModel);
	};
	self.hasMoreThanOneAggregateRequest = ko.computed(function() { return  self.AggregateRequests });
	self.moveAggregateRequestUp = function (aggregateRequestModel) {
		var index = self.AggregateRequests.indexOf(aggregateRequestModel);
		if (index == 0) {
			alert('Already at the top of the report!');
			return;
		}

		var item = self.AggregateRequests.splice(index, 1)[0];
		self.AggregateRequests.splice(index - 1, 0, item);
	}
	self.moveAggregateRequestDown = function (aggregateRequestModel) {
		var index = self.AggregateRequests.indexOf(aggregateRequestModel);
		if (index + 1 >= self.AggregateRequests().length) {
			alert('Already at the bottom of the report!');
			return;
		}

		var item = self.AggregateRequests.splice(index, 1)[0];
		self.AggregateRequests.splice(index + 1, 0, item);
	}
};

function doSubmit() {
	$('.groupByTB').trigger("change"); //
	var data = ko.toJSON(theModel);

	$.ajax({
		type: 'Post',
		dataType: 'json',
		url: '/Reports/Edit',
		data: data,
		contentType: 'application/json; charset=utf-8',
		async: false,
		success: function (data) {
			window.location = '/Reports/';
		},
		error: function (data) {
			console.debug(data);
		}
	});
}


var theModel = new reportModel();
ko.applyBindings(theModel, $('form')[0]);

$('body').on('click', '#SubmitBTN', function () {
	doSubmit();
});