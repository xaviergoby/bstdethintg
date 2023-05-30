using Hodl.Api.ViewModels.ReportModels;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Hodl.Api.Utils.PdfReporting;

public class PdfReportDocument : IDocument
{
    private const string PAGE_HEADER_BACKGROUND = "#669771";
    private const float PAGE_HEADER_PADDING = 5.0F;
    private const string FOOTER_TEXT_COLOR = "#AFC9B5";

    private const int FONT_SIZE_TITLE = 26;
    private const int FONT_SIZE_NORMAL = 11;
    private const int FONT_SIZE_SECTION_HEADER = 16;
    private const int FONT_SIZE_TABLE_HEADER = 11;
    private const int FONT_SIZE_TABLE_CELL = 9;

    private const float VERTICAL_PADDING = 5.0F;

    private const float TABLE_HEADER_BORDER = 0.3F;
    private const string TABLE_HEADER_BORDER_COLOR = Colors.Grey.Lighten3;
    private const string TABLE_HEADER_BACKGROUND_COLOR = "#AFC9B5";

    private const float TABLE_CELL_PADDING_VERT = 1.5F;
    private const float TABLE_CELL_PADDING_HORZ = 5;
    private const float TABLE_CELL_BORDER = 0.3F;
    private const string TABLE_CELL_BORDER_COLOR = Colors.Grey.Lighten1;

    private readonly TextStyle _titleStyle = TextStyle.Default.FontSize(FONT_SIZE_TITLE).SemiBold().FontColor(Colors.White);
    private readonly TextStyle _pageHeaderTextStyle = TextStyle.Default.FontSize(FONT_SIZE_SECTION_HEADER).FontColor(Colors.White).Bold();
    private readonly TextStyle _pageHeaderSmallTextStyle = TextStyle.Default.FontSize(FONT_SIZE_NORMAL).FontColor(Colors.White);
    private readonly TextStyle _footerTextStyle = TextStyle.Default.FontSize(FONT_SIZE_NORMAL).FontColor(FOOTER_TEXT_COLOR);

    private readonly TextStyle _defaultTextStyle = TextStyle.Default.FontSize(FONT_SIZE_NORMAL).FontColor(Colors.Black);
    private readonly TextStyle _headerTextStyle = TextStyle.Default.FontSize(FONT_SIZE_SECTION_HEADER).FontColor(Colors.Black).Bold();

    private readonly TransactionType[] sharesTransactionTypes = new TransactionType[2] { TransactionType.Inflow, TransactionType.Outflow };

    private readonly ReportFundViewModel _reportData;
    private readonly CultureInfo _cultureInfo;

    public PdfReportDocument(ReportFundViewModel reportData, CultureInfo cultureInfo)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        _reportData = reportData;
        _cultureInfo = cultureInfo;
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"{_reportData.FundName} report bookingperiod {_reportData.Nav.BookingPeriod}",
        Author = _reportData.FundOwner.ToString(),
        Creator = "Hodl Tradingdesk"
    };

    /// <summary>
    /// This method is responsible of "composing" each "container" element of the pdf report doc
    /// </summary>
    /// <param name="container"></param>
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Portrait());
            page.MarginHorizontal(30);
            page.MarginVertical(40);

            page.DefaultTextStyle(_defaultTextStyle);
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    /// <summary>
    /// This method deals with the "composition" of the header in the pdf report doc.
    /// The header simply comtains the name of the specific fund & booking period in question.
    /// </summary>
    /// <param name="container"></param>
    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            // Title header
            column.Item().Background(PAGE_HEADER_BACKGROUND).Padding(PAGE_HEADER_PADDING).Row(row =>
            {
                row.RelativeItem().AlignLeft().Text(text =>
                {
                    text.Span(_reportData.FundName.ToUpper()).Style(_titleStyle);
                    text.Span(" period report").Style(_pageHeaderTextStyle);
                });
                row.RelativeItem().AlignRight().AlignBottom().Text(text =>
                {
                    text.Span($"Booking period: ").Style(_pageHeaderSmallTextStyle);
                    text.Span(_reportData.Nav.BookingPeriod).Style(_pageHeaderTextStyle);
                });
            });

            // Description and owner overview section, show only once
            column.Item().ShowOnce().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    // Label-Value columns
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(5);
                });
                table.Cell().PaddingVertical(VERTICAL_PADDING);

                table.Cell().Row(1).Column(1).Text("Description:");
                table.Cell().Row(1).Column(2).Text(_reportData.Description);
                table.Cell().Row(2).Column(1).Text("Owner:");
                table.Cell().Row(2).Column(2).Text(_reportData.FundOwner.Name);
                table.Cell().Row(3).Column(1).Text("Country:");
                table.Cell().Row(3).Column(2).Text(_reportData.FundOwner.Country);
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container
            .AlignCenter()
            .Text(text =>
            {
                text.DefaultTextStyle(_footerTextStyle);
                text.Span($"Page ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
    }

    /// <summary>
    /// This method deals with the "composition" of the pdf report doc's main content section.
    /// This contains the following "sub"-sections:
    /// Owner Overview
    /// NAW Overview
    /// Layer Overview
    /// Holdings Overview
    /// Trades Overview
    /// Transfer Log Overview
    /// Trading Log Overview
    /// </summary>
    /// <param name="container"></param>
    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(5).Column(column =>
        {
            // NAV overview section
            AddSectionHeader(column, "NAV Overview");
            column.Item().Element(ComposeNAVOverview);

            // Layers overview section
            AddSectionHeader(column, "Layers Overview");
            column.Item().Element(ComposeLayersTable);
            column.Item().PageBreak();

            // Holdings overview section
            AddSectionHeader(column, "Holdings Overview");
            column.Item().Element(ComposeHoldingsTable);
            column.Item().PageBreak();

            // Trades (summary) overview section
            AddSectionHeader(column, "Trades Overview");
            column.Item().Element(ComposeSummaryTable);
            column.Item().PageBreak();

            // Transfer Log section
            AddSectionHeader(column, "Transfer Log Overview");
            column.Item().Element(ComposeTransferLogTable);
            column.Item().PageBreak();

            //Trading Log section
            AddSectionHeader(column, "Trading Log Overview");
            column.Item().Element(ComposeTradingLogTable);
        });
    }

    private void AddSectionHeader(ColumnDescriptor column, string text)
    {
        column.Item()
            .PaddingTop(5)
            .BorderBottom(1)
            .BorderColor(Colors.Black)
            .DefaultTextStyle(_headerTextStyle)
            .Text(text);
    }

    private static IContainer TableHeaderStyle(IContainer container) => container
        .DefaultTextStyle(x => x.FontSize(FONT_SIZE_TABLE_HEADER).SemiBold())
        .BorderHorizontal(TABLE_CELL_BORDER)
        .BorderVertical(TABLE_HEADER_BORDER)
        .BorderColor(TABLE_HEADER_BORDER_COLOR)
        .Background(TABLE_HEADER_BACKGROUND_COLOR)
        //.PaddingVertical(TABLE_CELL_PADDING_VERT)
        .PaddingHorizontal(TABLE_CELL_PADDING_HORZ)
        .AlignMiddle()
        .AlignCenter();

    private static IContainer TableCellStyleLeft(IContainer container) => container
        .DefaultTextStyle(x => x.FontSize(FONT_SIZE_TABLE_CELL))
        .BorderHorizontal(TABLE_CELL_BORDER)
        .BorderVertical(TABLE_CELL_BORDER)
        .BorderColor(TABLE_CELL_BORDER_COLOR)
        //.PaddingVertical(TABLE_CELL_PADDING_VERT)
        .PaddingHorizontal(TABLE_CELL_PADDING_HORZ)
        .AlignMiddle()
        .AlignLeft();

    private static IContainer TableCellStyleCenter(IContainer container) => container
        .DefaultTextStyle(x => x.FontSize(FONT_SIZE_TABLE_CELL))
        .BorderHorizontal(TABLE_CELL_BORDER)
        .BorderVertical(TABLE_CELL_BORDER)
        .BorderColor(TABLE_CELL_BORDER_COLOR)
        //.PaddingVertical(TABLE_CELL_PADDING_VERT)
        .PaddingHorizontal(TABLE_CELL_PADDING_HORZ)
        .AlignMiddle()
        .AlignCenter();

    private static IContainer TableCellStyleRight(IContainer container) => container
        .DefaultTextStyle(x => x
            .FontSize(FONT_SIZE_TABLE_CELL))
        .BorderHorizontal(TABLE_CELL_BORDER)
        .BorderVertical(TABLE_CELL_BORDER)
        .BorderColor(TABLE_CELL_BORDER_COLOR)
        //.PaddingVertical(TABLE_CELL_PADDING_VERT)
        .PaddingHorizontal(TABLE_CELL_PADDING_HORZ)
        .AlignMiddle()
        .AlignRight();

    private static IContainer TableSummaryStyle(IContainer container) => container
        .DefaultTextStyle(x => x
            .FontSize(FONT_SIZE_TABLE_CELL)
            .SemiBold())
        .BorderHorizontal(TABLE_CELL_BORDER)
        .BorderVertical(TABLE_CELL_BORDER)
        .BorderColor(TABLE_CELL_BORDER_COLOR)
        .PaddingVertical(TABLE_CELL_PADDING_VERT)
        .PaddingHorizontal(TABLE_CELL_PADDING_HORZ)
        .AlignMiddle()
        .AlignCenter();

    // The style for the nested header cells and row content cells
    private static IContainer NestedHeaderCellStyle(IContainer container) => container
        .DefaultTextStyle(x => x.FontSize(FONT_SIZE_TABLE_CELL).SemiBold())
        .BorderVertical(TABLE_HEADER_BORDER)
        .BorderColor(TABLE_HEADER_BORDER_COLOR)
        .Background(TABLE_HEADER_BACKGROUND_COLOR)
        //.PaddingVertical(TABLE_CELL_PADDING_VERT)
        .PaddingHorizontal(TABLE_CELL_PADDING_HORZ)
        .AlignMiddle()
        .AlignCenter();

    private static IContainer NestedCellStyleCenter(IContainer container) => container
        .DefaultTextStyle(x => x
            .FontSize(FONT_SIZE_TABLE_CELL))
        .BorderHorizontal(TABLE_CELL_BORDER)
        .BorderVertical(TABLE_CELL_BORDER)
        .BorderColor(TABLE_CELL_BORDER_COLOR)
        //.PaddingVertical(TABLE_CELL_PADDING_VERT)
        .PaddingHorizontal(TABLE_CELL_PADDING_HORZ)
        .AlignMiddle()
        .AlignCenter();

    private static IContainer NestedCellStyleRight(IContainer container) => container
        .DefaultTextStyle(x => x
            .FontSize(FONT_SIZE_TABLE_CELL))
        .BorderHorizontal(TABLE_CELL_BORDER)
        .BorderVertical(TABLE_CELL_BORDER)
        .BorderColor(TABLE_CELL_BORDER_COLOR)
        //.PaddingVertical(TABLE_CELL_PADDING_VERT)
        .PaddingHorizontal(TABLE_CELL_PADDING_HORZ)
        .AlignMiddle()
        .AlignRight();

    private void ComposeNAVOverview(IContainer container)
    {
        container.PaddingVertical(VERTICAL_PADDING).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                // Nav Overview cols 1 & 2
                columns.ConstantColumn(65);
                columns.RelativeColumn(2);
                // Spacing col 3
                columns.RelativeColumn(1);
                // Nav Overview cols 4 & 5 & 6
                columns.ConstantColumn(77);
                columns.RelativeColumn(2);
                columns.ConstantColumn(26);

                // Spacing col 7
                columns.RelativeColumn(1);
                // Nav Overview cols 8 & 9 & 10
                columns.ConstantColumn(110);
                columns.RelativeColumn(2);
                columns.ConstantColumn(26);
            });
            table.Cell().PaddingVertical(VERTICAL_PADDING);

            table.Cell().Row(1).Column(1).Text("Gross NAV:");
            table.Cell().Row(1).Column(2).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(_reportData.Nav.ShareGross, _cultureInfo)).Bold();
            table.Cell().Row(2).Column(1).Text("Net NAV:");
            table.Cell().Row(2).Column(2).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(_reportData.Nav.ShareNAV, _cultureInfo)).Bold();
            table.Cell().Row(3).Column(1).Text("HWM:");
            table.Cell().Row(3).Column(2).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(_reportData.Nav.ShareHWM, _cultureInfo)).Bold();
            table.Cell().Row(5).Column(1).Text($"{_reportData.ReportingCurrencyCode}/USD:");
            var holding = _reportData.Holdings.FirstOrDefault(h => h.CurrencySymbol.Equals(_reportData.ReportingCurrencyCode));
            if (holding != null)
                table.Cell().Row(5).Column(2).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(holding.EndUSDPrice, _cultureInfo)).Bold();

            table.Cell().Row(1).Column(4).Text($"Total Value:");
            table.Cell().Row(1).Column(5).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(_reportData.Nav.TotalValue, _cultureInfo)).Bold();
            table.Cell().Row(1).Column(6).AlignRight().Text(_reportData.ReportingCurrencyCode);
            table.Cell().Row(2).Column(4).Text("Total Shares:");
            table.Cell().Row(2).Column(5).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(_reportData.Nav.TotalShares, _cultureInfo)).Bold();
            table.Cell().Row(3).Column(4).Text($"In-Out Value:");
            table.Cell().Row(3).Column(5).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(_reportData.Nav.InOutValue, _cultureInfo)).Bold();
            table.Cell().Row(3).Column(6).AlignRight().Text(_reportData.ReportingCurrencyCode);
            table.Cell().Row(4).Column(4).Text("In-Out Shares:");
            table.Cell().Row(4).Column(5).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(_reportData.Nav.InOutShares, _cultureInfo)).Bold();

            table.Cell().Row(1).Column(8).Text($"Administration Fee:");
            table.Cell().Row(1).Column(9).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(_reportData.Nav.AdministrationFee, _cultureInfo)).Bold();
            table.Cell().Row(1).Column(10).AlignRight().Text(_reportData.ReportingCurrencyCode);
            table.Cell().Row(2).Column(8).Text($"Performance Fee:");
            table.Cell().Row(2).Column(9).AlignRight().Text(PdfReportingUtil.RoundDisplayValue(_reportData.Nav.PerformanceFee, _cultureInfo)).Bold();
            table.Cell().Row(2).Column(10).AlignRight().Text(_reportData.ReportingCurrencyCode);

            table.Cell().Row(5).Column(8).Text($"Generated at:");
            table.Cell().Row(5).Column(9).ColumnSpan(2).AlignRight().Text(_reportData.Nav.DateTime.ToString(_cultureInfo)).Bold();
        });
    }

    /// <summary>
    /// LAYERS OVERVIEW TABLE
    /// This method is meant for dealing with the table structure containing the risk layers
    /// for a specific fund & booking period.
    /// </summary>
    /// <param name="container"></param>
    private void ComposeLayersTable(IContainer container)
    {
        container.PaddingVertical(VERTICAL_PADDING).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                // layer index
                columns.RelativeColumn(1);
                // layer name
                columns.RelativeColumn(2);
                // number of holdings
                columns.RelativeColumn(1);
                // current percentage
                columns.RelativeColumn(1);
                // aim percentage
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Element(TableHeaderStyle).Text("Layer Index");
                header.Cell().Element(TableHeaderStyle).Text("Layer Name");
                header.Cell().Element(TableHeaderStyle).Text("Number of Holdings");
                header.Cell().Element(TableHeaderStyle).Text("Current %");
                header.Cell().Element(TableHeaderStyle).Text("Aim %");
            });

            foreach (var item in _reportData.Layers)
            {
                table.Cell().AlignCenter().Text(item.LayerIndex.ToString());
                table.Cell().AlignCenter().Text(item.Name);
                table.Cell().AlignCenter().Text(item.NumberOfHoldings.ToString());
                table.Cell().AlignCenter().Text(PdfReportingUtil.RoundPercentValue(item.CurrentPercentage, 0, _cultureInfo));
                table.Cell().AlignCenter().Text(PdfReportingUtil.RoundPercentValue(item.AimPercentage, 0, _cultureInfo));
            }
        });
    }

    /// <summary>
    /// HOLDINGS OVERVIEW TABLE
    /// This method is meant for dealing with the table structure containing the holdings
    /// for a specific fund & booking period.
    /// </summary>
    /// <param name="container"></param>
    private void ComposeHoldingsTable(IContainer container)
    {
        container.PaddingVertical(VERTICAL_PADDING).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                // Column for currency symbol
                columns.ConstantColumn(52);
                // USD Price
                columns.RelativeColumn(3);
                // Start Balance
                columns.RelativeColumn(3);
                // NAV Balance
                columns.RelativeColumn(3);
                // NAV USD Value
                columns.RelativeColumn(3);
                // End Balance
                columns.RelativeColumn(3);
                // USD Value
                columns.RelativeColumn(3);
                // Percentage
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(TableHeaderStyle).Text("Asset");
                header.Cell().Element(TableHeaderStyle).Text("USD Price");
                header.Cell().Element(TableHeaderStyle).Text("Start Balance");
                header.Cell().Element(TableHeaderStyle).Text("NAV Balance");
                header.Cell().Element(TableHeaderStyle).Text("NAV USD Value");
                header.Cell().Element(TableHeaderStyle).Text("End Balance");
                header.Cell().Element(TableHeaderStyle).Text("USD Value");
                header.Cell().Element(TableHeaderStyle).Text("%");
            });

            decimal totalNAVUSDValue = 0;
            decimal totalUSDValue = 0;
            decimal totalEndReportingCurrencyValue = 0;
            foreach (var item in _reportData.Holdings)
            {
                if (AllNullHoldingsRowCheck(item)) continue;

                table.Cell().Element(TableCellStyleLeft).Text(item.CurrencySymbol);
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.EndUSDPrice, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.StartBalance, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.NavBalance, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.NavUSDValue, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.EndBalance, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.EndUSDValue, _cultureInfo));
                table.Cell().Element(TableCellStyleRight).Text(PdfReportingUtil.RoundPercentValue(item.EndPercentage, 2, _cultureInfo));

                totalNAVUSDValue += item.NavUSDValue;
                totalUSDValue += item.EndUSDValue;
                totalEndReportingCurrencyValue += item.EndReportingCurrencyValue;
            }

            // The summary row
            table.Cell().ColumnSpan(3).Text(string.Empty);
            table.Cell().Element(TableSummaryStyle).Text("Totals:");
            table.Cell().Element(TableSummaryStyle).Text(PdfReportingUtil.RoundDisplayValue(totalNAVUSDValue, _cultureInfo)).Bold();
            table.Cell().Text(string.Empty);
            table.Cell().Element(TableSummaryStyle).Text(PdfReportingUtil.RoundDisplayValue(totalUSDValue, _cultureInfo)).Bold();
            table.Cell().Text(string.Empty);
        });
    }


    /// <summary>
    /// TRADES OVERVIEW TABLE
    /// This method is meant for dealing with the table structure containing the trades
    /// for a specific fund & booking period.
    /// </summary>
    /// <param name="container"></param>
    private void ComposeSummaryTable(IContainer container)
    {
        container.PaddingVertical(VERTICAL_PADDING).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                // Ticker
                columns.ConstantColumn(52);
                // Start
                columns.RelativeColumn(3);
                // In-and-Out (Shares)
                columns.RelativeColumn(5);
                // Trades
                columns.RelativeColumn(3);
                // Staking
                columns.RelativeColumn(3);
                // Profit and Loss
                columns.RelativeColumn(3);
                // All Fee
                columns.RelativeColumn(3);
                // Total Coins
                columns.RelativeColumn(3);
            });

            table.Header(header =>
            {
                header.Cell().Element(TableHeaderStyle).AlignLeft().Text("Asset");
                header.Cell().Element(TableHeaderStyle).Text("Start Balance");
                header.Cell().Element(TableHeaderStyle).Text("In-&-Out (Shares)");
                header.Cell().Element(TableHeaderStyle).Text("Trades");
                header.Cell().Element(TableHeaderStyle).Text("Staking Rewards");
                header.Cell().Element(TableHeaderStyle).Text("PNL");
                header.Cell().Element(TableHeaderStyle).Text("Fees");
                header.Cell().Element(TableHeaderStyle).Text("End Balance");
            });

            foreach (var item in _reportData.TradeSummary)
            {
                if (AllNullTradeSummaryRowCheck(item)) continue;

                table.Cell().Element(TableCellStyleLeft).Text(item.CurrencySymbol);
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.StartBalance, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(item.InOutFlow, _cultureInfo)} ({item.InOutFlowShares})");
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.TradeSum, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.StakingRewards, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.ProfitAndLoss, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.Fees, _cultureInfo));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.NavBalance, _cultureInfo));
            }
        });
    }

    /// <summary>
    /// TRANSFER LOGS
    /// This method is meant for dealing with the table structure containing the
    /// transfer log for a specific fund & booking period.
    /// </summary>
    /// <param name="container"></param>
    private void ComposeTransferLogTable(IContainer container)
    {
        container.PaddingVertical(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                // Transaction ID
                columns.RelativeColumn(7);
                // Date Time
                columns.ConstantColumn(100);
                // Transaction Type
                columns.RelativeColumn(4);
                // Direction AKA FLOW
                columns.ConstantColumn(34);
                // Ticker
                columns.ConstantColumn(52);
                // Transfer Amount (Shares)
                columns.RelativeColumn(5);
                // Transfer Fee 
                columns.RelativeColumn(3);
            });

            table.Header(header =>
            {
                header.Cell().Element(TableHeaderStyle).Text("Transaction ID");
                header.Cell().Element(TableHeaderStyle).Text("Date Time");
                header.Cell().Element(TableHeaderStyle).Text("Type");
                header.Cell().Element(TableHeaderStyle).Text("Dir");
                header.Cell().Element(TableHeaderStyle).Text("Asset");
                header.Cell().Element(TableHeaderStyle).Text("Amount (Shares)");
                header.Cell().Element(TableHeaderStyle).Text("Fee");
            });

            foreach (var item in _reportData.TransferLog)
            {
                table.Cell().Element(TableCellStyleLeft).Text($"{item.TransactionSource}:{item.TransactionId}");
                table.Cell().Element(TableCellStyleRight).Text(item.DateTime.ToString(_cultureInfo.DateTimeFormat));
                table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.TransactionTypeString(item.TransactionType));
                table.Cell().Element(TableCellStyleCenter).Text(item.Direction.ToString());
                table.Cell().Element(TableCellStyleCenter).Text(item.CurrencySymbol);
                if (sharesTransactionTypes.Contains(item.TransactionType))
                {
                    table.Cell().Element(TableCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(item.TransferAmount, _cultureInfo)} ({item.Shares})");
                }
                else
                {
                    table.Cell().Element(TableCellStyleCenter).Text(PdfReportingUtil.RoundDisplayValue(item.TransferAmount, _cultureInfo));
                }
                if (item.TransferFee != 0)
                {
                    table.Cell().Element(TableCellStyleCenter).Text($"{item.TransferFee} {item.FeeCurrencySymbol}");
                }
                else
                {
                    table.Cell().Element(TableCellStyleCenter).Text("-");
                }
            }
        });
    }

    /// <summary>
    /// This method is meant for dealing with the table structure containing the
    /// trading log for a specific fund & booking period.
    /// </summary>
    /// <param name="container"></param>
    private void ComposeTradingLogTable(IContainer container)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // For each order we create a table with headers, sub rows with the
            // trade summary and funding values, and a table with all the
            // trades for the order.
            foreach (var item in _reportData.TradeLog)
            {
                // First the Order
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        // Order Number
                        columns.ConstantColumn(90);
                        // Date Time
                        columns.ConstantColumn(100);
                        // Pair
                        columns.RelativeColumn(2);
                        // Type
                        columns.ConstantColumn(35);
                        // Status
                        columns.RelativeColumn(2);
                        // Order Price
                        columns.RelativeColumn(3);
                        // Order Amount
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(TableHeaderStyle).Text("Order Number");
                        header.Cell().Element(TableHeaderStyle).Text("Date Time");
                        header.Cell().Element(TableHeaderStyle).Text("Pair");
                        header.Cell().Element(TableHeaderStyle).Text("Type");
                        header.Cell().Element(TableHeaderStyle).Text("Status");
                        header.Cell().Element(TableHeaderStyle).Text("Order Price");
                        header.Cell().Element(TableHeaderStyle).Text("Order Amount");
                    });

                    // Order Number
                    table.Cell().Element(TableCellStyleLeft).Text($"{item.Exchange}\r\n{item.OrderNumber}");
                    // Date time
                    table.Cell().Element(TableCellStyleRight).Text(item.DateTime.ToString(_cultureInfo.DateTimeFormat));
                    // Pair
                    table.Cell().Element(TableCellStyleCenter).Text($"{item.BaseAssetSymbol}/{item.QuoteAssetSymbol}");
                    // Type
                    table.Cell().Element(TableCellStyleCenter).Text(item.Direction.ToString());
                    // Status
                    table.Cell().Element(TableCellStyleCenter).Text(item.State.ToString());
                    // "Order Price"
                    table.Cell().Element(TableCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(item.UnitPrice, _cultureInfo)} {item.QuoteAssetSymbol}");
                    // Orde Amount
                    table.Cell().Element(TableCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(item.Amount, _cultureInfo)} {item.BaseAssetSymbol}");
                });

                // Next a nested table for the summary
                column.Item().PaddingLeft(190).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        // Avg Trading Price
                        columns.RelativeColumn();
                        // Filled
                        columns.RelativeColumn();
                        // Filled Total
                        columns.RelativeColumn();
                        // Total Fees
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(NestedHeaderCellStyle).Text("Avg Price");
                        header.Cell().Element(NestedHeaderCellStyle).Text("Filled");
                        header.Cell().Element(NestedHeaderCellStyle).Text("Total");
                        header.Cell().Element(NestedHeaderCellStyle).Text("Fees");
                    });

                    // Avg Trading Price
                    table.Cell().Element(TableCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(item.FilledTotal / item.FilledAmount, _cultureInfo)} {item.QuoteAssetSymbol}");
                    // Filled
                    table.Cell().Element(TableCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(item.FilledAmount, _cultureInfo)} {item.BaseAssetSymbol}");
                    // Filled Total
                    table.Cell().Element(TableCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(item.FilledTotal, _cultureInfo)} {item.QuoteAssetSymbol}");
                    List<string> totalFeeAndSymbolPairs = new();
                    foreach (var totalFee in item.TotalFees)
                    {
                        totalFeeAndSymbolPairs.Add($"{PdfReportingUtil.RoundDisplayValue(totalFee.TotalFee, _cultureInfo)} {totalFee.FeeCryptoSymbol}");
                    }
                    table.Cell().Element(NestedCellStyleCenter).Text(string.Join("\n", totalFeeAndSymbolPairs));
                });

                // Then a nested table for the funding
                column.Item().PaddingLeft(190).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        // Avg Trading Price
                        columns.RelativeColumn();
                        // Filled
                        columns.RelativeColumn();
                        // Filled Total
                        columns.RelativeColumn();
                        // Total Fees
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        // Funding section
                        header.Cell().Element(NestedHeaderCellStyle).Text("Funding %");
                        // Fund Amount
                        header.Cell().Element(NestedHeaderCellStyle).Text("Fund Amount");
                        // Fund Total
                        header.Cell().Element(NestedHeaderCellStyle).Text("Fund Total");
                        // Fund Fees
                        header.Cell().Element(NestedHeaderCellStyle).Text("Fund Fees");
                    });

                    table.Cell().Element(TableSummaryStyle).Text(PdfReportingUtil.RoundPercentValue(item.FundingPercentage, 2, _cultureInfo));
                    table.Cell().Element(TableSummaryStyle).Text($"{PdfReportingUtil.RoundDisplayValue(item.FundAmount, _cultureInfo)} {item.BaseAssetSymbol}");
                    table.Cell().Element(TableSummaryStyle).Text($"{PdfReportingUtil.RoundDisplayValue(item.FundTotal, _cultureInfo)} {item.QuoteAssetSymbol}");
                    List<string> fundFeesAndSymbolPairs = new();
                    foreach (var fundFee in item.FundFees)
                    {
                        fundFeesAndSymbolPairs.Add($"{PdfReportingUtil.RoundDisplayValue(fundFee.TotalFee, _cultureInfo)} {fundFee.FeeCryptoSymbol}");
                    }
                    table.Cell().Element(TableSummaryStyle).Text(string.Join("\n", fundFeesAndSymbolPairs));
                });

                // And finally the nested table for the trades
                column.Item().PaddingLeft(90).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        // Date Time
                        columns.ConstantColumn(100);
                        // Avg Trading Price
                        columns.RelativeColumn();
                        // Filled
                        columns.RelativeColumn();
                        // Filled Total
                        columns.RelativeColumn();
                        // Total Fees
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        // Date (UTC)
                        header.Cell().Element(NestedHeaderCellStyle).Text("Date Time");
                        // Trading Price
                        header.Cell().Element(NestedHeaderCellStyle).Text("Price");
                        // Filled
                        header.Cell().Element(NestedHeaderCellStyle).Text("Filled");
                        // Total
                        header.Cell().Element(NestedHeaderCellStyle).Text("Total");
                        // Fee
                        header.Cell().Element(NestedHeaderCellStyle).Text("Fee");
                    });

                    foreach (var trade in item.Trades)
                    {
                        // Date (UTC)
                        table.Cell().Element(NestedCellStyleRight).Text(trade.DateTime.ToString(_cultureInfo.DateTimeFormat));
                        // Trading Price
                        table.Cell().Element(NestedCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(trade.UnitPrice, _cultureInfo)} {item.QuoteAssetSymbol}");
                        // Filled
                        table.Cell().Element(NestedCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(trade.Executed, _cultureInfo)} {item.BaseAssetSymbol}");
                        // Total
                        table.Cell().Element(NestedCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(trade.Total, _cultureInfo)} {item.QuoteAssetSymbol}");
                        // Fee
                        table.Cell().Element(NestedCellStyleCenter).Text($"{PdfReportingUtil.RoundDisplayValue(trade.Fee, _cultureInfo)} {trade.FeeCurrencySymbol}");
                    }
                });
            }
        });
    }

    private static bool AllNullTradeSummaryRowCheck(ReportTradeSummaryViewModel tradeSummary) =>
        tradeSummary.StartBalance.Equals(0) &&
        tradeSummary.InOutFlow.Equals(0) &&
        tradeSummary.InOutFlowShares.Equals(0) &&
        tradeSummary.TradeSum.Equals(0) &&
        tradeSummary.StakingRewards.Equals(0) &&
        tradeSummary.ProfitAndLoss.Equals(0) &&
        tradeSummary.Fees.Equals(0) &&
        tradeSummary.EndBalance.Equals(0);

    private static bool AllNullHoldingsRowCheck(ReportHoldingViewModel holdingsReport) =>
        holdingsReport.StartBalance.Equals(0) &&
        holdingsReport.NavBalance.Equals(0) &&
        holdingsReport.EndBalance.Equals(0);
}
