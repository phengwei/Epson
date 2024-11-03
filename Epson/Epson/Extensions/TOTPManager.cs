using OtpNet;
using QRCoder;
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using ZXing.QrCode;
using ZXing;

namespace Epson.Extensions
{
    public class TOTPManager
    {
        private readonly string _issuer;
        private readonly string _accountName;
        private readonly byte[] _secretKey;

        public TOTPManager(string issuer, string accountName, string secretKey)
        {
            _issuer = issuer;
            _accountName = accountName;
            _secretKey = Base32Encoding.ToBytes(secretKey);
        }

        public string GenerateTotpCode()
        {
            var totp = new Totp(_secretKey);
            return totp.ComputeTotp();
        }

        public string GetTotpProvisioningUri()
        {
            var totpUrl = $"otpauth://totp/{Uri.EscapeDataString(_issuer)}:{Uri.EscapeDataString(_accountName)}?secret={Base32Encoding.ToString(_secretKey)}&issuer={Uri.EscapeDataString(_issuer)}";
            return totpUrl;
        }

        public string GenerateQRString(string provisioningUri)
        {
            var qrWriter = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 500,
                    Width = 500,
                    Margin = 0
                }
            };

            var pixelData = qrWriter.Write(provisioningUri);

            using (var image = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height))
            using (var ms = new MemoryStream())
            {
                image.Save(ms, new PngEncoder());
                byte[] qrCodeBytes = ms.ToArray();
                return Convert.ToBase64String(qrCodeBytes);
            }
        }

        public string GenerateGoogleOTP()
        {
            string totpUrl = $"otpauth://totp/{Uri.EscapeDataString(_accountName)}?secret={Base32Encoding.ToString(_secretKey)}&issuer={Uri.EscapeDataString(_issuer)}";
            return GenerateQRString(totpUrl);
        }

        public string GenerateMSOTP()
        {
            string totpUrl = $"otpauth://totp/{Uri.EscapeDataString(_issuer)}:{Uri.EscapeDataString(_accountName)}?secret={Uri.EscapeDataString(Base32Encoding.ToString(_secretKey))}&issuer={Uri.EscapeDataString(_issuer)}";
            return GenerateQRString(totpUrl);
        }

        public bool Validate(string code)
        {
            var validator = new Totp(_secretKey);
            return validator.VerifyTotp(code, out long timeStepMatched);
        }
    }
}
