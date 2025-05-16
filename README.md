# 8BallPool

ASP.NET Core Web API project using Entity Framework Core, with support for AWS S3 or Azure Blob Storage for file management (e.g. profile pictures).

---

## 1. Setup & Installation

### Prerequisites

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- NuGet (comes with .NET SDK)
- Run this on your terminal to generate a project `dotnet new webapi -n 8-ball-pool`

### NuGet Package Dependencies

```xml
<PackageReference Include="AWSSDK.S3" Version="4.0.0.3" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.4" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
<PackageReference Include="DotNetEnv" Version="2.5.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="8.1.1" />
```
(Note: If using Azure Blob Storage, ensure the `Azure.Storage.Blobs` package is added to your .csproj file.)

## 2. Database Setup

1.  **Configure Connection String**:
    Update the `ConnectionStrings` section in your `appsettings.json` file.
    Example for SQL Server:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=8BallPoolDB;Trusted_Connection=True;MultipleActiveResultSets=true"
    }
    ```
    Example for PostgreSQL (ensure your `Program.cs` or `DbContext` is configured to use Npgsql if this is the primary choice):
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Database=8BallPoolDB;Username=youruser;Password=yourpassword"
    }
    ```
    Alternatively, you can set the connection string via an environment variable that your application reads (e.g., `DATABASE_URL`).

2.  **Apply Migrations**:
    Open a terminal in the project root directory and run the following command to apply Entity Framework Core migrations and create/update your database schema:
    ```bash
    dotnet ef database update
    ```
    If you haven't created initial migrations yet, you would first run:
    ```bash
    dotnet ef migrations add InitialCreate
    ```

## 3. Environment Variables
Create a `.env` file in the root of your project. The `DotNetEnv` package is used to load these variables.

### Required Environment Variables:

-   `DB_CONNECTION_STRING` (If you prefer to set the connection string via env var instead of `appsettings.json`)

#### AWS S3 Configuration:
-   `AWS_ACCESS_KEY_ID`
-   `AWS_SECRET_ACCESS_KEY`
-   `S3_BUCKET_NAME`
-   `AWS_REGION` (e.g., "us-east-1")

#### Azure Blob Storage Configuration (if used):
-   `AZURE_STORAGE_CONNECTION_STRING` (Connection string for your Azure Storage account)
-   `AZURE_BLOB_CONTAINER_NAME` (Name of the container for storing blobs)

---
Ensure your application is configured to read these variables, especially for selecting between AWS and Azure if both are supported.

## 4. Docker Setup

This project includes Docker support for easier development and deployment.

### Prerequisites
- [Docker](https://www.docker.com/get-started) installed on your machine
- [Docker Compose](https://docs.docker.com/compose/install/) (usually included with Docker Desktop)

### Running with Docker

1. **Environment Setup**:
   Create a `.env` file in the root directory based on the `.env.example` file:
   ```bash
   cp .env.example .env
   ```
   Then edit the `.env` file to add your AWS credentials and other environment variables.

2. **Build and Start Containers**:
   ```bash
   docker-compose up -d
   ```
   This will build the application and start both the app and PostgreSQL containers.

3. **Access the Application**:
   Once the containers are running, you can access the application at:
   ```
   http://localhost:8080
   ```
   The Swagger UI is available at:
   ```
   http://localhost:8080/swagger
   ```

4. **Database Migrations**:
   When running in Docker for the first time, you may need to apply migrations:
   ```bash
   docker-compose exec app dotnet ef database update
   ```

5. **Stop Containers**:
   ```bash
   docker-compose down
   ```
   To remove volumes as well (will delete database data):
   ```bash
   docker-compose down -v
   ```

### Docker Commands Reference

- View running containers:
  ```bash
  docker-compose ps
  ```

- View container logs:
  ```bash
  docker-compose logs -f app
  docker-compose logs -f postgres
  ```

- Connect to PostgreSQL container:
  ```bash
  docker-compose exec postgres psql -U pooluser -d poolapp
  ```

- Execute commands in the app container:
  ```bash
  docker-compose exec app /bin/bash
  ```

