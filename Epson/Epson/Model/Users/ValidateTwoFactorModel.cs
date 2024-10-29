using System.ComponentModel.DataAnnotations;

namespace Epson.Model.Users
{
    public partial class ValidateTwoFactorModel
    {

        public string OTP { get; set; }
        public string email { get; set; }
    }
}