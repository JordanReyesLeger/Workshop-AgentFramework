using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.ClientModel;

// ──────────────────────────────────────────────
// Configuración desde appsettings.json
// ──────────────────────────────────────────────
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var endpoint = configuration["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Falta la configuración AzureOpenAI:Endpoint");
var apiKey = configuration["AzureOpenAI:ApiKey"]
    ?? throw new InvalidOperationException("Falta la configuración AzureOpenAI:ApiKey");
var deploymentName = configuration["AzureOpenAI:DeploymentName"]
    ?? throw new InvalidOperationException("Falta la configuración AzureOpenAI:DeploymentName");

// ──────────────────────────────────────────────
// Crear el agente
// ──────────────────────────────────────────────
var chatClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey))
    .GetChatClient(deploymentName);

AIAgent agent = chatClient.AsAIAgent(
    instructions: "Eres un asistente amigable y útil. Responde de forma concisa y clara en español.",
    name: "AgenteConsola",
    description: "Agente interactivo de consola usando Agent Framework");

// Crear sesión para mantener el historial de la conversación
AgentSession session = await agent.CreateSessionAsync();

// ──────────────────────────────────────────────
// Loop interactivo de chat
// ──────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║   Agente de Consola - Agent Framework        ║");
Console.WriteLine("║   Escribe tu mensaje y presiona Enter.       ║");
Console.WriteLine("║   Escribe 'salir' para terminar.             ║");
Console.WriteLine("╚══════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Tú > ");
    Console.ResetColor();

    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("salir", StringComparison.OrdinalIgnoreCase))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("👋 ¡Hasta luego!");
        Console.ResetColor();
        break;
    }

    try
    {
        AgentResponse response = await agent.RunAsync(input, session);

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Agente > ");
        Console.ResetColor();
        Console.WriteLine(response.Text);
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine();
    }
}
