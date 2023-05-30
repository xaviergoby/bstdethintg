namespace Hodl.Api.UnitTest.Extensions;
internal class ActionResultExtensionsTest
{
    [Test]
    public void GetErrorResultFromObjectTest()
    {
        ///(this ErrorInformationItem error, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        //    var errors = new List<ErrorInformationItem> { error };
        //    return errors.GetErrorResult(httpStatusCode);
    }

    [Test]
    public void GetErrorResultFromArrayTest()
    {
        //(this IEnumerable<ErrorInformationItem> errors, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest)
        //return GetObjectResult(new { errors }, httpStatusCode);
    }

}
