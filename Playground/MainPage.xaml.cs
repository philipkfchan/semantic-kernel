// Copyright (c) Microsoft. All rights reserved.

namespace Playground;

public partial class MainPage : ContentPage
{
    private ChatCompletion chatCompletion;
    private TextCompletion textCompletion;
    private Intention intention;
    private string ApiKey
    {
        get => "";
    }
    private Func<string> readUserInput;
    private Action<string> writeBotOutput;
    private Action<string> changeGui;

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
        this.changeGui = reply =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                switch (reply.Trim().ToLower())
                {
                    case "redtrace":
                        this.blueTrace.IsVisible = false;
                        this.redTrace.IsVisible = true;
                        break;
                    case "bluetrace":
                        this.blueTrace.IsVisible = true;
                        this.redTrace.IsVisible = false;
                        break;
                    case "redbutton":
                        this.btn.BackgroundColor = Colors.Red;
                        break;
                    case "bluebutton":
                        this.btn.BackgroundColor = Colors.Blue;
                        break;
                    default:
                        throw new NotImplementedException($"case {reply} not implemented!");
                }
            });
        };

        this.chatCompletion = new ChatCompletion("gpt-3.5-turbo", this.ApiKey, "You are a professor", this.readUserInput, this.writeBotOutput);
        this.textCompletion = new TextCompletion("text-davinci-003", this.ApiKey, this.readUserInput, this.writeBotOutput);
        this.intention = new Intention("text-davinci-003", this.ApiKey, this.readUserInput, this.writeBotOutput, this.changeGui);
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

    private void OnClearBtnClicked(object sender, EventArgs e)
    {
        this.responseEditor.Text = string.Empty;
    }
}

