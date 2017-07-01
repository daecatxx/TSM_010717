using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TimeSheetManagementSystem.Controllers
{
    public class SessionSynopsesController : Controller
    {
        // SessionSynopses/Index
        public IActionResult Index()
        {
            return View();
        }
        // SessionSynopses/Create
        public IActionResult Create()
        {
            return View();
        }
        // SessionSynopses/Update
        public IActionResult Update()
        {
            return View();
        }
    }
}
