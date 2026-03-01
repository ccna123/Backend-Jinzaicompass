using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SystemBrightSpotBE.Dtos.UserProject;

namespace SystemBrightSpotBE.PDFDocument
{
    public class SkillSheetDoc : IDocument
    {
        private readonly SkillSheetDto _data;

        public SkillSheetDoc(SkillSheetDto data)
        {
            _data = data;

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
                    col.Item().Text("従業員のスキルシート").Bold().FontSize(16).AlignLeft();
                    col.Item().PaddingBottom(40);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                            columns.ConstantColumn(70);
                            columns.RelativeColumn();
                            columns.ConstantColumn(70);
                            columns.RelativeColumn();
                        });

                        table.Cell().Element(x => CellFielder(x, hasBottomBorder: false).Padding(8)).Text("管理コード");
                        table.Cell().Element(x => CellContenter(x, hasBottomBorder: false).Padding(8)).Text(_data.jid);

                        table.Cell().Element(x => CellFielder(x, hasBottomBorder: false).Padding(8)).Text("年齢");
                        table.Cell().Element(x => CellContenter(x, hasBottomBorder: false).Padding(8)).Text(_data.age != 0 ? _data.age.ToString() : "");

                        table.Cell().Element(x => CellFielder(x, hasBottomBorder: false).Padding(8)).Text("性別");
                        table.Cell().Element(x => CellContenter(x, hasBottomBorder: false).Padding(8)).Text(_data.gender_name);
                    });

                    col.Item().PaddingBottom(40);

                    foreach (var project in _data.projects)
                    {
                        var date = $"{project.start_date:yyyy/MM/dd} → {(project.end_date != null ? project.end_date.Value.ToString("yyyy/MM/dd") : "現在")}";

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(150);
                                c.RelativeColumn();
                            });

                            table.Cell().ColumnSpan(2)
                                  .Background("#95B9C8")
                                  .Padding(8)
                                  .Text(date)
                                  .Bold();

                            FieldRow(table, "案件名", project.name);
                            FieldRow(table, "参画プロジェクト内容", project.content);
                            FieldRow(table, "経験職種", JoinName(project.experience_job));
                            FieldRow(table, "経験分野", JoinName(project.experience_field));
                            FieldRow(table, "経験領域", JoinName(project.experience_area));
                            FieldRow(table, "個別スキル", JoinName(project.specific_skill));
                            FieldRow(table, "参画ポジション", JoinName(project.participation_position));
                            FieldRow(table, "参画工程", JoinName(project.participation_process));
                            FieldRow(table, "自由記入", project.description ?? String.Empty);
                        });

                        col.Item().PaddingVertical(10);
                    }
                });
            });
        }
        private void FieldRow(TableDescriptor table, string label, string value, bool hasBottomBorder = true)
        {
            table.Cell().Element(container => CellFielder(container, hasBottomBorder)).Padding(8).ExtendHorizontal().Text(label);
            table.Cell().Element(container => CellContenter(container, hasBottomBorder)).Padding(8).ExtendHorizontal().Text(string.IsNullOrWhiteSpace(value) ? " " : value);

            table.Cell().ColumnSpan(2).Height(1f);
        }
        private string JoinName(IEnumerable<dynamic> list)
        {
            return list == null ? " " : string.Join("、", list.Select(x => x?.name ?? ""));
        }
        private static IContainer CellFielder(IContainer container, bool hasBottomBorder = true) =>
            container
                .MinHeight(20)
                .Background("#D2E1E7")
                .BorderBottom(hasBottomBorder ? 0.5f : 0)
                .BorderColor("#6C9CAF");

        private static IContainer CellContenter(IContainer container, bool hasBottomBorder = true) =>
            container
                .MinHeight(20)
                .Background("#F3F3F3")
                .BorderBottom(hasBottomBorder ? 0.5f : 0)
                .BorderColor("#864F4F");
    }
}
