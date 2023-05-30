using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.UnitTest.Extensions;

internal class GenericExtensionMethodsTest
{
    private class Holding
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FundId { get; set; }
    }


    [Test]
    public void DeepCopyTest()
    {
        Holding holding = new()
        {
            FundId = Guid.NewGuid()
        };

        var copy = holding.DeepCopy();

        Assert.Multiple(() =>
        {
            Assert.That(copy, Is.Not.Null, "DeepCopy failed to copy the object");
            Assert.That(copy.Id, Is.EqualTo(holding.Id), "DeepCopy Id is not the same");
            Assert.That(copy.FundId, Is.EqualTo(holding.FundId), "DeepCopy FundId is not the same");
        });
    }
}
