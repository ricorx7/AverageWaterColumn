 /* All rights reserved.
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
 *       
 * 
 */


using System.Windows.Controls;

namespace RTI
{
    /// <summary>
    /// Interaction logic for ContourPlotLegendView.xaml
    /// </summary>
    public partial class ContourPlotLegendView : UserControl
    {
        /// <summary>
        /// Viewmodel for this view.
        /// </summary>
        private ContourPlotLegendViewModel _viewModel;
        /// <summary>
        /// Viewmodel for this view.
        /// </summary>
        public ContourPlotLegendViewModel ViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                // Set the data context
                this._viewModel = value;
                this.DataContext = value;
            }
        }

        /// <summary>
        /// Constructor
        /// Usings MEF dependency objects.
        /// </summary>
        /// <param name="vm">Get the view model through dependency objects.</param>
        public ContourPlotLegendView(ContourPlotLegendViewModel vm)
        {
            InitializeComponent();

            ViewModel = vm;
        }
    }
}
