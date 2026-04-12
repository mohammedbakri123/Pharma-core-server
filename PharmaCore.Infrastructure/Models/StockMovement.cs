using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class StockMovement
{
    public int StockMovementId { get; set; }

    public int? MedicineId { get; set; }

    public int? BatchId { get; set; }

    public int Quantity { get; set; }

    public short? Type { get; set; }

    public short? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Medicine? Medicine { get; set; }
}
