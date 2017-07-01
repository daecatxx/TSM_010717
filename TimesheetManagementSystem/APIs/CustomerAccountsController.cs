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
    public class CustomerAccountsController : Controller
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
        public CustomerAccountsController(UserManager<ApplicationUser> userManager,
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

        // GET: api/CustomerAccounts/GetCustomerAccounts
        [HttpGet("GetCustomerAccounts")]
        public JsonResult GetCustomerAccounts()
        {
            //var currentDateTime = DateTime.Now;

            //var ar = Database.AccountRates
            //    .Where(input => input.EffectiveStartDate <= currentDateTime).ToList();

            List<object> accountList = new List<object>();
            var accountsQueryResult = Database.CustomerAccounts
                     .Include(customerAccount => customerAccount.CreatedBy)
                     .Include(customerAccount => customerAccount.UpdatedBy)
                     .Include(customerAccount => customerAccount.InstructorAccounts)
                     .Include(customerAccount => customerAccount.AccountRates);
            //.Include(customerAccount => customerAccount.AccountRates);

            //.Include(ar);
            //.Where(customerAccount => customerAccount.AccountRates.EffectiveStartDate 
            //<= currentDateTime);

            //After obtaining all the customer account records from the database,
            //the accountList will become a container holding these CustomerAccount entity objects.
            //I need to loop through each  CustomerAccount instance inside accountsQueryResult
            //so that I can build a accountList which contains anonymous objects.
            foreach (var oneAccount in accountsQueryResult)
            ////.Where(ad => ad.AccountRates.EffectiveStartDate <= currentDateTime))
            {
                //{
                //    List<AccountRate> accountRateList = oneAccount.AccountRates
                //    .Where(ad => ad.EffectiveStartDate <= currentDateTime).ToList();

                //    foreach (var oneAccountRate in accountRateList)
                //    {

                accountList.Add(new
                {
                    customerAccountId = oneAccount.CustomerAccountId,
                    accountName = oneAccount.AccountName,
                    //comments = oneAccount.Comments,
                    //updatedBy = oneAccount.UpdatedBy.FullName,
                    //updatedAt = oneAccount.UpdatedAt,
                    //createdBy = oneAccount.CreatedBy.FullName,
                    //createdAt = oneAccount.CreatedAt,
                    //numOfInstructorsInvolved = oneAccount.InstructorAccounts.Count,
                    ////numOfRatesData = oneAccount.AccountRates.Count,
                    //isVisible = oneAccount.IsVisible,
                    ////ratePerHour = oneAccountRate.RatePerHour
                    ////accountRates = oneAccount.AccountRates.Select(input => new { ratePerHour = input.RatePerHour, effectiveStartDate = input.EffectiveStartDate })
                    //ratePerHour = oneAccountRate.RatePerHour

                    //accountRates = oneAccount.AccountRates.Select(input => new { ratePerHour = input.RatePerHour, effectiveStartDate = input.EffectiveStartDate })
                });
            }
            return new JsonResult(accountList);
        }//end of foreach loop which builds the accountList List container .
         //Use the JsonResult class to create a new JsonResult instance by using the accountList.
         //The ASP.NET framework will do the rest to translate it into a string JSON structured content
         //which can travel through the Internet wire to the client browser.
         //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
         //}
         //var response = new
         //{

        //    accountList = accountList,
        //   // accountRateList = 

        //};



        //end of GetCustomerAccounts()



        // GET: api/CustomerAccounts/GetCustomerAccounts
        [HttpGet("GetCustomerAccountsWithAccountRate")]
        public JsonResult GetCustomerAccountsWithAccountRate()
        {
            var currentDateTime = DateTime.Now;

            var ar = Database.AccountRates
                .Where(input => input.EffectiveStartDate <= currentDateTime).ToList();

            List<object> accountList = new List<object>();
            var accountsQueryResult = Database.CustomerAccounts
                     .Include(customerAccount => customerAccount.CreatedBy)
                     .Include(customerAccount => customerAccount.UpdatedBy)
                     .Include(customerAccount => customerAccount.InstructorAccounts)
                     .Include(customerAccount => customerAccount.AccountRates);
            //.Include(customerAccount => customerAccount.AccountRates);

            //.Include(ar);
            //.Where(customerAccount => customerAccount.AccountRates.EffectiveStartDate 
            //<= currentDateTime);

            //After obtaining all the customer account records from the database,
            //the accountList will become a container holding these CustomerAccount entity objects.
            //I need to loop through each  CustomerAccount instance inside accountsQueryResult
            //so that I can build a accountList which contains anonymous objects.
            foreach (var oneAccount in accountsQueryResult)
            //.Where(ad => ad.AccountRates.EffectiveStartDate <= currentDateTime))

            {
                List<AccountRate> accountRateList = oneAccount.AccountRates
                .Where(ad => ad.EffectiveStartDate <= currentDateTime).ToList();

                foreach (var oneAccountRate in accountRateList)
                {

                    accountList.Add(new
                    {
                        customerAccountId = oneAccount.CustomerAccountId,
                        accountName = oneAccount.AccountName,
                        comments = oneAccount.Comments,
                        updatedBy = oneAccount.UpdatedBy.FullName,
                        updatedAt = oneAccount.UpdatedAt,
                        createdBy = oneAccount.CreatedBy.FullName,
                        createdAt = oneAccount.CreatedAt,
                        numOfInstructorsInvolved = oneAccount.InstructorAccounts.Count,
                        //numOfRatesData = oneAccount.AccountRates.Count,
                        isVisible = oneAccount.IsVisible,
                        //ratePerHour = oneAccountRate.RatePerHour
                        //accountRates = oneAccount.AccountRates.Select(input => new { ratePerHour = input.RatePerHour, effectiveStartDate = input.EffectiveStartDate })
                        ratePerHour = oneAccountRate.RatePerHour

                        //accountRates = oneAccount.AccountRates.Select(input => new { ratePerHour = input.RatePerHour, effectiveStartDate = input.EffectiveStartDate })
                    });

                }//end of foreach loop which builds the accountList List container .
                 //Use the JsonResult class to create a new JsonResult instance by using the accountList.
                 //The ASP.NET framework will do the rest to translate it into a string JSON structured content
                 //which can travel through the Internet wire to the client browser.
                 //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            }
            //var response = new
            //{

            //    accountList = accountList,
            //   // accountRateList = 

            //};


            return new JsonResult(accountList);
        }//end of GetCustomerAccounts()



        //public decimal getCurrentAccountRateByCustomerAccountId(int customerAccountId, /*List<CustomerAccount> inCustomerAccountList,*/
        //    AccountRate inOneAccountRate, List<AccountRate> inAccountRateList, out object result)
        //{

        //    var accountRateQueryResult = Database.AccountRates


        //            //.Include(input => input.CustomerAccount)
        //            ////.Where(input => input.CustomerAccountId == customerAccountId)
        //            //.Find(input => ((input.inOneAccountRate.CustomerAccountId == customerAccountId)
        //            //&& x.DayOfWeekNumber == inOneAccountDetail.DayOfWeekNumber)
        //            //&& (((x.EffectiveStartDate <= endDate)
        //          //  && (ar => ar.EffectiveStartDate <= timeSheetMonthAndYear


        //}




        // GET: api/CustomerAccounts/GetCustomerAccounts
        [HttpGet("GetAccountRates/{inCustomerAccountId}")]
        public JsonResult GetAccountRates(int inCustomerAccountId)
        {
            List<object> accountRateList = new List<object>();
            var accountRatesQueryResult = Database.AccountRates
                .Where(input => input.CustomerAccountId == inCustomerAccountId);
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
            foreach (var oneAccountRate in accountRatesQueryResult)
            {
                accountRateList.Add(new
                {
                    accountRateId = oneAccountRate.AccountRateId,
                    ratePerHour = oneAccountRate.RatePerHour,
                    effectiveStartDate = oneAccountRate.EffectiveStartDate,
                    effectiveEndDate = oneAccountRate.EffectiveEndDate,
                });
            }//end of foreach loop which builds the accountRateList List container .

            var response = new
            {
                account = customerAccount,
                accountRateList = accountRateList
            };

            //Use the JsonResult class to create a new JsonResult instance by using the accountList.
            //The ASP.NET framework will do the rest to translate it into a string JSON structured content
            //which can travel through the Internet wire to the client browser.
            //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            return new JsonResult(response);
        }//end of GetAccountRates()

        // GET: api/CustomerAccounts/GetCustomerAccounts
        [HttpGet("GetOneAccountRate/{inAccountRateId}")]
        public JsonResult GetOneAccountRate(int inAccountRateId)
        {
            List<object> accountRateList = new List<object>();
            var foundOneAccountRate = Database.AccountRates
                .Where(input => input.AccountRateId == inAccountRateId).AsNoTracking().Single();

            var response = new
            {
                accountRateId = foundOneAccountRate.AccountRateId,
                customerAccountId = foundOneAccountRate.CustomerAccountId,
                ratePerHour = foundOneAccountRate.RatePerHour,
                effectiveStartDate = foundOneAccountRate.EffectiveStartDate,
                effectiveEndDate = foundOneAccountRate.EffectiveEndDate,
            };




            //Use the JsonResult class to create a new JsonResult instance by using the accountList.
            //The ASP.NET framework will do the rest to translate it into a string JSON structured content
            //which can travel through the Internet wire to the client browser.
            //https://google.github.io/styleguide/jsoncstyleguide.xml#Property_Name_Format
            return new JsonResult(response);
        }//end of GetOneAccountRate


        //POST api/CreateOneCustomerAccount
        [HttpPost("CreateOneCustomerAccount")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateOneCustomerAccount(IFormCollection inFormData)
        {
            string customMessage = "";
            //Obtain the user id of the user who has logon

            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;

            CustomerAccount oneNewAccount = new CustomerAccount();
            AccountRate oneNewAccountRate = new AccountRate();
            //After creating a new CustomerAccount and AccountRate type instances, 
            //fill them up with the incoming values
            oneNewAccount.AccountName = inFormData["accountName"];
            oneNewAccount.Comments = inFormData["comments"];
            oneNewAccount.CreatedById = userInfoId;
            oneNewAccount.UpdatedById = userInfoId;

            oneNewAccountRate.EffectiveStartDate = DateTime.ParseExact(inFormData["effectiveStartDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            if (inFormData["effectiveEndDate"] != "")
            {
                //If the there are values for the effectiveEndDate then start copying into the EffectiveEndDate property
                oneNewAccountRate.EffectiveEndDate = DateTime.ParseExact(inFormData["effectiveEndDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            }

            oneNewAccountRate.RatePerHour = Decimal.Parse(inFormData["ratePerHour"]);

            //---------------- Security and Authorization code ----(Start)------------------------------
            //When the record is created, the CreatedById and the UpdatedById property
            //has the same value.
            //oneNewAccount.CreatedById = (await _userManager.GetUserAsync(HttpContext.User))?.UserName;
            //oneNewAccount.UpdatedById = _userManager.GetUserId(User);
            //Don't need oneNewAccount.CreatedAt = DateTime.Now; because there is 
            //a default value using GETDATE() setup in the database.
            //---------------- Security and Authorization code ------(End)------------------------------
            try
            {
                Database.CustomerAccounts.Add(oneNewAccount);
                //Assign the generated customer account id value into the AccountRate instance
                oneNewAccountRate.CustomerAccountId = oneNewAccount.CustomerAccountId;
                Database.AccountRates.Add(oneNewAccountRate);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message
                                        .Contains("CustomerAccount_AccountName_UniqueConstraint") == true)
                {
                    customMessage = "Unable to account record due " +
                                "to another record having the same account name : " +
                    oneNewAccount.AccountName;
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
                message = "Saved one account record"
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of CreateOneCustomerAccount() method

        //POST api/UpdateCustomerAccount
        [HttpPut("UpdateCustomerAccount/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCustomerAccount(IFormCollection inFormData, int id)
        {
            string customMessage = "";
            //Obtain the user id of the user who has logon

            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;

            CustomerAccount foundOneAccount = Database.CustomerAccounts.Single(input => input.CustomerAccountId == id);

            //After creating a new Account type instance, fill it up with the incoming values
            foundOneAccount.AccountName = inFormData["accountName"];
            foundOneAccount.IsVisible = bool.Parse(inFormData["isVisible"]);
            /*
            foundOneAccount.EffectiveStartDate = DateTime.ParseExact(inFormData["effectiveStartDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            if (inFormData["effectiveEndDate"] != "")
            {
                //If the there are values for the effectiveEndDate then start copying into the EffectiveEndDate property
                foundOneAccount.EffectiveEndDate = DateTime.ParseExact(inFormData["effectiveEndDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                foundOneAccount.EffectiveEndDate = null;
            }
             foundOneAccount.RatePerHour = Decimal.Parse(inFormData["ratePerHour"]);
            */

            foundOneAccount.Comments = inFormData["comments"];
            foundOneAccount.UpdatedById = userInfoId;
            foundOneAccount.UpdatedAt = DateTime.Now;
            try
            {
                Database.CustomerAccounts.Update(foundOneAccount);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message
                                        .Contains("CustomerAccount_AccountName_UniqueConstraint") == true)
                {
                    customMessage = "Unable to account record due " +
                                "to another record having the same account name : " +
                    foundOneAccount.AccountName;
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
                message = "Saved account record"
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of UpdateCustomerAccount() method

        // DELETE api/CustomerAccounts/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            string customMessage = "";

            try
            {
                var foundOneAccount = Database.CustomerAccounts
                .Single(account => account.CustomerAccountId == id);


                //Update the database model
                Database.CustomerAccounts.Remove(foundOneAccount);
                //Tell the db model to commit/persist the changes to the database, 
                //I use the following command.
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                customMessage = "Unable to delete account record.";
                object httpFailRequestResultMessage = new { message = customMessage };
                //Return a bad http request message to the client
                return BadRequest(httpFailRequestResultMessage);
            }//End of try .. catch block on manage data

            //Build a custom message for the client
            //Create a success message anonymous object which has a 
            //Message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Deleted account record"
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                                                            new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;

        }
        // DELETE api/CustomerAccounts/DeleteOneAccountRate/5
        [HttpDelete("DeleteOneAccountRate/{inAccountRateId}")]
        public IActionResult DeleteOneAccountRate(int inAccountRateId)
        {
            string customMessage = "";

            try
            {
                var foundOneAccountRate = Database.AccountRates
                .Single(accountRate => accountRate.AccountRateId == inAccountRateId);


                //Update the database model
                Database.AccountRates.Remove(foundOneAccountRate);
                //Tell the db model to commit/persist the changes to the database, 
                //I use the following command.
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                customMessage = "Unable to delete rate record.";
                object httpFailRequestResultMessage = new { message = customMessage };
                //Return a bad http request message to the client
                return BadRequest(httpFailRequestResultMessage);
            }//End of try .. catch block on manage data

            //Build a custom message for the client
            //Create a success message anonymous object which has a 
            //Message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Deleted rate record"
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;

        }//End of DeleteOneAccountRate

        [HttpGet("GetAccountsByInstructorId/{inuserinfoid}")]
        public JsonResult GetAccountsByInstructorId(int inUserInfoId)
        {
            List<object> accountList = new List<object>();
            object dataSummary = new object();
            //The following command will create an object, foundOneInstructor
            //which represents the Instructor entity object which matches the given instructor id
            //provided by the input parameter, inInstructorId.
            //This foundOneInstructor object is needed because the FullName is required
            //in the custom JSON content resultset.
            var foundOneUser = Database.UserInfo
               .Where(user => user.UserInfoId == inUserInfoId).AsNoTracking().Single();

            //The following command will create an object, accountsQueryResult
            //which will only have one internal list of Account entities.
            //These Student entities has inInstructorId property which meets the search
            //criteria given by the inInstructorId input parameter.
            var accountsQueryResult = Database.InstructorAccounts
                 .Where(instructorAccount => (instructorAccount.InstructorId == inUserInfoId))
                 .Include(instructorAccount => instructorAccount.CustomerAccount)
                 .Include(instructorAccount => instructorAccount.Instructor);

            //The following foreach loop aims to create a
            //into a single List of anonymous objects, accountList.        
            foreach (var oneAccount in accountsQueryResult)
            {
                //Create new anonymous object and add into the accountList
                //container.
                accountList.Add(new
                {
                    accountId = oneAccount.CustomerAccount.CustomerAccountId,
                    accountName = oneAccount.CustomerAccount.AccountName,
                    effectiveStartDate = oneAccount.EffectiveStartDate,
                    effectiveEndDate = oneAccount.EffectiveEndDate,
                    //isCurrent = oneAccount.IsCurrent,
                    wageRatePerHour = oneAccount.WageRate
                });
            }//end of the foreach block
            if (accountsQueryResult.ToList().Count == 0)
            {
                dataSummary = new
                {
                    fullName = foundOneUser.FullName,
                    accountList = accountList,
                    message = "There are no accounts found for this instructor."
                };
            }
            else
            {
                dataSummary = new
                {
                    fullName = foundOneUser.FullName,
                    accountList = accountList,
                    message = string.Format("There are {0} accounts associated to {1} instructor.",
                           accountList.Count.ToString(), foundOneUser.FullName)
                };
            }//end if
            return new JsonResult(dataSummary);
        }//End of GetStudentListByCourse()
        [HttpGet("GetOneCustomerAccount/{id}")]
        public JsonResult GetOneCustomerAccount(int id)
        {
            var currentDateTime = DateTime.Now;

            var foundOneAccount = Database.CustomerAccounts.AsNoTracking()
                  .Where(account => account.CustomerAccountId == id)
                  .Include(input => input.AccountRates).Single();
            // .Where(accountRate => accountRate.EffectiveStart).Single();
            List<object> accountRateList = new List<object>();
            foreach (var oneAccountRate in foundOneAccount.AccountRates
                    .OrderByDescending(input => input.EffectiveStartDate)
                    .Where(input => input.EffectiveStartDate <= currentDateTime))
            {
                accountRateList.Add(new
                {
                    accountRateId = oneAccountRate.AccountRateId,
                    ratePerHour = oneAccountRate.RatePerHour,
                    effectiveStartDate = oneAccountRate.EffectiveEndDate,
                    effectiveEndDate = oneAccountRate.EffectiveEndDate
                });
            }
            var response = new
            {
                customerAccountId = foundOneAccount.CustomerAccountId,
                accountName = foundOneAccount.AccountName,
                isVisible = foundOneAccount.IsVisible,
                comments = foundOneAccount.Comments,
                accountRates = accountRateList
            };
            return new JsonResult(response);
        }//End of GetOneCustomerAccount

        //POST api/CreateAccountRate
        [HttpPost("CreateAccountRate")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAccountRate(IFormCollection inFormData)
        {
            string customMessage = "";
            //No need to fetch user id value because this AccountRate is not going to capture this value.

            AccountRate oneNewAccountRate = new AccountRate();
            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;
            oneNewAccountRate.EffectiveStartDate = DateTime.ParseExact(inFormData["effectiveStartDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            if (inFormData["effectiveEndDate"] != "")
            {
                //If the there are values for the effectiveEndDate then start copying into the EffectiveEndDate property
                oneNewAccountRate.EffectiveEndDate = DateTime.ParseExact(inFormData["effectiveEndDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            }

            oneNewAccountRate.RatePerHour = Decimal.Parse(inFormData["ratePerHour"]);
            oneNewAccountRate.CustomerAccountId = Int32.Parse(inFormData["customerAccountId"]);
            List<AccountRate> existingAccountRateList = Database.AccountRates
                .Where(input => input.CustomerAccountId == Int32.Parse(inFormData["customerAccountId"]))
                .AsNoTracking().ToList<AccountRate>();

            //Find the parent record to update the UpdatedAt property and UpdatedById property
            CustomerAccount foundParentCustomerAccount = Database.CustomerAccounts.Where(input => input.CustomerAccountId == oneNewAccountRate.CustomerAccountId).Single();
            foundParentCustomerAccount.UpdatedAt = DateTime.Now;
            foundParentCustomerAccount.UpdatedById = userInfoId;
            try
            {
                Database.AccountRates.Add(oneNewAccountRate);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                customMessage = "Unable to save rate information.";
                object httpFailRequestResultMessage = new { message = customMessage };
                //Return a bad http request message to the client
                return BadRequest(httpFailRequestResultMessage);

            }//End of try .. catch block on saving data
            object result;
            AreThereAnyOverlappingAccountRate(oneNewAccountRate, existingAccountRateList, out result);
            //Construct a custom message for the client
            //Create a success message anonymous object which has a 
            //message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Saved rate information.",
                additionalInfo = result
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of CreateAccountRate() method

        //PUT api/UpdateAccountRate/4
        [HttpPut("UpdateAccountRate/{inAccountRateId}")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAccountRate(IFormCollection inFormData, string inAccountRateId)
        {
            string customMessage = "";
            string userLoginId = _userManager.GetUserName(User);
            int userInfoId = Database.UserInfo.Single(input => input.LoginUserName == userLoginId).UserInfoId;
            int accountRateId = Int32.Parse(inFormData["accountRateId"]);
            //Fetch the correct entity object
            AccountRate foundOneAccountRate = Database.AccountRates.
                Where(input => input.AccountRateId == accountRateId).
                Single();

            foundOneAccountRate.EffectiveStartDate = DateTime.ParseExact(inFormData["effectiveStartDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            if (inFormData["effectiveEndDate"] != "")
            {
                //If the there are values for the effectiveEndDate then start copying into the EffectiveEndDate property
                foundOneAccountRate.EffectiveEndDate = DateTime.ParseExact(inFormData["effectiveEndDate"], "d/M/yyyy", CultureInfo.InvariantCulture);
            }
            if (inFormData["effectiveEndDate"] == "")
            {
                //If the there are no values for the effectiveEndDate then supply NULL value into the EffectiveEndDate property
                foundOneAccountRate.EffectiveEndDate = null;
            }

            foundOneAccountRate.RatePerHour = Decimal.Parse(inFormData["ratePerHour"]);
            //Find the parent record to update the UpdatedAt property and UpdatedById property
            CustomerAccount foundParentCustomerAccount = Database.CustomerAccounts.Where(input => input.CustomerAccountId == foundOneAccountRate.CustomerAccountId).Single();
            foundParentCustomerAccount.UpdatedAt = DateTime.Now;
            foundParentCustomerAccount.UpdatedById = userInfoId;
            try
            {
                Database.AccountRates.Update(foundOneAccountRate);
                Database.SaveChanges();
            }
            catch (Exception ex)
            {
                customMessage = "Unable to save rate information.";
                object httpFailRequestResultMessage = new { message = customMessage };
                //Return a bad http request message to the client
                return BadRequest(httpFailRequestResultMessage);

            }//End of try .. catch block on saving data
             //Construct a custom message for the client
             //Create a success message anonymous object which has a 
             //Message member variable (property)
            var successRequestResultMessage = new
            {
                message = "Saved rate information."
            };

            //Create a OkObjectResult class instance, httpOkResult.
            //When creating the object, provide the previous message object into it.
            OkObjectResult httpOkResult =
                        new OkObjectResult(successRequestResultMessage);
            //Send the OkObjectResult class object back to the client.
            return httpOkResult;
        }//End of UpdateAccountRate() method

        public bool AreThereAnyOverlappingAccountRate(AccountRate inOneAccountRate, List<AccountRate> inAccountRateList, out object result)
        {
            //The following two objects are used to build an anonymous type result
            var conflictingDataList = new List<object>();
            //Need to build this anonymous type object so that can be part of the JSON result to be returned later by the
            //calling program. The client-side JavaScript can use the information inside the intendedData to display meaningful
            //information.
            var intendedData = new
            {
                ratePerHour = inOneAccountRate.RatePerHour,
                effectiveStartDate = inOneAccountRate.EffectiveStartDate.ToString("dd/MM/yyyy"),
                effectiveEndDate = (inOneAccountRate.EffectiveEndDate != null ? inOneAccountRate.EffectiveEndDate.Value.ToString("dd/MM/yyyy") : "n/a"),
            };

            //The following endDate object is necessary because the user can provide an empty EffectiveEndDate.
            //Our AccountRate class's EffectiveEndDate is NULLABLE. That will create problems in the code when do searching.
            //The command below will make the endDate hold 2199, Dec 31st (If the system can last that long, it is a miracle)
            //http://stackoverflow.com/questions/36260060/compare-datetime-with-datetimeallows-null-values-in-c-sharp
            DateTime endDate = inOneAccountRate.EffectiveEndDate != null ? inOneAccountRate.EffectiveEndDate.Value : new DateTime(2199, 12, 31);
            //The following will search for all account rate information which shares overlapping start and
            //end date (including OPEN DATE, NULL) period.
            List<AccountRate> accountRateList = inAccountRateList
               .FindAll(x => (x.CustomerAccountId == inOneAccountRate.CustomerAccountId)
                    && (((x.EffectiveStartDate <= endDate)
                    && (x.EffectiveEndDate == null))
                    || ((x.EffectiveStartDate <= endDate)
                    && (x.EffectiveEndDate.GetValueOrDefault() >= inOneAccountRate.EffectiveStartDate))))
               .ToList();

            if (accountRateList.Count == 0)
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
                foreach (AccountRate oneAccountRate in accountRateList)
                {
                    var conflictingData = new
                    {
                        effectiveStartDate = oneAccountRate.EffectiveStartDate.ToString("dd/MM/yyyy"),
                        effectiveEndDate = (oneAccountRate.EffectiveEndDate != null ? oneAccountRate.EffectiveEndDate.GetValueOrDefault().ToString("dd/MM/yyyy") : "n/a"),
                        ratePerHour = oneAccountRate.RatePerHour
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
}
