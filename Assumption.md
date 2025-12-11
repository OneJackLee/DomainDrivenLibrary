# Domain Driven Library - Assumption

This document outlines the assumption that was done during the development

- Agentic coding tool is allowed in the development, but with transparency provided (with necessary design document
  provided)
    - Instead of Vibe-coding without checking the code, this should be done as a spec-driven development process where
      design of use cases, domain, and architectural specifications should be done by me.
- I am using UNIX-based operating system to develop this .NET 9 application, thus Windows-related feature (such as the
  docker compose) was created by assumption
- No UI or interact required, purely backend
- Authentication of the API endpoints is not required

## Design assumption

- Borrower is uniquely identified by the email address. In other words, an email address can only be owned by a single
  borrower.
- Books with the different titles or different authors but same ISBN numbers will be treated as a typo and rejected.
- Pagination is currently not required for any GetAll endpoints
