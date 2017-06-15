﻿using System;
using System.Collections.Generic;
using CsvHelper;
using System.IO;
using Trade;
using System.Net;
using System.Linq;

namespace EliteTrader.Models
{
    public class EDSystem
    {
        public int? id { get; set; }
        //public int? edsm_id { get; set; }
        public string name { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public long? population { get; set; }
        //public int? is_populated { get; set; }
        //public int? government_id { get; set; }
        ////public string government { get; set; }
        //public int? allegiance_id { get; set; }
        //public string allegiance { get; set; }
        //public int? state_id { get; set; }
        //public string state { get; set; }
        //public int? security_id { get; set; }
        //public string security { get; set; }
        //public int? primary_economy_id { get; set; }
        //public string primary_economy { get; set; }
        //public string power { get; set; }
        //public string power_state { get; set; }
        //public int? power_state_id { get; set; }
        public bool needs_permit { get; set; }
        public int? updated_at { get; set; }
        //public string simbad_ref { get; set; }
        //public int? controlling_minor_faction_id { get; set; }
        //public string controlling_minor_faction { get; set; }
        //public int? reserve_type_id { get; set; }
        //public string reserve_type { get; set; }


        private static string DataPath;
        private const int IMPORT_BATCH_SIZE = 50000;

        private static Dictionary<string, EDSystem> _MasterSystemList = new Dictionary<string, EDSystem>();

        static EDSystem()
        {
            DataPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "systems.csv");
        }

        public static void Update()
        {
            DateTime lastUpdate = DateTime.MinValue;
            
            if(lastUpdate < DateTime.Now.AddDays(-2))
            {
                LoadFromFile();
            }
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

            var totalSystemCount = File.ReadLines(DataPath).Count();

            IEnumerable<EDSystem> systems;

            using (Stream stream = File.Open(DataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(stream))
            using (CsvReader csv = new CsvReader(streamReader))
            {
                Console.WriteLine($"Importing {totalSystemCount:n0} systems");
                var start = DateTime.Now;

                systems = csv.GetRecords<EDSystem>();

                DateTime batchStart = DateTime.Now;
                int i = 0;

                // Stats
                var timeCreatingContext = new TimeSpan(0);
                var timePickingRecords = new TimeSpan(0);
                var timeSavingData = new TimeSpan(0);


                foreach(var sys in systems)
                {
                    var t = DateTime.Now;

                    try
                    {
                        _MasterSystemList.Add(sys.name, sys);
                    }
                    catch (ArgumentException)
                    {
                        // system already exists
                        if (sys.updated_at > _MasterSystemList[sys.name].updated_at)
                        {
                            Console.WriteLine($"Updating {sys.name} with more recent data");
                            _MasterSystemList[sys.name] = sys;
                        }
                    }

                    if (i % IMPORT_BATCH_SIZE == 0)
                    {
                        var batchTime = DateTime.Now - batchStart;
                        Console.WriteLine($"{i:n0} ({((float)i/totalSystemCount):P1}) {(IMPORT_BATCH_SIZE / batchTime.TotalSeconds):n0}/s ({IMPORT_BATCH_SIZE} took {batchTime.TotalSeconds:n1} seconds)");
                        batchStart = DateTime.Now;
                    }

                    i++;
                }

                var duration = DateTime.Now - start;
                Console.WriteLine($"Found {i:n0} systems in {duration.ToString()}");
            }

        }
        
        public static EDSystem Find(string name)
        {
            return _MasterSystemList[name];
        }

        public IEnumerable<KeyValuePair<string, EDSystem>> FindInRange(double range)
        {
            // straight in with an accurate range
            var systems = _MasterSystemList.Where(x => Astrogation.Distance(this, x.Value) <= range).ToList();
            return systems;
        }
    }
}
