using System.ComponentModel.DataAnnotations;

namespace Epson.Model.Users
{
    public partial class LoginModel
    {

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public bool DisplayCaptcha { get; set; }
        public bool IsActive { get; set; }
    }
}