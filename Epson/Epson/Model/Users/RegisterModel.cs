using System.ComponentModel.DataAnnotations;

namespace Epson.Model.Users
{
    public class RegisterModel
    {
        public string? Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public int TeamId { get; set; }
        public List<string> Roles { get; set; }
    }
}
