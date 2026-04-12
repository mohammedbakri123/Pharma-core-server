using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class PurchaseReturnItem
{
    public int PurchaseReturnItemId { get; set; }

    public int? PurchaseReturnId { get; set; }

    public int? PurchaseItemId { get; set; }

    public int? BatchId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual PurchaseItem? PurchaseItem { get; set; }

    public virtual PurchaseReturn? PurchaseReturn { get; set; }
}
