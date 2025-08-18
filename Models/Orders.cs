namespace Seed_Admin
{
    public class Orders : EntitiesBase
    {
        public override long Id { get; set; }
        public string Order_No { get; set; }
        public DateTime ? Order_Date { get; set; }       
        public string Order_Date_Text { get; set; }       
        public decimal ? Total_Qty { get; set; }
        public List<Order_Detail> list_Order_Details{ get; set; }
    }
    public class Order_Detail : EntitiesBase {
        
        public override long Id { get; set; }
        public long Order_ID { get; set; }
        public long Product_ID { get; set; }
        public string Product_Name { get; set; }
        public decimal?  Qty { get; set; }


    }
}
