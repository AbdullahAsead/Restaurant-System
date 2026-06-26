# Restaurant Management System

A professional desktop application for managing restaurant operations, built using **C#**, **WPF**, **Entity Framework Core**, and **SQLite**.

## Overview

Restaurant Management System is designed to simplify restaurant management by providing an easy-to-use interface for administrators and cashiers. The system manages orders, menu items, categories, employees, customers, delivery operations, and sales reports in one integrated application.

> This project was developed as a practical desktop application to improve restaurant workflow and management efficiency.

---
## Features

- Secure authentication system with separate **Admin** and **Cashier** login.
- Dashboard with an overview of restaurant operations.
- Menu and category management.
- Order creation and order tracking.
- Customer management.
- Delivery employee management.
- Delivery area management.
- Sales reports and analytics.
- Receipt printing.
- Administrator management.
- SQLite database integration using Entity Framework Core.
- Modern desktop user interface built with WPF.

---

## Technologies Used

- C#
- .NET
- WPF (Windows Presentation Foundation)
- Entity Framework Core
- SQLite
- XAML
- Visual Studio 2022
- Git & GitHub
------
- ---

## Project Modules

The system includes the following modules:

- Authentication (Admin & Cashier)
- Dashboard
- Order Management
- Order Summary
- Customer Management
- Category Management
- Menu Item Management
- Delivery Employee Management
- Delivery Area Management
- Reports & Sales
- Receipt Printing
- Administrator Management

---

## Getting Started

### Prerequisites

Before running the project, make sure you have:

- Visual Studio 2022
- .NET SDK
- SQLite
- Entity Framework Core

### Installation

1. Clone the repository

```bash
git clone https://github.com/AbdullahAsead/Restaurant-System.git
```

2. Open the solution file:

```
Restaurant System.sln
```

3. Restore NuGet packages.

4. Build and run the project.

---
## System Architecture

The application follows a layered architecture to improve maintainability and scalability.

### Main Components

- **Presentation Layer**
  - WPF (XAML)
  - User Interface

- **Business Logic**
  - Application Logic
  - Validation

- **Data Access**
  - Entity Framework Core
  - SQLite Database

- **Models**
  - Admin
  - Customer
  - Category
  - Item
  - Order
  - OrderItem
  - Sale
  - DeliveryEmployee
  - DeliveryPlace

---

## Project Highlights

- Desktop application built with WPF.
- Database management using Entity Framework Core.
- SQLite local database.
- Role-based authentication.
- Receipt printing support.
- Sales reporting.
- Clean and organized project structure.
- Designed for real restaurant environments.

---

## Future Improvements

- Online ordering.
- Multi-branch support.
- Inventory management.
- Barcode scanning.
- Cloud database support.
- Role-based permissions.
- Data backup and restore.

---
## Developer

**Abdullah Asaad**

Full Stack .NET Developer

### Contact

- GitHub: https://github.com/AbdullahAsead
- Email: abdullahasead10@gmail.com

---

## License

This project is available for educational and portfolio purposes.

© 2025 Abdullah Asaad. All Rights Reserved.
---
---

## Project Structure

```
Restaurant-System
│
├── Data
├── Helpers
├── Images
├── Migrations
├── Models
├── Properties
├── Utils
│
├── Login
├── Dashboard
├── Orders
├── Reports
├── Customers
├── Categories
├── Delivery
└── Admin Management
```

---

## Objectives

This project aims to provide a complete desktop solution for restaurant management by simplifying daily operations including order processing, employee management, sales tracking, customer management, and reporting.

---

## User Roles

### Administrator

- Manage administrators
- Manage categories
- Manage menu items
- Manage customers
- Manage delivery employees
- View reports
- Print receipts
- Configure printer settings
- View dashboard

### Cashier

- Create orders
- Manage customer orders
- Print receipts
- View order summaries

---

## Database

The application uses **SQLite** as the local database and **Entity Framework Core** as the ORM to simplify data access and database management.

---
