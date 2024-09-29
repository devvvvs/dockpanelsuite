#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace System.Windows.Forms;

internal static class FontExtensions
{
    [return: NotNullIfNotNull("templateFont")]
    public static Font? WithSize(this Font? templateFont, float emSize)
    {
        if (templateFont == null)
        {
            return null;
        }
        return new Font(templateFont.FontFamily, emSize, templateFont.Style, templateFont.Unit, templateFont.GdiCharSet, templateFont.GdiVerticalFont);
    }
}
