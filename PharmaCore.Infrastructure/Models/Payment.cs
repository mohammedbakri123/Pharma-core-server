using System;
using System.Collections.Generic;

namespace PharmaCore.Infrastructure.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public short Type { get; set; }

    public short ReferenceType { get; set; }

    public int ReferenceId { get; set; }

    public short? Method { get; set; }

    public int? UserId { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User? User { get; set; }
}
