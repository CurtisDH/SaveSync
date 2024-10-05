using System.Text.Json;

namespace Soulash2_SaveSync.Integrations.DropBox.Utils;

using System.Net.Http;
using System.Threading.Tasks;

public class DropBoxAuthHandling
{
    public static string GetAuthorizationUrl(string clientId, string redirectUri, string codeChallenge)
    {
        return
            $"https://www.dropbox.com/oauth2/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&code_challenge={codeChallenge}&code_challenge_method=S256";
    }

    public class DropboxTokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string Uid { get; set; }
    }
}