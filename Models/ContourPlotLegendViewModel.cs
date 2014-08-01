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
 * 12/19/2012      RC          2.17       Initial coding
 * 12/28/2012      RC          2.17       Added UpdateColor() to update the legend color.
 * 
 * 
 */

namespace RTI
{
    using System.Windows.Media.Imaging;
    using System.Windows;
    using System;
    using Caliburn.Micro;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ContourPlotLegendViewModel : PropertyChangedBase
    {
        #region Variables

        /// <summary>
        /// Default scale label is meters per second.
        /// </summary>
        private const string DEFAULT_SCALE_LABEL = "m/s";

        /// <summary>
        /// Default width of a rectangle for the scale.
        /// </summary>
        private const int DEFAULT_SCALE_WIDTH = 5;

        /// <summary>
        /// Default height of a rectangle for the scale.
        /// </summary>
        private const int DEFAULT_SCALE_HEIGHT = 30;

        #endregion

        #region Properties

        /// <summary>
        /// Colormap for the given legend.
        /// </summary>
        private ColormapBrush _Colormap;
        /// <summary>
        /// Colormap for the given legend.
        /// </summary>
        public ColormapBrush Colormap
        {
            get { return _Colormap; }
            set
            {
                this.NotifyOfPropertyChange("Colormap");

                // Update the legend
                Update();
            }
        }

        /// <summary>
        /// Minimum Velocity.
        /// This will represent to lowest value in the 
        /// color spectrum.  Anything with this value or
        /// lower will have the lowest color in the 
        /// color map.
        /// </summary>
        private double _MinVelocity;
        /// <summary>
        /// Minimum velocity property.
        /// </summary>
        public double MinVelocity
        {
            get { return _MinVelocity; }
            set
            {
                _MinVelocity = value;
                this.NotifyOfPropertyChange("MinVelocity");

                // Update the legend
                Update();
            }
        }

        /// <summary>
        /// Max Velocities.  This represents the greatest
        /// value in the color spectrum.  Anything with 
        /// this value or greater will have the greatest
        /// color in the color map.
        /// </summary>
        private double _MaxVelocity;
        /// <summary>
        /// Max velocity property.
        /// </summary>
        public double MaxVelocity
        {
            get { return _MaxVelocity; }
            set
            {
                _MaxVelocity = value;
                this.NotifyOfPropertyChange("MaxVelocity");

                // Update the legend
                Update();
            }
        }

        /// <summary>
        /// Scale label for the values.  (Default is m/s).
        /// </summary>
        public string _scaleLabel;
        /// <summary>
        /// Scale label for the values.  (Default is m/s).
        /// </summary>
        public string ScaleLabel
        {
            get
            {
                return _scaleLabel;
            }
            set
            {
                _scaleLabel = value;
                PropertyChangeVelocityScales();
            }
        }

        /// <summary>
        /// Used for scale to display the minimum velocity.
        /// </summary>
        public string MinVelocityScale
        {
            get { return _MinVelocity.ToString("0.00") + ScaleLabel; }
        }

        /// <summary>
        /// Used for scale to display the velocity between center and minimum.
        /// </summary>
        public string HalfMinVelocityScale
        {
            get { return (_MinVelocity + ((_MaxVelocity - _MinVelocity) * 0.25)).ToString("0.00") + ScaleLabel; }
        }

        /// <summary>
        /// Used for scale to display the center velocity.
        /// </summary>
        public string HalfVelocityScale
        {
            get { return (_MinVelocity + ((_MaxVelocity - _MinVelocity) * 0.50)).ToString("0.00") + ScaleLabel; }
        }

        /// <summary>
        /// Used for scale to display the velocity between center and maximum.
        /// </summary>
        public string HalfMaxVelocityScale
        {
            get { return (_MinVelocity + ((_MaxVelocity - _MinVelocity) * 0.75)).ToString("0.00") + ScaleLabel; }
        }

        /// <summary>
        /// Used for scale to display the velocity the maximum velocity.
        /// </summary>
        public string MaxVelocityScale
        {
            get { return _MaxVelocity.ToString("0.00") + ScaleLabel; }
        }

        /// <summary>
        /// Bitmap to store the scale based off
        /// the current options.  
        /// </summary>
        private SimpleBitmap _scaleBitmap;
        /// <summary>
        /// Use this as the image source for the
        /// view.  This will be the base of the
        /// _scaleBitmap SimpleBitmap.
        /// </summary>
        public WriteableBitmap LegendImage
        {
            get
            {
                if (_scaleBitmap != null)
                {
                    return _scaleBitmap.BaseBitmap;
                }

                return null;
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// Initalize all the values.
        /// <param name="colormap">Colormap for the legend.</param>
        /// <param name="min">Minimum Velocity.</param>
        /// <param name="max">Maximum Velocity.</param>
        /// </summary>
        public ContourPlotLegendViewModel(ColormapBrush colormap, double min, double max)
        {
            _MinVelocity = min;
            _MaxVelocity = max;
            _Colormap = colormap;

            ScaleLabel = DEFAULT_SCALE_LABEL;

            // Update the legend
            Update();
        }

        /// <summary>
        /// Constructor
        /// Initalize all the values.
        /// <param name="colormap">Colormap for the legend.</param>
        /// <param name="min">Minimum Velocity.</param>
        /// <param name="max">Maximum Velocity.</param>
        /// </summary>
        public ContourPlotLegendViewModel(ColormapBrush.ColormapBrushEnum colormap, double min, double max)
        {
            _MinVelocity = min;
            _MaxVelocity = max;
            _Colormap = new ColormapBrush();
            _Colormap.ColormapBrushType = colormap;

            ScaleLabel = DEFAULT_SCALE_LABEL;

            // Update the legend
            Update();
        }

        /// <summary>
        /// Update the color for the legend.
        /// </summary>
        /// <param name="colormap">Color for the legend.</param>
        public void UpdateColor(ColormapBrush.ColormapBrushEnum colormap)
        {
            _Colormap.ColormapBrushType = colormap;

            // Update the legend
            Update();
        }

        #region Methods

        /// <summary>
        /// Update the legend with the latest data.
        /// </summary>
        private void Update()
        {
            // Update the scale
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() => CreateLegend()));

            // Property Change the Scale text values
            PropertyChangeVelocityScales();
        }

        /// <summary>
        /// When the Minimum or maximum velocity changes,
        /// notify that the scale ranges have also changed.
        /// </summary>
        private void PropertyChangeVelocityScales()
        {
            //this.RaisePropertyChanging(x => x.MinVelocityScale);
            //this.RaisePropertyChanged(x => x.HalfMinVelocityScale);
            //this.RaisePropertyChanged(x => x.HalfVelocityScale);
            //this.RaisePropertyChanged(x => x.HalfMaxVelocityScale);
            //this.RaisePropertyChanged(x => x.MaxVelocityScale);
            this.RaisePropertyChangedEventImmediately(null);
        }

        /// <summary>
        /// Create a legend for the plot.
        /// This will create a bitmap of 
        /// rectangles that gradually change
        /// color based off the min and max 
        /// velocity value and the min and
        /// max color set.
        /// </summary>
        private void CreateLegend()
        {
            int SCALE_COUNT = Colormap.ColormapLength;
            double increment = (MaxVelocity - MinVelocity) / SCALE_COUNT;

            // Create the bitmap
            _scaleBitmap = new SimpleBitmap(SCALE_COUNT * DEFAULT_SCALE_WIDTH, DEFAULT_SCALE_HEIGHT);

            double value = MinVelocity;
            for (int x = 0; x < SCALE_COUNT; x++)
            {
                _scaleBitmap.SetPixelRect((x * DEFAULT_SCALE_WIDTH), 0, DEFAULT_SCALE_WIDTH, DEFAULT_SCALE_HEIGHT, ColormapHelper.GenerateColor(Colormap, value, MinVelocity, MaxVelocity));

                // Move further in the color spectrum
                value += increment;
            }

            //this.RaisePropertyChanged(x => x.LegendImage);
            this.RaisePropertyChangedEventImmediately("LegendImage");
        }

        #endregion
    }
}
