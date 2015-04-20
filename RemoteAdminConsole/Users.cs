using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
namespace RemoteAdminConsole
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string UUID { get; set; }
        public string Group { get; set; }
        public string Registered { get; set; }
        public string LastAccessed { get; set; }
        public string KnownIps { get; set; }

        public User(string name, string pass, string uuid, string group, string registered, string last, string known)
        {
            Name = name;
            Password = pass;
            UUID = uuid;
            Group = group;
            Registered = registered;
            LastAccessed = last;
            KnownIps = known;
        }

        public User()
        {
            Name = "";
            Password = "";
            UUID = "";
            Group = "";
            Registered = "";
            LastAccessed = "";
            KnownIps = "";
        }
    }
    
    public class Ban
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public string Reason { get; set; }
        public string BanningUser { get; set; }
        public string Date { get; set; }
        public string Expiration { get; set; }

        public Ban(string name, string ip, string reason, string banningUser, string date, string expiration)
        {
            Name = name;
            IP = ip;
            Reason = reason;
            BanningUser = banningUser;
            Date = date;
            Expiration = expiration;
        }

        public Ban()
        {
            Name = "";
            IP = "";
            Reason = "";
            BanningUser = "";
            Date = "";
            Expiration = "";
        }
    }
     public class Group
     {
        public string Name { get; set; }
        public string Parent { get; set; }
        public string ChatColor { get; set; }
        public string GroupPrefix { get; set; }
        public string GroupSuffix { get; set; }
        public JArray Permissions { get; set; }
        public JArray TotalPermissions { get; set; }

        public Group(string name, string parent, string chatcolor, string groupprefix, string groupsuffix, JArray permissions, JArray totalpermissions)
        {
            Name = name;
            Parent = parent;
            ChatColor = chatcolor;
            GroupPrefix = groupprefix;
            GroupSuffix = groupsuffix;
            Permissions = permissions;
            TotalPermissions = totalpermissions;
        }

        public Group()
        {
            Name = "";
            Parent = "";
            ChatColor = "";
            GroupPrefix = "";
            GroupSuffix = "";
            Permissions = null;
            TotalPermissions = null;
        }
    }
    

}
