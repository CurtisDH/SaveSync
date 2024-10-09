using System.Net;
using System.Reflection;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace SaveSync.Integrations.DropBox;

public class Dropbox : BaseIntegration
{
    private const string ApiKey = "mjjbsl9ue09cjmk";
    private const string LoopbackHost = "http://localhost:5000/";
    private const string FullFilePath = $"/CurtisDH-SaveSync/SaveSync.zip";
    private const string DropBoxPath = $"/CurtisDH-SaveSync/";
    private readonly Uri _redirectUri = new Uri(LoopbackHost + "authorize");
    private readonly Uri _jsRedirectUri = new Uri(LoopbackHost + "token");

    protected override byte[] Download()
    {
        try
        {
            var client = new DropboxClient(IntegrationManager.SettingsConfig.DropboxConfigModel.RefreshToken, ApiKey);
            var files = client.Files.DownloadAsync(FullFilePath);
            var filebytes = files.Result.GetContentAsByteArrayAsync().Result;
            return filebytes;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Download Failed -- save data may not be present. Error message: {e.Message}");
        }

        return [];
    }

    protected override bool Upload(byte[] zippedContents)
    {
        if (zippedContents == null || zippedContents.Length == 0)
        {
            throw new ArgumentException("File contents cannot be null or empty", nameof(zippedContents));
        }

        using var memStream = new MemoryStream(zippedContents);
        var task = UploadFileToDropbox("SaveSync.zip", memStream);
        task.Wait();

        return !string.IsNullOrEmpty(task.Result);
    }

    private async Task<string> UploadFileToDropbox(string fileName, MemoryStream mem)
    {
        try
        {
            var updated =
                await new DropboxClient(IntegrationManager.SettingsConfig.DropboxConfigModel.RefreshToken, ApiKey).Files
                    .UploadAsync(
                        $"{DropBoxPath}{fileName}",
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

    public override async Task DisplayUiOptions()
    {
        while (true)
        {
            Console.WriteLine("-= Dropbox Configuration Setup =-");
            if (string.IsNullOrEmpty(IntegrationManager.SettingsConfig.DropboxConfigModel.RefreshToken))
            {
                await AcquireAccessToken(null, IncludeGrantedScopes.None);
            }
            else
            {
                if (TestConnection())
                {
                    break;
                }

                IntegrationManager.SettingsConfig.DropboxConfigModel.Reset();
            }

            break;
        }
    }

    public override bool TestConnection()
    {
        try
        {
            var client = new DropboxClient(IntegrationManager.SettingsConfig.DropboxConfigModel.RefreshToken, ApiKey);
            var t = client.Users.GetCurrentAccountAsync();
            var result = t.Result;
            return !string.IsNullOrEmpty(result.Name.DisplayName);
        }
        catch
        {
            return false;
        }
    }

    private async Task AcquireAccessToken(string[] scopeList, IncludeGrantedScopes includeGrantedScopes)
    {
        var accessToken = IntegrationManager.SettingsConfig.DropboxConfigModel.AccessToken;
        var refreshToken = IntegrationManager.SettingsConfig.DropboxConfigModel.RefreshToken;

        if (!string.IsNullOrEmpty(accessToken)) return;

        try
        {
            Console.WriteLine("Waiting for credentials.");
            var state = Guid.NewGuid().ToString("N");
            var oAuthFlow = new PKCEOAuthFlow();
            var authorizeUri = oAuthFlow.GetAuthorizeUri(OAuthResponseType.Code, ApiKey, _redirectUri.ToString(),
                state: state, tokenAccessType: TokenAccessType.Offline, scopeList: scopeList,
                includeGrantedScopes: includeGrantedScopes);
            var http = new HttpListener();
            http.Prefixes.Add(LoopbackHost);

            http.Start();
            Console.WriteLine($"Please go to {authorizeUri.ToString()} to verify");

            await HandleOAuth2Redirect(http);

            var redirectUri = await HandleJsRedirect(http);

            Console.WriteLine("Exchanging code for token");
            var tokenResult =
                await oAuthFlow.ProcessCodeFlowAsync(redirectUri, ApiKey, _redirectUri.ToString(), state);
            Console.WriteLine("Finished Exchanging Code for Token");
            accessToken = tokenResult.AccessToken;
            refreshToken = tokenResult.RefreshToken;
            var uid = tokenResult.Uid;
            
            IntegrationManager.SettingsConfig.DropboxConfigModel.AccessToken = accessToken;
            IntegrationManager.SettingsConfig.DropboxConfigModel.RefreshToken = refreshToken;
            IntegrationManager.SettingsConfig.DropboxConfigModel.Uid = uid;
            IntegrationManager.SettingsConfig.Save();
            http.Stop();
            Console.WriteLine("Dropbox token successfully saved..");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e.Message);
        }
    }

    private async Task HandleOAuth2Redirect(HttpListener http)
    {
        var context = await http.GetContextAsync();

        while (context.Request.Url.AbsolutePath != _redirectUri.AbsolutePath)
        {
            context = await http.GetContextAsync();
        }

        context.Response.ContentType = "text/html";
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream("SaveSync.Integrations.DropBox.index.html"))
        {
            if (stream != null)
            {
                await stream.CopyToAsync(context.Response.OutputStream);
            }
            else
            {
                throw new FileNotFoundException("Embedded resource not found.");
            }
        }

        context.Response.OutputStream.Close();
    }

    private async Task<Uri> HandleJsRedirect(HttpListener http)
    {
        var context = await http.GetContextAsync();
        while (context.Request.Url.AbsolutePath != _jsRedirectUri.AbsolutePath)
        {
            context = await http.GetContextAsync();
        }

        var redirectUri = new Uri(context.Request.QueryString["url_with_fragment"]);

        return redirectUri;
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
}