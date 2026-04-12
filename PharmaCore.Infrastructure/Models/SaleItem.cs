using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class SaleItem
{
    public int SaleItemId { get; set; }

    public int? SaleId { get; set; }

    public int? MedicineId { get; set; }

    public int? BatchId { get; set; }

    public int Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public decimal? PurchasePrice { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Medicine? Medicine { get; set; }

    public virtual Sale? Sale { get; set; }

    public virtual ICollection<SalesReturnItem> SalesReturnItems { get; set; } = new List<SalesReturnItem>();
}
