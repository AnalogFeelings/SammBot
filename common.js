function LoadElements()
{
    let includeElements = document.querySelectorAll('[data-include]');

    for(const element of includeElements)
    {
        let name = element.getAttribute("data-include");

        let file = 'Elements/' + name + '.html';

        $.get(file, function(html_content)
        {
            $(element).replaceWith(html_content);
        }, 'html');
    }
}

function OpenBurger()
{
    let burgerMenu = document.getElementById("burgerMenu");
    let isShown = burgerMenu.classList.contains("shown");

    burgerMenu.classList.toggle("shown", !isShown);
}

function HandleClick(e)
{
    let burgerMenu = document.getElementById("burgerMenu");
    let burgerButton = document.getElementById("burgerButton");

    if (e.target !== burgerButton && e.target !== burgerMenu)
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