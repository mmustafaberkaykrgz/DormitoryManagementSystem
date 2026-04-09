# 🏠 Dormitory Management System

> A streamlined, locally-hosted web application for managing student housing operations — built with ASP.NET Core 8.0 MVC.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-ASP.NET_Core_MVC-239120?style=flat-square&logo=csharp)
![SQLite](https://img.shields.io/badge/Database-SQLite-003B57?style=flat-square&logo=sqlite)
![Bootstrap](https://img.shields.io/badge/UI-Bootstrap_5-7952B3?style=flat-square&logo=bootstrap)
![License](https://img.shields.io/badge/License-Academic-blue?style=flat-square)

---

## 📋 Table of Contents

- [About](#about)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
- [User Roles](#user-roles)
- [Project Structure](#project-structure)
- [Authors](#authors)

---

## About

The **Dormitory Management System** is a full-stack web application developed for the SENG 321 – Web Development with Modern Frameworks course. It digitizes and simplifies paper-based operations of a student housing facility — covering room allocation, membership tracking, financial records, maintenance requests, and administrative reporting.

The system is intentionally designed to be **simple, maintainable, and fast** without sacrificing enterprise-level architecture standards.

---

## Features

| Module | Description |
|---|---|
| 🔐 **Authentication & RBAC** | Cookie-based auth with Admin / Staff / Student roles |
| 🛏️ **Room Management** | CRUD for rooms, capacity tracking, occupancy status |
| 🎓 **Student Membership** | Registration, room assignment, contract date tracking |
| 💰 **Dues & Penalties** | Financial tracking with simple Paid/Unpaid toggle |
| 🔧 **Maintenance Tickets** | Submit → Review → Resolve workflow |
| 📄 **Document Storage** | Upload and link student documents (ID, contracts) |
| 🔔 **Notifications** | In-app alerts for dues, penalties, and ticket updates |
| 📊 **Dashboard & Reports** | Real-time metrics, occupancy rates, financial summaries |
| 🕵️ **Audit Logs** | Timestamped activity tracking for accountability |
| ⚙️ **System Settings** | Configurable due amounts, penalty fees, global variables |

---

## Tech Stack

```
Backend   → C# / ASP.NET Core 8.0 MVC
Database  → SQLite + Entity Framework Core (Code-First)
Frontend  → Razor Views (.cshtml) + HTML5 + CSS3 + Bootstrap 5
Auth      → ASP.NET Core Cookie-based Authentication
IDE       → Visual Studio Code
```

---

## Architecture

This project follows a strict **MVC (Model-View-Controller)** layered architecture:

```
┌─────────────────────────────────────────────┐
│                   Client                    │
│           (Browser / Razor Views)           │
└────────────────────┬────────────────────────┘
                     │ HTTP Request
┌────────────────────▼────────────────────────┐
│              Controller Layer               │
│   RBAC enforcement · Request routing        │
│   Business logic · EF Core operations       │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│               Model Layer                   │
│   Entities · ViewModels · Validation        │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│              Data Layer (SQLite)            │
│   AppDbContext · EF Core Migrations         │
└─────────────────────────────────────────────┘
```

---

## Database Schema

Key entities and their relationships:

```
Roles ──< Users ──< Students ──< DuesAndPenalties
                              ──< Documents
                              ──< Notifications
          Users ──< Staff
          Users ──< Admin

Rooms ──< Students
Rooms ──< MaintenanceTickets

SystemSettings (global config)
AuditLogs      (activity tracking)
```

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio Code (or any IDE)

### Run Locally

```bash
# 1. Clone the repository
git clone https://github.com/YOUR_USERNAME/DormitoryManagementSystem.git
cd DormitoryManagementSystem

# 2. Restore dependencies
dotnet restore

# 3. Apply database migrations
dotnet ef database update

# 4. Run the application
dotnet run
```

The app will be available at `https://localhost:5001` (or as configured in `launchSettings.json`).

### Default Credentials

After seeding (`DbSeeder.cs`), use the admin account to log in and create Staff/Student accounts through the Settings panel.

---

## User Roles

| Role | Capabilities |
|---|---|
| **Admin** | Full system access: user management, settings, audit logs, all CRUD |
| **Staff** | Room management, student registration, dues/penalties, maintenance updates |
| **Student** | View own membership, check dues, submit & track maintenance requests |

---

## Project Structure

```
DormitoryManagementSystem/
├── Controllers/          # Route handlers & business logic
│   ├── AccountController.cs
│   ├── DuesController.cs
│   ├── HomeController.cs
│   ├── MaintenanceController.cs
│   ├── ReportsController.cs
│   ├── RoomsController.cs
│   ├── SettingsController.cs
│   └── StudentsController.cs
├── Models/               # Entities & ViewModels
├── Views/                # Razor (.cshtml) templates
│   ├── Account/
│   ├── Dues/
│   ├── Home/             # Dashboard & landing
│   ├── Maintenance/
│   ├── Reports/
│   ├── Rooms/
│   ├── Settings/
│   └── Shared/
├── Data/
│   ├── AppDbContext.cs   # EF Core context
│   └── DbSeeder.cs       # Seed data
├── Migrations/           # EF Core migration files
├── Program.cs            # App entry point & DI config
└── appsettings.json
```

---

## Authors

This project was developed as part of **SENG 321 – Web Development with Modern Frameworks** under the supervision of **Lect. Dr. Ruhi Taş**.

| Name | Student ID |
|---|---|
| Şeyma Bayram | 220208045 |
| Mustafa Berkay Karagöz | 220208010 |
| Kerim Taşkın | 220208927 |

---

*SENG 321 · Web Development with Modern Frameworks · 2025–2026*