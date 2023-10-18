using AutoMapper;
using CUTFLI.ActionFilter;
using CUTFLI.Enums;
using CUTFLI.Models;
using CUTFLI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using NToastNotify;
using NuGet.Protocol.Plugins;
using System.Linq.Expressions;
using System.Security.Claims;
using static CUTFLI.Enums.SystemEnums;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace CUTFLI.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private List<string> allowedExtensions = new List<string>() { ".png", ".jpg" };
        private readonly CUTFLIDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IToastNotification _toastNotification;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersController> _logger;

        public UsersController(CUTFLIDbContext context,
            IMapper mapper,
            IPasswordHasher<User> passwordHasher,
            IToastNotification toastNotification, 
            IConfiguration configuration,
            ILogger<UsersController> logger)
        {
            _context = context;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _toastNotification = toastNotification;
            _configuration = configuration;
            _logger = logger;
        }

        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Index()
        {
            try
            {
                int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var users = await _context.Users.Where(x => x.Id != currentUserId).OrderByDescending(x => x.CreatedDate).ToListAsync();
                var userModel = _mapper.Map<List<User>, List<UserViewModel>>(users);
                return View(userModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "UsersController-Index exception :");
                return View(new List<UserViewModel>());
            }
        }

        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null || _context.Users == null)
                {
                    return NotFound();
                }

                var user = await _context.Users
                    .Where(m => m.Id == id).Include(x => x.UserServices).ThenInclude(x => x.Service).FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound();
                }
                var userModel = _mapper.Map<User, UserViewModel>(user);
                return View(userModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "UsersController-Details exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.UserServices = new SelectList(await _context.Services.ToListAsync(), "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "UsersController-Create exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Create(UserViewModel userModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_context.Users.Any(x => x.Email == userModel.Email.Trim()))
                    {
                        ModelState.AddModelError("Email", "Email is used");
                        return View(userModel);
                    }
                    else if (_context.Users.Any(x => x.PhoneNumber == userModel.PhoneNumber.Trim()))
                    {
                        ModelState.AddModelError("PhoneNumber", "PhoneNumber is used");
                        return View(userModel);
                    }
                    var files = Request.Form.Files;
                    using var stream = new MemoryStream();
                    if (files.Any())
                    {
                        var image = files.FirstOrDefault();
                        if (allowedExtensions.Contains(Path.GetExtension(image.FileName).ToLower()))
                        {
                            if (image.Length < 3145728) // 3 MB
                            {
                                await image.CopyToAsync(stream);
                            }
                            else
                            {
                                ModelState.AddModelError("Image", "Image can't be larger than 3 MB");
                                return View(userModel);
                            }
                        }
                    }
                    var user = _mapper.Map<UserViewModel, User>(userModel);
                    user.CreatedDate = DateTime.Now;
                    user.Permission = Permission.Employee;
                    user.Password = _passwordHasher.HashPassword(user, userModel.Password);
                    user.Image = files.Any() ? stream.ToArray() : null;
                    user.UserServices = new List<UserService>();
                    if (userModel.Services != null)
                    {
                        userModel.Services.ForEach(x =>
                        {
                            user.UserServices.Add(
                                new UserService()
                                {
                                    CreatedDate = DateTime.Now,
                                    ServiceId = x
                                }
                                );
                        });
                    }
                    await _context.AddAsync(user);
                    await _context.SaveChangesAsync();
                    await SendEmail(user.Email, userModel.Password);
                    _toastNotification.AddSuccessToastMessage("User Added successfully");
                    return RedirectToAction(nameof(Index));
                }
                return View(userModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "UsersController-Create exception :");
                return View(userModel);
            }
        }

        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null || _context.Users == null)
                {
                    return NotFound();
                }

                var user = await _context.Users.Where(x => x.Id == id).Include(x => x.UserServices).FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound();
                }
                ViewBag.UserServices = new MultiSelectList(await _context.Services.ToListAsync(), "Id", "Name", user.UserServices.Select(x => x.ServiceId).ToList());
                var userModel = _mapper.Map<User, UserViewModel>(user);
                return View(userModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "UsersController-Edit exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> Edit(UserViewModel userModel)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

                    var files = Request.Form.Files;
                    using var stream = new MemoryStream();
                    if (files.Any())
                    {
                        var image = files.FirstOrDefault();
                        if (allowedExtensions.Contains(Path.GetExtension(image.FileName).ToLower()))
                        {
                            if (image.Length < 3145728) // 3 MB
                            {
                                await image.CopyToAsync(stream);
                            }
                            else
                            {
                                ModelState.AddModelError("Image", "Image can't be larger than 3 MB");
                                return View(userModel);
                            }
                        }
                    }
                    var user = await _context.Users.Where(x => x.Id == userModel.Id).Include(x => x.UserServices).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        Expression<Func<User, bool>> predicate = s => s.Id != userModel.Id && s.Email == userModel.Email;
                        if (_context.Users.Any(predicate))
                        {
                            ModelState.AddModelError("Email", "Email is used");
                            return View(userModel);
                        }
                        predicate = s => s.Id != userModel.Id && s.PhoneNumber == userModel.PhoneNumber;
                        if (_context.Users.Any(predicate))
                        {
                            ModelState.AddModelError("PhoneNumber", "PhoneNumber is used");
                            return View(userModel);
                        }
                        user.FullName = userModel.FullName;
                        user.Address = userModel.Address;
                        user.PhoneNumber = userModel.PhoneNumber;
                        user.Email = userModel.Email;
                        user.Image = files.Any() ? stream.ToArray() : user.Image;
                        user.UserServices.Clear();
                        user.UserServices = new List<UserService>();
                        if (userModel.Services != null)
                        {
                            userModel.Services.ForEach(x =>
                            {
                                user.UserServices.Add(
                                    new UserService()
                                    {
                                        CreatedDate = DateTime.Now,
                                        ServiceId = x
                                    }
                                    );
                            });
                        }

                        await _context.SaveChangesAsync();
                        _toastNotification.AddSuccessToastMessage("User Updated successfully");
                        return RedirectToAction(nameof(Index));
                    }
                    return View(userModel);
                }
                return View(userModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "UsersController-Edit exception :");
                return RedirectToAction(nameof(Index));
            }

        }

        [ServiceFilter(typeof(AdminFilter))]
        public IActionResult Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var userModel = new UserViewModel().Id = id;
            return PartialView("_deleteUser", userModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminFilter))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (_context.Users == null)
                {
                    return Problem("Entity set 'CUTFLIDbContext.Users'  is null.");
                }
                var user = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (user != null)
                {
                    if (user.Permission == SystemEnums.Permission.Admin)
                    {
                        var adminsRole = _context.Users.Where(x => x.Permission == SystemEnums.Permission.Admin).Count();
                        if (adminsRole == 1)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                    }
                    _context.Users.Remove(user);
                }

                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("User deleted successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "UsersController-DeleteConfirmed exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.Where(x => x.Id == currentUserId).FirstOrDefaultAsync();
                var userModel = _mapper.Map<User, UserViewModel>(user);
                return View(userModel);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                _logger.LogError(ex, "UsersController-Profile exception :");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Profile")]
        public async Task<IActionResult> Profile(UserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                    var files = Request.Form.Files;
                    using var stream = new MemoryStream();
                    if (files.Any())
                    {
                        var image = files.FirstOrDefault();
                        if (allowedExtensions.Contains(Path.GetExtension(image.FileName).ToLower()))
                        {
                            if (image.Length < 3145728) // 3 MB
                            {
                                await image.CopyToAsync(stream);
                            }
                            else
                            {
                                ModelState.AddModelError("Image", "Image can't be larger than 3 MB");
                                return View(model);
                            }
                        }
                    }

                    var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
                    if (_context.Users.Any(x => x.PhoneNumber == model.PhoneNumber.Trim() && x.Id != model.Id))
                    {
                        ModelState.AddModelError("PhoneNumber", "PhoneNumber is used");
                        return View(model);
                    }
                    if (_context.Users.Any(x => x.Email == model.Email.Trim() && x.Id != model.Id))
                    {
                        ModelState.AddModelError("Email", "Email is used");
                        return View(model);
                    }
                    user.LastModifiedDate = DateTime.Now;
                    user.FullName = model.FullName;
                    user.Address = model.Address;
                    user.LastModifiedBy = currentUserId;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Email = model.Email;
                    user.Image = files.Any() ? stream.ToArray() : user.Image;
                    if (user.Image != null)
                    {
                        HttpContext.Session.SetString("Image", Convert.ToBase64String(user.Image));
                    }
                    _context.Update(user);
                    _toastNotification.AddSuccessToastMessage("Profile updated successfully");
                    await _context.SaveChangesAsync();
                    if (currentUserRole == "Admin")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    return RedirectToAction("UserDahboard", "Home");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UsersController-Profile exception :");
                string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (currentUserRole == "Admin")
                {
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("UserDahboard", "Home");
            }
        }

        [HttpGet]
        public IActionResult Password(int id)
        {
            try
            {
                PasswordModel model = new PasswordModel();
                model.Id = id;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UsersController-Password exception :");
                string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (currentUserRole == "Admin")
                {
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("UserDahboard", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Password(PasswordModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int? currentUserId = Convert.ToInt32(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
                    if (user != null)
                    {
                        if (model.NewPassword != model.ConfirmedPassword)
                        {
                            ModelState.AddModelError("ConfirmedPassword", "Password does't match");
                            return View(model);
                        }
                        user.Password = _passwordHasher.HashPassword(user, model.NewPassword);
                        _context.Users.Update(user);
                        _toastNotification.AddSuccessToastMessage("Password updated successfully");
                        await _context.SaveChangesAsync();
                        return View(model);
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UsersController-Password exception :");
                string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (currentUserRole == "Admin")
                {
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("UserDahboard", "Home");
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private async Task<bool> SendEmail(string consumer, string password)
        {
            try
            {
                var emailSetting = _configuration.GetSection("Email");
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(emailSetting["UserName"]));
                email.To.Add(MailboxAddress.Parse(consumer));
                email.Subject = "CUTFLIX - Employee Account";
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = "<!DOCTYPE html>\r\n<html>\r\n\r\n<head>\r\n    <style>\r\n        /* Define CSS styles here */\r\n        body {\r\n            font-family: Arial, sans-serif;\r\n            background-color: #f2f2f2;\r\n            margin: 0;\r\n            padding: 0;\r\n        }\r\n\r\n        .container {\r\n            max-width: 600px;\r\n            margin: 0 auto;\r\n            background-color: rgba(0, 0, 0, 0.9);\r\n            padding: 20px 0px;\r\n            border-radius: 5px;\r\n            color: #f8f9fa;\r\n            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);\r\n        }\r\n\r\n        .header {\r\n            text-align: center;\r\n            padding: 20px 0;\r\n        }\r\n\r\n        .header img {\r\n            max-width: 150px;\r\n            height: auto;\r\n        }\r\n\r\n        .content {\r\n            padding: 20px;\r\n        }\r\n\r\n        .footer {\r\n            text-align: center;\r\n            padding: 20px 0;\r\n        }\r\n    </style>\r\n</head>"
                    + $"<body>\r\n    <div class=\"container\">\r\n        <div class=\"header\">\r\n            <img src=\"https://i.ibb.co/L9PwLyN/2-2.png\" alt=\"Company Logo\">\r\n        </div>\r\n        <div class=\"content\">\r\n            <h3>Hello , find below your account information .</h3>\r\n            <p>Email : {consumer} </p>\r\n            <p>Password : {password} </p>\r\n        </div>\r\n        <div class=\"footer\">\r\n            <p>&copy; 2023 CutFlix. All rights reserved.</p>\r\n        </div>\r\n    </div>\r\n</body>\r\n</html>"
                };
                using var smtp = new SmtpClient();
                smtp.Connect(emailSetting["Server"], 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(emailSetting["UserName"], emailSetting["Password"]);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
                return true;
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Send email failed");
                _logger.LogError(ex, "UsersController-SendEmail exception :");
                return false;
            }
        }
    }
}
