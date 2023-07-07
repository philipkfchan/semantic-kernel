// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;

namespace Playground;

public class Intention
{
    private string Prompt
    {
        get => @"Bot: How can I help you?
                     User: {{$input}}

                     ---------------------------------------------

                     Provide the intent of the user.
                     The intent should be one of the following: {{$options}}

                     INTENT:";
    }
    private string AllowedOutputs
    {
        get => "RedTrace, BlueTrace, RedButton, BlueButton";
    }
    private Func<string> readUserInput;
    private Action<string> writeBotOutput;
    private IKernel kernel;

    public Intention(string modelId, string apiKey, Func<string> readUserInput, Action<string> writeBotOutput)
    {
        this.readUserInput = readUserInput;
        this.writeBotOutput = writeBotOutput;
        this.kernel = new KernelBuilder().WithOpenAITextCompletionService(modelId, apiKey).Build();
    }

    public async Task Run()
    {
        string input = this.readUserInput();
        this.WriteToOutput("Ask", input);

        var semanticFunction = this.kernel.CreateSemanticFunction(this.Prompt, maxTokens: 100, temperature: 0.4, topP: 1);
        var context = this.kernel.CreateNewContext();
        context["input"] = input;
        context["options"] = this.AllowedOutputs;

        try
        {
            var result = await semanticFunction.InvokeAsync(context);
            this.WriteToOutput("Intention", result.ToString());
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
        this.writeBotOutput($"Allowed Outputs:{Environment.NewLine}\t{this.AllowedOutputs}{Constants.DoubleNewLines}");
    }
}

