using System.ComponentModel.DataAnnotations;

namespace Epson.Model.Users
{
    public class RegisterModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool EnteringEmailTwice { get; set; }
        [DataType(DataType.EmailAddress)]
        public string ConfirmEmail { get; set; }

        public bool UsernamesEnabled { get; set; }
        public string Username { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public bool PhoneRequired { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
    }
}
