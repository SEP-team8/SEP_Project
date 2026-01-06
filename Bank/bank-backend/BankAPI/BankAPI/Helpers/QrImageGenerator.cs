using QRCoder;

namespace BankAPI.Helpers
{
    public static class QrImageGenerator
    {
        public static string GenerateBase64(string payload)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M, forceUtf8: true, utf8BOM: false);
            using var qrCode = new PngByteQRCode(qrData);

            var bytes = qrCode.GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
