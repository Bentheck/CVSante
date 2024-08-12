
// Scripts
// 
//document.getElementById('sidebarToggle').addEventListener('click', function () {
//    document.getElementById('layoutSidenav_nav').classList.toggle('sb-sidenav-toggled');
//    document.body.classList.toggle('sb-sidenav-toggled');
//});
window.addEventListener('DOMContentLoaded', event => {

    // Toggle the side navigation
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        // Uncomment Below to persist sidebar toggle between refreshes
        // if (localStorage.getItem('sb|sidebar-toggle') === 'true') {
        //     document.body.classList.toggle('sb-sidenav-toggled');
        // }
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
        });
    }

});
document.querySelectorAll('.nav-link.collapsed').forEach(function (element) {
    element.addEventListener('click', function () {
        const targetId = this.getAttribute('data-bs-target');
        const targetElement = document.querySelector(targetId);

        if (targetElement) {
            targetElement.classList.toggle('show');
        }
    });
});
document.addEventListener('DOMContentLoaded', function () {
    const rolesList = document.getElementById('rolesList');
    const createResponder = document.getElementById('createResponder');
    const viewHistory = document.getElementById('viewHistory');
    const viewCitizenFile = document.getElementById('viewCitizenFile');

    rolesList.addEventListener('click', function (event) {
        if (event.target.classList.contains('role-item')) {
            const role = event.target.getAttribute('data-role');

            // Clear all checkboxes
            createResponder.checked = false;
            viewHistory.checked = false;
            viewCitizenFile.checked = false;

            // Set checkboxes based on role
            if (role === 'Admin') {
                createResponder.checked = true;
                viewHistory.checked = true;
                viewCitizenFile.checked = true;
            } else if (role === 'Paramedic') {
                viewHistory.checked = true;
            } else if (role === 'Hybride') {
                createResponder.checked = true;
                viewCitizenFile.checked = true;
            }
        }
    });

    document.getElementById('returnBtn').addEventListener('click', function () {
        window.history.back();
    });
});
//document.getElementById('themeToggle').addEventListener('click', function () {
//    const body = document.body;
//    const sunIcon = document.getElementById('sunIcon');
//    const moonIcon = document.getElementById('moonIcon');

//    body.classList.toggle('dark-mode');

//    if (body.classList.contains('dark-mode')) {
//        sunIcon.classList.add('d-none');
//        moonIcon.classList.remove('d-none');
//    } else {
//        sunIcon.classList.remove('d-none');
//        moonIcon.classList.add('d-none');
//    }
//});
document.addEventListener("DOMContentLoaded", function () {
    const themeToggle = document.getElementById("themeToggle");
    const sunIcon = document.getElementById("sunIcon");
    const moonIcon = document.getElementById("moonIcon");

    // Check the current theme mode from localStorage or another source
    const isDarkMode = localStorage.getItem("darkMode") === "true";

    if (isDarkMode) {
        document.body.classList.add("dark-mode");
        moonIcon.classList.remove("d-none");
        sunIcon.classList.add("d-none");
    } else {
        document.body.classList.remove("dark-mode");
        moonIcon.classList.add("d-none");
        sunIcon.classList.remove("d-none");
    }

    themeToggle.addEventListener("click", function () {
        document.body.classList.toggle("dark-mode");
        const darkModeEnabled = document.body.classList.contains("dark-mode");

        // Update icons
        moonIcon.classList.toggle("d-none", !darkModeEnabled);
        sunIcon.classList.toggle("d-none", darkModeEnabled);

        // Save the current mode to localStorage
        localStorage.setItem("darkMode", darkModeEnabled);
    });
});
//document.addEventListener("DOMContentLoaded", function () {
//    const themeToggle = document.getElementById("themeToggle");
//    const sunIcon = document.getElementById("sunIcon");
//    const moonIcon = document.getElementById("moonIcon");

//    // Vérifier le mode actuel du thème à partir de localStorage ou d'une autre source
//    const isDarkMode = localStorage.getItem("darkMode") === "true";

//    if (isDarkMode) {
//        document.body.classList.add("dark-mode");
//        moonIcon.classList.remove("d-none");
//        sunIcon.classList.add("d-none");
//    } else {
//        document.body.classList.remove("dark-mode");
//        moonIcon.classList.add("d-none");
//        sunIcon.classList.remove("d-none");
//    }

//    themeToggle.addEventListener("click", function () {
//        document.body.classList.toggle("dark-mode");
//        const darkModeEnabled = document.body.classList.contains("dark-mode");

//        // Mettre à jour les icônes
//        moonIcon.classList.toggle("d-none", !darkModeEnabled);
//        sunIcon.classList.toggle("d-none", darkModeEnabled);

//        // Enregistrer le mode actuel dans localStorage
//        localStorage.setItem("darkMode", darkModeEnabled);
//    });
 
//});
