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
 *       
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ColormapHelper
    {

        /// <summary>
        /// Default color to use when no color or a
        /// bad value is given.
        /// </summary>
        public static SolidColorBrush DEFAULT_EMPTY_COLOR = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// Given a min and max value.  Create
        /// a value between 0 and 1 to describe 
        /// the given value between the min an max value.
        /// </summary>
        /// <param name="value">Value to normalize.</param>
        /// <param name="minValue">Minimum value in the range.</param>
        /// <param name="maxValue">Maximum value in the range.</param>
        /// <returns>Normalized value.</returns>
        public static double NormializeValue(double value, double minValue, double maxValue)
        {
            // If the value is bad, it will never be within the range
            if (value == DataSet.Ensemble.BAD_VELOCITY)
            {
                return 2.0;
            }

            double result = (value - minValue) / (maxValue - minValue);

            return result;
        }

        /// <summary>
        /// Unnormalize the value.
        /// Return back to the orignal value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minValue">Minimum value in the range.</param>
        /// <param name="maxValue">Maximum value in the range.</param>
        /// <returns></returns>
        public static double DeNormializeValue(double value, double minValue, double maxValue)
        {
            return ((minValue - maxValue) * value - maxValue * minValue + maxValue * minValue) / (minValue - maxValue);
        }

        /// <summary>
        /// Create a list of all possible color schemes.
        /// <returns>Return a list of all the possible colormaps.</returns>
        /// </summary>
        public static List<ColormapBrush.ColormapBrushEnum> GetColorList()
        {
            List<ColormapBrush.ColormapBrushEnum>  colormapList = new List<ColormapBrush.ColormapBrushEnum>();
            colormapList.Add(ColormapBrush.ColormapBrushEnum.Autumn);
            colormapList.Add(ColormapBrush.ColormapBrushEnum.Cool);
            colormapList.Add(ColormapBrush.ColormapBrushEnum.Gray);
            colormapList.Add(ColormapBrush.ColormapBrushEnum.Hot);
            colormapList.Add(ColormapBrush.ColormapBrushEnum.Jet);
            colormapList.Add(ColormapBrush.ColormapBrushEnum.Spring);
            colormapList.Add(ColormapBrush.ColormapBrushEnum.Summer);
            colormapList.Add(ColormapBrush.ColormapBrushEnum.Winter);

            return colormapList;
        }

        /// <summary>
        /// This method calculates the color that
        /// represents the interpolation of the two specified color ranges using
        /// the specified normalized value.
        /// </summary>
        /// <param name="minColor">The color representing the normalized value 0.0</param>
        /// <param name="maxColor">The color representing the normalized value 1.0</param>
        /// <param name="normalized_value">a value between 0.0 and 1.0 representing
        ///                                 where on the color scale between c1 and c2
        ///                                 the returned color should be.
        /// </param>
        /// <param name="emptyColor">Color to use if the normalized value is not good.</param>
        /// <returns>RGB of the value.</returns>
        public static SolidColorBrush GenerateColor(SolidColorBrush minColor, SolidColorBrush maxColor, double normalized_value, SolidColorBrush emptyColor = null)
        {
            // Bad Values get an empty color
            if (normalized_value > 1)
            {
                if (emptyColor == null)
                {
                    return DEFAULT_EMPTY_COLOR;
                }
                else
                {
                    return emptyColor;
                }
            }

            if (normalized_value <= 0.0)
            {
                return minColor;
            }
            if (normalized_value >= 1.0)
            {
                return maxColor;
            }

            byte red = (byte)((1.0 - normalized_value) * (minColor.Color.R + normalized_value * maxColor.Color.R));
            byte green = (byte)((1.0 - normalized_value) * (minColor.Color.G + normalized_value * maxColor.Color.G));
            byte blue = (byte)((1.0 - normalized_value) * (minColor.Color.B + normalized_value * maxColor.Color.B));

            SolidColorBrush color = new SolidColorBrush();
            color.Color = Color.FromArgb(255, red, green, blue);
            return color;
        }

        /// <summary>
        /// Create a brush based off the value given.
        /// The value will be based against the min and max velocity value.
        /// This will check if the value given is good.  If it is not good, it will
        /// give a color for a bad value.  
        /// </summary>
        /// <param name="colormap">Colormap to use to determine the color.</param>
        /// <param name="value">Value to convert to a color brush.</param>
        /// <param name="minVel">Minimum velocity.</param>
        /// <param name="maxVel">Maximum Velocity.</param>
        /// <param name="emptyColor">Color to use if the color is bad.</param>
        /// <returns>Color brush with the color based off the value, min and max velocity.</returns>
        public static SolidColorBrush GenerateColor(ColormapBrush colormap, double value, double minVel, double maxVel, SolidColorBrush emptyColor = null)
        {
            // Ensure the colormap was given
            if (colormap == null)
            {
                return DEFAULT_EMPTY_COLOR;
            }

            // Bad Values get an empty color
            if (value == DataSet.Ensemble.BAD_VELOCITY)
            {
                // If the user did not give a empty color, use the default
                if (emptyColor == null)
                {
                    return DEFAULT_EMPTY_COLOR;
                }
                else
                {
                    return emptyColor;
                }
            }

            return colormap.GetColormapColor(value, minVel, maxVel);
        }
    }
}
