using SerialCommunication.Common.Enums;
using SerialCommunication.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SerialCommunication.Master
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SerialDevice serialDevice;

        private string serialDeviceId = string.Empty;

        private bool? isLedBlinking;
        private double hzBlinkingFrequency = 10.0f;

        private ObservableCollection<string> diagnosticData = new ObservableCollection<string>();
        private ObservableCollection<DeviceInformation> devicesList = new ObservableCollection<DeviceInformation>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await SetupConnection();
            }
            catch(Exception ex)
            {
                DiagnosticInfo.Display(diagnosticData, ex.Message);
            }
        }

        private async Task SetupConnection()
        {
            //CLose any previous connections before associating the new one
            CloseConnection();
            serialDevice = await SerialDevice.FromIdAsync(serialDeviceId);

            if(serialDevice != null)
            {
                //Configure connection
                SerialCommunicationHelper.SetDefaultConfiguration(serialDevice);
            }
        }

        private void CloseConnection()
        {
            if(serialDevice != null)
            {
                serialDevice.Dispose();
                serialDevice = null;
            }
        }

        private async void ButtonSendData_Click(object sender, RoutedEventArgs e)
        {
            await SendCommand(CommandId.BlinkingFrequency);
            await SendCommand(CommandId.BlinkingStatus);
        }
        private async Task SendCommand(CommandId commandId)
        {
            if(serialDevice != null)
            {
                byte[] command = null;
                switch (commandId)
                {
                    case CommandId.BlinkingFrequency:
                        command = CommandHelper.PrepareSetFrequencyCommand(hzBlinkingFrequency);
                        break;
                    case CommandId.BlinkingStatus:
                        command = CommandHelper.PrepareBlinkingStatusCommand(isLedBlinking);
                        break;
                }

                await SerialCommunicationHelper.WriteBytes(serialDevice, command);
                DiagnosticInfo.Display(diagnosticData, "Data written: " + CommandHelper.CommandToString(command));
            }
            else
            {
                DiagnosticInfo.Display(diagnosticData, "No  active connection");
            }
        }

        public static string CommandToString(byte[] commandData)
        {
            string commandString = string.Empty;
            if(commandData != null)
            {
                foreach(byte b in commandData)
                {
                    commandString += " " + b;
                }
            }
            return commandString.Trim();
        }

        public void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            diagnosticData.Clear();
        }
    }
}
