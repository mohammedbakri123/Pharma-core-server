using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class Sale
{
    public int SaleId { get; set; }

    public int? UserId { get; set; }

    public int? CustomerId { get; set; }

    public short? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? Discount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Note { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    public virtual ICollection<SalesReturn> SalesReturns { get; set; } = new List<SalesReturn>();

    public virtual User? User { get; set; }
}
