/**
 * Loads external HTML elements dynamically.
 */
function LoadElements()
{
    let includeElements = document.querySelectorAll('[data-include]');

    for(const element of includeElements)
    {
        let name = element.getAttribute("data-include");
        let file = 'Elements/' + name + '.html';

        // Had to make sync to prevent race conditions.
        $.ajax({
            async: false,
            url: file,
            success: function(html_content)
            {
                $(element).replaceWith(html_content);
            },
            dataType: 'html'
        });
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
