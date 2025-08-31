# Husam.SalesDashboard

A **Sales Dashboard web application** built with **ASP.NET Core MVC, Entity Framework Core, SQL Server, Chart.js, and Bootstrap 5**.  
It allows managing Customers, Products, and Sales, while providing a real-time dashboard with charts and KPIs.

---

## ✨ Features
- **CRUD** operations for Customers, Products, and Sales
- **Dashboard KPIs** (Total Revenue, Sales Count)
- **Revenue by Month** chart (line chart using Chart.js)
- **Top 5 Products by Revenue** (bar chart)
- **Top 5 Customers by Revenue** (bar chart)
- **CSV Upload** for bulk import of sales (via CsvHelper)
- **Responsive UI** with Bootstrap 5 + Icons

---

## 🛠️ Tech Stack
- **ASP.NET Core 8** (MVC)
- **Entity Framework Core** + **SQL Server**
- **Chart.js** (for charts and data visualization)
- **Bootstrap 5 + Bootstrap Icons** (responsive UI)
- **CsvHelper** (CSV parsing and upload)

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- SQL Server (LocalDB, Express, or full SQL Server)
- Visual Studio 2022 (recommended) or VS Code

### Setup
1. Clone the repository:
   git clone https://github.com/husambaobaid/Husam.SalesDashboard.git
   cd Husam.SalesDashboard
2. Update the database
   dotnet ef database update
3. Run the app:
   dotnet run
4. Open in browser:
   https://localhost:7244


   
