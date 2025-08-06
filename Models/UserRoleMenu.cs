using System.ComponentModel.DataAnnotations;

namespace Seed_Admin
{
	public class UserRoleMenu
	{
		public long BranchId { get; set; } = 0!;
		public long CompanyId { get; set; } = 0!;
		public string Name { get; set; } = null!;


		public long SelectedRoleId { get; set; } = 0!;
		//public List<SelectListItem> Menus { get; set; } = null!;
		public long Menu_Id { get; set; } = 0!;
		public long Role_Id { get; set; } = 0!;
		public string Menu_Name { get; set; }="";
		public bool IsAddAllMenu { get; set; }
		public string Isselected { get; set; }
	}
}
