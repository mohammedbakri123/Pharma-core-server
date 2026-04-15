using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class Medicine
{
    public int MedicineId { get; set; }

    public string Name { get; set; } = null!;

    public string? ArabicName { get; set; }

    public string? Barcode { get; set; }

    public int? CategoryId { get; set; }

    public short? Unit { get; set; }
    
    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Adjustment> Adjustments { get; set; } = new List<Adjustment>();

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
