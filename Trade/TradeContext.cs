using EliteTrader.Models;
using System;
using System.Data.Entity;
//using System.Data.Entity.Infrastructure;
using Trade.Models;

namespace Trade
{
    class TradeContext : DbContext
    {
        public DbSet<EDStation> Stations { get; set; }
        public DbSet<EDSystem> Systems { get; set; }  
        public DbSet<Settings> Settings { get; set; }

        public TradeContext(): base("TradeContext")
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));    
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TradeContext, Migrations.Configuration>());
            ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.CommandTimeout = 180;
        }
    }
}
