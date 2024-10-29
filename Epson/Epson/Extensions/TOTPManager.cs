using OtpNet;
using System.Runtime.InteropServices;
using ZXing;
using ZXing.Common;
using System;
using System.Drawing;
using System.Linq;
using System.Drawing.Imaging;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;

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

        private Bitmap GenerateQrCode(string provisioningUri)
        {
            QrCodeEncodingOptions options = new()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 500,
                Height = 500
            };

            BarcodeWriter writer = new()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options
            };

            Bitmap qrcode = writer.Write(provisioningUri);
            return qrcode;
        }

        private string GenerateQRString(string provisioningUri)
        {
            QrCodeEncodingOptions options = new()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 500,
                Height = 500
            };

            BarcodeWriter writer = new()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = options
            };

            Bitmap qrcode = writer.Write(provisioningUri);
            using (MemoryStream ms = new MemoryStream())
            {
                qrcode.Save(ms, ImageFormat.Png);
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
