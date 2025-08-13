using Seed_Admin.Infra;

namespace Seed_Admin.Models
{
	public partial class District : EntitiesBase
	{
		public override long Id { get; set; }
		public long StateId { get; set; }
		public string Name { get; set; }
		public long CountryId { get; set; }


	}
}
