"use strict";

// the reference should be <script id="pageActualization" data-pageIdent"="xxx" src="~/js/page-actualization.js">
// For ex:
// <script src="~/js/signalr/dist/browser/signalr.js"></script>
// <script id="pageActualization" 
// data-pageIdent="@Model.GetType().ToString()" data-avlMsg="yopanat omtinovitch"
// src = "~/js/page-actualization.js" ></script >
// userDocumentIsOccuped (pageIdent) - to react from page
var pageIdent = document.getElementById("pageActualization").getAttribute("data-pageIdent");
var avlMsg = document.getElementById("pageActualization").getAttribute("data-avlMsg");
var pualiConnection = new signalR.HubConnectionBuilder()
    .withUrl("/UsersInterconnectHub")
    .build();
// should be in production becouse of long db and pdf gen operations
pualiConnection.serverTimeoutInMilliseconds = _serverAndClientTimeout * 1000;
pualiConnection.keepAliveIntervalInMilliseconds = _serverAndClientkeepAliveInterval * 1000;

pualiConnection.start().then(function () {
                                console.log("Actualization connection established.");
                                ViewOpenBracket({ pi: pageIdent, msg: avlMsg });
                            })
    .catch(function (err) { return console.error(err.toString()); })
    ;

function ViewOpenBracket(prms) {
    pualiConnection.invoke("ClientOperation", { operation: "{", pageIdent: prms.pi, msgWhenPageAvailable: prms.msg })
        .then(function (res) {
            switch (res.res) {
                case "queued":
                    userDocumentIsOccuped({ pageIdent: prms.pi });
                    break;
                case "catched":
                    break;
                default:
                    ViewOpenBracketFault({ result: res.res, message: res.msg });
                    break;
            }
            return console.log(res.res + " " + res.msg);
        })
        .catch(function (err) {
            ViewOpenBracketFault({ result: "fault", message: err.toString() });
            return console.error(err.toString());
        });
}
function ViewOpenBracketFault(parms) {
    window.alert("You cannot open this document because of" + parms.message);
    document.getElementById("courtine").classList.toggle("opened");
}
function ViewCloseBracket(pi) {
    pualiConnection.invoke("ClientOperation", { operation: "}", pageIdent: pi })
        .then(function (res) {
            return console.log(res.res + " " + res.msg);
        })
        .catch(function (err) {
            return console.error(err.toString());
        });
}
window.addEventListener("beforeunload",
    function (e) {
        ViewCloseBracket(pageIdent);
        pualiConnection.stop();
    }
);
