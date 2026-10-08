Cloud Development B Project 2

## Student and project details

- **Student:** Kukhanya Dlanjwa
- **Student number:** ST10364883
- **Project:** ABC Retail Cloud Storage
- **Assessment:** Project 2 — Integrating Azure Services into a Web Application
- **Source repository:** https://github.com/KhanyaD/Cloud-Development-B-Project-2
- **Deployed application:** To be added after confirming the working Azure App Service URL.
- **Module code:** Confirm the exact code on the assessment cover before submission.

## Project overview

ABC Retail Cloud Storage is an ASP.NET Core Razor Pages application that uses Azure storage services to manage customer information, products, product images, order messages and uploaded log files. Project 2 extends the Project 1 application with four function classes that coordinate storage operations through injected service interfaces.

This README documents the checked-in source and provides setup, testing and submission guidance. It does not claim that deployment or runtime tests have passed without evidence.

## Technologies

- C# and ASP.NET Core Razor Pages on .NET 8
- Microsoft Visual Studio with the ASP.NET and web development workload
- Azure Table Storage, Blob Storage, Queue Storage and Azure Files
- Azure App Service as the required web deployment target
- Azure SDK packages: `Azure.Data.Tables`, `Azure.Storage.Blobs`, `Azure.Storage.Queues` and `Azure.Storage.Files.Shares`
- Git and GitHub

## Project 2 functions

The files below are inside `Cloud Development B  project 1/Functions/`. The existing folder, solution and namespace retain their Project 1 names.

| Function class | Main operation | Storage service |
| --- | --- | --- |
| `CustomerTableFunction.cs` | Stores a customer and supplies missing partition and row keys | Azure Table Storage |
| `ProductImageFunction.cs` | Uploads product images, lists image URLs and deletes images | Azure Blob Storage |
| `OrderQueueFunction.cs` | Formats and sends order messages and peeks queued messages | Azure Queue Storage |
| `FileShareFunction.cs` | Uploads, lists, downloads and deletes log files | Azure Files |

The classes are registered in the nested project's `Program.cs` using dependency injection. Razor Page handlers call these classes, which call their storage services.

**Implementation boundary:** These are ordinary C# classes running inside the web application. The current repository does not contain a separate Azure Functions host project or trigger definitions. The assessment rubric requests evidence from an Azure Function App. That requirement still needs to be addressed or clarified with the lecturer; naming a class “Function” is not evidence of a deployed Azure Function.

## Storage resources and features

| Area | Resource | Behaviour in the source |
| --- | --- | --- |
| Customers | `Customers` table | Customer storage through the table service |
| Products | `Products` table | Product records through the table service |
| Orders | `Orders` table and `orders` queue | Order records and transaction messages |
| Images | Private `product-images` container | Upload, list and delete; image access through generated SAS URLs |
| Logs | `logs` file share | Upload, list, download and delete |

Queue reading currently uses **peek**, which leaves messages on the queue. It does not implement a worker that receives, processes and deletes messages. Storage services create their resources if they do not already exist, so using the app can write to the configured storage account.

## Repository structure

| Path | Purpose |
| --- | --- |
| `Cloud Development B  project 1.sln` | Solution pointing to the nested application project |
| `Cloud Development B  project 1/Cloud Development B  project 1.csproj` | .NET 8 web project |
| `Cloud Development B  project 1/Program.cs` | Razor Pages setup and service/function registration |
| `Cloud Development B  project 1/Models/` | Customer, product and order models |
| `Cloud Development B  project 1/Pages/` | Application pages and handlers |
| `Cloud Development B  project 1/Services/` | Azure storage interfaces and implementations |
| `Cloud Development B  project 1/Functions/` | Four Part 2 function classes |

There are also project and source files at the repository root. Use the solution and nested project listed above to work with the Part 2 implementation.

## Configure and run locally

1. Clone or download this repository and extract it if downloaded as a ZIP.
2. Install Visual Studio with the ASP.NET and web development workload and the .NET 8 SDK.
3. Open `Cloud Development B  project 1.sln`.
4. Configure the nested web project with your Azure Storage connection string under **`AzureStorage:ConnectionString`**. Keep the actual value out of GitHub.
5. Restore NuGet packages, rebuild the solution and run it with F5.
6. Open the localhost URL displayed by Visual Studio and test Customers, Products, Images, Orders and Logs.

An alternative is to run these commands from the nested project directory:

```powershell
cd "Cloud Development B  project 1"
dotnet restore
dotnet user-secrets set "AzureStorage:ConnectionString" "<YOUR_STORAGE_CONNECTION_STRING>"
dotnet build
dotnet run
```

The placeholder must be replaced locally with your own value. The project already declares a user-secrets ID. Internet access and a usable Azure Storage account are needed for storage operations.

## Deploy to Azure App Service

1. Publish the nested web project to a compatible .NET 8 Azure App Service using Visual Studio.
2. Set **`AzureStorage__ConnectionString`** in the App Service environment settings. The double underscore maps to the application's `AzureStorage:ConnectionString` key.
3. Restart the app if required and open the HTTPS deployment URL.
4. Repeat the storage tests on the deployed app and capture the actual results.
5. Add the confirmed deployment URL to this README and the submission document. The brief specifies a student-number-based `azurewebsites.net` address; confirm the hostname assigned to your deployment.

Publishing the web application to App Service does not, by itself, create the separate Function App evidence requested by the rubric.

## Local and deployed verification

Record actual outcomes for both environments. The checklist below is a test plan, not a record of passed tests.

| Check | Expected result | Evidence to capture |
| --- | --- | --- |
| Build and launch | Solution builds and the home page loads | Build output and home page |
| Customer storage | New customer is visible in the app and `Customers` table | App result and Azure table entity |
| Product storage | Product record is stored and displayed | Products page and table entity |
| Image upload | Image appears in the app and `product-images` container | Images page and blob listing |
| Order transaction | Order is stored and a message appears in `orders` | Orders page, table entity and queue message |
| Queue peek | Queued messages display without being removed | Message listing before and after peek |
| File upload | Uploaded log appears in `logs` and can be downloaded | Logs page and Azure Files listing |
| Input validation | Missing or invalid required values produce feedback | Actual validation message |
| Deployed app | The public deployment URL opens and storage operations work | Deployed pages with visible URL |
| Azure Functions requirement | Required functions run in a Function App, if required by the lecturer | Function App, function execution and storage output |

No successful build log, live deployment verification or Function App execution evidence was established during this README review.

## Discussion of services that could improve the customer experience

### Azure Event Hubs

Event Hubs ingests high-volume streams such as product views, searches and checkout activity. ABC Retail could process these events to identify trends and improve recommendations or detect checkout failures sooner. This would require producers, consumers and an analytics pipeline; Event Hubs alone does not produce personalised recommendations. This is a proposed extension, not a feature implemented in this repository. See Microsoft's [Event Hubs overview](https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-about).

### Azure Event Bus and Azure Service Bus

The brief uses the term “Azure Event Bus”. This discussion interprets that term as **Azure Service Bus**; confirm that interpretation with the lecturer. Service Bus provides queues and publish-subscribe topics for business messages. ABC Retail could use it to separate order acceptance from stock, fulfilment and notification tasks. Dead-lettering helps isolate failed messages for investigation, while consumers must handle retries and duplicate processing correctly. This could improve order reliability and customer updates. Service Bus is a proposed extension; the current app uses Azure Queue Storage. See Microsoft's [Service Bus overview](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview).

## Required Project 2 submission

The README supports the project but does not replace the required MS Word submission. Include:

- Student number and the confirmed module code.
- The working deployed web application URL and this GitHub repository link.
- Written discussion answers, diagrams where needed and referenced sources.
- Screenshots of all four required operations and their Azure storage outputs.
- Azure Function App and execution evidence requested by the rubric.
- Screenshots of the deployed application and actual local/deployed test results.
