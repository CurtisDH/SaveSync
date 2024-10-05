using System.Security.Cryptography;
using System.Text;

namespace Soulash2_SaveSync.Integrations.DropBox.Utils;

public static class PkceHelper
{
    public static (string codeVerifier, string codeChallenge) GeneratePkceCodes()
    {
        var codeVerifier = GenerateRandomString(128);

        var codeChallenge = ComputeSha256Base64Url(codeVerifier);
        return (codeVerifier, codeChallenge);
    }

    private static string GenerateRandomString(int length)
    {
        var randomBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string ComputeSha256Base64Url(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}