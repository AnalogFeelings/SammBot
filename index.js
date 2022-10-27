window.onload = InitPage;

function InitPage()
{
    LoadElements();

    twemoji.parse(document.body, {
        folder: "svg",
        ext: ".svg"
    });

    window.addEventListener("mouseup", (e) => HandleClick(e));
}