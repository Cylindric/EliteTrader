using System;
using System.Collections.Generic;
using CsvHelper;
using System.IO;
using System.Net;
using System.Linq;
using CsvHelper.Configuration;
using Trade.Maps;
using LiteDB;

namespace EliteTrader.Models
{
    public class EDSystem
    {
        public Guid gid { get; set; }
        public int id { get; set; }
        public int edsm_id { get; set; }

        [BsonIndex()]
        public string name { get; set; }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public long population { get; set; }
        public int is_populated { get; set; }
        public int government_id { get; set; }
        public int allegiance_id { get; set; }
        public int state_id { get; set; }
        public int security_id { get; set; }
        public int primary_economy_id { get; set; }
        public int power_state_id { get; set; }
        public bool needs_permit { get; set; }
        public int updated_at { get; set; }
        public int controlling_minor_faction_id { get; set; }
        public int reserve_type_id { get; set; }

        private static string DbFile;
        private static string DataPath;
        private const int IMPORT_BATCH_SIZE = 50000;

        static EDSystem()
        {
            DataPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "systems.csv");
            DbFile = @"E:\Temp\systems.db";
        }

        public EDSystem()
        {
            gid = Guid.NewGuid();
            //        public class RowSchema
            //        {
            //        }


            //        private static string DataPath;
            //        private const int IMPORT_BATCH_SIZE = 50000;

            //        static EDSystem()
            //        {


        }

        public static void Update()
        {
            //            DateTime lastUpdate = DateTime.MinValue;

            //            using (var db = new TradeContext())
            //            {
            //                var settings = db.Settings.FirstOrDefault();
            //                if (settings != null && settings.LastSystemUpdate.HasValue) {
            //                    lastUpdate = settings.LastSystemUpdate.Value;
            //                }
            //            }

            //            if(lastUpdate < DateTime.Now.AddDays(-2))
            //            {
            //                DownloadData();
            LoadFromFile();
            //            }

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

            var totalSystemCount = File.ReadLines(DataPath).Count();

            var config = new CsvConfiguration();
            config.WillThrowOnMissingField = false;
            config.RegisterClassMap<EDSystemMap>();

            IEnumerable<EDSystem> newSystems;

            // Initial import assumes a clean System list
            if (File.Exists(DbFile))
            {
                File.Delete(DbFile);
            }


            using (Stream stream = File.Open(DataPath, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(stream))
            using (CsvReader csv = new CsvReader(streamReader, config))
            {

                Console.WriteLine($"Importing {totalSystemCount:n0} systems");
                var start = DateTime.Now;

                newSystems = csv.GetRecords<EDSystem>();

                DateTime batchStart = DateTime.Now;
                int i = 0;


                var insertTime = new TimeSpan();
                var indexTime = new TimeSpan();

                var insertBatch = new List<EDSystem>();
                var insertedCount = 0;

                foreach (var sys in newSystems)
                {
                    insertBatch.Add(sys);
                    insertedCount++;

                    if (insertBatch.Count >= IMPORT_BATCH_SIZE)
                    {
                        try
                        {
                            using (var db = new LiteDatabase($"filename={DbFile};journal=false"))
                            {
                                var systems = db.GetCollection<EDSystem>("systems");
                                var t = DateTime.Now;
                                systems.Insert(insertBatch);
                                insertTime += (DateTime.Now - t);

                                t = DateTime.Now;
                                systems.EnsureIndex(x => x.name);
                                indexTime += (DateTime.Now - t);

                                insertBatch.Clear();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                    }

                    if (i > 0 && i % IMPORT_BATCH_SIZE == 0)
                    {
                        var batchTime = DateTime.Now - batchStart;
                        Console.WriteLine($"{i:n0} ({((float)i / totalSystemCount):P1}) {(IMPORT_BATCH_SIZE / batchTime.TotalSeconds):n0}/s ({IMPORT_BATCH_SIZE} took {batchTime.TotalSeconds:n1} seconds); inserterted {insertedCount:n0}. ins:{insertTime}, idx:{indexTime}");
                        batchStart = DateTime.Now;
                    }

                    i++;
                }

                if (insertBatch.Count >= IMPORT_BATCH_SIZE)
                {
                    using (var db = new LiteDatabase($"filename={DbFile};journal=false"))
                    {
                        var systems = db.GetCollection<EDSystem>("systems");
                        systems.Insert(insertBatch);
                        insertBatch.Clear();
                    }
                }

                var duration = DateTime.Now - start;
                Console.WriteLine($"Found {i} systems in {duration.ToString()}");

            }

        }

        public static EDSystem Find(string name)
        {
            using (var db = new LiteDatabase($"filename={DbFile}"))
            {

                var col = db.GetCollection<EDSystem>("systems");
                // col.EnsureIndex("name");
                var ix = col.GetIndexes();
                var results = col.FindOne(x => x.name == name);
                return results;

            }
        }

        //        private static void Truncate()
        //        {
        //            using (var db = new TradeContext())
        //            {
        //                db.Configuration.AutoDetectChangesEnabled = false;
        //                db.Configuration.ValidateOnSaveEnabled = false;
        //                db.Database.ExecuteSqlCommand("TRUNCATE TABLE [EDSystems]");

    }
}