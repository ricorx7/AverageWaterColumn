/*
 * Copyright © 2011 
 * Rowe Technology Inc.
 * All rights reserved.
 * http://www.rowetechinc.com
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification is NOT permitted.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE 
 * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 * 
 * HISTORY
 * -----------------------------------------------------------------
 * Date            Initials    Version    Comments
 * -----------------------------------------------------------------
 * 03/12/2012      RC          2.06       Initial coding
 * 03/11/2013      RC          2.18       Added ColormapBrushArray so the array would not have to be regenerated for each draw.
 * 03/12/2013      RC          2.18       Added GetColormapColor() to get the color based off a value given and the min and max of the colormap.
 *       
 * 
 */

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace RTI
{
    /// <summary>
    /// All the different types of color 
    /// schemes.
    /// </summary>
    public class ColormapBrush
    {
        #region Variables

        /// <summary>
        /// All possible types of Colormaps.
        /// </summary>
        public enum ColormapBrushEnum
        {
            /// <summary>
            /// Spring colormap.
            /// </summary>
            Spring = 0,

            /// <summary>
            /// Summer colormap.
            /// </summary>
            Summer = 1,

            /// <summary>
            /// Autumn colormap.
            /// </summary>
            Autumn = 2,

            /// <summary>
            /// Winter colormap.
            /// </summary>
            Winter = 3,

            /// <summary>
            /// Gray colormap.
            /// </summary>
            Gray = 4,

            /// <summary>
            /// Jet colormap.
            /// </summary>
            Jet = 5,

            /// <summary>
            /// Hot colormap.
            /// </summary>
            Hot = 6,

            /// <summary>
            /// Cool colormap.
            /// </summary>
            Cool = 7
        }


        /// <summary>
        /// Length of a colormap.
        /// </summary>
        private int colormapLength = 100;

        /// <summary>
        /// Translucenty value.
        /// </summary>
        private byte alphaValue = 255;

        /// <summary>
        /// Minimum Y value.
        /// </summary>
        private double ymin = 0;

        /// <summary>
        /// Maximum Y value.
        /// </summary>
        private double ymax = 10;

        /// <summary>
        /// Y divisions.
        /// </summary>
        private int ydivisions = 10;

        /// <summary>
        /// Colormap chosen.
        /// Default is Jet.
        /// </summary>
        private ColormapBrushEnum colormapBrushType = ColormapBrushEnum.Jet;

        #endregion

        #region Properties

        /// <summary>
        /// Get and set the colormap.
        /// </summary>
        public ColormapBrushEnum ColormapBrushType
        {
            get { return colormapBrushType; }
            set 
            { 
                colormapBrushType = value;
                ColormapBrushArray = ColormapBrushes();
            }
        }

        /// <summary>
        /// Array of all the colormaps.
        /// </summary>
        private SolidColorBrush[] colormapBrushArray;
        /// <summary>
        /// Array of all the colormaps.
        /// </summary>
        public SolidColorBrush[] ColormapBrushArray
        {
            get { return colormapBrushArray; }
            set
            {
                colormapBrushArray = value;
            }
        }

        /// <summary>
        /// Get and set the color map length.
        /// </summary>
        public int ColormapLength
        {
            get { return colormapLength; }
            set 
            {
                if (value != colormapLength)
                {
                    colormapLength = value;
                    ColormapBrushArray = ColormapBrushes();
                }
            }
        }

        /// <summary>
        /// Get and set the Alpha (transparency)
        /// value.
        /// </summary>
        public byte AlphaValue
        {
            get { return alphaValue; }
            set { alphaValue = value; }
        }

        /// <summary>
        /// Get and set the Y minimum.
        /// </summary>
        public double Ymin
        {
            get { return ymin; }
            set { ymin = value; }
        }

        /// <summary>
        /// Get and set the Y maximum.
        /// </summary>
        public double Ymax
        {
            get { return ymax; }
            set { ymax = value; }
        }

        /// <summary>
        /// Get and set the Y division.
        /// </summary>
        public int Ydivisions
        {
            get { return ydivisions; }
            set 
            {
                if (value != ydivisions)
                {
                    ydivisions = value;
                    ColormapBrushArray = ColormapBrushes();
                }
            }
        }

        #endregion

        /// <summary>
        /// Constructor does nothing.
        /// </summary>
        public ColormapBrush()
        {

        }

        /// <summary>
        /// Create the different Colormaps.  Store
        /// them to an array.
        /// </summary>
        /// <returns>Array of colormaps.</returns>
        public SolidColorBrush[] ColormapBrushes()
        {
            byte[,] cmap = new byte[ColormapLength, 4];
            double[] array = new double[ColormapLength];

            switch (ColormapBrushType)
            {
                case ColormapBrushEnum.Spring:
                    for (int i = 0; i < ColormapLength; i++)
                    {
                        array[i] = 1.0 * i / (ColormapLength - 1);
                        cmap[i, 0] = AlphaValue;
                        cmap[i, 1] = 255;
                        cmap[i, 2] = (byte)(255 * array[i]);
                        cmap[i, 3] = (byte)(255 - cmap[i, 2]);
                    }
                    break;

                case ColormapBrushEnum.Summer:
                    for (int i = 0; i < ColormapLength; i++)
                    {
                        array[i] = 1.0 * i / (ColormapLength - 1);
                        cmap[i, 0] = AlphaValue;
                        cmap[i, 1] = (byte)(255 * array[i]);
                        cmap[i, 2] = (byte)(255 * 0.5 * (1 + array[i]));
                        cmap[i, 3] = (byte)(255 * 0.4);
                    }
                    break;

                case ColormapBrushEnum.Autumn:
                    for (int i = 0; i < ColormapLength; i++)
                    {
                        array[i] = 1.0 * i / (ColormapLength - 1);
                        cmap[i, 0] = AlphaValue;
                        cmap[i, 1] = 255;
                        cmap[i, 2] = (byte)(255 * array[i]);
                        cmap[i, 3] = 0;
                    }
                    break;

                case ColormapBrushEnum.Winter:
                    for (int i = 0; i < ColormapLength; i++)
                    {
                        array[i] = 1.0 * i / (ColormapLength - 1);
                        cmap[i, 0] = AlphaValue;
                        cmap[i, 1] = 0;
                        cmap[i, 2] = (byte)(255 * array[i]);
                        cmap[i, 3] = (byte)(255 * (1.0 - 0.5 * array[i]));
                    }
                    break;

                case ColormapBrushEnum.Gray:
                    for (int i = 0; i < ColormapLength; i++)
                    {
                        array[i] = 1.0 * i / (ColormapLength - 1);
                        cmap[i, 0] = AlphaValue;
                        cmap[i, 1] = (byte)(255 * array[i]);
                        cmap[i, 2] = (byte)(255 * array[i]);
                        cmap[i, 3] = (byte)(255 * array[i]);
                    }
                    break;

                case ColormapBrushEnum.Jet:
                    int n = (int)Math.Ceiling(ColormapLength / 4.0);
                    double[,] cMatrix = new double[ColormapLength, 3];
                    int nMod = 0;
                    double[] array1 = new double[3 * n - 1];
                    int[] red = new int[array1.Length];
                    int[] green = new int[array1.Length];
                    int[] blue = new int[array1.Length];

                    if (ColormapLength % 4 == 1)
                        nMod = 1;

                    for (int i = 0; i < array1.Length; i++)
                    {
                        if (i < n)
                            array1[i] = (i + 1.0) / n;
                        else if (i >= n && i < 2 * n - 1)
                            array1[i] = 1.0;
                        else if (i >= 2 * n - 1)
                            array1[i] = (3.0 * n - 1.0 - i) / n;
                        green[i] = (int)Math.Ceiling(n / 2.0) - nMod + i;
                        red[i] = green[i] + n;
                        blue[i] = green[i] - n;
                    }

                    int nb = 0;
                    for (int i = 0; i < blue.Length; i++)
                    {
                        if (blue[i] > 0)
                            nb++;
                    }

                    for (int i = 0; i < ColormapLength; i++)
                    {
                        for (int j = 0; j < red.Length; j++)
                        {
                            if (i == red[j] && red[j] < ColormapLength)
                                cMatrix[i, 0] = array1[i - red[0]];
                        }
                        for (int j = 0; j < green.Length; j++)
                        {
                            if (i == green[j] && green[j] < ColormapLength)
                                cMatrix[i, 1] = array1[i - green[0]];
                        }
                        for (int j = 0; j < blue.Length; j++)
                        {
                            if (i == blue[j] && blue[j] >= 0)
                                cMatrix[i, 2] = array1[array1.Length - 1 - nb + i];
                        }
                    }

                    for (int i = 0; i < ColormapLength; i++)
                    {
                        cmap[i, 0] = AlphaValue;
                        for (int j = 0; j < 3; j++)
                            cmap[i, j + 1] = (byte)(cMatrix[i, j] * 255);
                    }
                    break;

                case ColormapBrushEnum.Hot:
                    int n1 = (int)3 * ColormapLength / 8;
                    double[] red1 = new double[ColormapLength];
                    double[] green1 = new double[ColormapLength];
                    double[] blue1 = new double[ColormapLength];
                    for (int i = 0; i < ColormapLength; i++)
                    {
                        if (i < n1)
                            red1[i] = 1.0 * (i + 1.0) / n1;
                        else
                            red1[i] = 1.0;
                        if (i < n1)
                            green1[i] = 0.0;
                        else if (i >= n1 && i < 2 * n1)
                            green1[i] = 1.0 * (i + 1 - n1) / n1;
                        else
                            green1[i] = 1.0;
                        if (i < 2 * n1)
                            blue1[i] = 0.0;
                        else
                            blue1[i] = 1.0 * (i + 1 - 2 * n1) / (ColormapLength - 2 * n1);

                        cmap[i, 0] = AlphaValue;
                        cmap[i, 1] = (byte)(255 * red1[i]);
                        cmap[i, 2] = (byte)(255 * green1[i]);
                        cmap[i, 3] = (byte)(255 * blue1[i]);
                    }                   
                    break;

                case ColormapBrushEnum.Cool:
                    for (int i = 0; i < ColormapLength; i++)
                    {
                        array[i] = 1.0 * i / (ColormapLength - 1);
                        cmap[i, 0] = AlphaValue;
                        cmap[i, 1] = (byte)(255 * array[i]);
                        cmap[i, 2] = (byte)(255 * (1 - array[i]));
                        cmap[i, 3] = 255;
                    }
                    break;
            }
            return SetBrush(cmap);
        }

        /// <summary>
        /// Get the color from the color map based off the value given
        /// and the min and max for the colormap.  This will first set the
        /// min and max value for the colormap if it has changed.  It will then
        /// set the resolution of the colormap.
        /// 
        /// It will then calculate where on the colormap the value givn is located
        /// and get the color value from the colormap array.  It will then set the
        /// color to the SolidColorBrush and return the brush.
        /// </summary>
        /// <param name="value">Value to determine the color.</param>
        /// <param name="valueMin">Minimum value for the colormap.</param>
        /// <param name="valueMax">Maximum value for the colormap.</param>
        /// <returns></returns>
        public SolidColorBrush GetColormapColor(double value, double valueMin, double valueMax)
        {
            SolidColorBrush brush = new SolidColorBrush();

            // Set the range for the colormap values
            // Set the min value for the colormap
            if (Ymin != valueMin)
            {
                Ymin = valueMin;
            }

            // Set the max value for the colormap
            if (Ymax != valueMax)
            {
                Ymax = valueMax;
            }

            // Set the resolution of the colormap
            if (Ydivisions != ColormapLength)
            {
                Ydivisions = ColormapLength;
            }

            // Determine where in the colormap the color is located
            int colorIndex = (int)(((ColormapLength - 1) * (value - valueMin) + valueMax - value) / (valueMax - valueMin));
            
            // Verify the the index is good
            if (colorIndex < 0)
            {
                colorIndex = 0;
            }
            if (colorIndex >= ColormapLength)
            {
                colorIndex = ColormapLength - 1;
            }

            // Set the brush color
            brush = ColormapBrushArray[colorIndex];

            // Freeze the brush for performance
            brush.Freeze();

            return brush;
        }

        /// <summary>
        /// Create the a brush based off the enum given and the minimum and maximum value.
        /// The brush will be created, then frozen and returned.
        /// Spectrum of the color is 0.0 to 1.0.
        /// 
        /// I added black to the begining of some some, ranging from 0.0-0.01 to 0.0 - 0.1 
        /// to display black for bad velocity with a velocity of 0.  
        /// </summary>
        /// <param name="brushEnum">Brush chosen.</param>
        /// <param name="alphaValue">The transparency of the display.</param>
        /// <returns>Brush for the given enum.</returns>
        public static Brush GetBrush(ColormapBrush.ColormapBrushEnum brushEnum, byte alphaValue = 255)
        {
            // Determine which brush is being selected
            // Then create the brush and return
            switch (brushEnum)
            {
                case ColormapBrush.ColormapBrushEnum.Spring:
                    LinearGradientBrush springBrush = new LinearGradientBrush();
                    springBrush.StartPoint = new Point(0, 0.5);                                                         // Creates a horizontal linear gradient instead of it looking diagonal
                    springBrush.EndPoint = new Point(1, 0.5);
                    springBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));                                 // Add Black for bad Velocity (0 Velocity)
                    springBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 255, 0, 255), 0.1));      // R = 255     G = 255*0 = 0        B = 255-G = 255
                    springBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 255, 255, 0), 1.0));      // R = 255     G = 255*1 = 255      B = 255-G = 0
                    springBrush.Freeze();
                    return springBrush;

                case ColormapBrush.ColormapBrushEnum.Summer:
                    LinearGradientBrush summerBrush = new LinearGradientBrush();
                    summerBrush.StartPoint = new Point(0, 0.5);                                                         // Creates a horizontal linear gradient instead of it looking diagonal
                    summerBrush.EndPoint = new Point(1, 0.5);
                    summerBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));                                 // Add Black for bad Velocity (0 Velocity)
                    summerBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 0, 128, 102), 0.01));     // R = 255*0 = 0        G = 255 * 0.5 * (1 + 0) = 127.5     B = 255*0.4 = 102
                    summerBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 255, 255, 102), 1.0));    // R = 255*1 = 255      G = 255 * 0.5 * (1 + 1) = 255       B = 255*0.4 = 102
                    summerBrush.Freeze();
                    return summerBrush;

                case ColormapBrush.ColormapBrushEnum.Autumn:
                    LinearGradientBrush autumnBrush = new LinearGradientBrush();
                    autumnBrush.StartPoint = new Point(0, 0.5);                                                         // Creates a horizontal linear gradient instead of it looking diagonal
                    autumnBrush.EndPoint = new Point(1, 0.5);
                    autumnBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));                                 // Add Black for bad Velocity (0 Velocity)
                    autumnBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 255, 0, 0), 0.1));        // R = 255     G = 255*0 = 0      B = 0
                    autumnBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 255, 255, 0), 1.0));      // R = 255     G = 255*1 = 255    B = 0
                    autumnBrush.Freeze();
                    return autumnBrush;

                case ColormapBrush.ColormapBrushEnum.Winter:
                    LinearGradientBrush winterBrush = new LinearGradientBrush();
                    winterBrush.StartPoint = new Point(0, 0.5);                                                         // Creates a horizontal linear gradient instead of it looking diagonal
                    winterBrush.EndPoint = new Point(1, 0.5);
                    winterBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));                                 // Add Black for bad Velocity (0 Velocity)
                    winterBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 0, 0, 255), 0.01));       // R = 0        G = 255*0 = 0       B = 255 * (1.0 - (0.5 * 0)) = 255
                    winterBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 0, 255, 128), 1.0));      // R = 0        G = 255*1 = 255     B = 255 * (1.0 - (0.5 * 1)) = 128
                    winterBrush.Freeze();
                    return winterBrush;

                case ColormapBrush.ColormapBrushEnum.Gray:
                    LinearGradientBrush grayBrush = new LinearGradientBrush();
                    grayBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 0, 0, 0), 0.0));            // R = 255*0 = 0        G = 255*0 = 0       B = 255*0 = 0
                    grayBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 255, 255, 255), 1.0));      // R = 255*1 = 255      G = 255*1 = 255     B = 255*1 = 255
                    grayBrush.Freeze();
                    return grayBrush;

                case ColormapBrush.ColormapBrushEnum.Hot:
                    LinearGradientBrush hotBrush = new LinearGradientBrush();
                    hotBrush.StartPoint = new Point(0, 0.5);                                                            // Creates a horizontal linear gradient instead of it looking diagonal
                    hotBrush.EndPoint = new Point(1, 0.5);
                    hotBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
                    hotBrush.GradientStops.Add(new GradientStop(Colors.Red, 0.25));
                    hotBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.50));
                    hotBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.75));
                    hotBrush.GradientStops.Add(new GradientStop(Colors.White, 1.0));
                    hotBrush.Freeze();
                    return hotBrush;

                case ColormapBrush.ColormapBrushEnum.Cool:
                    LinearGradientBrush coolBrush = new LinearGradientBrush();
                    coolBrush.StartPoint = new Point(0, 0.5);                                                           // Creates a horizontal linear gradient instead of it looking diagonal
                    coolBrush.EndPoint = new Point(1, 0.5);
                    coolBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));                                   // Add Black for bad Velocity (0 Velocity)
                    coolBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 0, 255, 255), 0.1));        // R = 255*0 = 0        G = 255 * (1.0 - 0) = 255       B = 255
                    coolBrush.GradientStops.Add(new GradientStop(Color.FromArgb(alphaValue, 255, 0, 255), 1.0));        // R = 255*1 = 255      G = 255 * (1.0 - 1) = 0         B = 255
                    coolBrush.Freeze();
                    return coolBrush;

                case ColormapBrush.ColormapBrushEnum.Jet:
                    LinearGradientBrush jetBrush = new LinearGradientBrush();
                    jetBrush.StartPoint = new Point(0, 0.5);                                                            // Creates a horizontal linear gradient instead of it looking diagonal
                    jetBrush.EndPoint = new Point(1, 0.5);
                    jetBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));                                    // Add Black for bad Velocity (0 Velocity)
                    jetBrush.GradientStops.Add(new GradientStop(Colors.DarkBlue, 0.01));
                    jetBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0.125));
                    jetBrush.GradientStops.Add(new GradientStop(Colors.Cyan, 0.375));
                    jetBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.625));
                    jetBrush.GradientStops.Add(new GradientStop(Colors.Red, 0.875));
                    jetBrush.GradientStops.Add(new GradientStop(Colors.DarkRed, 1.0));
                    jetBrush.Freeze();
                    return jetBrush;

                default:
                    SolidColorBrush defaultBrush = new SolidColorBrush(Colors.YellowGreen);
                    defaultBrush.Freeze();
                    return defaultBrush;
            }
        }

        /// <summary>
        /// List of all the possible color options.
        /// </summary>
        /// <returns>List of all the possible color options.</returns>
        public static List<ColormapBrush.ColormapBrushEnum> GetColormapList()
        {
            List<ColormapBrushEnum> list = new List<ColormapBrushEnum>();
            list.Add(ColormapBrush.ColormapBrushEnum.Spring);
            list.Add(ColormapBrush.ColormapBrushEnum.Summer);
            list.Add(ColormapBrush.ColormapBrushEnum.Autumn);
            list.Add(ColormapBrush.ColormapBrushEnum.Winter);
            list.Add(ColormapBrush.ColormapBrushEnum.Gray);
            list.Add(ColormapBrush.ColormapBrushEnum.Hot);
            list.Add(ColormapBrush.ColormapBrushEnum.Jet);
            list.Add(ColormapBrush.ColormapBrushEnum.Cool);

            return list;
        }

        /// <summary>
        /// Create an array of SolidColorBrushes
        /// used to color objects based off the colormaps.
        /// </summary>
        /// <param name="cmap">Colormaps.</param>
        /// <returns>Array of SolidColorBrushes.</returns>
        private SolidColorBrush[] SetBrush(byte[,] cmap)
        {
            SolidColorBrush[] brushes = new SolidColorBrush[Ydivisions];
            double dy = (Ymax - Ymin) / (Ydivisions - 1);
            for (int i = 0; i < Ydivisions; i++)
            {
                int colorIndex = (int)((ColormapLength - 1) * i * dy / (Ymax - Ymin));
                if (colorIndex < 0)
                    colorIndex = 0;
                if (colorIndex >= ColormapLength)
                    colorIndex = ColormapLength - 1;
                brushes[i] = new SolidColorBrush(Color.FromArgb(cmap[colorIndex, 0], 
                                                                cmap[colorIndex, 1], 
                                                                cmap[colorIndex, 2], 
                                                                cmap[colorIndex, 3]));
                // Freeze the brush for performance
                brushes[i].Freeze();
            }
            return brushes;
        }
    }
}
