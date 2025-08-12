using System;
using System.Collections.Generic;

namespace Seed_Admin;

public partial class LovMaster
{
    public string LovColumn { get; set; } = null!;

    public string LovCode { get; set; } = null!;

    public string LovDesc { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public long? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
