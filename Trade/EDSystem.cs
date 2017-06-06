using System;
using System.Collections.Generic;
using CsvHelper;
using System.IO;
using Trade;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EliteTrader
{
    public class EDSystem
    {
        [Key]
        public int? id { get; set; }

        public int? edsm_id { get; set; }

        [Index()]
        [MaxLength(100)]
        public string name { get; set; }

        public float? x { get; set; }
        public float? y { get; set; }
        public float? z { get; set; }
        public long? population { get; set; }
        public int? is_populated { get; set; }
        public int? government_id { get; set; }
        public string government { get; set; }
        public int? allegiance_id { get; set; }
        public string allegiance { get; set; }
        public int? state_id { get; set; }
        public string state { get; set; }
        public int? security_id { get; set; }
        public string security { get; set; }
        public int? primary_economy_id { get; set; }
        public string primary_economy { get; set; }
        public string power { get; set; }
        public string power_state { get; set; }
        public int? power_state_id { get; set; }
        public bool needs_permit { get; set; }
        public int? updated_at { get; set; }
        public string simbad_ref { get; set; }
        public int? controlling_minor_faction_id { get; set; }
        public string controlling_minor_faction { get; set; }
        public int? reserve_type_id { get; set; }
        public string reserve_type { get; set; }

        public static void LoadFromFile()
        {
            // https://eddb.io/archive/v5/systems.csv
            IEnumerable<EDSystem> systems;
            using (Stream stream = File.Open(@"C:\Users\passp\Documents\Dev\EliteTrader\Data\systems.csv", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(stream))
            using (CsvReader csv = new CsvReader(streamReader))
            using (var db = new TradeContext())
            {
                Console.Write("Importing systems");
                var start = DateTime.Now;

                systems = csv.GetRecords<EDSystem>();
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.Database.ExecuteSqlCommand("TRUNCATE TABLE [EDSystems]");
                int i = 0;
                foreach (var s in systems)
                {
                    i++;
                    db.Systems.Add(s);
                    if (i % 1000 == 0)
                    {
                        db.SaveChanges();
                        Console.Write(".");
                    }
                }
                db.SaveChanges();
                db.Configuration.AutoDetectChangesEnabled = true;
                db.Configuration.ValidateOnSaveEnabled = true;
                var duration = DateTime.Now - start;
                Console.WriteLine($"Found {i} systems in {duration.ToString()}");
            }

        }
    }
}
