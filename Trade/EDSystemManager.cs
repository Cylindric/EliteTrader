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

    public sealed class EDSystemManager
    {
        #region Singleton
        private static volatile EDSystemManager instance;
        private static object syncRoot = new object();

        private EDSystemManager() {
            DataPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "systems.csv");
            RecentDataPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "systems_recently.csv");
            EDSystem.BoxSize = BoxSize;
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
        public static int BoxSize = 2500;

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

        /// <summary>
        /// Find the system with the specified name.
        /// </summary>
        /// <param name="name">The name of the system to search for.</param>
        /// <returns>The matching system, or null if none found.</returns>
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


        /// <summary>
        /// Find all Systems within range of the specified system.
        /// </summary>
        /// <param name="origin">The reference system.</param>
        /// <param name="range">The range to include.</param>
        /// <returns>A list of all Systems that are within range of the origin.</returns>
        public IEnumerable<KeyValuePair<string, EDSystem>> FindInRange(EDSystem origin, double range)
        {
            // We only need to consider systems within the same box as the origin, and neighbouring boxes up to the jump range away.
            // One possible optimisation here is to calculate the boxes within the spherical range, but I suspect that for all but the
            // longest routes, the constant circle-maths would cost more than it gains. Something to try out sometime.

            // Find out how many boxes away from the origin we need to inspect.
            var boxRange = ((int)range / BoxSize) + 1;

            // Build up a list of boxes that might contain reachable systems. These are the boxes we'll be searching later, so the smaller
            // we can make this list the better, and the fewer the number of systems in the boxes the better too.
            var boxes = new List<BoxKey>();
            for(int x = origin.box.X - boxRange; x <= origin.box.X + boxRange; x++)
            {
                for (int z = origin.box.Z - boxRange; z <= origin.box.Z + boxRange; z++)
                {
                    var key = new BoxKey(x, z);
                    if (_BoxedList.ContainsKey(key))
                    {
                        boxes.Add(key);
                    }
                }
            }

            // Spin through every box in our search-space, and add any systems that are actually within range to the result list.
            // This can be done concurrently, because each system is independent.
            var systems = new ConcurrentDictionary<string, EDSystem>();
            Parallel.ForEach(boxes, (box) =>
            {
                // We're doing a faster box-based range check first, so we don't have to do an expensive √((x2-x1)² + (y2-y1)² + (z2-z1)²) calculation.
                foreach (var system in _BoxedList[box].Where(x => x.Value != origin && Math.Abs(x.Value.x - origin.x) <= range && Math.Abs(x.Value.y - origin.y) <= range && Math.Abs(x.Value.z - origin.z) <= range).ToList())
                {
                    // Now we have a list of systems that have their X/Y/Z coordinates all within ±jump of the origin, find out which really are in range.
                    // (Because the corners of the cube will be further from the centre than 1 jump-range)
                    if (Astrogation.Distance(origin, system.Value) <= range)
                    {
                        systems[system.Key] = system.Value;
                    }
                }
            });

            // We now should have a simple list of Systems within the specified range of the provided origin system.
            return systems;
        }
    }
}
