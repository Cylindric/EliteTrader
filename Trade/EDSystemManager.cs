using CsvHelper;
using EliteTrader.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
            RecentDataPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "systems_recently.csv");
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
        public string RecentDataPath { get; set; }

        private const int IMPORT_BATCH_SIZE = 50000;

        private ConcurrentDictionary<string, EDSystem> _MasterSystemList = new ConcurrentDictionary<string, EDSystem>();

        public void Update()
        {
            DateTime lastUpdate = DateTime.MinValue;

            // Remove old data
            if (File.Exists(DataPath) && File.GetLastWriteTime(DataPath) < DateTime.Now.AddDays(-7))
            {
                File.Delete(DataPath);
                if (File.Exists(RecentDataPath))
                {
                    File.Delete(RecentDataPath);
                }
            }

            // Download full data set
            if (!File.Exists(DataPath))
            {
                DownloadData();
            }

            var dataLastUpdated = File.GetLastWriteTime(DataPath);

            // Download latest delta
            if (dataLastUpdated < DateTime.Now.AddMinutes(-10) && (!File.Exists(RecentDataPath) || File.GetLastWriteTime(RecentDataPath) < DateTime.Now.AddMinutes(-10)))
            {
                DownloadData(true);
            }

            // Load the data into memory
            if (File.Exists(DataPath))
            {
                LoadFromFile(DataPath);
            }
            if (File.Exists(RecentDataPath))
            {
                LoadFromFile(RecentDataPath);
            }
        }

        public void DownloadData(bool recent = false)
        {
            Console.WriteLine($"Downloading {(recent ? "recent " : "")}system data from EDDB...");

            var sourceFile = @"https://eddb.io/archive/v5/systems_recently.csv";
            var destination = DataPath;

            if (recent)
            {
                sourceFile = @"https://eddb.io/archive/v5/systems_recently.csv";
                destination = RecentDataPath;
            }

            var t = new Stopwatch();
            t.Start();
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            if (!Directory.Exists(Path.GetDirectoryName(destination)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
            }

            using (var client = new WebClient())
            {
                client.DownloadFile(sourceFile, destination);
            }
            t.Stop();

            Console.WriteLine($"Download took {t.Elapsed.ToString()}");
        }

        public void LoadFromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new InvalidOperationException($"Specified data file does not exist. {filename}.");
            }

            Console.Write($"Loading system data...");
            var ts = new Stopwatch();
            ts.Start();
            var totalSystemCount = File.ReadLines(filename).Count();
            Console.WriteLine($" found {totalSystemCount} systems to load...");


            IEnumerable<EDSystem> systems;

            using (Stream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(stream))
            using (CsvReader csv = new CsvReader(streamReader))
            {
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
                        _MasterSystemList[sys.key] = sys;
                    }
                    catch (ArgumentException)
                    {
                        // system already exists
                        if (sys.updated_at > _MasterSystemList[sys.key].updated_at)
                        {
                            //Console.WriteLine($"Updating {sys.name} with more recent data");
                            _MasterSystemList[sys.key] = sys;
                        }
                    }

                    if (i > 0 && i % IMPORT_BATCH_SIZE == 0)
                    {
                        var batchTime = DateTime.Now - batchStart;
                        Console.WriteLine($"{i:n0} ({((float)i / totalSystemCount):P1}) {(IMPORT_BATCH_SIZE / batchTime.TotalSeconds):n0}/s ({IMPORT_BATCH_SIZE:n0} took {batchTime.TotalSeconds:n1} seconds)");
                        batchStart = DateTime.Now;
                    }

                    i++;
                }

                ts.Stop();
                 Console.WriteLine($"Loaded {i:n0} systems in {ts.Elapsed.ToString()}.");
            }

        }

        public EDSystem Find(string name)
        {
            return _MasterSystemList[name.ToLower()];
        }

        public IEnumerable<KeyValuePair<string, EDSystem>> FindInRange(EDSystem origin, double range)
        {
            var systems = _MasterSystemList.Where(x => x.Value != origin && Astrogation.Distance(origin, x.Value) <= range).ToList();
            return systems;
        }
    }
}
