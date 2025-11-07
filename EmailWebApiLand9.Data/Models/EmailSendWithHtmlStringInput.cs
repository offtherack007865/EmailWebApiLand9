using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailWebApiLand9.Data.Models
{
    public class EmailSendWithHtmlStringInput
    {
        public EmailSendWithHtmlStringInput()
        {
            emailSubject = string.Empty;
            emailHtmlStringAsBody = string.Empty;
            emailAddressList = new List<string>();
            fromEmailAddress = string.Empty;
            emailAttachmentList = new List<string>();
        }
        public string emailSubject { get; set; }
        public string emailHtmlStringAsBody { get; set; }
        public List<string> emailAddressList { get; set; }
        public string fromEmailAddress { get; set; }
        public List<string> emailAttachmentList { get; set; }
    }
}
