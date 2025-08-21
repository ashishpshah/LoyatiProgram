namespace Seed_Admin
{
    public class ChangePassword : EntitiesBase
    {
        public override long Id { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
