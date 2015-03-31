using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace RemoteAdminConsole
{
    class KickBan
    {
        public KickBan(RemoteUtils r)
        {
            ru = r;
        }

        private RemoteUtils ru;
        public String KickBanPlayer(String action, string playerName, String reason)
        {
            string why = "";

            if (playerName.Length == 0)
                return "Error in finding player " + playerName;

            if (reason.Length > 0)
                why = "&reason=" + reason;
            // And now use this to connect server
            JObject results = ru.communicateWithTerraria("v2/players/" + action, "&player=" + playerName + why);
            string status = (string)results["status"];
            if (status.Equals("200"))
            {

                return "Player " + playerName + " was " + action + "ed";
            }

            return null;
        }
    }

}
