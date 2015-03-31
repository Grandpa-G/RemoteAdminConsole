using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAdminConsole
{
    public class UserList
    {
        public Int32 Id { get; set; }

        public string UserName { get; set; }

        public string UserGroup { get; set; }
        public string Registered { get; set; }

        public string LastAccessed { get; set; }

        public string KnownIPs { get; set; }
        public Int32 InventoryCount { get; set; }

        public UserList(Int32 id, string username, string usergroup, string registered, string lastaccessed, string knownIPs, Int32 inventoryCount)
        {
            Id = id;
            UserName = username;
            UserGroup = usergroup;
            Registered = registered;
            LastAccessed = lastaccessed;
            KnownIPs = knownIPs;
            InventoryCount = inventoryCount;
        }

        public UserList()
        {
            Id = 0;
            UserName = string.Empty;
            UserGroup = string.Empty;
            Registered = string.Empty;
            LastAccessed = string.Empty;
            KnownIPs = string.Empty;
            InventoryCount = 0;
        }
    }

}
