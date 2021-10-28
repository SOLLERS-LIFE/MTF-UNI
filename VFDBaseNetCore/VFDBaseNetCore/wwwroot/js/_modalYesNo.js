var resType = 'not defined';
const _modalYN = document.getElementById('ModalYN')
const _modalYNtxt = document.getElementById('ModalYNinputTxt')
const _modalYNinstance = new mdb.Modal(_modalYN);

_modalYN.addEventListener('shown.mdb.modal',
    () => {
        _modalYNtxt.focus();
    }
);

function opnModalYN(caption, message, _value, resTp, numLines, placeholder) {
    resType = resTp;
    document.getElementById('ModalYNCaption').innerHTML = caption;
    document.getElementById('ModalYNMsg').innerHTML = message;
    //document.getElementById('ModalYNMsg-01').innerHTML = "";
    document.getElementById('ModalYNinputTxt').value = _value;
    if (numLines == null) { numLines = 1; };
    document.getElementById('ModalYNinputTxt').setAttribute("rows", numLines);
    document.getElementById('ModalYNinputTxt').setAttribute("placeholder", placeholder);

    _modalYNinstance.show();
}
// function to call user responce
function opnModalYNYesWrapper() {
    var resVal = document.getElementById('ModalYNinputTxt').value;
    _modalYNinstance.hide();
    window[resType+"Yes"](resType, resVal);
}
function opnModalYNNoWrapper() {
    var resVal = document.getElementById('ModalYNinputTxt').value;
    _modalYNinstance.hide();
    window[resType + "No"](resType, resVal);
}
