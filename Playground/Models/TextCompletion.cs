// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;

namespace Playground;

public class TextCompletion
{
    private string Prompt
    {
        get => @"
            Generate a creative reason or excuse for the given event.
            Be creative and be funny. Let your imagination run wild.

            Event: I am running late.
            Excuse: I was being held ransom by giraffe gangsters.

            Event: I haven't been to the gym for a year
            Excuse: I've been too busy training my pet dragon.

            Event: {{$input}}
            ";
    }
    private Func<string> readUserInput;
    private Action<string> writeBotOutput;
    private IKernel kernel;
    public TextCompletion(string modelId, string apiKey, Func<string> readUserInput, Action<string> writeBotOutput)
    {
        this.readUserInput = readUserInput;
        this.writeBotOutput = writeBotOutput;
        this.kernel = new KernelBuilder().WithOpenAITextCompletionService(modelId, apiKey).Build();
    }

    public async Task Run()
    {
        string input = this.readUserInput();
        this.WriteToOutput("Input", input);

        var excuseFunction = this.kernel.CreateSemanticFunction(this.Prompt, maxTokens: 100, temperature: 0.4, topP: 1);
        try
        {
            var result = await excuseFunction.InvokeAsync(input);
            this.WriteToOutput("Response", result.ToString());
        }
        catch (Exception ex)
        {
            this.writeBotOutput($"Error: {ex.Message}{Constants.DoubleNewLines}");
        }
    }

    private void WriteToOutput(string role, string message)
    {
        this.writeBotOutput($"{role}:{Environment.NewLine}\t{message}{Constants.DoubleNewLines}");
    }

    public void WritePromptToOutput()
    {
        this.writeBotOutput($"Prompt:{Environment.NewLine}\t{this.Prompt}{Constants.DoubleNewLines}");
    }
}

