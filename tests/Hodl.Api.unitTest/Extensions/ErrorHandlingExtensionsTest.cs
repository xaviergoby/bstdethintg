namespace Hodl.Api.UnitTest.Extensions;
internal class ErrorHandlingExtensionsTest
{
    [Test]
    public void CastAsErrorInformationItemsTest()
    {
        List<IdentityError> testErrors = new()
        {
            new IdentityError() { Code = "ERROR01", Description = "Error 01" },
            new IdentityError() { Code = "ERROR02", Description = "Error 02" },
        };

        var result = testErrors.CastAsErrorInformationItems();

        Assert.IsNotNull(result, "CastAsErrorInformationItems did not return anything.");
        var enumerator = result.GetEnumerator();
        Assert.IsNotNull(enumerator, "CastAsErrorInformationItems did not return an IEnumerable.");
        int count = 0;
        while (enumerator.MoveNext())
        {
            count++;
            Assert.IsNotNull(enumerator.Current, "Result has no value");
            Assert.IsAssignableFrom(typeof(ErrorInformationItem), enumerator.Current, "Result is not of type ErrorInformationItem");
        }
        Assert.AreEqual(testErrors.Count, count, "Number of items differs from input");
    }
}
