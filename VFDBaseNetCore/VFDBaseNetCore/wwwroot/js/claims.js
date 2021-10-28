"use strict";

const _modalYNclaim = document.getElementById('ModalYNClaim');
const claimModal = new mdb.Modal(
    _modalYNclaim,
    {
    backdrop: false,
    keyboard: false
    }
);

const _modalYNclaimtxt = document.getElementById('inputTxtClaim')

_modalYNclaim.addEventListener('shown.mdb.modal',
    () => {
        _modalYNclaimtxt.focus();
    }
);

function opnModalYNClaim(caption, message, _value, resTp, numLines, placeholder) {
    
    document.getElementById('ModalYNCaptionClaim').innerHTML = caption;
    document.getElementById('ModalYNMsgClaim').innerHTML = message;
    document.getElementById('inputTxtClaim').value = _value;
    if (numLines == null) { numLines = 1; };
    document.getElementById('inputTxtClaim').setAttribute("rows", numLines);
    document.getElementById('inputTxtClaim').setAttribute("placeholder", placeholder);
    claimModal.show();
};

//Disable send button until connection is established
document.getElementById("btnOKClaim").disabled = true;

var claimconnection = new signalR.HubConnectionBuilder().withUrl("/UsersInterconnectHub").build();
// just for development
//claimconnection.serverTimeoutInMilliseconds = 180000;
//claimconnection.keepAliveIntervalInMilliseconds = 90000;

claimconnection.start().then(function () {
    document.getElementById("btnOKClaim").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

function acceptMessageClaim(event) {
    var msg = document.getElementById("inputTxtClaim").value;
    if (msg == null || msg == "") {
        alert("Please write something in the feedback text");
        return 1;
    };
    claimconnection.invoke("SendClaim", { message: msg }).catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("inputTxtClaim").value = "";
    return 0;
};

document.getElementById("btnOKClaim").addEventListener("click",
    function (e) {
        var rc = acceptMessageClaim(e);
        //e.preventDefault();
        if (rc == 0) {
            claimModal.hide();
            alert("Thank you!");
        }
    });


