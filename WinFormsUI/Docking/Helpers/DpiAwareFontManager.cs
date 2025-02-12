#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public sealed class DpiAwareFontManager
{
    private readonly Font m_protoFont;
    private readonly Font m_protoIconFont;
    private readonly Control m_control;

    private Font? m_font;
    private Font? m_iconFont;

    public DpiAwareFontManager(Font font, Font iconFont, DockPanel control)
    {
        m_protoFont = font;
        m_protoIconFont = iconFont;
        m_control = control;

        control.DpiChangedBeforeParent += Control_DpiChangedAfterParent;
        control.Disposed += Control_Disposed;

        void Control_Disposed(object? sender, EventArgs e)
        {
            m_font?.Dispose();
            m_iconFont?.Dispose();
            m_font = null;
        }

        void Control_DpiChangedAfterParent(object? sender, EventArgs e)
        {
            m_font?.Dispose();
            m_iconFont.Dispose();
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

    public Font IconFont
    {
        get
        {
            if (m_iconFont == null)
            {
                var scale = m_protoIconFont.GetHeight(m_control.DeviceDpi) / m_protoIconFont.GetHeight();
                m_iconFont = m_protoIconFont.WithSize(m_protoIconFont.Size * scale);
            }
            return m_iconFont;
        }
    }
}
