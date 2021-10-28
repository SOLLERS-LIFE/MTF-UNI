var navTabsType = "tabs";

var navTabs = document.querySelector('.nav.nav-' + navTabsType);
if (navTabs == null) {
    navTabsType = "pills";
    navTabs = document.querySelector('.nav.nav-' + navTabsType);
};
if (navTabs) {
    const tabBarId = navTabs.id;

    const triggerTabList = [].slice.call(document.querySelectorAll('.nav-' + navTabsType + ' a'));
    triggerTabList.forEach((triggerEl) => {
        const tabTrigger = new mdb.Tab(triggerEl);

        triggerEl.addEventListener('shown.mdb.tab', (event) => {
            localStorage.setItem(tabBarId, event.target.getAttribute("href"));
        });
    });

    const activeTab = localStorage.getItem(tabBarId);
    if (activeTab) {
        const triggerEl = document.querySelector('.nav-' + navTabsType + ' a[href="' + activeTab + '"]');
        mdb.Tab.getInstance(triggerEl).show();
    };
};