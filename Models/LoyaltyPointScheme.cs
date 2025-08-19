using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin
{
    public class LoyaltyPointScheme : EntitiesBase
    {
        public override long Id { get; set; }
        public  long ProductID { get; set; }
        public int MaxPurchaseQty { get; set; }
        public int MinPurchaseQty { get; set; }
        public int LoyaltyPoints { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [NotMapped]public string Product_Name { get; set; }

    }
}
