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
        .map(checkbox => checkbox.value);

    if (document.querySelector('input[name="antecedents"][value="Autre"]').checked) {
        const manualEntryText = manualEntryField.value.trim();
        if (manualEntryText) {
            antecedentsText.push(manualEntryText);
        }
    }

    document.getElementById('antecedent-aggregate').value = antecedentsText.join('/');
}

// Function to be called on form submission
function handleSubmit(event) {
    updateAntecedentsString(); // Ensure antecedents are updated before form submission
    // Optionally: You can add validation or other submission logic here
}

showSection(sections[0]);

document.querySelectorAll('input[name="antecedents"]').forEach(checkbox => {
    checkbox.addEventListener('change', () => {
        toggleManualEntry();
        updateAntecedentsString();
    });
});

document.getElementById('autre-details').addEventListener('input', updateAntecedentsString);

// Add event listener to the form
document.querySelector('form').addEventListener('submit', handleSubmit);



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
        radio.removeEventListener('change', handleMedicationChange);
        radio.addEventListener('change', handleMedicationChange);
    });
}

function handleMedicationChange() {
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


function addDynamicField(type) {
    let containerId;
    let fieldHtml;
    let index;

    switch (type) {
        case 'address':
            containerId = 'address-fields';
            index = document.querySelectorAll(`#${containerId} .card`).length;
            fieldHtml = `
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Ville</label>
                            <div class="col-md-10">
                                <input type="text" name="Addresses[${index}].Ville" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Adresse Primaire</label>
                            <div class="col-md-10">
                                <input type="checkbox" name="Addresses[${index}].AdressePrimaire" class="address-primary" value="true" />
                                <span style="display: none; color: red;">Adresse primaire déjà sélectionnée</span>
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Num Civic</label>
                            <div class="col-md-10">
                                <input type="text" name="Addresses[${index}].NumCivic" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Rue</label>
                            <div class="col-md-10">
                                <input type="text" name="Addresses[${index}].Rue" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Appartement</label>
                            <div class="col-md-10">
                                <input type="text" name="Addresses[${index}].Appartement" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Code Postal</label>
                            <div class="col-md-10">
                                <input type="text" name="Addresses[${index}].CodePostal" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Téléphone filaire</label>
                            <div class="col-md-10">
                                <input type="text" name="Addresses[${index}].TelphoneAdresse" class="form-control" />
                            </div>
                        </div>
                        <div class="card-footer d-flex justify-content-between align-items-center">
                            <button type="button" class="col-1 btn btn-danger btn-sm ms-auto remove-field">Remove</button>
                        </div>
                    </div>
                </div>`;
            break;
        case 'allergy':
            containerId = 'allergy-fields';
            index = document.querySelectorAll(`#${containerId} .card`).length; // Adjust index based on current count
            fieldHtml = `
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Allergie ou Intolérance</label>
                            <div class="col-md-10">
                                <div class="d-flex flex-column flex-md-row align-items-start">
                                    <div class="me-3">
                                        <input type="radio" name="Allergies[${index}].AllergieIntolerance" value="Allergie" class="form-check-input allergy-intolerance-radio" id="allergie-${index}" />
                                        <label class="form-check-label" for="allergie-${index}">Allergie</label>
                                    </div>
                                    <div>
                                        <input type="radio" name="Allergies[${index}].AllergieIntolerance" value="Intolerance" class="form-check-input allergy-intolerance-radio" id="intolerance-${index}" />
                                        <label class="form-check-label" for="intolerance-${index}">Intolérance</label>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row severity-field" style="display: none;">
                            <label class="col-md-2 col-form-label">Sévérité</label>
                            <div class="col-md-10">
                                <div class="d-flex flex-column flex-md-row align-items-start">
                                    <div class="me-3">
                                        <input type="radio" name="Allergies[${index}].Gravite" value="Bénigne" class="form-check-input" id="benigne-${index}" />
                                        <label class="form-check-label" for="benigne-${index}">Bénigne</label>
                                    </div>
                                    <div>
                                        <input type="radio" name="Allergies[${index}].Gravite" value="Grave/Mortelle" class="form-check-input" id="grave-${index}" />
                                        <label class="form-check-label" for="grave-${index}">Grave/Mortelle</label>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Produit</label>
                            <div class="col-md-10">
                                <input type="text" name="Allergies[${index}].Produit" class="form-control" />
                            </div>
                        </div>
                        <div class="card-footer d-flex justify-content-between align-items-center">
                            <button type="button" class="col-1 btn btn-danger btn-sm ms-auto remove-field">Remove</button>
                        </div>
                    </div>
                </div>`;
            break;
        case 'handicap':
            containerId = 'handicap-fields';
            index = document.querySelectorAll(`#${containerId} .card`).length; // Adjust index based on current count
            fieldHtml = `
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Type</label>
                            <div class="col-md-10">
                                <div class="d-flex flex-column flex-md-row align-items-start">
                                    <div class="me-3">
                                        <input type="radio" name="Handicaps[${index}].Type" value="Autisme" class="form-check-input" id="autisme-${index}" />
                                        <label class="form-check-label" for="autisme-${index}">Autisme</label>
                                    </div>
                                    <div class="me-3">
                                        <input type="radio" name="Handicaps[${index}].Type" value="Mobilité réduite" class="form-check-input" id="mobilite-reduite-${index}" />
                                        <label class="form-check-label" for="mobilite-reduite-${index}">Mobilité réduite</label>
                                    </div>
                                    <div class="me-3">
                                        <input type="radio" name="Handicaps[${index}].Type" value="Déficience auditive" class="form-check-input" id="deficience-auditive-${index}" />
                                        <label class="form-check-label" for="deficience-auditive-${index}">Déficience auditive</label>
                                    </div>
                                    <div class="me-3">
                                        <input type="radio" name="Handicaps[${index}].Type" value="Déficience visuelle" class="form-check-input" id="deficience-visuelle-${index}" />
                                        <label class="form-check-label" for="deficience-visuelle-${index}">Déficience visuelle</label>
                                    </div>
                                    <div>
                                        <input type="radio" name="Handicaps[${index}].Type" value="Autre" class="form-check-input" id="autre-${index}" />
                                        <label class="form-check-label" for="autre-${index}">Autre</label>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Handicap</label>
                            <div class="col-md-10">
                                <input type="text" name="Handicaps[${index}].Definition" class="form-control" />
                            </div>
                        </div>
                        <div class="card-footer d-flex justify-content-between align-items-center">
                            <button type="button" class="col-1 btn btn-danger btn-sm ms-auto remove-field">Remove</button>
                        </div>
                    </div>
                </div>`;
            break;
        case 'medication':
            containerId = 'medication-fields';
            index = document.querySelectorAll(`#${containerId} .card`).length;
            fieldHtml = `
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Type</label>
                            <div class="col-md-10">
                                <div class="d-flex flex-column flex-md-row align-items-start">
                                    <div class="me-3">
                                        <input type="radio" name="Medications[${index}].MedicamentProduitNat" value="Medicament" class="form-check-input" id="medicament-${index}" />
                                        <label class="form-check-label" for="medicament-${index}">Médicament</label>
                                    </div>
                                    <div>
                                        <input type="radio" name="Medications[${index}].MedicamentProduitNat" value="Produit Naturel" class="form-check-input" id="produitNaturel-${index}" />
                                        <label class="form-check-label" for="produitNaturel-${index}">Produit Naturel</label>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Nom</label>
                            <div class="col-md-10">
                                <input type="text" name="Medications[${index}].Nom" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label" id="posologieLabel-${index}">Posologie</label>
                            <div class="col-md-10">
                                <input type="text" name="Medications[${index}].Posologie" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row" id="raisonGroup-${index}" style="display: none;">
                            <label class="col-md-2 col-form-label">Raison</label>
                            <div class="col-md-10">
                                <input type="text" name="Medications[${index}].Raison" class="form-control" />
                            </div>
                        </div>
                        <div class="card-footer d-flex justify-content-between align-items-center">
                            <button type="button" class="col-1 btn btn-danger btn-sm ms-auto remove-field">Remove</button>
                        </div>
                    </div>
                </div>`;
            break;
    }

    const container = document.getElementById(containerId);
    container.insertAdjacentHTML('beforeend', fieldHtml);

    switch (type) {
        case 'address':
            addressIndex++;
            handlePrimaryAddressCheckbox();
            break;
        case 'allergy':
            allergyIndex++;
            attachAllergyEventListeners();
            break;
        case 'handicap':
            handicapIndex++;
            break;
        case 'medication':
            medicationIndex++;
            attachMedicationEventListeners();
            break;
    }

    // Event listener for address primary checkbox
    const checkboxes = container.querySelectorAll('.address-primary');
    checkboxes.forEach((checkbox) => {
        checkbox.addEventListener('change', () => {
            checkboxes.forEach((cb) => {
                if (cb !== checkbox) {
                    cb.parentElement.querySelector('span').style.display = checkbox.checked ? 'inline' : 'none';
                }
            });
        });
    });
}

function attachRemoveEventListeners() {
    const removeButtons = document.querySelectorAll('.remove-field');
    removeButtons.forEach(button => {
        button.removeEventListener('click', handleRemoveField);
        button.addEventListener('click', handleRemoveField);
    });
}

function handleRemoveField() {
    this.closest('.card').remove();
}

document.addEventListener('DOMContentLoaded', function () {
    attachRemoveEventListeners();
    attachMedicationEventListeners();
    attachAllergyEventListeners();
});

showSection('userinfo');