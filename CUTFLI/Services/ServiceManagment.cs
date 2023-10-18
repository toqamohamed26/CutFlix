using CUTFLI.Models;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using static CUTFLI.Enums.SystemEnums;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace CUTFLI.Services
{
    public class ServiceManagment : IServiceManagment
    {
        private readonly CUTFLIDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public ServiceManagment(CUTFLIDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task CheckAppointmentsTime()
        {
            try
            {
                var dayToCompare = DateTime.Now;
                var nextHour = new DateTime(dayToCompare.Year, dayToCompare.Month, dayToCompare.Day, dayToCompare.Hour + 1, dayToCompare.Minute, 0);
                var appointments = await _dbContext.Appointments.Where(x => x.Date.Date == DateTime.Now.Date && x.StartTime == nextHour.TimeOfDay && x.VistiorId != null && x.Status == AppointmentStatus.Checked).Include(x => x.People).ToListAsync();
                if (appointments.Count > 0)
                {
                    appointments.ForEach(async x => await SendEmail(x.People.Email, x.Date, x.StartTime, x.EndTime));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async Task SendEmail(string consumer, DateTime date, TimeSpan srartTime, TimeSpan endTime)
        {
            try
            {
                string convertDate = date.ToString("D");
                string convertStartTime = new DateTime(srartTime.Ticks).ToString("hh:mm tt");
                string convertEndTime = new DateTime(endTime.Ticks).ToString("hh:mm tt");
                var emailSetting = _configuration.GetSection("Email");
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(emailSetting["UserName"]));
                email.To.Add(MailboxAddress.Parse(consumer));

                email.Subject = "CUTFLIX - Notify";
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = "<!DOCTYPE html>\r\n<html>\r\n\r\n<head>\r\n    <style>\r\n        /* Define CSS styles here */\r\n        body {\r\n            font-family: Arial, sans-serif;\r\n            background-color: #f2f2f2;\r\n            margin: 0;\r\n            padding: 0;\r\n        }\r\n\r\n        .container {\r\n            max-width: 600px;\r\n            margin: 0 auto;\r\n            background-color: rgba(0, 0, 0, 0.9);\r\n            padding: 20px 0px;\r\n            border-radius: 5px;\r\n            color: #f8f9fa;\r\n            box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.1);\r\n        }\r\n\r\n        .header {\r\n            text-align: center;\r\n            padding: 20px 0;\r\n        }\r\n\r\n        .header img {\r\n            max-width: 150px;\r\n            height: auto;\r\n        }\r\n\r\n        .content {\r\n            padding: 20px;\r\n        }\r\n\r\n        .footer {\r\n            text-align: center;\r\n            padding: 20px 0;\r\n        }\r\n    </style>\r\n</head>"
                    + $"<body>\r\n    <div class=\"container\">\r\n        <div class=\"header\">\r\n            <img src=\"https://i.ibb.co/L9PwLyN/2-2.png\" alt=\"Company Logo\">\r\n        </div>\r\n        <div class=\"content\">\r\n            <h3>Dear, your appointment start at : {convertStartTime} </h3>\r\n       </div>\r\n        <div class=\"footer\">\r\n            <p>&copy; 2023 CutFlix. All rights reserved.</p>\r\n        </div>\r\n    </div>\r\n</body>\r\n</html>"
                };

                using var smtp = new SmtpClient();
                smtp.Connect(emailSetting["Server"], 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(emailSetting["UserName"], emailSetting["Password"]);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
