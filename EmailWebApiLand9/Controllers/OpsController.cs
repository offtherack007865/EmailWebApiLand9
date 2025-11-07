using EmailWebApiLand9.Data;
using EmailWebApiLand9.Data.Models;
using EmailWebApiLand9.EncryptionDecryption;
using EmailWebApiLand9.SendEmailViaPowershell;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace EmailWebApiLand9.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EmailWebApiController : ControllerBase
    {
        ILog log = LogManager.GetLogger(typeof(Program));
        public EmailWebApiController(EmailWebApi8Context inputEmailWebApiContext)
        {
            MyEmailWebApiContext = inputEmailWebApiContext;
            MyRunPowerShellErrorMessage = string.Empty;
        }
        public EmailWebApi8Context MyEmailWebApiContext { get; set; }
        public string MyRunPowerShellErrorMessage { get; set; }

        // POST  /api/EmailWebApi/InsertEmailConfig
        [HttpPost]
        public InsertEmailConfigOutput InsertEmailConfig([FromBody] InsertEmailConfigInput inputInsertEmailConfigInput)
        {
            InsertEmailConfigOutput returnOutput = new InsertEmailConfigOutput();

            string myEncryptedClientSecret =
                 AES.EncryptText(inputInsertEmailConfigInput.ClientSecret);


            try
            {
                EmailConfig myEmailConfigToInsert =
                    new EmailConfig
                    {
                        Enabled = true,
                        ClientId = inputInsertEmailConfigInput.ClientId,
                        EncryptedClientSecret = myEncryptedClientSecret,
                        TenantId = inputInsertEmailConfigInput.TenantId,
                        CreatedBy = MyConstants.MyAppName,
                        CreatedTimestamp = DateTime.Now,
                        UpdatedBy = MyConstants.MyAppName,
                        UpdatedTimestamp = DateTime.Now,
                    };
                MyEmailWebApiContext.EmailConfigs.Add(myEmailConfigToInsert);
                MyEmailWebApiContext.SaveChanges();
            }
            catch (Exception ex)
            {
                string myExceptionMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    myExceptionMessage = $"{myExceptionMessage},  Inner Exception{ex.InnerException.Message}";
                }
                returnOutput.IsOk = false;
                returnOutput.ErrorMessage = myExceptionMessage;
                return returnOutput;
            }

            return returnOutput;
        }

        /*{
  "clientId": "14af91b4-e77b-4ef8-94ed-070e7b139730",
  "clientSecret": "4nP8Q~axypODv2-R9cvEdoObAfCNYRh4~el5bc4f",
  "tenantId": "26f211c8-652a-4673-8495-cdeeea6ff167"
}
         */
        //  /api/EmailWebApi/SendEmailWithHtmlStringInput

        [HttpPost]
        public EmailSendWithHtmlStringOutput SendEmailWithHtmlStringInput([FromBody] EmailSendWithHtmlStringInput inputEmailSendWithHtmlStringInput)
        {
            EmailSendWithHtmlStringOutput returnOutput = new EmailSendWithHtmlStringOutput();

            // In subject and body Replace double quotes and backslashes with spaces.
            inputEmailSendWithHtmlStringInput.emailSubject =
                inputEmailSendWithHtmlStringInput.emailSubject.Replace("\"", " ").Replace("\\", " ");

            inputEmailSendWithHtmlStringInput.emailHtmlStringAsBody =
                            inputEmailSendWithHtmlStringInput.emailHtmlStringAsBody.Replace("\"", " ").Replace("\\", " ");

            EmailConfigJsonOutput myConfigOutput =
                 GetConfig();

            if (!myConfigOutput.IsOk)
            {
                returnOutput.IsOk = false;
                returnOutput.ErrorMessage = myConfigOutput.ErrorMessage;
                return returnOutput;
            }

            if (myConfigOutput.Config.Count == 0)
            {
                returnOutput.IsOk = false;
                returnOutput.ErrorMessage = "The call to the database brought back NO configuration rows.";
                return returnOutput;

            }

            // Decrypt ClientSecret

            string myDecryptedClientSecret =
                AES.DecryptText(myConfigOutput.Config[0].EncryptedClientSecret);

            // Create Input record for Creating and Running Microsoft Graph Email.
            CreateAndRunSendMailWithHtmlStringPowershellScriptInput myPsInput =
                new CreateAndRunSendMailWithHtmlStringPowershellScriptInput();
            InsertEmailConfigInput myConfigInput =
                new InsertEmailConfigInput
                {
                    ClientId = myConfigOutput.Config[0].ClientId,
                    ClientSecret = myConfigOutput.Config[0].EncryptedClientSecret,
                    TenantId = myConfigOutput.Config[0].TenantId,
                };
            myPsInput.MyInsertEmailConfigInput = myConfigInput;
            myPsInput.MyEmailSendWithHtmlStringInput = inputEmailSendWithHtmlStringInput;


            // Create and run the Powershell script.
            // There are times when the PowerShell script errors out because of 
            // a glitch in the network.  So, if an error occurs in the script, we
            // re-run the script to see if we can get it to successfully send the message.

            for (int tryCtr = 0; tryCtr < 5; tryCtr++)
            {
                this.MyRunPowerShellErrorMessage = string.Empty;
                CreateAndRunSendMailPowerShellScript myRunPs =
                    new
                        CreateAndRunSendMailPowerShellScript
                        (
                            myPsInput
                        );
                log.Info($"B4 call to powershell.");
                myRunPs.RunPowershellScript();

                this.MyRunPowerShellErrorMessage =
                    myRunPs.MyRunScriptErrorMessage;
                // If this run sent the message successfully, break out of the loop.
                if (this.MyRunPowerShellErrorMessage.Length == 0)
                {
                    break;
                }
            }

            //// Delete any copied Email Attachment files.
            if (inputEmailSendWithHtmlStringInput.emailAttachmentList.Count > 0)
            {
                foreach (string loopAttachmentFullFilename in inputEmailSendWithHtmlStringInput.emailAttachmentList)
                {
                    FileInfo loopFi = new FileInfo(loopAttachmentFullFilename);
                    string deleteFileFolder =
                        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    string deleteFullFilename =
                        Path.Combine(deleteFileFolder, loopFi.Name);

                    if (System.IO.File.Exists(deleteFullFilename))
                    {
                        System.IO.File.Delete(deleteFullFilename);
                    }
                }
            }

            // If run the power shell script 5 times without successfully sending
            // the message, error out.
            if (this.MyRunPowerShellErrorMessage.Length > 0) 
            {
                returnOutput.IsOk = false;
                returnOutput.ErrorMessage = this.MyRunPowerShellErrorMessage;
                return returnOutput;
            }

            return returnOutput;
        }

        [NonAction]
        public EmailConfigJsonOutput GetConfig()
        {
            EmailConfigJsonOutput returnOutput = new EmailConfigJsonOutput();

            string sql = $"exec spGetEmailConfig";

            List<SqlParameter> parms = new List<SqlParameter>();

            try
            {
                returnOutput.Config =
                    MyEmailWebApiContext
                    .SpGetEmailConfigOutputList
                    .FromSqlRaw<spGetEmailConfigOutput>
                    (
                        sql
                        , parms.ToArray()
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                string myExceptionMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    myExceptionMessage = $"{myExceptionMessage}.  Inner Exception {ex.InnerException.Message}";
                }
                returnOutput.IsOk = false;
                returnOutput.ErrorMessage = myExceptionMessage;
                return returnOutput;
            }
            return returnOutput;
        }
    }
}
