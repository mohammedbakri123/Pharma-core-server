using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class Expense
{
    public int ExpenseId { get; set; }

    public int? UserId { get; set; }

    public decimal? Amount { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User? User { get; set; }
}
