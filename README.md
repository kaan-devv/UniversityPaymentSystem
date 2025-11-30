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

## ⚠️ Challenges and Solutions

This project involved several non-trivial issues solved during development and deployment:

* **Quota Exceeded (App Service Plan Creation):**
    * *Problem:* Attempting to create a second Free (F1) App Service Plan resulted in a "Quota Exceeded" error.
    * *Solution:* The Gateway was successfully deployed by configuring it to **reuse the existing App Service Plan** that was created for the API, circumventing the plan creation limit.

* **Port Binding Conflict in Azure:**
    * *Problem:* The Gateway failed to start because its code hardcoded the local development port 5200, preventing Azure from communicating with the application on port 8080.
    * *Solution:* The hardcoded `UseUrls` line was removed from the Gateway's `Program.cs`, allowing the application to properly bind to the port provided by the Azure environment.

* **Silent Deployment Failure (Missing Config):**
    * *Problem:* The initial deployment method failed to copy configuration files (`ocelot.json`) and core DLLs.
    * *Solution:* The deployment strategy was changed to use the **`dotnet publish`** command to create a complete package, and the Gateway's `.csproj` file was updated to explicitly force copy `ocelot.json`.

* **Project Structure Conflict:**
    * *Problem:* Moving the Gateway project inside the API project folder caused compilation errors (`CS8802`).
    * *Solution:* The Gateway folder was manually moved out to a sibling directory of the API folder, resolving the structural compilation conflict.

* **JWT Key Size Error:**
    * *Problem:* The original JWT secret key was too short (184 bits), causing a runtime failure.
    * *Solution:* The secret key value was increased in length to meet the security algorithm's requirement (256 bits).

---

## ✅ Live Capabilities Verification

The following features are functional and verifiable on the live Azure deployment:

* **End-to-End Connectivity:** Accessing the mobile endpoint successfully returns a JSON response (or "Not Found" if the student ID is invalid), confirming the path from **Browser → Gateway → API → AWS RDS** is fully operational.

* **Rate Limiting:** The Mobile App query is successfully blocked. Making the same request **4 times consecutively** results in a **`429 Too Many Requests`** error.

* **Authentication (JWT):** Accessing the Banking App endpoint without a token correctly returns a **`401 Unauthorized`** error.

* **Custom Logging:** The Azure Log Stream displays unique, color-coded log entries (including **IP**, **Latency**, and **HTTP Status Code**) for every transaction processed by the Gateway.

---

## 🛠️ Final Deliverables

**1. Live Deployment URL (Gateway Entry Point):**

**⚠️ Update this link with the final `uni-gateway-api` domain name after deployment.**

`https://uni-gateway-api-[your-suffix].azurewebsites.net/gateway/mobile/tuition/{studentNumber}`

**Swagger UI Adress (Live):** `https://uni-payment-api-decgg9f8bhdtgfd8.westeurope-01.azurewebsites.net/swagger/index.html`

**2. Local Setup:**
* Run the API: `cd UniversityPaymentApi` then `dotnet run`
* Run the Gateway: `cd UniversityGateway` then `dotnet run`
