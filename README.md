<p align="center">
  <img height="150" alt="Teaching Center Logo" src="https://github.com/user-attachments/assets/3f722f89-1613-4f6d-8f70-bf888f2b1039" />
</p>

<h1 align="center">PBL3: Teaching Center Management System (TCMS)</h1>

<p align="center">
  <strong>✨ Digitalizing Workflows - Elevating Education ✨</strong>
</p>

<p align="center">
A comprehensive management system to streamline operations, scheduling, attendance, and finance among the Administration Board, Teachers, and Students.
</p>

<p align="center">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" />
  <img alt=".NET WPF" src="https://img.shields.io/badge/.NET_WPF-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img alt="SQL Server" src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" />
  <img alt="Docker" src="https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white" />
</p>

## 📸 System UI & Core Modules

Below are some visual demonstrations of the system's core functionalities categorized by user roles.

### 👑 General Admin 
**Admin Dashboard:** Comprehensive overview of classes, accounts, and system data.
<img width="1919" height="1079" alt="1778906718707_7154354181070218536_g2971286853114616413_d6a0d8fd6acf8a56d364e79547ce426c" src="https://github.com/user-attachments/assets/cb69a77c-2d37-442a-bd82-cac0dd3055ef" />

<br/>

### 💰 Finance Admin
**Finance Management:** Tracking system for student tuition fees and teacher salary payouts.
<img width="2878" height="1799" alt="Screenshot 2026-07-08 235741" src="https://github.com/user-attachments/assets/11472859-f773-4485-bc1d-bc34624cbdd7" />


### 👨‍🏫 Teacher
**Teacher Panel / Grading System:** A streamlined interface for teachers to manage their classes and input grades.
<img width="1920" height="1200" alt="1778907366414_7640167550815314534_g2971286853114616413_6f467178388ccb1228345f225e13aeae" src="https://github.com/user-attachments/assets/505348f3-e3a0-4be9-b52b-f4fb444df29b" />

<br/>

### 👩‍🎓 Student
**Student Calendar:** An intuitive, visual calendar mapping out study schedules and attendance records.
<img width="1920" height="1200" alt="1778907366412_7640167550815314534_g2971286853114616413_fff278f900bed65bf2e05fc861f16e32" src="https://github.com/user-attachments/assets/d168acac-1cc2-4fc0-a074-f88bfdc62ef0" />


---

## 🚀 Key Features

The system is designed with a strict Role-Based Access Control (RBAC) model, divided into specialized modules:

### 👑 Admin / General Panel
* **Core Operations (CRUD):** Intuitive interface to fully manage Accounts, Courses, Classes, and Personnel.
* **Finance Management:** Digitalize tuition collection, teacher salary payouts, and revenue statistics.
* **Attendance Tracking:** Monitor daily student attendance synchronized directly with the class schedule.

### 👨‍🏫 Teacher Panel
* **Dynamic Schedule:** Automatically displays current assigned classes based on real-time data.
* **Grading System:** Secure and efficient interface for batch grade input and academic evaluation.

### 👩‍🎓 Student Panel
* **Visual Calendar UI:** Interactive monthly calendar view for class schedules.
* **Academic Tracking:** Monitor enrolled courses, grades, and export academic transcripts easily.

---

## 🧠 Technical Highlights

* **Data Integrity & Safety:** Implemented **SQL Transactions** (`BeginTransaction`, `Commit`, `Rollback`) to ensure complete data consistency when teachers perform batch grading operations.
* **Cross-Platform Stability:** Handled globalization issues using `CultureInfo.InvariantCulture`, completely preventing app crashes when inputting decimal numbers (dot vs. comma) across different OS environments.
* **Advanced UI Data Mapping:** Solved the time-axis offset mapping problem to convert flat SQL data tables into a dynamic, logical visual calendar UI in C#.
* **WPF Lifecycle Optimization:** Managed the WPF UI lifecycle natively using explicit `CommitEdit` commands on DataGrids to prevent data-binding loss.
* **High-Speed Reporting:** Integrated **ClosedXML** to allow blazing-fast `.xlsx` export directly from `DataView` structures, completely independent of Microsoft Office installations.
* **Database Optimization:** Utilized `LEFT JOIN` and `ISNULL` techniques for transparent academic views, ensuring even ungraded subjects display logically. Normalized database up to 3NF.

---

## 🛠️ Tech Stack
* **Programming Language:** C#
* **UI Framework:** .NET WPF (Windows Presentation Foundation)
* **Database:** Microsoft SQL Server (T-SQL)
* **Third-party Libraries:** `ClosedXML` (For OpenXML Excel manipulation).
* **Architecture:** Event-Driven, Data-Driven Design & 3NF Database Normalization.

---

## ⚙️ Installation & Setup

**Step 1: Clone the repository**
```bash
git clone https://github.com/lnamdeptrai392-dotcom/PBL3.git
```
**Step 2: Database Setup**
* Open **SQL Server Management Studio (SSMS)**.
* Execute the provided SQL script in the repository to generate the Schema and load mock data.
**Note:** The system relies on real-time logic (GETDATE()), so ensure the mock data dates align with your current testing timeline.

**Step 3: Configure Connection String**
* Open the Solution in **Visual Studio 2022**.
* Locate  `DatabaseHelper.cs` and modify the `Data Source` property to match your local SQL Server instance name.

**Step 4: Restore Packages**
* Right-click on the Solution -> Select **Restore NuGet Packages**.
* Ensure `ClosedXML` is successfully installed.

**Step 5: Run**
Press `F5` or click `Start` to launch the application.

---

## 🔑 Test Accounts

| Role | ID (UserName) | Password |
| :--- | :--- | :--- |
| **General Admin** | `adminc` | `123456` |
| **Finance Admin** | `admintc` | `123456` |
| **Teacher** | `T01` | `123456` |
| **Student** | `10110001` | `123456` |

---

## Contributors

* **Developers**
    * Lê Xuân Nam
    * Phạm Thị Ngọc Khuê
    * Nguyễn Phương Uyên
    * Nguyễn Quang Trường Duy
* **Course:** Project Based Learning 3 (PBL3)
* **Faculty:** Faculty of Information Technology - Da Nang University of Science and Technology (DUT)
