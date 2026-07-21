using System.ComponentModel;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

IChatClient chatClient = new OllamaChatClient(new Uri("http://localhost:11434"), "gemma4");

chatClient = ChatClientBuilderChatClientExtensions.AsBuilder(chatClient).UseFunctionInvocation().Build();

var app = builder.Build();
var connectionString = builder.Configuration["Database:ConnectionString"];

app.UseHttpsRedirection();

app.MapPost("/api/triage", async (IncidentReport report) =>
{
    var tools = new EnterpriseTools(connectionString);
    var phase1Options = new ChatOptions
    {
        Tools = [
            AIFunctionFactory.Create(tools.GetSystemStatus),
            AIFunctionFactory.Create(tools.EscalateToTeam),
            AIFunctionFactory.Create(tools.QueryDatabase),
        ],
        ResponseFormat = ChatResponseFormat.ForJsonSchema<TriageResult>(),
    };
    var phase1SystemPrompt = """
    You are an Enterprise Incident Triage Agent.
    Analyse incoming incidents, inspect relevant system states using tools, and escalate as necessary.
    Always summarise final action clearly
    """;

    var phase1Response = await chatClient.GetResponseAsync([
            new ChatMessage(ChatRole.System, phase1SystemPrompt),
            new ChatMessage(ChatRole.User, $"System: {report.SystemId}, Issue: {report.Description}")
    ], phase1Options);

    return Results.Ok(new
    {
        Resolution = JsonSerializer.Deserialize<JsonElement>(phase1Response.Text),
        ModelUsed = "gemma4"
    });
})
.WithName("TriageIncident");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public enum SeverityLevel
{
    Low,
    Medium,
    High,
    Critical
}
public record IncidentReport(string Description, string SystemId);
public record TriageResult(
       string IncidentId,
       SeverityLevel SeverityLevel,
       [Description("The action taken")] string ActionTaken,
       [Description("A list of the specified tools that were used")] List<string> ToolsUsed,
       [Description("The response given by the tools that were used")] List<string> ToolResponses,
       [Description("Suggested next action for team that this request is passed on to")] string NextAction);

public class EnterpriseTools(string connectionString)
{
    public string GetSystemStatus(string systemId)
    {
        return systemId.ToLower() switch
        {
            "auth-service" => "DEGRADED - high latency (database pool exhausted)",
            "payment-gateway" => "HEALTHY",
            _ => "UNKNOWN SYSTEM"
        };
    }

    [Description("Escalates to specified teamName")]
    public string EscalateToTeam(string teamName, string priority)
    {
        return $"SUCCESS: Escalated with {priority} priority to team '{teamName}'";
    }

    [Description("Executes a read-only SQL query against the database")]
    public async Task<string> QueryDatabase(
            [Description("A full SELECT SQL query string to be executed against the Azure SQL Database")]
            string sqlQuery)
    {
        if (string.IsNullOrWhiteSpace(sqlQuery))
        {
            Console.WriteLine("query is null");
            return "Query not entered?";
        }

        if (!sqlQuery.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return "ERROR: Only read-only SELECT queries are allowed";
        }

        var results = new List<Dictionary<string, object>>();
        try
        {
            await using var sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();

            await using var command = new SqlCommand(sqlQuery, sqlConnection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(row);
            }
        }
        catch (SqlException e)
        {
            Console.WriteLine($"SQL Error: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        return JsonSerializer.Serialize(results);
    }
}
