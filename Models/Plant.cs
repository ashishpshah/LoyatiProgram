

using System.ComponentModel.DataAnnotations.Schema;

namespace Seed_Admin.Models
{
	public class Plant : EntitiesBase
	{
		public override long Id { get; set; }

		public string PlantCode { get; set; } = null!;

		public string PlantName { get; set; } = null!;

		public string? AddressLine1 { get; set; }

		public string? AddressLine2 { get; set; }

		public long Country_Id { get; set; }

		public long State_Id { get; set; }

		public long District_Id { get; set; }

		public long Taluka_Id { get; set; }

		public long City_Id { get; set; }

		public int? PinCode { get; set; }

		[NotMapped] public string DistrictName { get; set; }
		[NotMapped] public string VillageName { get; set; }
		[NotMapped] public string TalukaName { get; set; }
		[NotMapped] public string CountryName { get; set; }
		[NotMapped] public string StateName { get; set; }


	}
}
