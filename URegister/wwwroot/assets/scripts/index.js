const navToggler = document.getElementById("navToggler");
const sidebar = document.getElementById("sidebar");
const mainNav = document.getElementById("mainNav");
const pageHeader = document.querySelector(".page-header");

const starCTA = document.querySelectorAll(".cta-star");

const caseTabBtn = document.querySelectorAll(".tabs-nav__btn");
const caseTabInfo = document.querySelectorAll(".case-tabs__info");

let timelineBtn = [];
let timelineContainer = [];

const dataLinks = document.querySelectorAll("[data-link]");

const navTogglerHandler = e => {
    sidebar.classList.toggle("sidebar--open");
    mainNav.classList.toggle("main-nav--open");
};

if (navToggler) {
    navToggler.addEventListener("click", navTogglerHandler);
}

if (caseTabBtn) {
    caseTabBtn.forEach(tabBtn => {
        tabBtn.addEventListener("click", tabNavHandler, false);
    });
}

if (dataLinks) {
    dataLinks.forEach(dataLink => {
        dataLink.addEventListener("click", dataLinkHandler);
    });
}

function initTimeline() {
    timelineBtn = document.querySelectorAll(".timeline__btn");
    timelineContainer = document.querySelectorAll(".timeline__container");

    if (timelineBtn) {
        timelineBtn.forEach(tabBtn => {
            tabBtn.addEventListener("click", timelineHandler, false);
        });
    }
}

window.addEventListener("scroll", throttle(onScrollHandler, 200));

//if menu is taller than screen
if (mainNav) {
    mainNav.querySelector(".main-nav__list").offsetHeight > window.innerHeight ? mainNav.classList.add("position-unset") : "";
}

function ariaToggleClickEvent(e) {
    e.preventDefault();
    e.stopPropagation();
    const el = e.target;

    if (el.getAttribute("aria-checked") === "true") {
        el.setAttribute("aria-checked", "false");

        el.querySelector("span:first-child").setAttribute("aria-hidden", "false");
        el.querySelector("span:last-child").setAttribute("aria-hidden", "true");
    } else {
        el.setAttribute("aria-checked", "true");
        el.querySelector("span:first-child").setAttribute("aria-hidden", "true");
        el.querySelector("span:last-child").setAttribute("aria-hidden", "false");
    }
}

function tabNavHandler(e) {
    const activeBtn = e.target;
    const targetCaseInfo = document.querySelector(`${activeBtn.getAttribute("data-bs-target")}`);

    activeBtn.classList.add("active");
    targetCaseInfo.classList.add("active");

    caseTabBtn.forEach(btn => {
        btn !== activeBtn ? btn.classList.remove("active") : "";
        btn !== activeBtn ? btn.setAttribute("aria-expanded", false) : "";
    });

    caseTabInfo.forEach(tabInfo => {
        tabInfo !== targetCaseInfo ? tabInfo.classList.remove("active") : "";
    });
}

function timelineHandler(e) {
    const activeBtn = e.target;
    const targetCaseInfo = document.querySelector(`${activeBtn.getAttribute("data-bs-target")}`);

    activeBtn.classList.add("active");
    targetCaseInfo.classList.add("active");

    timelineBtn.forEach(btn => {
        btn !== activeBtn ? btn.classList.remove("active") : "";
        btn !== activeBtn ? btn.setAttribute("aria-expanded", false) : "";
    });

    timelineContainer.forEach(tabInfo => {
        tabInfo !== targetCaseInfo ? tabInfo.classList.remove("active") : "";
    });
}

function dataLinkHandler(e) {
    const targetLink = this.dataset.link;
    window.open(`${targetLink}`, "_self");
}

function onScrollHandler() {
    if (window.innerWidth < 768) {
        window.scrollY > 120 ? pageHeader.classList.add("page-header--fixed") : pageHeader.classList.remove("page-header--fixed");
    }
}

function throttle(callback, limit) {
    var wait = false;
    return function () {
        if (!wait) {
            callback.call();
            wait = true;
            setTimeout(function () {
                wait = false;
            }, limit);
        }
    };
}

document.head.insertAdjacentHTML("beforeend", `<style></style>`);
