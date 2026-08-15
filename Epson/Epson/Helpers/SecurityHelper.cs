using System.Security.Cryptography;
using System.Text;

namespace Epson.Helpers
{
    public class SecurityHelper
    {
        private static readonly char[] Base32Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        public static string GenerateSecretKey()
        {
            string key = Guid.NewGuid().ToString("N");

            return key;
        }

        public static string GenerateBase32Key()
        {
            int length = 32;
            StringBuilder builder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = RandomNumberGenerator.GetInt32(0, Base32Characters.Length);
                builder.Append(Base32Characters[index]);
            }

            return builder.ToString();
        }
    }
}
