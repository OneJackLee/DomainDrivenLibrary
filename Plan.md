# Library System - Domain-Driven Design Plan

## Executive Summary

This document outlines the architectural design for a sandbox library system built using Domain-Driven Design (DDD)
principles, Clean Architecture, and RESTful API patterns in C#/.NET 9.0.

### Core Requirements

- **Borrower**: Unique ID, name, email address
- **Book**: Unique ID, ISBN, title, author
- **ISBN Invariant**: Same ISBN must have same title/author; different ISBNs are different books
- **Multiple Copies**: Books with same ISBN can exist as separate copies (different IDs)
- **Borrowing Invariant**: A book (by ID) can only be borrowed by one member at a time

### API Operations

| Endpoint          | Description                      |
|-------------------|----------------------------------|
| Register borrower | Add new library member           |
| Register book     | Add new book copy to library     |
| Get all books     | List all books with details      |
| Borrow book       | Borrower checks out a book by ID |
| Return book       | Borrower returns a borrowed book |

---

## Part 1: Domain Model

### Bounded Context: Library Lending

| Term             | Definition                                                    |
|------------------|---------------------------------------------------------------|
| **CatalogEntry** | Abstract book concept (ISBN + title + author)                 |
| **Book**         | Physical copy that can be borrowed (unique ID)                |
| **Borrower**     | Registered library member                                     |

### Invariants

1. **ISBN**: Same ISBN → Same title/author (enforced via CatalogEntry)
2. **Borrowing**: One borrower per book at a time (enforced by Book aggregate)

---

## Part 2: Aggregates & Value Objects

```
┌─────────────────────────────────────────────────────────────────────┐
│   CatalogEntry (Aggregate)         Borrower (Aggregate)             │
│   ┌───────────────────┐           ┌───────────────────┐             │
│   │ ISBN (PK)         │           │ Id: BorrowerId    │             │
│   │ Title             │           │ Name              │             │
│   │ Author            │           │ Email: Email      │             │
│   └─────────┬─────────┘           └─────────┬─────────┘             │
│             │ 1:N                           │ 0..1:N                 │
│             ▼                               │                        │
│   ┌───────────────────┐                     │                        │
│   │  Book (Aggregate) │◄────────────────────┘                        │
│   ├───────────────────┤                                              │
│   │ Id: BookId        │  Methods: Register(), Borrow(), Return()    │
│   │ ISBN: ISBN (FK)   │  Computed: IsAvailable                       │
│   │ BorrowedBy? (FK)  │                                              │
│   │ BorrowedAt?       │                                              │
│   └───────────────────┘                                              │
└─────────────────────────────────────────────────────────────────────┘
```

### Value Objects

| Type         | Validation                                      | Notes                    |
|--------------|-------------------------------------------------|--------------------------|
| `ISBN`       | 10 or 13 digits (after removing hyphens/spaces) | Normalized, digits only  |
| `Email`      | Contains '@', basic format                      | Normalized to lowercase  |
| `BookId`     | Non-empty string                                | System-generated via `IIdGenerator` |
| `BorrowerId` | Non-empty string                                | System-generated via `IIdGenerator` |

---

## Part 3: Repository Interfaces

```csharp
public interface ICatalogEntryRepository
{
    Task<CatalogEntry?> GetByISBNAsync(ISBN isbn, CancellationToken ct = default);
    Task<IReadOnlyList<CatalogEntry>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(ISBN isbn, CancellationToken ct = default);
    void Add(CatalogEntry catalogEntry);
    void Update(CatalogEntry catalogEntry);
}

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(BookId id, CancellationToken ct = default);
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BookWithCatalog>> GetAllWithCatalogAsync(CancellationToken ct = default);
    void Add(Book book);
}

public interface IBorrowerRepository
{
    Task<Borrower?> GetByIdAsync(BorrowerId id, CancellationToken ct = default);
    Task<IReadOnlyList<Borrower>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(BorrowerId id, CancellationToken ct = default);
    void Add(Borrower borrower);
}

public record BookWithCatalog(Book Book, CatalogEntry CatalogEntry);
```

---

## Part 4: Application Layer Design

### Command/Query Structure

```
Application/
├── Books/
│   ├── RegisterBook/
│   │   ├── RegisterBookCommand.cs
│   │   └── RegisterBookCommandHandler.cs
│   ├── GetAllBooks/
│   │   ├── GetAllBooksQuery.cs
│   │   ├── GetAllBooksQueryHandler.cs
│   │   └── BookDetailsDto.cs
│   ├── BorrowBook/
│   │   ├── BorrowBookCommand.cs
│   │   └── BorrowBookCommandHandler.cs
│   └── ReturnBook/
│       ├── ReturnBookCommand.cs
│       └── ReturnBookCommandHandler.cs
└── Borrowers/
    └── RegisterBorrower/
        ├── RegisterBorrowerCommand.cs
        └── RegisterBorrowerCommandHandler.cs
```

### RegisterBook Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                    REGISTER BOOK COMMAND                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Input: { ISBN, Title, Author }                                     │
│                                                                      │
│  Flow:                                                               │
│  1. Validate and create ISBN value object                           │
│  2. Check if CatalogEntry exists for ISBN                           │
│     ├─ EXISTS: Verify title/author match                            │
│     │   ├─ MATCH: Continue                                          │
│     │   └─ MISMATCH: Reject with error                              │
│     └─ NOT EXISTS: Create new CatalogEntry                          │
│  3. Generate new BookId via IIdGenerator                            │
│  4. Create Book with (id, isbn)                                     │
│  5. Add Book (and CatalogEntry if new) to repositories              │
│  6. Save via IUnitOfWork                                            │
│                                                                      │
│  Output: { BookId }                                                  │
│                                                                      │
│  Errors:                                                             │
│  - Invalid ISBN format                                               │
│  - ISBN exists with different title/author (REJECT strategy)        │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### BorrowBook Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                     BORROW BOOK COMMAND                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Input: { BookId, BorrowerId }                                      │
│                                                                      │
│  Flow:                                                               │
│  1. Load Book by BookId                                             │
│     └─ NOT FOUND: Error                                             │
│  2. Verify Borrower exists                                          │
│     └─ NOT FOUND: Error                                             │
│  3. Call Book.Borrow(borrowerId, DateTime.UtcNow)                   │
│     └─ NOT AVAILABLE: Error (already borrowed)                      │
│  4. Save via IUnitOfWork                                            │
│                                                                      │
│  Output: { Success }                                                 │
│                                                                      │
│  Errors:                                                             │
│  - Book not found                                                    │
│  - Borrower not found                                                │
│  - Book already borrowed                                             │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### ReturnBook Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                     RETURN BOOK COMMAND                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Input: { BookId, BorrowerId }                                      │
│                                                                      │
│  Flow:                                                               │
│  1. Load Book by BookId                                             │
│     └─ NOT FOUND: Error                                             │
│  2. Verify current borrower matches BorrowerId (optional safety)    │
│     └─ MISMATCH: Error                                              │
│  3. Call Book.Return()                                              │
│     └─ NOT BORROWED: Error                                          │
│  4. Save via IUnitOfWork                                            │
│                                                                      │
│  Output: { Success }                                                 │
│                                                                      │
│  Errors:                                                             │
│  - Book not found                                                    │
│  - Book not borrowed                                                 │
│  - Borrower mismatch (if validated)                                 │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### GetAllBooks Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                     GET ALL BOOKS QUERY                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Input: (none)                                                       │
│                                                                      │
│  Flow:                                                               │
│  1. Call IBookRepository.GetAllWithCatalogAsync()                   │
│  2. Map to BookDetailsDto list                                      │
│                                                                      │
│  Output: List<BookDetailsDto>                                       │
│                                                                      │
│  BookDetailsDto:                                                     │
│  {                                                                   │
│    Id: string,                                                       │
│    ISBN: string,                                                     │
│    CatalogEntry: {                                                   │
│      Title: string,                                                  │
│      Author: string                                                  │
│    },                                                                │
│    IsAvailable: bool,                                                │
│    BorrowedBy: string? (optional)                                   │
│  }                                                                   │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### RegisterBorrower Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                   REGISTER BORROWER COMMAND                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Input: { Name, Email }                                             │
│                                                                      │
│  Flow:                                                               │
│  1. Validate and create Email value object                          │
│  2. Generate new BorrowerId via IIdGenerator                        │
│  3. Create Borrower with (id, name, email)                          │
│  4. Add to repository                                               │
│  5. Save via IUnitOfWork                                            │
│                                                                      │
│  Output: { BorrowerId }                                              │
│                                                                      │
│  Errors:                                                             │
│  - Invalid email format                                              │
│  - Empty name                                                        │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Part 5: API Design

### Endpoints

| Method | Endpoint                     | Description       | Request Body              | Response             |
|--------|------------------------------|-------------------|---------------------------|----------------------|
| POST   | `/api/borrowers`             | Register borrower | `RegisterBorrowerRequest` | `BorrowerResponse`   |
| POST   | `/api/books`                 | Register book     | `RegisterBookRequest`     | `BookResponse`       |
| GET    | `/api/books`                 | Get all books     | -                         | `List<BookResponse>` |
| POST   | `/api/books/{bookId}/borrow` | Borrow book       | `BorrowBookRequest`       | 204 No Content       |
| POST   | `/api/books/{bookId}/return` | Return book       | `ReturnBookRequest`       | 204 No Content       |

### Request Contracts

```csharp
// POST /api/borrowers
public record RegisterBorrowerRequest(
    string Name,
    string Email
);

// POST /api/books
public record RegisterBookRequest(
    string ISBN,
    string Title,
    string Author
);

// POST /api/books/{bookId}/borrow
public record BorrowBookRequest(
    string BorrowerId
);

// POST /api/books/{bookId}/return
public record ReturnBookRequest(
    string BorrowerId
);
```

### Response Contracts

```csharp
// Nested structure (Option B - reflects domain)
public record BookResponse(
    string Id,
    string ISBN,
    CatalogEntryResponse CatalogEntry,
    bool IsAvailable,
    string? BorrowedBy
);

public record CatalogEntryResponse(
    string Title,
    string Author
);

public record BorrowerResponse(
    string Id,
    string Name,
    string Email
);
```

### Example API Response

```json
// GET /api/books
[
  {
    "id": "book-abc123",
    "isbn": "9780132350884",
    "catalogEntry": {
      "title": "Clean Code",
      "author": "Robert C. Martin"
    },
    "isAvailable": true,
    "borrowedBy": null
  },
  {
    "id": "book-def456",
    "isbn": "9780132350884",
    "catalogEntry": {
      "title": "Clean Code",
      "author": "Robert C. Martin"
    },
    "isAvailable": false,
    "borrowedBy": "borrower-xyz789"
  }
]
```

---

## Part 6: Database Schema

### Tables

```sql
-- CatalogEntries table (ISBN as primary key)
CREATE TABLE CatalogEntries (
    ISBN        VARCHAR(13) PRIMARY KEY,
    Title       VARCHAR(500) NOT NULL,
    Author      VARCHAR(255) NOT NULL
);

-- Borrowers table
CREATE TABLE Borrowers (
    Id          VARCHAR(36) PRIMARY KEY,
    Name        VARCHAR(255) NOT NULL,
    Email       VARCHAR(255) NOT NULL
);

-- Books table
CREATE TABLE Books (
    Id          VARCHAR(36) PRIMARY KEY,
    ISBN        VARCHAR(13) NOT NULL,
    BorrowedBy  VARCHAR(36) NULL,
    BorrowedAt  TIMESTAMP NULL,

    CONSTRAINT FK_Books_CatalogEntries
        FOREIGN KEY (ISBN) REFERENCES CatalogEntries(ISBN),
    CONSTRAINT FK_Books_Borrowers
        FOREIGN KEY (BorrowedBy) REFERENCES Borrowers(Id)
);

-- Indexes
CREATE INDEX IX_Books_ISBN ON Books(ISBN);
CREATE INDEX IX_Books_BorrowedBy ON Books(BorrowedBy);
CREATE INDEX IX_Books_IsAvailable ON Books((BorrowedBy IS NULL));
```

### EF Core Considerations

- ISBN stored as string (VARCHAR 13) after normalization
- Value objects (ISBN, Email, BookId, BorrowerId) mapped as owned types or value conversions
- CatalogEntry has no surrogate key - ISBN is the key
- Soft delete not required for current scope

---

## Part 7: Project Structure

```
DomainDrivenLibrary/
├── Common/
│   ├── DomainDrivenLibrary.Domain.Shared/
│   │   └── Entities/
│   │       ├── IEntity.cs                 # (existing)
│   │       └── EntityBase.cs              # (existing)
│   └── DomainDrivenLibrary.Application.Abstractions/
│       ├── Data/
│       │   └── IUnitOfWork.cs             # (existing)
│       ├── Dependencies/
│       │   ├── ITransientDependency.cs    # (existing)
│       │   ├── IScopedDependency.cs       # (existing)
│       │   ├── ISingletonDependency.cs    # (existing)
│       │   └── DependencyRegistrator.cs   # (existing)
│       └── Identifier/
│           └── IIdGenerator.cs            # (existing)
│
├── Sources/
│   ├── DomainDrivenLibrary.Domain/
│   │   ├── Aggregates/
│   │   │   ├── Books/
│   │   │   │   ├── Book.cs
│   │   │   │   ├── BookId.cs
│   │   │   │   └── IBookRepository.cs
│   │   │   ├── Borrowers/
│   │   │   │   ├── Borrower.cs
│   │   │   │   ├── BorrowerId.cs
│   │   │   │   └── IBorrowerRepository.cs
│   │   │   └── Catalog/
│   │   │       ├── CatalogEntry.cs
│   │   │       └── ICatalogEntryRepository.cs
│   │   ├── ValueObjects/
│   │   │   ├── ISBN.cs
│   │   │   └── Email.cs
│   │   └── Exceptions/
│   │       └── DomainException.cs
│   │
│   ├── DomainDrivenLibrary.Application/
│   │   ├── Books/
│   │   │   ├── RegisterBook/
│   │   │   │   ├── RegisterBookCommand.cs
│   │   │   │   └── RegisterBookCommandHandler.cs
│   │   │   ├── GetAllBooks/
│   │   │   │   ├── GetAllBooksQuery.cs
│   │   │   │   ├── GetAllBooksQueryHandler.cs
│   │   │   │   └── BookDetailsDto.cs
│   │   │   ├── BorrowBook/
│   │   │   │   ├── BorrowBookCommand.cs
│   │   │   │   └── BorrowBookCommandHandler.cs
│   │   │   └── ReturnBook/
│   │   │       ├── ReturnBookCommand.cs
│   │   │       └── ReturnBookCommandHandler.cs
│   │   └── Borrowers/
│   │       └── RegisterBorrower/
│   │           ├── RegisterBorrowerCommand.cs
│   │           └── RegisterBorrowerCommandHandler.cs
│   │
│   └── DomainDrivenLibrary.Infrastructure/
│       ├── Persistence/
│       │   ├── LibraryDbContext.cs
│       │   ├── Configurations/
│       │   │   ├── BookConfiguration.cs
│       │   │   ├── BorrowerConfiguration.cs
│       │   │   └── CatalogEntryConfiguration.cs
│       │   └── Repositories/
│       │       ├── BookRepository.cs
│       │       ├── BorrowerRepository.cs
│       │       └── CatalogEntryRepository.cs
│       ├── Identity/
│       │   └── GuidIdGenerator.cs
│       └── DependencyInjection.cs
│
├── DomainDrivenLibrary.Presentation/
│   ├── Controllers/
│   │   ├── BooksController.cs
│   │   └── BorrowersController.cs
│   ├── Contracts/
│   │   ├── Requests/
│   │   │   ├── RegisterBookRequest.cs
│   │   │   ├── RegisterBorrowerRequest.cs
│   │   │   ├── BorrowBookRequest.cs
│   │   │   └── ReturnBookRequest.cs
│   │   └── Responses/
│   │       ├── BookResponse.cs
│   │       ├── CatalogEntryResponse.cs
│   │       └── BorrowerResponse.cs
│   └── Program.cs
│
└── Tests/
    ├── DomainDrivenLibrary.Domain.UnitTests/
    │   ├── Aggregates/
    │   │   ├── BookTests.cs
    │   │   ├── BorrowerTests.cs
    │   │   └── CatalogEntryTests.cs
    │   └── ValueObjects/
    │       ├── ISBNTests.cs
    │       └── EmailTests.cs
    └── DomainDrivenLibrary.Application.UnitTests/
        ├── Books/
        │   ├── RegisterBookCommandHandlerTests.cs
        │   ├── BorrowBookCommandHandlerTests.cs
        │   └── ReturnBookCommandHandlerTests.cs
        └── Borrowers/
            └── RegisterBorrowerCommandHandlerTests.cs
```

---

## Part 8: Design Decisions Summary

| Decision                     | Choice                          | Rationale                                            |
|------------------------------|---------------------------------|------------------------------------------------------|
| **CatalogEntry Aggregate**   | Yes                             | DRY principle, structural ISBN invariant enforcement |
| **CatalogEntry Identity**    | ISBN (natural key)              | ISBN is globally unique by definition                |
| **Loan Aggregate**           | Deferred                        | Not needed for current requirements                  |
| **Borrowing State Location** | On Book aggregate               | Single aggregate owns invariant                      |
| **ISBN Conflict Strategy**   | Reject with error               | Safe, explicit, prevents accidental corruption       |
| **ISBN Validation**          | Medium (10/13 digits)           | Catches obvious errors without being overly strict   |
| **Value Objects**            | ISBN, Email, BookId, BorrowerId | Type safety, validation at construction              |
| **API Response Style**       | Nested (Option B)               | Reflects domain structure                            |
| **Read Pattern**             | Repository returns joined data  | Simple, efficient for current scale                  |
| **Error Handling**           | Simple exceptions               | Keep simple, enhance later                           |

---

## Part 9: Implementation Sequence

### Phase 1: Domain Layer

1. Create `DomainException.cs`
2. Create value objects: `ISBN.cs`, `Email.cs`, `BookId.cs`, `BorrowerId.cs`
3. Create `CatalogEntry` aggregate
4. Create `Borrower` aggregate
5. Create `Book` aggregate
6. Create repository interfaces
7. Write domain unit tests

### Phase 2: Application Layer

1. Create `BookDetailsDto` and `BookWithCatalog` read models
2. Create `RegisterBorrowerCommand` + handler
3. Create `RegisterBookCommand` + handler
4. Create `GetAllBooksQuery` + handler
5. Create `BorrowBookCommand` + handler
6. Create `ReturnBookCommand` + handler
7. Write application unit tests

### Phase 3: Infrastructure Layer

1. Create `LibraryDbContext`
2. Create EF Core configurations for all entities
3. Implement `CatalogEntryRepository`
4. Implement `BorrowerRepository`
5. Implement `BookRepository`
6. Implement `GuidIdGenerator`
7. Create `DependencyInjection.cs` for service registration

### Phase 4: Presentation Layer

1. Create request/response contracts
2. Implement `BorrowersController`
3. Implement `BooksController`
4. Update `Program.cs` with DI and middleware
5. Test API endpoints

### Phase 5: Polish

1. Add EF Core migrations
2. Configure Docker/PostgreSQL
3. Add OpenAPI documentation
4. End-to-end testing

---

## Part 10: Future Considerations (Out of Scope)

The following are explicitly deferred and not part of this implementation:

- **Loan Aggregate**: For tracking loan history, due dates, fines
- **Reservation System**: Allowing borrowers to reserve unavailable books
- **Domain Events**: BookBorrowed, BookReturned for audit/notifications
- **Advanced Error Handling**: Result pattern, problem details
- **Authentication/Authorization**: User identity, roles
- **Pagination**: For GetAllBooks query
- **Search/Filtering**: By title, author, availability
- **Soft Delete**: For books and borrowers

---

## Appendix A: Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                              ERD                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│   ┌──────────────────┐                                              │
│   │  CatalogEntries  │                                              │
│   ├──────────────────┤                                              │
│   │ PK ISBN          │──┐                                           │
│   │    Title         │  │                                           │
│   │    Author        │  │                                           │
│   └──────────────────┘  │                                           │
│                         │ 1:N                                        │
│                         │                                           │
│   ┌──────────────────┐  │    ┌──────────────────┐                   │
│   │     Borrowers    │  │    │      Books       │                   │
│   ├──────────────────┤  │    ├──────────────────┤                   │
│   │ PK Id            │◄─┼────│ FK BorrowedBy    │                   │
│   │    Name          │  │    │ PK Id            │                   │
│   │    Email         │  └───►│ FK ISBN          │                   │
│   └──────────────────┘       │    BorrowedAt    │                   │
│                              └──────────────────┘                   │
│            0..1:N                                                    │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Appendix B: State Diagram - Book

```
┌─────────────────────────────────────────────────────────────────────┐
│                      BOOK STATE DIAGRAM                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│                    ┌─────────────┐                                  │
│                    │  Registered │                                  │
│                    └──────┬──────┘                                  │
│                           │                                          │
│                           ▼                                          │
│                    ┌─────────────┐                                  │
│            ┌──────►│  Available  │◄──────┐                          │
│            │       └──────┬──────┘       │                          │
│            │              │              │                          │
│            │    Borrow()  │    Return()  │                          │
│            │              ▼              │                          │
│            │       ┌─────────────┐       │                          │
│            └───────│  Borrowed   │───────┘                          │
│                    └─────────────┘                                  │
│                                                                      │
│   Invariant: Only one borrower at a time (BorrowedBy is singular)  │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---