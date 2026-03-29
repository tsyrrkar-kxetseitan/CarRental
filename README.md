# 🚗 Car Rental Service

[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-blue)](https://docs.microsoft.com/en-us/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/EF_Core-7.0-green)](https://docs.microsoft.com/en-us/ef/core/)
[![SQLite](https://img.shields.io/badge/Database-SQLite-lightgrey)](https://www.sqlite.org/)
[![xUnit](https://img.shields.io/badge/Testing-xUnit-orange)](https://xunit.net/)
[![GitHub](https://img.shields.io/badge/Platform-GitHub-black)](https://github.com/)

> A web application for renting cars – built for the **Industrial Informatics** project.  
> Supports **clients** (browse cars, rent, view details) and **admins** (add/edit cars) with persistent SQLite storage.

---

## 📖 Table of Contents

- [Features](#features)
- [Technologies](#technologies)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Team](#team)
- [License](#license)

---

## ✨ Features

- 🔐 **Authentication & Authorization** – Login/Register via ASP.NET Core Identity; roles `Client` and `Admin`.
- 🚙 **Car listing** – Browse all cars, filter by brand, price, or availability.
- 📄 **Car details page** – Full info, price per day, and a **Rent** button (visible only to logged‑in clients).
- 🛒 **Rent a car** – Clients can rent available cars; rental history is stored in the database.
- 🛠️ **Admin panel** – Add, edit, or remove cars (admin only).
- 💾 **Persistent storage** – SQLite database with Entity Framework Core (Code First).
- ✅ **Unit tests** – xUnit + Moq for service layer and business logic.

---

## 🧰 Technologies

| Layer               | Technology                                                  |
| ------------------- | ----------------------------------------------------------- |
| **Frontend**        | ASP.NET Core MVC, Razor Views, Bootstrap 5                  |
| **Backend**         | C# (.NET 7), ASP.NET Core Controllers, Services             |
| **Database Access** | Entity Framework Core (Code First)                          |
| **Database**        | SQLite (development) – easily switch to SQL Server          |
| **Authentication**  | ASP.NET Core Identity                                       |
| **Testing**         | xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing                |
| **Version Control** | Git + GitHub (public repo with branch protection rules)     |

---

## 📁 Project Structure

CarRentalSolution/

├── CarRental.Web/ # MVC UI layer

│ ├── Controllers/

│ ├── Views/

│ ├── wwwroot/

│ └── Program.cs

├── CarRental.Core/ # Business logic, Models, Interfaces

│ ├── Models/

│ ├── Interfaces/

│ └── Services/

├── CarRental.Data/ # Data access, DbContext, Repositories

│ ├── AppDbContext.cs

│ └── Migrations/

├── CarRental.Tests/ # Unit tests (xUnit + Moq)

│ └── Services/

└── CarRental.sln

---

## 🚀 Getting Started

### Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with “ASP.NET and web development” workload)
- Git (optional, for cloning)

### Clone the repository

```bash
git clone https://github.com/tsyrrkar-kxetseitan/CarRental.git
cd CarRental
```
## Restore & build

Open CarRental.sln in Visual Studio and build the solution (NuGet packages restore automatically).

## Set up the database

In Package Manager Console (Tools → NuGet Package Manager → Package Manager Console):
Set Default project to CarRental.Data

Run:

```powershell
Add-Migration InitialCreate -StartupProject CarRental.Web
Update-Database -StartupProject CarRental.Web
```
This creates carrental.db (SQLite file) in the output folder.

## Run the application

Press F5 or click Run in Visual Studio.
Default URL: https://localhost:5001 or http://localhost:5000


Default seeded admin account (if implemented in DbInitializer):
Email: admin@carrental.com
Password: Admin123!
(You can change this inside OnModelCreating or a separate seeder class.)

---

## 🧪 Running Tests

In Visual Studio: Test → Run All Tests
Or via CLI:

```bash
dotnet test CarRental.Tests/CarRental.Tests.csproj
```
---

## 👥 Team Roles & Responsibilities

| Role | Name | Responsibilities |
| --- | --- | --- |
| Team Leader | [Mastan Andrei]() | Architecture, UML, coordination, deadlines, documents, code reviews |
| Fullstack Developer | [Kerestely Alexandru]() | Backend logic, database, services, EF Core, migrations, integration |
| Frontend Developer | [Moga Anton-Ioan]() | Razor Views, Bootstrap, CSS, car listing UI, filters, responsive design |
| Backend Developer	| [Șofan Laurențiu]() | Authentication, Identity, admin panel, API endpoints |
| Tester | [Papuc Sergiu Ioan]() | xUnit tests, test cases, bug reporting, manual UI testing |

## 📄 License

This project is for educational purposes as part of the Industrial Informatics course.
No commercial license is applied.



🚗💨
