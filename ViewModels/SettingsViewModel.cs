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
 * 
 * 
 */

namespace RTI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AverageWaterColumn.Properties;
    using Caliburn.Micro;

    /// <summary>
    /// Settings for the application.
    /// This will also be saved to Settings.settings.
    /// </summary>
    public class SettingsViewModel : PropertyChangedBase
    {
        #region Properties

        /// <summary>
        /// Flag if a fixed Max bin used be used.
        /// </summary>
        private bool _IsUseFixedMaxBin;
        /// <summary>
        /// Flag if a fixed Max bin used be used.
        /// </summary>
        public bool IsUseFixedMaxBin
        {
            get { return _IsUseFixedMaxBin; }
            set
            {
                _IsUseFixedMaxBin = value;

                // Save the option
                Settings.Default.UseFixedMaxBin = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("IsUseFixedMaxBin");
            }
        }

        /// <summary>
        /// Minimum bin to be used.  Usually it is 0.
        /// </summary>
        private int _MinBin;
        /// <summary>
        /// Minimum bin to be used.  Usually it is 0.
        /// </summary>
        public int MinBin
        {
            get { return _MinBin; }
            set
            {
                _MinBin = value;

                // Save the option
                Settings.Default.MinBin = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("MinBin");
            }
        }

        /// <summary>
        /// Maximum bin to be used.
        /// </summary>
        private int _MaxBin;
        /// <summary>
        /// Maximum bin to be used.
        /// </summary>
        public int MaxBin
        {
            get { return _MaxBin; }
            set
            {
                _MaxBin = value;

                // Save the option
                Settings.Default.MaxBin = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("MaxBin");
            }
        }

        /// <summary>
        /// Commands to send to the ADCP.
        /// </summary>
        private string _AdcpCommands;
        /// <summary>
        /// Commands to send to the ADCP.
        /// </summary>
        public string AdcpCommands
        {
            get { return _AdcpCommands; }
            set
            {
                _AdcpCommands = value;

                // Save the option
                Settings.Default.AdcpCommands = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("AdcpCommands");
            }
        }

        /// <summary>
        /// Output time in milliseconds.
        /// </summary>
        private int _OutputSpeed;
        /// <summary>
        /// Output time in milliseconds.
        /// </summary>
        public int OutputSpeed
        {
            get { return _OutputSpeed; }
            set
            {
                _OutputSpeed = value;

                // Save the option
                Settings.Default.OutputSpeed = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("OutputSpeed");
            }
        }

        /// <summary>
        /// Maximum running Average Count.
        /// </summary>
        private int _MaxRunningAvgCount;
        /// <summary>
        /// Maximum running Average Count.
        /// </summary>
        public int MaxRunningAvgCount
        {
            get { return _MaxRunningAvgCount; }
            set
            {
                _MaxRunningAvgCount = value;

                // Save the option
                Settings.Default.MaxRunningAvgCount = value;
                Settings.Default.Save();

                RaisePropertyChangedEventImmediately("MaxRunningAvgCount");
            }
        }
        #endregion

        /// <summary>
        /// Initialize the values.
        /// </summary>
        public SettingsViewModel()
        {
            _MinBin = Settings.Default.MinBin;
            _MaxBin = Settings.Default.MaxBin;
            _IsUseFixedMaxBin = Settings.Default.UseFixedMaxBin;
            _AdcpCommands = Settings.Default.AdcpCommands;
            _OutputSpeed = Settings.Default.OutputSpeed;
            _MaxRunningAvgCount = Settings.Default.MaxRunningAvgCount;

            RaisePropertyChangedEventImmediately("MinBin");
            RaisePropertyChangedEventImmediately("MaxBin");
            RaisePropertyChangedEventImmediately("IsUseFixedMaxBin");
            RaisePropertyChangedEventImmediately("AdcpCommands");
            RaisePropertyChangedEventImmediately("OutputSpeed");
            RaisePropertyChangedEventImmediately("MaxRunningAvgCount");
        }

    }
}
