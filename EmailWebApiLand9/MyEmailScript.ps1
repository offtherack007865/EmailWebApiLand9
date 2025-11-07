 param(
        [string]$Subject,
        [string[]]$BodyLineInTextFormatArray,		
        [string[]]$ToArray,
		[string]$FromEmailAddress,
		[string]$ClientId,
		[string]$ClientSecret,
		[string]$TenantId
    )

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

# Build To: Address List JSON
$emailAddressConstString = """emailAddress"""
$addressConstString = """address"""

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

#                        "saveToSentItems": "false"
$bodyInHtmlJson = " { " + 
					$messageConstString + ": { " + 
					$subjectConstString + ": """ + $Subject + """," + 
					$bodyConstString + ": { " +
					$contentTypeHtmlConstString + ", " +
					$contentConstString + ": " +
					"""" + $myBodyInHtmlFormat + """}, " +
					$toRecipientsConstString + ": [ " + $toAddressListJson + " ] }, " +
					$saveToSentItemsConstString + ": " + $falseConstString +
					"}"
Write-Output "Inputted Subject:  $Subject"
Write-Output "bodyInHtmlJson = $bodyInHtmlJson"

#Summit's Microsoft Graph Application Registration Info
#$clientId = "14af91b4-e77b-4ef8-94ed-070e7b139730"
#$clientSecret = "4nP8Q~axypODv2-R9cvEdoObAfCNYRh4~el5bc4f"
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

