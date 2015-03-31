using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAdminConsole
{
    class ClassDefinition
    {
    }
    public class Connecton
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public Connecton(string userId, string password, string server)
        {
            UserId = userId;
            Password = password;
            Server = server;
        }

               public Connecton()
        {
            UserId = null;
            Password = null;
            Server = null;
        }
    }
    public class Stats
    {
        public DatabaseStat DBStats { get; set; }
        public List<LogStat> LogStats { get; set; }

        public Stats(DatabaseStat dbStats, List<LogStat> logStats)
        {
            DBStats = dbStats;
            LogStats = logStats;
        }

        public Stats()
        {
            DBStats = null;
            LogStats = null;
        }
    }
    
    public class DatabaseStat
    {
        public double DBSize { get; set; }
        public int TableRows { get; set; }
        public double TableSize { get; set; }
        public string Version { get; set; }
        public string DBType { get; set; }

        public DatabaseStat(double dbSize, int tableRows, double tableSize, string version, string dbType)
        {
            DBSize = dbSize;
            TableRows = tableRows;
            TableSize = tableSize;
            Version = version;
            DBType = dbType;
                    }

        public DatabaseStat()
        {
            DBSize = 0;
            TableRows = 0;
            TableSize = 0;
            Version = "";
            DBType = "";
        }
    }
    
    public class LogStat
    {
        public int Count { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public LogStat(int count, int year, int month)
        {
            Count = count;
            Year = year;
            Month = month;
        }

        public LogStat()
        {
            Count = 0;
            Year = 0;
            Month = 0;
        }
    }

   public class LogList
    {
        public int ID { get; set; }
        public string TimeStamp { get; set; }
        public int LogLevel { get; set; }
        public string Caller { get; set; }
        public string Message { get; set; }

        public LogList(int id, string timeStamp, int loglevel, string caller, string message)
        {
            ID = id;
            LogLevel = loglevel;
            TimeStamp = timeStamp;
            Caller = caller;
            Message = message;
        }

        public LogList()
        {
            ID = 0;
            TimeStamp = "";
            LogLevel = 0;
             Caller = "";
            Message = "";
        }
    }
}
