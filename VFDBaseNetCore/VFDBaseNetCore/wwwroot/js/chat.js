"use strict";

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

var maxChatLines = 13;

var chtconnection = new signalR.HubConnectionBuilder().withUrl("/UsersInterconnectHub").build();
// just for development
//chtconnection.serverTimeoutInMilliseconds = 180000;
//chtconnection.keepAliveIntervalInMilliseconds = 90000;

chtconnection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

chtconnection.on("ReceiveMessage", function (prms) {
    var msg = prms.message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = prms.user + ": " + msg;
    var li = document.createElement("li");
    li.innerHTML = "<span class=\"text-warning font-weight-bold\">" + prms.user + ": "+"</span>"+msg;
    var list = document.getElementById("messagesList");
    if (list.childElementCount>maxChatLines)
    {
        list.removeChild(list.lastChild);
    }
    list.insertBefore(li, list.childNodes[0]);
});

function acceptMessage(event) {
    var msg = document.getElementById("messageInput").value;
    chtconnection.invoke("SendMessage", { message: msg }).catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("messageInput").value = "";
};
document.getElementById("messageInput").addEventListener("keydown",
    function (e) {
        if ((e.keyCode == 13) && (e.ctrlKey == true))
        {
            acceptMessage(e);
            e.preventDefault();
        };
    }
);
document.getElementById("sendButton").addEventListener("click",
    function (e) {
        acceptMessage(e);
        e.preventDefault();
    }
);
