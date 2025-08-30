namespace Seed_Admin
{
    public class Farmer : EntitiesBase
    {
        public override long Id { get; set; }
        public string User_ID { get; set; }
        public string Password { get; set; }
		public string? Email { get; set; }
		public string? ContactNo { get; set; }
		public string AadharNumber { get; set; }
        public string Land_Size { get; set; }     
        public long Country_Id { get; set; }
        public long State_Id { get; set; }
        public long District_Id { get; set; }
        public long Village_Id { get; set; }
        public long Taluka_Id { get; set; }
        public string DistrictName { get; set; }
        public string VillageName { get; set; }
        public string TalukaName { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string Preferred_Language { get; set; }
        public int PinCode { get; set; }
        public string Preferred_Language_TEXT { get; set; }
    }
}
