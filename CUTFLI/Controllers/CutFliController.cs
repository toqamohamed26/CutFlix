using AutoMapper;
using CUTFLI.Enums;
using CUTFLI.Models;
using CUTFLI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using NToastNotify;
using static CUTFLI.Enums.SystemEnums;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace CUTFLI.Controllers
{
    public class CutFliController : Controller
    {
        private readonly CUTFLIDbContext _dbContext;
        private readonly IToastNotification _toastNotification;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CutFliController> _logger;
        public CutFliController(CUTFLIDbContext dbContext,
            IToastNotification toastNotification,
            IMapper mapper,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            ILogger<CutFliController> logger
            )
        {
            _dbContext = dbContext;
            _toastNotification = toastNotification;
            _mapper = mapper;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }


        public IActionResult GetVideo(string videoName)
        {
            try
            {
                _logger.LogInformation($"CutFliController-GetVideo videoName={videoName}");
                string folderName = "Videos\\" + videoName;
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, folderName);
                byte[] b = System.IO.File.ReadAllBytes(filePath);
                return File(b, "video/mp4");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"CutFliController-GetVideo exception :");
                return NotFound();
            }
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogDebug($"CutFliController-Index");
                var videoName = await _dbContext.Videos.FirstOrDefaultAsync();
                if (videoName != null)
                {
                    ViewBag.Video = videoName.VideoName;
                }
                else
                {
                    ViewBag.Video = "cutflix.MP4";
                }
                var employees = await _dbContext.Users.Where(x => x.Permission != SystemEnums.Permission.Admin).Skip(0).Take(4).ToListAsync();
                var model = _mapper.Map<List<User>, List<Barbers>>(employees);
                return View(model);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "CutFliController-Index exception :");
                return View(new List<Barbers>());
            }
        }
        public async Task<IActionResult> Appointments(int? service, DateTime? date)
        {
            try
            {
                _logger.LogDebug($"CutFliController-Appointments service={service} date={date}");
                if (service != null)
                {
                    var serviceDb = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == (int)service);
                    if (serviceDb != null)
                    {
                        HttpContext.Session.SetInt32("serviceId", (int)service);
                        HttpContext.Session.SetString("service", serviceDb.Name);
                        HttpContext.Session.SetString("servicePrice", serviceDb.Price.ToString());
                    }
                }
                ViewData["minValue"] = DateTime.Now.ToString("yyyy-MM-dd");
                if (date != null)
                {
                    if (date.Value.Date < DateTime.Now.Date)
                    {
                        _toastNotification.AddErrorToastMessage("Wrong date");
                        return View(new List<CustomerAppointments>());
                    }
                    date = date.Value.Date;
                    ViewData["value"] = date?.ToString("yyyy-MM-dd");
                }
                else
                {
                    ViewData["value"] = DateTime.Now.ToString("yyyy-MM-dd");
                    date = DateTime.Now.Date;
                }

                int barber = (int)HttpContext.Session.GetInt32("barberId");
                var appointments = await _dbContext.Appointments.Where(x => x.UserId == barber && x.Status == SystemEnums.AppointmentStatus.Available && x.Date.Date == date).Include(x => x.User).ToListAsync();

                var customerAppointments = _mapper.Map<List<Appointment>, List<CustomerAppointments>>(appointments);

                var response = new BookAppointments();
                response.CustomerAppointments = customerAppointments;
                response.People = new PeopleViewModel();

                return View(response);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "CutFliController-Appointments exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> GetAppointmentsByDate(string date)
        {
            try
            {
                _logger.LogDebug($"CutFliController-GetAppointmentsByDate date={date}");
                DateTime dateTime = Convert.ToDateTime(date);
                int? barber = (int)HttpContext.Session.GetInt32("barberId");
                var appointments = await _dbContext.Appointments.Where(x => x.UserId == barber && x.Status == SystemEnums.AppointmentStatus.Available && x.Date.Date == dateTime.Date).ToListAsync();
                var customerAppointments = _mapper.Map<List<Appointment>, List<CustomerAppointments>>(appointments);
                return PartialView("_Appointments", customerAppointments);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "CutFliController-GetAppointmentsByDate exception :");
                return PartialView("_Appointments", new List<CustomerAppointments>());
            }
        }
        public async Task<IActionResult> Barbers()
        {
            try
            {
                _logger.LogDebug($"CutFliController-Barbers");
                var employees = await _dbContext.Users.Where(x => x.Permission != SystemEnums.Permission.Admin).ToListAsync();
                if (employees != null)
                {
                    var model = _mapper.Map<List<User>, List<Barbers>>(employees);
                    return View(model);
                }
                return View(new List<Barbers>());
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "CutFliController-Barbers exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Services(int? barber)
        {
            try
            {
                _logger.LogDebug($"CutFliController-Services barber={barber}");
                if (barber != null)
                {
                    HttpContext.Session.SetInt32("barberId", (int)barber);
                    var services = await _dbContext.UserServices.Where(x => x.UserId == barber).Include(x => x.Service).Include(u => u.User).ToListAsync();
                    if (services.Count > 0)
                    {
                        HttpContext.Session.SetString("barber", services.FirstOrDefault().User.FullName);
                        var model = _mapper.Map<List<UserService>, List<UserServiceViewModel>>(services);
                        return View(model);
                    }
                }
                return View(new List<UserServiceViewModel>());
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "CutFliController-Services exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Prices()
        {
            try
            {
                _logger.LogDebug($"CutFliController-Prices");
                var services = await _dbContext.Services.ToListAsync();
                if (services != null)
                {
                    var model = _mapper.Map<List<Service>, List<ServiceViewModel>>(services);
                    return View(model);
                }
                return View(new List<ServiceViewModel>());
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "CutFliController-Prices exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ContactUs(ContactUsViewModel contactUsModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var contactUs = _mapper.Map<ContactUsViewModel, ContactUs>(contactUsModel);
                    contactUs.CreatedDate = DateTime.Now;
                    await _dbContext.AddAsync(contactUs);
                    await _dbContext.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Send Successfully");
                    return RedirectToAction(nameof(Index));
                }
                return PartialView("_ContactUs", contactUsModel);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(PeopleViewModel peopleModel)
        {
            try
            {
                _logger.LogDebug($"CutFliController-Book");
                if (ModelState.IsValid)
                {
                    int id = 0;
                    string email = string.Empty;
                    var customer = await _dbContext.Peoples.Where(x => x.PhoneNumber == peopleModel.PhoneNumber.Trim()).FirstOrDefaultAsync();
                    if (customer == null)
                    {
                        var newCustomer = _mapper.Map<PeopleViewModel, People>(peopleModel);
                        newCustomer.CreatedDate = DateTime.Now;
                        await _dbContext.AddAsync(newCustomer);
                        await _dbContext.SaveChangesAsync();
                        id = newCustomer.Id;
                        email = newCustomer.Email;
                    }
                    else
                    {
                        id = customer.Id;
                        if (customer.Email != peopleModel.Email.Trim())
                        {
                            customer.Email = peopleModel.Email.Trim();
                        }
                        email = customer.Email;
                    }
                    var appointment = await _dbContext.Appointments.FindAsync(peopleModel.AppointmentId);
                    if (appointment != null)
                    {
                        if (appointment.Status == AppointmentStatus.Available)
                        {
                            appointment.VistiorId = id;
                            appointment.Status = AppointmentStatus.Checked;
                            int? serviceId = HttpContext.Session.GetInt32("serviceId");
                            if (serviceId != null)
                            {
                                var service = await _dbContext.Services.FirstOrDefaultAsync(x => x.Id == serviceId);
                                if (service != null)
                                {
                                    appointment.ServiceId = serviceId;
                                    appointment.Price = service.Price;
                                }
                            }
                            await _dbContext.SaveChangesAsync();
                            _toastNotification.AddSuccessToastMessage("Appointment booked successfully");
                            HttpContext.Session.Clear();
                            await SendEmail(email, appointment.Date, appointment.StartTime, appointment.EndTime);
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                return PartialView("Book", peopleModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "CutFliController-Book exception :");
                return PartialView("Book", peopleModel);
            }
        }

        private async Task SendEmail(string consumer, DateTime date, TimeSpan srartTime, TimeSpan endTime)
        {
            try
            {
                string convertDate = date.ToString("D");
                string convertStartTime = new DateTime(srartTime.Ticks).ToString("hh:mm tt");
                var emailSetting = _configuration.GetSection("Email");
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(emailSetting["UserName"]));
                email.To.Add(MailboxAddress.Parse(consumer));

                email.Subject = "CUTFLIX - Appointment";
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = "<!DOCTYPE html>\r\n<html>\r\n\r\n<head>\r\n    <style>\r\n        /* Define CSS styles here */\r\n        body {\r\n            font-family: Arial, sans-serif;\r\n            background-color: #f2f2f2;\r\n            margin: 0;\r\n            padding: 0;\r\n        }\r\n\r\n        .container {\r\n            max-width: 600px;\r\n            margin: 0 auto;\r\n            background-color: rgba(0, 0, 0, 0.9);\r\n            padding: 20px 0px;\r\n            border-radius: 5px;\r\n            color: #f8f9fa;\r\n            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);\r\n        }\r\n\r\n        .header {\r\n            text-align: center;\r\n            padding: 20px 0;\r\n        }\r\n\r\n        .header img {\r\n            max-width: 150px;\r\n            height: auto;\r\n        }\r\n\r\n        .content {\r\n            padding: 20px;\r\n        }\r\n\r\n        .footer {\r\n            text-align: center;\r\n            padding: 20px 0;\r\n        }\r\n    </style>\r\n</head>"
                    + $"<body>\r\n    <div class=\"container\">\r\n        <div class=\"header\">\r\n            <img src=\"https://i.ibb.co/L9PwLyN/2-2.png\" alt=\"Company Logo\">\r\n        </div>\r\n        <div class=\"content\">\r\n            <h3>Welcome to cutflix salon .</h3>\r\n            <p>Your appointment date is : {convertDate} </p>\r\n            <p>Time : {convertStartTime} </p>\r\n        </div>\r\n        <div class=\"footer\">\r\n            <p>&copy; 2023 CutFlix. All rights reserved.</p>\r\n        </div>\r\n    </div>\r\n</body>\r\n</html>"
                };

                using var smtp = new SmtpClient();
                smtp.Connect(emailSetting["Server"], 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(emailSetting["UserName"], emailSetting["Password"]);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "CutFliController-SendEmail exception :");
            }
        }
    }
}
