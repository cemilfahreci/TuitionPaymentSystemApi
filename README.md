# University Tuition Payment System API

This project implements a RESTful API for a University Tuition Payment System using **.NET 9 (ASP.NET Core Web API)**. It simulates a real-world payment gateway with authentication, rate limiting, and batch processing capabilities.

## Demo Presentation Video : https://youtu.be/Hqg_ZtVJIY8

## Features

- **API Gateway**: Implemented using **Ocelot** to route all requests through a single entry point.
- **Authentication**: JWT-based secure access for Banking and Admin endpoints.
- **Role-Based Access**: Separate endpoints for Mobile (Public/Limited), Banking (Auth), and Admin (Auth).
- **Rate Limiting**: Configured in API Gateway (Ocelot) to limit mobile queries to 3 requests per student/day.
- **Paging**: Supported in Banking and Mobile query endpoints (`page`, `pageSize`).
- **Logging**: Enhanced logging middleware captures Request/Response details, Headers, and Authentication status.
- **Batch Processing**: CSV upload support for bulk tuition entry.
- **Database**: PostgreSQL (Hosted on Render.com) with Entity Framework Core.
- **Documentation**: Integrated Swagger UI (Accessible via Backend).

## Getting Started

### Prerequisites
- .NET 9.0 SDK

### Installation & Run

To run the system, you need to start both the **Backend API** and the **API Gateway**.

1.  Clone the repository:
    ```bash
    git clone https://github.com/cemilfahreci/TuitionPaymentSystemApi.git
    cd TuitionPaymentSystemApi
    ```

2.  **Start the Backend API** (Terminal 1):
    ```bash
    dotnet run --project TuitionApi.csproj --urls "http://localhost:5200"
    ```

3.  **Start the API Gateway** (Terminal 2):
    ```bash
    dotnet run --project TuitionApi.Gateway/TuitionApi.Gateway.csproj --urls "http://localhost:5247"
    ```

4.  **Access the System**:
    - **API Gateway URL**: `http://localhost:5247` (Use this for all API requests)
    - **Swagger UI**: `http://localhost:5200/swagger` (Direct Backend Access for Docs)

### Default Credentials (for Testing)
- **Login Endpoint**: `POST /gateway/api/v1/auth/login`
- **Username**: `admin`
- **Password**: `admin`

### Database Configuration
The project is pre-configured to connect to a hosted **PostgreSQL** database on Render. The connection string is already set in `appsettings.json`, so you can run the project immediately without additional setup.

## API Endpoints (via Gateway)

Base URL: `http://localhost:5247/gateway`

| Module | Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :--- | :--- |
| **Auth** | POST | `/api/v1/auth/login` | Get JWT Token | No |
| **Mobile** | GET | `/api/v1/mobile/tuition/{studentNo}?page=1&pageSize=10` | Query Tuition (Rate Limited, Paged) | No |
| **Banking** | GET | `/api/v1/banking/tuition/{studentNo}?page=1&pageSize=10` | Query Tuition (Paged) | **Yes** |
| **Banking** | POST | `/api/v1/banking/payment` | Make Payment | No |
| **Admin** | POST | `/api/v1/admin/tuition` | Add Single Tuition | **Yes** |
| **Admin** | POST | `/api/v1/admin/tuition/batch` | Upload CSV (Batch) | **Yes** |
| **Admin** | GET | `/api/v1/admin/tuition/unpaid` | List Unpaid Tuitions | **Yes** |
| **Admin** | PUT | `/api/v1/admin/tuition/{studentNo}` | Update Tuition (Term required) | **Yes** |
| **Admin** | DELETE | `/api/v1/admin/tuition/{studentNo}` | Delete Tuition (Term required) | **Yes** |

## Database Design (ER Diagram)

![ER Diagram](er_diagram.png)

## 📋 Deliverables & Design Notes

### Source Code
- **Repository Link**: [https://github.com/cemilfahreci/TuitionPaymentSystemApi](https://github.com/cemilfahreci/TuitionPaymentSystemApi)

### Design & Assumptions
- **Architecture**: The project follows a layered architecture using ASP.NET Core Web API standards with an Ocelot API Gateway.
- **Assumptions**:
    - Currency is assumed to be TRY.
    - Rate limiting is based on the server's local time (Midnight reset).
    - Student numbers are unique identifiers.
- **Issues Encountered**:
    - *Database Migration*: Initially started with SQLite but migrated to PostgreSQL to ensure data persistence on cloud deployment (Render.com).
    - *SSL Configuration*: Adjusted Npgsql connection settings to support Render's internal network requirements.

## 🧪 Quick Test Scenario (For Evaluators)

1.  **Login**: Use `POST /gateway/api/v1/auth/login` with `admin` / `admin` to get your **Bearer Token**.
2.  **Authorize**: Click the "Authorize" button in Swagger (Backend URL) and paste the token as `Bearer <YOUR_TOKEN>`.
3.  **Batch Upload**:
    - Go to `POST /api/v1/admin/tuition/batch`.
    - Upload the **`sample_students.csv`** file found in the project root directory.
    - This will populate the database with test students.
4.  **Verify**:
    - Use `GET /gateway/api/v1/admin/tuition/unpaid` to see the uploaded debts.
    - Use `GET /gateway/api/v1/mobile/tuition/{studentNo}` (e.g., `100`, `101`) to check individual status.
