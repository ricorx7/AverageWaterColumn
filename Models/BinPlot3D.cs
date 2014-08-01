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
 * 03/13/2012      RC          2.06       Initial coding
 * 03/14/2012      RC          2.06       Fixed Y axis up.
 * 03/16/2012      RC          2.06       Put the final orientation of the plot in the XMAL.
 *                                         Use YAxis to set orientation to horizontal or vertical column.
 * 03/20/2012      RC          2.06       Only draw the base models when needed.
 * 03/11/2013      RC          2.18       Improved the performance of GetBrush().
 * 03/13/2013      RC          2.18       Updated the Helix package and changed the 3D text prorperties.
 * 06/28/2013      RC          2.19       Replaced Shutdown() with IDisposable.
 *       
 * 
 */

using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;


namespace RTI
{
    /// <summary>
    /// Create a plot to represent the a bin's magnitude and direction.
    /// This plot will be made up of arrows pointing in the direction
    /// with its length and color representing its magnitude.  The arrows
    /// will be in a column representing the bin location.
    /// 
    /// The model cannot have arrows append and the model must be redrawn
    /// with each new bin because each arrow model must be recreated
    /// to shift its position in the space.  Saving the models will not work
    /// because there position will always being wrong when appending a new
    /// arrow to the end.  This would be the same as redrawing the entire plot
    /// because the only thing really created on a new draw is the bin arrows.
    /// </summary>
    public class BinPlot3D : ModelVisual3D
    {

        #region Variables

        /// <summary>
        /// Default selected bin is none selected.
        /// </summary>
        private const int DEFAULT_SELECTED_BIN = -1;

        /// <summary>
        /// Default value for the Cylinder radius.
        /// This is equal to 1 m/s.
        /// </summary>
        private const double DEFAULT_CYLINDER_RADIUS = 0.5;

        /// <summary>
        /// Default Minimum velocity.
        /// </summary>
        private const double DEFAULT_MIN_VELOCITY = 0;

        /// <summary>
        /// Default Maximum velocity.
        /// </summary>
        private const double DEFAULT_MAX_VELOCITY = 2;

        /// <summary>
        /// Default color to use when no color or a
        /// bad value is given.
        /// </summary>
        private SolidColorBrush SELECTED_BIN_COLOR = new SolidColorBrush(Colors.Red);

        /// <summary>
        /// Scale the arrow size based off this factor.
        /// </summary>
        private const double SCALE_ARROW = 10.0;

        /// <summary>
        /// Scale the translation value by this to make
        /// the size of the arrow align with the velocity plot.
        /// </summary>
        private const double TRANSLATION_SCALE = 0.43;

        /// <summary>
        /// Size of the arrow head.
        /// </summary>
        private const double ARROW_HEAD_SIZE = 0.25;

        /// <summary>
        /// Size of a arrow for the labels.
        /// </summary>
        private const double LABEL_ARROW_HEAD_SIZE = (ARROW_HEAD_SIZE * 2);

        /// <summary>
        /// Store the previous vector, so if we need to redraw
        /// the plot, the vector is available.
        /// </summary>
        private DataSet.EnsembleVelocityVectors _prevVectors;

        /// <summary>
        /// Previous number of bins.  This is used to determine
        /// if any settings have changed.
        /// </summary>
        private int _prevNumBins;

        /// <summary>
        /// East Arrow and label.
        /// </summary>
        private Model3D _eastArrow;

        /// <summary>
        /// North Arrow and label.
        /// </summary>
        private Model3D _northArrow;

        /// <summary>
        /// Cylinder model.
        /// </summary>
        private Model3D _cylinder;

        /// <summary>
        /// Origin tube model.
        /// </summary>
        private Model3D _originTube;

        /// <summary>
        /// Set the colormap for the plot.
        /// This will be the color spectrum of the plot.
        /// </summary>
        private ColormapBrush _colormap;

        #endregion

        #region Properties

        /// <summary>
        /// Radius of the cylinder in meters/second.
        /// The radius is used to see which arrows go outside
        /// the cylinder to get a general knowledge of speed.
        /// </summary>
        private double _cylinderRadius;
        /// <summary>
        /// Radius of the cylinder in meters/second.
        /// The radius is used to see which arrows go outside
        /// the cylinder to get a general knowledge of speed.
        /// </summary>
        public double CylinderRadius
        {
            get { return _cylinderRadius; }
            set
            {
                _cylinderRadius = value;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => CreateBase(_prevNumBins)));
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot(_prevVectors)));
            }
        }

        /// <summary>
        /// Set the selected bin for the plot.
        /// The selected bin arrow will be highlighted
        /// with a differen color.
        /// </summary>
        private int _selectedBin;
        /// <summary>
        /// Set the selected bin for the plot.
        /// The selected bin arrow will be highlighted
        /// with a differen color.
        /// </summary>
        public int SelectedBin
        {
            get { return _selectedBin; }
            set
            {
                _selectedBin = value;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot(_prevVectors)));
            }
        }

        /// <summary>
        /// Color selection chosen by the user for the plot.
        /// </summary>
        private RTI.ColormapBrush.ColormapBrushEnum _colormapBrushSelection;
        /// <summary>
        /// Color selection chosen by the user for the plot.
        /// </summary>
        public RTI.ColormapBrush.ColormapBrushEnum ColormapBrushSelection
        {
            get { return _colormapBrushSelection; }
            set
            {
                _colormapBrushSelection = value;
                _colormap.ColormapBrushType = _colormapBrushSelection;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot(_prevVectors)));
            }
        }

        /// <summary>
        /// Minimum velocity.  This is used to
        /// show the velocity color in relation to
        /// the minimum and maximum set.
        /// </summary>
        private double _minVelocity;
        /// <summary>
        /// Minimum velocity.  This is used to
        /// show the velocity color in relation to
        /// the minimum and maximum set.
        /// </summary>
        public double MinVelocity 
        {
            get { return _minVelocity; }
            set
            {
                _minVelocity = value;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot(_prevVectors)));
            }
        }

        /// <summary>
        /// Maximum velocity.  This is used to
        /// show the velocity color in relation to
        /// the minimum and maximum set.
        /// </summary>
        private double _maxVelocity;
        /// <summary>
        /// Maximum velocity.  This is used to
        /// show the velocity color in relation to
        /// the minimum and maximum set.
        /// </summary>
        public double MaxVelocity
        {
            get { return _maxVelocity; }
            set
            {
                _maxVelocity = value;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot(_prevVectors)));
            }
        }

        /// <summary>
        /// By setting this true, all the values will be going down
        /// the Y axis.  From 0 to -(Bin Number).  If this is set false,
        /// all values are moving across the X axis.  From 0 to (Num Bins).
        /// </summary>
        public bool YAxis { get; set; }

        #endregion

        /// <summary>
        /// Create a plot to represent the a bin's magnitude and direction.
        /// This plot will be made up of arrows pointing in the direction
        /// with its length and color representing its magnitude.  The arrows
        /// will be in a column representing the bin location.
        /// </summary>
        public BinPlot3D()
        {
            // Initialize values
            SelectedBin = DEFAULT_SELECTED_BIN;
            _minVelocity = DEFAULT_MIN_VELOCITY;
            _maxVelocity = DEFAULT_MAX_VELOCITY;

            _colormap = new ColormapBrush();
            _colormap.ColormapBrushType = ColormapBrush.ColormapBrushEnum.Jet;

            _cylinderRadius = DEFAULT_CYLINDER_RADIUS;

            YAxis = true;

            _prevNumBins = 0;
        }

        /// <summary>
        /// Receive a list of vectors and add
        /// it to the list.  Limit the size of the list 
        /// of vectors.
        /// </summary>
        /// <param name="vectors">Vectors to add to the list.</param>
        public void AddIncomingData(DataSet.EnsembleVelocityVectors vectors)
        {
            _prevVectors = vectors;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawPlot(vectors)));
        }

        /// <summary>
        /// Clear the plot of all data.
        /// This is used to start a new plot.
        /// </summary>
        public void ClearIncomingData()
        {

            Application.Current.Dispatcher.BeginInvoke(new Action(() => ClearPlot()));
        }

        /// <summary>
        /// For each bin, create a arrow.  Set the arrow location to the
        /// column location of the bin.  Set the lenght of the arrow based off
        /// the magnitude and a scale factor.  Set the color of the arrow based
        /// off the magnitude.  If the arrow is selected, set the color to the
        /// selection color.  Then rotate the arrow based off the angle in the
        /// vector.  Then add the arrow to the model.
        /// 
        /// A North arrow will be created to know where North is relative
        /// to the scene.
        /// </summary>
        /// <param name="vectors">Vectors to display.</param>
        private void DrawPlot(DataSet.EnsembleVelocityVectors vectors)
        {
            // Ensure the data is good
            if (vectors.Vectors != null)
            {
                var group = new Model3DGroup();
                int numBins = vectors.Vectors.Length;

                // Go through each bin
                for (int bin = 0; bin < numBins; bin++)
                {
                    // Get the magnitude and direction
                    double mag = vectors.Vectors[bin].Magnitude;
                    double angleYNorth = vectors.Vectors[bin].DirectionYNorth;

                    // If the magnitude is bad, do not create an arrow
                    if (mag != DataSet.Ensemble.BAD_VELOCITY)
                    {
                        // Add the bin arrow to the model
                        group.Children.Add(CreateBin(bin, mag, angleYNorth));
                    }
                }

                // If the ensemble changed, reset the base
                if (numBins != _prevNumBins)
                {
                    CreateBase(vectors.Vectors.Length);

                    _prevNumBins = numBins;
                }

                // Create the North and East arrow
                group.Children.Add(_northArrow);
                group.Children.Add(_eastArrow);

                // Origin tube
                group.Children.Add(_originTube);

                // Last item to add
                group.Children.Add(_cylinder);

                Content = group;
            }
        }

        /// <summary>
        /// Clear the plot by giving the Content
        /// an empty object.
        /// </summary>
        private void ClearPlot()
        {
            if (Content != null)
            {
                // Remove all the elements in the model
                ((Model3DGroup)Content).Children.Clear();
            }
            SelectedBin = DEFAULT_SELECTED_BIN;
        }

        /// <summary>
        /// Create the base of the plot.
        /// This includes the arrows for North and East, the cylinder and
        /// the center tube.
        /// </summary>
        /// <param name="numBins">Number of bins in the plot.</param>
        private void CreateBase(int numBins)
        {
            // Create the North and East arrow
            _northArrow = NorthArrow(numBins);           // Create North arrow
            _eastArrow = EastArrow(numBins);             // Create East arrow
            _originTube = CreateOriginTube(numBins);     // Create Origin Tube
            _cylinder = CreateCylinder(numBins);         // Create cylinder
        }

        /// <summary>
        /// Create an arrow representing the magnitude and direction
        /// for a given bin.  This will create an arrow within a column.
        /// The Column will start at 0 and go down, using the bin times a scale
        /// factor to move down the column for each bin.  The magnitude will
        /// give the length and color of the arrow.  The angle will be used to rotate around
        /// the origin of 0,0, to give the direction.  The angle is based off North = 0 degrees.
        /// </summary>
        /// <param name="bin">Bin to create.</param>
        /// <param name="mag">Magnitude of the bin.</param>
        /// <param name="angleYNorth">Direction of the bin with reference to Y as North.</param>
        /// <returns>3D model of the arrow.</returns>
        private GeometryModel3D CreateBin(int bin, double mag, double angleYNorth)
        {
            // Location on the axis for each bin
            // Set the Camera UpDirection in XMAL to UpDirection="0, 1, 0" to make it use Y axis as Up.  Default is Z up. 
            double xAxisLoc = 0;
            double yAxisLoc = 0;
            double zAxisLoc = 0;

            if (YAxis)
            {
                yAxisLoc = -bin * TRANSLATION_SCALE;
            }
            else
            {
                xAxisLoc = bin * TRANSLATION_SCALE;
            }

            // Create the shape of the object
            // This will be an arrow
            // Use the magnitude to get the length of the arrow
            // Create a column of bins with arrows
            var mb = new MeshBuilder(false, false);
            if (YAxis)
            {
                mb.AddArrow(new Point3D(xAxisLoc, yAxisLoc, zAxisLoc), new Point3D(mag * SCALE_ARROW, yAxisLoc, zAxisLoc), ARROW_HEAD_SIZE);
            }
            else
            {
                mb.AddArrow(new Point3D(xAxisLoc, yAxisLoc, zAxisLoc), new Point3D(xAxisLoc, mag * SCALE_ARROW, zAxisLoc), ARROW_HEAD_SIZE);
            }
            Geometry3D geometry = mb.ToMesh();

            // Set the color based off the magnitude
            // If the bin is selected, use the selected color
            Material material;
            if (bin != SelectedBin)
            {
                material = MaterialHelper.CreateMaterial(GenerateColor(mag));
            }
            else
            {
                material = MaterialHelper.CreateMaterial(SELECTED_BIN_COLOR);
            }
            material.Freeze();

            // Rotate the object
            var rotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(xAxisLoc, yAxisLoc, zAxisLoc), angleYNorth));
            rotation.Freeze();

            // Create the model with the rotation
            var tg = new Transform3DGroup();
            tg.Children.Add(rotation);
            var model = new GeometryModel3D(geometry, material) { Transform = tg };

            return model;
        }

        /// <summary>
        /// Place a tube down the center of the plot
        /// so all the arrows will connect to a center point (origin).
        /// </summary>
        /// <param name="numBins">Number of bins in the plot.</param>
        /// <returns>3D model of the center tube.</returns>
        private GeometryModel3D CreateOriginTube(int numBins)
        {
            double tubeDia = ARROW_HEAD_SIZE;       // Tube Diameter.  Same as Arrows.      Min = 0.1 Max = 1.0
            int tubeTheta = 30;                     // Tube ThetaDiv.                       Min = 3 Max = 100

            double xAxisLoc = 0;
            double yAxisLoc = 0;
            double zAxisLoc = 0;

            if (YAxis)
            {
                yAxisLoc = -numBins * TRANSLATION_SCALE;
            }
            else
            {
                xAxisLoc = numBins * TRANSLATION_SCALE;
            }

            // Create 2 points, the top and bottom
            List<Point3D> pts = new List<Point3D>();
            pts.Add(new Point3D(0, 0, 0));
            pts.Add(new Point3D(xAxisLoc, yAxisLoc, zAxisLoc));

            // Create the tube based off the points
            var mb = new MeshBuilder(false, false);
            mb.AddTube(pts, tubeDia, tubeTheta, true);
            Geometry3D geometry = mb.ToMesh();

            // Set the color
            Material material = MaterialHelper.CreateMaterial(Color.FromArgb(255, 255,250,213));
            material.Freeze();

            var model = new GeometryModel3D(geometry, material);

            return model;
        }

        /// <summary>
        /// Create a cylinder to encapsulate the arrows.  The cylinder will
        /// be X m/s wide.  The cylinder will be transparent so the arrows within
        /// can be seen.
        /// </summary>
        /// <param name="numBins"></param>
        /// <returns></returns>
        private Model3D CreateCylinder(int numBins)
        {
            var mb = new MeshBuilder(false, false);
            TruncatedConeVisual3D cyl = new TruncatedConeVisual3D();            // This will create a cylinder, basically 2 cones on top of each other
            cyl.BaseRadius = CylinderRadius * SCALE_ARROW;                       // Set the radius of the cylinder based off X m/s using the scale factor
            cyl.TopRadius = cyl.BaseRadius;                                     // Set the radius of the cylinder
            cyl.Fill = new SolidColorBrush(Color.FromArgb(40, 00, 130, 255));   // Set the color of the cylinder.  Make it trasparent
            cyl.Origin = new Point3D(0,0,0);                                    // Set the origin of the cylinder as the origin of the plot

            if (YAxis)
            {
                cyl.Normal = new Vector3D(0, 1, 0);                                 // Set the orientation of the cylinder Y Up
                cyl.Height = -numBins * TRANSLATION_SCALE;                          // Set the height based off the number bins and scale (go in the negative direction)
            }
            else
            {
                cyl.Normal = new Vector3D(1, 0, 0);                                 // Set the orientation of the cylinder Y Up
                cyl.Height = numBins * TRANSLATION_SCALE;                          // Set the height based off the number bins and scale (go in the Positive direction)
            }

            return cyl.Content;
        }

        /// <summary>
        /// Create the text for the north arrow.
        /// 
        /// It is suggested to use an image instead
        /// of text for performance purposes.  I will
        /// look into this later.
        /// </summary>
        /// <returns>3D Text.</returns>
        private Model3D NorthArrow(int numBins)
        {
            Model3DGroup group = new Model3DGroup();

            // Get the length of the cylinder, if it is 0, then display something.
            double arrowLength = CylinderRadius * SCALE_ARROW;
            if (arrowLength <= 0)
            {
                arrowLength = SCALE_ARROW;
            }

            #region Arrow
            //Create the shape of the object
            //This will be an arrow
            //Length will be the scale value
            //The position will be above the plot
            double xAxisLoc = 0;
            double yAxisLoc = 0;
            double zAxisLoc = 0;

            if (YAxis)
            {
                yAxisLoc = -numBins * TRANSLATION_SCALE;
            }
            else
            {
                xAxisLoc = numBins * TRANSLATION_SCALE;
            }

            var mb = new MeshBuilder(false, false);
            if (YAxis)
            {
                mb.AddArrow(new Point3D(xAxisLoc, yAxisLoc, zAxisLoc), new Point3D(arrowLength, yAxisLoc, zAxisLoc), LABEL_ARROW_HEAD_SIZE, 3, 100);
            }
            else
            {
                mb.AddArrow(new Point3D(xAxisLoc, yAxisLoc, zAxisLoc), new Point3D(xAxisLoc, arrowLength, zAxisLoc), LABEL_ARROW_HEAD_SIZE, 3, 100);
            }
            Geometry3D geometry = mb.ToMesh();

            //Set the color
            Material material = MaterialHelper.CreateMaterial(SELECTED_BIN_COLOR);
            material.Freeze();

            var model = new GeometryModel3D(geometry, material);
            model.BackMaterial = material;
            #endregion


            #region Text
            double xAxisLocLabel = 0;
            double yAxisLocLabel = 0;
            double zAxisLocLabel = LABEL_ARROW_HEAD_SIZE;                                       // Make the text just in front of the North arrow
            if (YAxis)
            {
                xAxisLocLabel = arrowLength / 2;                                             // Make the text in the middle of the North arrow
                yAxisLocLabel = -numBins * TRANSLATION_SCALE;                                // Go to the bottom of the column
            }
            else
            {
                xAxisLocLabel = numBins * TRANSLATION_SCALE;
                yAxisLocLabel = arrowLength / 2;
            }

            TextVisual3D txt = new TextVisual3D();
            txt.Position = new Point3D(xAxisLocLabel, yAxisLocLabel, zAxisLocLabel);
            txt.Height = 0.5;
            txt.Text = string.Format("North {0}", (arrowLength / SCALE_ARROW).ToString("0.0"));   // Need to get the arrowLenght back to m/s
            txt.TextDirection = new Vector3D(1, 0, 0);                                              // Set text to run in line with X axis
            txt.UpDirection = new Vector3D(0, 1, 0);                                                     // Set text to Point Up on Y axis
            txt.Foreground = new SolidColorBrush(Colors.Black);
            txt.Background = new SolidColorBrush(Colors.WhiteSmoke);
            txt.Padding = new Thickness(2);

            #endregion

            group.Children.Add(model);
            group.Children.Add(txt.Content);

            return group;
        }

        /// <summary>
        /// Create the East and Radius label.
        /// This will display the text for "East" and the size
        /// of the cylinder radius.
        /// </summary>
        /// <param name="numBins">Number of bins in the ensemble.</param>
        /// <returns>Model of the label.</returns>
        private Model3D EastArrow(int numBins)
        {
            Model3DGroup group = new Model3DGroup();

            // Get the length of the cylinder, if it is 0, then display something.
            double arrowLength = CylinderRadius * SCALE_ARROW;
            if (arrowLength <= 0)
            {
                arrowLength = SCALE_ARROW;
            }

            #region Arrow
            //Create the shape of the object
            //This will be an arrow
            //Length will be the scale value
            //The position will be above the plot
            double xAxisLoc = 0;
            double yAxisLoc = 0;             
            double zAxisLoc = 0;
            if (YAxis)
            {
                yAxisLoc = -numBins * TRANSLATION_SCALE;                // Go to the bottom of the column
            }
            else
            {
                xAxisLoc = numBins * TRANSLATION_SCALE;
            }

            var mb = new MeshBuilder(false, false);
            mb.AddArrow(new Point3D(xAxisLoc, yAxisLoc, zAxisLoc), new Point3D(xAxisLoc, yAxisLoc, arrowLength), LABEL_ARROW_HEAD_SIZE, 3, 100);
            Geometry3D geometry = mb.ToMesh();

            //Set the color
            Material material = MaterialHelper.CreateMaterial(Colors.Lime);
            material.Freeze();

            ////Rotate the object
            //var rotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(xAxisLoc, yAxisLoc, zAxisLoc), 90));
            //rotation.Freeze();

            //var tg = new Transform3DGroup();
            //tg.Children.Add(rotation);

            var model = new GeometryModel3D(geometry, material) {};
            model.BackMaterial = material;
            #endregion

            #region Text
            double xAxisLocLabel = 0;
            double yAxisLocLabel = 0;
            double zAxisLocLabel = 0;
            if (YAxis)
            {
                xAxisLocLabel = -LABEL_ARROW_HEAD_SIZE;                                              // Make the text in the middle of the East arrow
                yAxisLocLabel = (-numBins * TRANSLATION_SCALE) - (ARROW_HEAD_SIZE * 2);              // Go to the bottom of the column
                zAxisLocLabel = (arrowLength / 2);                                                   // Make the text just in front of the East arrow
            }
            else
            {
                xAxisLocLabel = (numBins * TRANSLATION_SCALE) + (ARROW_HEAD_SIZE * 2);              // Go to the bottom of the column
                yAxisLocLabel = -(ARROW_HEAD_SIZE * 2);                                                   // Make the text just in front of the East arrow
                zAxisLocLabel = (arrowLength / 2);                                         // Make the text in the middle of the East arrow
            }

            TextVisual3D txt = new TextVisual3D();
            txt.Position = new Point3D(xAxisLocLabel, yAxisLocLabel, zAxisLocLabel);
            txt.Height = 0.5;
            txt.Text = string.Format("East {0}", (arrowLength / SCALE_ARROW).ToString("0.0"));
            txt.TextDirection = new Vector3D(0, 0, 1);                      // Set text to run in line with X axis
            txt.UpDirection = new Vector3D(0, 1, 0);                             // Set text to Point Up on Y axis
            txt.Foreground = new SolidColorBrush(Colors.Black);
            txt.Background = new SolidColorBrush(Colors.WhiteSmoke);
            txt.Padding = new Thickness(2);

            #endregion

            group.Children.Add(model);
            group.Children.Add(txt.Content);

            return group;
        }

        #region Color

        /// <summary>
        /// Create a brush based off the value given.
        /// The value will be based against the min and max velocity value.
        /// </summary>
        /// <param name="value">Value to convert to a color brush.</param>
        /// <returns>Color brush with the color based off the value, min and max velocity.</returns>
        private SolidColorBrush GenerateColor(double value)
        {
            return _colormap.GetColormapColor(value, _minVelocity, _maxVelocity);
        }

        #endregion
    }

}