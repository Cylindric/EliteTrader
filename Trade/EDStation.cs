using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Trade;

namespace EliteTrader
{
    public class EDStation
    {
        [Key]
        public int id { get; set; }

        [Index()]
        [MaxLength(100)]
        public string name { get; set; }

        public int? system_id { get; set; }
        public int? updated_at { get; set; }
        public string max_landing_pad_size { get; set; }
        public int? distance_to_star { get; set; }
        public int? government_id { get; set; }
        public string government { get; set; }
        public int? allegiance_id { get; set; }
        public string allegiance { get; set; }
        public int? state_id { get; set; }
        public string state { get; set; }
        public int? type_id { get; set; }
        public string type { get; set; }
        public bool has_blackmarket { get; set; }
        public bool has_market { get; set; }
        public bool has_refuel { get; set; }
        public bool has_repair { get; set; }
        public bool has_rearm { get; set; }
        public bool has_outfitting { get; set; }
        public bool has_shipyard { get; set; }
        public bool has_docking { get; set; }
        public bool has_commodities { get; set; }
        public List<string> import_commodities { get; set; }
        public List<string> export_commodities { get; set; }
        public List<string> prohibited_commodities { get; set; }
        public List<string> economies { get; set; }
        public int? shipyard_updated_at { get; set; }
        public int? outfitting_updated_at { get; set; }
        public int? market_updated_at { get; set; }
        public bool is_planetary { get; set; }
        public List<string> selling_ships { get; set; }
        public List<int> selling_modules { get; set; }
        public object settlement_size_id { get; set; }
        public object settlement_size { get; set; }
        public object settlement_security_id { get; set; }
        public object settlement_security { get; set; }
        public object body_id { get; set; }
        public int? controlling_minor_faction_id { get; set; }

        public static void LoadFromFile()
        {
            // https://eddb.io/archive/v5/stations.json
            IEnumerable<EDStation> stations;
            using (Stream stream = File.Open(@"C:\Users\passp\Documents\Dev\EliteTrader\Data\stations.json", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(stream))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            using (var db = new TradeContext())
            {
                Console.Write("Importing stations");

                var serializer = new JsonSerializer();
                var settings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                stations = serializer.Deserialize<IEnumerable<EDStation>>(reader);

                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.Database.ExecuteSqlCommand("TRUNCATE TABLE [EDStations]");
                int i = 0;
                foreach(var s in stations)
                {
                    i++;
                    db.Stations.Add(s);
                    if (i % 1000 == 0)
                    {
                        db.SaveChanges();
                        Console.Write(".");
                    }
                }
                db.SaveChanges();
                db.Configuration.AutoDetectChangesEnabled = true;
                db.Configuration.ValidateOnSaveEnabled = true;
                Console.WriteLine($"Found {i} stations");
            }
        }
    }
}
