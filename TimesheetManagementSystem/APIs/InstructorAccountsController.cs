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
    [Authorize]
    [Route("api/[controller]")]
    public class InstructorAccountsController : Controller
    {
        //Create a property Database so that the code in the Web API controller methods
        //can use this property to communicate with the database. Note that, this Database
        //property is required in every Web API controller. The property is initiatialized in the Controller's
        //Constructor. (In this case, the public StudentsController() constructor has been created
        //so that the Database property is instantiated as an ApplicationDbContext instance.

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
        public InstructorAccountsController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory, ApplicationDbContext database)
        {
            //The code below was useful to get my project working.
            //After that, I have referred to two online site content to improve the code
            //because having three lines of such code in every Web API controller class is definitely
            //a No No.
            /*
            var options = new DbContextOptionsBuilder<ApplicationDbContext>();
            options.UseSqlServer(@"Server=IT-NB147067\SQLEXPRESS;database=InternshipManagementSystemWithSecurityDB_V1;Trusted_Connection=True;MultipleActiveResultSets=True");
            Database = new ApplicationDbContext(options.Options); */
            Database = database;

            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }


        //POST api/CreateAccountRate
        [HttpPost("AssignOneInstructorToCustomerAccount")]
        //[ValidateAntiForgeryToken]
        public IActionResult AssignOneInstructorToCustomerAccount(IFormCollection inFormData)
        {
            string customMessage = "";
            //No need to fetch user id value because this AccountRate is not going to capture this value.

            InstructorAccount oneNewInstructorAccount = new InstructorAccount();
            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;
            oneNewInstructorAccount.EffectiveStartDate = DateTime.ParseExact(inFormData["effectiveStartDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            if (inFormData["effectiveEndDate"] != "")
            {
                //If the there are values for the effectiveEndDate then start copying into the EffectiveEndDate property
                oneNewInstructorAccount.EffectiveEndDate = DateTime.ParseExact(inFormData["effectiveEndDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            }

            oneNewInstructorAccount.InstructorId = Int32.Parse(inFormData["instructorId"]);
            oneNewInstructorAccount.CustomerAccountId = Int32.Parse(inFormData["customerAccountId"]);
            oneNewInstructorAccount.WageRate = Decimal.Parse(inFormData["wageRate"]);
            oneNewInstructorAccount.Comments = inFormData["comments"];
            //oneNewInstructorAccount.IsCurrent = bool.Parse(inFormData["isCurrent"]);

            List<AccountDetail> existingAccountDetailList = Database.AccountDetails
                .Where(input => input.CustomerAccountId == Int32.Parse(inFormData["customerAccountId"]))
                .AsNoTracking().ToList<AccountDetail>();

            try
            {
                Database.InstructorAccounts.Add(oneNewInstructorAccount);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                {
                    String innerMessage = (ex.InnerException != null)
                        ? ex.InnerException.Message
                        : "";


                    //should have unique constraint whereby unable to assign instructor to acc due to 
                    //already existing assignment.


                    customMessage = "Unable to assign Instructor to Account.";
                    object httpFailRequestResultMessage = new { message = customMessage + " : " + innerMessage };
                    //Return a bad http request message to the client
                    return BadRequest(httpFailRequestResultMessage);
                }
            }//End of try .. catch block on saving data
            object result;
            AreThereAnyAccountDetail(oneNewInstructorAccount, existingAccountDetailList, out result);

            ////Construct a custom message for the client
            ////Create a success message anonymous object which has a 
            ////message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Successfully Assigned Instructor to Account.",
                anyAccountDetailRecords = result,
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of CreateAccountRate() method

        //api/CustomerAccounts/GetAssignedInstructorsByCustomerAccount
        [HttpGet("GetAssignedInstructorsByCustomerAccount/{inCustomerAccountId}")]
        public JsonResult GetAssignedInstructorsByCustomerAccount(int inCustomerAccountId)
        {
            List<object> instructorAccountList = new List<object>();
            var instructorAccountsQueryResult = Database.InstructorAccounts
                .Where(input => input.CustomerAccountId == inCustomerAccountId)
                .Include(input => input.Instructor); // for Instructor fullname
            var foundOneCustomerAccount = Database.CustomerAccounts
                .Where(input => input.CustomerAccountId == inCustomerAccountId)
                .Single();
            var customerAccount = new
            {
                customerAccountId = foundOneCustomerAccount.CustomerAccountId,
                accountName = foundOneCustomerAccount.AccountName
            };

            //After obtaining all the account raterecords from the database,
            //the accountList will become a container holding these AccountRate entity objects.
            //I need to loop through each  AccountRate instance inside accountRatesQueryResult
            //so that I can build a accountList which contains anonymous objects.
            foreach (var oneInstructorAccount in instructorAccountsQueryResult)
            {
                instructorAccountList.Add(new
                {
                    instructorAccountId = oneInstructorAccount.InstructorAccountId,
                    instructorId = oneInstructorAccount.InstructorId,
                    fullName = oneInstructorAccount.Instructor.FullName,
                    comments = oneInstructorAccount.Comments,
                    wageRate = oneInstructorAccount.WageRate,
                    effectiveStartDate = oneInstructorAccount.EffectiveStartDate,
                    effectiveEndDate = oneInstructorAccount.EffectiveEndDate,
                    //isCurrent = oneInstructorAccount.IsCurrent
                });
            }//end of foreach loop which builds the accountRateList List container .

            var response = new
            {
                account = customerAccount,
                instructorAccountList = instructorAccountList
            };

            //Use the JsonResult class to create a new JsonResult instance by using the accountList.
            //The ASP.NET framework will do the rest to translate it into a string JSON structured content
            //which can travel through the Internet wire to the client browser.
            //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            return new JsonResult(response);
        }//end of GetAccountRates()


        //api/CustomerAccounts/GetOneAssignedInstructor
        [HttpGet("GetOneAssignedInstructor/{inInstructorAccountId}")]

        public JsonResult GetOneAssignedInstructor(int inInstructorAccountId)
        {
            List<object> instructorAccountList = new List<object>();
            var foundOneInstructorAccount = Database.InstructorAccounts
                .Where(input => input.InstructorAccountId == inInstructorAccountId).Single();

            var response = new
            {
                instructorAccountId = foundOneInstructorAccount.InstructorAccountId,
                customerAccountId = foundOneInstructorAccount.CustomerAccountId,
                instructorId = foundOneInstructorAccount.InstructorId,
                comments = foundOneInstructorAccount.Comments,
                wageRate = foundOneInstructorAccount.WageRate,
                effectiveStartDate = foundOneInstructorAccount.EffectiveStartDate,
                effectiveEndDate = foundOneInstructorAccount.EffectiveEndDate,
                //isCurrent = foundOneInstructorAccount.IsCurrent,

            };

            //Use the JsonResult class to create a new JsonResult instance by using the accountList.
            //The ASP.NET framework will do the rest to translate it into a string JSON structured content
            //which can travel through the Internet wire to the client browser.
            //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            return new JsonResult(response);
        }//end of GetOneAccountRate

        // PUT api/values/5
        [HttpPut("UpdateAssignmentOfInstructorToCustomerAccount/{inInstructorAccountId}")]
        //[ValidateAntiForgeryToken]

        public IActionResult UpdateAssignmentOfInstructorToCustomerAccount(IFormCollection inFormData, string inInstructorAccountId)
        {
            /// <summary>
            /// HTTP type is PUT and URL is /api/AccountDetails/UpdateOneAccountDetail
            /// The web api method takes in IFormCollection type object which has all the account detail 
            /// information sent from the client-side. The web api updates an existing account detail in the database
            /// </summary>   
            string customMessage = "";
            int instructorAccountId = Int32.Parse(inFormData["instructorAccountId"]);
            int customerAccountId = Int32.Parse(inFormData["customerAccountId"]);


            //Obtain the user id id of the user who has logon
            string userLoginName = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginName).UserInfoId;

            int instructorIdInInt;

            //Obtain an exisiting record and map it into the foundOneAccountDetail instance
            InstructorAccount foundOneInstructorAccount = Database.InstructorAccounts
                .Where(input => input.InstructorAccountId == instructorAccountId).Single();
            //By having the foundOneInstructorAccount instance representing the information
            //,begin update the necessary properties. Avoid touching the CustomerAccountId property


            //foundOneInstructorAccount.InstructorId = Int32.Parse(inFormData["instructorId"]);




            if (Int32.TryParse(inFormData["instructorId"], out instructorIdInInt))
            {
                foundOneInstructorAccount.InstructorId = instructorIdInInt;
            }


            foundOneInstructorAccount.CustomerAccountId = Int32.Parse(inFormData["customerAccountId"]);
            foundOneInstructorAccount.EffectiveStartDate = DateTime.ParseExact(inFormData["effectiveStartDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            if (inFormData["effectiveEndDate"] != "")
            {
                //If the there are values for the effectiveEndDate then start copying into the EffectiveEndDate property
                foundOneInstructorAccount.EffectiveEndDate = DateTime.ParseExact(inFormData["effectiveEndDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            }

            foundOneInstructorAccount.Comments = inFormData["comments"];
            foundOneInstructorAccount.WageRate = Decimal.Parse(inFormData["wageRate"]);
            //foundOneInstructorAccount.IsCurrent = bool.Parse(inFormData["isCurrent"]);



            //Find the parent record to update the UpdatedAt property and UpdatedById property
            CustomerAccount foundParentCustomerAccount = Database.CustomerAccounts.Where(input => input.CustomerAccountId == customerAccountId).Single();
            foundParentCustomerAccount.UpdatedAt = DateTime.Now;
            foundParentCustomerAccount.UpdatedById = userInfoId;


            //List<AccountDetail> existingAccountDetailList = Database.AccountDetails
            //    .Where(input => input.CustomerAccountId == Int32.Parse(inFormData["customerAccountId"]))
            //    .AsNoTracking().ToList<AccountDetail>();

            object result = null;
            //AreThereAnyAccountDetail(foundOneInstructorAccount, existingAccountDetailList, out result);
            try
            {
                // AreThereAnyOverlappingAccountDetail(foundOneAccountDetail, accountDetailsQueryResult, out result);
                Database.InstructorAccounts.Update(foundOneInstructorAccount);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                {
                    String innerMessage = (ex.InnerException != null)
                        ? ex.InnerException.Message
                        : "";


                    //should have unique constraint whereby unable to assign instructor to acc due to 
                    //already existing assignment.


                    customMessage = "Unable to assign Instructor to Account";
                    object httpFailRequestResultMessage = new { message = customMessage + " : " + innerMessage };
                    //Return a bad http request message to the client
                    return BadRequest(httpFailRequestResultMessage);
                }
            }//End of try .. catch block on saving data
             //Construct a custom message for the client
             //Create a success message anonymous object which has a 
             //Message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Successfully updated assignment of Instructor to Account",
                anyAccountDetailRecords = result,

            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of UpdateOneAccountDetail() method


        // DELETE api/values/5
        [HttpDelete("DeleteInstructorAccount/{inInstructorAccountId}")]
        public IActionResult DeleteInstructorAccount(IFormCollection inFormData, string inInstructorAccountId)
        {
            //    public void Delete(int id)
            //{
            //    [HttpPut("UpdateAssignmentOfInstructorToCustomerAccount/{inInstructorAccountId}")]
            //[ValidateAntiForgeryToken]

            //public IActionResult UpdateAssignmentOfInstructorToCustomerAccount(IFormCollection inFormData, string inInstructorAccountId)
            //{
            /// <summary>
            /// HTTP type is PUT and URL is /api/AccountDetails/UpdateOneAccountDetail
            /// The web api method takes in IFormCollection type object which has all the account detail 
            /// information sent from the client-side. The web api updates an existing account detail in the database
            /// </summary>   
            string customMessage = "";
            int instructorAccountId = Int32.Parse(inInstructorAccountId);
            //Int32.Parse(inFormData["instructorAccountId"]);

            InstructorAccount foundOneInstructorAccount = Database.InstructorAccounts
                .Where(input => input.InstructorAccountId == instructorAccountId).Single();

            int customerAccountId = foundOneInstructorAccount.CustomerAccountId;

            //int customerAccountId = Int32.Parse(inFormData["customerAccountId"]);


            //Obtain the user id id of the user who has logon
            string userLoginName = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginName).UserInfoId;

            //int instructorIdInInt;

            //Obtain an exisiting record and map it into the foundOneAccountDetail instance
            
            //By having the foundOneInstructorAccount instance representing the information
            //,begin update the necessary properties. Avoid touching the CustomerAccountId property

            //if (Int32.TryParse(inFormData["instructorId"], out instructorIdInInt))
            //{
            //    foundOneInstructorAccount.InstructorId = instructorIdInInt;
            //}

            //foundOneInstructorAccount.InstructorId = Int32.Parse(inFormData["instructorId"]);


            //foundOneInstructorAccount.CustomerAccountId = Int32.Parse(inFormData["customerAccountId"]);
            //foundOneInstructorAccount.EffectiveStartDate = DateTime.ParseExact(inFormData["effectiveStartDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            //if (inFormData["effectiveEndDate"] != "")
            //{
            //    //If the there are values for the effectiveEndDate then start copying into the EffectiveEndDate property
            //    foundOneInstructorAccount.EffectiveEndDate = DateTime.ParseExact(inFormData["effectiveEndDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            //}

            //foundOneInstructorAccount.Comments = inFormData["comments"];
            //foundOneInstructorAccount.WageRate = Decimal.Parse(inFormData["wageRate"]);
            //foundOneInstructorAccount.IsCurrent = bool.Parse(inFormData["isCurrent"]);



            //Find the parent record to update the UpdatedAt property and UpdatedById property
            CustomerAccount foundParentCustomerAccount = Database.CustomerAccounts.Where(input => input.CustomerAccountId == customerAccountId).Single();
            foundParentCustomerAccount.UpdatedAt = DateTime.Now;
            foundParentCustomerAccount.UpdatedById = userInfoId;

            
            object result = null;

            try
            {
                // AreThereAnyOverlappingAccountDetail(foundOneAccountDetail, accountDetailsQueryResult, out result);
                Database.InstructorAccounts.Remove(foundOneInstructorAccount);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                {
                    String innerMessage = (ex.InnerException != null)
                        ? ex.InnerException.Message
                        : "";


                    customMessage = "Unable to remove assignment of Instructor from Account";
                    object httpFailRequestResultMessage = new { message = customMessage + " : " + innerMessage };
                    //Return a bad http request message to the client
                    return BadRequest(httpFailRequestResultMessage);
                }
            }//End of try .. catch block on saving data
             //Construct a custom message for the client
             //Create a success message anonymous object which has a 
             //Message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Successfully removed Instructor assignment from Account",
                anyAccountDetailRecords = result,

            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of UpdateOneAccountDetail() method




        public bool AreThereAnyAccountDetail(InstructorAccount inOneInstructorAccount,/*, AccountDetail inOneAccountDetail,*/ List<AccountDetail> inAccountDetailList, out object result)
        {
            //The following two objects are used to build an anonymous type result
            var anyAccountDetailList = new List<object>();

            List<object> accountDetailList = new List<object>();
            var accountDetailsQueryResult = Database.AccountDetails
                .Where(input =>
                (input.CustomerAccountId == inOneInstructorAccount.CustomerAccountId))
                .AsNoTracking()
                .ToList();

            foreach (var oneAccountDetail in accountDetailsQueryResult)
            {
                accountDetailList.Add(new
                {
                    accountDetailId = oneAccountDetail.AccountDetailId,
                    startTimeInMinutes = oneAccountDetail.StartTimeInMinutes,
                    dayOfWeekNumber = oneAccountDetail.DayOfWeekNumber,
                    endTimeInMinutes = oneAccountDetail.EndTimeInMinutes,
                    startTimeInHHMM = ConvertFromMinutesToHHMM(oneAccountDetail.StartTimeInMinutes),
                    endTimeInHHMM = ConvertFromMinutesToHHMM(oneAccountDetail.EndTimeInMinutes),
                    effectiveStartDate = oneAccountDetail.EffectiveStartDate,
                    effectiveEndDate = oneAccountDetail.EffectiveEndDate,
                    isVisible = oneAccountDetail.IsVisible
                });
            }//end of foreach loop which builds the accountList List container .

            if (accountDetailList.Count == 0)
            {
                result = new
                {
                    //inspectedData = intendedData,
                    anyAccountDetailList = accountDetailList,
                    message = "No AccountDetail was created for this Account, Instructor might be unable to generate Timesheet Data"
                };
                return false;
            }

            else
            {

                result = new
                {
                    //inspectedData = intendedData,
                    anyAccountDetailList = accountDetailList,
                    //message = "Existing Account Details that Instructor can teach for:" +anyAccountDetailList

                };
                return true;

            }
        }

        string ConvertFromMinutesToHHMM(int inMinutes)
        {  //http://stackoverflow.com/questions/13044603/convert-time-span-value-to-format-hhmm-am-pm-using-c-sharp
            TimeSpan timespan = new TimeSpan(00, inMinutes, 00);
            DateTime time = DateTime.Today.Add(timespan);
            string formattedTime = time.ToString("hh:mm tt");
            return formattedTime;
        }
        private int ConvertHHMMToMinutes(string inHHMM)
        {
            int totalMinutesAfterMidnight = 0;
            inHHMM = inHHMM.Trim();//To play safe, trim leading and trailing spaces.
            DateTime timeValue = Convert.ToDateTime(inHHMM);
            string inHHMM_24HourFormat = timeValue.ToString("HH:mm");


            int minutesPart = Int32.Parse(inHHMM_24HourFormat.Split(':')[1]);
            int hoursPart = Int32.Parse(inHHMM_24HourFormat.Split(':')[0]);

            totalMinutesAfterMidnight = minutesPart + (hoursPart * 60);

            return totalMinutesAfterMidnight;
        }

    }

}

