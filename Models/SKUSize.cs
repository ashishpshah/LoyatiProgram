using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin
{
    public class SKUSize : EntitiesBase
    {
        public override long Id { get; set; }
        public decimal Value { get; set; }
        public string  Unit { get; set; }
        [NotMapped]public string UNIT_TEXT { get; set; }
        public string SKUSizeName { get; set; }
        public string Description { get; set; }
    }
}
