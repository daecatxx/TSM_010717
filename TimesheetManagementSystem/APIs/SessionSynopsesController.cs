using TimeSheetManagementSystem.Models;
//Need InternshipManagementSystem_V1.Data so that the .NET can find
//the ApplicationDbContext class.
using TimeSheetManagementSystem.Data;
using TimeSheetManagementSystem.Services;
using TimeSheetManagementSystem.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TimeSheetManagementSystem.APIs
{
    [Route("api/[controller]")]
    public class SessionSynopsesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;

        public ApplicationDbContext Database { get; }
        public IConfigurationRoot Configuration { get; }
        //Ah Tan notes:http://stackoverflow.com/questions/39174121/get-property-of-applicationuser-from-the-asp-net-core-identity
        //Create a Constructor, so that the .NET engine can pass in the ApplicationDbContext object
        //which represents the database session.
        //Without UserManager<ApplicationUser> you cannot access the ApplicationUser through the HttpContext of the HTTP request
        public SessionSynopsesController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory, ApplicationDbContext database)
        {
            Database = database;

            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }
        //POST api/CreateSessionSynopsis
        [HttpPost("CreateSessionSynopsis")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSessionSynopsis(IFormCollection inFormData)
        {
            string customMessage = "";
            //Obtain the user id of the user who has logon

            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;

            SessionSynopsis oneNewSessionSynopsis = new SessionSynopsis();

            //After creating a new SessionSynopsis type instance, fill it up with the incoming values
            oneNewSessionSynopsis.SessionSynopsisName = inFormData["sessionSynopsisName"];
            oneNewSessionSynopsis.IsVisible = bool.Parse(inFormData["isVisible"]);
            oneNewSessionSynopsis.CreatedById = userInfoId;
            oneNewSessionSynopsis.UpdatedById = userInfoId;

            try
            {
                Database.SessionSynopses.Add(oneNewSessionSynopsis);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message
                        .Contains("SessionSynopsis_SessionSynopsisName_UniqueConstraint") == true)
                {
                    customMessage = "Unable to save session synopsis record due " +
                                "to another record having the same name : " +
                    oneNewSessionSynopsis.SessionSynopsisName;
                    //Create a fail message anonymous object that has one property, message.
                    //This anonymous object's Message property contains a simple string message
                    object httpFailRequestResultMessage = new { message = customMessage };
                    //Return a bad http request message to the client
                    return BadRequest(httpFailRequestResultMessage);
                }
                else
                {
                    object httpFailRequestResultMessage = new { message = ex.InnerException.Message };
                    //Return a bad http request message to the client
                    return BadRequest(httpFailRequestResultMessage);
                }
            }//End of try .. catch block on saving data
             //Construct a custom message for the client
             //Create a success message anonymous object which has a 
             //Message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Saved session synopsis record"
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of CreateSessionSynopsis method

        //PUT api/UpdateSessionSynopsis
        [HttpPut("UpdateSessionSynopsis/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSessionSynopsis(int id,IFormCollection inFormData)
        {
            string customMessage = "";

            //Obtain the user id of the user who has logon
            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;

            SessionSynopsis oneSessionSynopsis = Database.SessionSynopses
                .Where(ss => ss.SessionSynopsisId == id).Single();

            //After creating a new SessionSynopsis type instance, fill it up with the incoming values
            oneSessionSynopsis.SessionSynopsisName = inFormData["sessionSynopsisName"];
            oneSessionSynopsis.IsVisible = bool.Parse(inFormData["isVisible"]);
            oneSessionSynopsis.UpdatedById = userInfoId;

            try
            {
                Database.SessionSynopses.Update(oneSessionSynopsis);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message
                        .Contains("SessionSynopsis_SessionSynopsisName_UniqueConstraint") == true)
                {
                    customMessage = "Unable to save session synopsis record due " +
                                "to another record having the same name : " +
                    oneSessionSynopsis.SessionSynopsisName;
                    //Create a fail message anonymous object that has one property, message.
                    //This anonymous object's Message property contains a simple string message
                    object httpFailRequestResultMessage = new { message = customMessage };
                    //Return a bad http request message to the client
                    return BadRequest(httpFailRequestResultMessage);
                }
                else
                {
                    object httpFailRequestResultMessage = new { message = ex.InnerException.Message };
                    //Return a bad http request message to the client
                    return BadRequest(httpFailRequestResultMessage);
                }
            }//End of try .. catch block on saving data
             //Construct a custom message for the client
             //Create a success message anonymous object which has a 
             //Message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Saved session synopsis record"
            };

            //Create a OkObjectResult type instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of UpdateSessionSynopsis method

        // GET: api/SessionSynopses/GetAllSessionSynopsisData
        [HttpGet("GetAllSessionSynopsisData")]
        public JsonResult GetAllSessionSynopsisData()
        {
            List<object> sessionSynopsisList = new List<object>();
            var sessionSynopsisQueryResults = Database.SessionSynopses
                     .Include(input => input.CreatedBy)
                     .Include(input => input.UpdatedBy);
            //After obtaining all the session synopsis records from the database,
            //the sessionSynopsisQueryResults will become a container holding these SessionSynopsis entity objects.
            //I need to loop through each  SessionSynopsis instance inside sessionSynopsisQueryResults
            //so that I can build a sessionSynopsisList which contains anonymous objects.
            foreach (var oneSessionSynopsis in sessionSynopsisQueryResults)
            {
                sessionSynopsisList.Add(new
                {
                    sessionSynopsisId = oneSessionSynopsis.SessionSynopsisId,
                    sessionSynopsisName = oneSessionSynopsis.SessionSynopsisName,
                    isVisible = oneSessionSynopsis.IsVisible,
                    updatedBy = oneSessionSynopsis.UpdatedBy.FullName,
                    createdBy = oneSessionSynopsis.CreatedBy.FullName
                 });
            }//end of foreach loop which builds the sessionSynopsisList List container .
             //Use the JsonResult class to create a new JsonResult instance by using the sessionSynopsisList.
             //The ASP.NET framework will do the rest to translate it into a string JSON structured content
             //which can travel through the Internet wire to the client browser.
             //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            return new JsonResult(sessionSynopsisList);
        }//end of GetAllSessionSynopsisData()

        // GET: api/SessionSynopses/GetSessionSynopsesForControls
        [HttpGet("GetSessionSynopsesForControls")]
        public JsonResult GetSessionSynopsesForControls()
        {
            List<object> sessionSynopsisList = new List<object>();
            var sessionSynopsisQueryResults = Database.SessionSynopses;
                     //.Include(input => input.CreatedBy)
                     //.Include(input => input.UpdatedBy);
            //After obtaining all the session synopsis records from the database,
            //the sessionSynopsisQueryResults will become a container holding these SessionSynopsis entity objects.
            //I need to loop through each  SessionSynopsis instance inside sessionSynopsisQueryResults
            //so that I can build a sessionSynopsisList which contains anonymous objects.
            foreach (var oneSessionSynopsis in sessionSynopsisQueryResults)
            {
                sessionSynopsisList.Add(new
                {
                    sessionSynopsisId = oneSessionSynopsis.SessionSynopsisId,
                    sessionSynopsisName = oneSessionSynopsis.SessionSynopsisName
                    //isVisible = oneSessionSynopsis.IsVisible,
                    //updatedBy = oneSessionSynopsis.UpdatedBy.FullName,
                    //createdBy = oneSessionSynopsis.CreatedBy.FullName
                });
            }//end of foreach loop which builds the sessionSynopsisList List container .
             //Use the JsonResult class to create a new JsonResult instance by using the sessionSynopsisList.
             //The ASP.NET framework will do the rest to translate it into a string JSON structured content
             //which can travel through the Internet wire to the client browser.
             //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            return new JsonResult(sessionSynopsisList);
        }//end of GetAllSessionSynopsisData()


        // GET: api/SessionSynopses/GetAllSessionSynopsisData
        [HttpGet("GetOneSessionSynopsis/{id}")]
        public JsonResult GetOneSessionSynopsis(int id)
        {
           
            var oneSessionSynopsis = Database.SessionSynopses
                     .Where(input=>input.SessionSynopsisId==id)
                     .Include(input => input.CreatedBy)
                     .Include(input => input.UpdatedBy).AsNoTracking().Single();

             var data = new {
                    sessionSynopsisId = oneSessionSynopsis.SessionSynopsisId,
                    sessionSynopsisName = oneSessionSynopsis.SessionSynopsisName,
                    isVisible = oneSessionSynopsis.IsVisible,
                    updatedBy = oneSessionSynopsis.UpdatedBy.FullName,
                    createdBy = oneSessionSynopsis.CreatedBy.FullName
                };
       
            return new JsonResult(data);
        }//end of GetOneSessionSynopsis()


        //-------------------------------------------------------------------------------------------------------------------------
        /*   DELETE WEB API  */
        //-------------------------------------------------------------------------------------------------------------------------
        // DELETE api/Students/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string customMessage = "";

            try
            {
                var sessionSynopsis = Database.SessionSynopses
                .Single(ss => ss.SessionSynopsisId == id);

   
                //Update the database model
                Database.SessionSynopses.Remove(sessionSynopsis);
                //Tell the db model to commit/persist the changes to the database, 
                //I use the following command.
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                customMessage = "Unable to delete student record.";
                object httpFailRequestResultMessage = new { message = customMessage };
                //Return a bad http request message to the client
                return BadRequest(httpFailRequestResultMessage);
            }//End of try .. catch block on manage data

            //Build a custom message for the client
            //Create a success message anonymous type object which has a 
            //message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Deleted session synopsis record"
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//end of Delete() Web API method


    }
}
