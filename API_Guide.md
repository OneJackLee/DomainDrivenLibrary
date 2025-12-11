# Domain Driven Library - API Reference

This document outlines the API endpoints for interacting with the Domain Driven Library system. It is intended for
client applications to integrate with this API for managing books, borrowers, and catalog entries.

## API access

We have shortened the endpoint URLs throughout this document by omitting the domain name. For example, the full URL
for `/api/books` is actually `https://localhost:7236/api/books`

## Get started

### Authentication

Currently, the API does not require authentication. All endpoints are publicly accessible.

## Error Handling

All error responses follow a consistent format:

```json
{
  "error": "ErrorType",
  "message": "Detailed error message"
}
```

### Error Types

| Error Type         | HTTP Status | Description                                       |
|--------------------|-------------|---------------------------------------------------|
| `NotFound`         | 404         | The requested resource was not found              |
| `Conflict`         | 409         | A conflict occurred (e.g., duplicate entry)       |
| `ValidationError`  | 400         | Invalid input data                                |
| `InvalidOperation` | 400         | The operation is not allowed in the current state |
| `InternalError`    | 500         | An unexpected server error occurred               |

---

## 1. Register Book

### Endpoint

| Method | URL          |
|--------|--------------|
| POST   | `/api/books` |

### Description

Registers a new book copy in the library. If a catalog entry with the same ISBN already exists, the book will be
associated with that entry. If the ISBN is new, a new catalog entry will be created.

### Request Body

The request body contains the book information to register.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "isbn": {
      "type": "string",
      "description": "The ISBN of the book (10 or 13 digits)"
    },
    "title": {
      "type": "string",
      "description": "The title of the book"
    },
    "author": {
      "type": "string",
      "description": "The author of the book"
    }
  },
  "required": ["isbn", "title", "author"],
  "additionalProperties": false
}
```

#### Example

```json
{
  "isbn": "9780134685991",
  "title": "Effective Java",
  "author": "Joshua Bloch"
}
```

### Response Body

The response contains the registered book information.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "description": "The ULID of the book copy"
    },
    "isbn": {
      "type": "string",
      "description": "The ISBN of the book"
    },
    "catalogEntry": {
      "type": "object",
      "properties": {
        "title": {
          "type": "string",
          "description": "The book title"
        },
        "author": {
          "type": "string",
          "description": "The book author"
        }
      },
      "required": ["title", "author"]
    },
    "isAvailable": {
      "type": "boolean",
      "description": "Whether the book is available for borrowing"
    },
    "borrowedBy": {
      "type": "string",
      "description": "The ULID of the borrower if currently borrowed",
      "nullable": true
    }
  },
  "required": ["id", "isbn", "catalogEntry", "isAvailable"]
}
```

#### Example (201 Created)

```json
{
  "id": "01JCZN3K5QVXHGW8F2E4D6B9A7",
  "isbn": "9780134685991",
  "catalogEntry": {
    "title": "Effective Java",
    "author": "Joshua Bloch"
  },
  "isAvailable": true,
  "borrowedBy": null
}
```

#### Example (400 Bad Request - Invalid ISBN)

```json
{
  "error": "ValidationError",
  "message": "Invalid ISBN format. ISBN must be 10 or 13 digits."
}
```

#### Example (409 Conflict - ISBN Mismatch)

```json
{
  "error": "Conflict",
  "message": "ISBN exists with different title/author"
}
```

---

## 2. Get All Books

### Endpoint

| Method | URL          |
|--------|--------------|
| GET    | `/api/books` |

### Description

Retrieves all books in the library, including their availability status and borrower information.

### Request Body

None

### Response Body

The response is an array of book objects.

#### Schema

```json
{
  "type": "array",
  "items": {
    "type": "object",
    "properties": {
      "id": {
        "type": "string",
        "description": "The unique identifier of the book copy"
      },
      "isbn": {
        "type": "string",
        "description": "The ISBN of the book"
      },
      "catalogEntry": {
        "type": "object",
        "properties": {
          "title": {
            "type": "string",
            "description": "The book title"
          },
          "author": {
            "type": "string",
            "description": "The book author"
          }
        },
        "required": ["title", "author"]
      },
      "isAvailable": {
        "type": "boolean",
        "description": "Whether the book is available for borrowing"
      },
      "borrowedBy": {
        "type": "string",
        "description": "The ID of the borrower if currently borrowed",
        "nullable": true
      }
    },
    "required": ["id", "isbn", "catalogEntry", "isAvailable"]
  }
}
```

#### Example (200 OK)

```json
[
  {
    "id": "01JCZN3K5QVXHGW8F2E4D6B9A7",
    "isbn": "9780134685991",
    "catalogEntry": {
      "title": "Effective Java",
      "author": "Joshua Bloch"
    },
    "isAvailable": true,
    "borrowedBy": null
  },
  {
    "id": "01JCZN4M7RWYPJX9G3H5K8C2B6",
    "isbn": "9780132350884",
    "catalogEntry": {
      "title": "Clean Code",
      "author": "Robert C. Martin"
    },
    "isAvailable": false,
    "borrowedBy": "01JCZN5P9SXZQKY0H4J6L8D3C7"
  }
]
```

---

## 3. Borrow Book

### Endpoint

| Method | URL                          |
|--------|------------------------------|
| POST   | `/api/books/{bookId}/borrow` |

### Description

Allows a registered borrower to borrow an available book from the library.

### Path Parameters

- `{bookId}`: The ULID of the book to borrow. Replace `{bookId}` with the actual book ULID in the API request.

### Request Body

The request body contains the borrower information.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "borrowerId": {
      "type": "string",
      "description": "The ULID of the borrower checking out the book"
    }
  },
  "required": ["borrowerId"],
  "additionalProperties": false
}
```

#### Example

```json
{
  "borrowerId": "01JCZN5P9SXZQKY0H4J6L8D3C7"
}
```

### Response Body

The response contains the updated book information.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "description": "The ULID of the book copy"
    },
    "isbn": {
      "type": "string",
      "description": "The ISBN of the book"
    },
    "catalogEntry": {
      "type": "object",
      "properties": {
        "title": {
          "type": "string",
          "description": "The book title"
        },
        "author": {
          "type": "string",
          "description": "The book author"
        }
      },
      "required": ["title", "author"]
    },
    "isAvailable": {
      "type": "boolean",
      "description": "Whether the book is available for borrowing"
    },
    "borrowedBy": {
      "type": "string",
      "description": "The ULID of the borrower",
      "nullable": true
    }
  },
  "required": ["id", "isbn", "catalogEntry", "isAvailable"]
}
```

#### Example (200 OK)

```json
{
  "id": "01JCZN3K5QVXHGW8F2E4D6B9A7",
  "isbn": "9780134685991",
  "catalogEntry": {
    "title": "Effective Java",
    "author": "Joshua Bloch"
  },
  "isAvailable": false,
  "borrowedBy": "01JCZN5P9SXZQKY0H4J6L8D3C7"
}
```

#### Example (404 Not Found - Book)

```json
{
  "error": "NotFound",
  "message": "Book not found"
}
```

#### Example (404 Not Found - Borrower)

```json
{
  "error": "NotFound",
  "message": "Borrower not found"
}
```

#### Example (409 Conflict - Already Borrowed)

```json
{
  "error": "Conflict",
  "message": "Book is already borrowed"
}
```

---

## 4. Return Book

### Endpoint

| Method | URL                          |
|--------|------------------------------|
| POST   | `/api/books/{bookId}/return` |

### Description

Allows a borrower to return a borrowed book to the library.

### Path Parameters

- `{bookId}`: The ULID of the book to return. Replace `{bookId}` with the actual book ULID in the API request.

### Request Body

The request body contains the borrower information.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "borrowerId": {
      "type": "string",
      "description": "The ULID of the borrower returning the book"
    }
  },
  "required": ["borrowerId"],
  "additionalProperties": false
}
```

#### Example

```json
{
  "borrowerId": "01JCZN5P9SXZQKY0H4J6L8D3C7"
}
```

### Response Body

The response contains the updated book information.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "description": "The ULID of the book copy"
    },
    "isbn": {
      "type": "string",
      "description": "The ISBN of the book"
    },
    "catalogEntry": {
      "type": "object",
      "properties": {
        "title": {
          "type": "string",
          "description": "The book title"
        },
        "author": {
          "type": "string",
          "description": "The book author"
        }
      },
      "required": ["title", "author"]
    },
    "isAvailable": {
      "type": "boolean",
      "description": "Whether the book is available for borrowing"
    },
    "borrowedBy": {
      "type": "string",
      "description": "The ULID of the borrower if currently borrowed",
      "nullable": true
    }
  },
  "required": ["id", "isbn", "catalogEntry", "isAvailable"]
}
```

#### Example (200 OK)

```json
{
  "id": "01JCZN3K5QVXHGW8F2E4D6B9A7",
  "isbn": "9780134685991",
  "catalogEntry": {
    "title": "Effective Java",
    "author": "Joshua Bloch"
  },
  "isAvailable": true,
  "borrowedBy": null
}
```

#### Example (404 Not Found)

```json
{
  "error": "NotFound",
  "message": "Book not found"
}
```

#### Example (409 Conflict - Not Borrowed)

```json
{
  "error": "Conflict",
  "message": "Book is not currently borrowed"
}
```

#### Example (400 Bad Request - Wrong Borrower)

```json
{
  "error": "InvalidOperation",
  "message": "Book was not borrowed by this borrower"
}
```

---

## 5. Get Catalog Entry

### Endpoint

| Method | URL                           |
|--------|-------------------------------|
| GET    | `/api/catalog-entries/{isbn}` |

### Description

Retrieves a catalog entry by its ISBN. A catalog entry contains the metadata for a book (title and author) that is
shared across all copies of that book.

### Path Parameters

- `{isbn}`: The ISBN of the catalog entry. Replace `{isbn}` with the actual ISBN in the API request.

### Request Body

None

### Response Body

The response contains the catalog entry details.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "isbn": {
      "type": "string",
      "description": "The ISBN of the catalog entry"
    },
    "title": {
      "type": "string",
      "description": "The book title"
    },
    "author": {
      "type": "string",
      "description": "The book author"
    }
  },
  "required": ["isbn", "title", "author"]
}
```

#### Example (200 OK)

```json
{
  "isbn": "9780134685991",
  "title": "Effective Java",
  "author": "Joshua Bloch"
}
```

#### Example (400 Bad Request - Invalid ISBN)

```json
{
  "error": "ValidationError",
  "message": "Invalid ISBN format"
}
```

#### Example (404 Not Found)

```json
{
  "error": "NotFound",
  "message": "Catalog entry not found"
}
```

---

## 6. Update Catalog Entry

### Endpoint

| Method | URL                           |
|--------|-------------------------------|
| PUT    | `/api/catalog-entries/{isbn}` |

### Description

Updates a catalog entry's title and author. This will affect all book copies associated with this catalog entry.

### Path Parameters

- `{isbn}`: The ISBN of the catalog entry to update. Replace `{isbn}` with the actual ISBN in the API request.

### Request Body

The request body contains the updated catalog entry information.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "title": {
      "type": "string",
      "description": "The new title for the catalog entry"
    },
    "author": {
      "type": "string",
      "description": "The new author for the catalog entry"
    }
  },
  "required": ["title", "author"],
  "additionalProperties": false
}
```

#### Example

```json
{
  "title": "Effective Java, 3rd Edition",
  "author": "Joshua Bloch"
}
```

### Response Body

The response contains the updated catalog entry details.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "isbn": {
      "type": "string",
      "description": "The ISBN of the catalog entry"
    },
    "title": {
      "type": "string",
      "description": "The updated book title"
    },
    "author": {
      "type": "string",
      "description": "The updated book author"
    }
  },
  "required": ["isbn", "title", "author"]
}
```

#### Example (200 OK)

```json
{
  "isbn": "9780134685991",
  "title": "Effective Java, 3rd Edition",
  "author": "Joshua Bloch"
}
```

#### Example (400 Bad Request - Invalid ISBN)

```json
{
  "error": "ValidationError",
  "message": "Invalid ISBN format"
}
```

#### Example (400 Bad Request - Empty Title)

```json
{
  "error": "ValidationError",
  "message": "Title cannot be empty"
}
```

#### Example (404 Not Found)

```json
{
  "error": "NotFound",
  "message": "Catalog entry not found"
}
```

---

## 7. Register Borrower

### Endpoint

| Method | URL              |
|--------|------------------|
| POST   | `/api/borrowers` |

### Description

Registers a new borrower in the library system. A borrower must be registered before they can borrow books.

### Request Body

The request body contains the borrower information.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "name": {
      "type": "string",
      "description": "The name of the borrower"
    },
    "email": {
      "type": "string",
      "description": "The email address of the borrower"
    }
  },
  "required": ["name", "email"],
  "additionalProperties": false
}
```

#### Example

```json
{
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

### Response Body

The response contains the registered borrower information.

#### Schema

```json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "string",
      "description": "The ULID of the borrower"
    },
    "name": {
      "type": "string",
      "description": "The borrower's name"
    },
    "email": {
      "type": "string",
      "description": "The borrower's email address"
    }
  },
  "required": ["id", "name", "email"]
}
```

#### Example (201 Created)

```json
{
  "id": "01JCZN5P9SXZQKY0H4J6L8D3C7",
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

#### Example (400 Bad Request - Invalid Email)

```json
{
  "error": "ValidationError",
  "message": "Invalid email format"
}
```

#### Example (400 Bad Request - Empty Name)

```json
{
  "error": "ValidationError",
  "message": "Name cannot be empty"
}
```

#### Example (409 Conflict - Duplicate Email)

```json
{
  "error": "Conflict",
  "message": "Email address already registered"
}
```

---

## 8. Get All Borrowers

### Endpoint

| Method | URL              |
|--------|------------------|
| GET    | `/api/borrowers` |

### Description

Retrieves all registered borrowers in the library system.

### Request Body

None

### Response Body

The response is an array of borrower objects.

#### Schema

```json
{
  "type": "array",
  "items": {
    "type": "object",
    "properties": {
      "id": {
        "type": "string",
        "description": "The unique identifier of the borrower"
      },
      "name": {
        "type": "string",
        "description": "The borrower's name"
      },
      "email": {
        "type": "string",
        "description": "The borrower's email address"
      }
    },
    "required": ["id", "name", "email"]
  }
}
```

#### Example (200 OK)

```json
[
  {
    "id": "01JCZN5P9SXZQKY0H4J6L8D3C7",
    "name": "John Doe",
    "email": "john.doe@example.com"
  },
  {
    "id": "01JCZN6R1TYARMZ2J5K7M9E4D8",
    "name": "Jane Smith",
    "email": "jane.smith@example.com"
  }
]
```

---