#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public sealed class DpiAwareFontManager
{
    private readonly Font m_protoFont;
    private readonly Control m_control;

    private Font? m_font;

    public DpiAwareFontManager(Font font, DockPanel control)
    {
        m_protoFont = font;
        m_control = control;

        control.DpiChangedBeforeParent += Control_DpiChangedAfterParent;
        control.Disposed += Control_Disposed;

        void Control_Disposed(object? sender, EventArgs e)
        {
            m_font?.Dispose();
            m_font = null;
        }

        void Control_DpiChangedAfterParent(object? sender, EventArgs e)
        {
            m_font?.Dispose();
            m_font = null;
        }
    }

    public Font Font
    {
        get
        {
            if (m_font == null)
            {
                var scale = m_protoFont.GetHeight(m_control.DeviceDpi) / m_protoFont.GetHeight();
                m_font = m_protoFont.WithSize(m_protoFont.Size * scale);
            }
            return m_font;
        }
    }
}
