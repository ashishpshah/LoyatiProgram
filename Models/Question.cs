using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin;

public partial class Question : EntitiesBase
{
	public override long Id { get; set; }

	public long SurveyId { get; set; }

	public string QuestionText { get; set; } = null!;

	public string QuestionType { get; set; } = null!;
	[NotMapped] public string QuestionType_Text { get; set; }
	[NotMapped] public List<OptionDto> Options { get; set; }
	[NotMapped] public OptionDto Rating { get; set; }
	[NotMapped] public bool IsYesNo { get; set; }

	public int? DisplayOrder { get; set; }
}

public class OptionDto
{
	public string Text { get; set; }
	public string Value { get; set; }
}