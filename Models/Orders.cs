using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin
{
    public class Orders : EntitiesBase
    {
        public override long Id { get; set; }
        public string Dealer_Name { get; set; }
        public string Order_No { get; set; }
        public DateTime ? Order_Date { get; set; }       
        public string Order_Date_Text { get; set; }  
        public decimal ? Total_Qty { get; set; }
		public string Status { get; set; }
		public string Status_Text { get; set; }
		public List<Order_Detail> list_Order_Details{ get; set; }
    }
    public class Order_Detail : EntitiesBase {
        
        public override long Id { get; set; }
        public string Dealer_Name { get; set; }
        public long Order_ID { get; set; }
        public long Product_ID { get; set; }
        public long PackageType_ID { get; set; }
        public long SKUSize_ID { get; set; }
        [NotMapped]public string Product_Name { get; set; }
        [NotMapped] public string PackageType_Name { get; set; }
        [NotMapped] public string SKUSize_Name { get; set; }
        public decimal?  Qty { get; set; }
		[NotMapped]public DateTime? Order_Date { get; set; }
		[NotMapped]public string Order_Date_Text { get; set; }
		[NotMapped]public string Order_No { get; set; }
		[NotMapped] public string Order_Status { get; set; }


	}
}
