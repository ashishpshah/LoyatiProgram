using Seed_Admin.Infra;

namespace Seed_Admin.Models
{
	public partial class City : EntitiesBase
	{
		public override long Id { get; set; }
		public long StateId { get; set; }
		public string Name { get; set; }
		public long CountryId { get; set; }
		public long DistrictId { get; set; }
		public long TalukaId { get; set; }

	}
}
