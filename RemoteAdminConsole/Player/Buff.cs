using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.IO;

namespace RemoteAdminConsole
{
    public class Buff
    {
        public Buff()
        {
            BuffType = 0;
            BuffTime = 0;
        }

        public Buff(int buffType, int buffTime)
        {
            this.BuffType = buffType;
            this.BuffTime = buffTime;
        }

        private int buffType;
        private int buffTime;
        private string item;

        public int BuffType { get { return this.buffType; } set { this.buffType = value; } }
        public int BuffTime { get { return this.buffTime; } set { this.buffTime = value; } }

        public Dictionary<int, string> buffName = new Dictionary<int, string> {

                                        { 0, "No Item" },
                                        { 1, "Obsidian Skin Potion" },    
                                        { 2, "Regeneration Potion" },   
                                        { 3, "Swiftness Potion" },   
                                        { 4, "Gills potion" },   
                                        { 5, "Ironskin Potion" },   
                                        { 6, "Mana Regeneration Potion" },   
                                        { 7, "Mana Power Potion" },   
                                        { 8, "Featherfall Potion" },   
                                        { 9, "Spelunker Potion" },   
                                        { 10, "Invisibility Potion" },   
                                        { 11, "Shine Potion" },   
                                        { 12, "Night Owl Potion" },   
                                        { 13, "Battle Potion" },   
                                        { 14, "Thorns Potion" },   
                                        { 15, "Water Walking Potion" },   
                                        { 16, "Archery Potion" },   
                                        { 17, "Hunter Potion" },   
                                        { 18, "Gravitation Potion" },
                                        { 19, "Orb of Light" },
                                        { 20, "Poisoned" },
                                        { 21, "Potion Sickness" },
                                        { 22, "Darkness" },
                                        { 23, "Cursed" },
                                        { 24, "On Fire!" },
                                        { 25, "Tipsy" },
                                        { 26, "Well Fed" },
        };

        public Dictionary<int, int> buffMax = new Dictionary<int, int> { 
                                        
                                        { 0, 0 },
                                        { 1, 3 },    
                                        { 2, 2 },   
                                        { 3, 4 },   
                                        { 4, 1 },   
                                        { 5, 5 },   
                                        { 6, 2 },   
                                        { 7, 2 },   
                                        { 8, 5 },   
                                        { 9, 5 },   
                                        { 10, 1 },   
                                        { 11, 10 },   
                                        { 12, 10 },   
                                        { 13, 7 },   
                                        { 14, 2 },   
                                        { 15, 5 },   
                                        { 16, 4 },   
                                        { 17, 5 },   
                                        { 18, 3 },
                                        // Need Buff Max Times
                                        { 19, 1},                                        
                                        { 20, 1},                                        
                                        { 21, 1},
                                        { 22, 1},
                                        { 23, 1},
                                        { 24, 1},
                                        { 25, 1},
                                        { 26, 1},
        };

        public int MaxBuff()
        {
            return buffMax[this.BuffType] * 60;
        }

        public string BuffName()
        {
            return buffName[this.BuffType];
        }

        public string BuffName(int index)
        {
            return buffName[index];
        }

        public System.Drawing.Image BuffImages()
        {
            try
            {
                System.Reflection.Assembly thisExe;
                thisExe = System.Reflection.Assembly.GetExecutingAssembly();
                item = "TerrariViewer.Resources.Buff_" + this.buffType + ".png";
                System.IO.Stream file = thisExe.GetManifestResourceStream(item);
                return Image.FromStream(file);

            }
            catch
            {
                return null;
            }
        }

        public System.Drawing.Image BuffImages(int buff)
        {
            try
            {
                System.Reflection.Assembly thisExe;
                thisExe = System.Reflection.Assembly.GetExecutingAssembly();
                item = "TerrariViewer.Resources.Buff_" + buff + ".png";
                System.IO.Stream file = thisExe.GetManifestResourceStream(item);
                return Image.FromStream(file);

            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
        {
            return this.BuffName() + "\n\nTime Left:  " + (this.BuffTime / 60).ToString() + " Seconds";
        }
    }
}
