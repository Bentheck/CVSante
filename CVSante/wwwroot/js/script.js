
window.onload = function () {
    var modal = document.getElementById('memberModal');

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
        modal.style.display = 'flex';
    }

    function closeModal() {
        modal.style.display = 'none';
    }

    document.querySelectorAll('.team-member').forEach(function (element) {
        element.addEventListener('click', function () {
            let name = element.dataset.name;
            let role = element.dataset.role;
            let description = element.dataset.description;
            showDetails(name, role, description);
        });
    });

    // Fermer le modal en cliquant sur le "X"
    let closeButton = document.querySelector('.modals .close');
    if (closeButton) {
        closeButton.addEventListener('click', closeModal);
    }

    // Fermer le modal en cliquant en dehors du contenu du modal
    if (modal) {
        modal.addEventListener('click', function (event) {
            if (event.target === modal) {
                closeModal();
            }
        });
    }
};
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





