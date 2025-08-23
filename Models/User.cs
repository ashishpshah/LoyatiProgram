using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin
{
	public class User : EntitiesBase
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public override long Id { get; set; }

		[NotMapped] public string UserId { get; set; }

		public string UserName { get; set; } = null!;

		public string Password { get; set; } = null!;

		public string? Email { get; set; }

		public string? ContactNo { get; set; }

		public string? PreferredLang { get; set; }

		public int? NoOfWrongPasswordAttempts { get; set; }

		public DateTime? NextChangePasswordDate { get; set; }

		public string? Department { get; set; }

		public string? Designation { get; set; }


		//public string Fullname { get { return (!string.IsNullOrEmpty(First_Name) ? First_Name : "") + (!string.IsNullOrEmpty(Last_Name) ? " " + Last_Name : ""); } }


		[NotMapped] public string Plant_Role { get; set; }
		[NotMapped] public long Default_Plant { get; set; }
		[NotMapped] public string User_Role { get; set; }
		[NotMapped] public long User_Role_Id { get; set; }
		[NotMapped] public long RoleId { get; set; }
		[NotMapped] public long PlantId { get; set; }
		[NotMapped] public bool IsPassword_Reset { get; set; }
		[NotMapped] public DateTime? Date { get; set; }
		[NotMapped] public string Date_Text { get; set; }

		[NotMapped] public List<UserRoleMapping> UserRoleMappings { get; set; }


	}
}
