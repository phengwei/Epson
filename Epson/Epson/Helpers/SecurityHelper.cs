using System.Text;

namespace Epson.Helpers
{
    public class SecurityHelper
    {
        private static readonly char[] Base32Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
        private static readonly Random Random = new Random();

        public static string GenerateSecretKey()
        {
            string key = Guid.NewGuid().ToString("N");

            return key;
        }

        public static string GenerateBase32Key()
        {
            Random random = new Random();

            int length = 32;
            StringBuilder builder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(0, Base32Characters.Length - 1);
                builder.Append(Base32Characters[index]);
            }

            return builder.ToString();
        }
    }
}
