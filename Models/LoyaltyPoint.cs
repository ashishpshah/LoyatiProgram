using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin;

public partial class LoyaltyPoint : EntitiesBase
{
	public override long Id { get; set; }

	public long UserId { get; set; }

	public long QrcodeId { get; set; }

	public decimal Points { get; set; }

	public DateTime EarnedDateTime { get; set; }
}

public partial class LoyaltyPointViewModel
{
	public long UserId { get; set; }
	public string ClaimedBy { get; set; }

	public long QrCodeId { get; set; }
	public string QrCode_Base64 { get; set; }
	public string QrCode { get; set; }

	public decimal Points { get; set; }
	public bool IsClaimed { get; set; }
	public string IsClaimed_Text { get { return (IsClaimed ? "Yes" : "No"); } }

	public long ClaimedDate_Ticks { get; set; }
	public long GenerateDate_Ticks { get; set; }
	public string ClaimedDate_Text { get; set; }
	public string GenerateDate_Text { get; set; }
}
