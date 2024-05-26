/**
 * Loads external HTML elements dynamically.
 */
async function LoadElements()
{
    let includeElements = document.querySelectorAll('[data-include]');

    for (const element of includeElements)
    {
        let name = element.getAttribute("data-include");
        let file = './elements/' + name + '.html';

        let response = await fetch(file);
        let data = await response.text();

        ReplaceElement(element, data);
    }
}

/**
 * Replaces an element with another, while executing scripts.
 * Will not work if the replacement element has more than 2 root nodes.
 * @param element {Element} The target element.
 * @param html {string} The HTML to replace the element with.
 */
function ReplaceElement(element, html)
{
    let next = element.nextElementSibling;
    element.outerHTML = html;

    let newElement = next.previousElementSibling;

    for (const originalScript of newElement.getElementsByTagName("script"))
    {
        const newScriptEl = document.createElement("script");

        for (const attr of originalScript.attributes)
        {
            newScriptEl.setAttribute(attr.name, attr.value)
        }

        const scriptText = document.createTextNode(originalScript.innerHTML);
        newScriptEl.appendChild(scriptText);

        originalScript.parentNode.replaceChild(newScriptEl, originalScript);
    }
}

/**
 * Opens the page burger menu.
 */
function OpenBurger()
{
    let burgerMenu = document.getElementById("burgerMenu");
    let isShown = burgerMenu.classList.contains("shown");

    burgerMenu.classList.toggle("shown", !isShown);
}

/**
 * Hides the burger menu if clicked outside of its bounds.
 * Only for mobile.
 * @param {MouseEvent} e The mouse click event.
 */
function HandleClick(e)
{
    let burgerMenu = document.getElementById("burgerMenu");
    let burgerButton = document.getElementById("burgerButton");

    if (!burgerMenu.contains(e.target) && !burgerButton.contains(e.target))
    {
        burgerMenu.classList.toggle("shown", false);
    }
}

function GoToTag(tag)
{
    let burgerMenu = document.getElementById("burgerMenu");

    window.location.hash = tag;

    burgerMenu.classList.toggle("shown", false);
}
