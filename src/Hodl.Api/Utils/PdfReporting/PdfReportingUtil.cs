using System.Globalization;

namespace Hodl.Api.Utils.PdfReporting;


/// <summary>
/// This is basically just a utility class for all small this necesary for generating pdf docs
/// </summary>
public static class PdfReportingUtil
{
    public static string RoundDisplayValue(decimal value, CultureInfo cultureInfo)
    {
        decimal testVal = Math.Abs(value);

        if (value == 0)
        {
            return value.ToString("0", cultureInfo);
        }
        else if (testVal >= 10000)
        {
            return value.ToString("N0", cultureInfo);
        }
        else if (testVal >= 100)
        {
            return value.ToString("N2", cultureInfo);
        }
        else if (testVal >= 10)
        {
            return value.ToString("N4", cultureInfo);
        }

        return value.ToString("N6", cultureInfo);
    }

    public static string RoundPercentValue(decimal value, int decimals, CultureInfo cultureInfo)
    {
        return (value / 100).ToString($"P{decimals}", cultureInfo);
    }

    public static string TransactionTypeString(TransactionType type)
    {
        return type switch
        {
            TransactionType.AdministrationFee => "Adm. fee",
            TransactionType.PerformanceFee => "Perf. fee",
            _ => type.ToString()
        };
    }
}

