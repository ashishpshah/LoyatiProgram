using System;
using System.Collections.Generic;

namespace Seed_Admin.Models_Temp;

public partial class Plant
{
    public long Id { get; set; }

    public string PlantCode { get; set; } = null!;

    public string PlantName { get; set; } = null!;

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public long CountryId { get; set; }

    public long StateId { get; set; }

    public long DistrictId { get; set; }

    public long TalukaId { get; set; }

    public long CityId { get; set; }

    public int? PinCode { get; set; }

    public bool IsActive { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public long? LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    public bool? IsDeleted { get; set; }
}
