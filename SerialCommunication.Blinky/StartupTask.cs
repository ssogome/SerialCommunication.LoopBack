using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.SerialCommunication;
using SerialCommunication.Common.Helpers;
using System.Threading.Tasks;
using SerialCommunication.Common.Enums;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace SerialCommunication.Blinky
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral taskDeferral;
        private LedControl ledControl;
        private SerialDevice serialDevice;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //
            taskDeferral = taskInstance.GetDeferral();

            ledControl = new LedControl();

            await SetupCommunication();
        }

        private async Task SetupCommunication()
        {
            serialDevice = await SerialCommunicationHelper.GetFirstDeviceAvailable();
            SerialCommunicationHelper.SetDefaultConfiguration(serialDevice);
            new Task(CommunicationListener).Start();
        }

        private async void CommunicationListener()
        {
            while (true)
            {
                var commandReceived = await SerialCommunicationHelper.ReadBytes(serialDevice);
                try
                {
                    ParseCommand(commandReceived);
                }
                catch(Exception ex)
                {
                    DiagnosticInfo.Display(null, ex.Message);
                }
            }
        }

        private void ParseCommand(byte[] command)
        {
            var errorCode = CommandHelper.VerifyCommand(command);
            if(errorCode == Common.Enums.ErrorCode.OK)
            {
                var commandId = (CommandId)command[CommandHelper.CommandIdIndex];
                switch (commandId)
                {
                    case CommandId.BlinkingFrequency:
                        HandleBlinkingFrequencyCommand(command);
                        break;
                    case CommandId.BlinkingStatus:
                        HandleBlinkingStatusCommand(command);
                        break;
                }
            }
        }

        private void HandleBlinkingFrequencyCommand(byte[] command)
        {
            var frequency = BitConverter.ToDouble(command, CommandHelper.CommandDataBeginIndex);
            ledControl.SetFrequency(frequency);
        }

        private void HandleBlinkingStatusCommand(byte[] command)
        {
            var isBlinking = Convert.ToBoolean(command[CommandHelper.CommandDataBeginIndex]);

            ledControl.Update(isBlinking);
        }
    }
}
