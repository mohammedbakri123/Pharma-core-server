using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class Adjustment
{
    public int AdjustmentId { get; set; }

    public int? MedicineId { get; set; }

    public int? BatchId { get; set; }

    public int? Quantity { get; set; }

    public short? Type { get; set; }

    public string? Reason { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Medicine? Medicine { get; set; }

    public virtual User? User { get; set; }
}
