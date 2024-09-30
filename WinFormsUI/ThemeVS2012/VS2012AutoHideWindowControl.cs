using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.ThemeVS2012
{
    using Docking;
    using System.ComponentModel;

    [ToolboxItem(false)]
    internal class VS2012AutoHideWindowControl : DockPanel.AutoHideWindowControl
    {
        public VS2012AutoHideWindowControl(DockPanel dockPanel)
            : base(dockPanel)
        {
        }

        protected override Rectangle DisplayingRectangle
        {
            get
            {
                Rectangle rect = ClientRectangle;

                // exclude the border and the splitter
                var splitterSize = LogicalToDeviceUnits(DockPanel.Theme.Measures.AutoHideSplitterSize);
                if (DockState == DockState.DockBottomAutoHide)
                {
                    rect.Y += splitterSize;
                    rect.Height -= splitterSize;
                }
                else if (DockState == DockState.DockRightAutoHide)
                {
                    rect.X += splitterSize;
                    rect.Width -= splitterSize;
                }
                else if (DockState == DockState.DockTopAutoHide)
                {
                    rect.Height -= splitterSize;
                }
                else if (DockState == DockState.DockLeftAutoHide)
                {
                    rect.Width -= splitterSize;
                }

                return rect;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            DockPadding.All = 0;
            if (DockState == DockState.DockLeftAutoHide)
            {
                m_splitter.Dock = DockStyle.Right;
            }
            else if (DockState == DockState.DockRightAutoHide)
            {
                m_splitter.Dock = DockStyle.Left;
            }
            else if (DockState == DockState.DockTopAutoHide)
            {
                m_splitter.Dock = DockStyle.Bottom;
            }
            else if (DockState == DockState.DockBottomAutoHide)
            {
                m_splitter.Dock = DockStyle.Top;
            }

            Rectangle rectDisplaying = DisplayingRectangle;
            Rectangle rectHidden = new Rectangle(-rectDisplaying.Width, rectDisplaying.Y, rectDisplaying.Width, rectDisplaying.Height);
            foreach (Control c in Controls)
            {
                DockPane pane = c as DockPane;
                if (pane == null)
                    continue;

                if (pane == ActivePane)
                    pane.Bounds = rectDisplaying;
                else
                    pane.Bounds = rectHidden;
            }

            base.OnLayout(levent);
        }
    }
}