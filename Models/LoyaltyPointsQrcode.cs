using System;
using System.Collections.Generic;

namespace Seed_Admin;

public partial class LoyaltyPointsQrcode
{
    public long Id { get; set; }

    public string Qrcode { get; set; } = null!;

    public bool IsScanned { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public long LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
