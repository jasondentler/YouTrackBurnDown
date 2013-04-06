///<reference path="~/SignalR"/>
///<reference path="~/Scripts/highcharts/"/>
$(function () {
    var hub = $.connection.youTrackHub;

    $.connection.hub.start().done(function () {
        var selectedSprint = '1307';
        var sprintFieldName = 'Fix Versions';
        var estimateFieldName = 'Estimation';
        var filters = [
            '#{Business Systems} -land-legal -JDEdwards',
            '#{Business Systems} #{JDEdwards}',
            '#{Marketing Systems}',
            ''
        ];

        var start = null;
        var finish = null;

        var byProject = {};
        var byLine = [];

        // Load Sprint Dates
        var sprintDatePromises = [];
        var sprintDatesLoadedPromise = $.Deferred();
        for (var projectIdx = 0; projectIdx < model.Projects.length; projectIdx++) {
            var project = model.Projects[projectIdx];
            var projectShortName = project.ShortName;
            for (var sprintIdx = 0; sprintIdx < project.Sprints.length; sprintIdx++) {
                var sprintName = project.Sprints[sprintIdx];
                if (sprintName == selectedSprint) {
                    sprintDatePromises.push(hub.server.sprintDates(projectShortName, sprintName));
                    byProject[projectShortName] = {};
                }
            }
        }

        $.when.apply(null, sprintDatePromises).done(function () {
            var results = Array.prototype.slice.call(arguments);
            for (var idx = 0; idx < results.length; idx++) {
                var result = results[idx];
                var projectShortName = result.Item1;
                var start = result.Item3;
                var finish = result.Item4;
                var item = byProject[projectShortName];
                item.start = start;
                item.finish = finish;
                if (start == null)
                    start = item.start;
                if (item.finish == null)
                    finish = item.finish;
                if (start != item.start)
                    throw 'Start dates differ between projects';
                if (finish != item.finish)
                    throw 'Finish dates differ between projects';
            }
            sprintDatesLoadedPromise.resolve();
        }).fail(function () {
            sprintDatesLoadedPromise.reject();
        });


        // Load Ticket Data
        var ticketDataLoadedPromise = $.Deferred();
        var sprintSpecificFilters = [];
        for (var i = 0; i < filters.length; i++)
            sprintSpecificFilters.push(filters[i] + ' #{' + selectedSprint + '}');

        function parseTicketData(results) {
            for (var sprintSpecificFilter in results) {
                var result = results[sprintSpecificFilter];
                var lineIndex = sprintSpecificFilters.indexOf(sprintSpecificFilter);
                var line = byLine[lineIndex] = byLine[lineIndex] || {};
                line.items = line.items || [];

                for (var ticketId in result) {
                    var ticket = result[ticketId];
                    line.items.push(ticket);
                }
            }
            ticketDataLoadedPromise.resolve();
        }

        var queryPromise = hub.server.query(
            sprintSpecificFilters,
            ['id', 'resolved', 'Estimation', 'projectShortName'],
            1000, 1);

        queryPromise.done(function (results) {
            parseTicketData(results);
        }).fail(function () {
            ticketDataLoadedPromise.reject();
        });


        function sumEstimates(items) {
            var itemsWithoutEstimate = 0;
            var sum = 0;
            for (var idx = 0; idx < items.length; idx++) {
                var item = items[idx];
                if (typeof item[estimateFieldName] === 'undefined') {
                    itemsWithoutEstimate++;
                } else {
                    item.estimate = parseInt(JSON.parse(item[estimateFieldName])[0]);
                    sum += item.estimate;
                }
            }
            if (itemsWithoutEstimate) {
                var defaultEstimate = sum / (items.length - itemsWithoutEstimate);
                setDefaultEstimate(items, defaultEstimate);
                return sumEstimates(items);
            }
            return sum;
        }

        function setDefaultEstimate(items, defaultEstimate) {
            for (var idx = 0; idx < items.length; idx++) {
                var item = items[idx];
                if (typeof item[estimateFieldName] === "undefined")
                    item[estimateFieldName] = "[\"" + defaultEstimate + "\"]";
            }
        }

        $.when(ticketDataLoadedPromise, sprintDatesLoadedPromise).done(function () {
            for (var idx = 0; idx < byLine.length; idx++) {
                var line = byLine[idx];
                line.estimate = sumEstimates(line.items);
            }
            console.log(byLine);
        });

    });


});