// Copyright (c) Microsoft. All rights reserved.

namespace Playground;

public partial class MainPage : ContentPage
{
    private ChatCompletion chatCompletion;
    private TextCompletion textCompletion;
    private Intention intention;
    private string ApiKey
    {
        get => "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";
    }
    private Func<string> readUserInput;
    private Action<string> writeBotOutput;

    public MainPage()
    {
        this.InitializeComponent();
        this.modelPicker.SelectedItem = "chat";

        this.redTrace.IsVisible = false;

        this.readUserInput = () =>
        {
            string input = string.Empty;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                input = this.promptEditor.Text;
                this.promptEditor.Text = string.Empty;
            });
            return input;
        };
        this.writeBotOutput = x =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                this.responseEditor.Text += x;
            });
        };

        this.chatCompletion = new ChatCompletion("gpt-3.5-turbo", this.ApiKey, "You are a professor", this.readUserInput, this.writeBotOutput);
        this.textCompletion = new TextCompletion("text-davinci-003", this.ApiKey, this.readUserInput, this.writeBotOutput);
        this.intention = new Intention("text-davinci-003", this.ApiKey, this.readUserInput, this.writeBotOutput);
    }

    private async void OnSubmitBtnClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        this.DisableInput(button);
        var picker = (Picker)this.FindByName("modelPicker");
        switch (picker.SelectedItem)
        {
            case "chat":
                await this.chatCompletion.Run();
                break;
            case "text":
                await this.textCompletion.Run();
                break;
            case "intention":
                await this.intention.Run();
                break;
            case null:
                this.responseEditor.Text += $"Error: please pick a model{Constants.DoubleNewLines}";
                break;
            default:
                this.responseEditor.Text += $"Error: model {picker.SelectedItem} not implemented!{Constants.DoubleNewLines}";
                break;
        }
        this.EnableInput(button);
    }

    private void DisableInput(Button button)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            button.Text = "Loading..";
            button.IsEnabled = false;
        });
    }

    private void EnableInput(Button button)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            button.IsEnabled = true;
            button.Text = "Submit";
        });
    }

    private void modelPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Picker picker = (Picker)sender;
        if (picker.SelectedItem.ToString() == "text")
        {
            this.textCompletion.WritePromptToOutput();
        }
        else if (picker.SelectedItem.ToString() == "intention")
        {
            this.intention.WritePromptToOutput();
        }
    }
}

