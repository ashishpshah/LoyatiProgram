using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin;

public partial class ProductQrCode : EntitiesBase
{
    public override long Id { get; set; }

    public long RequestId { get; set; }
    public long BatchId { get; set; }

    public long ProductId { get; set; }
    public long PackageType_ID { get; set; }
    public long SKUSize_ID { get; set; }

    public string? QrCode_Base64 { get; set; }

    public string QrCode { get; set; } = null!;

    public long DecimalValue { get; set; }
    public string YYYYMM { get; set; }
    public decimal? Points { get; set; }

    public bool IsScanned { get; set; }

    [NotMapped]public long SrNo { get; set; }
    [NotMapped]public string Status { get; set; }
    [NotMapped]public string Reason { get; set; }
    [NotMapped]public long CreatedDate_Ticks { get; set; }
    [NotMapped] public long LastModifiedDate_Ticks { get; set; }
    [NotMapped] public string Product_Text { get; set; }
    [NotMapped] public string PackageType_Text { get; set; }
    [NotMapped] public string SKUSize_Text { get; set; }
}
