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
    [Route("api/[controller]")]
    public class UserInfoController : Controller
    {

				private readonly UserManager<ApplicationUser> _userManager;
				private readonly SignInManager<ApplicationUser> _signInManager;
				private readonly IEmailSender _emailSender;
				private readonly ISmsSender _smsSender;
				private readonly ILogger _logger;

				public ApplicationDbContext Database { get; }
				public IConfigurationRoot Configuration { get; }
	  		public UserInfoController(UserManager<ApplicationUser> userManager,
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


				// GET: api/values
				[HttpGet("GetAllActiveInstructorData")]
        public IActionResult GetAllActiveInstructorData()
        {
						List<object> instructorList = new List<object>();
						var instructorsQueryResults = Database.UserInfo
										 .Where(input => input.IsActive == true).AsNoTracking().ToList<UserInfo>();
						//After obtaining all the session synopsis records from the database,
						//the sessionSynopsisQueryResults will become a container holding these SessionSynopsis entity objects.
						//I need to loop through each  SessionSynopsis instance inside sessionSynopsisQueryResults
						//so that I can build a sessionSynopsisList which contains anonymous objects.
						foreach (var oneInstructor in instructorsQueryResults)
						{
								instructorList.Add(new
								{
										instructorId = oneInstructor.UserInfoId,
										fullName = oneInstructor.FullName
								});
						}//end of foreach loop which builds the List container .
						 //Use the JsonResult class to create a new JsonResult instance by using the instructorList.
						 //The ASP.NET framework will do the rest to translate it into a string JSON structured content
						 //which can travel through the Internet wire to the client browser.
						 //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
						return new JsonResult(instructorList);
				}

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
