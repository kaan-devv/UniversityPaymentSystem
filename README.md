# University Payment System API (Group 2)

This project is a cloud-based **University Tuition Payment System API** developed for the SE 4458 Software Architecture course. The system handles student debt queries and payment processing via external clients.

## 🚀 Key Features and Hosting

* **Platform:** .NET 8.0 (Core Web API)
* **Database:** AWS RDS (SQL Server)
* **Authentication:** JWT (JSON Web Token) based authentication.
* **API Gateway:** Ocelot (For Rate Limiting, Logging, and Routing).
* **Hosting:** The entire system is hosted on **Azure App Service (Linux)**, demonstrating cloud deployment.

---

## 🏗️ Architecture & Design Decisions

1.  **Deployment Strategy (Resource Efficiency):**
    * The **API** and the **Gateway** share a single **Free (F1) App Service Plan** in Azure to host both applications, demonstrating resource efficiency and meeting cloud hosting requirements.
    * The Gateway's routing rules were updated from `localhost` to the live Azure HTTPS URL.

2.  **Security & Compliance:**
    * **Rate Limiting:** Mobile tuition queries are strictly limited to **3 requests per day** at the Gateway level, as required.
    * **Custom Logging:** The Gateway includes custom middleware to log critical Request/Response details (IP, Latency, Status Code) for monitoring purposes.
    * **JWT Auth:** Banking App and Admin endpoints require a valid JWT Token.

---

## 🗂️ Data Model (ER Diagram)

The database schema and key relationships are defined as follows:

![Database ER Diagram](ERDiagram.png)

---

## ⚠️ Challenges and Solutions (Critical for Grading)

This project involved several non-trivial issues solved during development and deployment:

* **Quota Exceeded (App Service Plan Creation):**
    * *Problem:* Attempting to create a second Free (F1) App Service Plan resulted in a "Quota Exceeded" error, blocking the deployment of the Gateway.
    * *Solution:* The issue was resolved by deploying the Gateway to **reuse the existing App Service Plan** that was created for the API, successfully avoiding the quota limit.

* **Port Binding Conflict in Azure:**
    * *Problem:* The Gateway failed to start after deployment because its code hardcoded the local development port 5200, which conflicts with Azure's required port 8080.
    * *Solution:* The hardcoded `builder.WebHost.UseUrls("...5200")` line was removed from the Gateway's `Program.cs`, allowing the application to correctly bind to the port provided by the Azure environment.

* **Silent Deployment Failure (Missing Config):**
    * *Problem:* Initial deployment attempts failed to copy essential configuration files (`ocelot.json`) and core DLLs to the Azure server.
    * *Solution:* The deployment strategy was changed to use the **`dotnet publish`** command to create a robust package, and the Gateway's `.csproj` file was updated to explicitly force copy `ocelot.json`.

* **Project Structure Conflict:**
    * *Problem:* The Gateway folder was initially nested inside the API folder, causing compilation errors (`CS8802`) due to conflicting startup code.
    * *Solution:* The Gateway folder was manually moved out to a sibling directory of the API folder, resolving the structural compilation conflict.

* **JWT Key Size Error:**
    * *Problem:* The original JWT secret key in `appsettings.json` was too short (184 bits), causing a runtime failure as the HS256 algorithm requires a key length of at least 256 bits.
    * *Solution:* The secret key value in `appsettings.json` was lengthened to meet the security algorithm's requirement.

---

## 🛠️ Access and Final Deliverables

**1. Live Deployment URL (Gateway Entry Point):**

**⚠️ Update this link with the final `uni-gateway-api` domain name after deployment.**

`https://uni-gateway-api-[your-suffix].azurewebsites.net/gateway/mobile/tuition/{studentNumber}`

**2. Local Setup:**
* Run the API: `cd UniversityPaymentApi` then `dotnet run`
* Run the Gateway: `cd UniversityGateway` then `dotnet run`

**3. Video Submission:**
* A short video demonstrating **Rate Limiting** (showing the 429 error) and **Gateway Logging** is required.