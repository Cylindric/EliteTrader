using CsvHelper;
using EliteTrader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Trade
{
    public sealed class EDSystemManager
    {
        private static volatile EDSystemManager instance;
        private static object syncRoot = new object();

        private EDSystemManager() {
            DataPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "systems.csv");
        }

        public static EDSystemManager Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (syncRoot)
                    {
                        if(instance == null)
                        {
                            instance = new EDSystemManager();
                        }
                    }
                }
                return instance;
            }
        }



        public string DataPath { get; set; }

        private const int IMPORT_BATCH_SIZE = 50000;

        private Dictionary<string, EDSystem> _MasterSystemList = new Dictionary<string, EDSystem>();

        public void Update()
        {
            DateTime lastUpdate = DateTime.MinValue;

            if (lastUpdate < DateTime.Now.AddDays(-2))
            {
                LoadFromFile();
            }
        }

        public void DownloadData()
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

        public void LoadFromFile()
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


                foreach (var sys in systems)
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
                        Console.WriteLine($"{i:n0} ({((float)i / totalSystemCount):P1}) {(IMPORT_BATCH_SIZE / batchTime.TotalSeconds):n0}/s ({IMPORT_BATCH_SIZE} took {batchTime.TotalSeconds:n1} seconds)");
                        batchStart = DateTime.Now;
                    }

                    i++;
                }

                var duration = DateTime.Now - start;
                Console.WriteLine($"Found {i:n0} systems in {duration.ToString()}");
            }

        }

        public EDSystem Find(string name)
        {
            return _MasterSystemList[name];
        }

        public IEnumerable<KeyValuePair<string, EDSystem>> FindInRange(EDSystem origin, double range)
        {
            var systems = _MasterSystemList.Where(x => x.Value != origin && Astrogation.Distance(origin, x.Value) <= range).ToList();
            return systems;
        }
    }
}
