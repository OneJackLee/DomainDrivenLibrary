using DomainDrivenLibrary.Data;
using Microsoft.EntityFrameworkCore;

namespace DomainDrivenLibrary;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
}