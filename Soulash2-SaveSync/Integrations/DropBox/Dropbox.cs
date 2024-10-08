using System.Diagnostics;
using System.Net;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Soulash2_SaveSync.Integrations.DropBox;

public class Dropbox : BaseIntegration
{
    private const string ApiKey = "smmi3ym2bdgwkk2";
    private const string LoopbackHost = "http://localhost:5000/";
    private const string fullFilePath = $"/Soulash2-SaveSync/SaveSync.zip";
    private readonly Uri _redirectUri = new Uri(LoopbackHost + "authorize");
    private readonly Uri _jsRedirectUri = new Uri(LoopbackHost + "token");
    private readonly ConfigModel _settingsConfig = IntegrationManager.SettingsConfig.ConfigModel;

    protected override byte[] Download()
    {
        var client = new DropboxClient(_settingsConfig.RefreshToken, ApiKey);
        var files = client.Files.DownloadAsync(fullFilePath);
        var filebytes = files.Result.GetContentAsByteArrayAsync().Result;
        return filebytes;
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
            var updated = await new DropboxClient(_settingsConfig.RefreshToken,ApiKey).Files.UploadAsync(
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

    public override async Task DisplayUiOptions()
    {
        while (true)
        {
            Console.WriteLine("-= Dropbox Configuration Setup =-");
            if (string.IsNullOrEmpty(_settingsConfig.RefreshToken))
            {
                await AcquireAccessToken(null, IncludeGrantedScopes.None);
            }
            else
            {
                if (TestConnection())
                {
                    break;
                }
                _settingsConfig.Reset();
            }
            break;
        }
    }

    public override bool TestConnection()
    {
        try
        {
            var client = new DropboxClient(_settingsConfig.RefreshToken, ApiKey);
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
        var accessToken = _settingsConfig.AccessToken;
        var refreshToken = _settingsConfig.RefreshToken;

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
            Console.WriteLine("Uid: {0}", uid);
            Console.WriteLine("AccessToken: {0}", accessToken);
            if (tokenResult.RefreshToken != null)
            {
                Console.WriteLine("RefreshToken: {0}", refreshToken);
                _settingsConfig.RefreshToken = refreshToken;
            }

            if (tokenResult.ExpiresAt != null)
            {
                Console.WriteLine("ExpiresAt: {0}", tokenResult.ExpiresAt);
            }

            if (tokenResult.ScopeList != null)
            {
                Console.WriteLine("Scopes: {0}", String.Join(" ", tokenResult.ScopeList));
            }

            _settingsConfig.AccessToken = accessToken;
            _settingsConfig.Uid = uid;
            IntegrationManager.SettingsConfig.Save();
            http.Stop();
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
        
        using (var file = File.OpenRead("Integrations/DropBox/index.html"))
        {
            file.CopyTo(context.Response.OutputStream);
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