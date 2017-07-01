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

namespace TimeSheetManagementSystem.APIs
{
    [Authorize]
    [Route("api/[controller]")]
    public class AdminTimeSheetDetailsController : Controller
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
      
        public AdminTimeSheetDetailsController(UserManager<ApplicationUser> userManager,
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



 
        //POST api/CreateTimeSheetData
        [HttpPost("CreateTimeSheetData")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTimeSheetData(IFormCollection inFormData)
        {
            string customMessage = "";
            //Obtain the user id of the user who has logon

            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;

            TimeSheet oneNewTimeSheet = new TimeSheet();
            oneNewTimeSheet.TimeSheetDetails = new List<TimeSheetDetail>();
            //This is mostly going to be an admin user id because
            //this web api supports the functionality for creating 
            //timesheet data for testing which is only available for admin account
            oneNewTimeSheet.CreatedById = userInfoId;
            oneNewTimeSheet.InstructorId = Int32.Parse(inFormData["instructorId"]);
            oneNewTimeSheet.CreatedAt = DateTime.ParseExact(inFormData["simulatedCreatedAtDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            oneNewTimeSheet.UpdatedAt = DateTime.ParseExact(inFormData["simulatedCreatedAtDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            oneNewTimeSheet.MonthAndYear = new DateTime(oneNewTimeSheet.CreatedAt.Year, oneNewTimeSheet.CreatedAt.Month, 1);
            DateTime timeSheetMonthAndYear = new DateTime(oneNewTimeSheet.CreatedAt.Year, oneNewTimeSheet.CreatedAt.Month, 1);
           

            List<InstructorAccount> instructorAccountList
                 = Database.InstructorAccounts.Where(ia => (ia.InstructorId == oneNewTimeSheet.InstructorId)/* && (ia.IsCurrent == true)*/)
                 .Include(ia => ia.Instructor).Include(ia => ia.CustomerAccount.AccountRates)
                 .Include(ia => ia.CustomerAccount.AccountDetails).ToList();

            foreach (InstructorAccount oneInstructorAccountData in instructorAccountList)
            {
                AccountRate accountRate = oneInstructorAccountData.CustomerAccount.
                    AccountRates.Where(ar => ar.EffectiveStartDate <= timeSheetMonthAndYear).
                    OrderByDescending(ar => ar.EffectiveStartDate).First();

                List<AccountDetail> accountDetailList =
                oneInstructorAccountData.CustomerAccount.AccountDetails
                .Where(ad => ad.EffectiveStartDate <= timeSheetMonthAndYear)
                .OrderByDescending(ar => ar.EffectiveStartDate).ToList();
                //https://stackoverflow.com/questions/1700725/int-weekdayname
                foreach (AccountDetail oneAccountDetail in accountDetailList)
                {
                    DayOfWeek dayOfWeek = (DayOfWeek)Enum.ToObject(typeof(DayOfWeek), (oneAccountDetail.DayOfWeekNumber - 1));
                    List<DateTime> dateList = GetDates(timeSheetMonthAndYear.Year, timeSheetMonthAndYear.Month, dayOfWeek);
                    foreach (DateTime oneDate in dateList) {
                        oneNewTimeSheet.TimeSheetDetails.Add(new TimeSheetDetail
                        {
                            TimeSheetId = oneNewTimeSheet.TimeSheetId,
                            CreatedAt = oneNewTimeSheet.CreatedAt,
                            UpdatedAt = oneNewTimeSheet.UpdatedAt,
                            DateOfLesson = oneDate,
                            AccountName = 
                              oneInstructorAccountData.CustomerAccount.AccountName,
                             OfficialTimeInMinutes = oneAccountDetail.StartTimeInMinutes,
                             OfficialTimeOutMinutes = oneAccountDetail.EndTimeInMinutes,
                              RatePerHour = accountRate.RatePerHour,
                              WageRatePerHour = oneInstructorAccountData.WageRate,
                              IsReplacementInstructor = false,
                              SessionSynopsisNames = "",
                              CreatedByName = "system",
                              UpdatedByName = ""

                        });
                    }
                }

            }

            try
            {
                Database.TimeSheets.Add(oneNewTimeSheet);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
               
                    object httpFailRequestResultMessage = new { message = ex.InnerException.Message };
                    //Return a bad http request message to the client
                    return BadRequest(httpFailRequestResultMessage);
                
            }//End of try .. catch block on saving data
             //Construct a custom message for the client
             //Create a success message anonymous object which has a 
             //Message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Created new time sheet data"
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of CreateTimeSheetData() method


        public static List<DateTime> GetDates(int year, int month, DayOfWeek dayName)
        {
            List<DateTime> dateList = new List<DateTime>();
            CultureInfo ci = new CultureInfo("en-GB");
            for (int i = 1; i <= ci.Calendar.GetDaysInMonth(year, month); i++)
            {
                DateTime tempDate = new DateTime(year, month, i);
                if (tempDate.DayOfWeek == dayName)
                    dateList.Add(tempDate);
            }
            return dateList;
        }
    }//End of class
}
