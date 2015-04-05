using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAdminConsole
{
    class Colors
    {
        public byte R;
        public byte G;
        public byte B;

        public System.Drawing.Color GetColor()
        {
            return System.Drawing.Color.FromArgb(this.R, this.G, this.B);
        }

        public System.Drawing.Color GetColor(byte red, byte green, byte blue)
        {
            this.R = red;
            this.G = green;
            this.B = blue;

            return System.Drawing.Color.FromArgb(this.R, this.G, this.B);
        }
    }
}
