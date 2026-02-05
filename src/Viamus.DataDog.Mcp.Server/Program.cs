using Viamus.DataDog.Mcp.Server.Configuration;
using Viamus.DataDog.Mcp.Server.Middleware;
using Viamus.DataDog.Mcp.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure settings
builder.Services.Configure<ServerSecuritySettings>(
    builder.Configuration.GetSection(ServerSecuritySettings.SectionName));
builder.Services.Configure<DatadogSettings>(
    builder.Configuration.GetSection(DatadogSettings.SectionName));

// Register Datadog client
builder.Services.AddHttpClient<IDatadogClient, DatadogClient>();

// Add health checks
builder.Services.AddHealthChecks();

// Configure MCP Server with HTTP/SSE transport
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// API key authentication middleware
app.UseApiKeyAuthentication();

// Health check endpoint
app.MapHealthChecks("/health");

// MCP endpoints
app.MapMcp();

app.Run();
