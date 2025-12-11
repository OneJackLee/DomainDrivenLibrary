# Domain Driven Library

Domain Driven Library is a sample library management system with Domain-Driven Design (DDD) and Clean Architectural
principles.

This simple web API server models a simple library system where:

- Borrowers (a.k.a. the library member) can register and borrow books
- Books existed as a physical copies that can be tracked and borrowed in this system
- Catalog contain metadata about books (ISBN, title, author), thus a catalog has one-to-many relationships with Books
    - ISBN served as the unique identifier for multiple physical copies

## Prerequisite

- [Install .NET 9 SDK on Windows, Linux, and macOS](https://learn.microsoft.com/en-us/dotnet/core/install/)
- [Install docker engine](https://docs.docker.com/engine/install/)

## Get started

There are 2 ways to build and host this web API server on your local, before build the web API server, ensure to
generate
a developer certificates by running the following command on your terminal:

- For Windows user:

   ```pwsh
   dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\aspnetapp.pfx -p password
   dotnet dev-certs https --trust
   ```

- For macOS or Linux user:

    ```zsh
    dotnet dev-certs https --trust -ep ~/.aspnet/https/aspnetapp.pfx -p password
    ```

### Build and host with Docker

1. Open your terminal, and clone the repository to your device.
    ```zsh
   git clone {the repository link} 
   ```
2. Switch the directory to the root folder of this repository with the following command.
    ```zsh
    cd DomainDrivenLibrary
   ```
3. On the root folder of this repository, create all the necessary containers in your Docker by running the command
   below. You may expect some exceptions thrown on the app container.
    - For macOS or Linux user, run `docker-compose up --build`.
    - For Windows user, run `docker-compose -f compose.windows.yaml up --build`.
4. Open your browser, and navigate to [this link](https://localhost:7236/swagger) for the Swagger UI.

## Introduction to the web API server

See [API_Guide.md](./API_Guide.md) for detailed guide on how to use this API.

### API Summary

| # | Endpoint             | Method | URL                           | Description               |
|---|----------------------|--------|-------------------------------|---------------------------|
| 1 | Register Book        | POST   | `/api/books`                  | Register a new book copy  |
| 2 | Get All Books        | GET    | `/api/books`                  | Get all books             |
| 3 | Borrow Book          | POST   | `/api/books/{bookId}/borrow`  | Borrow a book             |
| 4 | Return Book          | POST   | `/api/books/{bookId}/return`  | Return a borrowed book    |
| 5 | Get Catalog Entry    | GET    | `/api/catalog-entries/{isbn}` | Get catalog entry by ISBN |
| 6 | Update Catalog Entry | PUT    | `/api/catalog-entries/{isbn}` | Update catalog entry      |
| 7 | Register Borrower    | POST   | `/api/borrowers`              | Register a new borrower   |
| 8 | Get All Borrowers    | GET    | `/api/borrowers`              | Get all borrowers         |

### Assumptions

This project was built with the following assumptions:

- **Scope**: Backend API only - no UI or authentication required
- **Borrower identity**: Email address uniquely identifies a borrower
- **ISBN enforcement**: Books with mismatched title/author for an existing ISBN are rejected
- **Pagination**: Not required for GetAll endpoints

See [assumption.md](./assumption.md) for the complete list including development process assumptions.

### Limitations

- The checksum for ISBN-10/ISBN-13 is not implemented yet
- The book borrow history is not implemented yet

## Testing

- Domain Tests - Aggregate invariants, value object validation
- Application Tests - Handler logic with mocked dependencies

```bash
# Run all tests
dotnet test

# Run with verbosity
dotnet test --logger "console;verbosity=detailed"
```

## Implementation information

### Tech Stack

| Technology            | Version | Purpose          |
|-----------------------|---------|------------------|
| .NET                  | 9.0     | Runtime          |
| C#                    | 12      | Language         |
| ASP.NET Core          | 9.0     | Web API          |
| Entity Framework Core | 9.0     | ORM              |
| PostgreSQL            | 18.1    | Database         |
| ULID                  | 1.4.1   | ID Generation    |
| Docker                | -       | Containerization |

### How This Repository Was Built

To be transparent, the development process followed a spec-driven model:

- I provide the system design, domain modeling, and architectural specifications.
  I also code the base implementation such as the domain classes, and command/query handlers.
- Claude Code contributed by implementing the plan (check out the [design documents](./Plan.md) for more information),
  functioning as an intern/junior engineer under my direction.

The reason for this workflow is to maximize efficiency:

- I focus on high-value design and implementation.
- Routine or repetitive base coding tasks are shifted to AI assistance.
- Work proceeds in parallel, improving productivity by allowing me and the agent to operate concurrently.

#### About the Design Documents

The [Plan.md](./Plan.md) was created through a collaborative conversation with Claude Code (Opus 4.5),
where I drove the design decisions and domain modeling while Claude helped structure and document them.

## Who do I talk to?

Please speak to [OneJackLee](mailto:charliewanj@outlook.com) should you have any issue.