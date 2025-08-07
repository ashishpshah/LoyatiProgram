using System;
using System.Collections.Generic;

namespace Seed_Admin;

public partial class LoyaltyPointsQrcode : EntitiesBase
{
	public override long Id { get; set; }

	public string QRCode_Base64 { get; set; } = null!;
	public string Qrcode { get; set; } = null!;

	public decimal Points { get; set; }

	public bool IsScanned { get; set; }
}
