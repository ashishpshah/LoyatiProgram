using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin
{
    public class Product : EntitiesBase
    {
        public override long Id { get; set; }
        public long PackageType_ID { get; set; }
        public long SKUSize_ID { get; set; }
        public string Product_ID { get; set; }
        public string Name { get; set; }
        public string UOM { get; set; }
        public decimal MRP { get; set; }
        [NotMapped] public string UOM_TEXT { get; set; }
       [NotMapped] public string PackageType_Name { get; set; }
       [NotMapped] public string SKUSize_Name { get; set; }
    }
}
