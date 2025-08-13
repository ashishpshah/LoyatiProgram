using System;
using System.Collections.Generic;

namespace Seed_Admin;

public partial class QuestionOption : EntitiesBase
{
	public override long Id { get; set; }

	public long QuestionId { get; set; }

	public string OptionText { get; set; } = null!;

	public string OptionValue { get; set; } = null!;

	public int? DisplayOrder { get; set; }
}
