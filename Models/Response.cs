using System;
using System.Collections.Generic;

namespace Seed_Admin;

public partial class Response
{
    public long Id { get; set; }

    public long RespondentId { get; set; }

    public long QuestionId { get; set; }

    public long OptionId { get; set; }

    public string TextAnswer { get; set; } = null!;

    public decimal NumericAnswer { get; set; }

    public DateTime? AnswerDate { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public long LastModifiedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
