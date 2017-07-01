using TimeSheetManagementSystem.Models;
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

namespace TimeSheetManagementSystem.APIs
{
    [Authorize]
    [Route("api/[controller]")]
    public class AccountDetailsController : Controller
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
        public AccountDetailsController(UserManager<ApplicationUser> userManager,
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


        [HttpPut("UpdateOneAccountDetail")]
        public IActionResult UpdateOneAccountDetail(IFormCollection inFormData)
        {
            /// <summary>
            /// HTTP type is PUT and URL is /api/AccountDetails/UpdateOneAccountDetail
            /// The web api method takes in IFormCollection type object which has all the account detail 
            /// information sent from the client-side. The web api updates an existing account detail in the database
            /// </summary>   
            string customMessage = "";
            int accountDetailId = Int32.Parse(inFormData["accountDetailId"]);
            int customerAccountId = Int32.Parse(inFormData["customerAccountId"]);


            //Obtain the user id id of the user who has logon
            string userLoginName = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginName).UserInfoId;

            //Obtain an exisiting record and map it into the foundOneAccountDetail instance
            AccountDetail foundOneAccountDetail = Database.AccountDetails
                .Where(input => input.AccountDetailId == accountDetailId).Single();
            //By having the foundOneAccountDetail instance representing the information
            //,begin update the necessary properties. Avoid touching the CustomerAccountId property
            foundOneAccountDetail.EffectiveStartDate = DateTime.ParseExact(inFormData["effectiveStartDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            if (inFormData["effectiveEndDate"] != "")
            {
                //If the there are values for the effectiveEndDate then start copying into the EffectiveEndDate property
                foundOneAccountDetail.EffectiveEndDate = DateTime.ParseExact(inFormData["effectiveEndDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            }
            foundOneAccountDetail.IsVisible = true;
            //The incoming start time information from the client side is in HH:MM format. Need to do conversion
            //to total minutes from midnight
            foundOneAccountDetail.StartTimeInMinutes = ConvertHHMMToMinutes(inFormData["startTime"]);
            //The incoming end time information is in HH:MM format from the client-side. Need to do conversion
            //to total minutes from midnight
            foundOneAccountDetail.EndTimeInMinutes = ConvertHHMMToMinutes(inFormData["endTime"]);
            foundOneAccountDetail.IsVisible = bool.Parse(inFormData["isVisible"]);
            foundOneAccountDetail.DayOfWeekNumber = Int32.Parse(inFormData["dayOfWeekNumber"]);

            //Find the parent record to update the UpdatedAt property and UpdatedById property
            CustomerAccount foundParentCustomerAccount = Database.CustomerAccounts.Where(input => input.CustomerAccountId == customerAccountId).Single();
            foundParentCustomerAccount.UpdatedAt = DateTime.Now;
            foundParentCustomerAccount.UpdatedById = userInfoId;

            //The overlapping checking logic is "slightly different" from the CreateOneAccountDetail method.
            //Obtain all account detail information regardless of the IsVisible property value 
            //true or false. "Except" the current account detail information which the logic is currently focusing.
            var accountDetailsQueryResult = Database.AccountDetails
                    .Where(input =>
                    (input.CustomerAccountId == customerAccountId) && (input.AccountDetailId != accountDetailId))
                    .AsNoTracking()
                    .ToList();




            object result = null;
            try
            {
               AreThereAnyOverlappingAccountDetail(foundOneAccountDetail, accountDetailsQueryResult, out result);
               Database.AccountDetails.Update(foundOneAccountDetail);
               Database.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message
                          .Contains("AccountDetail_UniqueConstraint") == true)
                {
                    customMessage = "Unable to save account detail record due " +
                                "to another record having the exact (same) lesson time table configuration. ";
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
                message = "Saved account detail record",
                conflictingRecords = result,
                currentRecord = new
                {
                    accountDetailId = foundOneAccountDetail.AccountDetailId,
                    startTimeInMinutes = foundOneAccountDetail.StartTimeInMinutes,
                    dayOfWeekNumber = foundOneAccountDetail.DayOfWeekNumber,
                    endTimeInMinutes = foundOneAccountDetail.EndTimeInMinutes,
                    startTimeInHHMM = ConvertFromMinutesToHHMM(foundOneAccountDetail.StartTimeInMinutes),
                    endTimeInHHMM = ConvertFromMinutesToHHMM(foundOneAccountDetail.EndTimeInMinutes),
                    effectiveStartDate = foundOneAccountDetail.EffectiveStartDate,
                    effectiveEndDate = foundOneAccountDetail.EffectiveEndDate,
                    isVisible = foundOneAccountDetail.IsVisible
                }
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of UpdateOneAccountDetail() method


        [HttpPost("CreateOneAccountDetail")]
        public IActionResult CreateOneAccountDetail(IFormCollection inFormData)
        {
            /// <summary>
            /// Request type is POST and URL is /API/AccountDetails/CreateOneAccountDetail
            /// The web api method takes in IFormCollection type inFormData which represents
            /// all the form data which describes the account detail sent from the client.
            /// </summary>   
            string customMessage = "";
            object result = null;
            //Obtain the user id id of the user who has logon

            string userLoginName = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginName).UserInfoId;
            //Create a new AccountDetail type instance first, before filling it up with values.
            AccountDetail oneNewAccountDetail = new AccountDetail();
            int customerAccountId = Int32.Parse(inFormData["customerAccountId"]);
            //After creating a new AccountDetail type instance, fill it up with the incoming values
            oneNewAccountDetail.CustomerAccountId = customerAccountId;
            oneNewAccountDetail.EffectiveStartDate = DateTime.ParseExact(inFormData["effectiveStartDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            if (inFormData["effectiveEndDate"] != "")
            {
                //If the there are values for the effectiveEndDate then start copying into the EffectiveEndDate property
                oneNewAccountDetail.EffectiveEndDate = DateTime.ParseExact(inFormData["effectiveEndDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            }
            //The meaning of IsVisible property is, this AccountDetail information
            //can be used by the system to generate a list of dates for a particular month for the instructor to choose
            //when he needs to create timesheet.
            //The administrator can set this property to false, if administrator need to stop the instructor
            //from choosing any dates which are derived from the DayOfWeekNumber of the respective
            //AccountDetail information.
            oneNewAccountDetail.IsVisible = true;
            //The client side sends start time information is in HH:MM format. Need to do conversion
            oneNewAccountDetail.StartTimeInMinutes = ConvertHHMMToMinutes(inFormData["startTime"]);
            //The client side sends end time information is in HH:MM format. Need to do conversion
            oneNewAccountDetail.EndTimeInMinutes = ConvertHHMMToMinutes(inFormData["endTime"]);
            oneNewAccountDetail.IsVisible = bool.Parse(inFormData["isVisible"]);
            oneNewAccountDetail.DayOfWeekNumber = Int32.Parse(inFormData["dayOfWeekNumber"]);
            //---------------- Security and Authorization code ----(Start)------------------------------
            //When the record is created, the CreatedById and the UpdatedById property
            //has the same value.
            //oneNewAccount.CreatedById = (await _userManager.GetUserAsync(HttpContext.User))?.UserName;
            //oneNewAccount.UpdatedById = _userManager.GetUserId(User);
            //Don't need oneNewAccount.CreatedAt = DateTime.Now; because there is 
            //a default value using GETDATE() setup in the database.
            //---------------- Security and Authorization code ------(End)------------------------------

            //Obtain all account detail information (IsEnabled true or false all will be included)
            var accountDetailsQueryResult = Database.AccountDetails
                    .Where(input =>
                    (input.CustomerAccountId == customerAccountId))
                    .AsNoTracking()
                    .ToList();

            //Find the parent record to update the UpdatedAt property and UpdatedById property
            CustomerAccount foundParentCustomerAccount = Database.CustomerAccounts.Where(input => input.CustomerAccountId == customerAccountId).Single();
            foundParentCustomerAccount.UpdatedAt = DateTime.Now;
            foundParentCustomerAccount.UpdatedById = userInfoId;

            try
            {
                AreThereAnyOverlappingAccountDetail(oneNewAccountDetail, accountDetailsQueryResult, out result);
                Database.AccountDetails.Add(oneNewAccountDetail);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message
                         .Contains("AccountDetail_UniqueConstraint") == true)
                {
                    customMessage = "Unable to save account detail record due " +
                                "to another record having the exact (same) lesson time table configuration. ";
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
                message = "Saved account detail record",
                conflictingRecords = result,
                currentRecord = new
                {
                    accountDetailId = oneNewAccountDetail.AccountDetailId,
                    startTimeInMinutes = oneNewAccountDetail.StartTimeInMinutes,
                    dayOfWeekNumber = oneNewAccountDetail.DayOfWeekNumber,
                    endTimeInMinutes = oneNewAccountDetail.EndTimeInMinutes,
                    startTimeInHHMM = ConvertFromMinutesToHHMM(oneNewAccountDetail.StartTimeInMinutes),
                    endTimeInHHMM = ConvertFromMinutesToHHMM(oneNewAccountDetail.EndTimeInMinutes),
                    effectiveStartDate = oneNewAccountDetail.EffectiveStartDate,
                    effectiveEndDate = oneNewAccountDetail.EffectiveEndDate,
                    isVisible = oneNewAccountDetail.IsVisible
                }
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of CreateOneAccountDetail() method

        [HttpGet("GetOneAccountDetailByAccountDetailId/{inAccountDetailId}")]
        public JsonResult GetOneAccountDetailByAccountDetailId(int inAccountDetailId)
        {
            /// <summary>
            /// URL is /API/AccountDetails/GetOneAccountDetailByAccountDetailId/5
            /// The web api method takes in account detail unique id and returns an object
            /// which describes the account detail record which is associated to the given id.
            /// </summary>   
            var foundOneAccountDetail = Database.AccountDetails
                .Where(input =>
                (input.AccountDetailId == inAccountDetailId))
                .AsNoTracking().Single();


            //Create anonymous type object. This object has property-value pairs
            //decribing one account detail.
            var oneAccountDetail = new
            {
                accountDetailId = foundOneAccountDetail.AccountDetailId,
                startTimeInMinutes = foundOneAccountDetail.StartTimeInMinutes,
                dayOfWeekNumber = foundOneAccountDetail.DayOfWeekNumber,
                endTimeInMinutes = foundOneAccountDetail.EndTimeInMinutes,
                startTimeInHHMM = ConvertFromMinutesToHHMM(foundOneAccountDetail.StartTimeInMinutes),
                //If there is no effective end time, a null value is sent to the client-side. The JavaScript logic
                //need to take care of the null value to display it meaningfully to the user.
                endTimeInHHMM = ConvertFromMinutesToHHMM(foundOneAccountDetail.EndTimeInMinutes),
                effectiveStartDate = foundOneAccountDetail.EffectiveStartDate,
                effectiveEndDate = foundOneAccountDetail.EffectiveEndDate,
                isVisible = foundOneAccountDetail.IsVisible,
                customerAccountId = foundOneAccountDetail.CustomerAccountId /*Need this one for client-side */
            };//end of building one anonymous type object

            return new JsonResult(oneAccountDetail);
        }//End of GetOneAccountDetailByAccountDetailId


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
        [HttpGet("GetCurrentAccountDetailsByCustomerAccountId/{inCustomerAccountId}")]
        public JsonResult GetCurrentAccountDetailsByCustomerAccountId(int inCustomerAccountId)
        {
            List<object> accountDetailList = new List<object>();
            var accountDetailsQueryResult = Database.AccountDetails
                .Where(input => 
                (input.CustomerAccountId == inCustomerAccountId) )
                .AsNoTracking()
                .ToList();
            //Using for each looping technique to build up an array of anonymous type objects.
            //Each anonymous type object contains property-value information which decribe account detail.
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
             //Use the JsonResult class to create a new JsonResult instance by using the accountList.
             //The ASP.NET framework will do the rest to translate it into a string JSON structured content
             //which can travel through the Internet wire to the client browser.
             //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            return new JsonResult(accountDetailList);


        }

        [HttpGet("GetCurrentAccountDetailsByInstructorId")]
        public JsonResult GetCurrentAccountDetailsByInstructorId()
        {

            string userLoginName = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginName).UserInfoId;

            List<object> accountDetailList = new List<object>();
            var instructorAccountQueryResults = Database.InstructorAccounts
                .Where(input => input.InstructorId == userInfoId).AsNoTracking().ToList();
            foreach( var instructorAccountData in instructorAccountQueryResults)
            {
                var accountDetailQueryResults = Database.AccountDetails
                     .Include(input => input.CustomerAccount)
                    .Where(input => input.CustomerAccountId == instructorAccountData.CustomerAccountId )
                    .AsNoTracking()
                    .ToList();

                foreach (var oneAccountDetail in accountDetailQueryResults)
                {
                    accountDetailList.Add(new
                    {
                        accountDetailId = oneAccountDetail.AccountDetailId,
                        accountName = oneAccountDetail.CustomerAccount.AccountName,
                        startTimeInMinutes = oneAccountDetail.StartTimeInMinutes,
                        dayOfWeekNumber = oneAccountDetail.DayOfWeekNumber,
                        endTimeInMinutes = oneAccountDetail.EndTimeInMinutes,
                        startTimeInHHMM = ConvertFromMinutesToHHMM(oneAccountDetail.StartTimeInMinutes),
                        endTimeInHHMM = ConvertFromMinutesToHHMM(oneAccountDetail.EndTimeInMinutes),
                        effectiveStartDate = oneAccountDetail.EffectiveStartDate,
                        effectiveEndDate = oneAccountDetail.EffectiveEndDate,
                        isVisible = oneAccountDetail.IsVisible
                    });
                }//end of foreach loop which builds the accountDetailList List container .

            }
            //Using for each looping technique to build up an array of anonymous type objects.
            //Each anonymous type object contains property-value information which decribe account detail.

             //Use the JsonResult class to create a new JsonResult instance by using the accountList.
             //The ASP.NET framework will do the rest to translate it into a string JSON structured content
             //which can travel through the Internet wire to the client browser.
             //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            return new JsonResult(accountDetailList);
        }



        string ConvertFromMinutesToHHMM(int inMinutes)
        {  //http://stackoverflow.com/questions/13044603/convert-time-span-value-to-format-hhmm-am-pm-using-c-sharp
            TimeSpan timespan = new TimeSpan(00, inMinutes, 00);
            DateTime time = DateTime.Today.Add(timespan);
            string formattedTime = time.ToString("hh:mm tt");
            return formattedTime;
        }

        public bool AreThereAnyOverlappingAccountDetail(AccountDetail inOneAccountDetail,List<AccountDetail> inAccountDetailList, out object result)
        {
            //The following two objects are used to build an anonymous type result
            var conflictingDataList = new List<object>();
            var intendedData = new
            {
                effectiveStartDate = inOneAccountDetail.EffectiveStartDate.ToString("dd/MM/yyyy"),
                effectiveEndDate = (inOneAccountDetail.EffectiveEndDate != null ? inOneAccountDetail.EffectiveEndDate.Value.ToString("dd/MM/yyyy") : "n/a"),
                weekDayName = ((DayOfWeek)inOneAccountDetail.DayOfWeekNumber - 1).ToString(),
                startTime = ConvertFromMinutesToHHMM(inOneAccountDetail.StartTimeInMinutes),
                endTime = ConvertFromMinutesToHHMM(inOneAccountDetail.EndTimeInMinutes)
            };

            //The following endDate object is necessary because the user can provide an empty EffectiveEndDate.
            //Our AccountDetail class's EffectiveEndDate is NULLABLE. That will create problems in the code when do searching.
            //The command below will make the endDate hold 2199, Dec 31st (If the system can last that long, it is a miracle)
            //http://stackoverflow.com/questions/36260060/compare-datetime-with-datetimeallows-null-values-in-c-sharp
            DateTime endDate = inOneAccountDetail.EffectiveEndDate != null ? inOneAccountDetail.EffectiveEndDate.Value : new DateTime(2199, 12, 31);
            //The following will search for all account detail information which shares same week day number and overlapping start and
            //end date (including OPEN DATE, NULL) period.
            List<AccountDetail> accountDetailList = inAccountDetailList
               .FindAll(x => ((x.CustomerAccountId == inOneAccountDetail.CustomerAccountId)
                    && x.DayOfWeekNumber == inOneAccountDetail.DayOfWeekNumber)
                    && (((x.EffectiveStartDate <= endDate)
                    && (x.EffectiveEndDate == null))
                 || ((x.EffectiveStartDate <= endDate)
                    && (x.EffectiveEndDate.GetValueOrDefault() >= inOneAccountDetail.EffectiveStartDate))))
               .ToList();

            if (accountDetailList.Count == 0)
            {
                result = new
                {
                    inspectedData = intendedData,
                    dataListWhichMightConflict = conflictingDataList
                };
                return false;
            }

            List<AccountDetail> overallapingAccountDetailList = accountDetailList
                 .FindAll(x =>
                   (x.StartTimeInMinutes < inOneAccountDetail.EndTimeInMinutes)
                    && (x.EndTimeInMinutes > inOneAccountDetail.StartTimeInMinutes))
              .ToList();
            if (overallapingAccountDetailList.Count == 0)
            {
                result = new
                {
                    inspectedData = intendedData,
                    dataListWhichMightConflict = conflictingDataList
                };
                return false;
            }
            else
            {


                foreach (AccountDetail oneAccountDetail in overallapingAccountDetailList)
                {
                    var conflictingData = new {
                        effectiveStartDate = oneAccountDetail.EffectiveStartDate.ToString("dd/MM/yyyy"),
                        effectiveEndDate = (oneAccountDetail.EffectiveEndDate != null ? inOneAccountDetail.EffectiveEndDate.GetValueOrDefault().ToString("dd/MM/yyyy") : "n/a"),
                        weekDayName = ((DayOfWeek)oneAccountDetail.DayOfWeekNumber - 1).ToString(),
                        startTime = ConvertFromMinutesToHHMM(oneAccountDetail.StartTimeInMinutes),
                        endTime = ConvertFromMinutesToHHMM(oneAccountDetail.EndTimeInMinutes),
                        isVisible = oneAccountDetail.IsVisible
                    };
                    conflictingDataList.Add(conflictingData);
                    
                }
                result = new
                {
                    inspectedData = intendedData,
                    dataListWhichMightConflict = conflictingDataList
                };
                return true;
            }
        }
 

    }//End of class

    
}//End of namespace
