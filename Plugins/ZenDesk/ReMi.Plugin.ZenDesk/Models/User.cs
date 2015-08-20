namespace ReMi.Plugin.ZenDesk.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public bool Active { get; set; }
        public string Locale { get; set; }
        public int? OrganizationId { get; set; }
    }
}
