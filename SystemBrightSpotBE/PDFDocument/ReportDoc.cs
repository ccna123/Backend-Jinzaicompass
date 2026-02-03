using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SystemBrightSpotBE.Dtos.Report;

namespace SystemBrightSpotBE.PDFDocument
{
    public class ReportDoc : IDocument
    {
        private readonly ReportPDFDto _data;
        private readonly byte[]? _htmlContent;

        public ReportDoc(ReportPDFDto data, byte[]? htmlContent = null)
        {
            _data = data;
            _htmlContent = htmlContent;

            var fontPath = Path.Combine(AppContext.BaseDirectory, "PDFDocument/Fonts");
            FontManager.RegisterFont(File.OpenRead(Path.Combine(fontPath, "NotoSansJP-Bold.ttf")));
            FontManager.RegisterFont(File.OpenRead(Path.Combine(fontPath, "NotoSansJP-Medium.ttf")));
            FontManager.RegisterFont(File.OpenRead(Path.Combine(fontPath, "NotoSansJP-Regular.ttf")));
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontFamily("Noto Sans JP").FontSize(11));

                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);
                            columns.RelativeColumn();
                            columns.ConstantColumn(70);
                            columns.RelativeColumn();
                            columns.ConstantColumn(70);
                            columns.RelativeColumn();
                        });

                        table.Cell().Element(x => CellFielder(x, hasBottomBorder: false).Padding(8)).Text("起票者");
                        table.Cell().Element(x => CellContenter(x, hasBottomBorder: false).Padding(8)).Text(_data.user_fullname);

                        table.Cell().Element(x => CellFielder(x, hasBottomBorder: false).Padding(8)).Text("起票日");
                        table.Cell().Element(x => CellContenter(x, hasBottomBorder: false).Padding(8)).Text(_data.date.ToString("yyyy/MM/dd"));

                        table.Cell().Element(x => CellFielder(x, hasBottomBorder: false).Padding(8)).Text("タイプ");
                        table.Cell().Element(x => CellContenter(x, hasBottomBorder: false).Padding(8)).Text(_data.report_type_name);
                    });

                    col.Item().PaddingBottom(5);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);
                            columns.RelativeColumn();
                            columns.ConstantColumn(70);
                            columns.RelativeColumn();
                            columns.ConstantColumn(70);
                            columns.RelativeColumn();
                        });

                        table.Cell().Element(x => CellFielder(x, hasBottomBorder: false).Padding(8)).Text("閲覧者");
                        table.Cell().ColumnSpan(5).Element(x => CellContenter(x, hasBottomBorder: false).Padding(8)).Text(_data.target_all);
                    });

                    col.Item().PaddingBottom(20);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                        });

                        table.Cell().Element(x => CellFielder(x, hasBottomBorder: false).Padding(8)).Text("内容");

                        if (_htmlContent != null && _htmlContent.Length > 0)
                        {
                            var images = SplitImage(_htmlContent);

                            foreach (var img in images)
                            {
                                table.Cell().Element(x => CellContenter(x, false).Padding(4)).Image(img);
                            }
                        }
                    });
                });
            });
        }


        private static IContainer CellFielder(IContainer container, bool hasBottomBorder = true) =>
            container
                .MinHeight(20)
                .Background("#D2E1E7")
                .BorderBottom(hasBottomBorder ? 0.5f : 0)
                .BorderColor("#6C9CAF")
                .ShowOnce();

        private static IContainer CellContenter(IContainer container, bool hasBottomBorder = true) =>
            container
                .MinHeight(20)
                .Background("#F3F3F3")
                .BorderBottom(hasBottomBorder ? 0.5f : 0)
                .BorderColor("#864F4F")
                .ShowOnce();


        public static List<byte[]> SplitImage(byte[] imageData)
        {
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageData);

            float dpiY = image.Metadata.VerticalResolution > 0 ? (float)image.Metadata.VerticalResolution : 96f;

            // Header height
            float headerHeightPt = 105f;
            int headerHeightPx = (int)Math.Round(headerHeightPt / 72f * dpiY);

            // A4 usable height (842pt - 30pt margin bottom)
            int usablePageHeightPx = (int)Math.Round((842f - 30f) / 72f * dpiY);

            var result = new List<byte[]>();
            int y = 0;
            int pageIndex = 0;

            while (y < image.Height)
            {
                int maxHeight = (pageIndex == 0) ? (usablePageHeightPx - headerHeightPx) : usablePageHeightPx;
                int sliceHeight = Math.Min(maxHeight, image.Height - y);

                using var slice = image.Clone(ctx =>
                    ctx.Crop(new SixLabors.ImageSharp.Rectangle(0, y, image.Width, sliceHeight))
                );

                using var ms = new MemoryStream();
                slice.Save(ms, new PngEncoder());
                result.Add(ms.ToArray());

                y += sliceHeight;
                pageIndex++;
            }

            return result;
        }
    }
}
