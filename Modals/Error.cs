using Windows.UI.Popups;
using Windows.UI.Xaml;
using System;

namespace BTRReportProcesser.Modals
{
    class Error
    {
        public static async void ErrorOkay(string message)
        {
            MessageDialog messageDialog = new MessageDialog(message);
            messageDialog.Commands.Add(new UICommand("Exit"));
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 0;

            await messageDialog.ShowAsync();
        }
    }
}
