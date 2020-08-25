using System;
using Microsoft.AspNetCore.Mvc;

namespace LotteryApp.Controllers
{
    public class RoomController : Controller
    {
        [Route("/room/{id}")]
        public IActionResult Index(string id)
        {
            if (id == default)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("/Views/Room/Index.cshtml", id);
        }
    }
}