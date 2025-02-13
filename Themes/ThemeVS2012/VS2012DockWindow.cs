using System.ComponentModel;
using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking
{
    /// <summary>
    /// Dock window of Visual Studio 2012 Light theme.
    /// </summary>
    [ToolboxItem(false)]
    internal class VS2012DockWindow : DockWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VS2012DockWindow"/> class.
        /// </summary>
        /// <param name="dockPanel">The dock panel.</param>
        /// <param name="dockState">State of the dock.</param>
        public VS2012DockWindow(DockPanel dockPanel, DockState dockState) : base(dockPanel, dockState)
        {
        }

        public override Rectangle DisplayingRectangle
        {
            get
            {
                Rectangle rect = ClientRectangle;
                var splitterSize = LogicalToDeviceUnits(DockPanel.Theme.Measures.SplitterSize);
                if (DockState == DockState.DockLeft)
                    rect.Width -= splitterSize;
                else if (DockState == DockState.DockRight)
                {
                    rect.X += splitterSize;
                    rect.Width -= splitterSize;
                }
                else if (DockState == DockState.DockTop)
                    rect.Height -= splitterSize;
                else if (DockState == DockState.DockBottom)
                {
                    rect.Y += splitterSize;
                    rect.Height -= splitterSize;
                }

                return rect;
            }
        }
    }
}
