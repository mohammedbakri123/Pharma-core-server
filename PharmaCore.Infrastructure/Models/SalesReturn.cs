using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class SalesReturn
{
    public int SalesReturnId { get; set; }

    public int? SaleId { get; set; }

    public int? CustomerId { get; set; }

    public int? UserId { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Sale? Sale { get; set; }

    public virtual ICollection<SalesReturnItem> SalesReturnItems { get; set; } = new List<SalesReturnItem>();

    public virtual User? User { get; set; }
}
