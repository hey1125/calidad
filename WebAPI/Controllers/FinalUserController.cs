using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using CoreApp;
using CoreApp.Classes;
using DataAccess.CRUD;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using WebAPI.Classes;
using static System.Net.WebRequestMethods;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinalUserController : ControllerBase
    {
        /*
         {
  "id": 0,
  "created": "2025-07-10T06:15:43.048Z",
  "updated": "2025-07-10T06:15:43.048Z",
  "nationalId": "117980889",
  "firstName": "string",
  "lastName1": "string",
  "lastName2": "string",
  "passwordHash": "P4ssrword!",
  "phone": "88888888",
  "email": "string@email.com",
  "dateOfBirth": "2000-07-10T06:15:43.048Z",
  "locationLat": 0.01,
  "locationLng": 0.01,
  "selfieUrl": "https://res.cloudinary.com/billetico/image/upload/v1752467717/Users/egxmj6i5nqt8xahpyprn.jpg",
  "facialVerificationPassed": true,
  "profilePhotoUrl": "string.png",
  "idCardFrontPhotoUrl": "https://res.cloudinary.com/billetico/image/upload/v1752467914/Users/sq4vufzkonyelmqqx0ui.jpg",
  "idCardBackPhotoUrl": "https://res.cloudinary.com/billetico/image/upload/v1752467717/Users/ferj2urr3rw6l9ellvqa.jpg",
  "status": "string"
}
        {
  "url": "https://res.cloudinary.com/billetico/image/upload/v1752467717/Users/egxmj6i5nqt8xahpyprn.jpg",
  "publicId": "Users/egxmj6i5nqt8xahpyprn"
}
    {
  "url": "https://res.cloudinary.com/billetico/image/upload/v1752467717/Users/ferj2urr3rw6l9ellvqa.jpg",
  "publicId": "Users/ferj2urr3rw6l9ellvqa"
}
        {
  "url": "https://res.cloudinary.com/billetico/image/upload/v1752467914/Users/sq4vufzkonyelmqqx0ui.jpg",
  "publicId": "Users/sq4vufzkonyelmqqx0ui"
}
         */
        [HttpPost]
        [Route("Create")]
        public ActionResult Create(FinalUser finalUser)
        {
            try
            {
                var fum = new FinalUserManager();
                fum.Create(finalUser);
                return Ok(finalUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("ValidateUser")]
        public async Task<IActionResult> ValidateUser([FromBody] OTPBody otpBody)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var keyId = config["AmazonRekognition:KeyID"];
            var keyAccess = config["AmazonRekognition:KeyAccess"];

            try
            {
                var fum = new FinalUserManager();
                bool verificationResult = await fum.ValidateFinalUser(
                    otpBody.nationalId, otpBody.otp, keyId, keyAccess);

                return Ok(verificationResult); 
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.ToString(), statusCode: 500);
            }
        }



        [HttpGet]
        [Route("RetrieveByNationalId")]
        public ActionResult RetrieveByNationalId(string nationalId)
        {
            try
            {
                var m = new FinalUserManager();
                var user = m.RetrieveByNationalId(nationalId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Usuario no encontrado"))
                    return NotFound("Usuario no encontrado.");
                return StatusCode(500, ex.Message);
            }
        }



        //[HttpGet]
        //[Route("RetriveAll")]
        //public ActionResult RetrieveAll()
        //{
        //    try
        //    {
        //        var fum = new FinalUserManager();
        //        var listResults = fum.();
        //        return Ok(listResults);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        //[HttpGet]
        //[Route("RetriveById")]
        //public ActionResult RetrieveById(User user)
        //{
        //    try
        //    {
        //        var um = new UserManager();
        //        var listResults = um.RetrieveAll();
        //        return Ok(listResults);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        //[HttpGet]
        //[Route("RetriveByEmail")]
        //public ActionResult RetrieveByEmail(User user)
        //{
        //    try
        //    {
        //        var um = new UserManager();
        //        var listResults = um.RetrieveByEmail(user);
        //        return Ok(listResults);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        //[HttpGet]
        //[Route("RetriveByUserCode")]
        //public ActionResult RetrieveByUserCode(User user)
        //{
        //    try
        //    {
        //        var um = new UserManager();
        //        var listResults = um.RetrieveByUserCode(user);
        //        return Ok(listResults);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        //[HttpDelete]
        //[Route("Delete")]
        //public ActionResult Delete(User user)
        //{
        //    try
        //    {
        //        var um = new UserManager();
        //        var deletedUser = um.Delete(user);
        //        return Ok(deletedUser);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        [HttpPost]
        [Route("Login")]
        public ActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
   
                // Validación normal para usuarios finales
                var crud = new FinalUserCrudFactory();
                var user = crud.RetrieveByEmail<FinalUser>(request.Email);

                if (user == null)
                    return Unauthorized("Correo no registrado.");

                if (user.PasswordHash != request.Password)
                    return Unauthorized("Contraseña incorrecta.");

                if (user.Status != "Active")
                    return Unauthorized("La cuenta no está activa.");

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("RecoverPassword")]
        public ActionResult RecoverPassword([FromBody] string email)
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
            SendGridSettings sendGridConfig = new SendGridSettings();
            sendGridConfig.ApiKey = config["SendGrid:ApiKey"];
            try
            {
                var crud = new FinalUserCrudFactory();
                var user = crud.RetrieveByEmail<FinalUser>(email);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                var emailManager = new EmailManager(sendGridConfig.ApiKey);
                emailManager.SendEmail(user.FirstName, user.Email, "Recuparación Contraseña", $"Su contraseña es: {user.PasswordHash}.");

                return Ok(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while recovering password: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("Update")]
        public ActionResult Update(FinalUser user)
        {
            try
            {
                var fum = new FinalUserManager();
                var updatedUser = fum.Update(user);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}