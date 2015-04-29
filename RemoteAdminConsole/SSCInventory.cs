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
        public Color HairColor { get; set; }
        public Color PantsColor { get; set; }
        public Color ShirtColor { get; set; }
        public Color UnderShirtColor { get; set; }
        public Color ShoeColor { get; set; }
        public Color SkinColor { get; set; }
        public Color EyeColor { get; set; }
        public Int32 Health { get; set; }
        public Int32 MaxHealth { get; set; }
        public Int32 Mana { get; set; }
        public Int32 MaxMana { get; set; }
        public Int32 QuestsCompleted { get; set; }
        public Boolean IsPlaying { get; set; }

        public SSCInventory(Int32 id, Int32 health, Int32 maxhealth, Int32 mana, Int32 maxmana, Int32 questscompleted, string inventory, Int32 hair, Int32 hairdye, Color haircolor,
             Color pantscolor, Color shirtcolor, Color undershirtcolor, Color shoecolor, Color skincolor, Color eyecolor, Boolean isplaying)
        {
            Id = id;
            Inventory = inventory;
            Hair = hair;
            HairDye = hairdye;
            HairColor = haircolor;
            PantsColor = pantscolor;
            ShirtColor = shirtcolor;
            UnderShirtColor = undershirtcolor;
            ShoeColor = shoecolor;
            SkinColor = skincolor;
            EyeColor = eyecolor;
            Health = health;
            MaxHealth = maxhealth;
            Mana = mana;
            MaxMana = maxmana;
            QuestsCompleted = questscompleted;
            IsPlaying = isplaying;
        }

        public SSCInventory()
        {
            Id = 0;
            Inventory = string.Empty;
            Hair = 0;
            HairDye = 0;
            HairColor = Color.Black;
            PantsColor = Color.Black;
            ShirtColor = Color.Black;
            UnderShirtColor = Color.Black;
            ShoeColor = Color.Black;
            SkinColor = Color.Black;
            EyeColor = Color.Black;
            Health = 0;
            MaxHealth = 0;
            Mana = 0;
            MaxMana = 0;
            QuestsCompleted = 0;
            IsPlaying = false;
        }
    }
}
