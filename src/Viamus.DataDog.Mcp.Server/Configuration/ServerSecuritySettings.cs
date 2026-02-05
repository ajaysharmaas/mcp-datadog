namespace Viamus.DataDog.Mcp.Server.Configuration;

public class ServerSecuritySettings
{
    public const string SectionName = "ServerSecurity";

    public string ApiKey { get; set; } = string.Empty;
    public bool RequireApiKey { get; set; } = false;
}
