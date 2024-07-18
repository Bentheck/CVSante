document.addEventListener('DOMContentLoaded', function () {

    function showDetails(name, role, description) {
        var details = `
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
            var name = element.dataset.name;
            var role = element.dataset.role;
            var description = element.dataset.description;
            showDetails(name, role, description);
        });
    });

    // Vérifiez l'existence de l'élément avant d'ajouter l'écouteur d'événements
    var closeButton = document.querySelector('.modal .close');
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






