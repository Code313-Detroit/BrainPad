﻿using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Pins;

namespace Drivers.Led
{
    public class LPD8806LEDStrip
    {
        private byte[] pixelData;
        private SpiController controller;
        private SpiDevice device;
        private byte basePixelValue = 0b1000_0000;

        public LPD8806LEDStrip(int pixelCount)
        {
            PixelCount = pixelCount;
            Setup(pixelCount);
        }

        /// <summary>
        /// The number of pixels.
        /// </summary>
        public int PixelCount { get; private set; }

        private void Setup(int ledCount)
        {
            this.pixelData = new byte[3 * ledCount + 3];

            this.controller = SpiController.FromName(BrainPad.Expansion.SpiBus.Spi1);
            var settings = new SpiConnectionSettings
            {
                Mode = SpiMode.Mode0,
                ClockFrequency = 100000,
                DataBitLength = 8,
                ChipSelectType = SpiChipSelectType.None,

            };

            this.device = this.controller.GetDevice(settings);
        }

        /// <summary>
        /// Sets the color of a pixel with the brightness.
        /// </summary>
        /// <param name="pixelNumber">The 1 based index of the pixel to set.</param>
        /// <param name="red">The red value. Should be between 0 and 127.</param>
        /// <param name="green">The green value. Should be between 0 and 127.</param>
        /// <param name="blue">The blue value. Should be between 0 and 127.</param>
        /// <param name="intensity">The brightness of the pixel. Should be between 0 and 5.</param>
        public void SetLedColor(int pixelNumber, int red, int green, int blue, int intensity)
        {
            SetLedColorWithIntensity(pixelNumber, (byte)red, (byte)green, (byte)blue, (byte)intensity);
        }

        /// <summary>
        /// Sets the color of a pixel with the brightness.
        /// </summary>
        /// <param name="pixelNumber">The 1 based index of the pixel to set.</param>
        /// <param name="color">The color to set the pixel.</param>
        /// <param name="intensity">The brightness of the pixel.</param>
        public void SetLedColor(int pixelNumber, Color color, Intensity intensity)
        {
            SetLedColorWithIntensity(pixelNumber, color.R, color.G, color.B, (byte)intensity);
        }

        /// <summary>
        /// Sets the color of a pixel with the brightness.
        /// </summary>
        /// <param name="pixelNumber">The 1 based index of the pixel to set.</param>
        /// <param name="red">The red value. Should be between 0 and 127.</param>
        /// <param name="green">The green value. Should be between 0 and 127.</param>
        /// <param name="blue">The blue value. Should be between 0 and 127.</param>
        /// <param name="intensity">The brightness of the pixel. Should be between 0 and 5.</param>
        public void SetLedColorWithIntensity(int pixelNumber, byte red, byte green, byte blue, byte intensity)
        {
            pixelNumber *= 3;

            byte greenLocal = (byte)(green >> intensity);
            byte redLocal = (byte)(red >> intensity);
            byte blueLocal = (byte)(blue >> intensity);

            pixelData[pixelNumber] = (byte)(basePixelValue | greenLocal);
            pixelData[pixelNumber + 1] = (byte)(basePixelValue | redLocal);
            pixelData[pixelNumber + 2] = (byte)(basePixelValue | blueLocal);
        }

        /// <summary>
        /// Writes the data to the LED strip.
        /// </summary>
        public void WriteData()
        {
            device.Write(pixelData);
        }

        /// <summary>
        /// Turn off all the LEDs.
        /// </summary>
        public void ClearAll()
        {
            for (int i = 0; i < PixelCount ; i++)
            {
                ClearSingle(i);
            }
            WriteData();
        }

        /// <summary>
        /// Turns off a single pixel.
        /// </summary>
        /// <param name="pixelNumber"></param>
        public void ClearSingle(int pixelNumber)
        {
            if (pixelNumber >= 0 && pixelNumber <= PixelCount)
            {
                SetLedColor(pixelNumber, Color.Black, Intensity.Highest);
            }
        }

        public void FlashAllLights(int red, int green, int blue, int intensity, int cycles)
        {
            for (int i = 0; i < cycles; i++)
            {
                Thread.Sleep(100);
                for (int j = 0; j < PixelCount; j++)
                {
                    SetLedColor(j, red, green, blue, intensity);
                }
                WriteData();
                Thread.Sleep(100);
                ClearAll();
            }
        }
    }
}
