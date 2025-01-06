function ef(type, attr, ...children) {
    const el = document.createElement(type);

    for (const key in attr) {
        if (key === "event") {
            if (Array.isArray(attr[key])) {
                attr[key].map(function ({ type, callback }) {
                    el.addEventListener(type, callback);
                });
            } else {
                el.addEventListener(attr[key].type, attr[key].callback);
            }
        } else {
            el.setAttribute(key, attr[key]);
        }
    }

    children.map(function (child) {
        if (typeof child === "string" || typeof child === "number") {
            el.appendChild(document.createTextNode(child));
        } else {
            el.appendChild(child);
        }
    });
    return el;
}

const accessibility = (parentSelector = "body", navSelector = ".main-nav", mainSelector = "main", linkSelector = "h1") => {
    const parent = document.querySelector(parentSelector);
    const nav = document.querySelector(navSelector);
    const main = document.querySelector(mainSelector);
    const link = document.querySelectorAll(linkSelector);

    const html = document.querySelector("html");
    const body = document.querySelector("body");

    const increaseFontSize = e => {
        html.classList.toggle("font-resize");
        body.classList.toggle("font-resize");
        localStorage.setItem("accesibility.fontSize", body.classList.contains("font-resize"));
        document.querySelector(".a11y__item--increase").querySelector("span").textContent = body.classList.contains("font-resize") ? "Намали шрифта" : "Увеличи шрифта";
    };

    const toggleDyslexicFont = () => {
        localStorage.setItem("accesibility.dyslectic", !localStorage.getItem("accesibility.dyslectic"));
        html.classList.toggle("dyslectic-font");
        body.classList.toggle("dyslectic-font");
    };

    const themeChange = theme => {
        const classlist = ["accessibility-yellow", "accessibility-dark", "accessibility-blue"];
        localStorage.setItem("accesibility.theme", theme);
        classlist.map(e => html.classList.remove(e));
        if (theme) {
            html.classList.add("accessibility-" + theme);
        }
    };
    let textOnly = { v: false };
    const textVersion = () => {
        for (let i = 0; i < document.styleSheets.length; i++) {
            if (document.styleSheets.item(i)) {
                textOnly.v
                    ? //@ts-ignore
                      (document.styleSheets.item(i).disabled = false)
                    : //@ts-ignore
                      (document.styleSheets.item(i).disabled = !document.styleSheets.item(i).disabled);
            }
        }
        textOnly.v = !textOnly.v;
        localStorage.setItem("accesibility.textOnly", textOnly.v);
    };

    const reset = () => {
        themeChange(null);
        html.classList.remove("dyslectic-font");
        body.classList.remove("dyslectic-font");
        html.classList.remove("font-resize");
        body.classList.remove("font-resize");
        if (textOnly.v) {
            textVersion();
        }
        const lc = ["fontSize", "dyslectic", "theme", "textOnly"];
        lc.map(e => {
            localStorage.removeItem("accesibility." + e);
        });
    };
    const getSettings = () => {
        localStorage.getItem("accesibility.fontSize") && increaseFontSize();
        localStorage.getItem("accesibility.dyslectic") && toggleDyslexicFont();
        localStorage.getItem("accesibility.theme") && themeChange(localStorage.getItem("accesibility.theme"));
        localStorage.getItem("accesibility.textOnly") && textVersion();
    };
    const buttonsData = [
        ["a11y__item a11y__item--increase", localStorage.getItem("accesibility.fontSize") ? "Намали шрифта" : "Увеличи шрифта", increaseFontSize],
        ["a11y__item a11y__item--dyslexic", "За хора с дислексия", toggleDyslexicFont],
        ["a11y__item a11y__item--textonly", "Текстова версия", textVersion.bind(null, textOnly.v)],
        ["a11y__item a11y__item--clear", "Нулирай", reset]
    ];
    const themeButtonsData = [
        ["a11y__btn a11y__btn--c a11y__btn--yellow", "жълта тема", themeChange.bind(null, "yellow")],
        ["a11y__btn a11y__btn--c a11y__btn--dark", "тъмна тема", themeChange.bind(null, "dark")],
        ["a11y__btn a11y__btn--c a11y__btn--blue", "синя тема", themeChange.bind(null, "blue")],
        ["a11y__btn a11y__btn--c a11y__btn--clear", "оригинална тема", themeChange.bind(null)]
    ];

    const controlPanelNav = ef(
        "nav",
        {
            class: "a11y-tools__nav",
            "aria-hidden": false,
            "aria-label": "accessibility skip content navigation",
            tabIndex: 0
        },
        ef("ul", { class: "a11y-tools__list" }, ef("li", {}, ef("h2", { class: "a11y-tools__title" }, "ПРОМЕНИ КОНТРАСТА"), ef("div", { class: "a11y-tools__contrast" }, ...themeButtonsData.map(([c, t, f]) => ef("button", { class: c, "area-label": t, event: { type: "click", callback: f } }, ef("span", { "area-hidden": true }, "C"))))), ...buttonsData.map(([c, t, f]) => ef("li", { class: c, event: { type: "click", callback: f } }, ef("button", {}, ef("span", {}, t)))), ef("li", { class: "a11y__item a11y__item--credits" }, ef("p", {}, "© Информационно обслужване АД")))
    );

    const controlPanel = ef("div", { class: "accessibility-controls a11y-tools", tabIndex: 0 }, ef("button", { class: "a11y-tools__button", "aria-label": "покажи меню за достъпност", tabIndex: -1 }, ef("div", { class: "sr-only" }, "Меню за достъпност")), controlPanelNav);
    parent.insertAdjacentElement("afterbegin", controlPanel);

    const leftContent = ef("nav", { class: "accessibility-content" }, ef("ul", { class: "innerUl" }, ef("li", { "data-target": navSelector, class: "a11y-def-click" }, ef("a", { href: "#" }, "КЪМ НАВИГАЦИЯТА")), ef("li", { "data-target": mainSelector, class: "a11y-def-click" }, ef("a", { href: "#" }, "КЪМ ОСНОВНОТО СЪДЪРЖАНИЕ")), ef("li", { class: "ally-click" }, ef("ul", {}, ...[...document.querySelectorAll(linkSelector)].map(e => ef("li", {}, ef("a", { href: "#" }, e.textContent)))))));
    parent.insertAdjacentElement("afterbegin", leftContent);
    [...document.querySelector(".ally-click").querySelectorAll("li"), ...document.querySelectorAll(".a11y-def-click")].map((e, i) => {
        e.addEventListener("click", ev => {
            ev.preventDefault();

            const el = e.dataset.target ? document.querySelector(e.dataset.target) : document.querySelectorAll(linkSelector)[i];
            el.tabIndex = 0;
            el.focus();
            el.scrollIntoView({ behavior: "smooth", block: "center", inline: "center" });

            el.addEventListener("blur", () => {
                el.removeAttribute("tabIndex");
            });
        });
    });
    getSettings();
};

if (screen.width >= 768) {
    document.addEventListener("DOMContentLoaded", () => accessibility("body", "#mainNav", "main", "sss"));
}
