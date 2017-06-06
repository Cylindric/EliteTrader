using EliteTrader;
using System;
using System.Data.Entity;

namespace Trade
{
    class TradeContext : DbContext
    {
        public DbSet<EDStation> Stations { get; set; }
        public DbSet<EDSystem> Systems { get; set; }  
        
        public TradeContext(): base("TradeContext")
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));    
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TradeContext, Migrations.Configuration>());
        }
    }
}
