using Seed_Admin.Infra;

namespace Seed_Admin
{
	public partial class UserRole : EntitiesBase
	{
		public override long Id { get; set; }
		public string Name { get; set; }
	}
}
