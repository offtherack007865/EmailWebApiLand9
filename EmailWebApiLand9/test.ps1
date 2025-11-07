$AttachmentArray = @(
    'F:\source\repos\emailwebapidotnet7\emailwebapidotnet7\test1.txt'
    'F:\source\repos\emailwebapidotnet7\emailwebapidotnet7\test2.txt'
    'F:\source\repos\emailwebapidotnet7\emailwebapidotnet7\test3.txt'
    'F:\source\repos\emailwebapidotnet7\emailwebapidotnet7\test4.txt'
)
$attachmentsConstString = """attachments"""
$attachmentDataTypeAndNameConst = "`r`n@{`r`n""@odata.type"" = ""#microsoft.graph.fileAttachment""`r`n""name"" = "
$attachmentContentTypeConst = "`r`n""contenttype"" = ""application/vnd.openxmlformats-officedocument.wordprocessingml.document"""

# Build attachment JSON
#"attachments" = @(
#             @{
#              "@odata.type" = "#microsoft.graph.fileAttachment"
#              "name" = $AttachmentFile
#              "contenttype" = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
#              "contentBytes" = $ContentBase64 } )  

$attachmentListJson = $attachmentsConstString + " = @("

$attachmentCtr = 1
foreach($Attachment in $AttachmentArray)
{
    $ContentBase64 = [convert]::ToBase64String( [system.io.file]::readallbytes($Attachment))
    $attachmentContentBytesConst = "`r`n""contentBytes"" = $ContentBase64"

	if ($attachmentCtr -gt 1)
	{
		$attachmentListJson = $attachmentListJson + "},"
	}
	$attachmentListJson = $attachmentListJson + $attachmentDataTypeAndNameConst + $Attachment + " " + $attachmentContentTypeConst + " " + $attachmentContentBytesConst
	$attachmentCtr++
}
$attachmentListJson = $attachmentListJson + "})"

Write-Output "attachmentListJson:`r`n$attachmentListJson"
