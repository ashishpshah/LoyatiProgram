using Seed_Admin.Infra;

namespace Seed_Admin.Models
{
	public partial class State : EntitiesBase
	{
		public override long Id { get; set; }
		public long CountryId {  get; set; }
		public string StateName { get; set; }
		

	}
}
