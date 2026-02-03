namespace SystemBrightSpotBE.Services.S3Service
{
    public interface IS3Service
    {
        Task<string?> UploadFileAsync(IFormFile file, string folder = "avatar", int? width = 150);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<string?> UploadPdfAsync(byte[] pdfBytes, string folder, string fileName);
    }
}
