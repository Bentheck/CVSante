const sections = ['userinfo', 'address', 'antecedent', 'allergy', 'handicap', 'medication'];
let currentSectionIndex = 0;

// Initialize telephone input mask
$(document).ready(function () {
    $('#telephoneCell').inputmask('999-999-9999');
});

// Show specific section and update navigation
function showSection(sectionId) {
    sections.forEach((section, index) => {
        document.getElementById(section).style.display = section === sectionId ? 'block' : 'none';
        if (section === sectionId) {
            currentSectionIndex = index;
        }
    });
    updateNavPills();
    updateNavigationButtons();
}

// Navigate to the previous section
function previousSection() {
    if (currentSectionIndex > 0) {
        currentSectionIndex--;
        showSection(sections[currentSectionIndex]);
    }
}

// Navigate to the next section
function nextSection() {
    if (currentSectionIndex < sections.length - 1) {
        currentSectionIndex++;
        showSection(sections[currentSectionIndex]);
    }
}

// Update navigation pills to reflect the current section
function updateNavPills() {
    sections.forEach((section, index) => {
        const navLink = document.getElementById(`nav-${section}`);
        navLink.classList.toggle('active', index === currentSectionIndex);
    });
}

// Enable/disable navigation buttons based on the current section
function updateNavigationButtons() {
    document.getElementById('prev-btn').disabled = currentSectionIndex === 0;
    document.getElementById('next-btn').disabled = currentSectionIndex === sections.length - 1;
}

// Manage "other" checkbox in antecedents
document.addEventListener('DOMContentLoaded', () => {
    const toggleManualEntry = () => {
        const otherCheckbox = document.querySelector('input[name="antecedents"][value="Autre"]');
        const manualEntry = document.getElementById('manual-entry');
        manualEntry.style.display = otherCheckbox.checked ? 'block' : 'none';
        updateAntecedentsString();
    };

    const updateAntecedentsString = () => {
        const checkedCheckboxes = document.querySelectorAll('input[name="antecedents"]:checked');
        const manualEntryField = document.getElementById('autre-details');
        let antecedentsText = Array.from(checkedCheckboxes).map(checkbox => checkbox.value);

        if (document.querySelector('input[name="antecedents"][value="Autre"]').checked) {
            const manualEntryText = manualEntryField.value.trim();
            if (manualEntryText) {
                antecedentsText.push(manualEntryText);
            }
        }

        document.getElementById('antecedent-aggregate').value = antecedentsText.join('/');
    };

    document.querySelectorAll('input[name="antecedents"]').forEach(checkbox => {
        checkbox.addEventListener('change', () => {
            toggleManualEntry();
            updateAntecedentsString();
        });
    });

    document.getElementById('autre-details').addEventListener('input', updateAntecedentsString);

    document.querySelector('form').addEventListener('submit', (event) => {
        updateAntecedentsString();
    });

    toggleManualEntry();
});

showSection(sections[0]);

// Ensure only one primary address checkbox is selected
function handlePrimaryAddressCheckbox() {
    const allPrimaryCheckboxes = document.querySelectorAll('input.address-primary');
    let anyChecked = Array.from(allPrimaryCheckboxes).some(box => box.checked);

    allPrimaryCheckboxes.forEach(box => {
        box.disabled = anyChecked && !box.checked;
        box.closest('.card-body').querySelector('span').style.display = box.checked ? 'none' : (anyChecked ? 'inline' : 'none');
    });
}

document.addEventListener('click', function (event) {
    if (event.target.matches('input.address-primary')) {
        handlePrimaryAddressCheckbox();
    }
});

// Manage allergy severity display based on the selection
function attachAllergyEventListeners() {
    const handleAllergyChange = function () {
        const severityField = this.closest('.card-body').querySelector('.severity-field');
        severityField.style.display = this.value === 'Allergie' ? 'block' : 'none';
    };

    document.querySelectorAll('.allergy-intolerance-radio').forEach(radio => {
        radio.addEventListener('change', handleAllergyChange);
    });
}

// Remove dynamic field
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

// Update sex field based on the selection
$(document).ready(function () {
    const updateSexeField = () => {
        let selectedValue = $('input[name="UserInfo.Sexe"]:checked').val();
        if (selectedValue === "Autre") {
            $('#autrePrecisionGroup').show();
            let autrePrecision = $('#autrePrecision').val();
            $('#hiddenSexe').val('Autre: ' + autrePrecision);
        } else {
            $('#autrePrecisionGroup').hide();
            $('#hiddenSexe').val(selectedValue);
        }
    };

    $('input[name="UserInfo.Sexe"]').change(updateSexeField);
    updateSexeField();
});

// Attach medication event listeners and handle medication type changes
function attachMedicationEventListeners() {
    const handleMedicationChange = function () {
        const card = this.closest('.card-body');
        const posologieLabel = card.querySelector('label[id^="posologieLabel"]');
        const raisonGroup = card.querySelector('[id^="raisonGroup"]');
        posologieLabel.textContent = this.value === "Medicament" ? 'Posologie' : 'Quantité';
        raisonGroup.style.display = this.value === "Medicament" ? 'block' : 'none';
    };

    document.querySelectorAll('.card:last-child input[name^="Medications"][name$="MedicamentProduitNat"]').forEach(radio => {
        radio.addEventListener('change', handleMedicationChange);
    });
}

// Add dynamic fields based on the type
function addDynamicField(type) {
    let containerId, fieldHtml, index;

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
            index = document.querySelectorAll(`#${containerId} .card`).length;
            fieldHtml = `
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Produit</label>
                            <div class="col-md-10">
                                <input type="text" name="Allergies[${index}].Produit" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Réaction</label>
                            <div class="col-md-10">
                                <input type="text" name="Allergies[${index}].Reaction" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Type</label>
                            <div class="col-md-10">
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input allergy-intolerance-radio" type="radio" name="Allergies[${index}].Type" value="Allergie" />
                                    <label class="form-check-label">Allergie</label>
                                </div>
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input allergy-intolerance-radio" type="radio" name="Allergies[${index}].Type" value="Intolérance" />
                                    <label class="form-check-label">Intolérance</label>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row severity-field" style="display: none;">
                            <label class="col-md-2 col-form-label">Sévérité</label>
                            <div class="col-md-10">
                                <select name="Allergies[${index}].Sévérité" class="form-control">
                                    <option value="Mineure">Mineure</option>
                                    <option value="Moyenne">Moyenne</option>
                                    <option value="Sévère">Sévère</option>
                                </select>
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
            index = document.querySelectorAll(`#${containerId} .card`).length;
            fieldHtml = `
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="form-group row">
                        <label class="col-md-2 col-form-label">Type</label>
                            <div class="col-md-10">
                                <input type="text" name="Handicaps[${index}].Type" class="form-control" />
                            </div>
                        </div>
                            <div class="form-group row">
                                <label class="col-md-2 col-form-label">Description</label>
                                <div class="col-md-10">
                                    <input type="text" name="Handicaps[${index}].Description" class="form-control" />
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
                            <label class="col-md-2 col-form-label">Produit</label>
                            <div class="col-md-10">
                                <input type="text" name="Medications[${index}].Produit" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label">Type</label>
                            <div class="col-md-10">
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="Medications[${index}].Type" value="Medicament" />
                                    <label class="form-check-label">Médicament</label>
                                </div>
                                <div class="form-check form-check-inline">
                                    <input class="form-check-input" type="radio" name="Medications[${index}].Type" value="Produit Naturel" />
                                    <label class="form-check-label">Produit Naturel</label>
                                </div>
                            </div>
                        </div>
                        <div class="form-group row">
                            <label class="col-md-2 col-form-label" id="posologieLabel_${index}">Posologie</label>
                            <div class="col-md-10">
                                <input type="text" name="Medications[${index}].Posologie" class="form-control" />
                            </div>
                        </div>
                        <div class="form-group row" id="raisonGroup_${index}">
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
        default:
            return;
    }

    const container = document.getElementById(containerId);
    container.insertAdjacentHTML('beforeend', fieldHtml);

    switch (type) {
        case 'allergy':
            attachAllergyEventListeners();
            break;
        case 'medication':
            attachMedicationEventListeners();
            break;
    }
}
