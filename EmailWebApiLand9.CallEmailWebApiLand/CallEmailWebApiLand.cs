using EmailWebApiLand9.Data.Models;
using log4net;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace EmailWebApiLand9.CallEmailWebApiLand
{
    public class CallEmailWebApiLand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CallEmailWebApiLand));

        // Constructor for sending an email whose body is comprised of an inputted HTML string.
        public CallEmailWebApiLand
        (
            string inputEemailSubject
            , string inputBodyWithHtmlString
            , List<string> inputEmailAddressList
            , string inputFromEmailAddress
            , string inputEmailWebApiBaseUrl
            , List<string> inputAttachmentList
        )
        {
            MyEmailSendWithHtmlStringInput = new EmailSendWithHtmlStringInput()
            {
                emailSubject = inputEemailSubject,
                emailHtmlStringAsBody = inputBodyWithHtmlString,
                emailAddressList = inputEmailAddressList,
                fromEmailAddress = inputFromEmailAddress,
                emailAttachmentList = inputAttachmentList
            };

            MyEmailWebApiBaseUrl = inputEmailWebApiBaseUrl;
        }
        public EmailSendWithHtmlStringInput MyEmailSendWithHtmlStringInput { get; set; }
        public string MyEmailWebApiBaseUrl { get; set; }

        // Call the Email web API to send an inputted HTML String the body of the email.
        public EmailSendWithHtmlStringOutput CallIHtmlStringBody()
        {
            EmailSendWithHtmlStringOutput returnOutput =
                CallIHtmlStringBodyAsync().Result;

            return returnOutput;
        }

        // Async Call the Email web API to send an inputted HTML String the body of the email.
        public async Task<EmailSendWithHtmlStringOutput> CallIHtmlStringBodyAsync()
        {
            EmailSendWithHtmlStringOutput returnOutput = new EmailSendWithHtmlStringOutput();

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress =
                new Uri(MyEmailWebApiBaseUrl);

                string url = MyEmailWebApiBaseUrl;
                if (!url.Contains("api"))
                {
                    url = $"{MyEmailWebApiBaseUrl}/api/EmailWebApi/SendEmailWithHtmlStringInput";
                }

                var json = JsonConvert.SerializeObject(MyEmailSendWithHtmlStringInput);
                using (var response = await httpClient.PostAsJsonAsync(url, MyEmailSendWithHtmlStringInput))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    returnOutput = JsonConvert.DeserializeObject<EmailSendWithHtmlStringOutput>(apiResponse);
                }
            }
            return returnOutput;
        }
    }
}
