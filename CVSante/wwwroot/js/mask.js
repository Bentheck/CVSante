function applyMasks() {
    Inputmask({ "mask": "999-999-9999" }).mask(document.querySelectorAll('.phone-mask'));
    Inputmask({ "mask": "A9A 9A9" }).mask(document.querySelectorAll('.CP-mask'));
    Inputmask({ "mask": "9[9][9]" + ' Kg' }).mask(document.querySelectorAll('.poid-mask'));
    Inputmask({ "mask": "9.9[9]" + ' Mètres' }).mask(document.querySelectorAll('.taille-mask'));
}

// Apply masks to existing fields on page load
document.addEventListener("DOMContentLoaded", function () {
    applyMasks();
});
