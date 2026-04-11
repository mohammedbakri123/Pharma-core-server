using Microsoft.EntityFrameworkCore;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<Medicine> Medicines { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}