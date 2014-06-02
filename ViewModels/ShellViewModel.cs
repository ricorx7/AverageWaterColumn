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
    using Caliburn.Micro;
    using System.Diagnostics;

    /// <summary>
    /// Interface for the ShellViewModel.
    /// Empty.
    /// </summary>
    public interface IShellViewModel
    {
    }

    /// <summary>
    /// Initialize the application shell.
    /// </summary>
    public class ShellViewModel : Conductor<object>, IShellViewModel
    {
        #region Variables


        #endregion

        ///// <summary>
        ///// Summary
        ///// </summary>
        //private MenuViewModel _MenuVM;
        ///// <summary>
        ///// Summary
        ///// </summary>
        //public MenuViewModel MenuVM
        //{
        //    get { return _MenuVM; }
        //    set
        //    {
        //        _MenuVM = value;
        //        RaisePropertyChangedEventImmediately("MenuVM");
        //    }
        //}

        public ShellViewModel()
        {
            //Title = "Hello Caliburn.Micro";
            //_MenuVM = IoC.Get<MenuViewModel>();
            //_MenuVM = new MenuViewModel();
            //ActivateItem(IoC.Get<HomeViewModel>());
            ActivateItem(new HomeViewModel());
        }
    }
}
