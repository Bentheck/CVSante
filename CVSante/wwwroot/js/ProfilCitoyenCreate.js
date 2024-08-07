const sections = ['userinfo', 'address', 'antecedent', 'allergy', 'handicap', 'medication'];
let currentSectionIndex = 0;
let addressIndex = 1;
let allergyIndex = 0;
let handicapIndex = 0;
let medicationIndex = 0;

//Mask pour le numéro de téléphone//
$(document).ready(function () {
    $('#telephoneCell').inputmask('999-999-9999');
});

//Fonction gérant les onglets//
function showSection(sectionId) {
    sections.forEach((section, index) => {
        const sectionElement = document.getElementById(section);
        sectionElement.style.display = section === sectionId ? 'block' : 'none';
        if (section === sectionId) {
            currentSectionIndex = index;
        }
    });
    updateNavPills();
    updateNavigationButtons();
}

function previousSection() {
    if (currentSectionIndex > 0) {
        currentSectionIndex--;
        showSection(sections[currentSectionIndex]);
    }
}

function nextSection() {
    if (currentSectionIndex < sections.length - 1) {
        currentSectionIndex++;
        showSection(sections[currentSectionIndex]);
    }
}

function updateNavPills() {
    sections.forEach((section, index) => {
        const navLink = document.getElementById(`nav-${section}`);
        if (index === currentSectionIndex) {
            navLink.classList.add('active');
        } else {
            navLink.classList.remove('active');
        }
    });
}

function updateNavigationButtons() {
    const prevButton = document.getElementById('prev-btn');
    const nextButton = document.getElementById('next-btn');

    prevButton.disabled = currentSectionIndex === 0;
    nextButton.disabled = currentSectionIndex === sections.length - 1;
}


//Fonctions gérant "autre" dans les antécédents//
function toggleManualEntry() {
    const otherCheckbox = document.querySelector('input[name="antecedents"][value="Autre"]');
    const manualEntry = document.getElementById('manual-entry');

    if (otherCheckbox.checked) {
        manualEntry.style.display = 'block';
    } else {
        manualEntry.style.display = 'none';
    }
    updateAntecedentsString();
}

function updateAntecedentsString() {
    const checkedCheckboxes = document.querySelectorAll('input[name="antecedents"]:checked');
    const manualEntryField = document.getElementById('autre-details');

    let antecedentsText = Array.from(checkedCheckboxes)
        .map(checkbox => checkbox.value)

    if (document.querySelector('input[name="antecedents"][value="Autre"]').checked) {
        const manualEntryText = manualEntryField.value.trim();
        if (manualEntryText) {
            antecedentsText.push(manualEntryText);
        }
    }

    document.getElementById('antecedent-aggregate').value = antecedentsText.join('/');
}

showSection(sections[0]);

document.querySelectorAll('input[name="antecedents"]').forEach(checkbox => {
    checkbox.addEventListener('change', () => {
        toggleManualEntry();
        updateAntecedentsString();
    });
});

document.getElementById('autre-details').addEventListener('input', updateAntecedentsString);


//Fonctions assurant qu'une seule adresse primaire est cochée//
function handlePrimaryAddressCheckbox() {
    const allPrimaryCheckboxes = document.querySelectorAll('input.address-primary');
    let anyChecked = Array.from(allPrimaryCheckboxes).some(box => box.checked);

    allPrimaryCheckboxes.forEach(box => {
        box.disabled = anyChecked && !box.checked;

        const errorMsg = box.closest('.card-body').querySelector('span');
        if (box.checked) {
            errorMsg.style.display = 'none';
        } else {
            errorMsg.style.display = anyChecked ? 'inline' : 'none';
        }
    });
}

document.addEventListener('click', function (event) {
    if (event.target.matches('input.address-primary')) {
        handlePrimaryAddressCheckbox();
    }
});


//Fonctions gérant les allergies//
function attachAllergyEventListeners() {
    const allergyRadioButtons = document.querySelectorAll('.allergy-intolerance-radio');
    allergyRadioButtons.forEach(radio => {
        radio.removeEventListener('change', handleAllergyChange);
        radio.addEventListener('change', handleAllergyChange);
    });
}

function handleAllergyChange() {
    setTimeout(() => {
        const severityField = this.closest('.card-body').querySelector('.severity-field');
        if (this.value === 'Allergie') {
            severityField.style.display = 'block';
        } else {
            severityField.style.display = 'none';
        }
    }, 0); // Ensure this runs after DOM updates
}

//Fonction permettant de retirer un dynamic field//
document.addEventListener('DOMContentLoaded', function () {
    document.body.addEventListener('click', function (event) {
        if (event.target.classList.contains('remove-field')) {
            const card = event.target.closest('.card');
            if (card) {
                card.remove();
            }
        }
    });
});

//Fonction permettant l'ajout de précision pour le sexe//
$(document).ready(function () {
    function updateSexeField() {
        let selectedValue = $('input[name="UserInfo.Sexe"]:checked').val();
        if (selectedValue === "Autre") {
            $('#autrePrecisionGroup').show();
            let autrePrecision = $('#autrePrecision').val();
            $('#hiddenSexe').val('Autre: ' + autrePrecision);
        } else {
            $('#autrePrecisionGroup').hide();
            $('#hiddenSexe').val(selectedValue);
        }
    }

    // Attach change event to radio buttons
    $('input[name="UserInfo.Sexe"]').change(function () {
        updateSexeField();
    });

    // Initial check
    updateSexeField();
});

//Fonction premettant la génération de la form des médicaments//
function attachMedicationEventListeners() {
    const medicationRadioButtons = document.querySelectorAll('.card:last-child input[name^="Medications"][name$="MedicamentProduitNat"]');
    medicationRadioButtons.forEach(radio => {
        radio.removeEventListener('change', handleMedicationChange); // Remove previous handler if exists
        radio.addEventListener('change', handleMedicationChange); // Add new handler
    });
}

function handleMedicationChange() {
    // Get the card containing the changed radio button
    const card = this.closest('.card-body');
    const posologieLabel = card.querySelector('label[id^="posologieLabel"]');
    const raisonGroup = card.querySelector('[id^="raisonGroup"]');
    if (this.value === "Medicament") {
        posologieLabel.textContent = 'Posologie';
        raisonGroup.style.display = 'block';
    } else if (this.value === "Produit Naturel") {
        posologieLabel.textContent = 'Quantité';
        raisonGroup.style.display = 'none';
    }
}



//Dynamic fields//


function addDynamicField(type) {
    let containerId, fieldHtml;
    switch (type) {
        case 'address':
            containerId = 'address-fields';
            fieldHtml = `
                    <div class="card mb-3">
                        <div class="card-body">
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Ville</label>
                                <div class="col-md-10">
                                    <input asp-for="Addresses[${addressIndex}].Ville" type="text" name="Addresses[${addressIndex}].Ville" class="form-control" />
                                </div>
                            </div>
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Adresse Primaire</label>
                                <div class="col-md-10">
                                    <input asp-for="Addresses[${addressIndex}].AdressePrimaire" type="checkbox" name="Addresses[${addressIndex}].AdressePrimaire" class="address-primary" value="true" />
                                    <span style="display: none; color: red;">Adresse primaire déja sélectionnée</span>
                                </div>
                            </div>
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Num Civic</label>
                                <div class="col-md-10">
                                    <input asp-for="Addresses[${addressIndex}].NumCivic" type="text" name="Addresses[${addressIndex}].NumCivic" class="form-control" />
                                </div>
                            </div>
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Rue</label>
                                <div class="col-md-10">
                                    <input asp-for="Addresses[${addressIndex}].Rue" type="text" name="Addresses[${addressIndex}].Rue" class="form-control" />
                                </div>
                            </div>
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Appartement</label>
                                <div class="col-md-10">
                                    <input asp-for="Addresses[${addressIndex}].Appartement" type="text" name="Addresses[${addressIndex}].Appartement" class="form-control" />
                                </div>
                            </div>
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Code Postal</label>
                                <div class="col-md-10">
                                    <input asp-for="Addresses[${addressIndex}].CodePostal" type="text" name="Addresses[${addressIndex}].CodePostal" class="form-control" />
                                </div>
                            </div>
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Téléphone filaire</label>
                                <div class="col-md-10">
                                    <input asp-for="Addresses[${addressIndex}].TelphoneAdresse" type="text" name="Addresses[${addressIndex}].TelphoneAdresse" class="form-control" id="telephoneCell" />
                                </div>
                            </div>
                            <div class="card-footer d-flex justify-content-between align-items-center">
                                <button type="button" class=" col-1 btn btn-danger btn-sm ms-auto remove-field">Remove</button>
                            </div>
                        </div>
                    </div>`;
            addressIndex++;
            break;
        case 'allergy':
            containerId = 'allergy-fields';
            fieldHtml = `
                    <div class="card mb-3">
                        <div class="card-body">
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Allergie ou Intolérance</label>
                                <div class="col-md-10">
                                    <div class="d-flex flex-column flex-md-row align-items-start">
                                        <div class="me-3">
                                            <input asp-for="Allergies[${allergyIndex}].AllergieIntolerance" type="radio" name="Allergies[${allergyIndex}].AllergieIntolerance" value="Allergie" class="form-check-input allergy-intolerance-radio" id="allergie-${allergyIndex}" />
                                            <label class="form-check-label" for="allergie-${allergyIndex}">Allergie</label>
                                        </div>
                                        <div>
                                            <input asp-for="Allergies[${allergyIndex}].AllergieIntolerance" type="radio" name="Allergies[${allergyIndex}].AllergieIntolerance" value="Intolerance" class="form-check-input allergy-intolerance-radio" id="intolerance-${allergyIndex}" />
                                            <label class="form-check-label" for="intolerance-${allergyIndex}">Intolérance</label>
                                        </div>
                                    </div>
                                </div>
                            </div>
                                <div class="form-group row severity-field" style="display: none;">
                                    <label class="col-md-2 col-form-label">Sévérité</label>
                                    <div class="col-md-10">
                                        <div class="d-flex flex-column flex-md-row align-items-start">
                                            <div class="me-3">
                                                <input asp-for="Allergies[${allergyIndex}].Gravite" type="radio" name="Allergies[${allergyIndex}].Gravite" value="Bénigne" class="form-check-input" id="benigne-${allergyIndex}" />
                                                <label class="form-check-label" for="benigne-${allergyIndex}">Bénigne</label>
                                            </div>
                                            <div>
                                                <input asp-for="Allergies[${allergyIndex}].Gravite" type="radio" name="Allergies[${allergyIndex}].Gravite" value="Grave/Mortelle" class="form-check-input" id="grave-${allergyIndex}" />
                                                <label class="form-check-label" for="grave-${allergyIndex}">Grave/Mortelle</label>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Produit</label>
                                <div class="col-md-10">
                                    <input asp-for="Allergies[${allergyIndex}].Produit" type="text" name="Allergies[${allergyIndex}].Produit" class="form-control" />
                                </div>
                            </div>
                            <div class="card-footer d-flex justify-content-between align-items-center">
                                <button type="button" class=" col-1 btn btn-danger btn-sm ms-auto remove-field">Remove</button>
                            </div>
                        </div>
                    </div>`;
            allergyIndex++;
            break;
        case 'handicap':
            containerId = 'handicap-fields';
            fieldHtml = `
                    <div class="card mb-3">
                        <div class="card-body">
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Type</label>
                                <div class="col-md-10 d-flex flex-wrap">
                                    <!-- Flex container for horizontal alignment -->
                                    <div class="radio-container">
                                        <input asp-for="Handicaps[${handicapIndex}].Type" type="radio" id="autisme${handicapIndex}" name="Handicaps[${handicapIndex}].Type" value="Autisme" class="form-check-input" />
                                        <label for="autisme${handicapIndex}" class="form-check-label">Autisme</label>
                                    </div>
                                    <div class="radio-container">
                                        <input asp-for="Handicaps[${handicapIndex}].Type" type="radio" id="mobilite-reduite${handicapIndex}" name="Handicaps[${handicapIndex}].Type" value="Mobilité réduite" class="form-check-input" />
                                        <label for="mobilite-reduite${handicapIndex}" class="form-check-label">Mobilité réduite</label>
                                    </div>
                                    <div class="radio-container">
                                        <input asp-for="Handicaps[${handicapIndex}].Type" type="radio" id="deficience-auditive${handicapIndex}" name="Handicaps[${handicapIndex}].Type" value="Déficience auditive" class="form-check-input" />
                                        <label for="deficience-auditive${handicapIndex}" class="form-check-label">Déficience auditive</label>
                                    </div>
                                    <div class="radio-container">
                                        <input asp-for="Handicaps[${handicapIndex}].Type" type="radio" id="deficience-visuelle${handicapIndex}" name="Handicaps[${handicapIndex}].Type" value="Déficience visuelle" class="form-check-input" />
                                        <label for="deficience-visuelle${handicapIndex}" class="form-check-label">Déficience visuelle</label>
                                    </div>
                                    <div class="radio-container">
                                        <input asp-for="Handicaps[${handicapIndex}].Type" type="radio" id="autre${handicapIndex}" name="Handicaps[${handicapIndex}].Type" value="Autre" class="form-check-input" />
                                        <label for="autre${handicapIndex}" class="form-check-label">Autre</label>
                                    </div>
                                </div>
                            </div>
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Définition</label>
                                <div class="col-md-10">
                                    <input asp-for="Handicaps[${handicapIndex}].Definition" type="text" name="Handicaps[${handicapIndex}].Definition" class="form-control" />
                                </div>
                            </div>
                            <div class="card-footer d-flex justify-content-between align-items-center">
                                <button type="button" class=" col-1 btn btn-danger btn-sm ms-auto remove-field">Remove</button>
                            </div>
                        </div>
                    </div>`;
            handicapIndex++;
            break;
        case 'medication':
            containerId = 'medication-fields';
            fieldHtml = `
            <div class="card mb-3">
                <div class="card-body">
                    <div class="form-group row">
                        <label class="col-md-2 col-form-label">Type</label>
                        <div class="col-md-10">
                            <div class="form-check form-check-inline">
                                <input asp-for="Medications[${medicationIndex}].MedicamentProduitNat" class="form-check-input" type="radio" id="medicament${medicationIndex}" name="Medications[${medicationIndex}].MedicamentProduitNat" value="Medicament" />
                                <label class="form-check-label" for="medicament${medicationIndex}">Médicament</label>
                            </div>
                            <div class="form-check form-check-inline">
                                <input asp-for="Medications[${medicationIndex}].MedicamentProduitNat" class="form-check-input" type="radio" id="produitNaturel${medicationIndex}" name="Medications[${medicationIndex}].MedicamentProduitNat" value="Produit Naturel" />
                                <label class="form-check-label" for="produitNaturel${medicationIndex}">Produit Naturel</label>
                            </div>
                        </div>
                    </div>

                    <div class="form-group row">
                        <label class="col-md-2 col-form-label">Nom</label>
                        <div class="col-md-10">
                            <input asp-for="Medications[${medicationIndex}].Nom" type="text" name="Medications[${medicationIndex}].Nom" class="form-control" />
                        </div>
                    </div>

                    <div class="form-group row">
                        <label class="col-md-2 col-form-label" id="posologieLabel${medicationIndex}">Posologie</label>
                        <div class="col-md-10">
                            <input asp-for="Medications[${medicationIndex}].Posologie" type="text" name="Medications[${medicationIndex}].Posologie" class="form-control" />
                        </div>
                    </div>

                    <div class="form-group row" id="raisonGroup${medicationIndex}" style="display: none;">
                        <label class="col-md-2 col-form-label">Raison</label>
                        <div class="col-md-10">
                            <input asp-for="Medications[${medicationIndex}].Raison" type="text" name="Medications[${medicationIndex}].Raison" class="form-control" />
                        </div>
                    </div>

                    <!-- Remove Button -->
                    <div class="card-footer d-flex justify-content-between align-items-center">
                        <button type="button" class="col-1 btn btn-danger btn-sm ms-auto remove-field">Remove</button>
                    </div>
                </div>
            </div>`;
            medicationIndex++;
            break;
    }

    document.getElementById(containerId).insertAdjacentHTML('beforeend', fieldHtml);

    handlePrimaryAddressCheckbox();
    attachAllergyEventListeners();
    attachMedicationEventListeners(); // Attach event listeners for new medication fields

    container.querySelector('.card:last-child').offsetHeight;
}

// Initialize by showing the first section
showSection('userinfo');
