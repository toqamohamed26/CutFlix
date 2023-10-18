using CUTFLI.ActionFilter;
using CUTFLI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Diagnostics;
using System.Linq;
using static CUTFLI.Enums.SystemEnums;

namespace CUTFLI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CUTFLIDbContext _cUTFLIDbContext;
        private readonly IToastNotification _toastNotification;

        public HomeController(ILogger<HomeController> logger,CUTFLIDbContext cUTFLIDbContext, IToastNotification toastNotification)
        {
            _logger = logger;
            _cUTFLIDbContext = cUTFLIDbContext;
            _toastNotification = toastNotification;
        }

        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogDebug($"HomeController-Index");
                ViewBag.Customers = await _cUTFLIDbContext.Peoples.CountAsync();
                ViewBag.Users = await _cUTFLIDbContext.Users.Where(x=> x.Permission != Permission.Admin).CountAsync();
                ViewBag.Appointments = await _cUTFLIDbContext.Appointments.CountAsync();
                ViewBag.Profit = await _cUTFLIDbContext.Appointments.Where(x => x.Status == AppointmentStatus.Complete).Select(x => x.Price).SumAsync();
                return View();
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "HomeController-Index exception :");
                return View();
            }
        }

        public IActionResult UserDahboard()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}