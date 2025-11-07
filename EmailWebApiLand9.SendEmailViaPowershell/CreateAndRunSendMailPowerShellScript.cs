using EmailWebApiLand9.Data.Models;
using log4net;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace EmailWebApiLand9.SendEmailViaPowershell
{
    public class CreateAndRunSendMailPowerShellScript
    {
       ILog log = LogManager.GetLogger(typeof(CreateAndRunSendMailPowerShellScript));

        public CreateAndRunSendMailPowerShellScript(CreateAndRunSendMailWithHtmlStringPowershellScriptInput myInputWithHtmlString)
        {
            MyInputWithHtmlString = myInputWithHtmlString;
            MyPowerShellScriptWithHtmlStringLineList = new List<string>();
            CreatePowershellScriptWithHtmlStringLines();
        }

        public CreateAndRunSendMailWithHtmlStringPowershellScriptInput MyInputWithHtmlString { get; set; }

        public List<string> MyPowerShellScriptWithHtmlStringLineList { get; set; }

        public string MyPowerShellScriptFullLocation { get; set; }
        public string MyScriptString { get; set; }
        public List<string> MyCopiedAttachmentFullFilenameList { get; set; }
        public string MyRunScriptErrorMessage { get; set; }
        public CreatePowershellScriptWithHtmlStringLinesOutput CreatePowershellScriptWithHtmlStringLines()
        {
            CreatePowershellScriptWithHtmlStringLinesOutput returnOutput =
                new CreatePowershellScriptWithHtmlStringLinesOutput();

            /*$TLS12Protocol = [System.Net.SecurityProtocolType] 'Ssl3 , Tls12'
[System.Net.ServicePointManager]::SecurityProtocol = $TLS12Protocol

             */
            // TLS12ProtocolLine
            string MyTLS12ProtocolLine = $"$TLS12Protocol = [System.Net.SecurityProtocolType] 'Ssl3 , Tls12'";
            MyPowerShellScriptWithHtmlStringLineList.Add(MyTLS12ProtocolLine);

            // SecurityProtocolLine
            string MySecurityProtocolLine = $"[System.Net.ServicePointManager]::SecurityProtocol = $TLS12Protocol";
            MyPowerShellScriptWithHtmlStringLineList.Add(MySecurityProtocolLine);


            // Client Id line
            string MyClientIdLine = $"$myClientId = \"{MyInputWithHtmlString.MyInsertEmailConfigInput.ClientId}\"";
            MyPowerShellScriptWithHtmlStringLineList.Add(MyClientIdLine);

            // Client Secret line
            string MyClientSecretLine = $"$myClientSecret = \"{MyInputWithHtmlString.MyInsertEmailConfigInput.ClientSecret}\"";
            MyPowerShellScriptWithHtmlStringLineList.Add(MyClientSecretLine);

            // Tenant Id line
            string MyTenantIdLine = $"$myTenantId = \"{MyInputWithHtmlString.MyInsertEmailConfigInput.TenantId}\"";
            MyPowerShellScriptWithHtmlStringLineList.Add(MyTenantIdLine);


            // Subject Line
            string MySubjectLine = $"$mySubject = \"\"\"{MyInputWithHtmlString.MyEmailSendWithHtmlStringInput.emailSubject}\"\"\"";
            MyPowerShellScriptWithHtmlStringLineList.Add(MySubjectLine);


            // Body Line
            StringBuilder bodyBldr = new StringBuilder();
            bodyBldr.Append($"$myBodyLineInTextFormatArray = @(");
            bodyBldr.Append($"\"\"\"{MyInputWithHtmlString.MyEmailSendWithHtmlStringInput.emailHtmlStringAsBody}\"\"\"");
            bodyBldr.Append($")");
            MyPowerShellScriptWithHtmlStringLineList.Add(bodyBldr.ToString());


            // My To Recipient List Line.
            StringBuilder toRecipientListBldr = new StringBuilder();
            toRecipientListBldr.Append($"$myToRecipientArr = @(");
            foreach (string loopEmailAddress in MyInputWithHtmlString.MyEmailSendWithHtmlStringInput.emailAddressList)
            {
                if (loopEmailAddress.CompareTo(MyInputWithHtmlString.MyEmailSendWithHtmlStringInput.emailAddressList[0]) != 0)
                {
                    toRecipientListBldr.Append($",");
                }
                toRecipientListBldr.Append($"\"\"\"{loopEmailAddress}\"\"\"");
            }
            toRecipientListBldr.Append($")");
            MyPowerShellScriptWithHtmlStringLineList.Add(toRecipientListBldr.ToString());

            // From Address line
            string fromAddressLine = $"$myFromEmailAddress = \"{MyInputWithHtmlString.MyEmailSendWithHtmlStringInput.fromEmailAddress}\"";
            MyPowerShellScriptWithHtmlStringLineList.Add(fromAddressLine);

            // Joined Body Line List.
            MyPowerShellScriptWithHtmlStringLineList.Add($"$joinedMyBodyLineArrayString = $myBodyLineInTextFormatArray -join \",\"");

            // Joined To Recipient list.
            MyPowerShellScriptWithHtmlStringLineList.Add("$joinedMyToRecipientArrString = $myToRecipientArr -join \",\"");

            string myExecutableDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            MyCopiedAttachmentFullFilenameList =
                new List<string>();

            // Full Command With Attachments parameter. 
            if (MyInputWithHtmlString.MyEmailSendWithHtmlStringInput.emailAttachmentList.Count > 0)
            {

                List<FileInfo> fileInfoList = new List<FileInfo>();
                foreach (string loopFile in MyInputWithHtmlString.MyEmailSendWithHtmlStringInput.emailAttachmentList)
                {
                    fileInfoList.Add(new FileInfo(loopFile));
                }

                // make copies of the attachment files in the executing directory.
                foreach (FileInfo loopFi in fileInfoList)
                {
                    string destinationFullFilename = Path.Combine(myExecutableDirectory, loopFi.Name.Replace(" ", "_"));
                    MyCopiedAttachmentFullFilenameList.Add(destinationFullFilename);
                    File.Copy(loopFi.FullName, destinationFullFilename, true);
                }

                // My Attachment List Line.
                StringBuilder attachmentListBldr = new StringBuilder();
                attachmentListBldr.Append($"$myAttachmentArr = @(");

                int filenameCtr = 1;
                foreach (string loopAttachmentFullFilename in MyCopiedAttachmentFullFilenameList)
                {
                    if (filenameCtr > 1)
                    {
                        attachmentListBldr.Append($",");
                    }
                    attachmentListBldr.Append($"'{loopAttachmentFullFilename}'");
                    filenameCtr++;
                }
                attachmentListBldr.Append($")");
                MyPowerShellScriptWithHtmlStringLineList.Add(attachmentListBldr.ToString());

                // Joined Attachment List..
                MyPowerShellScriptWithHtmlStringLineList.Add("$joinedMyAttachmentArrString = $myAttachmentArr -join \",\"");



                // Full Command. 
                MyPowerShellScriptWithHtmlStringLineList.Add($"$fullCommand =  \"{myExecutableDirectory}\\MyEmailScriptWithAttachments.ps1 -Subject $mySubject -BodyLineInTextFormatArray $joinedMyBodyLineArrayString  -ToArray $joinedMyToRecipientArrString -FromEmailAddress $myFromEmailAddress -ClientId $myClientId -ClientSecret $myClientSecret -TenantId $myTenantId -AttachmentArray $joinedMyAttachmentArrString\"");

            }

            // Full Command Without Attachments parameter. 
            else
            {
                // Full Command. 
                MyPowerShellScriptWithHtmlStringLineList.Add($"$fullCommand =  \"{myExecutableDirectory}\\MyEmailScriptWithAttachments.ps1 -Subject $mySubject -BodyLineInTextFormatArray $joinedMyBodyLineArrayString  -ToArray $joinedMyToRecipientArrString -FromEmailAddress $myFromEmailAddress -ClientId $myClientId -ClientSecret $myClientSecret -TenantId $myTenantId -AttachmentArray Empty\"");

            }


            // invoke-expression -Command $fullCommand
            MyPowerShellScriptWithHtmlStringLineList.Add("invoke-expression -Command $fullCommand");


            // Combine the script line list into a carriage-return-line-feed delimited list.
            MyScriptString = string.Join("\r\n", MyPowerShellScriptWithHtmlStringLineList.ToArray());

            string myExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            MyPowerShellScriptFullLocation = Path.Combine(myExecutablePath, "CallMyEmailScript.ps1");
            if (File.Exists(MyPowerShellScriptFullLocation))
            {
                File.Delete(MyPowerShellScriptFullLocation);
            }
            log.Info($"MyPowerShellScriptFullLocation:  {MyPowerShellScriptFullLocation}\r\n\r\nScript lines:  {MyScriptString}");

            try
            {
                File.WriteAllText(MyPowerShellScriptFullLocation, MyScriptString);
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    exceptionMessage = $"{exceptionMessage}.  Inner Exception:  {ex.InnerException.Message}";
                }
                log.Error(exceptionMessage);
                returnOutput.IsOk = false;
                returnOutput.ErrorMessage = exceptionMessage;
                return returnOutput;
            }
            if (!File.Exists(MyPowerShellScriptFullLocation))
            {
                returnOutput.IsOk = false;
                returnOutput.ErrorMessage = $"Calling Powershell script {MyPowerShellScriptFullLocation} could not be written.";
                log.Error(returnOutput.ErrorMessage);
                return returnOutput;
            }
            log.Info($"No exception in the writing of the script file:  {MyPowerShellScriptFullLocation}");

            /*$mySubject = """Test Email for the calling of the Microsoft Graph SendEmail Web API end-point"""
$myBodyLineInTextFormatArray = @("""One""","""Two""","""Three""")
$myToRecipientArr = @("""pwmorrison@summithealthcare.com""")
$myFromEmailAddress = "smgapplications@summithealthcare.com"

$myExecutionPath = Split-Path $MyInvocation.MyCommand.Path

$joinedMyBodyLineArrayString = $myBodyLineInTextFormatArray -join ","

$joinedMyToRecipientArrString = $myToRecipientArr -join ","

$fullCommand = $myExecutionPath + "\MyEmailScript.ps1 -Subject $mySubject -BodyLineInTextFormatArray $joinedMyBodyLineArrayString  -ToArray $joinedMyToRecipientArrString -FromEmailAddress $myFromEmailAddress"

invoke-expression -Command $fullCommand

            */



            return returnOutput;
        }


        public void RunPowershellScript()
        {
            this.MyRunScriptErrorMessage = string.Empty;

            Process callPowershellProcess = new Process();
            FileInfo whereAmIFileInfo = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            callPowershellProcess.StartInfo.WorkingDirectory = whereAmIFileInfo.Directory.FullName;
            callPowershellProcess.StartInfo.Arguments = "-File " + MyPowerShellScriptFullLocation;
            callPowershellProcess.StartInfo.FileName = "powershell.exe";

            callPowershellProcess.StartInfo.UseShellExecute = false;
            callPowershellProcess.StartInfo.RedirectStandardOutput = true;
            callPowershellProcess.StartInfo.RedirectStandardError = true;
            callPowershellProcess.StartInfo.CreateNoWindow = true;
            callPowershellProcess.ErrorDataReceived += p_ErrorDataReceived;
            callPowershellProcess.OutputDataReceived += p_OutputDataReceived;
            callPowershellProcess.EnableRaisingEvents = true;
            callPowershellProcess.Start();
            callPowershellProcess.BeginOutputReadLine();
            callPowershellProcess.BeginErrorReadLine();
            callPowershellProcess.WaitForExit();
            callPowershellProcess.Close();


            // Delete copied files.
            foreach(string loopCopiedFullFilename in MyCopiedAttachmentFullFilenameList)
            {
                if (File.Exists(loopCopiedFullFilename))
                {
                    File.Delete(loopCopiedFullFilename);
                }
            }
        }
        public void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e != null && e.Data != null)
            {
                log.Info($"Power Shell Script Standard Output:  {e.Data}");
            }
        }

        public void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e != null && e.Data != null)
            {
                this.MyRunScriptErrorMessage = $"Error Message upon running script:  {e.Data}";
                log.Error($"Power Shell Script Error Output:  {e.Data}");
            }
        }
    }
}
