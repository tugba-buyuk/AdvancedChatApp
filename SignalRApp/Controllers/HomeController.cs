﻿using Microsoft.AspNetCore.Mvc;

namespace SignalRApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Chat()
        {
            return View();
        }
    }
}
