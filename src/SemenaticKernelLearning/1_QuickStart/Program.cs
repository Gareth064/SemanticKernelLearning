//Import Packages
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using QuickStart.Plugins;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

//Create kernel
var builder = Kernel.CreateBuilder();

//Add OpenAI chat completion
builder.AddOpenAIChatCompletion(
    modelId: "gpt-3.5-turbo",
    apiKey: configuration["ApiKey"]);

// Add logging
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Warning));

// Build the kernel
var kernel = builder.Build();

// Retrieve the chat completion service
var chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();

// Add plugin to the kernel
kernel.Plugins.AddFromType<LightsPlugin>("Lights");

// Set execution settings so the kernel knows to automatically invoke the functions in plugins
OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

// Create a Chat History to store the context of the conversation
var chatHistory = new ChatHistory();

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.ForegroundColor = ConsoleColor.White;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    chatHistory.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: executionSettings,
        kernel: kernel);

    // Print the results
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("Assistant > " + result);
    Console.ForegroundColor = ConsoleColor.White;

    // Add the message from the agent to the chat history
    chatHistory.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);