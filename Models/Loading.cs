using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin
{
	public class Loading : EntitiesBase
	{
		public override long Id { get; set; }
		public long Order_ID { get; set; }
		public long User_ID { get; set; }
		public long Product_ID { get; set; }
		public long PackageType_ID { get; set; }
		public long SKUSize_ID { get; set; }
		[NotMapped] public string Product_Name { get; set; }
		[NotMapped] public string PackageType_Name { get; set; }
		[NotMapped] public string SKUSize_Name { get; set; }
		[NotMapped] public string Dealer_Name { get; set; }
		public decimal? Qty { get; set; }
		[NotMapped] public decimal? Ordered_Qty { get; set; }
		[NotMapped] public decimal? Loaded_Qty { get; set; }
		[NotMapped] public string Order_No { get; set; }


		[NotMapped] public List<(int SrNo, long QR_Code_Id, string QR_Code, string Status)> listQRCode { get; set; }
	}
}
