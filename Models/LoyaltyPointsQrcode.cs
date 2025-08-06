using System;
using System.Collections.Generic;

namespace Seed_Admin;

public partial class LoyaltyPointsQrcode : EntitiesBase
{
	public override long Id { get; set; }

	public string Qrcode { get; set; } = null!;

	public bool IsScanned { get; set; }
}
