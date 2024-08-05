// validation.js

// Fonction d'affichage des messages d'erreur
function afficherErreur(element, message) {
    const elementErreur = document.createElement('span');
    elementErreur.classList.add('error-message');
    elementErreur.style.color = 'red';
    elementErreur.textContent = message;
    element.parentNode.appendChild(elementErreur);
}

// Supprimer tous les messages d'erreur
function supprimerErreurs() {
    document.querySelectorAll('.error-message').forEach(e => e.remove());
}

// Valider la section des informations utilisateur
function validerInformationsUtilisateur() {
    supprimerErreurs();
    let estValide = true;

    const nom = document.getElementById('nom');
    if (!nom.value.trim()) {
        afficherErreur(nom, 'Le nom est requis.');
        estValide = false;
    }

    const prenom = document.getElementById('prenom');
    if (!prenom.value.trim()) {
        afficherErreur(prenom, 'Le prénom est requis.');
        estValide = false;
    }

    const dateNaissance = document.getElementById('dateNaissance');
    if (!dateNaissance.value.trim() || isNaN(new Date(dateNaissance.value).getTime())) {
        afficherErreur(dateNaissance, 'La date de naissance est invalide.');
        estValide = false;
    }

    const telephoneCell = document.getElementById('telephoneCell');
    if (!telephoneCell.value.trim() || !/^\d{3}-\d{3}-\d{4}$/.test(telephoneCell.value)) {
        afficherErreur(telephoneCell, 'Le numéro de téléphone est invalide.');
        estValide = false;
    }

    const typeSanguin = document.querySelector('input[name="UserInfo.TypeSanguin"]:checked');
    if (!typeSanguin) {
        afficherErreur(document.getElementById('typeSanguinGroup'), 'Le type sanguin est requis.');
        estValide = false;
    }

    return estValide;
}

// Valider la section des adresses
function validerAdresses() {
    supprimerErreurs();
    let estValide = false;

    const adressesPrimaires = document.querySelectorAll('input.address-primary');
    adressesPrimaires.forEach(checkbox => {
        if (checkbox.checked) {
            estValide = true;
        }
    });

    if (!estValide) {
        afficherErreur(adressesPrimaires[0].parentNode, 'Vous devez sélectionner au moins une adresse comme primaire.');
    }

    return estValide;
}

// Valider la section des antécédents
function validerAntecedents() {
    supprimerErreurs();
    let estValide = true;

    const autreCheckbox = document.querySelector('input[name="antecedents"][value="Autre"]');
    const saisieManuelle = document.getElementById('autre-details');
    if (autreCheckbox.checked && !saisieManuelle.value.trim()) {
        afficherErreur(saisieManuelle, 'Veuillez fournir des détails si "Autre" est sélectionné.');
        estValide = false;
    }

    return estValide;
}

// Valider la section des allergies
function validerAllergies() {
    supprimerErreurs();
    let estValide = true;

    const radiosAllergies = document.querySelectorAll('.allergy-intolerance-radio');
    const champsSeverite = document.querySelectorAll('.severity-field');
    let severiteSelectionnee = false;

    radiosAllergies.forEach(radio => {
        if (radio.checked && radio.value === 'Allergie') {
            champsSeverite.forEach(champ => {
                if (champ.style.display === 'block') {
                    const radiosSeverite = champ.querySelectorAll('input[name$=".Gravite"]');
                    severiteSelectionnee = Array.from(radiosSeverite).some(severite => severite.checked);
                }
            });
        }
    });

    if (!severiteSelectionnee && Array.from(radiosAllergies).some(radio => radio.checked && radio.value === 'Allergie')) {
        afficherErreur(document.querySelector('.allergy-intolerance-radio'), 'Veuillez sélectionner la sévérité de l\'allergie.');
        estValide = false;
    }

    return estValide;
}

// Valider la section des handicaps
function validerHandicaps() {
    supprimerErreurs();
    let estValide = true;

    const handicaps = document.querySelectorAll('input[name^="Handicaps"][type="radio"]');
    let aUnHandicap = false;

    handicaps.forEach(radio => {
        if (radio.checked) {
            aUnHandicap = true;
        }
    });

    if (!aUnHandicap) {
        afficherErreur(document.getElementById('handicap-fields'), 'Vous devez sélectionner au moins un handicap.');
        estValide = false;
    }

    return estValide;
}

// Valider la section des médicaments
function validerMedicaments() {
    supprimerErreurs();
    let estValide = true;

    const radiosMedicaments = document.querySelectorAll('.card:last-child input[name^="Medications"][name$="MedicamentProduitNat"]');
    radiosMedicaments.forEach(radio => {
        if (radio.checked) {
            const card = radio.closest('.card-body');
            const posologie = card.querySelector('input[name$=".Posologie"]').value.trim();
            const raison = card.querySelector('input[name$=".Raison"]').value.trim();

            if (radio.value === 'Medicament' && !posologie) {
                afficherErreur(card.querySelector('input[name$=".Posologie"]'), 'La posologie est requise pour les médicaments.');
                estValide = false;
            }

            if (radio.value === 'Produit Naturel' && raison) {
                afficherErreur(card.querySelector('input[name$=".Raison"]'), 'La raison ne doit pas être remplie pour les produits naturels.');
                estValide = false;
            }
        }
    });

    return estValide;
}

// Valider toutes les sections
function validerFormulaire() {
    const sections = ['userinfo', 'address', 'antecedent', 'allergy', 'handicap', 'medication'];
    let estValide = true;

    // Fonction pour valider chaque section selon l'index
    function validerSection(index) {
        if (index >= sections.length) return true;

        let sectionEstValide = false;
        switch (sections[index]) {
            case 'userinfo':
                sectionEstValide = validerInformationsUtilisateur();
                break;
            case 'address':
                sectionEstValide = validerAdresses();
                break;
            case 'antecedent':
                sectionEstValide = validerAntecedents();
                break;
            case 'allergy':
                sectionEstValide = validerAllergies();
                break;
            case 'handicap':
                sectionEstValide = validerHandicaps();
                break;
            case 'medication':
                sectionEstValide = validerMedicaments();
                break;
        }

        if (!sectionEstValide) {
            estValide = false;
        }

        return validerSection(index + 1);
    }

    return validerSection(0) && estValide;
}

// Attacher la validation au bouton de sauvegarde
document.getElementById('save-btn').addEventListener('click', function (event) {
    if (!validerFormulaire()) {
        event.preventDefault(); // Empêcher la soumission du formulaire si la validation échoue
    }
});

// Attacher la validation aux boutons de navigation
document.getElementById('prev-btn').addEventListener('click', previousSection);
document.getElementById('next-btn').addEventListener('click', function () {
    if (validerFormulaire()) {
        nextSection();
    }
});
