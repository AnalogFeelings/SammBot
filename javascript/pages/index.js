window.onload = InitPage;

async function InitPage()
{
    await LoadElements();

    window.addEventListener("mouseup", (e) => HandleClick(e));
}
