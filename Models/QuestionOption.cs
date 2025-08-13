using System;
using System.Collections.Generic;

namespace Seed_Admin;

public partial class QuestionOption
{
    public long Id { get; set; }

    public long QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public string OptionValue { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public long LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
