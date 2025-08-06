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

		public int? NoOfWrongPasswordAttempts { get; set; }

		public DateTime? NextChangePasswordDate { get; set; }

		public string? PersonName { get; set; }

		public string? CompanyName { get; set; }

		public string? Email { get; set; }

		public string? ContactNo { get; set; }

		public string? AadharNo { get; set; }

		public string? Gstno { get; set; }

		public string? Address { get; set; }

		public string? Village { get; set; }

		public long? CityId { get; set; }

		public long? DistrictId { get; set; }

		public long? StateId { get; set; }

		public long? CountryId { get; set; }

		public int? Pincode { get; set; }

		public decimal? LandSize { get; set; }

		public string? GeoLocation { get; set; }

		public string? PreferredLang { get; set; }


		//public string Fullname { get { return (!string.IsNullOrEmpty(First_Name) ? First_Name : "") + (!string.IsNullOrEmpty(Last_Name) ? " " + Last_Name : ""); } }


		[NotMapped] public string User_Role { get; set; }
		[NotMapped] public long User_Role_Id { get; set; }
		[NotMapped] public long RoleId { get; set; }
		[NotMapped] public bool IsPassword_Reset { get; set; }
		[NotMapped] public DateTime? Date { get; set; }
		[NotMapped] public string Date_Text { get; set; }
		[NotMapped] public string User_Id_Str { get; set; }
		[NotMapped] public string Role_Id_Str { get; set; }
	}
}
