/*!
    * Start Bootstrap - SB Admin v7.0.7 (https://startbootstrap.com/template/sb-admin)
    * Copyright 2013-2023 Start Bootstrap
    * Licensed under MIT (https://github.com/StartBootstrap/startbootstrap-sb-admin/blob/master/LICENSE)
    */
    // 
// Scripts
// 

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
document.addEventListener("DOMContentLoaded", function () {
    const themeToggle = document.getElementById("themeToggle");
    const sunIcon = document.getElementById("sunIcon");
    const moonIcon = document.getElementById("moonIcon");

    // Vérifiez le mode actuel du thème à partir de localStorage ou d'une autre source
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

        // Mettez à jour les icônes
        moonIcon.classList.toggle("d-none", !darkModeEnabled);
        sunIcon.classList.toggle("d-none", darkModeEnabled);

        // Enregistrez le mode actuel dans localStorage
        localStorage.setItem("darkMode", darkModeEnabled);
    });
});
