using System;
using System.Threading;
using Windows.Devices.Gpio;

namespace SerialCommunication.Blinky
{
    internal class LedControl
    {
        public static double MinFrequency { get; } = 1;
        public static double MaxFrequency { get; } = 50;

        private const int ledPinNumber = 47;
        private const int hzDefaultBlinkFrequency = 5;

        private double hzBlinkFrequency = hzDefaultBlinkFrequency;

        private GpioPin ledGpioPin;

        private Timer timer;

        private TimeSpan timeSpanZero = TimeSpan.FromMilliseconds(0);

        public static bool IsValidFrequency(double hzFrequency)
        {
            return hzFrequency >= MinFrequency && hzFrequency <= MaxFrequency;
        }

        public LedControl()
        {
            ConfigureTimer();

            ConfigureGpioPin();

            Start();
        }

        public void Start()
        {
            timer.Change(timeSpanZero, HertzToTimeSpan(hzBlinkFrequency));
        }

        public void Stop()
        {
            timer.Change(Timeout.InfiniteTimeSpan, timeSpanZero);
        }

        public void Update(bool isBlinkingActive)
        {
            if(isBlinkingActive)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        public void SetFrequency(double hzBlinkFrequency)
        {
            if (IsValidFrequency(hzBlinkFrequency))
            {
                this.hzBlinkFrequency = hzBlinkFrequency;

                timer.Change(timeSpanZero, HertzToTimeSpan(hzBlinkFrequency));
            }
        }                

        private void ConfigureGpioPin()
        {
            var gpioController = GpioController.GetDefault();

            if (gpioController != null)
            {
                ledGpioPin = gpioController.OpenPin(ledPinNumber);

                if (ledGpioPin != null)
                {
                    ledGpioPin.SetDriveMode(GpioPinDriveMode.Output);
                    ledGpioPin.Write(GpioPinValue.Low);
                }
            }
        }        

        private void ConfigureTimer()
        {            
            var timerCallback = new TimerCallback((arg) => { BlinkLed(); });

            timer = new Timer(timerCallback, null, Timeout.InfiniteTimeSpan, HertzToTimeSpan(hzBlinkFrequency));            
        }

        private void BlinkLed()
        {
            GpioPinValue invertedGpioPinValue;

            var currentPinValue = ledGpioPin.Read();

            if (currentPinValue == GpioPinValue.High)
            {
                invertedGpioPinValue = GpioPinValue.Low;
            }
            else
            {
                invertedGpioPinValue = GpioPinValue.High;
            }

            ledGpioPin.Write(invertedGpioPinValue);
        }

        private static TimeSpan HertzToTimeSpan(double hzFrequency)
        {
            var msDelay = (int)Math.Floor(1000.0 / hzFrequency);

            return TimeSpan.FromMilliseconds(msDelay);
        }
    }
}
