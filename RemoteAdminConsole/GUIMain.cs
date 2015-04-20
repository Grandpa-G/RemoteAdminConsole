using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Runtime.Serialization.Json;



using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
namespace RemoteAdminConsole
{
    public partial class GUIMain : Form
    {
        private const int SERVERTAB = 0;
        private const int CONSOLETAB = 1;
        private const int PLAYERTAB = CONSOLETAB + 1;
        private const int BANTAB = PLAYERTAB + 1;
        private const int USERSTAB = BANTAB + 1;
        private const int INVENTORYTAB = USERSTAB + 1;
        private const int GROUPSTAB = INVENTORYTAB + 1;
        private const int LOGTAB = GROUPSTAB + 1;
        private const int ABOUTTAB = LOGTAB + 1;

        public static String PROGRAMNAME = "Remote Admin Console";
        public static bool DEBUG = false;
        public static bool DEVELOPMENT = false;

        public static int ITEMOFFSET = 48;
        public static int MAXITEMS = 2748 + ITEMOFFSET + 1;
        public static int MAXITEMSPREFIX = 83 + 1;
        public static int MAXPREFIX = 86;
        public static int EQUIPMENTITEMS = 58;
        public static int MAXITEMSLOTS = 58;
        public static int MAXSLOTS = 58 + 3 + 5 + 3 + 5 + 8;

        Bitmap[] sprites = new Bitmap[MAXITEMS];
        Bitmap item_0;
        Bitmap sprite;

        private Item[] itemList = new Item[MAXITEMS];
        private Prefixs[] prefixList = new Prefixs[MAXPREFIX];
        private List<Group> groupList = new List<Group>();
        DialogResult usersChoice;

        RemoteUtils ru = new RemoteUtils();

        private bool playerFound = false;

        public GUIMain()
        {
            AppDomain.CurrentDomain.UnhandledException += MyHandler;
            try
            {
                AdminConsole();
            }
            catch (IndexOutOfRangeException e) { Console.WriteLine("error"); }
        }
        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("MyHandler caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }
        private void AdminConsole()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();

            foreach (string arg in args)
            {
                if (arg.Length > 0 && arg.Equals("-d"))
                {
                    DEBUG = true;
                    Console.WriteLine("Debug Mode On.");
                }
                if (arg.Length > 0 && arg.Equals("-dev"))
                {
                    DEVELOPMENT = true;
                    Console.WriteLine("Development Mode On.");
                }

            }

            getDefaults();
            bool connected = false;
            stopServer.Visible = false;
            if (ru.conn != null)
                if (ru.conn.Server != null)
                    if (ru.conn.Server.Length > 0 && ru.getToken())
                    {
                        connected = true;
                        stopServer.Visible = true;
                        userIcon.Visible = true;
                        userLoggedIn.Text = "Logged in as " + ru.conn.UserId + ".";
                    }

            getItemSpriteImage();
            loadItemNames();
            setupAbout();


            this.banDataBan.CellClick += this.banDataBan_CellClick;

            this.groupDataPermissions.CellClick += this.groupDataPermissions_CellClick;

            this.usersDataList.CellClick += this.usersDataList_CellClick;
            this.usersDataList.CellBeginEdit += this.usersDataList_CellBeginEdit;
            tabInventory.Enabled = false;

            banDataBan.RowHeadersWidth = 22;
            groupDataList.RowHeadersWidth = 22;
            groupDataPermissions.RowHeadersWidth = 10;
            usersDataList.RowHeadersWidth = 22;

            inventoryToolTip = new ToolTip();
            inventoryToolTip.IsBalloon = false;
            inventoryToolTip.ShowAlways = true;

            if (!connected)
                tabPane.SelectedTab = tabSettings;

            if (!DEVELOPMENT)
            {
            }
            else
            {
                button1.Visible = true;
                button2.Visible = true;
                inventoryExport.Visible = true;
            }
            getServerDetails();
        }

        private void tabPane_Selected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case BANTAB:
                    //                   getBannedList(true);
                    break;
                case SERVERTAB:
                    getServerDetails();
                    break;
                case USERSTAB:
                    //                   getUsers();
                    break;
                case INVENTORYTAB:
                    getInventory();
                    break;
                case GROUPSTAB:
                    //                   getGroupList();
                    break;
                case LOGTAB:
                    //                   getLog();
                    break;
                case ABOUTTAB:
                    setupAbout();
                    break;
            }
        }


        void tabPane_DrawItem(object sender, DrawItemEventArgs e)
        {

            if (e.Index == INVENTORYTAB)
                //Draw yellow tab headers.
                e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds);
        }

        private void GUIMain_Load(object sender, EventArgs e)
        {
            this.BackColor = System.Drawing.Color.FromArgb(238, 232, 170);
        }

        #region  Server Tab
        //  Server Tab
        private void stopServer_Click(object sender, EventArgs e)
        {
            DialogResult usersChoice =
    MessageBox.Show("Are you sure you want to shutdown this server?", PROGRAMNAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // cancel the delete event
            if (usersChoice == DialogResult.No)
                return;

            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("v2/server/off", "&confirm=true&nosave=false");
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                MessageBox.Show("Server shutdown.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void getServerDetails()
        {
            string name = "";
            JObject rules = null;
            JArray players = null;
            int playercount = 0;
            int maxplayers = 0;
            int port = 0;
            string version = "";
            string upTime = "";
            string world = "";
            int worldId = 0;
            string nickname = "";
            string username = "";
            int account;
            string group = "";
            string ip = "";
            int index = 0;
            Boolean state;
            String extraAdminRESTVersion = "";
            String dbSupported = "";

            serverChatStatus.Text = "";
            serverChat.Text = "";
            serverChat.Enabled = false;
            serverChatPlayer.Text = "Chat closed.";

            JObject results = ru.communicateWithTerraria("v2/server/status", "players=true&rules=true");
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                name = (string)results["name"];
                port = (int)results["port"];
                version = (string)results["serverversion"];
                upTime = (string)results["uptime"];
                rules = (JObject)results["rules"];
                players = (JArray)results["players"];
                playercount = (int)results["playercount"];
                maxplayers = (int)results["maxplayers"];
                world = (string)results["world"];

                lblServerNamevalue.Text = ru.conn.Server;
                lblPortValue.Text = port.ToString();
                lblWorldName.Text = world;
                lblWorldId.Text = "";
                lblInvasionSizeValue.Text = "";
                lblVersionValue.Text = version;
                aboutServerVersion.Text = version;

                lblUptime.Text = upTime;
                lblPlayerCountValue.Text = playercount.ToString();
                lblMaxPlayerValue.Text = maxplayers.ToString();

                serverDisableBuild.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                serverDisableClownBombsre.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                serverDisableDungeonGuardian.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                serverDisableInvisPvP.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                serverDisableSnowBalls.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                serverDisableTombstones.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                serverEnableWhitelist.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                serverHardcoreOnly.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                serverSpawnProtection.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                serverServerSideInventory.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);

                string[] rule = null;
                if (rules != null)
                {
                    string[] rulesString = rules.ToString().Replace("\"", "").Replace("}", "").Replace("\r\n", "").Replace("{", "").Split(',');

                    for (int i = 0; i < rulesString.Length; i++)
                    {
                        rule = rulesString[i].Split(':');
                        state = false;
                        state = rule[1].ToLower().Contains("true");
                        switch (rule[0].Trim())
                        {
                            case "AutoSave":
                                if (state)
                                    serverAutoSave.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "DisableBuild":
                                if (state)
                                    serverDisableBuild.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "DisableClownBombs":
                                if (state)
                                    serverDisableClownBombsre.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "DisableDungeonGuardian":
                                if (state)
                                    serverDisableDungeonGuardian.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "DisableInvisPvP":
                                if (state)
                                    serverDisableInvisPvP.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "DisableSnowBalls":
                                if (state)
                                    serverDisableSnowBalls.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "DisableTombstones":
                                if (state)
                                    serverDisableTombstones.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "EnableWhitelist":
                                if (state)
                                    serverEnableWhitelist.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "HardcoreOnly":
                                if (state)
                                    serverHardcoreOnly.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "SpawnProtection":
                                if (state)
                                    serverSpawnProtection.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "ServerSideInventory":
                                if (state)
                                    serverServerSideInventory.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                                break;
                            case "PvPMode":
                                serverPvPMode.Text = rule[1].ToString();
                                break;
                            case "SpawnProtectionRadius":
                                serverSpawnProtectionRadius.Text = rule[1].ToString();
                                break;
                        }
                    }
                }
                serverDataPlayers.Rows.Clear();

                results = ru.communicateWithTerraria("AdminREST/PlayerList", "");
                status = (string)results["status"];
                if (status.Equals("200"))
                {
                    players = (JArray)results["players"];

                    if (players != null)
                    {
                        for (int i = 0; i < players.Count; i++)
                        {
                            JObject innerObj = (JObject)players[i];
                            index = (int)innerObj["index"];
                            account = (int)innerObj["account"];
                            ip = (string)innerObj["ip"];
                            nickname = (string)innerObj["nickname"];
                            username = (string)innerObj["username"];
                            group = (string)innerObj["group"];
                            if (nickname != null)
                            {
                                serverDataPlayers.Rows.Add(nickname, username, group.ToString(), ip, index, account);
                            }
                        }
                    }
                }
                serverDataPlayers.ClearSelection();
                //                serverDataPlayers.Rows[0].Cells[0].DefaultCellStyle.SelectionBackColor = serverDataPlayers.DefaultCellStyle.BackColor;
                tabPlayer.Enabled = false;
                playerFound = false;

                double serverTime;
                Boolean bloodmoon;
                int invasionsize;
                Boolean daytime;
                string worldsize;

                // And now use this to connect server
                results = ru.communicateWithTerraria("world/read", "");
                status = (string)results["status"];
                if (status.Equals("200"))
                {
                    name = (string)results["name"];
                    worldsize = (string)results["size"];
                    serverTime = (double)results["time"];
                    daytime = (Boolean)results["daytime"];
                    bloodmoon = (Boolean)results["bloodmoon"];
                    invasionsize = (int)results["invasionsize"];

                    lblWorldName.Text = name;
                    lblWorldSizeValue.Text = worldsize;

                    double time = serverTime / 3600.0;
                    time += 4.5;
                    if (!daytime)
                        time += 15.0;
                    time = time % 24.0;

                    lblTimeValue.Text = String.Format("{0:0}:{1:00}", (int)Math.Floor(time), (int)Math.Round((time % 1.0) * 60.0));

                    if (daytime)
                        serverDaytime.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                    else
                        serverDaytime.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);

                    if (bloodmoon)
                        serverBloodmoon.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._checked);
                    else
                        serverBloodmoon.Image = new Bitmap(RemoteAdminConsole.Properties.Resources._unchecked);
                    lblInvasionSizeValue.Text = invasionsize.ToString();
                }

                // And now use this to connect server
                results = ru.communicateWithTerraria("AdminREST/version", "");
                status = (string)results["status"];
                if (status.Equals("200"))
                {
                    extraAdminRESTVersion = (string)results["version"];
                    dbSupported = (string)results["db"];
                    dbSupported = dbSupported + " currently being used.";

                    aboutExtraAdminRestVersion.Text = extraAdminRESTVersion;
                    aboutSQLSupport.Text = dbSupported;
                }
                // And now use this to connect server
                results = ru.communicateWithTerraria("AdminREST/serverInfo", "players=true&rules=true");
                status = (string)results["status"];
                if (status.Equals("200"))
                {
                    worldId = (int)results["worldID"];

                    lblWorldId.Text = worldId.ToString();
                }
                lblServerRefresh.Text = DateTime.Now.ToString("ddd h:mm:ss tt");

                serverDataPlayers.ClearSelection();
                serverDataPlayers.ClearSelection();

            }
        }
        private void serverRefresh_Click(object sender, EventArgs e)
        {
            getServerDetails();
        }
        private void tabPane_Selecting(object sender, TabControlCancelEventArgs e)
        {
            e.Cancel = !e.TabPage.Enabled;
        }
        private void serverDataPlayers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //check if row index is not selected
            DataGridViewRow row = serverDataPlayers.Rows[e.RowIndex];

            serverDataPlayers.Rows[e.RowIndex].Selected = true;

            string name = row.Cells[0].Value.ToString();

            serverChatStatus.Text = "";
            serverChat.Text = "";
            serverChat.Enabled = true;
            serverChatPlayer.Text = "Chat with " + name;

            playerFound = true;
            tabPlayer.Enabled = true;

        }


        #endregion

        #region  Player Tab
        // Player Tab
        private void tabPlayer_Enter(object sender, EventArgs e)
        {
            Color hairColor;
            Color skinColor;
            Color pantsColor;
            Color shirtColor;
            Color underShirtColor;
            Color shoeColor;
            Color eyeColor;

            if (!playerFound)
            {
                return;
            }

            DataGridViewRow selectedRow = serverDataPlayers.CurrentRow;
            string player = selectedRow.Cells[0].Value.ToString();
            string userId = selectedRow.Cells[1].Value.ToString();
            string account = selectedRow.Cells[5].Value.ToString();
            string index = selectedRow.Cells[4].Value.ToString();

            if (account.Length == 0)
                lblPlayerName.Text = String.Format("{0} [index={1} UserId= UserAccountName=]", player, index);
            else
                lblPlayerName.Text = String.Format("{0} [index={1} UserId={2} UserAccountName={3}]", player, index, account, userId);

            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("AdminREST/getPlayerData", "player=" + player);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                JObject color;
                color = (JObject)results["haircolor"];
                int red = (int)(color["R"]);
                int green = (int)(color["G"]);
                int blue = (int)(color["B"]);
                hairColor = Color.FromArgb(red, green, blue);
                color = (JObject)results["skincolor"];
                red = (int)(color["R"]);
                green = (int)(color["G"]);
                blue = (int)(color["B"]);
                skinColor = Color.FromArgb(red, green, blue);
                color = (JObject)results["pantsColor"];
                red = (int)(color["R"]);
                green = (int)(color["G"]);
                blue = (int)(color["B"]);
                pantsColor = Color.FromArgb(red, green, blue);
                color = (JObject)results["shirtColor"];
                red = (int)(color["R"]);
                green = (int)(color["G"]);
                blue = (int)(color["B"]);
                shirtColor = Color.FromArgb(red, green, blue);
                color = (JObject)results["underShirtColor"];
                red = (int)(color["R"]);
                green = (int)(color["G"]);
                blue = (int)(color["B"]);
                underShirtColor = Color.FromArgb(red, green, blue);
                color = (JObject)results["shoeColor"];
                red = (int)(color["R"]);
                green = (int)(color["G"]);
                blue = (int)(color["B"]);
                shoeColor = Color.FromArgb(red, green, blue);
                color = (JObject)results["eyeColor"];
                red = (int)(color["R"]);
                green = (int)(color["G"]);
                blue = (int)(color["B"]);
                eyeColor = Color.FromArgb(red, green, blue);

                string inventoryList = (string)results["inventory"];
                string[] inventory = inventoryList.Split(',');
                string armorList = (string)results["armor"];
                string[] armorItems = armorList.Split(',');
                string dyeList = (string)results["dyes"];
                string[] dyeItems = dyeList.Split(',');
                string[] newInventory = new String[MAXITEMSPREFIX - 1];
                int counter = 0;
                string[] slot;
                for (int i = 0; i < inventory.Length; i++)
                {
                    slot = inventory[i].Split(':');
                    int netId = findInventoryItem(slot[0].Trim());
                    slot[0] = netId.ToString();
                    newInventory[counter++] = slot[0].ToString() + "," + slot[1] + "," + slot[2];
                    //                   newInventory[counter++] = slot[0].ToString() + "," + slot[1];
                }
                for (int i = 0; i < armorItems.Length; i++)
                {
                    slot = armorItems[i].Split(':');
                    newInventory[counter++] = slot[0].ToString() + "," + slot[1];
                }
                for (int i = 0; i < dyeItems.Length; i++)
                {
                    slot = dyeItems[i].Split(':');
                    int netId = findInventoryItem(slot[0].Trim());
                    slot[0] = netId.ToString();
                    newInventory[counter++] = slot[0].ToString() + ",0";
                }

                SetInventorySlot(this, "inventoryItem", newInventory);

                panelPlayerHairColor.BackColor = hairColor;
                ToolTipColor(panelPlayerHairColor, hairColor);
                panelPlayerSkinColor.BackColor = skinColor;
                ToolTipColor(panelPlayerSkinColor, skinColor);
                panelPlayerEyeColor.BackColor = eyeColor;
                ToolTipColor(panelPlayerEyeColor, eyeColor);
                panelPlayerShirtColor.BackColor = shirtColor;
                ToolTipColor(panelPlayerShirtColor, shirtColor);
                panelPlayerUnderShirtColor.BackColor = underShirtColor;
                ToolTipColor(panelPlayerUnderShirtColor, underShirtColor);
                panelPlayerPantsColor.BackColor = pantsColor;
                ToolTipColor(panelPlayerPantsColor, pantsColor);
                panelPlayerShoesColor.BackColor = shoeColor;
                ToolTipColor(panelPlayerShoesColor, shoeColor);
            }

        }

        private void btnKick_Click(object sender, EventArgs e)
        {
            String reason = txtKickBanReason.Text;

            DataGridViewRow selectedRow = serverDataPlayers.CurrentRow;
            string playerName = selectedRow.Cells[0].Value.ToString();

            KickBan kb = new KickBan(ru);
            String results = kb.KickBanPlayer("Kick", playerName, reason);
            lblKickBanStatus.Text = results;
        }

        private void btnBan_Click(object sender, EventArgs e)
        {
            String reason = txtKickBanReason.Text;

            DataGridViewRow selectedRow = serverDataPlayers.CurrentRow;
            string playerName = selectedRow.Cells[0].Value.ToString();

            KickBan kb = new KickBan(ru);
            String results = kb.KickBanPlayer("Ban", playerName, reason);
            lblKickBanStatus.Text = results;
        }

        public void SetInventorySlot(Control control, String group, string[] inventory)
        {
            if (control is Label)
            {
                Label lbl = (Label)control;
                if (lbl.Name.StartsWith(group))
                {
                    int slotIndex = Int32.Parse(lbl.Name.Substring(group.Length)) - 1;
                    String n;
                    if (slotIndex >= inventory.Length)
                        n = "0,0,0";
                    else
                        n = inventory[slotIndex];
                    int netId = Int32.Parse(n.Split(',')[0]);
                    int prefix = 0;
                    string stacks = n.Split(',')[1];
                    try
                    {
                        stacks = n.Split(',')[1];
                        prefix = Int32.Parse(n.Split(',')[2]);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        prefix = 0;
                    }
                    if (slotIndex > EQUIPMENTITEMS || netId == 0)
                        lbl.Text = "";
                    else
                    {
                        if (stacks.Length == 0)
                            lbl.Text = "";
                        else
                            lbl.Text = stacks;
                    }

                    lbl.Image = sprites[ITEMOFFSET + netId];

                    if (prefix > 0 && prefix <= prefixList.Length)
                        inventoryToolTip.SetToolTip(lbl, prefixList[prefix - 1].Name + " " + itemList[ITEMOFFSET + netId].Name);
                    else
                        inventoryToolTip.SetToolTip(lbl, itemList[ITEMOFFSET + netId].Name);
                }
            }
            else
                foreach (Control child in control.Controls)
                {
                    SetInventorySlot(child, group, inventory);
                }

        }

        #endregion

        #region  About Tab
        private void setupAbout()
        {
            aboutVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }


        #endregion


        #region  Ban Tab
        //      Ban Tab

        private void getBannedList(bool fullSearch)
        {
            JArray bans = null;
            String name = null;
            String reason = null;
            String ip = null;
            String banningUser = null;
            String dateBanned = null;
            String dateExpiration = null;
            DateTime BannedDt;
            DateTime ExpirationDt;

            banSearchResults.Text = "";

            string whereClause = "";
            string andClause = " where ";
            string searchString = "%";

            if (banSearchName.Text != null)
                if (banSearchName.Text.Length > 0)
                {
                    searchString = banSearchName.Text;
                    if (banFuzzyName != null && banFuzzyName.Checked)
                        whereClause = whereClause + andClause + " Name like '%25" + banSearchName.Text + "%25'";
                    else
                        whereClause = whereClause + andClause + " Name like '" + banSearchName.Text + "'";
                    andClause = " and ";
                }

            if (banSearchIP.Text != null)
                if (banSearchIP.Text.Length > 0)
                {
                    searchString = banSearchIP.Text;
                    if (banFuzzyIP != null && banFuzzyIP.Checked)
                        whereClause = whereClause + andClause + " IP like '%25" + banSearchIP.Text + "%25'";
                    else
                        whereClause = whereClause + andClause + " IP like '" + banSearchIP.Text + "'";
                    andClause = " and ";
                }


            banUnBan.Enabled = false;
            banDataBan.Rows.Clear();

            // And now use this to connect server 
            JObject results = ru.communicateWithTerraria("AdminREST/BanList", "&where=" + whereClause);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                bans = (JArray)results["bans"];
                if (bans.Count == 0)
                    banSearchResults.Text = "No players found.";
                if (bans.Count == 1)
                    banSearchResults.Text = bans.Count.ToString() + " player found.";
                if (bans.Count > 1)
                    banSearchResults.Text = bans.Count.ToString() + " payers found.";

                for (int i = 0; i < bans.Count; i++)
                {
                    JObject innerObj = (JObject)bans[i];
                    name = (String)innerObj["Name"];
                    ip = (String)innerObj["IP"];
                    reason = (String)innerObj["Reason"];
                    banningUser = (String)innerObj["BanningUser"];
                    try
                    {
                        dateBanned = (String)innerObj["Date"];
                        if (dateBanned != null && dateBanned.Length > 0)
                        {
                            BannedDt = DateTime.Parse(dateBanned);
                            dateBanned = String.Format("{0:G}", BannedDt.ToLocalTime());
                        }
                        else
                            dateBanned = "";
                    }
                    catch (NullReferenceException e)
                    {
                        dateBanned = "";
                    }

                    try
                    {
                        dateExpiration = (String)innerObj["Expiration"];
                        if (dateExpiration != null && dateExpiration.Length > 0)
                        {
                            ExpirationDt = DateTime.Parse(dateExpiration);
                            dateExpiration = String.Format("{0:G}", ExpirationDt.ToLocalTime());
                        }
                        else
                            dateExpiration = "";
                    }
                    catch (NullReferenceException e)
                    {
                        dateExpiration = "";
                    }

                    banDataBan.Rows.Add(false, name, ip, reason, banningUser, dateBanned, dateExpiration);
                }
            }

            this.banDataBan.Sort(this.banDataUser, ListSortDirection.Ascending);

        }

        private void refreshBan_Click(object sender, EventArgs e)
        {
            banSearchName.Text = "";
            banSearchIP.Text = "";
            banFuzzyName.Checked = false;
            banFuzzyIP.Checked = false;
            banSearchResults.Text = "";

            getBannedList(true);
        }
        private void banDataBan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //check if row index is not selected
            if (e.ColumnIndex != 0)
                return;

            DataGridViewCheckBoxCell unBan = new DataGridViewCheckBoxCell();
            unBan = (DataGridViewCheckBoxCell)banDataBan.Rows[e.RowIndex].Cells[0];
            if (unBan.Value == null)
                unBan.Value = false;
            switch (unBan.Value.ToString())
            {
                case "False":
                    unBan.Value = true;
                    banUnBan.Enabled = true;
                    break;
                case "True":
                    unBan.Value = false;
                    break;
            }
        }

        private void banUnBan_Click(object sender, EventArgs e)
        {
            bool someUnBanned = false;
            foreach (DataGridViewRow row in banDataBan.Rows)
            {
                if (row.Cells != null)
                {
                    if (!row.IsNewRow)
                    {
                        DataGridViewCheckBoxCell unBan = new DataGridViewCheckBoxCell();
                        unBan = (DataGridViewCheckBoxCell)row.Cells[0];
                        if (unBan.Value.ToString().Equals("True"))
                        {
                            // And now use this to connect server
                            JObject results = ru.communicateWithTerraria("v2/bans/destroy", "type=name&ban=" + ((DataGridViewCell)row.Cells[1]).Value.ToString());
                            string status = (string)results["status"];
                            if (status.Equals("200"))
                            {

                            }
                            //                        if (unBan.Value.ToString().Equals("True"))
                            //                            TShock.Bans.RemoveBan(((DataGridViewCell)row.Cells[1]).Value.ToString(), true, true, true);
                            someUnBanned = true;
                        }
                    }
                }
            }
            if (someUnBanned)
                getBannedList(true);
        }

        private void banStartSearch_Click(object sender, EventArgs e)
        {
            getBannedList(false);
        }

        private void banClearSearch_Click(object sender, EventArgs e)
        {
            banSearchName.Text = "";
            banSearchIP.Text = "";
            banFuzzyName.Checked = false;
            banFuzzyIP.Checked = false;
        }
        #endregion

        #region  Group Tab
        //     Group tab

        // Group Tab
        private int groupRowIndex = -1;
        private string groupRowName = "";

        private void getGroupList()
        {
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            List<string> parents = new List<string>();
            JArray groups;
            string name;
            string parent;
            string chatColor;
            string groupPrefix;
            string groupSuffix;
            JArray totalPermissions;
            JArray permissions;

            usersDataGroup.Items.Clear();
            groupDataList.Rows.Clear();
            groupDataPermissions.Rows.Clear();
            groupDataParentList.Items.Clear();
            groupDataList.ClearSelection();
            groupList.Clear();
            parents.Clear();

            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("AdminREST/GroupList", "");
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                groupDataList.SuspendLayout();
                groups = (JArray)results["groups"];
                for (int i = 0; i < groups.Count; i++)
                {
                    JObject innerObj = (JObject)groups[i];
                    name = (String)innerObj["name"];
                    parent = (String)innerObj["parent"];
                    chatColor = (String)innerObj["chatcolor"];
                    groupPrefix = (String)innerObj["groupprefix"];
                    groupSuffix = (String)innerObj["groupsuffix"];
                    try
                    {
                        totalPermissions = (JArray)innerObj["totalpermissions"];
                        permissions = (JArray)innerObj["permissions"];
                    }
                    catch (NullReferenceException e)
                    {
                        totalPermissions = null;
                        permissions = null;
                    }
                    //                    usersDataGroup.Items.Add(name);
                    parents.Add(name);
                    //                   groupDataParentList.Items.Add(name);
                    Group g = new Group(name, parent, chatColor, groupPrefix, groupSuffix, permissions, totalPermissions);
                    groupList.Add(g);
                    rows.Add(new DataGridViewRow());
                    rows[rows.Count - 1].CreateCells(groupDataList, name, parent, chatColor, "", groupPrefix, groupSuffix);
                    //                    groupDataList.Rows.Add(name, parent, chatColor, "", groupPrefix, groupSuffix);

                }
                groupDataList.Rows.AddRange(rows.ToArray());
                groupDataParentList.Items.AddRange(parents.ToArray());
                usersDataGroup.Items.AddRange(parents.ToArray());

            }
            foreach (DataGridViewRow row in groupDataList.Rows)
            {
                if (row.Cells[2] != null)
                {
                    System.Drawing.Color oldColor = row.Cells[2].Style.BackColor;
                    if (row.Cells[2].Value != null)
                        if (row.Cells[2].Value.ToString().Length > 0)
                        {
                            row.Cells[2].Style.BackColor = tabColorDecode(row.Cells[2].Value.ToString());
                            row.Cells[3].Style.BackColor = row.Cells[2].Style.BackColor;
                        }
                }
            }

            groupDataParentList.Sorted = true;
            this.groupDataList.Sort(this.groupDatagroup, ListSortDirection.Ascending);
            groupDataList.ClearSelection();

            /*
                       if (groupRowName.Length > 0)
                       {
                           groupDataList.ClearSelection();
                           for (int i = 0; i < groupDataList.RowCount; i++)
                           {
                               if (groupDataList.Rows[i].Cells[0].Equals(groupRowName))
                               {
                                   groupDataList.Rows[i].Selected = true;
                                   break;
                               }
                           }
                           groupDataList_CellClick(groupDataList, new DataGridViewCellEventArgs(0, 0));
                       }
             * */
        }

        private System.Drawing.Color tabColorDecode(string colorString)
        {
            ColorDialog dlg = new ColorDialog();
            byte[] color = new Byte[] { 0, 0, 0 };
            string[] bytes = colorString.Split(',');
            for (int i = 0; i < Math.Min(bytes.Length, 3); i++)
            {
                if (bytes[i].Length > 0)
                {
                    int c = Int32.Parse(bytes[i]);
                    byte[] b = BitConverter.GetBytes(c);
                    color[i] = b[0];
                }
            }
            return System.Drawing.Color.FromArgb(color[0], color[1], color[2]);
        }
        private System.Drawing.Color tabColorPickerDialog(string colorString)
        {
            ColorDialog dlg = new ColorDialog();
            byte[] color = new Byte[] { 0, 0, 0 };
            string[] bytes = colorString.Split(',');
            for (int i = 0; i < Math.Min(bytes.Length, 3); i++)
            {
                if (bytes[i].Length > 0)
                {
                    int c = Int32.Parse(bytes[i]);
                    byte[] b = BitConverter.GetBytes(c);
                    color[i] = b[0];
                }
            }
            dlg.Color = System.Drawing.Color.FromArgb(color[0], color[1], color[2]);

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dlg.Color;
            }
            return dlg.Color;
        }
        private int groupsNewRow;
        private Boolean groupsModified;
        private void tabUpdatePermissions_Click(object sender, EventArgs e)
        {
            string action = "";
            DataGridViewRow selectedRow = groupDataList.Rows[groupsNewRow];

            string group = "";
            if (selectedRow.Cells[0] != null)
                if (selectedRow.Cells[0].Value != null)
                {
                    group = selectedRow.Cells[0].Value.ToString();
                    action = action + "&group=" + group;
                }

            string parent = "";
            if (selectedRow.Cells[1] != null)
                if (selectedRow.Cells[1].Value != null)
                {
                    parent = selectedRow.Cells[1].Value.ToString();
                    action = action + "&parent=" + parent;
                }
            string chatColor = "";
            if (selectedRow.Cells[2] != null)
                if (selectedRow.Cells[2].Value != null)
                {
                    chatColor = selectedRow.Cells[2].Value.ToString();
                    action = action + "&chatColor=" + chatColor;
                }
            string prefix = "";
            if (selectedRow.Cells[3] != null)
                if (selectedRow.Cells[3].Value != null)
                {
                    prefix = selectedRow.Cells[3].Value.ToString();
                    action = action + "&prefix=" + prefix;
                }
            string suffix = "";
            if (selectedRow.Cells[4] != null)
                if (selectedRow.Cells[4].Value != null)
                {
                    suffix = selectedRow.Cells[4].Value.ToString();
                    action = action + "&suffix=" + suffix;
                }
            string permissions = "";
            string comma = "";
            foreach (DataGridViewRow row in groupDataPermissions.Rows)
            {
                if (row.IsNewRow)
                    if (row.Cells[1] != null)
                        if (row.Cells[1].Value != null)
                        {
                            if (row.Cells[1].Value.ToString().Equals("*"))
                                continue;
                        }

                if (row.Cells[0] != null)
                    if (row.Cells[0].Value != null)
                    {
                        if (row.Cells[1].Value != null)
                            if (row.Cells[1].Value.ToString().Equals("*"))
                                continue;
                        permissions = permissions + comma + row.Cells[0].Value.ToString();
                        comma = ",";
                    }
            }
            if (permissions.Length > 0)
            {
                action = action + "&permissions=" + permissions;
            }

            if (group.Length == 0)
            {
                groupsUpdateStatus.Text = "No group given.";
                //                usersChoice = MessageBox.Show("No group given.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (groupsModified)
            {
                // And now use this to connect server
                JObject results = ru.communicateWithTerraria("v2/groups/update", action);
                string status = (string)results["status"];
                if (status.Equals("200"))
                {
                    groupRowIndex = selectedRow.Index;
                    groupsUpdateStatus.Text = "Group " + group + " updated";
                    //                   usersChoice = MessageBox.Show("Group " + group + " updated", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    groupsUpdateStatus.Text = "Group " + group + " already exists!";
                //                   usersChoice = MessageBox.Show("Group " + group + " already exists!\r\n" + (string)results["error"], PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                // And now use this to connect server
                JObject results = ru.communicateWithTerraria("v2/groups/create", action);
                string status = (string)results["status"];
                if (status.Equals("200"))
                {
                    groupsUpdateStatus.Text = "Group " + group + " added";
                    groupRowIndex = selectedRow.Index;
                    //                     usersChoice = MessageBox.Show("Group " + group + " added", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    groupsUpdateStatus.Text = "Group " + group + " already exists!";
                //                    usersChoice = MessageBox.Show("Group " + group + " already exists!\r\n" + (string)results["error"], PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            getGroupList();
        }

        private void groupDataList_RowsAdded(object sender, System.Windows.Forms.DataGridViewRowsAddedEventArgs e)
        {
            for (int index = e.RowIndex; index <= e.RowIndex + e.RowCount - 1; index++)
            {
                DataGridViewRow row = groupDataList.Rows[index];
                groupsModified = !row.IsNewRow;
            }
        }
        private void groupDataPermissions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return; //check if row index is not selected
            DataGridViewRow row = groupDataPermissions.Rows[e.RowIndex];
            row.Selected = true;
        }

        private void groupDataList_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex == -1) return; //check if row index is not selected

            DataGridViewRow row = groupDataList.Rows[e.RowIndex];
            groupsNewRow = e.RowIndex;

            if (e.ColumnIndex == 3) // chat color
            {
                if (row.Cells[2].Value == null)
                    row.Cells[2].Value = "255,255,255";
                System.Drawing.Color newColor = tabColorPickerDialog(row.Cells[2].Value.ToString());
                groupDataList.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = newColor;
                row.Cells[2].Value = newColor.R.ToString() + "," + newColor.G.ToString() + "," + newColor.B.ToString();
            }

            groupDataPermissions.Rows.Clear();
            deletedPermissions.Clear();

            if (row.Cells[0].Value != null)
            {
                string name = row.Cells[0].Value.ToString();

                JArray totalPermissions;
                JArray permissions;
                if (groupsModified && name.Length > 0)
                {
                    for (int k = 0; k < groupList.Count; k++)
                    {
                        if (groupList[k].Name.Equals(name))
                        {
                            permissions = groupList[k].Permissions;
                            totalPermissions = groupList[k].TotalPermissions;
                            if (totalPermissions != null)
                                if (totalPermissions.Count > 0)
                                {
                                    string Inherited = "";
                                    string[,] permissionList = new String[totalPermissions.Count, 2];
                                    for (int i = 0; i < totalPermissions.Count; i++)
                                    {
                                        Inherited = "*";
                                        for (int j = 0; j < permissions.Count; j++)
                                        {
                                            if (totalPermissions[i].Equals(permissions[j]))
                                            {
                                                Inherited = "";
                                                break;
                                            }
                                        }
                                        String s = totalPermissions[i].ToString();
                                        permissionList[i, 0] = totalPermissions[i].ToString();
                                        permissionList[i, 1] = Inherited;
                                    }

                                    for (int j = 0; j < totalPermissions.Count; j++)
                                        groupDataPermissions.Rows.Add(permissionList[j, 0], permissionList[j, 1]);
                                    break;
                                }
                            break;
                        }
                    }
                    this.groupDataPermissions.Sort(this.permissionsDataPermissons, ListSortDirection.Ascending);
                }
                //?                usersDataGroup.Items.Add(name);
            }

        }

        private void tabGroupRefresh_Click(object sender, EventArgs e)
        {
            getGroupList();
        }
        List<string> deletedPermissions = new List<string>();
        private void groupDataPermissions_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Cells[1].Value.ToString().Equals("*"))
            {
                // Cancel the deletion if the permission belongs to parent.
                e.Cancel = true;
            }
            deletedPermissions.Add(e.Row.Cells[0].Value.ToString());
        }

        private void groupDataPermissions_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
        }

        private void groupDataList_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow row = groupDataList.SelectedRows[0];
            DialogResult usersChoice =
                          MessageBox.Show("Are you sure you want to delete " + row.Cells[0].Value.ToString() + "?", PROGRAMNAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // cancel the delete event
            if (usersChoice == DialogResult.No)
                e.Cancel = true;

        }

        private void groupDataList_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            string name = "";
            DataGridViewRow row = e.Row;
            name = row.Cells[0].Value.ToString();

            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("v2/groups/destroy", "group=" + name);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                groupsUpdateStatus.Text = "Group " + name + " was deleted";
                //                usersChoice = MessageBox.Show("Group " + name + " was deleted", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
                groupRowIndex = -1;
            }
            else
                groupsUpdateStatus.Text = "Group " + name + " could not be deleted, check console for details.";
            //            usersChoice = MessageBox.Show("Group " + name + " could not be deleted, check console for details.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            getGroupList();
        }

        #endregion

        #region  users Tab
        //          Users Tab

        private void loadDefaultGroups()
        {

            JArray groups;
            string name;
            string parent;
            string chatColor;
            string groupPrefix;
            string groupSuffix;
            JArray permissions = null;
            JArray totalPermissions = null;

            List<string> parents = new List<string>();
            List<DataGridViewRow> rows = new List<DataGridViewRow>();

            groupList.Clear();
            usersDataGroup.Items.Clear();
            groupDataParentList.Items.Clear();
            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("AdminREST/GroupList", "");
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                groups = (JArray)results["groups"];
                for (int i = 0; i < groups.Count; i++)
                {
                    JObject innerObj = (JObject)groups[i];
                    name = (String)innerObj["name"];
                    parent = (String)innerObj["parent"];
                    chatColor = (String)innerObj["chatcolor"];
                    groupPrefix = (String)innerObj["groupprefix"];
                    groupSuffix = (String)innerObj["groupsuffix"];
                    permissions = (JArray)innerObj["permissions"];
                    totalPermissions = (JArray)innerObj["totalpermissions"];
                    if (name.Length > 0)
                    {
                        parents.Add(name);
                        //                        usersDataGroup.Items.Add(name);
                        //                        groupDataParentList.Items.Add(name);
                        Group g = new Group(name, parent, chatColor, groupPrefix, groupSuffix, permissions, totalPermissions);
                        groupList.Add(g);
                    }
                }

                usersDataGroup.Items.AddRange(parents.ToArray());
                //               groupDataParentList.Items.AddRange(parents.ToArray());
                groupList.Sort(delegate(Group p1, Group p2)
    {
        return p1.Name.CompareTo(p2.Name);
    }
);
                usersDataGroup.Sorted = true;
            }
            return;
        }

        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            
            MessageBox.Show("Error happened " + anError.Context.ToString());

            if (anError.Context == DataGridViewDataErrorContexts.Commit)
            {
                MessageBox.Show("Commit error");
            }
            if (anError.Context == DataGridViewDataErrorContexts.CurrentCellChange)
            {
                MessageBox.Show("Cell change");
            }
            if (anError.Context == DataGridViewDataErrorContexts.Parsing)
            {
                MessageBox.Show("parsing error");
            }
            if (anError.Context == DataGridViewDataErrorContexts.LeaveControl)
            {
                MessageBox.Show("leave control error");
            }

            if ((anError.Exception) is ConstraintException)
            {
                DataGridView view = (DataGridView)sender;
                view.Rows[anError.RowIndex].ErrorText = "an error";
                view.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "an error";

                anError.ThrowException = false;
            }
        }
        private void getUsers()
        {
            JArray userList = null;
            int id = 0;
            String name = null;
            String group = null;
            DateTime registeredDt;
            DateTime lastAccessedDt;

            string knownIPs;
            string[] IPString;
            int inventoryCount;
            bool hasInventory;
            String searchString;

            String whereClause = "";
            String andClause = " where ";
            searchString = "%";

            usersSearchResults.Text = "";

            if (usersSearchName.Text != null)
                if (usersSearchName.Text.Length > 0)
                {
                    searchString = usersSearchName.Text;
                    if (usersFuzzyName != null && usersFuzzyName.Checked)
                        whereClause = whereClause + andClause + " UserName like '%25" + usersSearchName.Text + "%25'";
                    else
                        whereClause = whereClause + andClause + " UserName like '" + usersSearchName.Text + "'";
                    andClause = " and ";
                }
            if (usersSearchGroup.Text != null)
                if (usersSearchGroup.Text.Length > 0)
                {
                    searchString = usersSearchGroup.Text;
                    if (usersFuzzyGroup != null && usersFuzzyGroup.Checked)
                        whereClause = whereClause + andClause + " UserGroup like '%25" + usersSearchGroup.Text + "%25'";
                    else
                        whereClause = whereClause + andClause + " UserGroup like '" + usersSearchGroup.Text + "'";
                    andClause = " and ";
                }
            if (usersSearchIP.Text != null)
                if (usersSearchIP.Text.Length > 0)
                {
                    searchString = usersSearchIP.Text;
                    if (usersFuzzyIP != null && usersFuzzyIP.Checked)
                        whereClause = whereClause + andClause + " KnownIPs like '%25" + usersSearchIP.Text + "%25'";
                    else
                        whereClause = whereClause + andClause + " KnownIPs like '" + usersSearchIP.Text + "'";
                    andClause = " and ";
                }

            String registered = "";
            String lastAccessed = "";

            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("AdminREST/searchusers", "where=" + whereClause);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                usersDataList.Rows.Clear();
                usersListPermissions.Items.Clear();
                userList = (JArray)results["Users"];
                if (userList.Count == 0)
                    usersSearchResults.Text = "No users found.";
                if (userList.Count == 1)
                    usersSearchResults.Text = userList.Count.ToString() + " user found.";
                if (userList.Count > 1)
                    usersSearchResults.Text = userList.Count.ToString() + " users found.";
                for (int i = 0; i < userList.Count; i++)
                {
                    JObject innerObj = (JObject)userList[i];
                    try
                    {
                        id = (int)innerObj["Id"];
                    }
                    catch (NullReferenceException e)
                    {
                        id = 0;
                    }
                    name = (String)innerObj["UserName"];
                    try
                    {
                        group = (String)innerObj["UserGroup"];
                    }
                    catch (NullReferenceException)
                    {
                        group = "";
                    }

                    try
                    {
                        knownIPs = (String)innerObj["KnownIPs"];
                        knownIPs = knownIPs.Replace("\"", "");
                        knownIPs = knownIPs.Replace("\r\n", "");
                        knownIPs = knownIPs.Replace("[", "");
                        knownIPs = knownIPs.Replace("]", "");
                        IPString = knownIPs.Split(',');
                        String comma = "";
                        knownIPs = "";
                        for (int j = 0; j < IPString.Length; j++)
                        {
                            knownIPs = knownIPs + comma + IPString[j].Trim();
                            comma = ", ";
                        }
                    }
                    catch (NullReferenceException)
                    {
                        knownIPs = "";
                    }

                    try
                    {
                        registered = (String)innerObj["Registered"];
                        registeredDt = DateTime.Parse(registered);
                        registered = String.Format("{0:G}", registeredDt.ToLocalTime());
                    }
                    catch (NullReferenceException)
                    {
                        registered = "";
                    }

                    try
                    {
                        lastAccessed = (String)innerObj["LastAccessed"];
                        if (lastAccessed != null)
                        {
                            lastAccessedDt = DateTime.Parse(lastAccessed);
                            lastAccessed = String.Format("{0:G}", lastAccessedDt.ToLocalTime());
                        }
                        else
                            lastAccessed = "";
                    }
                    catch (NullReferenceException)
                    {
                        lastAccessed = "";
                    }
                    inventoryCount = (int)innerObj["InventoryCount"];
                    if (inventoryCount == 0)
                        hasInventory = false;
                    else
                        hasInventory = true;

                    usersDataList.Rows.Add(name, group, registered, lastAccessed, knownIPs, id, hasInventory);
                }

            }
            else
                usersSearchResults.Text = "No users found.";

            this.usersDataList.Sort(this.usersDataUser, ListSortDirection.Ascending);
            usersAddUser.Enabled = false;
            usersDataList.ClearSelection();
        }
        private Boolean usersModified;
        private void usersDataList_RowsAdded(object sender, System.Windows.Forms.DataGridViewRowsAddedEventArgs e)
        {
            for (int index = e.RowIndex; index <= e.RowIndex + e.RowCount - 1; index++)
            {
                DataGridViewRow row = usersDataList.Rows[index];
                usersModified = !row.IsNewRow;
            }
        }
        private void refreshUsers_Click(object sender, EventArgs e)
        {
            if (groupList == null || groupList.Count == 0)
                loadDefaultGroups();
            getUsers();
        }


        private void usersDataList_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex == -1) return; //check if row index is not selected
            DataGridViewRow row = usersDataList.Rows[e.RowIndex];

            usersKnowIPs.Text = "";
            if (row.Cells[4] == null)
                return;
            if (row.Cells[4].Value == null)
                return;
            usersKnowIPs.Text = row.Cells[4].Value.ToString();

            string groupName = row.Cells[1].Value.ToString();
            usersListPermissions.Items.Clear();

            for (int i = 0; i < groupList.Count; i++)
            {
                if (groupList[i].TotalPermissions != null)
                {
                    if (groupList[i].Name.Equals(groupName))
                    {
                        for (int j = 0; j < groupList[i].TotalPermissions.Count; j++)
                        {
                            ListViewItem item = new ListViewItem(groupList[i].TotalPermissions[j].ToString());
                            usersListPermissions.Items.Add(item);
                        }
                    }
                }
            }

            playerFound = true;
            tabInventory.Enabled = true;
            inputFromImport = false;

        }
        private void usersDataList_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (!usersDataList.Rows[e.RowIndex].IsNewRow)
            {
                if (e.ColumnIndex == 0)
                    e.Cancel = true;
            }
            DataGridViewRow selectedRow = usersDataList.Rows[e.RowIndex];
            DataGridViewComboBoxCell dgv = (DataGridViewComboBoxCell)selectedRow.Cells[1];
            dgv.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;

            usersNewRow = e.RowIndex;
            usersAddUser.Enabled = true;
        }
        private int usersNewRow;
        private void searchUsers_Click(object sender, EventArgs e)
        {
            if (groupList == null || groupList.Count == 0)
                loadDefaultGroups();
            getUsers();
        }
        private void usersDataList_Click(object sender, EventArgs e)
        {
            playerFound = true;
            tabInventory.Enabled = true;
            tabPane.SelectedIndex = INVENTORYTAB;
            inputFromImport = false;
        }
        private void clearUserSearch_Click(object sender, EventArgs e)
        {
            usersSearchName.Text = "";
            usersSearchGroup.Text = "";
            usersSearchIP.Text = "";
            usersFuzzyName.Checked = false;
            usersFuzzyGroup.Checked = false;
            usersFuzzyIP.Checked = false;
            usersSearchResults.Text = "";
        }

        private void usersAddUser_Click(object sender, EventArgs e)
        {
            string action = action = "&type=name";
            DataGridViewRow selectedRow = usersDataList.Rows[usersNewRow];

            string name = "";
            if (selectedRow.Cells[0] != null)
                if (selectedRow.Cells[0].Value != null)
                {
                    name = selectedRow.Cells[0].Value.ToString();
                    action = action + "&user=" + name;
                }
            string group = "";
            if (selectedRow.Cells[1] != null)
                if (selectedRow.Cells[1].Value != null)
                {
                    group = selectedRow.Cells[1].Value.ToString();

                    action = action + "&group=" + group;
                }

            if (name.Length == 0)
            {
                usersUpdateStatus.Text = "No name given.";
                return;
            }
            if (group.Length == 0)
            {
                usersUpdateStatus.Text = "No group given.";
                return;
            }
            if (usersModified)
            {
                // And now use this to connect server
                JObject results = ru.communicateWithTerraria("v2/users/update", action);
                string status = (string)results["status"];
                if (status.Equals("200"))
                {
                    usersUpdateStatus.Text = name + " updated";
                }
                else
                {
                    usersUpdateStatus.Text = "User " + name + " could not be updated";
                    string error = (string)results["error"];
                }
            }
            else
            {
                string password = promptPassword("Password for " + name + "?");
                if (password.Length == 0)
                {
                    usersUpdateStatus.Text = "Invalid password.";
                    return;
                }
                action = action + "&password=" + password;
                // And now use this to connect server
                JObject results = ru.communicateWithTerraria("v2/users/create", action);
                string status = (string)results["status"];
                if (status.Equals("200"))
                {
                    usersUpdateStatus.Text = "Invalid password.";
                }
                else
                {
                    usersUpdateStatus.Text = "User " + name + " could not be added";
                    string error = (string)results["error"];
                }
            }
            getUsers();
        }

        private void usersDataList_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {

            DataGridViewRow row = usersDataList.SelectedRows[0];
            DialogResult usersChoice =
                MessageBox.Show("Are you sure you want to delete " + row.Cells[0].Value.ToString() + "?", PROGRAMNAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // cancel the delete event
            if (usersChoice == DialogResult.No)
                e.Cancel = true;
        }

        private void usersDataList_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            string name = "";
            DataGridViewRow row = e.Row;
            name = row.Cells[0].Value.ToString();

            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("v2/users/destroy", "&type=name&user=" + name);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                usersUpdateStatus.Text = "User " + name + " was deleted";
            }
            else
                usersUpdateStatus.Text = "User " + name + " could not be deleted";
        }

        public static string promptPassword(string text)
        {
            string caption = "What is the password?";
            Form prompt = new Form();
            prompt.Width = 250;
            prompt.Height = 150;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 150 };
            Button confirmation = new Button() { Text = "Ok", Left = 75, Width = 100, Top = 75 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.ShowDialog();
            return textBox.Text;
        }

        #endregion

        #region  Log Tab
        // Log Tab

        private void getLog()
        {
            int lineCount;
            lineCount = int.Parse(logNumberOfLines.Text);

            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("AdminREST/getLog", "count=" + lineCount);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                JArray log = (JArray)results["log"];

                logDataList.Items.Clear();
                for (int i = 0; i < log.Count; i++)
                {
                    ListViewItem item = new ListViewItem(log[i].ToString());
                    logDataList.Items.Add(item);
                }
                String OriginalPath = (String)results["file"];
                logFilename.Text = OriginalPath;
            }

            /*
            //            var directory = new DirectoryInfo(TShock.Config.LogPath);
            //                        String searchPattern = @"(19|20)\d\d[-](0[1-9]|1[012])[-](0[1-9]|[12][0-9]|3[01]).*.log";
            //                       var log = Directory.GetFiles(TShock.Config.LogPath).Where(path => Regex.Match(path, searchPattern).Success).Last();

                       String logFile = Path.GetFullPath(TextLog.fileName);
                       String fileName = Path.GetFileName(TextLog.fileName);
                       logFilename.Text = fileName;
            
                        FileStream logFileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        StreamReader logFileReader = new StreamReader(logFileStream);

                        int limit = System.Convert.ToInt32(lineCount);
                        var buffor = new Queue<string>(limit);
                        while (!logFileReader.EndOfStream)
                        {
                            string line = logFileReader.ReadLine();
                            if (buffor.Count >= limit)
                                buffor.Dequeue();
                            buffor.Enqueue(line);
                        }

                        logDataList.Items.Clear();
                        foreach (string line in buffor)
                        {
                            ListViewItem item = new ListViewItem(line.ToString());
                            logDataList.Items.Add(item);
                        }
                        string[] LogLinesEnd = buffor.ToArray();
                        // Clean up
                        logFileReader.Close();
                        logFileStream.Close();

                         * */

        }

        private void refreshLog_Click(object sender, EventArgs e)
        {
            getLog();
        }
        #endregion

        #region Utility
        private void btnSaveDefaults_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.RegistryKey exampleRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("RemoteAdminConsole");
            exampleRegistryKey.SetValue("UserId", txtUserId.Text);
            exampleRegistryKey.SetValue("Password", passwordField.Text);
            exampleRegistryKey.SetValue("Server", txtURL.Text);
            ru.conn.UserId = (string)exampleRegistryKey.GetValue("UserId");
            ru.conn.Password = (string)exampleRegistryKey.GetValue("Password");
            ru.conn.Server = (string)exampleRegistryKey.GetValue("Server");
            exampleRegistryKey.Close();
        }
        private void getDefaults()
        {
            Microsoft.Win32.RegistryKey exampleRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("RemoteAdminConsole");
            ru.conn.UserId = (string)exampleRegistryKey.GetValue("UserId");
            ru.conn.Password = (string)exampleRegistryKey.GetValue("Password");
            ru.conn.Server = (string)exampleRegistryKey.GetValue("Server");
            txtUserId.Text = ru.conn.UserId;
            passwordField.Text = ru.conn.Password;
            txtURL.Text = ru.conn.Server;

        }
        private void btnClearDefaults_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.RegistryKey exampleRegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("RemoteAdminConsole");
            exampleRegistryKey.SetValue("UserId", "");
            exampleRegistryKey.SetValue("Password", "");
            exampleRegistryKey.SetValue("Server", "");
            ru.conn.UserId = "";
            ru.conn.Password = "";
            ru.conn.Server = "";
            txtUserId.Text = ru.conn.UserId;
            passwordField.Text = ru.conn.Password;
            txtURL.Text = ru.conn.Server;
            exampleRegistryKey.Close();

        }

        private System.Drawing.Color getColor(Color value)
        {
            return System.Drawing.Color.FromArgb(value.R, value.G, value.B);
        }

        private void getItemSpriteImage()
        {
            item_0 = new Bitmap(RemoteAdminConsole.Properties.Resources.item_0);
            if (item_0 == null)
                Console.WriteLine("null item_0");

            sprite = new Bitmap(RemoteAdminConsole.Properties.Resources.sprite);
            if (item_0 == null)
                Console.WriteLine("null sprite");
            int width = 48;
            int height = 48;
            int rows = MAXITEMS / 100 + 1;   //we assume the no. of rows and cols are known and each chunk has equal width and height
            int cols = 100;
            sprites = new Bitmap[rows * cols];

            int lastRow = 0;
            System.Drawing.Rectangle rectangle;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols && (lastRow < itemList.Length); j++)
                {
                    try
                    {
                        rectangle = new System.Drawing.Rectangle(j * width, i * height, width, height);
                        sprites[lastRow] = (Bitmap)sprite.Clone(rectangle, sprite.PixelFormat);
                        lastRow++;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("E-" + (lastRow - 1) + " " + itemList[lastRow - 1] + " " + j + "-" + (j * width) + ":" + i + "-" + (i * height));
                    }
                }
            }
        }

        private void loadItemNames()
        {
            int counter = 0;
            string[] linearray;
            string name;
            int netId;
            int stackSize;
            int prefix;
            for (int i = 0; i < itemList.Length; i++)
                itemList[i] = new Item();
            Assembly assem = Assembly.GetExecutingAssembly();

            for (int i = 0; i < prefixList.Length; i++)
                prefixList[i] = new Prefixs();
            counter = 0;
            inventoryItemsList.Items.Clear();
            string[] line = RemoteAdminConsole.Properties.Resources.itemlist.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < line.Length; i++)
            {
                linearray = line[i].Split('`');
                netId = Int32.Parse(linearray[0].Trim());
                name = linearray[1].Trim();
                stackSize = Int32.Parse(linearray[3].Trim());
                prefix = Int32.Parse(linearray[2].Trim());
                itemList[counter] = new Item(name, netId, stackSize, prefix);
                if (name.Length > 0)
                {
                    ListViewItem item = new ListViewItem(name);
                    item.SubItems.Add(netId.ToString());
                    item.SubItems.Add(stackSize.ToString());
                    item.SubItems.Add(prefix.ToString());
                    inventoryItemsList.Items.Add(item);
                }
                counter++;
            }
            counter = 0;
            cmbxUserItemPrefix.Items.Clear();
            cmbxUserItemPrefix.Items.Add("<none>");

            line = RemoteAdminConsole.Properties.Resources.prefixlist.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < line.Length; i++)
            {
                linearray = line[i].Split(':');
                name = linearray[0].Trim();
                prefix = Int32.Parse(linearray[1].Trim());
                prefixList[counter] = new Prefixs(name, prefix);
                cmbxUserItemPrefix.Items.Add(name);

                counter++;
            }

        }

        /// <summary>
        /// Gives an image based on the item name given
        /// </summary>
        /// <param name="img">The name of the item to create an image for</param>
        /// <returns>Retrurns image associated with the item in question</returns>
        public System.Drawing.Image ItemImage(string img)
        {
            try
            {
                System.Reflection.Assembly thisExe;
                thisExe = System.Reflection.Assembly.GetExecutingAssembly();
                String item = "GUIMain.Resources.Item_" + img + ".png";
                System.IO.Stream file = thisExe.GetManifestResourceStream(item);
                return Image.FromStream(file);

            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Console Tab
        private void getConsole()
        {
            consoleCommand.Text = "";
            consoleBroadcast.Text = "";
            consoleOutput.Text = "";
            consoleMOTD.Text = "";

            getMOTD();
        }
        private void consoleCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                consoleSubmitCommand.PerformClick();
            }
        }
        private void consoleBroadcast_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                consoleSubmitBroadcast.PerformClick();
            }
        }
        private void getMOTD()
        {
            String output = "";
            consoleMOTD.Text = "";

            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("v3/server/motd", "");
            string status = (string)results["status"];
            if (status.Equals("200"))
            {

                JArray motd = (JArray)results["motd"];
                // get an array from the JSON object 

                for (int i = 0; i < motd.Count(); i++)
                {
                    output = output + motd[i] + "\r\n";
                }

            }
            consoleMOTD.Text = output;

            /*
             string motdFilePath = Path.Combine(TShock.SavePath, "motd.txt");
             consoleMOTD.Text = "";

             string motd = consoleMOTD.Text;
             string line;
             if (File.Exists(motdFilePath))
             {
                 motd = "";
                 // Read the file and display it line by line.
                 System.IO.StreamReader file = new System.IO.StreamReader(motdFilePath);
                 while ((line = file.ReadLine()) != null)
                 {
                     motd = motd + line + "\r\n";
                 }
                 file.Close();

             }
              * */
        }
        private void consoleSubmitCommand_Click(object sender, EventArgs e)
        {
            string output = "";
            JObject results = ru.communicateWithTerraria("v3/server/rawcmd", "&cmd=/" + consoleCommand.Text);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                JArray response = (JArray)results["response"];
                // get an array from the JSON object 

                // add text to it; we want to make it scroll
                for (int i = 0; i < response.Count; i++)
                {
                    output = output + response[i].ToString().Replace('[', ' ').Replace(']', ' ') + "\r\n";
                }
            }
            consoleOutput.Text = output;
        }
        private void serverChat_TextChanged(object sender, EventArgs e)
        {
            serverChatStatus.Text = "";
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                int rowIndex = serverDataPlayers.CurrentRow.Index;
                if (rowIndex < 0)
                {
                    serverChatStatus.Text = "No player selected.";
                    return;
                }
                string playerIndex = serverDataPlayers.Rows[rowIndex].Cells[4].Value.ToString();
                JObject results = ru.communicateWithTerraria("AdminREST/Chat", "index=" + playerIndex + "&msg=" + serverChat.Text);
                string status = (string)results["status"];
                if (status.Equals("200"))
                {
                    String response = (String)results["response"];
                    // get an array from the JSON object 

                    // add text to it; we want to make it scroll
                    serverChatStatus.Text = response;
                }
            }
        }

        private void consoleSubmitBroadcast_Click(object sender, EventArgs e)
        {
            JObject results = ru.communicateWithTerraria("AdminREST/Broadcast", "index=0&msg=" + consoleBroadcast.Text);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                String response = (String)results["response"];
                // get an array from the JSON object 

                // add text to it; we want to make it scroll
                consoleBroadcast.Text = response;
            }
        }

        private void consoleSubmitMOTD_Click(object sender, EventArgs e)
        {
            JObject results = ru.communicateWithTerraria("AdminREST/updateMOTD", "&motd=" + consoleMOTD.Text);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                getMOTD();
            }
        }
        private void consoleSaveWorld_Click(object sender, EventArgs e)
        {
            //           TShockAPI.TShock.s SaveManager.Instance.wSaveWorld();

        }

        private void consoleReloadServer_Click(object sender, EventArgs e)
        {
            /*
            TSPlayer tr = new TSPlayer(0);
            TShock.Utils.Reload(tr);
            MessageBox.Show("Configuration, permissions, and regions reload complete. Some changes may require a server restart.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
             * */
        }

        private void tabConsole_Enter(object sender, EventArgs e)
        {
            getConsole();
        }

        private void consoleRefresh_Click(object sender, EventArgs e)
        {
            getConsole();
        }
        #endregion

        #region  Inventory Tab
        // Inventory Tab
        /// <summary>
        /// Gets a list of SSCInventoryLog entries. 
        /// </summary>
        public SSCInventory GetSSCInventory(int account)
        {
            JArray inventory;
            String inventoryList;
            Color hairColor;
            Color pantsColor;
            Color shirtColor;
            Color underShirtColor;
            Color shoeColor;
            Color skinColor;
            Color eyeColor;
            int hair = 0;
            int hairDye = 0;
            bool isPlaying = false;

            SSCInventory SSCInventory = new SSCInventory();
            SSCInventory.Inventory = "";
            hairColor = Color.Black;
            pantsColor = Color.Black;
            shirtColor = Color.Black;
            underShirtColor = Color.Black;
            shoeColor = Color.Black;
            skinColor = Color.Black;
            eyeColor = Color.Black;
            inventoryList = "";

            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("AdminREST/getinventory", "account=" + account);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                try
                {
                    inventory = (JArray)results["inventory"];
                    JObject innerObj = (JObject)inventory[0];
                    inventoryList = (String)innerObj["Inventory"];
                    JObject colorObj = null;
                    colorObj = (JObject)innerObj["SkinColor"];
                    skinColor = Color.FromArgb((int)colorObj["A"], (int)colorObj["R"], (int)colorObj["G"], (int)colorObj["B"]);
                    colorObj = (JObject)innerObj["HairColor"];
                    hairColor = Color.FromArgb((int)colorObj["A"], (int)colorObj["R"], (int)colorObj["G"], (int)colorObj["B"]);
                    colorObj = (JObject)innerObj["PantsColor"];
                    pantsColor = Color.FromArgb((int)colorObj["A"], (int)colorObj["R"], (int)colorObj["G"], (int)colorObj["B"]);
                    colorObj = (JObject)innerObj["ShirtColor"];
                    shirtColor = Color.FromArgb((int)colorObj["A"], (int)colorObj["R"], (int)colorObj["G"], (int)colorObj["B"]);
                    colorObj = (JObject)innerObj["UnderShirtColor"];
                    underShirtColor = Color.FromArgb((int)colorObj["A"], (int)colorObj["R"], (int)colorObj["G"], (int)colorObj["B"]);
                    colorObj = (JObject)innerObj["ShoeColor"];
                    shoeColor = Color.FromArgb((int)colorObj["A"], (int)colorObj["R"], (int)colorObj["G"], (int)colorObj["B"]);
                    colorObj = (JObject)innerObj["EyeColor"];
                    eyeColor = Color.FromArgb((int)colorObj["A"], (int)colorObj["R"], (int)colorObj["G"], (int)colorObj["B"]);
                    isPlaying = (Boolean)innerObj["IsPlaying"];
                }
                catch (NullReferenceException)
                {
                    inventoryList = "0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~";
                }
                catch (InvalidCastException)
                {
                    inventoryList = "0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~";
                }
            }
            SSCInventory = new SSCInventory(account, inventoryList, hair, hairDye, hairColor, pantsColor, shirtColor, underShirtColor, shoeColor, skinColor, eyeColor, isPlaying);
            return SSCInventory;
        }
        SSCInventory SSCInventory;
        private void getInventory()
        {
            if (!playerFound)
            {
                return;
            }
            DataGridViewRow selectedRow = usersDataList.CurrentRow;
            string name = selectedRow.Cells[0].Value.ToString();
            int account = Int32.Parse(selectedRow.Cells[5].Value.ToString());
            if (!inputFromImport)
                SSCInventory = GetSSCInventory(account);
            inventoryUpdate.Enabled = true;
            if (SSCInventory.IsPlaying)
            {
                sscAccountName.Text = name + " [Player Active]";
                inventoryUpdate.Enabled = false;
            }
            else
                sscAccountName.Text = name;

            inventoryUpdateStatus.Text = "";
            itemPreview.Image = item_0;
            itemPreview.Text = "";
            itemPreviewText.Text = "";
            txtStackSize.Text = "";

            inventoryToolTip.SetToolTip(itemPreview, "");
            inventoryPreviewNetId = 0;
            inventoryPrefixIndex = 0;

            string[] inventory = SSCInventory.Inventory.Split('~');
            for (int i = 0; i < slotItems.Length; i++)
                slotItems[i] = new Item();

            SetSSCInventorySlot(this, "sscItem", inventory);

            sscHairColor.BackColor = getColor(SSCInventory.HairColor);
            ToolTipColor(sscHairColor, SSCInventory.HairColor);
            sscEyeColor.BackColor = getColor(SSCInventory.EyeColor);
            ToolTipColor(sscEyeColor, SSCInventory.EyeColor);
            sscSkinColor.BackColor = getColor(SSCInventory.SkinColor);
            ToolTipColor(sscSkinColor, SSCInventory.SkinColor);
            sscShirtColor.BackColor = getColor(SSCInventory.ShirtColor);
            ToolTipColor(sscShirtColor, SSCInventory.ShirtColor);
            sscUnderShirtColor.BackColor = getColor(SSCInventory.UnderShirtColor);
            ToolTipColor(sscUnderShirtColor, SSCInventory.UnderShirtColor);
            sscPantsColor.BackColor = getColor(SSCInventory.PantsColor);
            ToolTipColor(sscPantsColor, SSCInventory.PantsColor);
            sscShoesColor.BackColor = getColor(SSCInventory.ShoeColor);
            ToolTipColor(sscShoesColor, SSCInventory.ShoeColor);

        }
        private Item[] slotItems = new Item[MAXITEMS];
        public void SetSSCInventorySlot(Control control, String group, string[] inventory)
        {
            if (control is Button)
            {
                Button lbl = (Button)control;
                if (lbl.Name.StartsWith(group))
                {
                    int slotIndex = Int32.Parse(lbl.Name.Substring(group.Length)) - 1;
                    String n;
                    if (slotIndex >= inventory.Length)
                        n = "0,0,0";
                    else
                        n = inventory[slotIndex];
                    int netId = Int32.Parse(n.Split(',')[0]);
                    int prefix = 0;
                    int stacks = 0;
                    try
                    {
                        stacks = Int32.Parse(n.Split(',')[1]);
                        prefix = Int32.Parse(n.Split(',')[2]);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        prefix = 0;
                        stacks = 0;
                    }
                    if (slotIndex > EQUIPMENTITEMS || netId == 0)
                        lbl.Text = "";
                    else
                    {
                        if (stacks == 0)
                            lbl.Text = "";
                        else
                            lbl.Text = stacks.ToString();
                    }

                    lbl.Image = sprites[ITEMOFFSET + netId];

                    if (prefix > 0 && prefix <= prefixList.Length)
                        inventoryToolTip.SetToolTip(lbl, prefixList[prefix - 1].Name + " " + itemList[ITEMOFFSET + netId].Name);
                    else
                        inventoryToolTip.SetToolTip(lbl, itemList[ITEMOFFSET + netId].Name);
                    Item slot = new Item(itemList[ITEMOFFSET + netId].Name, netId, stacks, prefix);
                    slotItems[slotIndex] = slot;
                }
            }
            else
                foreach (Control child in control.Controls)
                {
                    SetSSCInventorySlot(child, group, inventory);
                }

        }
        private void ToolTipColor(Label lbl, Color c)
        {
            System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(lbl, "RGB(" + c.R + "," + c.G + "," + c.B + ")");
        }

        private int findInventoryItem(String name)
        {
            for (int i = 0; i < MAXITEMS; i++)
            {
                if (itemList[i].Name.Equals(name))
                    return i - ITEMOFFSET;
            }

            return 0;

        }
        public int? EncodeColor(Color? color)
        {
            if (color == null)
                return null;

            return BitConverter.ToInt32(new[] { color.Value.R, color.Value.G, color.Value.B, color.Value.A }, 0);
        }

        private void inventoryClearAll_Click(object sender, EventArgs e)
        {
            DialogResult usersChoice =
MessageBox.Show("Are you sure you want to erase all items in this inventory?", PROGRAMNAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // cancel the delete event
            if (usersChoice == DialogResult.No)
                return;

            SSCInventory.Inventory = "0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~0,0,0~";

            string[] inventory = SSCInventory.Inventory.Split('~');
            for (int i = 0; i < slotItems.Length; i++)
                slotItems[i] = new Item();

            SetSSCInventorySlot(this, "sscItem", inventory);

        }
        private void inventoryInit_Click(object sender, EventArgs e)
        {
            DialogResult usersChoice =
MessageBox.Show("Are you sure you want to replace all items in this inventory\r\nwith the default config inventory list?", PROGRAMNAME, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // cancel the delete event
            if (usersChoice == DialogResult.No)
                return;

            // And now use this to connect server 
            JObject results = ru.communicateWithTerraria("AdminREST/getConfig", "config=sscconfig.json");
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                JObject configOptions = (JObject)results["config"];
                JArray startingInventory = (JArray)configOptions["StartingInventory"];

                int netId = 0;
                int stack = 0;
                int prefix = 0;
                int slot = 0;
                string sep = "";
                SSCInventory.Inventory = "";
                for (int i = 0; i < startingInventory.Count; i++)
                {
                    netId = (int)startingInventory[i]["netID"];
                    stack = (int)startingInventory[i]["stack"];
                    prefix = (int)startingInventory[i]["prefix"];

                    SSCInventory.Inventory = SSCInventory.Inventory + sep + netId.ToString() + "," + stack.ToString() + "," + prefix.ToString();
                    sep = "~";
                    slot++;
                }
                for (int i = slot; i < MAXITEMSLOTS; i++)
                {
                    SSCInventory.Inventory = SSCInventory.Inventory + sep + "0,0,0";
                    sep = "~";
                }

                string[] inventory = SSCInventory.Inventory.Split('~');
                for (int i = 0; i < slotItems.Length; i++)
                    slotItems[i] = new Item();

                SetSSCInventorySlot(this, "sscItem", inventory);
            }
        }
        private void inventorySlotReplace_Click(object sender, EventArgs e)
        {
            if (itemLastClicked != null)
            {
                if (itemLastClicked.Image != null)
                    if (itemPreview.Image != null)
                        itemLastClicked.Image = itemPreview.Image;

                string name = itemLastClicked.Name.Substring(7);
                int slot = Int32.Parse(name) - 1;
                string[] inventory = SSCInventory.Inventory.Split('~');
                if (slot > EQUIPMENTITEMS)
                {
                    itemLastClicked.Text = "";
                    inventoryToolTip.SetToolTip(itemLastClicked, inventoryToolTip.GetToolTip(itemPreview));
                    inventory[slot] = inventoryPreviewNetId.ToString() + ",0," + inventoryPrefixStackSize.ToString();
                }
                else
                {
                    itemLastClicked.Text = itemPreview.Text;
                    inventoryToolTip.SetToolTip(itemLastClicked, inventoryToolTip.GetToolTip(itemPreview));
                    inventory[slot] = inventoryPreviewNetId.ToString() + "," + inventoryPrefixStackSize.ToString() + "," + inventoryPrefixStackSize.ToString();
                }
                SSCInventory.Inventory = string.Join("~", inventory);
            }
        }

        private void inventoryUpdate_Click(object sender, EventArgs e)
        {
            string update = "UPDATE tsCharacter set ";
            update += " HairColor=" + EncodeColor(sscHairColor.BackColor).ToString();
            update += ",EyeColor=" + EncodeColor(sscEyeColor.BackColor).ToString();
            update += ",SkinColor=" + EncodeColor(sscSkinColor.BackColor).ToString();
            update += ",ShirtColor=" + EncodeColor(sscShirtColor.BackColor).ToString();
            update += ",UnderShirtColor=" + EncodeColor(sscUnderShirtColor.BackColor).ToString();
            update += ",PantsColor=" + EncodeColor(sscPantsColor.BackColor).ToString();
            update += ",ShoeColor=" + EncodeColor(sscShoesColor.BackColor).ToString();
            update += ",Inventory='" + SSCInventory.Inventory + "'";

            DataGridViewRow row = usersDataList.CurrentRow;
            int account = Int32.Parse(row.Cells[5].Value.ToString());

            Console.WriteLine(SSCInventory.Inventory);
            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("AdminREST/updateSSCAccount", "account=" + account + "&update=" + update);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                inventoryUpdateStatus.Text = (string)results["update"];

            }

        }
        private Button itemLastClicked = null;
        private void sscItem_Click(object sender, EventArgs e)
        {
            if (itemLastClicked != null)
            {
                itemLastClicked.FlatAppearance.BorderColor = Color.SteelBlue;
            }
            Button btn = (Button)sender;
            btn.FlatAppearance.BorderColor = Color.Red;
            itemLastClicked = btn;

            if (itemPreview.Image == item_0)
            {
                string name = btn.Name.Substring(7);
                int slot = Int32.Parse(name) - 1;

                inventoryPreviewNetId = slotItems[slot].NetId;
                itemPreview.Image = sprites[ITEMOFFSET + inventoryPreviewNetId];
                if (slotItems[slot].StackSize == 0)
                    itemPreview.Text = "";
                else
                    itemPreview.Text = slotItems[slot].StackSize.ToString();
                inventoryPrefixStackSize = slotItems[slot].StackSize;
                txtStackSize.Text = itemPreview.Text;

                if (slotItems[slot].Prefix > 0 && slotItems[slot].Prefix <= prefixList.Length)
                    inventoryToolTip.SetToolTip(itemPreview, prefixList[slotItems[slot].Prefix - 1].Name + " " + itemList[ITEMOFFSET + slotItems[slot].NetId].Name);
                else
                    inventoryToolTip.SetToolTip(itemPreview, itemList[ITEMOFFSET + slotItems[slot].NetId].Name);
                itemPreviewText.Text = inventoryToolTip.GetToolTip(itemPreview);

            }
        }

        int inventoryPrefixStackSize = 0;
        int inventoryPrefixIndex = 0;
        int inventoryPreviewNetId;
        ToolTip inventoryToolTip;
        private void txtStackSize_TextChanged(object sender, EventArgs e)
        {
            int priorPrefixStackSize = inventoryPrefixStackSize;
            if (txtStackSize.Text.Length == 0)
            {
                itemPreview.Text = "";
                return;
            }
            if (!Int32.TryParse(txtStackSize.Text, out inventoryPrefixStackSize))
            {
                MessageBox.Show("Invalid input - numbers only.", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (inventoryPreviewNetId > 0)
            {
                int maxStackSize = itemList[ITEMOFFSET + inventoryPreviewNetId].StackSize;
                if (inventoryPrefixStackSize > maxStackSize)
                {
                    inventoryPrefixStackSize = maxStackSize;
                    txtStackSize.Text = maxStackSize.ToString();
                    MessageBox.Show("Stack Size greater than allowed for " + itemList[ITEMOFFSET + inventoryPreviewNetId].Name + "\nMax = " + maxStackSize.ToString() + ".", PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            if (inventoryPrefixStackSize == 0)
                itemPreview.Text = "";
            else
                itemPreview.Text = inventoryPrefixStackSize.ToString();
        }

        private void cmbxUserItemPrefix_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = sender as ComboBox;

            string prefix = cmb.SelectedItem.ToString();
            if (prefix.Equals("<none>"))
            {
                inventoryPrefixIndex = 0;
                inventoryToolTip.SetToolTip(itemPreview, " " + itemList[ITEMOFFSET + inventoryPreviewNetId].Name);
                return;
            }
            for (int i = 0; i < prefixList.Length; i++)
            {
                if (prefix.Equals(prefixList[i].Name))
                {
                    inventoryPrefixIndex = prefixList[i].Prefix;
                    inventoryToolTip.SetToolTip(itemPreview, prefixList[i].Name + " " + itemList[ITEMOFFSET + inventoryPreviewNetId].Name);
                    itemPreviewText.Text = inventoryToolTip.GetToolTip(itemPreview);
                    break;
                }

            }
        }

        ListViewItem lastFoundItem = null;
        private void txtItemFilter_TextChanged(object sender, EventArgs e)
        {
            string search = txtItemFilter.Text.ToLower();
            if (lastFoundItem != null)
                lastFoundItem.BackColor = Color.Transparent;
            if (search.Length > 0)
                foreach (ListViewItem item in inventoryItemsList.Items)
                    if (item.Text.ToLower().Contains(search))
                    {
                        inventoryItemsList.Items[item.Index].Selected = true;
                        inventoryItemsList.Select();
                        inventoryItemsList.EnsureVisible(item.Index);
                        item.BackColor = Color.LightSteelBlue;
                        lastFoundItem = item;
                        break;
                    }

            txtItemFilter.Focus();
        }
        private void clearPreviewItem_Click(object sender, EventArgs e)
        {
            inventoryUpdateStatus.Text = "";
            itemPreview.Image = item_0;
            itemPreview.Text = "";
            itemPreviewText.Text = "";
            txtStackSize.Text = "";

            inventoryToolTip.SetToolTip(itemPreview, "");
            inventoryPreviewNetId = 0;
            inventoryPrefixIndex = 0;
        }

        private void inventoryItemsList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void slot_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            // others...
        }

        private void slot_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                var item = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;

            }
            // others...
        }
        private void preview_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            // others...
        }

        private void preview_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListViewItem)))
            {
                ListViewItem item = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
                string name = item.SubItems[0].Text;
                int netId = Int32.Parse(item.SubItems[1].Text);
                int stacks = Int32.Parse(item.SubItems[2].Text);
                int prefix = Int32.Parse(item.SubItems[3].Text);

                inventoryPreviewNetId = netId;
                itemPreview.Image = sprites[ITEMOFFSET + netId];
                if (stacks == 0)
                    itemPreview.Text = "";
                else
                    itemPreview.Text = stacks.ToString();
                inventoryPrefixStackSize = stacks;
                txtStackSize.Text = itemPreview.Text;

                if (prefix > 0 && prefix <= prefixList.Length)
                    inventoryToolTip.SetToolTip(itemPreview, prefixList[prefix - 1].Name + " " + itemList[ITEMOFFSET + netId].Name);
                else
                    inventoryToolTip.SetToolTip(itemPreview, itemList[ITEMOFFSET + netId].Name);
                itemPreviewText.Text = inventoryToolTip.GetToolTip(itemPreview);
            }
        }
        private void inventoryItemsList_DoubleClick(object sender, EventArgs e)
        {

            if (inventoryItemsList.SelectedItems.Count > 0)
            {
                ListViewItem item = inventoryItemsList.SelectedItems[0];

                string name = item.SubItems[0].Text;
                int netId = Int32.Parse(item.SubItems[1].Text);
                int stacks = Int32.Parse(item.SubItems[2].Text);
                int prefix = Int32.Parse(item.SubItems[3].Text);

                inventoryPreviewNetId = netId;
                itemPreview.Image = sprites[ITEMOFFSET + netId];
                if (stacks == 0)
                    itemPreview.Text = "";
                else
                    itemPreview.Text = stacks.ToString();
                inventoryPrefixStackSize = stacks;
                txtStackSize.Text = itemPreview.Text;

                if (prefix > 0 && prefix <= prefixList.Length)
                    inventoryToolTip.SetToolTip(itemPreview, prefixList[prefix - 1].Name + " " + itemList[ITEMOFFSET + netId].Name);
                else
                    inventoryToolTip.SetToolTip(itemPreview, itemList[ITEMOFFSET + netId].Name);
                itemPreviewText.Text = inventoryToolTip.GetToolTip(itemPreview);
            }
        }
        #endregion

        #region Settings
        private void btnLogin_Click(object sender, EventArgs e)
        {
            ru.conn.UserId = txtUserId.Text;
            ru.conn.Password = passwordField.Text;
            ru.conn.Server = txtURL.Text;

            userIcon.Visible = false;
            userLoggedIn.Text = "Not Logged In.";
            if (txtURL.Text.Length == 0)
            {
                MessageBox.Show("Invalid userid/password/server", GUIMain.PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!ru.getToken())
            {
                MessageBox.Show("Invalid userid/password/server", GUIMain.PROGRAMNAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            stopServer.Visible = true;
            userIcon.Visible = true;
            userLoggedIn.Text = "Logged in as " + ru.conn.UserId + ".";
            getServerDetails();
            tabPane.SelectedTab = tabServer;
        }

        #endregion

        #region  Config Tab
        // Player Config
        /// <summary>

        Dictionary<string, JObject> configDescription = new Dictionary<string, JObject>();
        private void getConfig()
        {
            JObject results;
            string status;
            if (configDescription.Count == 0)
            {
                // And now use this to connect server 
                results = ru.communicateWithTerraria("AdminREST/getConfigDescription", "configFile=config.json");
                status = (string)results["status"];
                if (status.Equals("200"))
                {
                    string key;

                    JObject cd = (JObject)results["description"];
                    foreach (JProperty prop in cd.Properties())
                    {
                        key = prop.Name;
                        JObject d = (JObject)prop.Value;
                        string x = (string)d["definition"];
                        configDescription.Add(key, d);
                    }
                }
            }
            configDataList.Rows.Clear();
            // And now use this to connect server 
            results = ru.communicateWithTerraria("AdminREST/getConfig", "config=config.json");
            status = (string)results["status"];
            if (status.Equals("200"))
            {
                string key;
                String definition = "";
                String defaultx = "";
                object rawOption;
                JObject description;
                JObject configOptions = (JObject)results["config"];
                foreach (JProperty prop in configOptions.Properties())
                {
                    key = prop.Name;
                    rawOption = prop.Value;
                    try
                    {
                        description = (JObject)configDescription[key];
                        definition = (string)description["definition"];
                        defaultx = (string)description["default"];
                    }
                    catch (KeyNotFoundException)
                    {
                        definition = "";
                        defaultx = "";
                    }
                    configDataList.Rows.Add(key, rawOption, definition, defaultx);
                }
            }
        }
        private void refreshConfig_Click(object sender, EventArgs e)
        {
            getConfig();
        }
        #endregion

        #region  SSCConfig Tab
        // Player SSCConfig
        /// <summary>

        private void getSSCConfig()
        {

            sscconfigDataList.Rows.Clear();
            // And now use this to connect server 
            JObject results = ru.communicateWithTerraria("AdminREST/getConfig", "config=sscconfig.json");
            string status = (string)results["status"];
            if (status.Equals("200"))
            {
                string key;
                object rawOption;
                JObject configOptions = (JObject)results["config"];
                foreach (JProperty prop in configOptions.Properties())
                {
                    key = prop.Name;
                    rawOption = prop.Value;
                    sscconfigDataList.Rows.Add(key, rawOption);
                }
            }
        }
        private void refreshSSCConfig_Click(object sender, EventArgs e)
        {
            getSSCConfig();
        }
        #endregion

        private void bodyColor_Click(object sender, EventArgs e)
        {
            //attempt to cast the sender as a label
            Label lbl = sender as Label;

            if (lbl != null)
            {
                System.Drawing.Color newColor = tabColorPickerDialog(lbl.BackColor.R.ToString() + "," + lbl.BackColor.G.ToString() + "," + lbl.BackColor.B.ToString());
                lbl.BackColor = newColor;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }
        bool inputFromImport = false;
        private void inventoryImport_Click(object sender, EventArgs e)
        {
            Color hairColor;
            Color pantsColor;
            Color shirtColor;
            Color underShirtColor;
            Color shoeColor;
            Color skinColor;
            Color eyeColor;
            int hair = 0;
            int hairDye = 0;
            int account = -1;
            bool isPlaying = false;

            // Create an instance of the open file dialog box.
            OpenFileDialog playerFile = new OpenFileDialog();

            // Set filter options and filter index.
            playerFile.Filter = "Player Files (.plr)|*.plr|All Files (*.*)|*.*";
            playerFile.FilterIndex = 1;

            playerFile.Multiselect = false;

            // Call the ShowDialog method to show the dialog box.
            DialogResult response = playerFile.ShowDialog();

            // Process input if the user clicked OK.
            if (response == DialogResult.OK)
            {

                Player p = new Player(playerFile.FileName);
                p.LoadPlayer(playerFile.FileName);
                inputFromImport = true;
                int i = 0;

                hairColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                skinColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                eyeColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                shirtColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                underShirtColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                pantsColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                shoeColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                isPlaying = false;
                string inventoryList = "";
                string sep = "";

                for (int j = 0; j < p.inv.Length; j++)
                {
                    inventoryList = inventoryList + sep + p.inv[j].NetId + "," + p.inv[j].StackSize + "," + p.inv[j].Prefix;
                    sep = "~";
                }
                for (int j = 0; j < p.armor.Length; j++)
                    inventoryList = inventoryList + sep + p.armor[j].NetId + "," + p.armor[j].StackSize + "," + p.armor[j].Prefix;

                for (int j = 0; j < p.accessories.Length; j++)
                    inventoryList = inventoryList + sep + p.accessories[j].NetId + "," + p.accessories[j].StackSize + "," + p.accessories[j].Prefix;

                for (int j = 0; j < p.vanity.Length; j++)
                    inventoryList = inventoryList + sep + p.vanity[j].NetId + "," + p.vanity[j].StackSize + "," + p.vanity[j].Prefix;

                for (int j = 0; j < p.socialAccessories.Length; j++)
                    inventoryList = inventoryList + sep + p.socialAccessories[j].NetId + "," + p.socialAccessories[j].StackSize + "," + p.socialAccessories[j].Prefix;

                for (int j = 0; j < p.dye.Length; j++)
                    inventoryList = inventoryList + sep + p.dye[j].NetId + "," + p.dye[j].StackSize + "," + p.dye[j].Prefix;
                SSCInventory = new SSCInventory(account, inventoryList, hair, hairDye, hairColor, pantsColor, shirtColor, underShirtColor, shoeColor, skinColor, eyeColor, isPlaying);
                getInventory();
            }
        }

        private void inventoryExport_Click(object sender, EventArgs e)
        {
            Color hairColor;
            Color pantsColor;
            Color shirtColor;
            Color underShirtColor;
            Color shoeColor;
            Color skinColor;
            Color eyeColor;
            int hair = 0;
            int hairDye = 0;
            int account = -1;
            bool isPlaying = false;

            // Create an instance of the open file dialog box.
            SaveFileDialog playerFile = new SaveFileDialog();

            // Set filter options and filter index.
            playerFile.Filter = "Player Files (.plr)|*.plr|All Files (*.*)|*.*";
            playerFile.FilterIndex = 1;

            // Call the ShowDialog method to show the dialog box.
            DialogResult response = playerFile.ShowDialog();

            // Process input if the user clicked OK.
            if (response == DialogResult.OK)
            {
                Player p = new Player(playerFile.FileName);
                p.LoadPlayer("");
                int i = 0;

                hairColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                skinColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                eyeColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                shirtColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                underShirtColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                pantsColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                shoeColor = Color.FromArgb(p.Colors[i].R, p.Colors[i].G, p.Colors[i++].B);
                isPlaying = false;

                string[] inventoryList = SSCInventory.Inventory.Split('~');
                string[] inv;
                i = 0;
                for (int j = 0; j < p.inv.Length; j++)
                {
                    inv = inventoryList[i++].Split(',');
                    p.inv[j].NetId = Int32.Parse(inv[0]);
                    p.inv[j].StackSize = Int32.Parse(inv[0]);
                    p.inv[j].Prefix = Int16.Parse(inv[0]);
                }
                for (int j = 0; j < p.armor.Length; j++)
                {
                    inv = inventoryList[i++].Split(',');
                    p.armor[j].NetId = Int32.Parse(inv[0]);
                    p.armor[j].Prefix = Int16.Parse(inv[0]);
                }

                for (int j = 0; j < p.accessories.Length; j++)
                {
                    inv = inventoryList[i++].Split(',');
                    p.accessories[j].NetId = Int32.Parse(inv[0]);
                    p.accessories[j].Prefix = Int16.Parse(inv[0]);
                }

                for (int j = 0; j < p.vanity.Length; j++)
                {
                    inv = inventoryList[i++].Split(',');
                    p.vanity[j].NetId = Int32.Parse(inv[0]);
                    p.vanity[j].Prefix = Int16.Parse(inv[0]);
                }

                for (int j = 0; j < p.socialAccessories.Length; j++)
                {
                    inv = inventoryList[i++].Split(',');
                    p.socialAccessories[j].NetId = Int32.Parse(inv[0]);
                    p.socialAccessories[j].Prefix = Int16.Parse(inv[0]);
                }

                for (int j = 0; j < p.dye.Length; j++)
                {
                    inv = inventoryList[i++].Split(',');
                    p.dye[j].NetId = Int32.Parse(inv[0]);
                    p.dye[j].Prefix = Int16.Parse(inv[0]);
                }
                p.SavePlayer(playerFile.FileName);
            }

        }




    }
}