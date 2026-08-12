# TrackHub Security API

## Key Features

- **Role-Based Access Control (RBAC)**: Comprehensive permission management with roles, policies, and actions
- **GraphQL Interface**: Efficient, flexible querying for security data using Hot Chocolate server
- **Fine-Grained Permissions**: Resource-level and action-level access control for maximum security
- **User Management**: Complete CRUD operations for user accounts with role assignments
- **Password Validation**: Enforced password strength requirements including minimum length, uppercase, lowercase, digit, and special character rules
- **Policy Engine**: Define and enforce custom access policies at user and role levels
- **Service Client Management**: Secure registration and permission management for external service clients
- **Default Role Templates**: Pre-configured Administrator, Manager, and User roles for quick setup
- **Clean Architecture**: Maintainable, testable codebase following SOLID principles

---

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 14+
- TrackHub Authority Server running (for authentication)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/shernandezp/TrackHubSecurity.git
   cd TrackHubSecurity
   ```

2. **Configure the database connection** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "SecurityConnection": "Host=localhost;Database=trackhub_security;Username=postgres;Password=yourpassword"
     }
   }
   ```

3. **Run database migrations**:
   ```bash
   dotnet ef database update
   ```

4. **Seed initial data** (roles, resources, actions):
   ```bash
   dotnet run --project src/DBInitializer
   ```

5. **Start the application**:
   ```bash
   dotnet run --project src/Web
   ```

6. **Access GraphQL Playground** at `https://localhost:5001/graphql`

### Default Roles

| Role | Description |
|------|-------------|
| **Administrator** | Full access to all resources and actions |
| **Manager** | Intermediate access for resource management |
| **User** | Limited access based on assigned responsibilities |

---

## Components and Resources

| Component                | Description                                           | Documentation                                                                 |
|--------------------------|-------------------------------------------------------|-------------------------------------------------------------------------------|
| Hot Chocolate            | GraphQL server for .NET                               | [Hot Chocolate Documentation](https://chillicream.com/docs/hotchocolate/v13)  |
| .NET Core                | Development platform for modern applications          | [.NET Core Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview) |
| Postgres                 | Relational database management system                 | [Postgres Documentation](https://www.postgresql.org/)                         |

---

## Overview

The Security API is a GraphQL service responsible for managing TrackHub's security database. It exposes essential methods to handle role-based access control (RBAC), enabling the management of user roles, permissions, and resources.

## Key Entities

The core entities in this service are:

- **Action**: Defines the operations (e.g., create, read, update, delete) that can be performed on a resource. Actions are mapped to resources and govern what operations are allowed.

- **Client**: Stores information about service-type clients that interact with the API. It manages permissions and access for external clients to ensure secure interactions.

- **Policy**: Represents a set of rules that govern access control. Policies are essential for enforcing security measures, either at the user or role level, and control what actions can be performed on which resources.

- **Resource**: Represents the different components or modules of the application that require access control. Resources can include APIs, endpoints, UI elements, or other entities that need permission management.

- **ResourceAction**: Links specific actions to resources, defining which operations are allowed for each resource.

- **ResourceActionPolicy**: Defines access policies for specific combinations of resources and actions, providing detailed control over permissions.

- **ResourceActionRole**: Associates specific roles with combinations of resources and actions, determining what actions each role can perform on a resource.

- **Role**: Defines roles within the application (e.g., Administrator, Manager, User) for RBAC. Roles determine access to various resources and actions in the system.

- **User**: Contains user-specific information, such as credentials and profile data. Users are assigned roles and policies to define their access rights.

- **UserPolicy**: Contains policies that apply to individual users, allowing for more granular control over user access beyond roles.

- **UserRole**: Assigns roles to users, determining their level of access to resources. There are three default roles:
  - **Administrator**: Full access to all resources and actions.
  - **Manager**: Intermediate access, with permissions to manage specific resources.
  - **User**: Limited access, with permissions to interact with specific resources based on user responsibilities.

## Clean Architecture Approach

This API follows Clean Architecture principles, which emphasize separation of concerns and ensure that the business logic remains decoupled from external frameworks, databases, and data sources. This architecture promotes scalability, maintainability, and flexibility, making it easier to evolve the application over time.

GraphQL is utilized to offer a flexible and efficient query mechanism, allowing clients to request only the data they need, which optimizes performance and reduces over-fetching.

## Role-Based Access Control (RBAC) Benefits

By leveraging RBAC, the Security API ensures that users and clients only have access to the resources and actions they are authorized to interact with. This minimizes the risk of unauthorized access and maintains the principle of least privilege throughout the system.



## License

This project is licensed under the Apache 2.0 License. See the [LICENSE file](https://www.apache.org/licenses/LICENSE-2.0) for more information.


