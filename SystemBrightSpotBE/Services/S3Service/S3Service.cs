using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System.Xml.Linq;

namespace SystemBrightSpotBE.Services.S3Service
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly string _region;
        private readonly string _bucketName;
        private readonly string _bucketNameforPDF;

        public S3Service(
            IConfiguration configuration
        ) {
            
            _configuration = configuration;

            _region = Environment.GetEnvironmentVariable("AWS_REGION")
                ?? _configuration["AWS:Region"]
                ?? "ap-northeast-1";
            _bucketName = _configuration.GetSection("AWS:BucketName").Value ?? String.Empty;
            _bucketNameforPDF = _configuration.GetSection("AWS:BucketNameforPDF").Value ?? String.Empty;
            // Get access key and secret key
            string accessKey = _configuration.GetSection("AWS:AccessKey").Value ?? String.Empty;
            string secretKey = _configuration.GetSection("AWS:SecretKey").Value ?? String.Empty;

            if (!String.IsNullOrEmpty(accessKey) && !String.IsNullOrEmpty(secretKey))
            {
                // use credentials
               _s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(_region));
            }
            else
            {
                // use IAM Role
                _s3Client = new AmazonS3Client();
            }
        }

        /// <summary>
        /// Upload file on S3 and return url
        /// </summary>
        public async Task<string?> UploadFileAsync(IFormFile file, string folder, int? width)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fileKey = $"app/images/bright-spot/{folder}/{fileName}";

            if (ext == ".svg")
            {
                // --- Directly resize SVG ---
                string svgText;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    svgText = await reader.ReadToEndAsync();
                }

                var doc = XDocument.Parse(svgText);
                var svgElement = doc.Root;

                if (svgElement != null && svgElement.Name.LocalName == "svg")
                {
                    // Remove resize if width == null
                    if (width.HasValue)
                    {
                        // Get original width/height or viewBox
                        double originalWidth = 0, originalHeight = 0;
                        var widthAttr = svgElement.Attribute("width");
                        var heightAttr = svgElement.Attribute("height");
                        var viewBoxAttr = svgElement.Attribute("viewBox");

                        if (widthAttr != null && heightAttr != null &&
                            double.TryParse(widthAttr.Value.Replace("px", ""), out originalWidth) &&
                            double.TryParse(heightAttr.Value.Replace("px", ""), out originalHeight))
                        {
                            double ratio = (double)width / originalWidth;
                            double newHeight = originalHeight * ratio;
                            svgElement.SetAttributeValue("width", width);
                            svgElement.SetAttributeValue("height", newHeight);
                        }
                        else if (viewBoxAttr != null)
                        {
                            var parts = viewBoxAttr.Value.Split(' ');
                            if (parts.Length == 4 &&
                                double.TryParse(parts[2], out originalWidth) &&
                                double.TryParse(parts[3], out originalHeight))
                            {
                                double ratio = (double)width / originalWidth;
                                double newHeight = originalHeight * ratio;
                                svgElement.SetAttributeValue("width", width);
                                svgElement.SetAttributeValue("height", newHeight);
                            }
                        }
                    }
                }

                // Upload resized SVG to S3
                using var ms = new MemoryStream();
                using (var writer = new StreamWriter(ms))
                {
                    doc.Save(writer);
                    writer.Flush();
                    ms.Position = 0;

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = ms,
                        Key = fileKey,
                        BucketName = _bucketName,
                        ContentType = "image/svg+xml"
                    };

                    var transferUtility = new TransferUtility(_s3Client);
                    await transferUtility.UploadAsync(uploadRequest);
                }
            }
            else
            {
                // --- Bitmap Processing with ImageSharp ---
                using (var stream = file.OpenReadStream())
                {
                    using var image = Image.Load(stream);
                    // Remove resize if width == null
                    if (width.HasValue)
                    {
                        image.Mutate(x => x.Resize(width.Value, 0));
                    }

                    using var ms = new MemoryStream();
                    await image.SaveAsync(ms, image.DetectEncoder(ext));
                    ms.Position = 0;

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = ms,
                        Key = fileKey,
                        BucketName = _bucketName,
                        ContentType = file.ContentType
                    };

                    var transferUtility = new TransferUtility(_s3Client);
                    await transferUtility.UploadAsync(uploadRequest);
                }
            }

            // return URL
            string fileUrl = $"https://{_bucketName}.s3.{_region}.amazonaws.com/{fileKey}";
            return fileUrl;
        }

        /// <summary>
        /// Delete file
        /// </summary>
        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return false;

            // get filename from url
            var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            var response = await _s3Client.DeleteObjectAsync(deleteRequest);
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public async Task<string?> UploadPdfAsync(byte[] pdfBytes, string folder, string fileName)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
                return null;

            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                fileName += ".pdf";

            string fileKey = $"{folder}/{Guid.NewGuid()}_{fileName}";

            try
            {
                // Upload PDF in S3
                using (var ms = new MemoryStream(pdfBytes))
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = ms,
                        Key = fileKey,
                        BucketName = _bucketNameforPDF,
                        ContentType = "application/pdf",
                        CannedACL = S3CannedACL.PublicRead // file public
                    };

                    var transferUtility = new TransferUtility(_s3Client);
                    await transferUtility.UploadAsync(uploadRequest);
                }

                string fileUrl = $"https://{_bucketNameforPDF}.s3.{_region}.amazonaws.com/{fileKey}";
                return fileUrl;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
