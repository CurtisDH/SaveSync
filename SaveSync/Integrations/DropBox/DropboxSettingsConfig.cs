namespace SaveSync.Integrations.DropBox;

public class ConfigModel
{
    public string AccessToken { get; set; }
    public string Uid { get; set; }
    public string RefreshToken { get; set; }

    public void Reset()
    {
        AccessToken = "";
        Uid = "";
        RefreshToken = "";
    }
}