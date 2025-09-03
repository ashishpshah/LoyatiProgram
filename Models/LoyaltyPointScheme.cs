using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin
{
	public class LoyaltyPointScheme : EntitiesBase
	{
		public override long Id { get; set; }
		public string? SchemeName { get; set; }

		public long ProductID { get; set; }

		public long? PackageType_ID { get; set; }

		public long? SKUSize_ID { get; set; }

		public int MinPurchaseQty { get; set; }

		public int? MaxPurchaseQty { get; set; }

		public int? LoyaltyPoints { get; set; }

		public DateTime? EffectiveStartDate { get; set; }

		public DateTime? EffectiveEndDate { get; set; }

		public string? SchemeFor { get; set; }

		[NotMapped] public string SchemeFor_Text { get; set; }
		[NotMapped] public string Product_Name { get; set; }
		[NotMapped] public string PackageType_Name { get; set; }
		[NotMapped] public string SKUSize_Name { get; set; }

	}
}
