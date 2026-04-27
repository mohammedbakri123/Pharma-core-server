using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public sealed partial class Purchase
{
    public int PurchaseId { get; set; }

    public int? SupplierId { get; set; }

    public string? InvoiceNumber { get; set; }

    public decimal? TotalAmount { get; set; }

    public short? Status { get; set; }

    public DateTime? CreatedAt { get; init; }

    public string? Note { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

    public ICollection<PurchaseReturn> PurchaseReturns { get; set; } = new List<PurchaseReturn>();

    public Supplier? Supplier { get; set; }
}
