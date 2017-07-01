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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TimeSheetManagementSystem.APIs
{
    [Route("api/[controller]")]
    public class TimeInTimeOutDataController : Controller
    {

        //7 important variables which require declaration
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;

        public ApplicationDbContext Database { get; }
        public IConfigurationRoot Configuration { get; }
        //End of 7 important variables which require declaration

        public TimeInTimeOutDataController(UserManager<ApplicationUser> userManager,
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
        // GET: api/GetOneTimeSheetDetailData/
        [HttpGet("GetOneTimeSheetDetailData/{id}")]
        public IActionResult GetOneTimeSheetDetailData(int id)
        {
                var oneTimeSheetDetail = Database.TimeSheetDetails
                         .Where(input => input.TimeSheetDetailId ==
                                   id)
                         .AsNoTracking().Single();

            var formatedTimeSheetDetail = new
            {

                timeSheetDetailId = oneTimeSheetDetail.TimeSheetDetailId,
                dateOfLesson = oneTimeSheetDetail.DateOfLesson,
                officialTimeIn = oneTimeSheetDetail.OfficialTimeInMinutes,
                officialTimeOut = oneTimeSheetDetail.OfficialTimeOutMinutes,
                officialTimeInHHMM = ConvertFromMinutesToHHMM(oneTimeSheetDetail.OfficialTimeInMinutes),
                officialTimeOutHHMM = ConvertFromMinutesToHHMM(oneTimeSheetDetail.OfficialTimeOutMinutes),
                actualTimeIn = oneTimeSheetDetail.TimeInInMinutes,
                actualTimeInHHMM = ConvertFromMinutesToHHMM(oneTimeSheetDetail.TimeInInMinutes.GetValueOrDefault()),
                actualTimeOut = oneTimeSheetDetail.TimeOutInMinutes,
                actualTimeOutHHMM = ConvertFromMinutesToHHMM(oneTimeSheetDetail.TimeOutInMinutes.GetValueOrDefault()),
                wageRatePerHour = oneTimeSheetDetail.WageRatePerHour,
                ratePerHour = oneTimeSheetDetail.RatePerHour,
                customerAccountName = oneTimeSheetDetail.AccountName,
                sessionSynopsisNames = oneTimeSheetDetail.SessionSynopsisNames,
                status = (oneTimeSheetDetail.TimeInInMinutes != null) ? "done" : ""
            };
 
            return new JsonResult(formatedTimeSheetDetail);

        }//End of GetOneTimeSheetDetailData

        // GET: api/GetTimeSheetAndTimeSheetDetails/
        [HttpGet("GetTimeSheetAndTimeSheetDetails")]
        public IActionResult GetTimeSheetAndTimeSheetDetails(TimeSheetDetailQueryModelByInstructor query)
        {
            List<object> timeSheetDetailList = new List<object>();
            object oneTimeSheetData = null;
            object response;
            TimeSheet oneTimeSheetQueryResult = new TimeSheet();
            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;
            if (query.Month != null)
            {
                oneTimeSheetQueryResult = Database.TimeSheets
                         .Include(input => input.Instructor)
                         .Where(input => (input.InstructorId == userInfoId) &&
                         (input.MonthAndYear.Month == query.Month) &&
                         (input.MonthAndYear.Year == query.Year)).AsNoTracking().FirstOrDefault();
            }
            else
            {
                oneTimeSheetQueryResult = Database.TimeSheets
                         .Include(input => input.Instructor)
                         .Where(input => (input.InstructorId == userInfoId) &&
                         (input.MonthAndYear.Month == DateTime.Now.Month) &&
                         (input.MonthAndYear.Year ==DateTime.Now.Year)).AsNoTracking().FirstOrDefault();
            }

            if (oneTimeSheetQueryResult == null)
            {
                response = new
                {
                    timeSheet = oneTimeSheetData,
                    timeSheetDetails = timeSheetDetailList
                };

                return new JsonResult(response);
            }
            oneTimeSheetData = new
            {
                timeSheetId = oneTimeSheetQueryResult.TimeSheetId,
                instructorName = oneTimeSheetQueryResult.Instructor.FullName,
                year = oneTimeSheetQueryResult.MonthAndYear.Year,
                month = oneTimeSheetQueryResult.MonthAndYear.Month,
                instructorId = oneTimeSheetQueryResult.InstructorId,
                createdAt = oneTimeSheetQueryResult.CreatedAt,
                updatedAt = oneTimeSheetQueryResult.UpdatedAt,
                approvedAt = oneTimeSheetQueryResult.ApprovedAt
            };
            List<TimeSheetDetail> timeSheetDetailsQueryResult = new List<TimeSheetDetail>();
            if (oneTimeSheetQueryResult != null)
            {
                timeSheetDetailsQueryResult = Database.TimeSheetDetails
                         .Where(input => input.TimeSheetId ==
                                   oneTimeSheetQueryResult.TimeSheetId)
                         .AsNoTracking().ToList<TimeSheetDetail>();
            }
            //The following block of LINQ code is used for testing purpose to sort the 
            //timesheetdetail information by lesson dates.
            var sortedTimeSheetDetailList = from e in timeSheetDetailsQueryResult
                                            select new
                                            {
                                                timeSheetDetailId = e.TimeSheetDetailId,
                                                dateOfLesson = e.DateOfLesson,
                                                officialTimeIn = e.OfficialTimeInMinutes,
                                                officialTimeOut = e.OfficialTimeOutMinutes,
                                                actualTimeIn = e.TimeInInMinutes,
                                                officialTimeInHHMM = ConvertFromMinutesToHHMM(e.OfficialTimeInMinutes),
                                                actualTimeOut = e.TimeOutInMinutes,
                                                officialTimeOutHHMM = ConvertFromMinutesToHHMM(e.OfficialTimeOutMinutes),
                                                wageRatePerHour = e.WageRatePerHour,
                                                ratePerHour = e.RatePerHour,
                                                customerAccountName = e.AccountName,
                                                sessionSynopsisNames = e.SessionSynopsisNames,
                                                status = (e.TimeInInMinutes!=null)?"done":""
                         }
                   into temp
                       orderby temp.dateOfLesson ascending
                       select temp;




            foreach (var oneTimeSheetDetail in timeSheetDetailsQueryResult)
            {
                timeSheetDetailList.Add(new
                {
                    timeDetailSheetId = oneTimeSheetDetail.TimeSheetDetailId,
                    dateOfLesson = oneTimeSheetDetail.DateOfLesson,
                    officialTimeIn = oneTimeSheetDetail.OfficialTimeInMinutes,
                    officialTimeOut = oneTimeSheetDetail.OfficialTimeOutMinutes,
                    actualTimeIn = oneTimeSheetDetail.TimeInInMinutes,
                    actualTimeOut = oneTimeSheetDetail.TimeOutInMinutes,
                    wageRatePerHour = oneTimeSheetDetail.WageRatePerHour,
                    ratePerHour = oneTimeSheetDetail.RatePerHour,
                    customerAccountName = oneTimeSheetDetail.AccountName,
                    sessionSynopsisNames = oneTimeSheetDetail.SessionSynopsisNames
                });
            }//end of foreach loop which builds the timeSheetDetailList List container .
            response = new
            {
                timeSheet = oneTimeSheetData,
                timeSheetDetails = sortedTimeSheetDetailList

            };

            return new JsonResult(response);

        }//End of GetTimeSheetAndTimeSheetDetails


        //POST api/CreateTimeSheetData
        [HttpPost("CreateTimeSheetData")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTimeSheetData(IFormCollection inFormData)
        {
            string customMessage = "";
            //Obtain the user id of the user who has logon

            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;

            DateTime timeSheetMonthAndYear = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            //Check if timesheet data already exist for the particular current month
            var timeSheetQueryResults = Database.TimeSheets.Where(ts => (ts.MonthAndYear == timeSheetMonthAndYear) && (ts.InstructorId == userInfoId)).ToList();

            if ((timeSheetQueryResults.Count) != 0)
            {
                object httpFailRequestResultMessage = new { message = String.Format("There is an existing time sheet data for month {0} year {1}",
               timeSheetMonthAndYear.ToString("MMM", CultureInfo.InvariantCulture),
               timeSheetMonthAndYear.Year)
                };
                //Return a bad http request message to the client
                return BadRequest(httpFailRequestResultMessage);
            }
            //End of checking

            TimeSheet oneNewTimeSheet = new TimeSheet();
            oneNewTimeSheet.TimeSheetDetails = new List<TimeSheetDetail>();

            oneNewTimeSheet.CreatedById = userInfoId;
            oneNewTimeSheet.InstructorId = userInfoId;

            oneNewTimeSheet.CreatedAt = DateTime.Now;
            oneNewTimeSheet.UpdatedAt = DateTime.Now;
            //If the instructor creates the timesheet parent record on 10 June 2018, the MonthAndYear property should
            //hold 2018/06/1 DateTime value
            oneNewTimeSheet.MonthAndYear = timeSheetMonthAndYear;

            List<Int64> accountDetailIdList = new List<Int64>();
            foreach(string value in inFormData["accountDetailIds[]"])
            {
                accountDetailIdList.Add(Int64.Parse(value));
            }
          


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
                    if (accountDetailIdList.Contains(oneAccountDetail.AccountDetailId)){
                        DayOfWeek dayOfWeek = (DayOfWeek)Enum.ToObject(typeof(DayOfWeek), (oneAccountDetail.DayOfWeekNumber - 1));
                        List<DateTime> dateList = GetDates(timeSheetMonthAndYear.Year, timeSheetMonthAndYear.Month, dayOfWeek);
                        foreach (DateTime oneDate in dateList)
                        {
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
                                TimeInInMinutes = null,
                                TimeOutInMinutes = null,
                                RatePerHour = accountRate.RatePerHour,
                                WageRatePerHour = oneInstructorAccountData.WageRate,
                                IsReplacementInstructor = false,
                                SessionSynopsisNames = "",
                                CreatedByName = "system",
                                UpdatedByName = ""
                            });
                        }
                        }
              
                }//foreach

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
                message = String.Format("Created new time sheet data for month {0} year {1}",
                oneNewTimeSheet.MonthAndYear.ToString("MMM", CultureInfo.InvariantCulture), 
                oneNewTimeSheet.MonthAndYear.Year)
        };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of CreateTimeSheetData() method


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
        }//ConvertHHMMToMinutes() method

        //POST api/UpdateTimeInTimeOutData
        [HttpPut("UpdateTimeInTimeOutData")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateTimeInTimeOutData(IFormCollection inFormData)
        {
            string customMessage = "";

            //Obtain the user id of the user who has logon
            long timeSheetDetailId = Int64.Parse(inFormData["timeSheetDetailId"]);
            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;
            var oneTimeSheetDetail = Database.TimeSheetDetails
                  .Where(input => input.TimeSheetDetailId == timeSheetDetailId)
                  .AsNoTracking().Single();
            oneTimeSheetDetail.TimeInInMinutes = ConvertHHMMToMinutes(inFormData["actualTimeIn"]);
            //The incoming end time information is in HH:MM format from the client-side. Need to do conversion
            //to total minutes from midnight
            oneTimeSheetDetail.TimeOutInMinutes = ConvertHHMMToMinutes(inFormData["actualTimeOut"]);
            try
            {
                Database.TimeSheetDetails.Update(oneTimeSheetDetail);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                object httpFailRequestResultMessage = new { message = ex.InnerException.Message };
                //Return a bad http request message to the client
                return BadRequest(httpFailRequestResultMessage);

            }//End of try .. catch block on update data
             //Construct a custom message for the client
             //Create a success message anonymous object which has a 
             //Message member variable (property)
            var successRequestResultMessage = new
            {
                message ="Updated time in time out data."
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of UpdateTimeInTimeOutData() method


        //POST api/CheckAvailableTimeSheetData
        [HttpPost("CheckAvailableTimeSheetData")]
        [ValidateAntiForgeryToken]
        public IActionResult CheckAvailableTimeSheetData(IFormCollection inFormData)
        {
            object responseData = "";

            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;

            DateTime timeSheetMonthAndYear = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            //Check if timesheet data already exist for the particular current month
            var timeSheetQueryResults = Database.TimeSheets.Where(ts => (ts.MonthAndYear == timeSheetMonthAndYear) && (ts.InstructorId == userInfoId)).ToList();

            if ((timeSheetQueryResults.Count) != 0)
            {
                responseData = new
                {
                    message = new { isTimeSheetDataFound = true }
                };

            }
            else
            {
                responseData = new
                {
                    message = new { isTimeSheetDataFound = false }
                };

                //Create a OkObjectResult class instance, httpOkResult.
                //When creating the object, provide the previous message object into it.

            }
            //End of checking
            OkObjectResult httpOkResult =
            new OkObjectResult(responseData);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;

        }//End of CheckAvailableTimeSheetData() method

        //ConvertFromMinutesToHHMM
        string ConvertFromMinutesToHHMM(int inMinutes)
        {  //http://stackoverflow.com/questions/13044603/convert-time-span-value-to-format-hhmm-am-pm-using-c-sharp
            TimeSpan timespan = new TimeSpan(00, inMinutes, 00);
            DateTime time = DateTime.Today.Add(timespan);
            string formattedTime = time.ToString("hh:mm tt");
            return formattedTime;
        }//end of ConvertFromMinutesToHHMM

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
    }
}
