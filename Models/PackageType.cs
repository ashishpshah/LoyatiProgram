namespace Seed_Admin.Models
{
	public class PackageType : EntitiesBase
	{
		public override long Id { get; set; }
		public string PackageTypeName { get; set; }
		public string Description { get; set; }
	}
}
