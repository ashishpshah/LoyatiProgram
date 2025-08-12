using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin;

public partial class Survey : EntitiesBase
{
	public override long Id { get; set; }

	public string Title { get; set; } = null!;

	public string Description { get; set; } = null!;

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	[NotMapped] public string StartDate_Text { get; set; }
	[NotMapped] public string EndDate_Text { get; set; }
}
