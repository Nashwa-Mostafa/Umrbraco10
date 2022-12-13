using Clean.Core.Models.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;

using Newtonsoft.Json.Linq;

using System.Net;

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using System.Text;
using MimeKit;
using System.Net.Mail;

namespace Umbraco10.Controller
{
    public class ContactUsController :SurfaceController
    {
        public ContactUsController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
        }

        public IActionResult Index()
        {

            return View();
        }
        [HttpPost]
    
        [ActionName("SubmitForm")]
        public IActionResult SubmitForm(ContactViewModel contactViewModel)
        {
                if (!ModelState.IsValid) 
                {

                TempData["Success"] = false;
                return CurrentUmbracoPage();
            }
                else
                {
                    if (!ReCaptchaPassed(Request.Form["recapcha"]))
                    {
                    TempData["Success"] = false;
                    ModelState.AddModelError(string.Empty, "You failed the CAPTCHA.");
                    return CurrentUmbracoPage();
                }
                }
            TempData["Success"] = true;
            sendEmail(contactViewModel);
            return RedirectToCurrentUmbracoPage();
        }

        public void sendEmail(ContactViewModel model)
        {
            MailMessage message = new MailMessage("sara.osama@linkdev.com",model.Email);
           
         
            message.Subject = "Sample Contact Us";

            StringBuilder builder = new StringBuilder();
            builder.Append($"<h1>A new contact us forms has been submitted.</h1><br /><br />");
            builder.Append($"<strong>Name: </strong>{model.Name}<br />");
            builder.Append($"<strong>Email: </strong>{model.Email}<br />");
            builder.Append($"<strong>Coment:</strong><br />{model.Message}<br />");

            message.IsBodyHtml = true;

            message.Body = builder.ToString();
            try
            {

                SmtpClient smtp = new SmtpClient("server", 25);
                NetworkCredential NetworkCred = new NetworkCredential("user", "password");
                smtp.Credentials = NetworkCred;
                smtp.EnableSsl = false;
                smtp.Send(message);

            }
            catch (Exception ex)
            {

            }
           
        }
        public static bool ReCaptchaPassed(string gRecaptchaResponse)
        {
            HttpClient httpClient = new HttpClient();

            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret=6LdKLsYbAAAAAMq-2oLn6drMWGNmIsxN5qNXqImN&response={gRecaptchaResponse}").Result;

            if (res.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }
            string JSONres = res.Content.ReadAsStringAsync().Result;
            dynamic JSONdata = JObject.Parse(JSONres);

            if (JSONdata.success != "true" || JSONdata.score <= 0.5m)
            {
                return false;
            }

            return true;
        }
    }
}
