using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class Batch
{
    public int BatchId { get; set; }

    public int MedicineId { get; set; }

    public string? BatchNumber { get; set; }

    public int QuantityRemaining { get; set; }
    
    public int? QuantityEntered { get; set; }


    public decimal PurchasePrice { get; set; }

    public decimal SellPrice { get; set; }

    public DateOnly? ExpireDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Adjustment> Adjustments { get; set; } = new List<Adjustment>();

    public virtual Medicine Medicine { get; set; } = null!;

    public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

    public virtual ICollection<PurchaseReturnItem> PurchaseReturnItems { get; set; } = new List<PurchaseReturnItem>();

    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    public virtual ICollection<SalesReturnItem> SalesReturnItems { get; set; } = new List<SalesReturnItem>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
