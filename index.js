window.onload = InitPage;

function InitPage()
{
    LoadElements();

    window.addEventListener("mouseup", (e) => HandleClick(e));
}