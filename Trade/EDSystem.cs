using System;
using System.Collections.Generic;
using CsvHelper;
using System.IO;
using Trade;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Data.Entity.Spatial;

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

        [Index("PositionIndex", 1)]
        public float x { get; set; }

        [Index("PositionIndex", 2)]
        public float y { get; set; }

        [Index("PositionIndex", 3)]
        public float z { get; set; }

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

        public DbGeometry location { get; set; }

        private static string DataPath;

        static EDSystem()
        {
            DataPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "systems.csv");
        }

        public static void DownloadData()
        {
            Console.WriteLine("Downloading system data from EDDB...");

            if (File.Exists(DataPath))
            {
                File.Delete(DataPath);
            }

            if (!Directory.Exists(Path.GetDirectoryName(DataPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DataPath));
            }

            var systemsCsv = @"https://eddb.io/archive/v5/systems.csv";
            using (var client = new WebClient())
            {
                client.DownloadFile(systemsCsv, DataPath);
            }
        }

        public static void LoadFromFile()
        {
            if (!File.Exists(DataPath))
            {
                DownloadData();
            }

            // https://eddb.io/archive/v5/systems.csv
            IEnumerable<EDSystem> systems;
            using (Stream stream = File.Open(DataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(stream))
            using (CsvReader csv = new CsvReader(streamReader))
            {
                Console.Write("Importing systems");
                var start = DateTime.Now;

                systems = csv.GetRecords<EDSystem>();

                Truncate();

                TradeContext db = null;
                int i = 0;
                foreach (var s in systems)
                {
                    if(db == null)
                    {
                        db = new TradeContext();
                        db.Configuration.AutoDetectChangesEnabled = false;
                        db.Configuration.ValidateOnSaveEnabled = false;
                    }

                    i++;
                    s.location = DbGeometry.PointFromText($"POINT({s.x} {s.y})", 0);
                    db.Systems.Add(s);
                    if (i % 1000 == 0)
                    {
                        db.SaveChanges();
                        Console.Write(".");
                        db.Dispose();
                        db = null;
                    }
                }

                if(db != null)
                {
                    db.SaveChanges();
                    db.Dispose();
                }

                var duration = DateTime.Now - start;
                Console.WriteLine($"Found {i} systems in {duration.ToString()}");
            }

        }

        private static void Truncate()
        {
            using (var db = new TradeContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;
                db.Database.ExecuteSqlCommand("TRUNCATE TABLE [EDSystems]");
            }
        }
    }
}
