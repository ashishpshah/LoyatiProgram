

namespace Seed_Admin.Models
{
	public class Plant : EntitiesBase
	{
		public override long Id { get; set; }
		public string PlantCode { get; set; }
		public string PlantName { get; set; }
		public string AddressLine1 { get; set; }	
		public string AddressLine2 { get; set; }
		public long Country_Id { get; set; }
		public long State_Id { get; set; }
		public long District_Id { get; set; }
		public long City_Id { get; set; }
		public long Taluka_Id { get; set; }
		public string DistrictName { get; set; }
		public string VillageName { get; set; }
		public string TalukaName { get; set; }
		public string CountryName { get; set; }
		public string StateName { get; set; }
		public int PinCode { get; set; }


	}
}
