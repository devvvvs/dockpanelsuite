using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WeifenLuo.WinFormsUI.Docking
{
    public static class Win32Helper
    {
        private static readonly bool _isRunningOnMono = Type.GetType("Mono.Runtime") != null;

        public static bool IsRunningOnMono { get { return _isRunningOnMono; } }

        internal static Control ControlAtPoint(Point pt)
        {
            return Control.FromChildHandle(NativeMethods.WindowFromPoint(pt));
        }

        internal static uint MakeLong(int low, int high)
        {
            return (uint)((high << 16) + low);
        }

        internal static uint HitTestCaption(Control control)
        {
            var captionRectangle = new Rectangle(0, 0, control.Width, control.ClientRectangle.Top - control.PointToClient(control.Location).X);
            return captionRectangle.Contains(Control.MousePosition) ? (uint)2 : 0;
        }

        // See https://stackoverflow.com/questions/19237034/c-sharp-need-to-psuedo-click-a-window
        internal static unsafe nint HitTestFix(HWND hWnd, LPARAM lparam)
        {
            var point = MAKEPOINT(lparam);
            var titleBarInfo = new TITLEBARINFOEX();
            titleBarInfo.cbSize = (uint)Marshal.SizeOf(titleBarInfo);
            var pTitleBarInfo = &titleBarInfo;
            PInvoke.SendMessage(new(hWnd), (uint)Win32.Msgs.WM_GETTITLEBARINFOEX, default, new LPARAM((nint)pTitleBarInfo));
            if (PInvoke.PtInRect(titleBarInfo.rgrect._2, point))
                return (nint)PInvoke.HTMINBUTTON;
            if (PInvoke.PtInRect(titleBarInfo.rgrect._3, point))
                return (nint)PInvoke.HTMAXBUTTON;
            if (PInvoke.PtInRect(titleBarInfo.rgrect._5, point))
                return (nint)PInvoke.HTCLOSE;

            return PInvoke.SendMessage(hWnd, PInvoke.WM_NCHITTEST, default, MAKELPARAM(point.X, point.Y));
        }

        private static nint MAKELPARAM(int p, int p_2)
        {
            return ((p_2 << 16) | (p & 0xFFFF));
        }

        private static Point MAKEPOINT(LPARAM lParam)
        {
            return new Point((short)lParam.Value, (short)(lParam.Value >> 16));
        }
    }
}
