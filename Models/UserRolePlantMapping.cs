using System;
using System.Collections.Generic;

namespace Seed_Admin;

public partial class UserRolePlantMapping
{
    public long Id { get; set; }

    public long UserId { get; set; }

	public long RoleId { get; set; }

	public long PlantId { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public long LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
