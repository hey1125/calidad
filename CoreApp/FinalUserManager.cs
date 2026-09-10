using Azure;
using Azure.AI.Vision.Face;
using DataAccess.CRUD;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using CoreApp.Classes;

namespace CoreApp
{
    public class FinalUserManager : BaseManager
    {

        public void Create(FinalUser finalUser, TwilioSettings twilioSettings = null, SendGridSettings sendGridSettings = null)
        {
            try
            {
                // Validations
                ValidateNationalId(finalUser.NationalId);
                ValidateName(finalUser.FirstName);
                ValidateLastName1(finalUser.LastName1);
                ValidateLastName2(finalUser.LastName2);
                ValidatePhone(finalUser.Phone);
                ValidateEmail(finalUser.Email);
                ValidatePassword(finalUser.PasswordHash);
                ValidateDateOfBirth(finalUser.DateOfBirth);
                ValidateLocation(finalUser.LocationLat, finalUser.LocationLng);
                finalUser.IdCardFrontPhotoUrl ??= "";
                finalUser.IdCardBackPhotoUrl ??= "";
                finalUser.FacialVerificationPassed = false;

                var fCrud = new FinalUserCrudFactory();
                // Check if email already exists
                var existing = fCrud.RetrieveByEmail<FinalUser>(finalUser.Email);
                if (existing != null)
                    throw new Exception("Este correo electrónico ya está registrado");

                finalUser.Status = "Pending"; // "Pendiente"
                fCrud.Create(finalUser);
                finalUser.Id = fCrud.RetrieveByEmail<FinalUser>(finalUser.Email).Id;

            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear el usuario final: {ex.Message}");
            }
        }
        #region Create Validation Methods

        private void ValidateNationalId(string id)
        {
            var fCrud = new FinalUserCrudFactory();
            var existing = fCrud.RetrieveByNationalId<FinalUser>(id); ;
            if (existing != null)
                throw new Exception("Ya hay un usuario registrado con ese numero de cédula.");
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception("La cédula es obligatoria.");
            if (id.Length != 9 || !id.All(char.IsDigit))
                throw new Exception("La cédula debe tener 9 dígitos numéricos.");
        }

        private void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("El nombre es obligatorio.");
            if (name.Length > 40)
                throw new Exception("El nombre no debe superar los 40 caracteres.");
        }

        private void ValidateLastName1(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new Exception("El primer apellido es obligatorio.");
            if (lastName.Length > 40)
                throw new Exception("El primer apellido no debe superar los 40 caracteres.");
        }

        private void ValidateLastName2(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new Exception("El segundo apellido es obligatorio.");
            if (lastName.Length > 40)
                throw new Exception("El segundo apellido no debe superar los 40 caracteres.");
        }

        private void ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                throw new Exception("El teléfono es obligatorio.");
            if (phone.Length != 8 || !phone.All(char.IsDigit))
                throw new Exception("El teléfono debe tener 8 dígitos numéricos.");
        }

        private void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("El correo electrónico es obligatorio.");
            if (email.Length > 250)
                throw new Exception("El correo no debe superar los 250 caracteres.");
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, pattern))
                throw new Exception("Ingrese un correo válido.");
        }

        private void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("La contraseña es obligatoria.");
            if (password.Length < 8)
                throw new Exception("La contraseña debe tener al menos 8 caracteres.");
            var pattern = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$";
            if (!Regex.IsMatch(password, pattern))
                throw new Exception("Debe contener letras, números y al menos un carácter especial.");
        }

        private void ValidateDateOfBirth(DateTime dob)
        {
            if (dob == default)
                throw new Exception("La fecha de nacimiento es obligatoria.");
            if (dob > DateTime.Today)
                throw new Exception("Ingrese una fecha válida.");
            var age = DateTime.Today.Year - dob.Year;
            if (dob.AddYears(age) > DateTime.Today) age--;
            if (age < 18)
                throw new Exception("Debe ser mayor de edad.");
        }

        private void ValidateLocation(decimal lat, decimal lng)
        {
            if (lat == 0m && lng == 0m)
                throw new Exception("La ubicación es obligatoria.");
            if (lat < -90m || lat > 90m || lng < -180m || lng > 180m)
                throw new Exception("Seleccione una ubicación válida en el mapa.");
        }

        private void ValidateIdCardPhotos(string frontUrl, string backUrl)
        {
            if (string.IsNullOrWhiteSpace(frontUrl) || string.IsNullOrWhiteSpace(backUrl))
                throw new Exception("Debe subir fotos del frente y reverso de la cédula.");
            ValidateImageExtension(frontUrl);
            ValidateImageExtension(backUrl);
        }

        private void ValidateImageExtension(string url)
        {
            var ext = Path.GetExtension(url)?.ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                throw new Exception("Cada imagen debe ser JPG o PNG.");
        }
        #endregion

        public FinalUser Update(FinalUser finalUser)
        {
            try
            {
                if (finalUser.Id <= 0)
                    throw new Exception("El ID del usuario es obligatorio para la actualización.");

                var fCrud = new FinalUserCrudFactory();
                var existing = fCrud.RetrieveById<FinalUser>(finalUser.Id);
                if (existing == null)
                    throw new Exception("No se encontró el usuario que se desea actualizar.");

                fCrud.Update(finalUser);
                return finalUser;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar el usuario final: {ex.Message}");
            }
        }


        public FinalUser RetrieveByNationalId(string nationalId)
        {
            var fCrud = new FinalUserCrudFactory();
            var user = fCrud.RetrieveByNationalId<FinalUser>(nationalId);
            if (user == null)
                throw new Exception("Usuario no encontrado.");
            return user;
        }

        async public Task<bool> ValidateFinalUser(string nationalId, string otp, string keyId, string keyAccess)
        {
            var finalUser = RetrieveByNationalId(nationalId);
            // El registro sin fotografías se activa únicamente con el OTP.
            if (string.IsNullOrWhiteSpace(finalUser.IdCardFrontPhotoUrl))
            {
                if (!string.Equals(otp, OTPVerificationManager.FixedCode, StringComparison.Ordinal))
                    return false;

                finalUser.Status = "Active";
                finalUser.FacialVerificationPassed = false;
                new FinalUserCrudFactory().Update(finalUser);
                return true;
            }
            try
            {
                // Realizar verificación facial
                var finalUserCrud = new FinalUserCrudFactory();
                bool isFacialVerificationPassed = false;

                var rekognitionClient = new AmazonRekognitionClient(
                    keyId, 
                    keyAccess,  
                    RegionEndpoint.USEast1 // Change if using another region
                );

                using var httpClient = new HttpClient();

                byte[] idCardBytes = await httpClient.GetByteArrayAsync(finalUser.IdCardFrontPhotoUrl);
                byte[] selfieBytes = await httpClient.GetByteArrayAsync(finalUser.SelfieUrl);

                // Update the code to explicitly use the Amazon Rekognition Image class
                var compareRequest = new CompareFacesRequest
                {
                    SourceImage = new Amazon.Rekognition.Model.Image
                    {
                        Bytes = new MemoryStream(idCardBytes)
                    },
                    TargetImage = new Amazon.Rekognition.Model.Image
                    {
                        Bytes = new MemoryStream(selfieBytes)
                    },
                    SimilarityThreshold = 70f // Adjust this value as needed
                };

                var compareResponse = await rekognitionClient.CompareFacesAsync(compareRequest);

                if (compareResponse.FaceMatches.Count > 0)
                {
                    var similarity = compareResponse.FaceMatches[0].Similarity;
                    finalUser.FacialVerificationPassed = true;
                    // Update the user with the verification result
                    var fum = new FinalUserCrudFactory();
                    fum.Update(finalUser);
                    Console.WriteLine($"Facial verification passed with {similarity}% similarity.");
                    isFacialVerificationPassed = true;
                }
                else
                {
                    throw new Exception("❌ Facial verification failed: no matches found between the selfie and the ID card.");
                }

                if (isFacialVerificationPassed)
                {
                    // Validación OTP
                    var otpCrud = new OTPVerificationCrudFactory();
                    if (string.Equals(otp, OTPVerificationManager.FixedCode, StringComparison.Ordinal))
                    {
                        var fum = new FinalUserCrudFactory();
                        finalUser.Status = "Active";
                        fum.Update(finalUser);
                        var otpVerified = otpCrud.RetrieveByUserId<OTPVerification>(finalUser.Id);
                        if (otpVerified != null)
                        {
                            otpVerified.IsVerified = true;
                            otpCrud.Update(otpVerified);
                        }
                        Console.WriteLine($"OTP {otp} is valid for user {finalUser.Id}.");

                        return true;
                    }
                }
                else
                {
                    return false; // Facial verification failed, do not proceed with OTP validation
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error",ex);
            }

            // Ensure all code paths return a value
            return false;
        }
    }
}
