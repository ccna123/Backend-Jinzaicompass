using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.ComponentModel.DataAnnotations;

namespace SystemBrightSpotBE.Attributes
{
    public class AllowedContentTypeAttribute : ValidationAttribute
    {
        private readonly string[] _allowedTypes = [];
        public int MaxSize { get; set; } = 5; // 5MB
        public int MaxWidth { get; set; } = 1000;
        public int MaxHeight { get; set; } = 1000;

        public AllowedContentTypeAttribute(string[] allowedTypes)
        {
            _allowedTypes = allowedTypes;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file == null)
            {
                return ValidationResult.Success;
            }

            // Check content type
            if (!_allowedTypes.Contains(file.ContentType))
            {
                var allowedList = string.Join("、", _allowedTypes);
                return new ValidationResult($"{allowedList} の画像形式でアップロードしてください。");
            }

            // Check max size
            long maxBytes = MaxSize * 1024 * 1024;
            if (file.Length > maxBytes)
            {
                return new ValidationResult($"アップロードされた画像が要件を満たしていません。");
            }

            // Check width, height image
            if (file.ContentType.StartsWith("image/") && file.ContentType != "image/svg+xml")
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    using var image = Image.Load<Rgba32>(stream);

                    if (image.Width > MaxWidth || image.Height > MaxHeight)
                    {
                        return new ValidationResult($"アップロードされた画像が要件を満たしていません。");
                    }
                }
                catch
                {
                    return new ValidationResult("アップロードされた画像が要件を満たしていません。");
                }
            }

            return ValidationResult.Success;
        }
    }
}
