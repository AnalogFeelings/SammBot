window.onload = InitPage;

function InitPage()
{
    twemoji.parse(document.body, {
        folder: "svg",
        ext: ".svg"
    });
    InitBanner();

    window.addEventListener("mouseup", (e) => HandleClick(e));
}

function GoToUrl(url)
{
    location.href = url;
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

function InitBanner()
{
    let bannerLogoElement = document.getElementById("bannerLogo");
    let navBarElement = document.querySelector('[id="navBar"]');
    let stickyBannerElement = document.querySelector('[id="stickyBanner"]');
    let stickyObserver = new IntersectionObserver(([e]) => BannerCallback(e),
        {
            rootMargin: '-1px 0px 0px 0px',
            threshold: [1]
        });

    stickyObserver.observe(stickyBannerElement);

    function BannerCallback(e)
    {
        let isIntersecting = e.boundingClientRect.top === 0;

        bannerLogoElement.classList.toggle("shown", isIntersecting);

        stickyBannerElement.classList.toggle("sticky", isIntersecting);
        navBarElement.classList.toggle("sticky", isIntersecting);
    }
}