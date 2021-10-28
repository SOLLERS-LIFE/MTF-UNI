var resTypeSimple = 'not defined';

const _modalYNsimpl = document.getElementById('modalYNSimple')
const _modalYNsimpleNo = document.getElementById('modalYNsimpleNo')
const _modalYNsimpleinstance = new mdb.Modal(_modalYNsimpl);

_modalYNsimpl.addEventListener('shown.mdb.modal',
    () => {
        _modalYNsimpleNo.focus();
    }
);

function opnModalYNSimple(message, resTp, header) {
    resTypeSimple = resTp;
    document.getElementById('modalYNSimpleMsg').innerHTML = message;
    document.getElementById('modalYNSimpleLabel').innerHTML = header;

    _modalYNsimpleinstance.show();
};
function opnModalYNSimpleNoWrapper() {
    _modalYNsimpleinstance.hide();
    window[resTypeSimple + "SimpleNo"](resTypeSimple);
};
function opnModalYNSimpleYesWrapper() {
    _modalYNsimpleinstance.hide();
    window[resTypeSimple + "SimpleYes"](resTypeSimple);
};