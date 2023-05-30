using Hodl.Api.Utils.PdfReporting;
using System.Globalization;

namespace Hodl.Api.UnitTest.Utils.PdfReporting;

internal class PdfReportingUtilTest
{
    [Test]
    [TestCase("0.123456789", "nl-NL", "0,123457")]
    [TestCase("0.123456789", "en-US", "0.123457")]
    [TestCase("9.123456789", "nl-NL", "9,123457")]
    [TestCase("10.123456789", "nl-NL", "10,1235")]
    [TestCase("99.123456789", "nl-NL", "99,1235")]
    [TestCase("100.123456789", "nl-NL", "100,12")]
    [TestCase("999.123456789", "nl-NL", "999,12")]
    [TestCase("1000.123456789", "nl-NL", "1.000,12")]
    [TestCase("9999.123456789", "nl-NL", "9.999,12")]
    [TestCase("10000.123456789", "nl-NL", "10.000")]
    [TestCase("99999.123456789", "nl-NL", "99.999")]
    [TestCase("100000.123456789", "nl-NL", "100.000")]
    [TestCase("999999.123456789", "nl-NL", "999.999")]
    [TestCase("1000000.123456789", "nl-NL", "1.000.000")]
    [TestCase("-10.123456789", "nl-NL", "-10,1235")]
    [TestCase("-99.123456789", "nl-NL", "-99,1235")]
    [TestCase("-100.123456789", "nl-NL", "-100,12")]
    [TestCase("-999.123456789", "nl-NL", "-999,12")]
    [TestCase("-1000.123456789", "nl-NL", "-1.000,12")]
    [TestCase("-9999.123456789", "nl-NL", "-9.999,12")]
    [TestCase("-10000.123456789", "nl-NL", "-10.000")]
    [TestCase("-99999.123456789", "nl-NL", "-99.999")]
    public void DisplayValueTest(decimal test, string cultureString, string expected)
    {
        var culture = new CultureInfo(cultureString);

        var displayValue = PdfReportingUtil.RoundDisplayValue(test, culture);

        Assert.AreEqual(expected, displayValue);
    }

    [Test]
    [TestCase("0.123456789", 4, "nl-NL", "0,1235%")]
    [TestCase("0.123456789", 4, "en-US", "0.1235%")]
    [TestCase("9.123456789", 2, "nl-NL", "9,12%")]
    [TestCase("10.123456789", 0, "nl-NL", "10%")]
    public void PercentageValueTest(decimal test, int decimals, string cultureString, string expected)
    {
        var culture = new CultureInfo(cultureString);

        var displayValue = PdfReportingUtil.RoundPercentValue(test, decimals, culture);

        Assert.AreEqual(expected, displayValue);
    }

}
