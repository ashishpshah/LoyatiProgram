namespace Seed_Admin;

public partial class LoyaltyPointSchemeUser
{
    public long UserId { get; set; }

    public long OrderId { get; set; }

    public long OrderDetailId { get; set; }

    public long ProductId { get; set; }

    public long PackageTypeId { get; set; }

    public long SkusizeId { get; set; }

    public long LoyaltyPointSchemeId { get; set; }

    public decimal Points { get; set; }

    public DateTime EarnedDateTime { get; set; }
}
