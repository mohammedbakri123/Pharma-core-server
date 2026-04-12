using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class PurchaseItem
{
    public int PurchaseItemId { get; set; }

    public int? PurchaseId { get; set; }

    public int? MedicineId { get; set; }

    public int? BatchId { get; set; }

    public int Quantity { get; set; }

    public decimal? PurchasePrice { get; set; }

    public decimal? SellPrice { get; set; }

    public DateOnly? ExpireDate { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Medicine? Medicine { get; set; }

    public virtual Purchase? Purchase { get; set; }

    public virtual ICollection<PurchaseReturnItem> PurchaseReturnItems { get; set; } = new List<PurchaseReturnItem>();
}
