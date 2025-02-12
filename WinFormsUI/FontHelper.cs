using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeifenLuo.WinFormsUI
{
    internal static class FontHelper
    {
        internal static Font TextToMeasure(ref string text, Font textFont, Font iconFont)
        {
            if (text == "Settings")
            {
                text = "⚙️";
                return iconFont;
            }
            return textFont;
        }
    }
}
