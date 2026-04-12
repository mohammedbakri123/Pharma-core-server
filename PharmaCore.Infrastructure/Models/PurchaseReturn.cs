using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class PurchaseReturn
{
    public int PurchaseReturnId { get; set; }

    public int? PurchaseId { get; set; }

    public int? SupplierId { get; set; }

    public int? UserId { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Purchase? Purchase { get; set; }

    public virtual ICollection<PurchaseReturnItem> PurchaseReturnItems { get; set; } = new List<PurchaseReturnItem>();

    public virtual Supplier? Supplier { get; set; }

    public virtual User? User { get; set; }
}
