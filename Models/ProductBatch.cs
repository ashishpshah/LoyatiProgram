using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin;

public partial class ProductBatch : EntitiesBase
{
    public override long Id { get; set; }
    public string ? BatchNo { get; set; } = null!;

    public long  ProductId { get; set; }
    public long PackageType_ID { get; set; }
    public long  SKUSize_ID { get; set; }


    public string? SeedType { get; set; }

    public decimal? Qty { get; set; }

    public long? MfgBy { get; set; }

    public DateTime? MfgDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Remark { get; set; }

    [NotMapped] public string MfgDate_Text { get; set; }
    [NotMapped] public string ExpiryDate_Text { get; set; }
    [NotMapped] public string MfgBy_Text { get; set; }
    [NotMapped] public string Product_Text { get; set; }
    [NotMapped] public string PackageType_Text { get; set; }
    [NotMapped] public string SKUSize_Text { get; set; }

}
