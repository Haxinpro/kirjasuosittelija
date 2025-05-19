document.addEventListener("DOMContentLoaded", function () {
    const toggle = document.getElementById("themeToggle");
    const root = document.documentElement;

    if (toggle) {  // 👉 tarkistetaan löytyykö napin elementti
        // Lataa tallennettu tila
        const savedTheme = localStorage.getItem("theme");
        if (savedTheme === "dark" || savedTheme === "dark-mode") {
            root.classList.add("dark-mode");
            toggle.checked = true;
        }

        toggle.addEventListener("change", () => {
            const isDark = toggle.checked;
            root.classList.toggle("dark-mode", isDark);
            localStorage.setItem("theme", isDark ? "dark" : "light");
        });
    }
});

