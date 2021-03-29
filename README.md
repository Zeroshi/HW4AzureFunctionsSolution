# HW4AzureFunctionsSolution

CSCI E-94 Assignment #4
03/10/2021 Page 1 of 18 Version 4.0.0
Overview:
In this exercise you will create six Azure Functions using Version 3.x of Azure Functions and .NET Core 3.1
1. ImageConsumerGreyScale
Triggers off blob storage container converttogreyscale. Converts images uploaded into GreyScale. Upon success stores the result image into a blob container called convertedimages, upon failure stores the original image into a blob container called failedimages.
2. ImageConsumerSepia
Triggers off blob storage container converttosepia. Converts images uploaded into sepia. Upon success stores the result image into a blob container called convertedimages, upon failure stores the original image into a blob container called failedimages.
3. ImageStatusUpdaterSuccess
Triggers off blob storage container convertedimages updating the job status entity in the imageconversionjobs Azure Storage Table with success indication.
4. ImageStatusUpdaterFailed
Triggers off blob storage container failedimages updating the job status entity in the imageconversionjobs Azure Storage table with failed indication.
5. ConversionJobStatus
Implements a REST interface that supports returning all jobs in the imageconversionjobs Azure Storage table.
6. ConversionJobStatusById
Implements a REST interface that supports returning the job status information for a specific job by job id from the imageconversionjobs Azure Storage table.
Be sure to read the Implementation notes: section as it has important guidelines and restrictions.
CSCI E-94 Assignment #4
03/10/2021 Page 2 of 18 Version 4.0.0
Goals
- Setup and configure.
o Azure Functions
- Familiarize yourself with
o Azure Functions
o Azure Function Triggers
o Azure Function Outputs
- Gain experience with:
o Azure Table Storage
o Azure Blob Storage
o Azure Portal & Creating services in Azure
o Debugging Azure Functions
Contents
Goals ............................................................................................................................................................. 2
Architectural Overview ................................................................................................................................. 3
1. Azure Function: ImageConsumerGreyScale .............................................................................................. 9
2. Azure Function: ImageConsumerSepia ................................................................................................... 10
3. Azure Function: ImageStatusUpdaterSuccess ........................................................................................ 10
4. Azure Function: ImageStatusUpdaterFailed ........................................................................................... 10
5. Azure Function: ConversionJobStatus .................................................................................................... 11
6. Azure Function: ConversionJobStatusById ............................................................................................. 14
7. ErrorResponse JSON................................................................................................................................ 16
8. Create an Azure Function using Consumption Service Plan ................................................................... 17
9. Use your existing storage account .......................................................................................................... 17
10. Upload and convert the following data ................................................................................................ 17
Implementation notes: ............................................................................................................................... 18
Other Notes: ............................................................................................................................................... 18
Extra Credit: ................................................................................................................................................ 18
CSCI E-94 Assignment #4
03/10/2021 Page 3 of 18 Version 4.0.0
Architectural Overview
Legend
Write Operation
Error Operation
Trigger Operation
Storage table
imageconversionjobs
Storage (Azure)
convertogreyscale
Azure Function
ImageConsumerGreyScale
Blob Triggered
convertogreyscale
Azure Function
ImageConsumerSepia
Blob Triggered
convertosepia
Azure Function
ImageStatusUpdaterSuccess
Blob Triggered
convertedimages
Azure Function
ImageStatusUpdaterFailed
Blob Triggered
failedimages
Azure Function
ConversionJobStatus
HTTP Trigger REST GET
jobs
Storage (Azure)
convertosepia
Storage (Azure)
convertedimages
Storage (Azure)
failedimages
Read Operation
1
2
3
4
5
Human Operation
Azure Function
ConversionJobStatusById
HTTP Trigger REST GET
job by id
6
CSCI E-94 Assignment #4
03/10/2021 Page 4 of 18 Version 4.0.0
Solution, Project Names & Technology
You shall create one Solution containing one Visual Studio Project. The solution shall be called HW4AzureFunctionsSolution
The solution shall contain one project:
1. This project shall be called HW4AzureFunctions
Note: Do not include any Azure credential and/or connection string information in your submitted homework other than the Shared Access Signature (SAS) connection information described and the Azure Function access code(s) allowing the TA to call your Azure Function(s).
As stated in the syllabus: The completed assignment shall be placed in a single zip file that preserves the directory structure. The zip file shall contain:
a) All assets such as configuration files, images, initialization scripts and resources that are part of the application. The TA must be able to build and deploy your homework assignment to Azure and experience the full functionality of your application.
b) A text file named ProjectNotes.txt that includes.
1) The homework assignment tile and number
2) Your name and email address
3) Any notes for the TA that may be needed for the TA to install, setup, login to and operate your homework assignment.
CSCI E-94 Assignment #4
03/10/2021 Page 5 of 18 Version 4.0.0
In addition, you shall provide:
1. The URL to the ConversionJobsStatus & ConversionJobsStatusById azure functions along with the function credentials (code) in your ProjectNotes.txt file.
2. The Shared Access Signature to your storage account including read, write, delete, list, add, create and update access to all allowed services and resource types. Also, be sure the start date is 2021-03-10 and the end date is 2021-04-30, see settings shown below:
To create the Shared Access Signature (SAS), (1) select key2 and (2) press the Generate SAS button. Copy the urls identified by steps (3), (4), (5) and (6) shown below and put them in your ProjectNotes.txt file.
CSCI E-94 Assignment #4
03/10/2021 Page 6 of 18 Version 4.0.0
Required Nuget Package for Web Job
You shall use the SixLabors.ImageSharp nuget package for all image processing functions.
Nuget information is here: https://www.nuget.org/packages/SixLabors.ImageSharp/1.0.3
Documentation is available here: https://github.com/SixLabors/ImageSharp
Choose Manage Nuget Packages:
Select SixLabors.ImageSharp Version 1.0.3
Choose Microsoft.Azure.Webjobs.Extensions.Storage Version 3.0.11
CSCI E-94 Assignment #4
03/10/2021 Page 7 of 18 Version 4.0.0
To verify you have all the nuget packages installed, right click on your HW4AzureFunctions project and choose Manage Nuget Packages…
CSCI E-94 Assignment #4
03/10/2021 Page 8 of 18 Version 4.0.0
You should then see the following. Don’t use version 5.x as that requires .NET 5
CSCI E-94 Assignment #4
03/10/2021 Page 9 of 18 Version 4.0.0
1. Azure Function: ImageConsumerGreyScale
This azure function will be triggered when images are uploaded into the converttogreyscale container. It will then write out a record to the imageconversionjobs with a status of 1 and an imageConversionMode of GreyScale. It will then convert the image to a grey scale image and store the resultant image as a blob in the convertedimages container if the conversion was a success or the failedimages container if conversion failed. When your code enters the stage where it's about to convert the image, change the status in the jobs table to 2. When initially setting or updating the status ensure that you also update the statusDescription with the human readable description of the status as defined in the job status table definition.
Assign the blob a unique ID, with the original blob name appended, using the .NET class Guid. This is the id of the blob that will be put into the convertedimages or failedimages depending on success or failure of conversion. The format is <guid>-<original blobId>
For example, if the original blob id was dogs.jpg, the resultant converted image blob id would be e55286cd-9464-47d8-b399-62dd0355fa92-dogs.jpg.
Assign the job a different unique ID using the .NET class Guid.
Assign a partition key of: imageconversions
Ensure that the job record contains the Azure public url to the uploaded image in the imageSource column.
Ensure that the job record contains the imageConversionMode set to GreyScale
CSCI E-94 Assignment #4
03/10/2021 Page 10 of 18 Version 4.0.0
2. Azure Function: ImageConsumerSepia
This azure function will be triggered when images are uploaded into the converttosepia container. It will then write out a record to the imageconversionjobs with a status of 1 and an imageconversionmode of Sepia. It will then convert the image to a sepia image and store the resultant image as a blob in the convertedimages container if the conversion was a success or the failedimages container if conversion failed. When your code enters the stage where it's about to convert the image, change the status in the jobs table to 2. When initially setting or updating the status ensure that you also update the statusDescription with the human readable description of the status as defined in the job status table definition.
Assign the blob a unique ID, with the original blob name appended, using the .NET class Guid. This is the id of the blob that will be put into the convertedimages or failedimages depending on success or failure of conversion. The format is <guid>-<original blobId>
For example, if the original blob id was dogs.jpg, the resultant converted image blob id would be e55286cd-9464-47d8-b399-62dd0355fa93-dogs.jpg.
Assign the job a different unique ID using the .NET class Guid.
Assign a partition key of: imageconversions
Ensure that the job record contains the Azure public url to the uploaded image in the imageSource column.
Ensure that the job record contains the imageConversionMode set to Sepia
3. Azure Function: ImageStatusUpdaterSuccess
This azure function will be triggered when images are uploaded into the convertedimages container. Update the job with the status of 3. Also update the statusDescription with the human readable description of the status as defined in the job status table definition. Update the imageResult property with the Azure public url to the converted image.
4. Azure Function: ImageStatusUpdaterFailed
This azure function will be triggered when images are uploaded into the failedimages container. Update the job with the status of 4. Also update the statusDescription with the human readable description of the status as defined in the job status table definition. Update the imageResult property with the Azure public url to the failed image.
CSCI E-94 Assignment #4
03/10/2021 Page 11 of 18 Version 4.0.0
5. Azure Function: ConversionJobStatus
API Operation – Retrieve all jobs
Retrieves all jobs using Azure Function-based authentication, be sure to provide the key (code) to your TA in the project notes.txt file HTTP GET https://[functionappname].azurewebsites.net/api/v1/jobs?code=xxxxx
URI Parameters Name Required JSON Type Length Constraints Description
code
true
String
N/A
1. Required
The function key for the azure function
Be sure to provide this to your TA in the ProjectNotes.txt file
Request Body
None
Responses
200 (OK)
Returned regardless of whether there are any jobs to return.
Headers:
None, beyond standard
CSCI E-94 Assignment #4
03/10/2021 Page 12 of 18 Version 4.0.0
Response Body:
The blob names.
[{
"jobId":"<id 1>",
"imageConversionMode":"<image conversion mode>",
"status":<job status>,
"statusDescription":"<error description or status text>",
"imageSource":"<the url to retrieve the image used for conversion>",
"imageResult":"<the url to retrieve the image converted or failed conversion>"
},
{
"jobId":"<id 2>",
"imageConversionMode":"<image conversion mode>",
"status":<job status>,
"statusDescription":"<error description or status text>",
"imageSource":"<the url to retrieve the image used for conversion>",
"imageResult":"<the url to retrieve the image converted or failed conversion >"
}
]
CSCI E-94 Assignment #4
03/10/2021 Page 13 of 18 Version 4.0.0
Job status table definition Name JSON Type Length Description
jobId
String
36
This the id of the job. Note unlike previous homework's this ID is a different ID than the ID than the ID Assigned to the converted image.
imageConversionMode
String
N/A
This is the image conversion mode that the job will perform.
Valid values are:
GreyScale
Sepia
status
Number
N/A
A number indicating the status of the job
1 = Image Obtained
2 = Image Being Converted
3 = Image Converted with success
4 = Image Failed Conversion
statusDescription
String
512
If no error occurred this value contains the text associated with the status defined above.
Ex: Conversion completed with success
If an error occurred a human readable description of the problem running converting the image. Always prefix this with "Job failed:" followed by a more descriptive message. This can be the message from an exception that occurred but NOT THE STACK TRACE.
Ex:
Image Conversion Failed: The uploaded image could not be converted.
imageSource
String
512
This is the url to the blob storage entry for the image uploaded to be converted.
imageResult
String
512
This is the url to the blob storage entry for the blob that contains the converted or failed to be converted image.
CSCI E-94 Assignment #4
03/10/2021 Page 14 of 18 Version 4.0.0
6. Azure Function: ConversionJobStatusById
API Operation – Retrieve job by id.
Retrieves a job by its Id using azure function-based authentication, be sure to provide the key to your TA in the project notes.txt file. HTTP GET https://[functionappname].azurewebsites.net/api/v1/jobs/id?code=xxxxx
URI Parameters Name Required JSON Type Length Constraints Description
id
true
String
36
1. Required
2. Must Exist
This the id of the job. Note unlike previous homework's this ID is a different ID than the ID than the ID Assigned to the converted image.
.NET Type: String
code
true
String
N/A
1. Required
The function key for the azure function
Be sure to provide this to your TA in the ProjectNotes.txt file
Responses
200 (OK)
Returned if the job Id exists and the job is returned.
Headers:
None, beyond standard
Response Body:
The information about the job corresponding to the job id specified. See the Job status table definition for the definition of the payload described below.
{
"jobId":"<id>",
"imageConversionMode":"<image conversion mode>",
"status":<job status>,
"statusDescription":"<error description or status text>",
"imageSource":"<the url to retrieve the image used for conversion>",
"imageResult":"<the url to retrieve the image converted or failed conversion>"
}
CSCI E-94 Assignment #4
03/10/2021 Page 15 of 18 Version 4.0.0
404 (Not Found)
Returned if the job could not be found.
Headers:
None, beyond standard
Response Body:
Return an error response using the ErrorResponse JSON format as presented below. See: ErrorResponse definition.
Example: Here the service name is myazurefunctionapp and the job Id specified, 43, does not exist HTTP GET https://myazurefunctionapp.azurewebsites.net/api/v1/jobs/43
{
"errorNumber":3,
"parameterName":"id",
"parameterValue":"43"
"errorDescription":"The entity could not be found."
}
{
"errorNumber":3,
"parameterName":"id",
"parameterValue":"<value provided that caused the error >",
"errorDescription":"The entity could not be found."
}
CSCI E-94 Assignment #4
03/10/2021 Page 16 of 18 Version 4.0.0
7. ErrorResponse JSON
This is the standard error response that shall be returned. Name Required JSON Type Length Description
errorNumber
true
number
N/A
Numeric error that represents the issue
.NET Type: int
parameterName
false
string
1024
The name of the parameter that has the issue.
If the error is not tied to a specific parameter, then this value can be null.
.NET Type: string
parameterValue
false
string
2048
The value of the parameter caused the error.
If the error is not tied to a specific parameter, then this value can be null.
.NET Type: string
errorDescription
true
string
1024
A description of the error, not localized, intended for developer consumption.
.NET Type: string
{
"errorNumber":<error number>,
"parameterName":"<name of parameter that caused the error>",
"parameterValue":"<value of parameter that caused the error>",
"errorDescription":"<Description of the error intended developer consumption>"
}
CSCI E-94 Assignment #4
03/10/2021 Page 17 of 18 Version 4.0.0
Error numbers and descriptions
When required to return an ErrorResponse JSON object, this table defines the standard error numbers and their corresponding error descriptions. errorNumber parameterName parameterValue errorDescription
1
Required
Required
The entity already exists
2
Required
Required
The parameter is required.
3
Required
Required
The entity could not be found
4
Required
Null
The parameter cannot be null
8. Create an Azure Function using Consumption Service Plan
Create an Azure Function App and deploy your REST interface to it
Be sure to provide the URL to your REST interface endpoints and azure function keys in your ProjectNotes.txt file.
9. Use your existing storage account.
This Azure Storage Account shall contain the blobs, and table needed for this assignment. You can use the same Azure Account used for previous assignments.
10. Upload and convert the following data.
Use your API to upload the following files with the specified conversion mode.
FileName imageConversionMode
dogs.jpg
GreyScale seagull.jpg Sepia
turkey.jpg
GreyScale
usconstitution.pdf
Sepia
CSCI E-94 Assignment #4
03/10/2021 Page 18 of 18 Version 4.0.0
Implementation notes:
1. You may not use / employ.
a. The samples from the Azure Dashboard or other samples you must write this yourself.
2. All exceptions must be handled, the service must not return any exception information to the caller and must not return the HTTP Status 500 family of errors to the caller.
3. Use built in exceptions to handle error exceptional cases wherever possible.
4. Map API App Service REST errors to standard HTTP Error codes as defined in the requirements.
a. If a situation is not specified in the requirements pick a standard HTTP status code and document under what conditions, it is returned in your ProjectNotes.txt file.
5. See the syllabus for additional information about what needs to be submitted for all assignments.
Other Notes:
• Up to 15 Points shall be deducted for lack of comments in code.
• All methods, classes etc. must be documented.
a. C# with XML comments.
• Your project must build with no errors or warning.
Extra Credit:
The chosen extra credit item must be fully implemented to receive any credit.
1. Create a Timer Triggered Azure function.
That deletes all images successfully converted from the converttogreyscale and converttosepia containers. Configure this function to run every 2 minutes.
