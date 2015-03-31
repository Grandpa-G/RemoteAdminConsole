using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace RemoteAdminConsole
{
     public class SSCInventory
    {
        public Int32 Id { get; set; }
        public string Inventory { get; set; }
        public Int32 Hair { get; set; }
        public Int32 HairDye { get; set; }
        public System.Drawing.Color HairColor { get; set; }
        public Color PantsColor { get; set; }
        public Color ShirtColor { get; set; }
        public Color UnderShirtColor { get; set; }
        public Color ShoeColor { get; set; }
        public Color SkinColor { get; set; }
        public Color EyeColor { get; set; }

        public SSCInventory(Int32 id, string inventory, Int32 hair, Int32 hairdye, Color haircolor,
            Color pantscolor, Color shirtcolor, Color undershirtcolor, Color shoecolor, Color skincolor, Color eyecolor)
        {
            Id = id;
            Inventory = inventory;
            Hair = hair;
            HairDye = hairdye;
            HairColor = haircolor;
            PantsColor = pantscolor;
            ShirtColor = shirtcolor;
            UnderShirtColor = undershirtcolor;
            ShoeColor = haircolor;
            SkinColor = skincolor;
            EyeColor = eyecolor;
        }

        public SSCInventory()
        {
            Id = 0;
            Inventory = string.Empty;
            Hair = 0;
            HairDye = 0;
            HairColor = Color.FromArgb(0, 0, 0);
            PantsColor = Color.FromArgb(0, 0, 0);
            ShirtColor = Color.FromArgb(0, 0, 0);
            UnderShirtColor = Color.FromArgb(0, 0, 0);
            ShoeColor = Color.FromArgb(0, 0, 0);
            SkinColor = Color.FromArgb(0, 0, 0);
            EyeColor = Color.FromArgb(0, 0, 0);

        }
    }
}
