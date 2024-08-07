document.addEventListener('DOMContentLoaded', function () {

    function showDetails(name, role, description) {
        let details = `
            <h2>${name}</h2>
            <p><strong>${role}</strong></p>
            <p>${description}</p>
        `;
        document.getElementById('memberDetails').innerHTML = details;
        openModal();
    }

    function openModal() {
        document.getElementById('memberModal').style.display = 'flex';
    }

    function closeModal() {
        document.getElementById('memberModal').style.display = 'none';
    }

    document.querySelectorAll('.team-member').forEach(function (element) {
        element.addEventListener('click', function () {
            let name = element.dataset.name;
            let role = element.dataset.role;
            let description = element.dataset.description;
            showDetails(name, role, description);
        });
    });

    // Vérifiez l'existence de l'élément avant d'ajouter l'écouteur d'événements
    let closeButton = document.querySelector('.modal .close');
    if (closeButton) {
        closeButton.addEventListener('click', closeModal);
    }
});
function toggleAnswer(element) {
    const answerElement = element.nextElementSibling;
    if (answerElement.style.display === 'none' || answerElement.style.display === '') {
        answerElement.style.display = 'block';
    } else {
        answerElement.style.display = 'none';
    }
}

$(window).scroll(function () {
    if ($(this).scrollTop() > 50) {
        $('.navbar-custom').addClass('scrolled');
    } else {
        $('.navbar-custom').removeClass('scrolled');
    }
});




