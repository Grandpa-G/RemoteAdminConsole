using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace RemoteAdminConsole
{
    class Player
    {
        public int GameVersion { get; set; }
        public string CharName { get; set; }
        public byte Difficulty { get; set; }
        public int CharHairType { get; set; }
        public int HairDye { get; set; }
        public bool Gender { get; set; }
        public int HideVisual { get; set; }
        public int CurrentLife { get; set; }
        public int MaxLife { get; set; }
        public int CurrentMana { get; set; }
        public int MaxMana { get; set; }
        public Colors[] Colors { get; set; }
        public Item[] armor { get; set; }
        public Item[] accessories { get; set; }
        public Item[] vanity { get; set; }
        public Item[] socialAccessories { get; set; }
        public Item[] dye { get; set; }

        public Item[] inv { get; set; }
        public Item[] bank1 { get; set; }
        public Item[] bank2 { get; set; }
        public Buff[] buffs { get; set; }

        public int[] spX { get; set; }
        public int[] spY { get; set; }
        public int[] spI { get; set; }
        public string[] spN { get; set; }

        public bool hbLocked { get; set; }
        public string FilePath { get; set; }
        public int AnglerQuestsFinished { get; set; }
       
        public Player(string path)
        {
            this.FilePath = path;

            spX = new int[200];
            spY = new int[200];
            spI = new int[200];
            spN = new string[200];
            for (int i = 0; i < 200; i++)
            {
                this.spN[i] = "";
            }

            Colors = new Colors[7];
            for (int i = 0; i < 7; i++)
            {
                this.Colors[i] = new Colors();
            }

            armor = new Item[3];
            for (int i = 0; i < armor.Length; i++)
                armor[i] = new Item();

            accessories = new Item[5];
            for (int i = 0; i < accessories.Length; i++)
                accessories[i] = new Item();

            vanity = new Item[3];
            for (int i = 0; i < vanity.Length; i++)
                vanity[i] = new Item();

            socialAccessories = new Item[5];
            for (int i = 0; i < socialAccessories.Length; i++)
                socialAccessories[i] = new Item();

            dye = new Item[8];
            for (int i = 0; i < dye.Length; i++)
                dye[i] = new Item();

            inv = new Item[58];
            for (int i = 0; i < inv.Length; i++)
                inv[i] = new Item();

            bank1 = new Item[40];
            for (int i = 0; i < bank1.Length; i++)
                bank1[i] = new Item();

            bank2 = new Item[40];
            for (int i = 0; i < bank2.Length; i++)
                bank2[i] = new Item();

            buffs = new Buff[22];
            for (int i = 0; i < buffs.Length; i++)
                buffs[i] = new Buff();
        }

        public void LoadPlayer(string path)
        {
            FileStream stream;
            BinaryReader reader;
            string inputFile;

            if (path == "")
                inputFile = this.FilePath;
            else
                inputFile = path;

            bool decryptionCheck;

            try
            {
                string outputFile = this.FilePath + ".dat";

                decryptionCheck = DecryptFile(inputFile, outputFile);

                if (!decryptionCheck)
                {
                    using (stream = new FileStream(outputFile, FileMode.Open))
                    {
                        using (reader = new BinaryReader(stream))
                        { 
                              this.GameVersion = reader.ReadInt32();
                            this.CharName = reader.ReadString();

                            Difficulty = reader.ReadByte();

                            this.CharHairType = reader.ReadInt32();
                            HairDye = reader.ReadByte();

                            Gender = reader.ReadBoolean();
                            HideVisual = reader.ReadByte();

                            this.CurrentLife = reader.ReadInt32();
                            if (this.CurrentLife > 1000)
                                this.CurrentLife = 1000;

                            this.MaxLife = reader.ReadInt32();
                            if (this.MaxLife > 1000)
                                this.MaxLife = 1000;

                            this.CurrentMana = reader.ReadInt32();
                            if (this.CurrentMana > 1000)
                                this.CurrentMana = 1000;

                            this.MaxMana = reader.ReadInt32();
                            if (this.MaxMana > 1000)
                                this.MaxMana = 1000;

                            for (int i = 0; i < Colors.Length; i++)
                            {
                                Colors[i].R = reader.ReadByte();
                                Colors[i].G = reader.ReadByte();
                                Colors[i].B = reader.ReadByte();
                            }

                            for (int i = 0; i < armor.Length; i++)
                            {
                                armor[i].NetId = reader.ReadInt32();
                                armor[i].Prefix = reader.ReadByte();
                            }

                            for (int i = 0; i < accessories.Length; i++)
                            {
                                accessories[i].NetId = reader.ReadInt32();
                                accessories[i].Prefix = reader.ReadByte();
                            }
                            for (int i = 0; i < vanity.Length; i++)
                            {
                                vanity[i].NetId = reader.ReadInt32();
                                vanity[i].Prefix = reader.ReadByte();
                            }

                            for (int i = 0; i < socialAccessories.Length; i++)
                            {
                                socialAccessories[i].NetId = reader.ReadInt32();
                                socialAccessories[i].Prefix = reader.ReadByte();
                            }
                            for (int i = 0; i < dye.Length; i++)
                            {
                                dye[i].NetId = reader.ReadInt32();
                                dye[i].Prefix = reader.ReadByte();
                            }

                            for (int i = 0; i < inv.Length; i++)
                            {
                                this.inv[i].NetId = reader.ReadInt32();
                                this.inv[i].StackSize = reader.ReadInt32();
                                this.inv[i].Prefix = reader.ReadByte();
                            }

                            for (int i = 0; i < bank1.Length; i++)
                            {
                                      this.bank1[i].NetId = reader.ReadInt32();
                                    this.bank1[i].StackSize = reader.ReadInt32();
                                    this.bank1[i].Prefix = reader.ReadByte();
                           }

                            for (int i = 0; i < bank2.Length; i++)
                            {
                                this.bank2[i].NetId = reader.ReadInt32();
                                this.bank2[i].StackSize = reader.ReadInt32();
                                this.bank2[i].Prefix = reader.ReadByte();
                            }
                            for (int i = 0; i < buffs.Length; i++)
                            {
                                this.buffs[i].BuffType = reader.ReadInt32();
                                this.buffs[i].BuffTime = reader.ReadInt32();
                            }

                            for (int m = 0; m < spX.Length; m++)
                            {
                                int num = reader.ReadInt32();
                                if (num == -1)
                                {
                                    break;
                                }
                                this.spX[m] = num;
                                this.spY[m] = reader.ReadInt32();
                                this.spI[m] = reader.ReadInt32();
                                this.spN[m] = reader.ReadString();
                            }

                            this.hbLocked = reader.ReadBoolean();

                            int AnglerQuestsFinished = reader.ReadInt32();
                            reader.Close();
                        }
                    }
                    //                    File.Delete(outputFile);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                decryptionCheck = true;
            }

            if (decryptionCheck)
            {
                string backupPlayerFile = inputFile + ".bak";

                if (File.Exists(backupPlayerFile))
                {
                    File.Delete(inputFile);
                    File.Move(backupPlayerFile, inputFile);
                    this.LoadPlayer(inputFile);
                }
            }
        }

        public void SavePlayer(string path)
        {
            FileStream stream;
            BinaryWriter writer;
            this.FilePath = path;

            string destinationFile = path + ".bak";

            if (File.Exists(path))
            {
                File.Copy(path, destinationFile, true);
            }

            string tempFile = path + ".dat";

            using (stream = new FileStream(tempFile, FileMode.Create))
            {
                using (writer = new BinaryWriter(stream))
                {
                    writer.Write(this.GameVersion);
                    writer.Write(this.CharName);
                    writer.Write(this.Difficulty);
                    writer.Write(this.CharHairType);
                    writer.Write(this.HairDye);
                    writer.Write(this.Gender);
                    writer.Write(this.HideVisual);
                    writer.Write(this.CurrentLife);
                    writer.Write(this.MaxLife);
                    writer.Write(this.CurrentMana);
                    writer.Write(this.MaxMana);


                    for (int i = 0; i < Colors.Length; i++)
                    {
                        writer.Write(this.Colors[i].R);
                        writer.Write(this.Colors[i].G);
                        writer.Write(this.Colors[i].B);
                    }

                    for (int i = 0; i < armor.Length; i++)
                    {
                        writer.Write(this.armor[i].NetId);
                        writer.Write(this.armor[i].StackSize);
                    }

                    for (int i = 0; i < accessories.Length; i++)
                    {
                        writer.Write(this.accessories[i].NetId);
                        writer.Write(this.accessories[i].StackSize);
                    }

                    for (int i = 0; i < vanity.Length; i++)
                    {
                        writer.Write(this.vanity[i].NetId);
                        writer.Write(this.vanity[i].StackSize);
                    }

                    for (int i = 0; i < socialAccessories.Length; i++)
                    {
                        writer.Write(this.socialAccessories[i].NetId);
                        writer.Write(this.socialAccessories[i].StackSize);
                    }
                    for (int i = 0; i < dye.Length; i++)
                    {
                        writer.Write(this.dye[i].NetId);
                        writer.Write(this.dye[i].StackSize);
                    }

                    for (int i = 0; i < inv.Length; i++)
                    {
                        writer.Write(this.inv[i].NetId);
                        writer.Write(this.inv[i].StackSize);
                        writer.Write(this.inv[i].Prefix);
                    }

                    for (int i = 0; i < bank1.Length; i++)
                    {

                        writer.Write(this.bank1[i].Name);
                        writer.Write(this.bank1[i].StackSize);
                    }

                    for (int i = 0; i < bank2.Length; i++)
                    {

                        writer.Write(this.bank2[i].Name);
                        writer.Write(this.bank2[i].StackSize);
                    }
                    for (int i = 0; i < buffs.Length; i++)
                    {
                        writer.Write(this.buffs[i].BuffType);
                        writer.Write(this.buffs[i].BuffTime);
                    }

                    for (int m = 0; m < spX.Length; m++)
                    {
                        writer.Write(this.spX[m]);
                        writer.Write(this.spY[m]);
                        writer.Write(this.spI[m]);
                        writer.Write(this.spN[m]);
                    }
                    writer.Write(this.hbLocked);
                    writer.Write(this.AnglerQuestsFinished);
                    writer.Close();
                }
            }
            EncryptFile(tempFile, this.FilePath);
 //           File.Delete(tempFile);
        }

        /// <summary>
        /// Decrypts the Terraria player file to make it readable
        /// </summary>
        private static bool DecryptFile(string inputFile, string outputFile)
        {
            string key = "h3y_gUyZ";
            byte[] bytes = new UnicodeEncoding().GetBytes(key);
            FileStream fileStream1 = new FileStream(inputFile, FileMode.Open);
            RijndaelManaged managed = new RijndaelManaged();
            CryptoStream cryptoStream = new CryptoStream(fileStream1, managed.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            FileStream fileStream2 = new FileStream(outputFile, FileMode.Create);
            try
            {
                int num;
                int counter = 0;
                while ((num = cryptoStream.ReadByte()) != -1)
                {
                    fileStream2.WriteByte((byte)num);
                    counter++;
                }
                fileStream2.Close();
                cryptoStream.Close();
                fileStream1.Close();
                Console.WriteLine("Bytes:" + counter);
            }
            catch
            {
                fileStream2.Close();
                fileStream1.Close();
                File.Delete(outputFile);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Encrypts the Terraria player file
        /// </summary>
        private static void EncryptFile(string inputFile, string outputFile)
        {
            int num;
            string key = "h3y_gUyZ";
            byte[] bytes = new UnicodeEncoding().GetBytes(key);
            string path = outputFile;
            FileStream fileStream1 = new FileStream(path, FileMode.Create);
            RijndaelManaged managed = new RijndaelManaged();
            CryptoStream cryptoStream = new CryptoStream(fileStream1, managed.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);
            FileStream fileStream2 = new FileStream(inputFile, FileMode.Open);
            while ((num = fileStream2.ReadByte()) != -1)
            {
                cryptoStream.WriteByte((byte)num);
            }
            fileStream2.Close();
            cryptoStream.Close();
            fileStream1.Close();
        }
    }
}
