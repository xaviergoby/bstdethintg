using System.Runtime.Serialization;

namespace Hodl.Api.UnitTest.Extensions;

public class StringExtensionsTest
{
    private enum TransactionType
    {
        [EnumMember(Value = "WALLET")]
        Wallet,

        [EnumMember(Value = "BANK")]
        Bank,
    }

    private enum OrderDirection
    {
        Unknown = 0,
        Buy = 1,
        Sell = 2
    }

    private enum OrderState
    {
        Unknown = 0,
        Cancelled = 7,
        PartFilled = 9
    }



    [Test]
    public void ParseEnumTest()
    {
        Assert.Multiple(() =>
        {
            Assert.That("WALLET".ParseEnum<TransactionType>(), Is.EqualTo(TransactionType.Wallet), "ParseEnum Value attribute test failed.");
            Assert.That("1".ParseEnum<OrderDirection>(), Is.EqualTo(OrderDirection.Buy), "ParseEnum Index value test failed");
            Assert.That("PartFilled".ParseEnum<OrderState>(), Is.EqualTo(OrderState.PartFilled), "ParseEnum name string test failed");
        });
    }

    [Test]
    [TestCase("ABCD", -1, '-')]
    [TestCase("ABCD", 0, '-')]
    public void SplitFormatTestErrors(string input, int partsLength, char fillChar)
    {
        try
        {
            StringExtensions.SplitFormat(input, partsLength, fillChar);
            Assert.Fail();
        }
        catch
        {
            Assert.Pass();
        }
    }

    [Test]
    [TestCase("", 2, '-', true, "")]
    [TestCase("", 2, '-', false, "")]
    [TestCase("1234", 4, '-', true, "1234")]
    [TestCase("1234", 4, '-', false, "1234")]
    [TestCase("12345", 4, '-', true, "1234-5")]
    [TestCase("12345", 4, '-', false, "1234")]
    [TestCase("12345678", 4, '-', true, "1234-5678")]
    [TestCase("12345678", 4, '-', false, "1234-5678")]
    [TestCase("12345678", 2, '-', true, "12-34-56-78")]
    [TestCase("12345678", 2, '-', false, "12-34-56-78")]
    [TestCase("ABCDEFGH", 3, ' ', true, "ABC DEF GH")]
    [TestCase("AbCdEfGh", 3, ' ', false, "AbC dEf")]
    public void SplitFormatTest(string input, int partsLength, char fillChar, bool addLeftover, string expected)
    {
        string output = StringExtensions.SplitFormat(input, partsLength, fillChar, addLeftover);
        Assert.That(output, Is.EqualTo(expected), "Input: \"{0}\"", input);
    }

    [Test]
    [TestCase("", "")]
    [TestCase("Test 1234", "test-1234")]
    [TestCase("AbcdeF", "abcdef")]
    [TestCase("Allow-hyphens", "allow-hyphens")]
    [TestCase("Test with spaces", "test-with-spaces")]
    [TestCase("Remove !@#$%*():/| special chars", "remove-special-chars")]
    [TestCase("Remove  double   spaces", "remove-double-spaces")]
    public void GenerateSlugTest(string input, string expected)
    {
        string output = StringExtensions.GenerateSlug(input);
        Assert.That(expected, Is.EqualTo(output), "Input: \"{0}\", Expected output: \"{1}\", Actual output: \"{2}\"", input, expected, output);
    }

    [Test]
    public void ParamToUtcDateTestIsUTCDate()
    {
        var output = StringExtensions.ParamToUtcDate("", new DateTime());

        Assert.That(output.Kind, Is.EqualTo(DateTimeKind.Utc), "Expected datetime kind to be UTC but got {0}", output.Kind);
    }

    [Test]
    [TestCase("", 2020, 1, 1, 2020, 1, 1, 0, 0, 0)]
    [TestCase("2022-05-21", 2020, 1, 1, 2022, 5, 21, 0, 0, 0)]
    [TestCase("2022-05-21T14:15:16", 2020, 1, 1, 2022, 5, 21, 14, 15, 16)]
    [TestCase("2022-05-06T14:15:16", 2020, 1, 1, 2022, 5, 6, 14, 15, 16)]
    [TestCase("2022-05-21T01:02:03", 2020, 1, 1, 2022, 5, 21, 1, 2, 3)]
    [TestCase("2022/05/21T14:15:16", 2020, 1, 1, 2022, 5, 21, 14, 15, 16)]
    [TestCase("2022-05-21 14:15:16", 2020, 1, 1, 2022, 5, 21, 14, 15, 16)]
    public void ParamToUtcDateTest(string input, int defYear, int defMonth, int defDay, int outYear, int outMonth, int outDay, int outHour, int outMinute, int outSeconds)
    {
        var expected = new DateTime(outYear, outMonth, outDay, outHour, outMinute, outSeconds, DateTimeKind.Utc);
        var output = StringExtensions.ParamToUtcDate(input, new DateTime(defYear, defMonth, defDay));

        Assert.That(output, Is.EqualTo(expected), "Input: \"{0}\"", input);
    }
}
