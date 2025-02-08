# SlipBasket

*SlipBasket* is a web-based platform for managing group purchases of grocery items. It allows users to create, update, and track purchases, link and unlink grocery items from purchases, and maintain organized records of group buying activities.

## Features

- *Group Purchase Management*
  - Create, update, and delete group purchases.
  - Link and unlink grocery items to specific purchases.
- *Grocery Item and Purchase Linking*
  - Link grocery items to a purchase to keep items organized.
  - Unlink grocery items from purchases as needed.
- *CRUD Operations*
  - Full Create, Read, Update, and Delete (CRUD) operations for group purchases, grocery items, and purchases.
- *Flexible Data Handling*
  - Bridge table for linking grocery items and purchases, allowing many-to-many relationships.
  - API endpoints for managing linking and unlinking of items and purchases.

## Technologies Used

- *Backend*: ASP.NET Core, C#
- *Database*: Microsoft SQL Server, Entity Framework
- *API*: RESTful API for managing group purchases and linking/unlinking operations

## Setup Instructions

1. Clone the repository:
   sh
   git clone https://github.com/yourusername/SlipBasket.git
   cd SlipBasket

2. Install dependencies:
   sh
   dotnet restore
   
3. Configure the database:
   - Update `appsettings.json` with your database connection string.
   - Run migrations:
     sh
     dotnet ef database update
     
4. Run the application:
   sh
   dotnet run
   ```

## Extra Features

- *Add/Remove Member’s Photo*  
  Allow users to upload and manage photos for group members.

- *Sort and Filter Grocery List by Price*  
  Provide sorting and filtering options to organize grocery items based on price.

- *Budget Tracker*  
  Implement a feature that helps users track their spending and stay within budget.

- *Grocery List for Common and Personal Items*  
  Support the creation of separate grocery lists for shared and individual items.

- *Multiple Currency Support*  
  Enable users to choose and switch between different currencies for purchases.

- *Export Databases to Excel*  
  Allow users to export their grocery list and purchase history to an Excel file for easier management.

- *Search History Purchases by Range or Date*  
  Enable searching of past purchases based on a specified date range or specific date.


## Contact

For any queries or suggestions, reach out to [Dhruv Shah](https://github.com/DhruvShah28).
