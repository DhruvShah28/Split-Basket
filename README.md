# SplitBasket

*SplitBasket* is an intuitive platform designed to simplify the process of managing group expenses and keeping track of shared payments. It enables users to easily split bills, manage balances, and ensure fair sharing of costs within a group.

## Features

- *User Authentication* (To be implemented in the future)  
  - Secure login with user roles to track individual spending and contributions.
  
- *Group Management*  
  - Create, update, and delete groups for organizing shared expenses.
  - Invite members to join groups and track their contributions.

- *Expense Tracking*  
  - Add, update, and delete expenses.
  - Split expenses equally or proportionally between group members.
  
- *Balance Calculation*  
  - Automatically calculates how much each person owes or is owed based on the expenses added.
  - Displays balances for each member in the group.

- *History and Reports*  
  - Keep a detailed record of past expenses and payments within the group.
  - Generate reports summarizing expenses and balances for review.

- *Database Integration*  
  - Built using ASP.NET Core, C#, and Entity Framework to ensure seamless data management.

- *CRUD Operations*  
  - Full Create, Read, Update, and Delete (CRUD) functionality for users, groups, and expenses.
  - Track and display the contribution of each member per group and per expense.

## Technologies Used

- *Backend*: ASP.NET Core, C#
- *Frontend: Razor Pages / MVC *(To be implemented in the future)
- *Database*: Microsoft SQL Server, Entity Framework
- *Authentication: Identity Framework *(To be implemented in the future)

## Design and Architecture

- *Service Layer & Interfaces*  
  - The service layer manages business logic and data operations, ensuring clear separation of concerns. Interfaces provide abstraction, making the application more flexible and testable.

- *Razor Views*  
  - Razor Views are used for rendering dynamic, data-driven pages, enhancing user interaction and providing a smooth experience.

## API, Interface, and Service Layer

In *SplitBasket*, we adopt a layered architecture to promote maintainability and separation of concerns:

- *API (Controller Layer)*  
  - The *API* layer is responsible for handling HTTP requests. It processes requests, communicates with the service layer, and returns responses to the client.

- *Interface*  
  - The *Interface* layer defines the contract that services must implement, ensuring consistency across the application.

- *Service Layer*  
  - The *Service* layer implements the logic for business operations, interacting with the database and performing calculations like expense splitting and balance management.

### Flow of Execution:

1. *API Controller* receives the request from the user.
2. The *Controller* invokes methods from the *Service* layer through the *Interface*.
3. The *Service* layer executes business logic and returns data to the *Controller*.
4. The *Controller* sends the response back to the user interface.

This modular architecture promotes scalability, flexibility, and ease of testing.

## Page Controllers, View Models, and Views

The *SplitBasket* application uses *Page Controllers, **View Models, and **Views* to provide a clean structure and separate concerns for maintainable code:

- *Page Controllers*  
  - *Page Controllers* process HTTP requests, interact with the service layer, and pass data to the *View Models*.

- *View Models*  
  - *View Models* contain the data needed for a particular view, separating presentation logic from domain models. They also handle validation for data formatting and ensure clean data for the view.

- *Views*  
  - *Views* are responsible for rendering the user interface and displaying data using *Razor Views. They present the necessary data from the **View Models* and ensure that the user sees the correct information.

### Flow of Execution:

1. *Page Controller* handles the HTTP request.
2. The *Page Controller* prepares the necessary data in the *View Model*.
3. The *View Model* structures the data for the view.
4. The *View* renders the UI and presents the data to the user.

This design ensures clear responsibilities for each part of the application, promoting maintainability and easy updates.

## Setup Instructions

Follow these steps to set up *SplitBasket* on your local machine:

1. Clone the repository:
```sh
git clone https://github.com/Himani1609/SplitBasket.git
cd SplitBasket
```

2. Install dependencies:
``` sh
dotnet restore
```
3. Configure the database:
- Update appsettings.json with your database connection string.
- Run migrations:
```sh
dotnet ef database update
```
4. Run the application:
``` sh
- dotnet run
```
## Future Enhancements

  - *Add/Remove Member’s Photo*: Allow users to upload and manage photos for group members.
  - *Sort and Filter Grocery List by Price*: Provide sorting and filtering options to organize grocery items based on price.
  - *Budget Tracker*: Implement a feature that helps users track their spending and stay within budget.
  - *Grocery List for Common and Personal Items*: Support the creation of separate grocery lists for shared and individual items.
  - *Multiple Currency Support*: Enable users to choose and switch between different currencies for purchases.
  - *Export Databases to Excel*: Allow users to export their grocery list and purchase history to an Excel file for easier management.
  - *Search History Purchases by Range or Date*: Enable searching of past purchases based on a specified date range or specific date.

## Contact

For any queries or suggestions, reach out to [Dhruv Shah](https://github.com/DhruvShah28).
