using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailWebApiLand9.Data.Models
{
    public class CreateAndRunSendMailWithHtmlStringPowershellScriptInput
    {
        public CreateAndRunSendMailWithHtmlStringPowershellScriptInput()
        {
            MyInsertEmailConfigInput = new InsertEmailConfigInput();
            MyEmailSendWithHtmlStringInput = new EmailSendWithHtmlStringInput();
        }
        public EmailSendWithHtmlStringInput MyEmailSendWithHtmlStringInput { get; set; }
        public InsertEmailConfigInput MyInsertEmailConfigInput { get; set; }

    }
}
