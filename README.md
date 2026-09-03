# 🔮 AstroConnect

AstroConnect is a full-stack, role-based astrology appointment booking and management system built with **ASP.NET Core MVC**, **.NET 9**, **Entity Framework Core**, **SQL Server**, and **ASP.NET Core Identity**.

The application provides a complete workflow for customers to book astrology consultations while allowing receptionists and administrators to manage appointments, customers, services, astrologers, and users through dedicated role-based portals.

AstroConnect is deployed on **AWS** with the web application hosted using **AWS Elastic Beanstalk** and the production SQL Server database hosted privately using **Amazon RDS**.

---

## 🌐 Live Demo

AstroConnect is deployed on AWS and is publicly accessible.

### 🔗 Live Application

http://astroconnect.ap-south-1.elasticbeanstalk.com

> The current portfolio deployment uses HTTP on a single-instance AWS Elastic Beanstalk environment to keep infrastructure costs minimal.

The deployed application has been tested successfully on both desktop and mobile devices.

---

## ✨ Features

### 👤 Customer Portal

- Customer registration and secure login
- Personal customer dashboard
- Create astrology consultation bookings
- Select service and astrologer
- Appointment date and time-slot selection
- Booking conflict prevention
- View booking history
- View booking details
- Cancel eligible appointments
- Manage customer profile
- Receive booking notifications
- Track appointment status

### 🧑‍💼 Receptionist Portal

- Dedicated receptionist dashboard
- View customer bookings
- View detailed booking information
- Confirm pending appointments
- Complete confirmed appointments
- Cancel eligible appointments
- Calendar-based booking view
- View customer information
- Booking status management

### 🛡️ Admin Portal

- Administrative dashboard
- User management
- Role management
- Service management
- Astrologer management
- Customer management
- Booking management
- Booking status control
- Protected Admin-only management endpoints

---

## 📅 Booking Workflow

AstroConnect supports a structured appointment lifecycle:

```text
Customer creates booking
        ↓
      Pending
        ↓
Receptionist/Admin confirms
        ↓
     Confirmed
        ↓
Consultation takes place
        ↓
     Completed
```

Bookings can also be cancelled when permitted.

The system validates appointment dates and times and prevents conflicting bookings for the same astrologer.

---

## 🕒 Appointment Rules

The booking system includes server-side validation for:

- Valid booking dates
- Start and end times
- 30-minute appointment duration
- Business hours
- 30-minute time-slot alignment
- Past-date prevention
- Same-day past-time prevention
- Active services
- Active astrologers
- Booking conflict prevention

---

## 🧰 Technology Stack

### Backend

- C#
- ASP.NET Core MVC
- .NET 9
- Entity Framework Core
- ASP.NET Core Identity

### Frontend

- HTML5
- CSS3
- Bootstrap
- JavaScript
- Razor Views
- Font Awesome

### Database

- Microsoft SQL Server
- Entity Framework Core Code First
- Entity Framework Core Migrations
- Amazon RDS for SQL Server Express

### Cloud & Deployment

- AWS Elastic Beanstalk
- Amazon EC2
- Amazon RDS
- AWS Secrets Manager
- Amazon Linux 2023
- AWS Security Groups

### Development Tools

- Visual Studio 2022
- SQL Server Express
- Git
- GitHub

---

## 🏗️ Architecture

AstroConnect follows a layered **Clean Architecture** approach.

```text
AstroConnect
│
├── AstroConnect.Domain
│   ├── Entities
│   ├── Enums
│   └── Common
│
├── AstroConnect.Application
│   └── Interfaces
│       └── Repositories
│
├── AstroConnect.Infrastructure
│
├── AstroConnect.Persistence
│   ├── Context
│   ├── Identity
│   ├── Migrations
│   └── Repositories
│
├── AstroConnect.Web
│   ├── Controllers
│   ├── ViewModels
│   ├── Views
│   └── wwwroot
│
└── AstroConnect.Tests
```

This architecture separates domain models, application contracts, infrastructure, data access, Identity, and presentation concerns.

The project also uses the **Repository Pattern** to separate data-access logic from the presentation layer.

---

## ☁️ AWS Production Architecture

The production version of AstroConnect is deployed using AWS cloud services.

```text
Desktop / Mobile Browser
          │
          ▼
     HTTP Request
          │
          ▼
AWS Elastic Beanstalk
          │
          ▼
Amazon EC2
.NET 9 / Amazon Linux 2023
          │
          ▼
ASP.NET Core MVC Application
          │
          ▼
Entity Framework Core
          │
          ▼
Amazon RDS
SQL Server Express
```

### Production Infrastructure

**AWS Elastic Beanstalk**

Hosts and manages the ASP.NET Core web application.

**Amazon EC2**

Runs the deployed .NET 9 application using Amazon Linux 2023.

**Amazon RDS**

Hosts the production Microsoft SQL Server Express database.

**AWS Secrets Manager**

Stores the production database connection string outside the application source code.

**AWS Security Groups**

Control communication between the web application and the private database.

The RDS database is not publicly accessible and accepts SQL Server traffic from the application's AWS security group.

---

## 👥 User Roles

AstroConnect uses role-based authorization with three primary roles.

| Role | Main Responsibilities |
| --- | --- |
| Admin | Manages users, services, astrologers, customers and bookings |
| Receptionist | Manages appointment workflow and views customer information |
| Customer | Creates bookings, manages profile and tracks appointments |

Sensitive management actions are protected using ASP.NET Core role-based authorization.

---

## 🔐 Authentication & Authorization

AstroConnect uses **ASP.NET Core Identity** for authentication and user management.

The authentication system includes:

- Customer registration
- Secure login and logout
- Password hashing through ASP.NET Core Identity
- Role-based access control
- Admin authorization
- Receptionist authorization
- Customer authorization
- Account lockout protection
- Unique email configuration
- Customer-specific resource protection

Users are redirected to functionality appropriate to their assigned role.

---

## 🔐 Security

The application includes several security measures:

- ASP.NET Core Identity authentication
- Role-based authorization
- Admin-only management endpoints
- Customer-specific resource authorization
- Anti-forgery validation on POST operations
- Account lockout protection
- Unique email configuration
- Secure password handling through ASP.NET Core Identity
- Production HSTS support
- Production exception handling
- Development configuration excluded from Git
- No database passwords stored in the repository
- Production database hosted privately on Amazon RDS
- AWS security-group-based database access
- Production connection string managed through AWS Secrets Manager
- Production secrets excluded from source control

---

## 🗃️ Database

The application uses **Microsoft SQL Server** with **Entity Framework Core Code First migrations**.

### Main Entities

- Customer
- Astrologer
- Service
- Booking
- Notification
- ApplicationUser

Soft deletion is used for important business records where applicable.

### Local Development

SQL Server Express can be used for local development.

### Production

The deployed application uses:

**Amazon RDS for Microsoft SQL Server Express**

Entity Framework Core migrations are used to maintain the production database schema.

---

## 🚀 Getting Started

### Prerequisites

Install:

- .NET 9 SDK
- Visual Studio 2022 or another compatible IDE
- SQL Server / SQL Server Express
- Entity Framework Core CLI tools
- Git

---

### 1. Clone the Repository

```bash
git clone <your-repository-url>
cd AstroConnect
```

> Replace `<your-repository-url>` with the URL of this GitHub repository.

---

### 2. Configure the Development Database

The repository intentionally does not include the local `appsettings.Development.json`.

Create:

```text
AstroConnect.Web/appsettings.Development.json
```

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AstroConnectDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Replace `YOUR_SERVER` with your local SQL Server instance.

> Never commit database credentials, passwords, AWS secrets, or production connection strings to GitHub.

---

### 3. Apply Database Migrations

From the solution directory:

```bash
dotnet ef database update --project AstroConnect.Persistence --startup-project AstroConnect.Web
```

---

### 4. Run the Application

```bash
dotnet run --project AstroConnect.Web
```

Open the local URL displayed in the terminal.

---

## 🚀 Production Deployment

AstroConnect has been successfully deployed to AWS.

### Deployment Process

The production deployment includes:

1. Building the application using the Release configuration
2. Publishing the ASP.NET Core application
3. Deploying the published application to AWS Elastic Beanstalk
4. Running the application using .NET 9 on Amazon Linux 2023
5. Hosting the production database on Amazon RDS
6. Managing the database connection string through AWS Secrets Manager
7. Restricting RDS access using AWS Security Groups
8. Applying Entity Framework Core migrations to the production database
9. Verifying registration, login and database persistence in production

### Live Environment

```text
AWS Region:
Asia Pacific (Mumbai)

Application Platform:
.NET 9 on Amazon Linux 2023

Database:
Amazon RDS - Microsoft SQL Server Express
```

---

## 📸 Screenshots

### Customer Dashboard



### Create Booking



### Booking History



### Receptionist Dashboard



### Receptionist Calendar



### Admin Dashboard



---

## 📱 Responsive Design

AstroConnect provides a responsive interface designed for:

- Desktop
- Tablet
- Mobile

Bootstrap and custom responsive styling are used throughout the Customer, Receptionist, and Admin portals.

The deployed application has also been tested from a mobile device through the public AWS environment.

---

## 🧪 Validation & Reliability

The application includes:

- Server-side model validation
- Booking conflict validation
- Appointment time validation
- Role and authorization checks
- Customer ownership checks
- Soft deletion
- EF Core migration management
- Central production exception handling
- Production database persistence
- Structured application logging

---

## ✅ Production Verification

The AWS production deployment has been tested for:

- Public application access
- Desktop browser access
- Mobile browser access
- Customer registration
- Customer login
- ASP.NET Core Identity persistence
- Production database connectivity
- Entity Framework Core database operations
- Responsive UI rendering
- Static CSS and JavaScript resources

This verifies the complete production flow:

```text
User
  ↓
AWS Elastic Beanstalk
  ↓
ASP.NET Core MVC
  ↓
Entity Framework Core
  ↓
Amazon RDS SQL Server
```

---

## 🔮 Future Improvements

Potential future enhancements include:

- Online payment integration
- Email notifications
- WhatsApp notifications
- Additional astrologers
- Advanced appointment scheduling
- Customer reviews and ratings
- Reports and analytics
- Custom domain and HTTPS
- CI/CD deployment pipeline
- Expanded automated testing

---

## 📄 Project Status

**AstroConnect is deployed and running in an AWS production environment.**

The application currently includes:

- Complete customer booking workflow
- Customer, Receptionist, and Admin portals
- ASP.NET Core Identity authentication
- Role-based authorization
- Booking and appointment lifecycle management
- Responsive desktop and mobile interface
- Production SQL Server database on Amazon RDS
- Application hosting with AWS Elastic Beanstalk
- Production secret management with AWS Secrets Manager
- Entity Framework Core production migrations

Customer registration, authentication, database persistence, desktop access, and mobile access have been verified in the deployed AWS environment.

### 🌐 Live Application

http://astroconnect.ap-south-1.elasticbeanstalk.com

---

## 👨‍💻 Developer

Developed as a full-stack ASP.NET Core MVC project demonstrating:

- C# / .NET 9
- ASP.NET Core MVC
- Clean Architecture
- Entity Framework Core
- Microsoft SQL Server
- ASP.NET Core Identity
- Repository Pattern
- Role-Based Authorization
- Responsive Web Development
- AWS Elastic Beanstalk
- Amazon EC2
- Amazon RDS
- AWS Secrets Manager
- Cloud Database Configuration
- Production Application Deployment
- Git & GitHub

---

## 📜 License

This project is intended for educational and portfolio purposes.