using System.Net;
using System.Text;
using System.Text.Json;
using Dropbox.Api;
using Dropbox.Api.Files;
using Soulash2_SaveSync.Integrations.DropBox.Utils;

namespace Soulash2_SaveSync.Integrations.DropBox;

public class Dropbox : BaseIntegration
{
    private string _accessToken;
    private DropboxClient _dropboxClient;

    protected override byte[] Download()
    {
        throw new NotImplementedException();
    }

    protected override bool Upload(byte[] zippedContents)
    {
        throw new NotImplementedException();
    }

    public override async Task DisplayUiOptions()
    {
        while (true)
        {
            Console.WriteLine("-= Dropbox Configuration Setup =-");
            _dropboxClient = await AuthenticateWithDropbox();

            await UploadTestFile();
        }
    }

    private async Task<DropboxClient> AuthenticateWithDropbox()
    {
        var clientId = "smmi3ym2bdgwkk2"; 
        var redirectUri = "http://localhost:5000/callback";

        var (codeVerifier, codeChallenge) = PkceHelper.GeneratePkceCodes();

        var authorizationUrl = DropBoxAuthHandling.GetAuthorizationUrl(clientId, redirectUri, codeChallenge);
        Console.WriteLine($"Please go to this URL to authorize the app: {authorizationUrl}");

        var authorizationCode = await StartLocalServerAndGetAuthorizationCode(redirectUri);

        _accessToken = await GetAccessToken(clientId, redirectUri, authorizationCode, codeVerifier);
        
        var client = new DropboxClient(_accessToken, new DropboxClientConfig("Soulash2-SaveSync", 3));
        var accountInfo = await client.Users.GetCurrentAccountAsync();
        Console.WriteLine($"Connected to Dropbox account: {accountInfo.Name.DisplayName}");
        return client;
    }
    private async Task<string> StartLocalServerAndGetAuthorizationCode(string redirectUri)
    {
        var uriParts = redirectUri.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries);
        var host = uriParts[1].Split('/')[0]; 
        var path = "/" + string.Join("/", uriParts[1].Split('/').Skip(1)); 

        using (var listener = new HttpListener())
        {
            listener.Prefixes.Add(redirectUri + "/"); 
            listener.Start();
            Console.WriteLine("Waiting for authorization code...");

            var context = await listener.GetContextAsync(); 
            var response = context.Response;
            string code = context.Request.QueryString["code"];

            string responseString = "Authorization successful! You can close this window.";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            output.Close();

            listener.Stop();
            return code; 
        }
    }

    private async Task<string> GetAccessToken(string clientId, string redirectUri, string authorizationCode, string codeVerifier)
    {
        using (var httpClient = new HttpClient())
        {
            var tokenUrl = "https://api.dropboxapi.com/oauth2/token";
            var parameters = new Dictionary<string, string>
            {
                { "code", authorizationCode },
                { "client_id", clientId },
                { "redirect_uri", redirectUri },
                { "code_verifier", codeVerifier },
                { "grant_type", "authorization_code" }
            };

            var response = await httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(parameters));
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<DropBoxAuthHandling.DropboxTokenResponse>(responseBody);
            return tokenResponse.AccessToken; 
        }
    }

    private async Task UploadTestFile()
    {
        const string fileName = "Test.txt";
        File.WriteAllText(fileName, "Lets see if this works");
        var content = File.ReadAllText(fileName);

        using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))
        {
            string revision = await UploadFileToDropbox(fileName, mem);
            Console.WriteLine(revision);
        }
    }

    private async Task<string> UploadFileToDropbox(string fileName, MemoryStream mem)
    {
        try
        {
            var updated = await _dropboxClient.Files.UploadAsync(
                $"/Soulash2-SaveSync/{fileName}",
                WriteMode.Overwrite.Instance,
                body: mem);

            return updated.Rev;
        }
        catch (ApiException<UploadError> e)
        {
            HandleUploadError(e, fileName);
            return string.Empty; 
        }
    }

    private void HandleUploadError(ApiException<UploadError> e, string fileName)
    {
        var uploadError = e.ErrorResponse.AsPath;
        if (uploadError != null)
        {
            var reason = uploadError.Value.Reason;
            var id = Path.GetFileNameWithoutExtension(fileName);
            var message = string.Format("Unable to update {0}. Reason: {1}", id, reason);
            Console.WriteLine(message);
        }
    }

    public override void Run()
    {
        throw new NotImplementedException();
    }
    


}

