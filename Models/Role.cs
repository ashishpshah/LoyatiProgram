using Microsoft.AspNetCore.Mvc.Rendering;
using Seed_Admin.Infra;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin
{
    public partial class Role : EntitiesBase
	{
		public override long Id { get; set; }

		public string Name { get; set; } = null!;

		public int? DisplayOrder { get; set; }

		public bool IsAdmin { get; set; }

		[NotMapped] public long SelectedRoleId { get; set; } = 0;
		[NotMapped] public List<SelectListItem> Menus { get; set; } = null;

	}


}