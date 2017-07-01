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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TimeSheetManagementSystem.APIs
{
    [Authorize]
    [Route("api/[controller]")]
    public class TimeSheetsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;

        public ApplicationDbContext Database { get; }
        public IConfigurationRoot Configuration { get; }

        //Create a Constructor, so that the .NET engine can pass in the ApplicationDbContext object
        //which represents the database session.
        public TimeSheetsController(UserManager<ApplicationUser> userManager,
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


        [HttpGet]
        public JsonResult Get()
        {
            List<object> timeSheetList = new List<object>();
            var timeSheetQueryResults = Database.TimeSheets
                     .Include(timeSheet => timeSheet.TimeSheetDetails)
                     .Include(timeSheet => timeSheet.CreatedBy);
            //After obtaining all the timesheet records from the database,
            //the timeSheetList will become a container holding these TimeSheet entity objects.
            //I need to loop through each  TimeSheet instance inside timeSheetQueryResults
            //so that I can build a timeSheetList which contains anonymous objects.
            foreach (var oneTimeSheet in timeSheetQueryResults)
            {
                timeSheetList.Add(new
                {
                    timeSheetId = oneTimeSheet.TimeSheetId,
                    instructorFullName = oneTimeSheet.CreatedBy.FullName,
                    createdAt = oneTimeSheet.CreatedAt,
                    numOfTimeSheetDetails = oneTimeSheet.TimeSheetDetails.Count,
                });
            }//end of foreach loop which builds the studentList List container .
             //Use the JsonResult class to create a new JsonResult instance by using the timeSheetList.
             //The ASP.NET framework will do the rest to translate it into a string JSON structured content
             //which can travel through the Internet wire to the client browser.
             //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            return new JsonResult(timeSheetList);
        }//end of Get()

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
