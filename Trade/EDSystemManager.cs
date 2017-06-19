using CsvHelper;
using EliteTrader.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Trade
{
    [DebuggerDisplay("{X},{Y}")]
    public struct BoxKey
    {
        public int X { get; set; }
        public int Z { get; set; }

        public BoxKey(int x, int y)
        {
            X = x;
            Z = y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is BoxKey))
            {
                return false;
            }
            else
            {
                return X == ((BoxKey)obj).X && Z == ((BoxKey)obj).Z;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                int multi = 486187739;
                hash = hash * multi + X.GetHashCode();
                hash = hash * multi + Z.GetHashCode();
                return hash;
            }
        }
    }

    public sealed class EDSystemManager
    {
        #region Singleton
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
        #endregion

        public string DataPath { get; set; }
        public string RecentDataPath { get; set; }

        private const int ImportBatchSize = 50000;
        private const int BoxSize = 2500;

        private ConcurrentDictionary<BoxKey, ConcurrentDictionary<string, EDSystem>> _BoxedList = new ConcurrentDictionary<BoxKey, ConcurrentDictionary<string, EDSystem>>();
        private static object _boxLock = new object();

        public ConcurrentDictionary<string, EDSystem> Systems {
            get
            {
                var systems = new ConcurrentDictionary<string, EDSystem>();
                foreach(var b in _BoxedList)
                {
                    foreach(var s in b.Value)
                    {
                        systems[s.Key] = s.Value;
                    }
                }
                return systems;
            }
        }

        public void Update(bool includeLatest = true)
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
            if (includeLatest)
            {
                if (dataLastUpdated < DateTime.Now.AddMinutes(-10) && (!File.Exists(RecentDataPath) || File.GetLastWriteTime(RecentDataPath) < DateTime.Now.AddMinutes(-10)))
                {
                    DownloadData(true);
                }
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

            var sourceFile = @"https://eddb.io/archive/v5/systems.csv";
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
            Console.WriteLine($" found {totalSystemCount:n0} systems to load...");


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


                foreach(var sys in systems)
                {
                    var t = DateTime.Now;

                    sys.box = new BoxKey((int)sys.x / BoxSize, (int)sys.z / BoxSize);
                    if (!_BoxedList.ContainsKey(sys.box))
                    {
                        _BoxedList[sys.box] = new ConcurrentDictionary<string, EDSystem>();
                    }
                    _BoxedList[sys.box][sys.key] = sys;


                    if (i > 0 && i % ImportBatchSize == 0)
                    {
                        var batchTime = DateTime.Now - batchStart;
                        Console.WriteLine($"{i:n0} ({((float)i / totalSystemCount):P1}) {(ImportBatchSize / batchTime.TotalSeconds):n0}/s ({ImportBatchSize:n0} took {batchTime.TotalSeconds:n1} seconds)");
                        batchStart = DateTime.Now;
                    }

                    i++;
                };

                ts.Stop();
                Console.WriteLine($"Loaded {i:n0} systems in {ts.Elapsed.ToString()}.");
            }

        }

        public EDSystem Find(string name)
        {
            EDSystem ret = null;

            Parallel.ForEach(_BoxedList, (b, state) =>
            {
                if (b.Value.ContainsKey(name.ToLower()))
                {
                    ret = b.Value[name.ToLower()];
                    state.Break();
                }
            });

            return ret;
        }

        public IEnumerable<KeyValuePair<string, EDSystem>> FindInRange(EDSystem origin, double range)
        {
            var boxRange = ((int)range / BoxSize) + 1;

            var boxes = new List<BoxKey>();
            for(int x = origin.box.X - boxRange; x <= origin.box.X + boxRange; x++)
            {
                for (int z = origin.box.Z - boxRange; z <= origin.box.Z + boxRange; z++)
                {
                    var key = new BoxKey(x, z);
                    if (_BoxedList.ContainsKey(key))
                    boxes.Add(key);
                }
            }

            var systems = new ConcurrentDictionary<string, EDSystem>();
            Parallel.ForEach(boxes, (box) =>
            {
                foreach (var system in _BoxedList[box].Where(x => x.Value != origin && Math.Abs(x.Value.x - origin.x) <= range && Math.Abs(x.Value.y - origin.y) <= range && Math.Abs(x.Value.z - origin.z) <= range).ToList())
                {
                    if (Astrogation.Distance(origin, system.Value) <= range)
                    {
                        systems[system.Key] = system.Value;
                    }
                }
            });
            return systems;

            // 24980ms
            // var systems = _MasterSystemList.Where(x => x.Value != origin && Math.Abs(x.Value.x - origin.x) <= range && Math.Abs(x.Value.y - origin.y) <= range && Math.Abs(x.Value.z - origin.z) <= range).ToList();
            // return systems.Where(x => Astrogation.Distance(origin, x.Value) <= range).ToList();

            // 48327ms
            // var systems = _MasterSystemList.Where(x => x.Value != origin && Astrogation.Distance(origin, x.Value) <= range).ToList();
            // return systems;
        }
    }
}
