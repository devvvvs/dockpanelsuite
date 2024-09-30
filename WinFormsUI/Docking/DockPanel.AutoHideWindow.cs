using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Threading;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        [ToolboxItem(false)]
        public class AutoHideWindowControl : Panel, ISplitterHost
        {
            protected class SplitterControl : SplitterBase
            {
                public SplitterControl(AutoHideWindowControl autoHideWindow)
                {
                    m_autoHideWindow = autoHideWindow;
                }

                private AutoHideWindowControl m_autoHideWindow;
                private AutoHideWindowControl AutoHideWindow
                {
                    get { return m_autoHideWindow; }
                }

                protected override int SplitterSize
                {
                    get { return LogicalToDeviceUnits(AutoHideWindow.DockPanel.Theme.Measures.AutoHideSplitterSize); }
                }

                protected override void StartDrag()
                {
                    AutoHideWindow.DockPanel.BeginDrag(AutoHideWindow, AutoHideWindow.RectangleToScreen(Bounds));
                }
            }

            #region consts
            private const int ANIMATE_TIME = 100;    // in mini-seconds
            #endregion


            protected SplitterBase m_splitter { get; private set; }

            public AutoHideWindowControl(DockPanel dockPanel)
            {
                m_dockPanel = dockPanel;

                Visible = false;
                m_splitter = DockPanel.Theme.Extender.WindowSplitterControlFactory.CreateSplitterControl(this);
                Controls.Add(m_splitter);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                }
                base.Dispose(disposing);
            }

            public bool IsDockWindow
            {
                get { return false; }
            }

            private DockPanel m_dockPanel = null;
            public DockPanel DockPanel
            {
                get { return m_dockPanel; }
            }

            private DockPane m_activePane = null;
            public DockPane ActivePane
            {
                get { return m_activePane; }
            }

            private void SetActivePane()
            {
                DockPane value = (ActiveContent == null ? null : ActiveContent.DockHandler.Pane);

                if (value == m_activePane)
                    return;

                m_activePane = value;
            }

            private static readonly object AutoHideActiveContentChangedEvent = new object();
            public event EventHandler ActiveContentChanged
            {
                add { Events.AddHandler(AutoHideActiveContentChangedEvent, value); }
                remove { Events.RemoveHandler(AutoHideActiveContentChangedEvent, value); }
            }

            protected virtual void OnActiveContentChanged(EventArgs e)
            {
                EventHandler handler = (EventHandler)Events[ActiveContentChangedEvent];
                if (handler != null)
                    handler(this, e);
            }

            private IDockContent m_activeContent = null;
            public IDockContent ActiveContent
            {
                get { return m_activeContent; }
                set
                {
                    if (value == m_activeContent)
                        return;

                    if (value != null)
                    {
                        if (!DockHelper.IsDockStateAutoHide(value.DockHandler.DockState) || value.DockHandler.DockPanel != DockPanel)
                            throw (new InvalidOperationException(Strings.DockPanel_ActiveAutoHideContent_InvalidValue));
                    }

                    DockPanel.SuspendLayout();

                    if (m_activeContent != null)
                    {
                        if (m_activeContent.DockHandler.Form.ContainsFocus)
                        {
                            if (!Win32Helper.IsRunningOnMono)
                            {
                                DockPanel.ContentFocusManager.GiveUpFocus(m_activeContent);
                            }
                        }

                        AnimateWindow(false);
                    }

                    m_activeContent = value;
                    SetActivePane();
                    if (ActivePane != null)
                        ActivePane.ActiveContent = m_activeContent;

                    if (m_activeContent != null)
                        AnimateWindow(true);

                    DockPanel.ResumeLayout();
                    DockPanel.RefreshAutoHideStrip();

                    SetTimerMouseTrack();

                    OnActiveContentChanged(EventArgs.Empty);
                }
            }

            public DockState DockState
            {
                get { return ActiveContent == null ? DockState.Unknown : ActiveContent.DockHandler.DockState; }
            }

            private bool m_flagDragging = false;
            internal bool FlagDragging
            {
                get { return m_flagDragging; }
                set
                {
                    if (m_flagDragging == value)
                        return;

                    m_flagDragging = value;
                    SetTimerMouseTrack();
                }
            }

            private void AnimateWindow(bool show)
            {
                if (Visible != show)
                {
                    Visible = show;
                    return;
                }
            }

            private void SetTimerMouseTrack()
            {
                if (ActivePane == null || ActivePane.IsActivated || FlagDragging)
                {
                    return;
                }

                SynchronizationContext.Current.Post(_ => TimerMouseTrack_Tick(), null);
            }

            protected virtual Rectangle DisplayingRectangle
            {
                get
                {
                    Rectangle rect = ClientRectangle;

                    // exclude the border and the splitter
                    var splitterSize = LogicalToDeviceUnits(2 + DockPanel.Theme.Measures.AutoHideSplitterSize);
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
                        rect.Height -= splitterSize;
                    else if (DockState == DockState.DockLeftAutoHide)
                        rect.Width -= splitterSize;

                    return rect;
                }
            }

            public void RefreshActiveContent()
            {
                if (ActiveContent == null)
                    return;

                if (!DockHelper.IsDockStateAutoHide(ActiveContent.DockHandler.DockState))
                {
                    ActiveContent = null;
                }
            }

            public void RefreshActivePane()
            {
                SetTimerMouseTrack();
            }

            private void TimerMouseTrack_Tick()
            {
                if (IsDisposed)
                    return;

                if (ActivePane == null || ActivePane.IsActivated || FlagDragging)
                {
                    return;
                }

                DockPane pane = ActivePane;
                Point ptMouseInAutoHideWindow = PointToClient(Control.MousePosition);
                Point ptMouseInDockPanel = DockPanel.PointToClient(Control.MousePosition);

                Rectangle rectTabStrip = DockPanel.GetTabStripRectangle(pane.DockState);

                if (!ClientRectangle.Contains(ptMouseInAutoHideWindow) && !rectTabStrip.Contains(ptMouseInDockPanel))
                {
                    ActiveContent = null;
                }
            }

            #region ISplitterDragSource Members

            void ISplitterDragSource.BeginDrag(Rectangle rectSplitter)
            {
                FlagDragging = true;
            }

            void ISplitterDragSource.EndDrag()
            {
                FlagDragging = false;
            }

            bool ISplitterDragSource.IsVertical
            {
                get { return (DockState == DockState.DockLeftAutoHide || DockState == DockState.DockRightAutoHide); }
            }

            Rectangle ISplitterDragSource.DragLimitBounds
            {
                get
                {
                    Rectangle rectLimit = DockPanel.DockArea;

                    if ((this as ISplitterDragSource).IsVertical)
                    {
                        rectLimit.X += LogicalToDeviceUnits(MeasurePane.MinSize);
                        rectLimit.Width -= LogicalToDeviceUnits(2 * MeasurePane.MinSize);
                    }
                    else
                    {
                        rectLimit.Y += LogicalToDeviceUnits(MeasurePane.MinSize);
                        rectLimit.Height -= LogicalToDeviceUnits(2 * MeasurePane.MinSize);
                    }

                    return DockPanel.RectangleToScreen(rectLimit);
                }
            }

            void ISplitterDragSource.MoveSplitter(int offset)
            {
                Rectangle rectDockArea = DockPanel.DockArea;
                IDockContent content = ActiveContent;
                if (DockState == DockState.DockLeftAutoHide && rectDockArea.Width > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                        content.DockHandler.AutoHidePortion += ((double)offset) / (double)rectDockArea.Width;
                    else
                        content.DockHandler.AutoHidePortion = Width + offset;
                }
                else if (DockState == DockState.DockRightAutoHide && rectDockArea.Width > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                        content.DockHandler.AutoHidePortion -= ((double)offset) / (double)rectDockArea.Width;
                    else
                        content.DockHandler.AutoHidePortion = Width - offset;
                }
                else if (DockState == DockState.DockBottomAutoHide && rectDockArea.Height > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                        content.DockHandler.AutoHidePortion -= ((double)offset) / (double)rectDockArea.Height;
                    else
                        content.DockHandler.AutoHidePortion = Height - offset;
                }
                else if (DockState == DockState.DockTopAutoHide && rectDockArea.Height > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                        content.DockHandler.AutoHidePortion += ((double)offset) / (double)rectDockArea.Height;
                    else
                        content.DockHandler.AutoHidePortion = Height + offset;
                }
            }

            #region IDragSource Members

            Control IDragSource.DragControl
            {
                get { return this; }
            }

            #endregion

            #endregion
        }

        private AutoHideWindowControl AutoHideWindow
        {
            get { return m_autoHideWindow; }
        }

        internal Control AutoHideControl
        {
            get { return m_autoHideWindow; }
        }

        internal void RefreshActiveAutoHideContent()
        {
            AutoHideWindow.RefreshActiveContent();
        }

        internal Rectangle AutoHideWindowRectangle
        {
            get
            {
                DockState state = AutoHideWindow.DockState;
                Rectangle rectDockArea = DockArea;
                if (ActiveAutoHideContent == null)
                    return Rectangle.Empty;

                if (Parent == null)
                    return Rectangle.Empty;

                Rectangle rect = Rectangle.Empty;
                double autoHideSize = ActiveAutoHideContent.DockHandler.AutoHidePortion;
                var minSize = LogicalToDeviceUnits(MeasurePane.MinSize);
                var dockPadding = LogicalToDeviceUnits(Theme.Measures.DockPadding);
                if (state == DockState.DockLeftAutoHide)
                {
                    if (autoHideSize < 1)
                        autoHideSize = rectDockArea.Width * autoHideSize;
                    if (autoHideSize > rectDockArea.Width - minSize)
                        autoHideSize = rectDockArea.Width - minSize;
                    rect.X = rectDockArea.X - dockPadding;
                    rect.Y = rectDockArea.Y;
                    rect.Width = (int)autoHideSize;
                    rect.Height = rectDockArea.Height;
                }
                else if (state == DockState.DockRightAutoHide)
                {
                    if (autoHideSize < 1)
                        autoHideSize = rectDockArea.Width * autoHideSize;
                    if (autoHideSize > rectDockArea.Width - minSize)
                        autoHideSize = rectDockArea.Width - minSize;
                    rect.X = rectDockArea.X + rectDockArea.Width - (int)autoHideSize + dockPadding;
                    rect.Y = rectDockArea.Y;
                    rect.Width = (int)autoHideSize;
                    rect.Height = rectDockArea.Height;
                }
                else if (state == DockState.DockTopAutoHide)
                {
                    if (autoHideSize < 1)
                        autoHideSize = rectDockArea.Height * autoHideSize;
                    if (autoHideSize > rectDockArea.Height - minSize)
                        autoHideSize = rectDockArea.Height - minSize;
                    rect.X = rectDockArea.X;
                    rect.Y = rectDockArea.Y - dockPadding;
                    rect.Width = rectDockArea.Width;
                    rect.Height = (int)autoHideSize;
                }
                else if (state == DockState.DockBottomAutoHide)
                {
                    if (autoHideSize < 1)
                        autoHideSize = rectDockArea.Height * autoHideSize;
                    if (autoHideSize > rectDockArea.Height - minSize)
                        autoHideSize = rectDockArea.Height - minSize;
                    rect.X = rectDockArea.X;
                    rect.Y = rectDockArea.Y + rectDockArea.Height - (int)autoHideSize + dockPadding;
                    rect.Width = rectDockArea.Width;
                    rect.Height = (int)autoHideSize;
                }

                return rect;
            }
        }

        internal Rectangle GetAutoHideWindowBounds(Rectangle rectAutoHideWindow)
        {
            if (DocumentStyle == DocumentStyle.SystemMdi ||
                DocumentStyle == DocumentStyle.DockingMdi)
                return (Parent == null) ? Rectangle.Empty : Parent.RectangleToClient(RectangleToScreen(rectAutoHideWindow));
            else
                return rectAutoHideWindow;
        }

        internal void RefreshAutoHideStrip()
        {
            AutoHideStripControl.RefreshChanges();
        }
    }
}
