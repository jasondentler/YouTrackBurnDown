///<reference path="~/SignalR"/>
///<reference path="~/Scripts/lib/"/>
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
            '#{land-legal}'
        ];
        var filterLabels = [
            'BS NET',
            'BS JDE',
            'Marketing',
            'Headspring'
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
                var item = byProject[result.Item1];
                item.start = result.Item3;
                item.finish = result.Item4;
                if (start == null)
                    start = item.start;
                if (finish == null)
                    finish = item.finish;
                if (start != item.start) {
                    console.log(start);
                    console.log(item.start);
                    throw 'Start dates differ between projects';
                }
                if (finish != item.finish) {
                    console.log(finish);
                    console.log(item.finish);
                    throw 'Finish dates differ between projects';
                }
            }

            var startDt = new Date(start);
            startDt.setUTCHours(8, 0, 0, 0);
            start = startDt.toString();

            var finishDt = new Date(finish);
            finishDt.setUTCHours(8, 0, 0, 0);
            finishDt = new Date(finishDt.getTime() + 24 * 60 * 60 * 1000);
            finish = finishDt.toString();

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

        function deleteUnresolved(items) {
            console.log(items);
            for (var idx = 0; idx < items.length; idx++) {
                var item = items[idx];
                console.log(idx);
                console.log(item);
                while (idx < items.length && typeof item.resolved === 'undefined') {
                    items.splice(idx, 1);
                    item = items[idx];
                }
            }
        }

        function sortByResolved(items) {
            var sortFunc = function (a, b) {
                if (a.resolved < b.resolved) return -1;
                if (a.resolved > b.resolved) return 1;
                return 0;
            };
            items.sort(sortFunc);
        }

        function normalizeMaximums(chart) {
            var max = 0;
            for (var seriesIdx = 0; seriesIdx < chart.series.length; seriesIdx++) {
                var series = chart.series[seriesIdx].data;
                if (series[0][1] > max)
                    max = series[0][1];
            }

            for (seriesIdx = 0; seriesIdx < chart.series.length; seriesIdx++) {
                series = chart.series[seriesIdx].data;
                //if (series[0][1] == max)
                //continue;

                var factor = 100 * (max / series[0][1]) / max;
                for (var idx = 0; idx < series.length; idx++) {
                    var item = series[idx];
                    item[1] *= factor;
                }
            }
        }

        function countWorkingDays(start, finish) {
            var currentTicks = start.getTime();
            var finishTicks = finish.getTime();
            var workingDays = 0;

            while (currentTicks < finishTicks) {
                currentTicks += 24 * 60 * 60 * 1000;
                switch (new Date(currentTicks).getDay()) {
                    case 0:
                    case 1:
                        break;
                    default:
                        workingDays += 1;
                        break;
                }
            }
            return workingDays;
        }

        function getIdealPercentage(totalWorkingDays, date) {
            var currentWorkingDays = countWorkingDays(new Date(start), date);
            return (1 - (currentWorkingDays / totalWorkingDays)) * 100;
        }

        $.when(ticketDataLoadedPromise, sprintDatesLoadedPromise).done(function () {
            var chart = {
                chart: {
                    type: 'spline'
                },
                title: {
                    text: 'Sprint ' + selectedSprint + ' Burndown'
                },
                subtitle: {
                    text: new Date(new Date(start).getTime() + new Date().getTimezoneOffset() * 60 * 1000).toLocaleDateString() +
                        ' to ' + new Date(new Date(finish).getTime() + new Date().getTimezoneOffset() * 60 * 1000).toLocaleDateString()
                },
                xAxis: {
                    type: 'datetime'
                },
                yAxis: {
                    max: 100,
                    min: 0,
                    labels: {
                        format: '{value}%'
                    }
                },
                series: []
            };

            for (var idx = 0; idx < byLine.length; idx++) {
                var line = byLine[idx];
                line.estimate = sumEstimates(line.items);
                deleteUnresolved(line.items);
                sortByResolved(line.items);

                var series = { name: filterLabels[idx] + ': ' + Math.round(line.estimate) + ' pts', data: [[Date.parse(start), line.estimate]] };

                var remaining = line.estimate;
                for (var idx2 = 0; idx2 < line.items.length; idx2++) {
                    var item = line.items[idx2];
                    remaining -= item.estimate;

                    var last = series.data[series.data.length - 1][0];
                    var current = Date.parse(item.resolved);
                    var offset = new Date().getTimezoneOffset() * 60 * 1000;
                    current -= offset;
                    if (current <= last) {
                        if (series.data.length == 1) {
                            // If we get started early, leave the first point with the maximum
                            series.data.push([last, remaining]);
                        } else {
                            series.data[series.data.length - 1][1] = remaining;
                        }
                    } else {
                        var datapoint = [current, remaining];
                        series.data.push(datapoint);
                    }
                }

                last = Date.parse(finish);
                var now = new Date().getTime();
                console.log('Calculating last dot');
                console.log(last);
                console.log(now);
                console.log(new Date(last));
                console.log(new Date(now));
                if (now < last)
                    last = now;
                last -= new Date().getTimezoneOffset() * 60 * 1000;
                series.data.push([last, remaining]);

                chart.series.push(series);
            }

            normalizeMaximums(chart);

            series = { name: 'Ideal', lineWidth: 5, data: [] };
            var totalWorkingDays = countWorkingDays(new Date(start), new Date(finish));

            current = new Date(start);
            while (current < new Date(finish)) {
                series.data.push([current.getTime(), getIdealPercentage(totalWorkingDays, current)]);
                current = new Date(current.getTime() + 24 * 60 * 60 * 1000);
            }
            series.data.push([Date.parse(finish), 0]);

            chart.series.splice(0, 0, series);

            for (idx = 0; idx < chart.series.length; idx++) {
                series = chart.series[idx];
                for (idx2 = 0; idx2 < series.data.length; idx2++) {
                    item = series.data[idx2];
                    item[1] = Math.round(item[1]);
                }
            }

            $('#chart')
                .css('height', $(document).height() + 'px')
                .highcharts(chart);
        });

    });


});