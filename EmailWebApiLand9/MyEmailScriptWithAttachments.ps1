 param(
        [string]$Subject,
        [string[]]$BodyLineInTextFormatArray,		
        [string[]]$ToArray,
		[string]$FromEmailAddress,
		[string]$ClientId,
		[string]$ClientSecret,
		[string]$TenantId,
        [string[]]$AttachmentArray
    )
$AttachmentArrayCount = $AttachmentArray.Count	

Write-Output "AttachmentArrayCount $AttachmentArrayCount"
$ToArrayCount = $ToArray.Count
Write-Output "ToArrayCount $ToArrayCount"

if ( $AttachmentArrayCount -eq 1 )	
{
	if ( $AttachmentArray[0] -eq "Empty" )
	{
		$AttachmentArray = @()
	}
}

$AttachmentArrayCount = $AttachmentArray.Count	
Write-Output "After checking for empty, AttachmentArrayCount $AttachmentArrayCount"
	
#Build Body in HTML format
$myBodyInHtmlFormat = "<p>"
$lineCtr = 1
foreach($BodyLineInTextFormat in $BodyLineInTextFormatArray)
{
	if ( $lineCtr -gt 1 )
	{
		$myBodyInHtmlFormat = $myBodyInHtmlFormat + "</p><p>"
	}
	$myBodyInHtmlFormat = $myBodyInHtmlFormat + $BodyLineInTextFormat
	$lineCtr++
}
$myBodyInHtmlFormat = $myBodyInHtmlFormat + "</p>"


# Constants to help build the JSON for the Message.
$messageConstString = """message"""
$subjectConstString = """subject"""
$bodyConstString = """body"""
$htmlConstString = """HTML"""
$contentTypeConstString = """contentType"""
$contentConstString = """content"""
$contentTypeHtmlConstString = $contentTypeConstString + ": " + $htmlConstString
$toRecipientsConstString = """toRecipients"""
$saveToSentItemsConstString = """saveToSentItems"""
$falseConstString = """false"""
$emailAddressConstString = """emailAddress"""
$addressConstString = """address"""
$attachmentsConstString = """attachments"""
$attachmentDataTypeAndNameConst = "`r`n{`r`n""@odata.type"": ""#microsoft.graph.fileAttachment"",`r`n""name"": "
$attachmentContentTypeConst = ",`r`n""contenttype"": ""application/vnd.openxmlformats-officedocument.wordprocessingml.document"""

# Build attachment JSON
#"attachments" = @(
#             @{
#              "@odata.type" = "#microsoft.graph.fileAttachment"
#              "name" = $AttachmentFile
#              "contenttype" = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
#              "contentBytes" = $ContentBase64 } )  
if ( $AttachmentArrayCount -eq 0)
{
	$attachmentListJson = $attachmentsConstString + ": []"
}
else
{

   $attachmentListJson = $attachmentsConstString + ": ["

   $attachmentCtr = 1
   $AttachmentArray.ForEach({
      Write-Output "Attachment Array Value:  $_"
	  $fileName = (Get-Item -Path $_).Name
	  $ContentBase64 = [convert]::ToBase64String( [system.io.file]::readallbytes($_))
      $attachmentContentBytesConst = ",`r`n""contentBytes"": """ + $ContentBase64 + """"
	  if ($attachmentCtr -gt 1)
	  {
		   $attachmentListJson = $attachmentListJson + "},"
 	  }
	  $attachmentListJson = $attachmentListJson + $attachmentDataTypeAndNameConst + " """ +  $fileName + """ " + $attachmentContentTypeConst + " " + $attachmentContentBytesConst
	  $attachmentCtr++
   })
   $attachmentListJson = $attachmentListJson + "}]"
}

Write-Output "attachmentListJson $attachmentListJson"


# Build To: Address List JSON
$toAddressListJson = "{"

$toAddressCtr = 1
foreach($To in $ToArray)
{
	if ($toAddressCtr -gt 1)
	{
		$toAddressListJson = $toAddressListJson + "},{"
	}
	$toAddressListJson = $toAddressListJson + $emailAddressConstString + ": { " + $addressConstString + ": """ + $To + """ }"  
	$toAddressCtr++
}
$toAddressListJson = $toAddressListJson + "}"

#  Put body parts together.
$bodyInHtmlJson = " { " + 
					$messageConstString + ": { " + 
					$subjectConstString + ": """ + $Subject + """," + 
					$bodyConstString + ": { " +
					$contentTypeHtmlConstString + ", " +
					$contentConstString + ": " +
					"""" + $myBodyInHtmlFormat + """}, " +
                    $attachmentListJson + "," +					
					$toRecipientsConstString + ": [ " + $toAddressListJson + " ]}, " +
					$saveToSentItemsConstString + ": " + $falseConstString +
					"}"
Write-Output "Inputted Subject:  $Subject"
Write-Output "bodyInHtmlJson = $bodyInHtmlJson"

#Summit's Microsoft Graph Application Registration Info
#$clientId = "14af91b4-e77b-4ef8-94ed-070e7b139730"
#$clientSecret = "b76b4da7-0f83-4da8-ac9d-01b08b35fc3d"
#$tenantID = "26f211c8-652a-4673-8495-cdeeea6ff167"

$MailSender = $FromEmailAddress

#Connect to GRAPH API
$tokenBody = @{
    Grant_Type    = "client_credentials"
    Scope         = "https://graph.microsoft.com/.default"
    Client_Id     = $ClientId
    Client_Secret = $ClientSecret
}
$tokenResponse = Invoke-RestMethod -Uri "https://login.microsoftonline.com/$tenantID/oauth2/v2.0/token" -Method POST -Body $tokenBody
$headers = @{
    "Authorization" = "Bearer $($tokenResponse.access_token)"
    "Content-type"  = "application/json"
}

#Send Mail    
$URLsend = "https://graph.microsoft.com/v1.0/users/$MailSender/sendMail"
$BodyJsonsend = $bodyInHtmlJson

Invoke-RestMethod -Method POST -Uri $URLsend -Headers $headers -Body $BodyJsonsend

