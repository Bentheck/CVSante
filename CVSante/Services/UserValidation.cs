using System.Collections.Generic;
using System.Linq;
using CVSante.Models;
using CVSante.ViewModels;

namespace CVSante.Services
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class UserValidation
    {
        // Validate that at least one address is marked as primary
        public ValidationResult ValidatePrimaryAddress(List<UserAdresse> addresses)
        {
            var result = new ValidationResult();

            if (addresses.Any(a => a.AdressePrimaire))
            {
                result.IsValid = true;
            }
            else
            {
                result.IsValid = false;
                result.Errors.Add("Une adresse principale doit être sélectionnée.");
            }

            return result;
        }

        // Validate that the phone number follows a specific format (e.g., French phone number format)
        private bool ValidatePhoneNumber(string phoneNumber)
        {
            // Example regex for a French phone number; adjust as needed
            var phoneNumberPattern = @"^\d{3}-\d{3}-\d{4}$";
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, phoneNumberPattern);
        }

        // Validate the user profile data
        public ValidationResult ValidateUserProfile(User user)
        {
            var result = new ValidationResult();

            if (user.UserInfo == null)
            {
                result.IsValid = false;
                result.Errors.Add("Les informations utilisateur sont requises.");
                return result;
            }

            // Check if the primary address is selected
            var addressValidation = ValidatePrimaryAddress(user.Addresses.ToList());
            if (!addressValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(addressValidation.Errors);
            }

            // Validate user info fields
            var userInfo = user.UserInfo;
            if (string.IsNullOrWhiteSpace(userInfo.Nom))
                result.Errors.Add("Le nom est requis.");
            if (string.IsNullOrWhiteSpace(userInfo.Prenom))
                result.Errors.Add("Le prénom est requis.");
            if (userInfo.DateNaissance == default)
                result.Errors.Add("La date de naissance est requise.");
            if (!string.IsNullOrWhiteSpace(userInfo.TelephoneCell) && !ValidatePhoneNumber(userInfo.TelephoneCell))
                result.Errors.Add("Le numéro de téléphone est invalide. Format accepté 999-999-9999");
            if (string.IsNullOrWhiteSpace(userInfo.Poids))
                result.Errors.Add("Le poids est requise.");
            if (string.IsNullOrWhiteSpace(userInfo.Taille))
                result.Errors.Add("La taille est requise.");

            if (result.Errors.Any())
            {
                result.IsValid = false;
            }

            // Validate sexe
            if (string.IsNullOrWhiteSpace(userInfo.Sexe))
                result.Errors.Add("Le sexe à la naissance est requis.");
            if (userInfo.Sexe == "Autre" && string.IsNullOrWhiteSpace(userInfo.AutrePrecision))
                result.Errors.Add("La précision est requise pour 'Autre'.");

            // Validate addresses
            foreach (var address in user.Addresses ?? new List<UserAdresse>())
            {
                if (string.IsNullOrWhiteSpace(address.NumCivic))
                    result.Errors.Add("Le numéro de civique est requis pour les adresses.");
                if (string.IsNullOrWhiteSpace(address.Rue))
                    result.Errors.Add("La rue est requise pour les adresses.");
                if (string.IsNullOrWhiteSpace(address.CodePostal))
                    result.Errors.Add("Le code postal est requis pour les adresses.");
                if (string.IsNullOrWhiteSpace(address.Ville))
                    result.Errors.Add("La ville est requise pour les adresses.");
                if (!string.IsNullOrWhiteSpace(address.TelphoneAdresse) && !ValidatePhoneNumber(address.TelphoneAdresse))
                    result.Errors.Add("Le téléphone de l'adresse est invalide.");
            }

            // Validate antecedents
            if (user.Antecedent == null)
            {
                result.Errors.Add("Les antécédents sont requis.");
            }
            else if (string.IsNullOrWhiteSpace(user.Antecedent.Antecedent))
            {
                result.Errors.Add("Les antécédents doivent être spécifiés.");
            }

            // Validate allergies
            if (user.Allergies != null)
            {
                foreach (var allergy in user.Allergies)
                {
                    if (string.IsNullOrWhiteSpace(allergy.Produit))
                        result.Errors.Add("Le produit d'allergie est requis.");
                }
            }

            // Validate handicaps
            if (user.Handicaps != null)
            {
                foreach (var handicap in user.Handicaps)
                {
                    if (string.IsNullOrWhiteSpace(handicap.Type))
                        result.Errors.Add("Le type de handicap est requis.");
                    if (handicap.Type == "Autre" && string.IsNullOrWhiteSpace(handicap.Definition))
                        result.Errors.Add("La définition est requise pour 'Autre'.");
                }
            }

            // Validate medications
            if (user.Medications != null)
            {
                foreach (var medication in user.Medications)
                {
                    if (string.IsNullOrWhiteSpace(medication.Nom))
                        result.Errors.Add("Le nom du médicament est requis.");
                    if (string.IsNullOrWhiteSpace(medication.Posologie))
                        result.Errors.Add("La posologie est requise.");
                    if (medication.MedicamentProduitNat == "Medicament" && string.IsNullOrWhiteSpace(medication.Raison))
                        result.Errors.Add("La raison est requise pour les médicaments.");
                }
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        // Validate other aspects as necessary
    }
}
