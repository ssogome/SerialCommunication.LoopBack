using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SerialCommunication.Common;
using SerialCommunication.Common.Helpers;

namespace SerialCommunication.LoopBack
{
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<string> diagnosticData = new ObservableCollection<string>();

        private const string testMessage = "UART transfer";

        private SerialDevice serialDevice;

        private DataWriter dataWriter;
        private DataReader dataReader;

        public MainPage()
        {
            InitializeComponent();

            ButtonPerformTest_Click(null, null);
        }

        private async void ButtonPerformTest_Click(object sender, RoutedEventArgs e)
        {
            await InitializeDevice();

            await WriteTestMessage();

            await ReadTestMessage();
        }

        private async Task InitializeDevice()
        {
            if (serialDevice == null)
            {
                serialDevice = await SerialCommunicationHelper.GetFirstDeviceAvailable();

                SerialCommunicationHelper.SetDefaultConfiguration(serialDevice);

                if (serialDevice != null)
                {
                    dataWriter = new DataWriter(serialDevice.OutputStream);
                    dataReader = new DataReader(serialDevice.InputStream);
                }
            }
        }

        private async Task WriteTestMessage()
        {
            if (dataWriter != null)
            {
                dataWriter.WriteString(testMessage);

                var bytesWritten = await dataWriter.StoreAsync();

                DiagnosticInfo.Display(diagnosticData, "Test message written successfully. Bytes written: " + bytesWritten);
            }
            else
            {
                DiagnosticInfo.Display(diagnosticData, "Data writer has been not initialized");
            }
        }

        private async Task ReadTestMessage()
        {
            if (dataReader != null)
            {
                var stringLength = dataWriter.MeasureString(testMessage);

                await dataReader.LoadAsync(stringLength);

                var messageReceived = dataReader.ReadString(dataReader.UnconsumedBufferLength);

                DiagnosticInfo.Display(diagnosticData, "Message received: " + messageReceived);
            }
            else
            {
                DiagnosticInfo.Display(diagnosticData, "Data reader has been not initialized");
            }
        }

        private void ButtonClearList_Click(object sender, RoutedEventArgs e)
        {
            diagnosticData.Clear();
        }
    }
}
