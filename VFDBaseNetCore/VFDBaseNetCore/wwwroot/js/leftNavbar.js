//"use strict";
var bkgOffset = 50;
const browserStore = window.localStorage;

if (browserStore.getItem("LeftNavbarstateNotByHands") == null) {
    browserStore.setItem("LeftNavbarstateNotByHands", "true"); // "true"
}
if (browserStore.getItem("LeftNavbarCurState") == null) {
    browserStore.setItem("LeftNavbarCurState", "hidden"); // "hidden"
}

const sidenav = document.getElementById("sidenav01");
const sidenavInstance = new mdb.Sidenav(sidenav); //mdb.Sidenav.getInstance(sidenav);
//sidenavInstance.update({ transitionDuration: 0 }); // switch off fading
//sidenavInstance.update({ width: 340 }); // just width

let _innerWidth = null;
let _prevInnerWidth = null;

sidenav.addEventListener('shown.mdb.sidenav', event => {
    var br = document.getElementById("pageBarBottom");
    if (br != null) {
        br.style.marginLeft = bkgOffset + "px";
        unloadBrowserScrollBars();
    }

    browserStore.setItem("LeftNavbarCurState", "shown");
    document.getElementById("leftNavbarToggler").style.display = 'none';
});
sidenav.addEventListener('hidden.mdb.sidenav', event => {
    var br = document.getElementById("pageBarBottom");
    if (br != null) {
        br.style.marginLeft = "0px";
        unloadBrowserScrollBars();
    }

    browserStore.setItem("LeftNavbarCurState", "hidden");
    document.getElementById("leftNavbarToggler").style.display = 'block';
});

const setMode = (e) => {
    _innerWidth = window.innerWidth;
    if (_innerWidth == null) {
        _innerWidth = window.screen.width;
    }
    var curState = browserStore.getItem("LeftNavbarCurState");
    var stateNotByHands = browserStore.getItem("LeftNavbarstateNotByHands");

    // sidenavInstance = mdb.Sidenav.getInstance(sidenav);

    // if it is window loading (including reloading)
    if (_prevInnerWidth == null) {
        if (_innerWidth < medianLine) {
            sidenavInstance.changeMode("over");
            if (stateNotByHands == "false") {
                if (curState == "shown") {
                    sidenavInstance.show();
                };
            };
        } else {
            sidenavInstance.changeMode("side");
            if (stateNotByHands == "false") {
                if (curState == "shown") {
                    sidenavInstance.show();
                };
            } else {
                sidenavInstance.show();
            };
        }

        _prevInnerWidth = _innerWidth;
        return;
    }

    // if window is alredy loaded
    if ((_innerWidth < medianLine) && (_prevInnerWidth >= medianLine)) {
        sidenavInstance.changeMode("over");
        if ((curState == "shown") && (stateNotByHands == true)) {
            sidenavInstance.hide();
        };
    } else if ((_innerWidth >= medianLine) && (_prevInnerWidth < medianLine)) {
        sidenavInstance.changeMode("side");
        if ((curState == "hidden") && (stateNotByHands == true)) {
            sidenavInstance.show();
        };
    }

    _prevInnerWidth = _innerWidth;
    return;
};

function sidenavChangedByHands() {
    browserStore.setItem("LeftNavbarstateNotByHands", "false");
    setMode();

    return;
};

// call this during window loading
setMode();

// Event listeners
window.addEventListener("resize", setMode);