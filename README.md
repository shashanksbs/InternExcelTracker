# InternExcelTracker

A full-stack application for tracking intern Excel assignments and product reports.

## Tech Stack

- **Backend**: ASP.NET Core Web API (.NET 8), Entity Framework Core, PostgreSQL
- **Frontend**: Angular 17+, Tailwind CSS
- **Authentication**: Simple Username/Role based (No JWT)

## Prerequisites

- .NET 8 SDK
- Node.js (v18+)
- PostgreSQL Database

## Setup & Run

### 1. Database Setup

Ensure PostgreSQL is running.
Update the connection string in `backend/InternExcelTracker.Api/appsettings.json` if necessary.

The application will automatically apply migrations on startup if configured, or you can run:

```bash
cd backend/InternExcelTracker.Api
dotnet ef database update
```

### 2. Backend

```bash
cd backend/InternExcelTracker.Api
dotnet run
```
The API will be available at `http://localhost:5282`.

### 3. Frontend

```bash
cd frontend/intern-tracker-ui
npm install
npm start
```
The application will be available at `http://localhost:4200`.

## Features

### Admin
- Log in with role "Admin".
- Upload Excel files for interns.
- View performance stats and approve/reject reports.

### Intern
- Log in with role "Intern".
- Download assigned Excel files.
- Submit product reports with details (validations included).
- View status of submitted reports.

## Default Users

Register a new user via the Register page.
- Select role "Admin" for admin access.
- Select role "Intern" for intern access.
