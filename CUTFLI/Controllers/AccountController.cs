using CUTFLI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using CUTFLI.ViewModels;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using MimeKit;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using CUTFLI.Enums;

namespace CUTFLI.Controllers
{
    public class AccountController : Controller
    {
        private readonly CUTFLIDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IToastNotification _toastNotification;
        private readonly IConfiguration _configuration;

        public AccountController(CUTFLIDbContext context, IPasswordHasher<User> passwordHasher, IToastNotification toastNotification, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _toastNotification = toastNotification;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            ClaimsPrincipal claims = HttpContext.User;
            if (claims.Identity.IsAuthenticated)
            {
                string currentUserRole = HttpContext.User.FindFirstValue(ClaimTypes.Role);
                if (currentUserRole == "Admin")
                {
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("UserDahboard", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AccountViewModel model)
        {
            try
            {
                var user = _context.Users.Where(x => x.Email == model.Email).FirstOrDefault();
                if (user != null)
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                    if (result == PasswordVerificationResult.Success)
                    {
                        List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Role,user.Permission.ToString()),
                };

                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                            CookieAuthenticationDefaults.AuthenticationScheme);

                        AuthenticationProperties properties = new AuthenticationProperties()
                        {
                            AllowRefresh = true,
                            IsPersistent = true,
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity), properties);

                        HttpContext.Session.SetString("Name", user.FullName);
                        HttpContext.Session.SetString("Role", user.Permission.ToString());
                        if (user.Image != null)
                        {
                            HttpContext.Session.SetString("Image", Convert.ToBase64String(user.Image));
                        }

                        if (user.Permission == SystemEnums.Permission.Admin)
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return RedirectToAction("UserDahboard", "Home");
                        }
                    }
                    ModelState.AddModelError("Password", "Password was uncorrect");
                    return View();
                }
                _toastNotification.AddErrorToastMessage("Email or password was uncorrect");
                return View();
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            try
            {
                var user = _context.Users.Where(x => x.Email == email.Trim()).FirstOrDefault();
                if (user != null)
                {
                    var password = GeneratePassword(true, true, true, true, 8);
                    var hashPassword = _passwordHasher.HashPassword(user, password);
                    user.Password = hashPassword;
                    await _context.SaveChangesAsync();
                    if (SendEmail(user.Email, password))
                    {
                        _toastNotification.AddSuccessToastMessage("Check your email inbox for new password");
                        return RedirectToAction(nameof(Login));
                    }
                }
                _toastNotification.AddWarningToastMessage("Email not exist");
                return View();
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return RedirectToAction(nameof(Login));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return RedirectToAction(nameof(Login));
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private bool SendEmail(string consumer, string password)
        {
            try
            {
                var emailSetting = _configuration.GetSection("Email");
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(emailSetting["UserName"]));
                email.To.Add(MailboxAddress.Parse(consumer));
                email.Subject = "CUTFLIX - New Password";
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = "<!DOCTYPE html>\r\n<html>\r\n\r\n<head>\r\n    <style>\r\n        /* Define CSS styles here */\r\n        body {\r\n            font-family: Arial, sans-serif;\r\n            background-color: #f2f2f2;\r\n            margin: 0;\r\n            padding: 0;\r\n        }\r\n\r\n        .container {\r\n            max-width: 600px;\r\n            margin: 0 auto;\r\n            background-color: rgba(0, 0, 0, 0.9);\r\n            padding: 20px 0px;\r\n            border-radius: 5px;\r\n            color: #f8f9fa;\r\n            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);\r\n        }\r\n\r\n        .header {\r\n            text-align: center;\r\n            padding: 20px 0;\r\n        }\r\n\r\n        .header img {\r\n            max-width: 150px;\r\n            height: auto;\r\n        }\r\n\r\n        .content {\r\n            padding: 20px;\r\n        }\r\n\r\n        .footer {\r\n            text-align: center;\r\n            padding: 20px 0;\r\n        }\r\n    </style>\r\n</head>"
                    + $"<body>\r\n    <div class=\"container\">\r\n        <div class=\"header\">\r\n            <img src=\"https://i.ibb.co/L9PwLyN/2-2.png\" alt=\"Company Logo\">\r\n        </div>\r\n        <div class=\"content\">\r\n            <h3>Hello , find below new password .</h3>\r\n            <p>New Password : {password} </p>\r\n       </div>\r\n        <div class=\"footer\">\r\n            <p>&copy; 2023 CutFlix. All rights reserved.</p>\r\n        </div>\r\n    </div>\r\n</body>\r\n</html>"
                };
                using var smtp = new SmtpClient();
                smtp.Connect(emailSetting["Server"], 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(emailSetting["UserName"], emailSetting["Password"]);
                smtp.Send(email);
                smtp.Disconnect(true);
                return true;
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Error Found");
                return false;
            }
        }
        private string GeneratePassword(bool useLowercase, bool useUppercase,
           bool useNumbers, bool useSpecial, int passwordSize)
        {
            const string LOWER_CASE = "abcdefghijklmnopqursuvwxyz";
            const string UPPER_CAES = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string NUMBERS = "123456789";
            const string SPECIALS = @"!@£$%^&*()#€";
            char[] _password = new char[passwordSize];
            string charSet = "";
            System.Random _random = new Random();
            int counter;

            if (useLowercase) charSet += LOWER_CASE;

            if (useUppercase) charSet += UPPER_CAES;

            if (useNumbers) charSet += NUMBERS;

            if (useSpecial) charSet += SPECIALS;

            for (counter = 0; counter < passwordSize; counter++)
            {
                _password[counter] = charSet[_random.Next(charSet.Length - 1)];
            }
            return String.Join(null, _password);
        }
    }
}
