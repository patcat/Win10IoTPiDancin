using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Gpio;
using System.Diagnostics; // Contains Stopwatch

namespace ServoDancin
{
    public sealed partial class MainPage : Page
    {
        private const int SERVO_PIN_A = 18;
        private const int SERVO_PIN_B = 23;
        private GpioPin servoPinA;
        private GpioPin servoPinB;
        private DispatcherTimer timer;
        private double BEAT_PACE = 1000; // Switch side every second
        private double CounterClockwiseDanceMove = 1;
        private double ClockwiseDanceMove = 2;
        private double currentDirection;
        private double PulseFrequency = 20;
        Stopwatch stopwatch;

        public MainPage()
        {
            InitializeComponent();

            this.InitDancing();
        }

        private void InitDancing()
        {
            // Preparing our GPIO controller
            var gpio = GpioController.GetDefault();
            
            if (gpio == null)
            {
                servoPinA = null;
                if (GpioStatus != null)
                {
                    GpioStatus.Text = "No GPIO controller found";
                }

                return;
            }

            // Servo set up
            servoPinA = gpio.OpenPin(SERVO_PIN_A);
            servoPinA.SetDriveMode(GpioPinDriveMode.Output);

            servoPinB = gpio.OpenPin(SERVO_PIN_B);
            servoPinB.SetDriveMode(GpioPinDriveMode.Output);

            stopwatch = Stopwatch.StartNew();

            currentDirection = 0; // Initially we aren't dancing at all.

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(BEAT_PACE);
            timer.Tick += Beat;

            if (servoPinA != null && servoPinB != null)
            {
                timer.Start();
                Windows.System.Threading.ThreadPool.RunAsync(this.MotorThread, Windows.System.Threading.WorkItemPriority.High);
            }

            if (GpioStatus != null)
            {
                GpioStatus.Text = "GPIO pin ready";
            }
        }

        private void Beat(object sender, object e)
        {
            if (currentDirection != ClockwiseDanceMove)
            {
                currentDirection = ClockwiseDanceMove;
                GpioStatus.Text = "Yay!";
            }
            else
            {
                currentDirection = CounterClockwiseDanceMove;
                GpioStatus.Text = "Windows 10!";
            }
        }

        private void MotorThread(IAsyncAction action)
        {
            //This motor thread runs on a high priority task and loops forever to pulse the motor as determined by the drive buttons
            while (true)
            {
                if (currentDirection != 0)
                {
                    servoPinA.Write(GpioPinValue.High);
                    servoPinB.Write(GpioPinValue.High);
                }
                //Use the wait helper method to wait for the length of the pulse
                Wait(currentDirection);
                //The pulse if over and so set the pin to low and then wait until it's time for the next pulse
                servoPinA.Write(GpioPinValue.Low);
                servoPinB.Write(GpioPinValue.Low);
                Wait(PulseFrequency - currentDirection);
            }
        }

        private void Wait(double milliseconds)
        {
            long initialTick = stopwatch.ElapsedTicks;
            long initialElapsed = stopwatch.ElapsedMilliseconds;
            double desiredTicks = milliseconds / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {

            }
        }
    }
}
