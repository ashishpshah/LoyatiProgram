using System;
using System.Collections.Generic;

namespace Seed_Admin;

public partial class LoyaltyPoint
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long QrcodeId { get; set; }

    public decimal Points { get; set; }

    public DateTime EarnedDateTime { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public long LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
