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

        public int SpeedLevel { get; private set; } = 1;

        public bool IsGameInProgress { get; private set; }

        public int CurrentPosition { get; private set; }

        public void StartUp()
        {
            buttons.WhenUpButtonReleased += UpButtonReleasedHandler;
            buttons.WhenDownButtonReleased += DownButtonReleasedHandler;
            buttons.WhenRightButtonReleased += RightButtonReleasedHandler;
            buttons.WhenLeftButtonReleased += LeftButtonReleasedHandler;
            ledStrip.ClearAll();
            UpdateDisplay();
        }

        private void LeftButtonReleasedHandler()
        {
            if (IsGameInProgress && CurrentPosition < ledStrip.PixelCount)
            {
                StopGame();
                StopTimer();
                ledStrip.FlashAllLights(0,100,0, 2, 20);
                Reset();
            }
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
            //if (CurrentPosition > 0)
            //{
            //    ledStrip.ClearSingle(CurrentPosition);
            //}


            ledStrip.SetLedColor(CurrentPosition, 100, 0, 0, 2);
            ledStrip.WriteData();
            CurrentPosition += 1;
        }

        /// <summary>
        /// Checks the current status of the game.
        /// </summary>
        private void CheckGameStatus()
        {
            if (!IsGameInProgress)
            {
                return;
            }

            if ((CurrentPosition + 1) > ledStrip.PixelCount)
            {
                StopGame();
                Reset();
                PlayLosingSound();
                ledStrip.FlashAllLights(100, 0, 0, 2, 20);
                return;
            }

            AdvanceForward();
        }

        private void PlayLosingSound()
        {
            buzzer.StartBuzzing(100);
            Thread.Sleep(1000);
            buzzer.StopBuzzing();
        }

        private void StopGame()
        {
            IsGameInProgress = false;
        }

        private void StopTimer()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        private void Reset()
        {
            StopTimer();
            CurrentPosition = 0;
            ledStrip.ClearAll();
        }

        private TimeSpan GetMotionInterval()
        {
            double intervalInMilliseconds = startingInterval - (Math.Log(SpeedLevel) * SpeedLevel * .4);
            return TimeSpan.FromMilliseconds(intervalInMilliseconds);
        }

        /// <summary>
        /// Down button pressed and released.
        /// </summary>
        private void DownButtonReleasedHandler()
        {
            if (IsGameInProgress)
            {
                return;
            }

            //decrease the speed, 1 is the lowest level
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
            if (IsGameInProgress)
            {
                return;
            }

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
            StopGame();
            Reset();
            //cleanup the button handlers
            buttons.WhenUpButtonReleased -= UpButtonReleasedHandler;
            buttons.WhenDownButtonReleased -= DownButtonReleasedHandler;
            buttons.WhenRightButtonReleased -= RightButtonReleasedHandler;
            buttons.WhenLeftButtonReleased -= LeftButtonReleasedHandler;
        }

        /// <summary>
        /// Show the data on the display.
        /// </summary>
        private void UpdateDisplay()
        {
            display.Clear();
            //update the speed level on the display
            display.DrawNumber(0, 0, SpeedLevel);

            display.RefreshScreen();
        }
    }
}
