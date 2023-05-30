using System.Security.Cryptography;
using System.Text;

namespace Hodl.Api.Utils.Configurations;

public class PasswordHasher : IPasswordHasher
{
    // TODO: Key in config?? Or DB??
    private readonly HMACSHA512 hasher = new(Encoding.UTF8.GetBytes("hodl.hmac.key"));

    public async Task<byte[]> Hash(string password, byte[] salt)
    {
        var bytes = Encoding.UTF8.GetBytes(password);

        var allBytes = new byte[bytes.Length + salt.Length];
        Buffer.BlockCopy(bytes, 0, allBytes, 0, bytes.Length);
        Buffer.BlockCopy(salt, 0, allBytes, bytes.Length, salt.Length);

        return await hasher.ComputeHashAsync(new MemoryStream(allBytes));
    }
}
