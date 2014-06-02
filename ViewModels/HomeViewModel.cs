﻿/*
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

    /// <summary>
    /// Display the options and output for the application.
    /// </summary>
    public class HomeViewModel : PropertyChangedBase, IDeactivate
    {

        #region Structs 

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
            /// String of the latest data.
            /// </summary>
            public string DataStr;
        }

        #endregion

        #region Variables

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
        private AdcpBinaryCodec _binaryCodec;

        /// <summary>
        /// ADCP Serial Port options.
        /// </summary>
        private SerialOptions _outputSerialOptions;

        /// <summary>
        /// Serial port to output the data.
        /// </summary>
        private AdcpSerialPort _outputSerialPort;

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
        /// The maximum number of ensembles to
        /// accumulate for the average.
        /// </summary>
        private int _maxRunningAvgCount;

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

        #endregion

        #region Average Display

        /// <summary>
        /// Average Velocity.
        /// </summary>
        private double _AvgVel;
        /// <summary>
        /// Average Velocity.
        /// </summary>
        public double AvgVel
        {
            get { return _AvgVel; }
            set
            {
                _AvgVel = value;
                RaisePropertyChangedEventImmediately("AvgVel");
            }
        }

        /// <summary>
        /// Average Direction.
        /// </summary>
        private double _AvgDir;
        /// <summary>
        /// Average Direction.
        /// </summary>
        public double AvgDir
        {
            get { return _AvgDir; }
            set
            {
                _AvgDir = value;
                RaisePropertyChangedEventImmediately("AvgDir");
            }
        }

        /// <summary>
        /// Maximum Velocity.
        /// </summary>
        private double _MaxVel;
        /// <summary>
        /// Maximum Velocity.
        /// </summary>
        public double MaxVel
        {
            get { return _MaxVel; }
            set
            {
                _MaxVel = value;
                RaisePropertyChangedEventImmediately("MaxVel");
            }
        }

        #endregion

        #region Settings Display

        /// <summary>
        /// Settings ViewModel.
        /// </summary>
        public SettingsViewModel SettingsVM { get; set; }

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

        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public HomeViewModel()
        {
            // Set the list
            CommPortList = SerialOptions.PortOptions;
            BaudRateList = SerialOptions.BaudRateOptions;

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

            // Get the latest baud rates
            _SelectedAdcpBaudRate = Settings.Default.AdcpBaud;
            _SelectedOutputBaudRate = Settings.Default.OutputBaud;
            RaisePropertyChangedEventImmediately("SelectedAdcpBaudRate");
            RaisePropertyChangedEventImmediately("SelectedOutputBaudRate");

            // Create the Serial options
            _adcpSerialOptions = new SerialOptions() { Port = SelectedAdcpCommPort, BaudRate = SelectedAdcpBaudRate };
            _outputSerialOptions = new SerialOptions() { Port = _SelectedOutputCommPort, BaudRate = SelectedOutputBaudRate };

            // Create codec to decode the data
            // Subscribe to process event
            _binaryCodec = new AdcpBinaryCodec();
            _binaryCodec.ProcessDataEvent += new AdcpBinaryCodec.ProcessDataEventHandler(_binaryCodec_ProcessDataEvent);

            // Initialize the list to accumulate a running average
            _runningAvgBuffer = new List<AvgData>();

            // Initialize output buffer
            _outputBuffer = new AvgDataBuffer();

            // Create the timer
            CreateTimer();

            // Try to connect the serial ports
            ConnectAdcp();
            ConnectOutputSerialPort();

            // Settings ViewModel
            SettingsVM = new SettingsViewModel();
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

        #region Process Data

        /// <summary>
        /// Process the incoming data.
        /// </summary>
        /// <param name="ensemble">Ensemble to process.</param>
        private void ProcessIncomingData(DataSet.Ensemble ensemble)
        {
            string ensNum = ensemble.EnsembleData.EnsembleNumber.ToString();
            AvgData avgData = AverageWaterColumn(ensemble);

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

            _outputBuffer.Data = runningAvg;
            _outputBuffer.DataStr = sb.ToString();
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

            // Calculate the average for the water column
            if (ensemble.IsEarthVelocityAvail && ensemble.EarthVelocityData.IsVelocityVectorAvail)
            {
                avgData = AverageData(ensemble.EarthVelocityData.VelocityVectors, minBin, maxBin);
            }
            else if (ensemble.IsEarthVelocityAvail && !ensemble.EarthVelocityData.IsVelocityVectorAvail)
            {
                // Velocity vectors not created, so create now
                DataSet.VelocityVectorHelper.CreateVelocityVector(ref ensemble);

                avgData = AverageData(ensemble.EarthVelocityData.VelocityVectors, minBin, maxBin);
            }

            return avgData;
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
                int beamAngle = 0;          // Temp until find actual
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
                    if (_runningAvgBuffer[x].MaxVel > maxVel)
                    {
                        maxVel = _runningAvgBuffer[x].MaxVel;
                    }

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

            // Update the display
            AvgVel = _outputBuffer.Data.AvgVel;
            AvgDir = _outputBuffer.Data.AvgDir;
            MaxVel = _outputBuffer.Data.MaxVel;
        }

        #endregion

    }
}