var MComms = function () {
    var selectedUser = {};
    var selectedFlight = {};
    var _init = function (MasterDiv) {
        $.ajax({
            type: 'GET',
            url: '/ADSB/GetActivePilotsList',
            dataType: "json",
            async: true,
            success: function (data) {

                LoadActivePilots(MasterDiv, data);
            },
            error: function () {
                alert('error')
            }
        });
    };

    var LoadActivePilots = function (MasterDiv, PilotList) {
        jQuery.each(PilotList.ActiveUser, function (i, val) {
            $newListItem = $('<li class="ActPilot"/>');
            var flightinfo = jQuery.grep(PilotList.ActiveFlight, function (element, index) {
                return (element.PilotID == val.UserID);
            });

                $newListItem.on("click", function () {
                    $("ul.Masterul li").removeClass('selected');
                    $("ul.Masterul li").attr('class', 'ActPilot');
                $(this).attr('class', 'selected');
                selectedUser = val;
                FlightInfo.FlightID = flightinfo[0].FlightID;
                $('#ComsList').empty();
                COMMS.Init();
            });
                $newdiv = $('<p/>').text(val.Name);
                $newListItem.append($newdiv);
                MasterDiv.append($newListItem);

            });
         
        };

    
    return {
        Init: _init

    };
}();

var Util = function () {
    this._FmtTime = function (nDate) {
        return _pad(nDate.getHours()) + ':' + _pad(nDate.getMinutes()) + ':' + _pad(nDate.getSeconds());
    };


    var _toDateTime = function (sNetDate) {
        var nDate = new Date();
        if (sNetDate instanceof Date && isFinite(sNetDate)) {
            nDate = sNetDate;
        } else if (sNetDate !== null) {
            var r = /\/Date\(([0-9]+)\)\//i
            var matches = sNetDate.match(r);
            if (matches.length === 2) {
                nDate = new Date(parseInt(matches[1]));
            }
        }
        //Convert date to UTC
        //nDate.setMinutes(nDate.getMinutes() + nDate.getTimezoneOffset());
        return nDate;
    };


    var _pad = function (Num) {
        if (Num >= 10) return Num;
        return '0' + Num;
    };

    var _toDateString = function (nDate) {
        var data =
            _pad(nDate.getFullYear()) + '-' + _pad(nDate.getMonth() + 1) + '-' + _pad(nDate.getDate()) + " " +
            _pad(nDate.getHours()) + ':' + _pad(nDate.getMinutes()) + ':' + _pad(nDate.getSeconds()) + '.' + nDate.getMilliseconds();

        return data;
    };


    var _toTime = function (time) {
        var sec_num = parseInt(time, 10); // don't forget the second param
        var hours = Math.floor(sec_num / 3600);
        var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
        var seconds = sec_num - (hours * 3600) - (minutes * 60);

        if (hours < 10) { hours = "0" + hours; }
        if (minutes < 10) { minutes = "0" + minutes; }
        if (seconds < 10) { seconds = "0" + seconds; }
        return hours + ':' + minutes + ':' + seconds;
    };


    var rad = function (x) {
        return x * Math.PI / 180;
    };

    var _getDistance = function (p1, p2) {
        var R = 6378137; // Earth’s mean radius in meter
        var dLat = rad(p2.lat - p1.lat);
        var dLong = rad(p2.lng - p1.lng);
        var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
            Math.cos(rad(p1.lat)) * Math.cos(rad(p2.lat)) *
            Math.sin(dLong / 2) * Math.sin(dLong / 2);
        var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        var d = R * c;
        return d; // returns the distance in meter
    };

    return {
        FmtTime: _FmtTime,
        toDateTime: _toDateTime,
        toTime: _toTime,
        toString: _toDateString,
        getDistance: _getDistance
    };
}();