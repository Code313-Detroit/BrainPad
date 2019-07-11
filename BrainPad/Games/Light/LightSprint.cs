using Drivers.Led;
using GHIElectronics.TinyCLR.BrainPad;
using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Games.Light
{
    public class LightSprint
    {
        private readonly LPD8806LEDStrip ledStrip;
        private readonly Buttons buttons;
        private readonly Display display;
        private readonly Buzzer buzzer;
        private Timer timer;
        private double startingInterval = 50;

        public LightSprint(int pixelCount)
        {
            this.ledStrip = new LPD8806LEDStrip(pixelCount);
            this.buttons = new Buttons();
            this.display = new Display();
            this.buzzer = new Buzzer();
        }

        public int SpeedLevel { get; private set; }

        public bool IsGameInProgress { get; private set; }

        public int CurrentPosition { get; private set; }

        public void StartUp()
        {
            buttons.WhenUpButtonReleased += UpButtonReleasedHandler;
            buttons.WhenDownButtonReleased += DownButtonReleasedHandler;
            buttons.WhenRightButtonReleased += RightButtonReleasedHandler;
            ledStrip.ClearAll();
        }

        /// <summary>
        /// Right button pressed and released.
        /// </summary>
        private void RightButtonReleasedHandler()
        {
            if (!IsGameInProgress)
            {
                IsGameInProgress = true;
                UpdateDisplay();
                if (timer == null)
                {
                    TimeSpan interval = GetMotionInterval();
                    this.timer = new Timer(TimerTick, null, interval, interval);
                }
                
            }
        }


        private void TimerTick(Object state)
        {
            CheckGameStatus();
        }

        private void AdvanceForward()
        {
            ledStrip.ClearSingle(CurrentPosition);
            CurrentPosition += 1;
            ledStrip.SetLedColor(CurrentPosition, Color.Green, Intensity.MediumHigh);
        }

        /// <summary>
        /// Checks the current status of the game.
        /// </summary>
        private void CheckGameStatus()
        {
            if ((CurrentPosition + 1) > ledStrip.PixelCount)
            {
                CleanUp();
                PlayLosingSound();
                return;
            }

            AdvanceForward();
        }

        private void PlayWinningSound()
        {
            buzzer.StartBuzzing(500);
            Thread.Sleep(1000);
            buzzer.StopBuzzing();
        }

        private void PlayLosingSound()
        {
            buzzer.StartBuzzing(100);
            Thread.Sleep(1000);
            buzzer.StopBuzzing();
        }

        private void CleanUp()
        {
            IsGameInProgress = false;
            this.timer.Dispose();
            ledStrip.ClearAll();
        }

        private TimeSpan GetMotionInterval()
        {
            double intervalInMilliseconds = startingInterval - (Math.Log(SpeedLevel) * SpeedLevel * .4);
            return TimeSpan.FromMilliseconds(intervalInMilliseconds);
        }

        private void SetLightsInMotion()
        {
            ledStrip.ClearAll();

            for (int ledIndex = 0; ledIndex <= ledStrip.PixelCount; ledIndex++)
            {

            }
        }

        /// <summary>
        /// Down button pressed and released.
        /// </summary>
        private void DownButtonReleasedHandler()
        {
            //decrease the speed, 0 is the lowest level
            if (SpeedLevel > 1)
            {
                SpeedLevel -= 1;
            }
            
            UpdateDisplay();
        }

        /// <summary>
        /// Up button is pressed and released.
        /// </summary>
        private void UpButtonReleasedHandler()
        {
            //increase the speed
            if (SpeedLevel < 35)
            {
                SpeedLevel += 1;
            }
            
            UpdateDisplay();
        }

        /// <summary>
        /// Stop the game.
        /// </summary>
        public void Exit()
        {
            CleanUp();
            //cleanup the button handlers
            buttons.WhenUpButtonReleased -= UpButtonReleasedHandler;
            buttons.WhenDownButtonReleased -= DownButtonReleasedHandler;
            buttons.WhenRightButtonReleased -= RightButtonReleasedHandler;
        }

        /// <summary>
        /// Show the data on the display.
        /// </summary>
        private void UpdateDisplay()
        {
            //update the speed level on the display
            display.DrawScaledText(0, 0, SpeedLevel.ToString(), 100, 100);
        }
    }
}
