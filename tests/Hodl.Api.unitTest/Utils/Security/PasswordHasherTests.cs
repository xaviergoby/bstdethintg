namespace Hodl.Api.UnitTest.Utils.Security;

internal class PasswordHasherTests
{
    private readonly byte[] salt = Encoding.UTF8.GetBytes("Just a string to generate a salt");

    [Test]
    public async Task HashSameTest()
    {
        // Test 2 times same input
        PasswordHasher hasher = new();

        var result1 = await hasher.Hash("123StrongPassword!", salt);
        var result2 = await hasher.Hash("123StrongPassword!", salt);

        Assert.AreEqual(result1, result2, "Hashing the same password twice fails to generate the same output.");
    }

    [Test]
    public async Task HashDifferentTest()
    {
        // Test 2 times different input
        PasswordHasher hasher = new();

        var result1 = await hasher.Hash("123StrongPassword!", salt);
        var result2 = await hasher.Hash("DifferentPassword!", salt);

        Assert.AreNotEqual(result1, result2, "Hashing different passwords fails to generate different output.");

        var result3 = await hasher.Hash("123 Strong Password!", salt);
        var result4 = await hasher.Hash("123 strong Password!", salt); // lowercase s for strong

        Assert.AreNotEqual(result3, result4, "Hashing different passwords fails to generate different output.");
    }
}
