/*
 * Copyright 2011, Rowe Technology Inc. 
 * All rights reserved.
 * http://www.rowetechinc.com
 * https://github.com/rowetechinc
 * 
 * Redistribution and use in source and binary forms, with or without modification, are
 * permitted provided that the following conditions are met:
 * 
 *  1. Redistributions of source code must retain the above copyright notice, this list of
 *      conditions and the following disclaimer.
 *      
 *  2. Redistributions in binary form must reproduce the above copyright notice, this list
 *      of conditions and the following disclaimer in the documentation and/or other materials
 *      provided with the distribution.
 *      
 *  THIS SOFTWARE IS PROVIDED BY Rowe Technology Inc. ''AS IS'' AND ANY EXPRESS OR IMPLIED 
 *  WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 *  FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> OR
 *  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 *  CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 *  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 *  ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 *  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 *  ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *  
 * The views and conclusions contained in the software and documentation are those of the
 * authors and should not be interpreted as representing official policies, either expressed
 * or implied, of Rowe Technology Inc.
 * 
 * 
 * HISTORY
 * -----------------------------------------------------------------
 * Date            Initials    Version    Comments
 * -----------------------------------------------------------------
 * 05/30/2014      RC          0.0.1      Initial coding
 * 06/02/2014      RC          0.0.2      Added Running average.
 * 06/02/2014      RC          0.0.3      Added new line at the end of the string output.
 * 07/29/2014      RC          0.0.4      Fixed setting the Min and Max bin to the output.
 * 08/01/2014      RC          0.0.4      Added WaterDataList to display the data.
 * 
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Caliburn.Micro;
    using System.Threading;
    using System.Diagnostics;
    using AverageWaterColumn.Properties;
    using System.Net;
    using AutoUpdaterDotNET;
    using System.Windows;

    /// <summary>
    /// Display the options and output for the application.
    /// </summary>
    public class HomeViewModel : PropertyChangedBase, IDeactivate
    {

        #region Class and Structs 

        /// <summary>
        /// Struct to hold the average velocity,
        /// average direction and maximum
        /// velocity.
        /// </summary>
        private struct AvgData
        {
            /// <summary>
            /// Average velocity.
            /// </summary>
            public double AvgVel;

            /// <summary>
            /// Average direction.
            /// </summary>
            public double AvgDir;

            /// <summary>
            /// Maximum velocity.
            /// </summary>
            public double MaxVel;

            /// <summary>
            /// Minimum bin used.
            /// </summary>
            public int MinBin;

            /// <summary>
            /// Maximum bin used.
            /// </summary>
            public int MaxBin;
        }

        private struct ShipData
        {
            /// <summary>
            /// Ship velocity.
            /// </summary>
            public double ShipVel;

            /// <summary>
            /// Ship direction.
            /// </summary>
            public double ShipDir;

            /// <summary>
            /// Ship Maximum velocity.
            /// </summary>
            public double ShipMaxVel;
        }

        /// <summary>
        /// Buffer to hold the average data.
        /// </summary>
        private struct AvgDataBuffer
        {
            /// <summary>
            /// Averaged data.
            /// </summary>
            public AvgData Data;

            /// <summary>
            /// Ship Data.
            /// </summary>
            public ShipData ShipData;

            /// <summary>
            /// String of the latest data.
            /// </summary>
            public string DataStr;
        }

        /// <summary>
        /// Water data info.
        /// </summary>
        public class WaterData
        {
            /// <summary>
            /// Bin number.
            /// </summary>
            public int Bin { get; set; }

            /// <summary>
            /// Magintude.
            /// </summary>
            public string Mag { get; set; }

            /// <summary>
            /// Direction.
            /// </summary>
            public string Dir { get; set; }
        }

        #endregion

        #region Variables

        /// <summary>
        ///  Setup logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// ADCP Serial Port options.
        /// </summary>
        private SerialOptions _adcpSerialOptions;

        /// <summary>
        /// ADCP Serial port.
        /// </summary>
        private AdcpSerialPort _adcpSerialPort;

        /// <summary>
        /// Binary ADCP codec to decode the data.
        /// </summary>
        private AdcpBinaryCodecNew _binaryCodec;

        /// <summary>
        /// Output Serial Port options.
        /// </summary>
        private SerialOptions _outputSerialOptions;

        /// <summary>
        /// Serial port to output the data.
        /// </summary>
        private AdcpSerialPort _outputSerialPort;

        /// <summary>
        /// GPS Serial Port options.
        /// </summary>
        private SerialOptions _gpsSerialOptions;

        /// <summary>
        /// Serial port to take in the GPS data.
        /// </summary>
        private GpsSerialPort _gpsSerialPort;

        /// <summary>
        /// Timer to have a fixed output.
        /// </summary>
        private System.Threading.Timer _timer;

        /// <summary>
        /// Output buffer to store the latest string to 
        /// write data.
        /// </summary>
        private AvgDataBuffer _outputBuffer;

        /// <summary>
        /// A buffer to hold the running average of data.
        /// </summary>
        private List<AvgData> _runningAvgBuffer;

        /// <summary>
        /// Vessel Mount Options.
        /// </summary>
        private VesselMountOptions _vmOptions;

        /// <summary>
        /// Previous Good Bottom Track value.
        /// </summary>
        private float _prevBtEast;

        /// <summary>
        /// Previous Good Bottom Track North value.
        /// </summary>
        private float _prevBtNorth;

        /// <summary>
        /// Previous Good Bottom Track Vertical value.
        /// </summary>
        private float _prevBtVert;

        /// <summary>
        /// Heading source.
        /// </summary>
        private Transform.HeadingSource _headingSource;

        /// <summary>
        /// Keep track of the maximum ship velocity.
        /// </summary>
        private double _maxShipVelocity;

        #endregion

        #region Properties

        #region Comm Port

        /// <summary>
        /// List of all the comm ports on the computer.
        /// </summary>
        private List<string> _CommPortList;
        /// <summary>
        /// List of all the comm ports on the computer.
        /// </summary>
        public List<string> CommPortList
        {
            get { return _CommPortList; }
            set
            {
                _CommPortList = value;
                RaisePropertyChangedEventImmediately("CommPortList");
            }
        }

        /// <summary>
        /// Selected ADCP Comm Port.
        /// </summary>
        private string _SelectedAdcpCommPort;
        /// <summary>
        /// Selected ADCP Comm Port.
        /// </summary>
        public string SelectedAdcpCommPort
        {
            get { return _SelectedAdcpCommPort; }
            set
            {
                _SelectedAdcpCommPort = value;

                // Set the options
                _adcpSerialOptions.Port = value;
                ReconnectAdcp();

                // Save the option
                Settings.Default.AdcpCommPort = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("SelectedAdcpCommPort");
            }
        }

        /// <summary>
        /// Selected Output Comm Port.
        /// </summary>
        private string _SelectedOutputCommPort;
        /// <summary>
        /// Selected Output Comm Port.
        /// </summary>
        public string SelectedOutputCommPort
        {
            get { return _SelectedOutputCommPort; }
            set
            {
                _SelectedOutputCommPort = value;

                // Set the options
                _outputSerialOptions.Port = value;
                ReconnectOutputSerialPort();

                // Save the option
                Settings.Default.OutputCommPort = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("SelectedOutputCommPort");
            }
        }

        /// <summary>
        /// Set a flag to enable to disable the GPS serial port.
        /// </summary>
        private bool _IsGpsEnabled;
        /// <summary>
        /// Set a flag to enable to disable the GPS serial port.
        /// </summary>
        public bool IsGpsEnabled
        {
            get { return _IsGpsEnabled; }
            set
            {
                _IsGpsEnabled = value;

                // Set the options
                if (value)
                {
                    ReconnectGpsSerialPort();
                }
                else
                {
                    DisconnectGpsSerialPort();
                }

                // Save the option
                Settings.Default.IsGpsEnabled = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("IsGpsEnabled");
            }
        }

        
        /// <summary>
        /// Selected GPS Comm Port.
        /// </summary>
        private string _SelectedGpsCommPort;
        /// <summary>
        /// Selected GPS Comm Port.
        /// </summary>
        public string SelectedGpsCommPort
        {
            get { return _SelectedGpsCommPort; }
            set
            {
                _SelectedGpsCommPort = value;

                // Set the options
                _gpsSerialOptions.Port = value;
                if (_IsGpsEnabled)
                {
                    ReconnectGpsSerialPort();
                }

                // Save the option
                Settings.Default.GpsCommPort = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("SelectedGpsCommPort");
            }
        }

        #endregion

        #region Baud Rate

        /// <summary>
        /// List of all the baud rate options.
        /// </summary>
        public List<int> BaudRateList { get; set; }

        /// <summary>
        /// Selected ADCP baud rate.
        /// </summary>
        private int _SelectedAdcpBaudRate;
        /// <summary>
        /// Selected ADCP baud rate.
        /// </summary>
        public int SelectedAdcpBaudRate
        {
            get { return _SelectedAdcpBaudRate; }
            set
            {
                _SelectedAdcpBaudRate = value;

                // Set the options
                _adcpSerialOptions.BaudRate = value;
                ReconnectAdcp();

                // Save the option
                Settings.Default.AdcpBaud = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("SelectedAdcpBaudRate");
            }
        }

        /// <summary>
        /// Selected output baud rate.
        /// </summary>
        private int _SelectedOutputBaudRate;
        /// <summary>
        /// Selected output baud rate.
        /// </summary>
        public int SelectedOutputBaudRate
        {
            get { return _SelectedOutputBaudRate; }
            set
            {
                _SelectedOutputBaudRate = value;

                // Set the options
                _outputSerialOptions.BaudRate = value;
                ReconnectOutputSerialPort();

                // Save the option
                Settings.Default.OutputBaud = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("SelectedOutputBaudRate");
            }
        }

        /// <summary>
        /// Selected GPS baud rate.
        /// </summary>
        private int _SelectedGpsBaudRate;
        /// <summary>
        /// Selected GPS baud rate.
        /// </summary>
        public int SelectedGpsBaudRate
        {
            get { return _SelectedGpsBaudRate; }
            set
            {
                _SelectedGpsBaudRate = value;

                // Set the options
                _gpsSerialOptions.BaudRate = value;
                if (_IsGpsEnabled)
                {
                    ReconnectGpsSerialPort();
                }

                // Save the option
                Settings.Default.GpsBaud = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("SelectedGpsBaudRate");
            }
        }

        #endregion

        #region Average Water Display

        /// <summary>
        /// Average Velocity.
        /// </summary>
        private double _AvgVel;
        /// <summary>
        /// Average Velocity.
        /// </summary>
        public string AvgVel
        {
            get { return _AvgVel.ToString("0.000"); }
        }

        /// <summary>
        /// Average Direction.
        /// </summary>
        private double _AvgDir;
        /// <summary>
        /// Average Direction.
        /// </summary>
        public string AvgDir
        {
            get { return _AvgDir.ToString("0.000"); }
        }

        /// <summary>
        /// Maximum Velocity.
        /// </summary>
        private double _MaxVel;
        /// <summary>
        /// Maximum Velocity.
        /// </summary>
        public string MaxVel
        {
            get { return _MaxVel.ToString("0.000"); }
        }

        #endregion

        #region Ship Speed Display

        /// <summary>
        /// Ship Velocity.
        /// </summary>
        private double _ShipVel;
        /// <summary>
        /// Ship Velocity.
        /// </summary>
        public string ShipVel
        {
            get { return _ShipVel.ToString("0.000"); }
        }

        /// <summary>
        /// Ship  Direction.
        /// </summary>
        private double _ShipDir;
        /// <summary>
        /// Ship  Direction.
        /// </summary>
        public string ShipDir
        {
            get { return _ShipDir.ToString("0.000"); }
        }

        /// <summary>
        /// Ship Maximum Velocity.
        /// </summary>
        private double _ShipMaxVel;
        /// <summary>
        /// Ship Maximum Velocity.
        /// </summary>
        public string ShipMaxVel
        {
            get { return _ShipMaxVel.ToString("0.000"); }
        }

        #endregion

        #region Settings Display

        /// <summary>
        /// Settings ViewModel.
        /// </summary>
        public SettingsViewModel SettingsVM { get; set; }

        #endregion

        #region Screening

        /// <summary>
        /// List of available heading sources.
        /// </summary>
        public List<string> HeadingSourceList {get; set;}

        /// <summary>
        /// Maximum Velocity.
        /// </summary>
        public string SelectedHeadingSource
        {
            get
            {
                if (_headingSource == Transform.HeadingSource.ADCP)
                {
                    return "ADCP";
                }

                return "GPS";
            }
            set
            {
                if (value == "ADCP")
                {
                    _headingSource = Transform.HeadingSource.ADCP;
                }
                else
                {
                    _headingSource = Transform.HeadingSource.GPS1;
                }
                RaisePropertyChangedEventImmediately("SelectedHeadingSource");
            }
        }

        #endregion

        #region Output String

        /// <summary>
        /// Output string that is output to the serial port.
        /// </summary>
        private string _OutputStr;
        /// <summary>
        /// Output string that is output to the serial port.
        /// </summary>
        public string OutputStr
        {
            get { return _OutputStr; }
            set
            {
                _OutputStr = value;
                RaisePropertyChangedEventImmediately("OutputStr");
            }
        }

        #endregion

        #region Plot

        /// <summary>
        /// Velocity plot.
        /// </summary>
        public BinPlot3D VelPlot { get; set; }

        /// <summary>
        /// List of vector data.
        /// </summary>
        public List<WaterData> WaterDataList { get; set; }

        /// <summary>
        /// Legend for the plot.  This will keep track of the
        /// legend image and min to max values for the legend.
        /// </summary>
        public ContourPlotLegendViewModel Legend { get; set; }

        
        /// <summary>
        /// Plot size.
        /// </summary>
        private int _PlotSize;
        /// <summary>
        /// Plot size.
        /// </summary>
        public int PlotSize
        {
            get { return _PlotSize; }
            set
            {
                _PlotSize = value;
                RaisePropertyChangedEventImmediately("PlotSize");
            }
        }

        /// <summary>
        /// Minimum Velocity.
        /// This will represent to lowest value in the 
        /// color spectrum.  Anything with this value or
        /// lower will have the lowest color in the 
        /// color map.
        /// </summary>
        private double _minVelocity;
        /// <summary>
        /// Minimum velocity property.
        /// </summary>
        public double MinVelocity
        {
            get { return _minVelocity; }
            set
            {
                _minVelocity = value;

                // Update the legend
                Legend.MinVelocity = value;
                VelPlot.MinVelocity = value;

                this.RaisePropertyChangedEventImmediately("MinVelocity");
                this.RaisePropertyChangedEventImmediately("Legend");
                this.RaisePropertyChangedEventImmediately("VelPlot");
            }
        }

        /// <summary>
        /// Max Velocities.  This represents the greatest
        /// value in the color spectrum.  Anything with 
        /// this value or greater will have the greatest
        /// color in the color map.
        /// </summary>
        private double _maxVelocity;
        /// <summary>
        /// Max velocity property.
        /// </summary>
        public double MaxVelocity
        {
            get { return _maxVelocity; }
            set
            {
                _maxVelocity = value;
                
                // Update the legend
                Legend.MaxVelocity = value;
                VelPlot.MaxVelocity = value;

                this.RaisePropertyChangedEventImmediately("MaxVelocity");
                this.RaisePropertyChangedEventImmediately("Legend");
                this.RaisePropertyChangedEventImmediately("VelPlot");
            }
        }

        #endregion

        #region Version and Updates

        /// <summary>
        /// RTI Average Water Column version number.
        /// </summary>
        private string _AvgWaterColumnVersionVersion;
        /// <summary>
        /// RTI Average Water Column version number.
        /// </summary>
        public string AvgWaterColumnVersion
        {
            get { return _AvgWaterColumnVersionVersion; }
            set
            {
                _AvgWaterColumnVersionVersion = value;
                this.NotifyOfPropertyChange(() => this.AvgWaterColumnVersion);
            }
        }

        /// <summary>
        /// Flag to determine if we are looking for the update.
        /// </summary>
        private bool _IsCheckingForUpdates;
        /// <summary>
        /// Flag to determine if we are looking for the update.
        /// </summary>
        public bool IsCheckingForUpdates
        {
            get { return _IsCheckingForUpdates; }
            set
            {
                _IsCheckingForUpdates = value;
                this.NotifyOfPropertyChange(() => this.IsCheckingForUpdates);
            }
        }

        /// <summary>
        /// RTI Average Water Column Update URL.
        /// </summary>
        private string _AvgWaterColumnUpdateUrl;
        /// <summary>
        /// RTI  Average Water Column Update URL.
        /// </summary>
        public string AvgWaterColumnUpdateUrl
        {
            get { return _AvgWaterColumnUpdateUrl; }
            set
            {
                _AvgWaterColumnUpdateUrl = value;
                this.NotifyOfPropertyChange(() => this.AvgWaterColumnUpdateUrl);
            }
        }

        /// <summary>
        /// A string to nofity the user if the version is not update to date.
        /// </summary>
        private string _AvgWaterColumnUpdateToDate;
        /// <summary>
        /// A string to nofity the user if the version is not update to date.
        /// </summary>
        public string AvgWaterColumnUpdateToDate
        {
            get { return _AvgWaterColumnUpdateToDate; }
            set
            {
                _AvgWaterColumnUpdateToDate = value;
                this.NotifyOfPropertyChange(() => this.AvgWaterColumnUpdateToDate);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public HomeViewModel()
        {
            // Auto Update
            IsCheckingForUpdates = false;
            AvgWaterColumnUpdateToDate = "Checking for an update...";
            AvgWaterColumnUpdateUrl = "";
            AvgWaterColumnVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

            // Set the list
            CommPortList = SerialOptions.PortOptions;
            BaudRateList = SerialOptions.BaudRateOptions;

            // Setup plot
            VelPlot = new BinPlot3D();
            VelPlot.CylinderRadius = 0;
            VelPlot.ColormapBrushSelection = ColormapBrush.ColormapBrushEnum.Jet;
            VelPlot.MinVelocity = Settings.Default.MinVelocity;
            VelPlot.MaxVelocity = Settings.Default.MaxVelocity;

            Legend = new ContourPlotLegendViewModel(VelPlot.ColormapBrushSelection, VelPlot.MinVelocity, VelPlot.MaxVelocity);

            PlotSize = Settings.Default.PlotSize;

            _maxShipVelocity = 0.0;

            // Vessel Mount Options
            _prevBtEast = DataSet.Ensemble.BAD_VELOCITY;
            _prevBtNorth = DataSet.Ensemble.BAD_VELOCITY;
            _prevBtVert = DataSet.Ensemble.BAD_VELOCITY;
            _vmOptions = new VesselMountOptions();


            // Try to select any available comm ports
            if (!string.IsNullOrEmpty(Settings.Default.AdcpCommPort))
            {
                _SelectedAdcpCommPort = Settings.Default.AdcpCommPort;
                RaisePropertyChangedEventImmediately("SelectedAdcpCommPort");
            }
            else if (CommPortList.Count > 0)
            {
                _SelectedAdcpCommPort = CommPortList[0];
                RaisePropertyChangedEventImmediately("SelectedAdcpCommPort");
            }

            // Output Serial Port
            if (!string.IsNullOrEmpty(Settings.Default.OutputCommPort))
            {
                _SelectedOutputCommPort = Settings.Default.OutputCommPort;
                RaisePropertyChangedEventImmediately("SelectedOutputCommPort");
            }
            else if (CommPortList.Count > 1)
            {
                _SelectedOutputCommPort = CommPortList[1];
                RaisePropertyChangedEventImmediately("SelectedOutputCommPort");
            }

            // GPS Serial Port
            if (!string.IsNullOrEmpty(Settings.Default.GpsCommPort))
            {
                _SelectedGpsCommPort = Settings.Default.GpsCommPort;
                RaisePropertyChangedEventImmediately("SelectedGpsCommPort");
            }
            else if (CommPortList.Count > 1)
            {
                _SelectedGpsCommPort = CommPortList[1];
                RaisePropertyChangedEventImmediately("SelectedGpsCommPort");
            }

            // Get the latest baud rates
            _IsGpsEnabled = false;
            _SelectedAdcpBaudRate = Settings.Default.AdcpBaud;
            _SelectedOutputBaudRate = Settings.Default.OutputBaud;
            _SelectedGpsBaudRate = Settings.Default.GpsBaud;
            _IsGpsEnabled = Settings.Default.IsGpsEnabled;
            RaisePropertyChangedEventImmediately("SelectedAdcpBaudRate");
            RaisePropertyChangedEventImmediately("SelectedOutputBaudRate");
            RaisePropertyChangedEventImmediately("SelectedGpsBaudRate");
            RaisePropertyChangedEventImmediately("IsGpsEnabled");

            // Create the Serial options
            _adcpSerialOptions = new SerialOptions() { Port = SelectedAdcpCommPort, BaudRate = SelectedAdcpBaudRate };
            _outputSerialOptions = new SerialOptions() { Port = _SelectedOutputCommPort, BaudRate = SelectedOutputBaudRate };
            _gpsSerialOptions = new SerialOptions() { Port = _SelectedGpsCommPort, BaudRate = SelectedGpsBaudRate };

            // Create codec to decode the data
            // Subscribe to process event
            _binaryCodec = new AdcpBinaryCodecNew();
            _binaryCodec.ProcessDataEvent += new AdcpBinaryCodecNew.ProcessDataEventHandler(_binaryCodec_ProcessDataEvent);

            // Initialize the list to accumulate a running average
            _runningAvgBuffer = new List<AvgData>();

            // Initialize output buffer
            _outputBuffer = new AvgDataBuffer();

            // Create the timer
            CreateTimer();

            // Try to connect the serial ports
            ConnectAdcp();
            ConnectOutputSerialPort();
            if (_IsGpsEnabled)
            {
                ConnectGpsSerialPort();
            }

            // Settings ViewModel
            SettingsVM = new SettingsViewModel(this);

            CheckForUpdates();
        }


        #region Scan Commands

        /// <summary>
        /// Update the list of serial ports.
        /// </summary>
        private void ScanSerialPorts()
        {
            CommPortList = SerialOptions.PortOptions;
        }

        /// <summary>
        /// Update the Output serial port list.
        /// </summary>
        public void ScanAdcpSerialPortsCmd()
        {
            ScanSerialPorts();
        }

        /// <summary>
        /// Update the Output serial port list.
        /// </summary>
        public void ScanOutputSerialPortsCmd()
        {
            ScanSerialPorts();
        }

        /// <summary>
        /// Update the GPS serial port list.
        /// </summary>
        public void ScanGpsSerialPortsCmd()
        {
            ScanSerialPorts();
        }

        #endregion

        #region ADCP Serial Port

        /// <summary>
        /// Connect the ADCP to the serial port.
        /// </summary>
        private void ConnectAdcp()
        {
            // Create the connection and connect
            _adcpSerialPort = new AdcpSerialPort(_adcpSerialOptions);
            _adcpSerialPort.Connect();

            // Clear the codec
            _binaryCodec.ClearIncomingData();

            // Subscribe to receive data
            _adcpSerialPort.ReceiveAdcpSerialDataEvent += new AdcpSerialPort.ReceiveAdcpSerialDataEventHandler(ReceiveAdcpBinaryData);

            Debug.WriteLine(string.Format("ADCP Connect: {0}", _adcpSerialPort.ToString()));
        }

        /// <summary>
        /// Disconnect the serial connection to the ADCP.
        /// </summary>
        private void DisconnectAdcp()
        {
            if (_adcpSerialPort != null)
            {
                Debug.WriteLine(string.Format("ADCP Disconnect: {0}", _adcpSerialPort.ToString()));

                // Unsubscribe from receiving data
                _adcpSerialPort.ReceiveAdcpSerialDataEvent -= ReceiveAdcpBinaryData;

                // Shutdown the serial port
                _adcpSerialPort.Dispose();

                // Set to null
                _adcpSerialPort = null;
            }
        }

        /// <summary>
        /// Reconnect the ADCP.
        /// </summary>
        private void ReconnectAdcp()
        {
            DisconnectAdcp();

            // Wait for Disconnect to finish
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            ConnectAdcp();
        }

        #endregion

        #region Output Serial Port

        /// <summary>
        /// Create the output serial port.
        /// </summary>
        private void ConnectOutputSerialPort()
        {
            // Create the connection
            _outputSerialPort = new AdcpSerialPort(_outputSerialOptions);
            _outputSerialPort.Connect();

            Debug.WriteLine(string.Format("ADCP Connect: {0}", _outputSerialPort.ToString()));
        }

        /// <summary>
        /// Disconnect the output serial port.
        /// </summary>
        private void DisconnectOutputSerialPort()
        {
            // Shutdown the connection if it exist
            if (_outputSerialPort != null)
            {
                Debug.WriteLine(string.Format("Output SerialPort Disconnect: {0}", _outputSerialPort.ToString()));

                // Shutdown the serial port
                _outputSerialPort.Dispose();

                // Set to null
                _outputSerialPort = null;
            }
        }

        /// <summary>
        /// Reconnect the output serial port.
        /// </summary>
        private void ReconnectOutputSerialPort()
        {
            DisconnectOutputSerialPort();

            // Wait for Disconnect to finish
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            ConnectOutputSerialPort();
        }

        /// <summary>
        /// Send the data through the serial port.
        /// </summary>
        /// <param name="data">Data to send through the serial port.</param>
        private void WriteOutputData(string data)
        {
            try
            {
                // Write out to the serial port
                if (_outputSerialPort != null && _outputSerialPort.IsAvailable())
                {
                    _outputSerialPort.SendData(data);
                }

                // Set the output string
                OutputStr = data;
            }
            catch (Exception)
            {
                // Do nothing then
            }
        }

        #endregion

        #region GPS Serial Port

        /// <summary>
        /// Create the GPS serial port.
        /// </summary>
        private void ConnectGpsSerialPort()
        {
            // Create the connection
            _gpsSerialPort = new GpsSerialPort(_gpsSerialOptions);
            _gpsSerialPort.Connect();

            // Subscribe to receive data
            _gpsSerialPort.ReceiveGpsSerialDataEvent += _gpsSerialPort_ReceiveGpsSerialDataEvent;

            Debug.WriteLine(string.Format("GPS Connect: {0}", _gpsSerialPort.ToString()));
        }

        /// <summary>
        /// Receive the GPS data and pass it to the Binary codec.
        /// </summary>
        /// <param name="data"></param>
        private void _gpsSerialPort_ReceiveGpsSerialDataEvent(string data)
        {
            _binaryCodec.AddNmeaData(data);
        }

        /// <summary>
        /// Disconnect the GPS serial port.
        /// </summary>
        private void DisconnectGpsSerialPort()
        {
            // Shutdown the connection if it exist
            if (_gpsSerialPort != null)
            {
                Debug.WriteLine(string.Format("GPS SerialPort Disconnect: {0}", _gpsSerialPort.ToString()));

                _gpsSerialPort.ReceiveGpsSerialDataEvent -= _gpsSerialPort_ReceiveGpsSerialDataEvent;

                // Shutdown the serial port
                _gpsSerialPort.Dispose();

                // Set to null
                _gpsSerialPort = null;
            }
        }

        /// <summary>
        /// Reconnect the GPS serial port.
        /// </summary>
        private void ReconnectGpsSerialPort()
        {
            DisconnectGpsSerialPort();

            // Wait for Disconnect to finish
            Thread.Sleep(RTI.AdcpSerialPort.WAIT_STATE);

            ConnectGpsSerialPort();
        }

        #endregion

        #region ADCP Commands

        /// <summary>
        /// Get all the commands from the Command set.  Then convert it to an array by spliting
        /// on the new lines.  Each line should be a single command.
        /// Then send the command array to the ADCP serial port.
        /// </summary>
        private void SendCommandSetToAdcp()
        {
            // Get the command set
            string adcpCommandSet = Settings.Default.AdcpCommands;

            // Verify there are any commands
            if (!string.IsNullOrEmpty(adcpCommandSet))
            {
                string[] result = adcpCommandSet.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Remove all line feed, carrage returns, new lines and tabs
                for (int x = 0; x < result.Length; x++)
                {
                    result[x] = result[x].Replace("\n", String.Empty);
                    result[x] = result[x].Replace("\r", String.Empty);
                    result[x] = result[x].Replace("\t", String.Empty);
                }

                _adcpSerialPort.SendCommands(result.ToList());
            }
        }

        #endregion

        #region Auto Update

        /// <summary>
        /// Check for updates to the application.  This will download the version of the application from 
        /// website/pulse/Pulse_AppCast.xml.  It will then check the version against the verison of this application
        /// set in Properties->AssemblyInfo.cs.  If the one on the website is greater, it will display a message 
        /// to update the application.
        /// 
        /// Also subscribe to the event to determine if an update is necssary.
        /// </summary>
        private void CheckForUpdates()
        {
            string url = @"http://www.rowetechinc.co/pulse/AverageWaterColumn_AppCast.xml";

            try
            {
                WebRequest request = WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response != null && response.StatusCode == HttpStatusCode.OK && response.ResponseUri == new System.Uri(url))
                {
                    AutoUpdater.Start(url);
                    AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
                }
                response.Close();
            }
            catch (System.Net.WebException e)
            {
                // No Internet connection, so do nothing
                log.Error("No Internet connection to check for updates.", e);
            }
            catch (Exception e)
            {
                log.Error("Error checking for an update on the web.", e);
            }
        }

        /// <summary>
        /// Event handler for the AutoUpdater.   This will get if an update is available
        /// and if so, which version is available.
        /// </summary>
        /// <param name="args">Results for checking if an update exist.</param>
        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null)
            {
                if (!args.IsUpdateAvailable)
                {
                    AvgWaterColumnUpdateToDate = string.Format("Rowe Technologies Inc. - Average Water Column  is up to date");
                    AvgWaterColumnUpdateUrl = "";
                }
                else
                {
                    AvgWaterColumnUpdateToDate = string.Format("Rowe Technologies Inc. - Average Water Column version {0} is available", args.CurrentVersion);
                    AvgWaterColumnUpdateUrl = args.DownloadURL;
                }
                // Unsubscribe
                AutoUpdater.CheckForUpdateEvent -= AutoUpdaterOnCheckForUpdateEvent;
                IsCheckingForUpdates = false;


                if (args.IsUpdateAvailable)
                {
                    MessageBoxResult dialogResult;
                    if (args.Mandatory)
                    {
                        dialogResult =
                            MessageBox.Show(@"There is new version " + args.CurrentVersion + "  available. \nYou are using version " + args.InstalledVersion + ". \nThis is required update. \nPress Ok to begin updating the application.",
                                            @"Update Available",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Information);
                    }
                    else
                    {
                        dialogResult =
                            MessageBox.Show(
                                @"There is new version " + args.CurrentVersion + " available. \nYou are using version " + args.InstalledVersion + ".  \nDo you want to update the application now?",
                                @"Update Available",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Information);
                    }

                    if (dialogResult.Equals(MessageBoxResult.Yes))
                    {
                        try
                        {
                            if (AutoUpdater.DownloadUpdate())
                            {
                                //Application.Current.Exit();
                                System.Windows.Application.Current.Shutdown();
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message,
                                exception.GetType().ToString(),
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    //MessageBox.Show(@"There is no update available please try again later.", 
                    //                @"No update available",
                    //                MessageBoxButton.OK,
                    //                MessageBoxImage.Information);
                }
            }
            else
            {
                //MessageBox.Show(
                //        @"There is a problem reaching update server please check your internet connection and try again later.",
                //        @"Update check failed", 
                //        MessageBoxButton.OK,
                //        MessageBoxImage.Error);
            }
        }

        #endregion

        #region Process Data

        /// <summary>
        /// Process the incoming data.
        /// </summary>
        /// <param name="ensemble">Ensemble to process.</param>
        private void ProcessIncomingData(DataSet.Ensemble ensemble)
        {
            // Get the latest ensemble numer
            string ensNum = ensemble.EnsembleData.EnsembleNumber.ToString();

            // Remove the ship speed
            ScreenData.RemoveShipSpeed.RemoveVelocity(ref ensemble, _prevBtEast, _prevBtNorth, _prevBtVert, true, true);

            // Mark bad below button
            ScreenData.ScreenMarkBadBelowBottom.Screen(ref ensemble);

            // Check if the data needs to be reprocessed with the GPS heading instead of the ADCP heading
            if (Settings.Default.SelectedHeadingSource != "ADCP")
            {
                // Ensure GPS data exists
                if (ensemble.IsNmeaAvail && ensemble.NmeaData.IsGphdtAvail())
                {
                    Transform.ProfileTransform(ref ensemble, AdcpCodec.CodecEnum.Binary, 0.25f, Transform.HeadingSource.GPS1);
                    Transform.BottomTrackTransform(ref ensemble, AdcpCodec.CodecEnum.Binary, 0.9f, 10f, Transform.HeadingSource.GPS1);
                }
            }
            
            // Generate the averaged data
            AvgData avgData = AverageWaterColumn(ensemble);

            // Get the Ship data
            ShipData shipData = GetShipData(ensemble);

            // Set the max velocity for the plot
            VelPlot.MaxVelocity = avgData.MaxVel;

            // Accumulate the data
            // Keep the list length for the running average
            while (_runningAvgBuffer.Count >= Settings.Default.MaxRunningAvgCount)
            {
                _runningAvgBuffer.RemoveAt(0);
            }

            // Add the data to the list
            _runningAvgBuffer.Add(avgData);

            // Calculate the running average
            AvgData runningAvg = CalculateRunningAverage();

            // Construct the output
            StringBuilder sb = new StringBuilder();
            sb.Append("$RTIAWC,");                             // ID
            sb.Append(ensNum + ",");                           // Ensemble number
            sb.Append(runningAvg.AvgVel + ",");                // Average velocity
            sb.Append(runningAvg.AvgDir + ",");                // Average Direction Y-Axis = North
            sb.Append(runningAvg.MaxVel + ",");                // Maximum Velocity
            sb.Append(runningAvg.MinBin + ",");                // Minimum Bin
            sb.Append(runningAvg.MaxBin + ",");                // Maximum Bin
            sb.Append(_runningAvgBuffer.Count);                // Number of running average data accumulated
            sb.Append("\n");

            // Set the data to the output buffer
            // This data will be displayed when the timer ticks
            _outputBuffer.Data = runningAvg;
            _outputBuffer.DataStr = sb.ToString();
            _outputBuffer.ShipData = shipData;

            // Keep track of the previous good bttom track data
            if (ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.IsEarthVelocityGood())
            {
                _prevBtEast = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_EAST_INDEX];
                _prevBtNorth = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_NORTH_INDEX];
                _prevBtVert = ensemble.BottomTrackData.EarthVelocity[DataSet.Ensemble.BEAM_VERTICAL_INDEX];
            }
        }


        /// <summary>
        /// Average the water column data.
        /// </summary>
        /// <param name="ensemble">Ensemble to average.</param>
        /// <returns>Averaged data.</returns>
        private AvgData AverageWaterColumn(DataSet.Ensemble ensemble)
        {
            AvgData avgData = new AvgData();

            int minBin = Settings.Default.MinBin;                   // Get the minimum bin from the settings
            int maxBin = FindMaxBin(ensemble);                      // Get the maximum bin from the settings for from BT
            avgData.MinBin = minBin;
            avgData.MaxBin = maxBin;

            if (Settings.Default.SelectedTransform == "EARTH")
            {
                // Calculate the average for the water column
                if (ensemble.IsEarthVelocityAvail && ensemble.EarthVelocityData.IsVelocityVectorAvail)
                {
                    avgData = AverageData(ensemble.EarthVelocityData.VelocityVectors, minBin, maxBin);
                    SetWaterDataList(ensemble.EarthVelocityData.VelocityVectors);
                }
                else if (ensemble.IsEarthVelocityAvail && !ensemble.EarthVelocityData.IsVelocityVectorAvail)
                {
                    // Velocity vectors not created, so create now
                    DataSet.VelocityVectorHelper.CreateVelocityVector(ref ensemble);

                    avgData = AverageData(ensemble.EarthVelocityData.VelocityVectors, minBin, maxBin);
                    SetWaterDataList(ensemble.EarthVelocityData.VelocityVectors);
                }

                // Velocity 3D plot
                VelPlot.AddIncomingData(DataSet.VelocityVectorHelper.GetEarthVelocityVectors(ensemble));

            }
            else
            {
                // Calculate the average for the water column
                if (ensemble.IsInstrumentVelocityAvail && ensemble.InstrumentVelocityData.IsVelocityVectorAvail)
                {
                    avgData = AverageData(ensemble.InstrumentVelocityData.VelocityVectors, minBin, maxBin);
                    SetWaterDataList(ensemble.InstrumentVelocityData.VelocityVectors);
                }
                else if (ensemble.IsInstrumentVelocityAvail && !ensemble.InstrumentVelocityData.IsVelocityVectorAvail)
                {
                    // Velocity vectors not created, so create now
                    DataSet.VelocityVectorHelper.CreateVelocityVector(ref ensemble);

                    avgData = AverageData(ensemble.InstrumentVelocityData.VelocityVectors, minBin, maxBin);
                    SetWaterDataList(ensemble.InstrumentVelocityData.VelocityVectors);
                }

                // Velocity 3D plot
                VelPlot.AddIncomingData(DataSet.VelocityVectorHelper.GetInstrumentVelocityVectors(ensemble));
            }

            return avgData;
        }

        /// <summary>
        /// Collect the ship data. This will use the GPS data as a priority.  It will then use the
        /// Bottom Track as a backup.
        /// </summary>
        /// <param name="ensemble">Ensemble data.</param>
        /// <returns>Ship data.</returns>
        private ShipData GetShipData(DataSet.Ensemble ensemble)
        {
            ShipData shipData = new ShipData();

            if(ensemble.IsNmeaAvail)
            {
                // Set the ship speed
                if(ensemble.NmeaData.IsGpvtgAvail() && ensemble.NmeaData.GPVTG.IsValid)
                {
                    shipData.ShipVel = ensemble.NmeaData.GPVTG.Speed.ToMetersPerSecond().Value;
                }
                else if(ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.IsEarthVelocityGood())
                {
                    shipData.ShipVel = ensemble.BottomTrackData.GetVelocityMagnitude();
                }
                // Set the max velocity
                _maxShipVelocity = Math.Max(_maxShipVelocity, shipData.ShipVel);
                shipData.ShipMaxVel = _maxShipVelocity;


                // Set the ship direction
                if (ensemble.NmeaData.IsGphdtAvail() && ensemble.NmeaData.GPHDT.IsValid)
                {
                    shipData.ShipDir = ensemble.NmeaData.GPHDT.Heading.DecimalDegrees;
                }
                else if (ensemble.NmeaData.IsGpvtgAvail() && ensemble.NmeaData.GPVTG.IsValid)
                {
                    shipData.ShipDir = ensemble.NmeaData.GPVTG.Bearing.DecimalDegrees;
                }
                else if (ensemble.IsBottomTrackAvail && ensemble.BottomTrackData.IsEarthVelocityGood())
                {
                    shipData.ShipDir = ensemble.BottomTrackData.GetVelocityDirection(true);
                }
            }

            return shipData;
        }

        /// <summary>
        /// Find the maximum bin.
        /// </summary>
        /// <param name="ensemble">Ensemble to find the maximum bin.</param>
        /// <returns>Maximum bin to use.</returns>
        private int FindMaxBin(DataSet.Ensemble ensemble)
        {
            int bin = ensemble.EnsembleData.NumBins;

            // Use the fixed value if the user selected
            if (Settings.Default.UseFixedMaxBin && Settings.Default.MaxBin <= ensemble.EnsembleData.NumBins)
            {
                bin = Settings.Default.MaxBin;
            }
            // If bottom track is available
            else if (ensemble.IsBottomTrackAvail)
            {
                // Range = BtDepth * cos(BeamAngle)
                // Max Bin = (Range / binLength) - 1Bin
                float binLength = ensemble.AncillaryData.BinSize;
                int beamAngle = 20;          // Temp until find actual
                double range = ensemble.BottomTrackData.GetAverageRange() * Math.Cos(beamAngle);

                // Check for an error
                if (binLength != 0)
                {
                    bin = (int)Math.Round((range / binLength) - 1.0);
                }
            }

            // If it can not be calculated
            // Used the total number of bins
            if (bin <= 0)
            {
                bin = ensemble.EnsembleData.NumBins;
            }

            return bin;
        }

        /// <summary>
        /// Calculate a running average.
        /// This will average all the data within
        /// the average data buffer.
        /// </summary>
        /// <returns>Average of all the accumlated data.</returns>
        private AvgData CalculateRunningAverage()
        {
            AvgData avgData = new AvgData();
            double maxVel = 0.0;
            double accumAvgVel = 0.0;
            double accumAvgDir = 0.0;

            if (_runningAvgBuffer.Count > 0)
            {
                // Accumulate the data
                for (int x = 0; x < _runningAvgBuffer.Count; x++)
                {
                    // Set the maximum velocity
                    maxVel = Math.Max(_runningAvgBuffer[x].MaxVel, maxVel);

                    // Set the Min and Max bin
                    avgData.MinBin = _runningAvgBuffer[x].MinBin;
                    avgData.MaxBin = _runningAvgBuffer[x].MaxBin;

                    // Accumulate the average data
                    accumAvgVel += _runningAvgBuffer[x].AvgVel;
                    accumAvgDir += _runningAvgBuffer[x].AvgDir;
                }

                // Calculate the average
                avgData.AvgVel = accumAvgVel / _runningAvgBuffer.Count;
                avgData.AvgDir = accumAvgDir / _runningAvgBuffer.Count;
                avgData.MaxVel = maxVel;
            }

            return avgData;
        }

        /// <summary>
        /// Average the velocity vector data.
        /// </summary>
        /// <param name="vv">Velocity vectors to average.</param>
        /// <param name="minBin">Minimum bin.</param>
        /// <param name="maxBin">Maximum bin.</param>
        /// <returns>Average data.</returns>
        private AvgData AverageData(DataSet.VelocityVector[] vv, int minBin, int maxBin)
        {
            AvgData avgData = new AvgData();

            double accumVel = 0.0;
            double accumDir = 0.0;
            double maxVel = 0.0;

            // Keep a count of the accumulated data
            int count = 0;

            // Accumulate the velocities and directions
            for (int x = minBin; x < maxBin; x++)
            {
                if (vv[x].Magnitude != DataSet.Ensemble.BAD_VELOCITY)
                {

                    // Check for maximum velocity
                    if (vv[x].Magnitude > maxVel)
                    {
                        maxVel = vv[x].Magnitude;
                    }

                    // Accumulate the magnitude
                    accumVel += vv[x].Magnitude;

                    // Accumulate the direction
                    // Use Y as north
                    accumDir += vv[x].DirectionYNorth;

                    // Increment counter
                    count++;
                }
            }

            // Set the values if an average could be taken
            if (count > 0)
            {
                avgData.AvgVel = accumVel / count;
                avgData.AvgDir = accumDir / count;
            }

            // Set the values
            avgData.MaxVel = maxVel;
            avgData.MinBin = minBin;
            avgData.MaxBin = maxBin;

            return avgData;
        }

        /// <summary>
        /// Set the WaterData list.
        /// </summary>
        /// <param name="vv">Velocity Vectors to set the values.</param>
        private void SetWaterDataList(RTI.DataSet.VelocityVector[] vv)
        {
            // Create a new list
            WaterDataList = new List<WaterData>(); 

            if (vv.Length > 0)
            {
                for (int x = 0; x < vv.Count(); x++)
                {
                    // Check for bad value for mag
                    string mag = "-";
                    if (vv[x].Magnitude != DataSet.Ensemble.BAD_VELOCITY)
                    {
                        mag = vv[x].Magnitude.ToString("0.00");
                    }

                    // Check for bad value for dir
                    string dir = "-";
                    if (vv[x].DirectionYNorth != DataSet.Ensemble.BAD_VELOCITY)
                    {
                        dir = vv[x].DirectionYNorth.ToString("0.00");
                    }

                    // Add the data to the list
                    WaterDataList.Add(new WaterData() { Bin = x + 1, Dir = dir, Mag = mag});
                }
            }

            this.RaisePropertyChangedEventImmediately("WaterDataList");
        }

        #endregion

        #region EventHandlers

        #region Serial Port

        /// <summary>
        /// Receive binary data from the ADCP serial port.
        /// Then pass the binary data to the codec to decode the
        /// data into ensembles.
        /// 
        /// The data could be binary or dvl data.
        /// The data will go to both codec and
        /// if the codec can process the data it will.
        /// </summary>
        /// <param name="data">Data to decode.</param>
        public void ReceiveAdcpBinaryData(byte[] data)
        {
            // Add the data to the binary codec
            _binaryCodec.AddIncomingData(data);

            //// Add the data to the dvl codec
            //_dvlCodec.AddIncomingData(data);

            //// Add the data to the PD0 codec
            //_pd0Codec.AddIncomingData(data);
        }

        #endregion

        #region Pinging

        /// <summary>
        /// Send the command to start pinging.
        /// </summary>
        public void StartPingingCmd()
        {
            // Send commands from the list of commands 
            SendCommandSetToAdcp();

            // Start pinging
            _adcpSerialPort.StartPinging();
        }

        /// <summary>
        /// Send the command to stop pinging.
        /// </summary>
        public void StopPingingCmd()
        {
            _adcpSerialPort.StopPinging();
        }

        #endregion

        #region Codec

        /// <summary>
        /// Receive decoded data from the Binary codec.  This will be 
        /// the latest data decoded.  It will include the complete
        /// binary array of the data and the ensemble object.
        /// </summary>
        /// <param name="binaryEnsemble">Binary data of the ensemble.</param>
        /// <param name="ensemble">Ensemble object.</param>
        void _binaryCodec_ProcessDataEvent(byte[] binaryEnsemble, DataSet.Ensemble ensemble)
        {
            ProcessIncomingData(ensemble);
        }

        #endregion

        #endregion

        #region Deactivate

        /// <summary>
        /// Shutdown the view model.
        /// </summary>
        /// <param name="close">Flag to allow to close.</param>
        public void Deactivate(bool close)
        {
            DisconnectAdcp();
            DisconnectOutputSerialPort();
            _binaryCodec.Dispose();
        }

        /// <summary>
        /// Not used.
        /// </summary>
        public event EventHandler<DeactivationEventArgs> Deactivated = null;

        #endregion

        #region Timer

        /// <summary>
        /// Create the timer.
        /// </summary>
        public void CreateTimer()
        {
            // Craete the timer
            TimerCallback callback = TimerTick;
            _timer = new System.Threading.Timer(callback, null, 0, Settings.Default.OutputSpeed);
        }

        /// <summary>
        /// Output the data when the timer goes off.
        /// </summary>
        /// <param name="stateInfo">Not used.</param>
        void TimerTick(object stateInfo)
        {
            // Write output to serial port
            WriteOutputData(_outputBuffer.DataStr);
            Debug.WriteLine(_outputBuffer.DataStr);

            // Update the Average Water display
            _AvgVel = _outputBuffer.Data.AvgVel;
            _AvgDir = _outputBuffer.Data.AvgDir;
            _MaxVel = _outputBuffer.Data.MaxVel;
            RaisePropertyChangedEventImmediately("AvgVel");
            RaisePropertyChangedEventImmediately("AvgDir");
            RaisePropertyChangedEventImmediately("MaxVel");

            // Update the Ship display
            _ShipVel = _outputBuffer.ShipData.ShipVel;
            _ShipDir = _outputBuffer.ShipData.ShipDir;
            _ShipMaxVel = _outputBuffer.ShipData.ShipMaxVel;
            RaisePropertyChangedEventImmediately("ShipVel");
            RaisePropertyChangedEventImmediately("ShpDir");
            RaisePropertyChangedEventImmediately("ShipMaxVel");


        }

        #endregion

    }
}
