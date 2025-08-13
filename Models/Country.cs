using Seed_Admin.Infra;

namespace Seed_Admin.Models
{
	public partial class Country : EntitiesBase
	{
		public override long Id {  get; set; }
		public string CountryName { get; set; }

	}
}
