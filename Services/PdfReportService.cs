using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ventas.ViewModels;

namespace Ventas.Services;

public class PdfReportService
{
    public byte[] Generate(ReportResultViewModel model)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text(model.Company.BusinessName).Bold().FontSize(18);
                    if (!string.IsNullOrWhiteSpace(model.Company.TaxId))
                    {
                        column.Item().Text($"RNC/Cedula: {model.Company.TaxId}");
                    }
                    if (!string.IsNullOrWhiteSpace(model.Company.Address))
                    {
                        column.Item().Text(model.Company.Address);
                    }
                    column.Item().Text($"{model.Company.Phone} {model.Company.Email}".Trim());
                    column.Item().PaddingTop(8).Text(model.Title).Bold().FontSize(15);
                    column.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Fecha");
                        header.Cell().Element(CellStyle).Text("Principal");
                        header.Cell().Element(CellStyle).Text("Detalle");
                        header.Cell().Element(CellStyle).Text("Estado");
                        header.Cell().Element(CellStyle).AlignRight().Text("Monto");
                    });

                    foreach (var row in model.Rows)
                    {
                        table.Cell().Element(BodyStyle).Text(row.Date);
                        table.Cell().Element(BodyStyle).Text(row.Main);
                        table.Cell().Element(BodyStyle).Text(row.Secondary);
                        table.Cell().Element(BodyStyle).Text(row.Status);
                        table.Cell().Element(BodyStyle).AlignRight().Text(row.Amount.ToString("N2"));
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Pagina ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateInvoice(InvoicePdfViewModel model)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text(model.Company.BusinessName).Bold().FontSize(20);
                    if (!string.IsNullOrWhiteSpace(model.Company.TaxId))
                    {
                        column.Item().Text($"RNC/Cedula: {model.Company.TaxId}");
                    }
                    if (!string.IsNullOrWhiteSpace(model.Company.Address))
                    {
                        column.Item().Text(model.Company.Address);
                    }
                    column.Item().Text($"{model.Company.Phone} {model.Company.Email}".Trim());
                    column.Item().PaddingTop(10).Text($"Factura {model.Number}").Bold().FontSize(16);
                    column.Item().Text($"Fecha de creacion: {model.Date}");
                });

                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text($"Cliente: {model.Customer}");
                    if (!string.IsNullOrWhiteSpace(model.CustomerTaxId))
                    {
                        column.Item().Text($"RNC/Cedula: {model.CustomerTaxId}");
                    }
                    if (!string.IsNullOrWhiteSpace(model.CustomerAddress))
                    {
                        column.Item().Text($"Direccion: {model.CustomerAddress}");
                    }
                    column.Item().Text($"Metodo de pago: {model.PaymentMethod}");
                    column.Item().Text($"Estado: {model.Status}");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Producto");
                            header.Cell().Element(CellStyle).AlignRight().Text("Cant.");
                            header.Cell().Element(CellStyle).AlignRight().Text("Precio");
                            header.Cell().Element(CellStyle).AlignRight().Text("Total");
                        });

                        foreach (var item in model.Items)
                        {
                            table.Cell().Element(BodyStyle).Text(item.Product);
                            table.Cell().Element(BodyStyle).AlignRight().Text(item.Quantity.ToString("N2"));
                            table.Cell().Element(BodyStyle).AlignRight().Text(item.UnitPrice.ToString("N2"));
                            table.Cell().Element(BodyStyle).AlignRight().Text(item.Total.ToString("N2"));
                        }
                    });

                    column.Item().AlignRight().Text($"Total general: {model.Total:N2}").Bold().FontSize(14);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Documento generado con QuestPDF Community");
                });
            });
        }).GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
        => container.Background(Colors.Grey.Lighten2).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);

    private static IContainer BodyStyle(IContainer container)
        => container.PaddingVertical(4).PaddingHorizontal(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
}
