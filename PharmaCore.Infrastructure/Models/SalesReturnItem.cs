using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class SalesReturnItem
{
    public int SalesReturnItemId { get; set; }

    public int? SalesReturnId { get; set; }

    public int? SaleItemId { get; set; }

    public int? BatchId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual SaleItem? SaleItem { get; set; }

    public virtual SalesReturn? SalesReturn { get; set; }
}
