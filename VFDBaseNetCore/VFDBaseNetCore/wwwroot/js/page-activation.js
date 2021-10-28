"use strict";

// the reference should be <script id="pageActivation" [data-reloadTimeout="xxx"] src="~/js/page-activation.js">
var reloadTimeout = document.getElementById("pageActivation").getAttribute("data-reloadTimeout");
if (!reloadTimeout) { reloadTimeout = 15 * 60 * 1000 };
setTimeout("location.reload(true);", reloadTimeout);

var pivatConnection = new signalR.HubConnectionBuilder()
    .withUrl("/UsersInterconnectHub")
    .build();
// should be in production becouse of long db and pdf gen operations
pivatConnection.serverTimeoutInMilliseconds = _serverAndClientTimeout * 1000;
pivatConnection.keepAliveIntervalInMilliseconds = _serverAndClientkeepAliveInterval * 1000;

pivatConnection.start()
    .then(function() { return console.log("Activation commection established."); } )
    .catch(function (err) { return console.error(err.toString()); })
    ;

window.addEventListener("beforeunload",
    function (e) {
        pivatConnection.stop();
    }
);

pivatConnection.on("PageIsAvailable", function (prms) {
    //notify(prms.msg); // notify is from site.js, extention
    window.alert(prms.msg);
    // prms.pageIdent
});

pivatConnection.on("BroadCast", function (prms) {
    //notify(prms.msg); // notify is from site.js, extention
    window.alert(prms.msg);
});
