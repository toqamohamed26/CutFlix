using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CUTFLI.Models;
using AutoMapper;
using CUTFLI.ViewModels;
using CUTFLI.Enums;
using System.Security.Claims;
using NToastNotify;
using Microsoft.AspNetCore.Authorization;

namespace CUTFLI.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly CUTFLIDbContext _context;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(CUTFLIDbContext context,
            IMapper mapper,
            IToastNotification toastNotification,
            ILogger<AppointmentsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTime? currentDate)
        {
            try
            {

                int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                var appointments = new List<Appointment>();
                if (currentDate == null)
                {
                    ViewData["value"] = DateTime.Now.ToString("yyyy-MM-dd");
                    if (currentUserRole == "Admin")
                    {
                        appointments = await _context.Appointments.Where(x => x.Date.Date == DateTime.Now.Date)
                                           .Include(a => a.People).Include(a => a.User).Include(x => x.Service).ToListAsync();
                    }
                    else
                    {
                        appointments = await _context.Appointments.Where(x => x.Date.Date == DateTime.Now.Date && x.UserId == currentUserId)
                                           .Include(a => a.People).Include(a => a.User).Include(x => x.Service).ToListAsync();
                    }
                }
                else
                {
                    ViewData["value"] = currentDate?.ToString("yyyy-MM-dd");
                    if (currentUserRole == "Admin")
                    {
                        appointments = await _context.Appointments.Where(x => x.Date.Date == currentDate.Value.Date)
                                           .Include(a => a.People).Include(a => a.User).ToListAsync();
                    }
                    else
                    {
                        appointments = await _context.Appointments.Where(x => x.Date.Date == currentDate.Value.Date && x.UserId == currentUserId)
                                           .Include(a => a.People).Include(a => a.User).ToListAsync();
                    }

                }

                var appointmentsModel = _mapper.Map<List<Appointment>, List<AppointmentViewModel>>(appointments);
                return View(appointmentsModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "AppointmentsController-Index exception :");
                return View(new List<AppointmentViewModel>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null || _context.Appointments == null)
                {
                    return NotFound();
                }

                var appointment = await _context.Appointments
                    .Include(a => a.People)
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (appointment == null)
                {
                    return NotFound();
                }
                var appointmentsModel = _mapper.Map<Appointment, AppointmentViewModel>(appointment);
                return View(appointmentsModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "AppointmentsController-Details exception :");
                return View(new List<AppointmentViewModel>());
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (currentUserRole != "Admin")
                {
                    ViewData["UserId"] = new SelectList(await _context.Users.Where(x => x.Id == currentUserId).ToListAsync(), "Id", "FullName");
                }
                else
                {
                    ViewData["UserId"] = new SelectList(await _context.Users.Where(x => x.Permission != SystemEnums.Permission.Admin).ToListAsync(), "Id", "FullName");
                }
                return View();
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "AppointmentsController-Create exception :");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel appointmentModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var appointment = _mapper.Map<AppointmentViewModel, Appointment>(appointmentModel);
                    appointment.CreatedBy = currentUserId;
                    appointment.CreatedDate = DateTime.Now;
                    appointment.Status = SystemEnums.AppointmentStatus.Available;
                    appointment.Price = 0;
                    await _context.AddAsync(appointment);
                    await _context.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Appointment Added successfully");
                    return RedirectToAction(nameof(Index));
                }
                ViewData["VistiorId"] = new SelectList(await _context.Peoples.ToListAsync(), "Id", "FullName", appointmentModel.VistiorId);
                ViewData["UserId"] = new SelectList(await _context.Users.ToListAsync(), "Id", "FullName", appointmentModel.UserId);
                return View(appointmentModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "AppointmentsController-Create exception :");
                return View(appointmentModel);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null || _context.Appointments == null)
                {
                    return NotFound();
                }

                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }
                int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (currentUserRole != "Admin")
                {
                    if (appointment.UserId != currentUserId)
                    {
                        return Unauthorized();
                    }
                    ViewData["UserId"] = new SelectList(await _context.Users.Where(x => x.Id == currentUserId).ToListAsync(), "Id", "FullName", appointment.UserId);
                    ViewData["VistiorId"] = new SelectList(await _context.Peoples.Where(x => x.Id == appointment.VistiorId).ToListAsync(), "Id", "FullName");
                }
                else
                {
                    ViewData["UserId"] = new SelectList(await _context.Users.Where(x => x.Permission != SystemEnums.Permission.Admin).ToListAsync(), "Id", "FullName", appointment.UserId);
                    ViewData["VistiorId"] = new SelectList(await _context.Peoples.ToListAsync(), "Id", "FullName", appointment.VistiorId);
                }
                var appointmentsModel = _mapper.Map<Appointment, AppointmentViewModel>(appointment);
                return View(appointmentsModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "AppointmentsController-Edit exception :");
                return View(new List<AppointmentViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppointmentViewModel appointmentModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var appointment = await _context.Appointments.FindAsync(appointmentModel.Id);
                    if (appointment != null)
                    {
                        int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                        string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                        if (currentUserRole != "Admin")
                        {
                            if (appointment.UserId != currentUserId)
                            {
                                return Unauthorized();
                            }
                        }
                        appointment.UserId = appointmentModel.UserId;
                        appointment.Date = appointmentModel.Date;
                        appointment.StartTime = appointmentModel.StartTime;
                        appointment.EndTime = appointmentModel.EndTime;
                        appointment.VistiorId = appointmentModel.VistiorId;
                        appointment.Status = appointmentModel.Status;
                        if (appointment.VistiorId != null && appointment.Status == SystemEnums.AppointmentStatus.Available)
                        {
                            appointment.Status = SystemEnums.AppointmentStatus.Checked;
                        }
                        appointment.PaymentMethod = appointmentModel.PaymentMethod;
                        appointment.Price = appointmentModel.Price;
                        await _context.SaveChangesAsync();
                        _toastNotification.AddSuccessToastMessage("Appointment updated successfully");
                        return RedirectToAction(nameof(Index));
                    }
                    return View(appointmentModel);
                }
                ViewData["VistiorId"] = new SelectList(_context.Peoples, "Id", "FullName", appointmentModel.VistiorId);
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", appointmentModel.UserId);
                return View(appointmentModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "AppointmentsController-Edit exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            var appointmentModel = _mapper.Map<AppointmentViewModel>(appointment);
            return View(appointmentModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.Appointments == null)
                {
                    return Problem("Entity set 'CUTFLIDbContext.Appointments'  is null.");
                }
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment != null)
                {
                    int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    if (currentUserRole != "Admin")
                    {
                        if (appointment.UserId != currentUserId)
                        {
                            return Unauthorized();
                        }
                    }
                    _context.Appointments.Remove(appointment);
                }

                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("Appointment deleted successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "AppointmentsController-DeleteConfirmed exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
