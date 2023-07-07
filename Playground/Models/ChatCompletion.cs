// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;

namespace Playground;

public class ChatCompletion
{
    private IChatCompletion chatGPT;
    private ChatHistory chatHistory;
    private Func<string> readUserInput;
    private Action<string> writeBotOutput;

    public ChatCompletion(string modelId, string apiKey, string newChatInstruction,
        Func<string> readUserInput, Action<string> writeBotOutput)
    {
        this.chatGPT = new OpenAIChatCompletion(modelId, apiKey);
        this.chatHistory = this.chatGPT.CreateNewChat(newChatInstruction);
        this.readUserInput = readUserInput;
        this.writeBotOutput = writeBotOutput;
    }

    public async Task Run()
    {
        this.AddUserMessage(this.readUserInput());
        try
        {
            var reply = await this.chatGPT.GenerateMessageAsync(this.chatHistory);
            this.AddAssistantMessage(reply);
        }
        catch (Exception ex)
        {
            this.writeBotOutput($"Error: {ex.Message}{Constants.DoubleNewLines}");
        }
    }

    private void AddUserMessage(string prompt)
    {
        this.chatHistory.AddUserMessage(prompt);
        this.OutputMostRecentChatHistory();
    }

    private void AddAssistantMessage(string reply)
    {
        this.chatHistory.AddAssistantMessage(reply);
        this.OutputMostRecentChatHistory();
    }

    private void OutputMostRecentChatHistory()
    {
        var message = this.chatHistory.Messages.Last();
        this.writeBotOutput($"{message.Role}:{Environment.NewLine}\t{message.Content}{Constants.DoubleNewLines}");
    }
}

