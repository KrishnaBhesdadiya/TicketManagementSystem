# 🎫 Support Ticket Management System API

A Role-Based Support Ticket Management REST API built using ASP.NET Core Web API, Entity Framework Core, SQL Server, and JWT Authentication.

---

## 📌 Project Overview

This backend system allows organizations to manage support tickets efficiently with role-based access control.

The system supports:

- User Authentication (JWT)
- Role-based Authorization (MANAGER, SUPPORT, USER)
- Ticket Creation & Assignment
- Ticket Status Tracking
- Comment System
- Status Change Logs

---

## 🛠 Tech Stack

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt Password Hashing
- Swagger (API Testing)

---

## 🔐 Roles & Permissions

### 👑 MANAGER
- View all users
- View all tickets
- Assign tickets
- Update ticket status
- Add / View comments
- Edit / Delete any comment

### 🛠 SUPPORT
- View assigned tickets
- Assign tickets
- Update ticket status
- Add / View comments (only assigned tickets)
- Edit / Delete own comments

### 👤 USER
- Create tickets
- View own tickets
- Add / View comments (only own tickets)
- Edit / Delete own comments

---

## 🗄 Database Structure

### Tables
- roles
- users
- tickets
- ticket_comments
- ticket_status_logs

### Relationships
- users → roles (Many-to-One)
- tickets → users (CreatedBy, AssignedTo)
- ticket_comments → tickets
- ticket_status_logs → tickets

---

## 🚀 Getting Started

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/KrishnaBhesdadiya/TicketManagementSystem.git
cd TicketManagementSystem
