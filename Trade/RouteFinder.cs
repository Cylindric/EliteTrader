using EliteTrader.Models;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Trade
{
    public class RouteFinder
    {
        [DebuggerDisplay("{Start} to {End} in {JumpRange}Ly jumps")]
        private struct CacheKey {
            public float JumpRange { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
        }

        [DebuggerDisplay("{System} ({Distance}LY)")]
        private struct RouteNode
        {
            public EDSystem System { get; set; }
            public float Distance { get; set; }
            internal float Priority { get; set; }
        }

        public bool LastResultWasFromCache { get; set; } = false;
        public bool AcceptPartialRoutes { get; set; } = false;
        public bool DebugDumpRouteGraphs { get; set; } = false;

        private float _jumpRange;
        public float JumpRange
        {
            get
            {
                return _jumpRange;
            }
            set
            {
                _jumpRange = value;
            }
        }

        private SimplePriorityQueue<EDSystem> _frontier;
        private Dictionary<string, RouteNode> _cameFrom;
        private Dictionary<string, RouteNode> _costSoFar;

        private static Dictionary<CacheKey, List<EDSystem>> _routeCache = new Dictionary<CacheKey, List<EDSystem>>();

        public static void ClearCache()
        {
            _routeCache = new Dictionary<CacheKey, List<EDSystem>>();
        }

        public List<EDSystem> Route(EDSystem start, EDSystem end)
        {
            if(JumpRange == 0)
            {
                throw new InvalidOperationException("Jump range has not been set.");
            }

            var key = new CacheKey() { JumpRange = JumpRange, Start = start.key, End = end.key };
            if (_routeCache.ContainsKey(key))
            {
                Console.WriteLine($"Found a cached route from {start.name} to {end.name} in {JumpRange:n2} Ly jumps.");
                LastResultWasFromCache = true;
                return _routeCache[key];
            }
            else
            {
                LastResultWasFromCache = false;
            }


            var swTotal = new Stopwatch();
            var swSort = new Stopwatch();
            var swPriority = new Stopwatch();
            var swNeighbours = new Stopwatch();
            swTotal.Start();

            var edsm = EDSystemManager.Instance;

            _frontier = new SimplePriorityQueue<EDSystem>();
            _cameFrom = new Dictionary<string, RouteNode>();
            _costSoFar = new Dictionary<string, RouteNode>();

            _frontier.Enqueue(start, 0);
            _costSoFar.Add(start.key, new RouteNode() { System = start, Distance = 0, Priority = 0 });

            var routeFound = false;

            while (_frontier.Count > 0)
            {
                var current = _frontier.Dequeue();

                if (current == end) {
                    routeFound = true;
                    break;
                }

                swNeighbours.Start();
                var neighbours = edsm.FindInRange(current, JumpRange).ToList();
                swNeighbours.Stop();

                // sort neighbours to put those closest to the target first
                swSort.Start();
                neighbours.OrderBy(x => Astrogation.Distance(x.Value, end));
                swSort.Stop();

                foreach(var kvNext in neighbours)
                {
                    var next = kvNext.Value;

                    var new_cost = _costSoFar[current.key].Priority + 1;
                    
                    if (_costSoFar.ContainsKey(next.key) == false || new_cost < _costSoFar[next.key].Priority)
                    {
                        if (_costSoFar.ContainsKey(next.key))
                        {
                            var cost = _costSoFar[next.key];
                            cost.Priority = new_cost;
                        }
                        else
                        {
                            _costSoFar.Add(next.key, new RouteNode() { System = next, Priority = new_cost });
                        }

                        var node = new RouteNode() { System = current, Distance = Astrogation.Distance(current, next) };

                        swPriority.Start();
                        var priority = new_cost + Astrogation.Distance(next, end);
                        swPriority.Stop();

                        _frontier.Enqueue(next, priority);

                        _cameFrom[next.key] = node;
                    }
                }
            }

            if(DebugDumpRouteGraphs) DumpSearchSpaceGraph(start, end, _cameFrom, _costSoFar);

            var path = new List<EDSystem>();
            if (routeFound || AcceptPartialRoutes)
            {
                var c = end;
                while (c != start)
                {
                    path.Add(c);
                    if (_cameFrom.ContainsKey(c.key) == false)
                    {
                        // This should only happen if there is no route to the requested destination.
                        break;
                    }
                    c = _cameFrom[c.key].System;
                }
                path.Reverse();
            }

            _routeCache.Add(key, path);

            swTotal.Stop();
            // Console.WriteLine($"total:{swTotal.ElapsedMilliseconds}, sort:{swSort.ElapsedMilliseconds}, priority:{swPriority.ElapsedMilliseconds}, neigh:{swNeighbours.ElapsedMilliseconds}");

            return path; 
        }

        private void DumpSearchSpaceGraph(EDSystem start, EDSystem end, Dictionary<string, RouteNode> cameFromList, Dictionary<string, RouteNode> costSoFarList)
        {
            var g = new StringBuilder();

            g.AppendLine("digraph G {");

            foreach(var system in cameFromList.Select(l => l.Key).Union(costSoFarList.Select(l => l.Key).Distinct()))
            {
                var inCameFrom = cameFromList.ContainsKey(system);
                var inCostList = costSoFarList.ContainsKey(system);

                var style = inCameFrom ? "style=bold," : "style=dotted,"; // Dotted outline means "not in the came-from list"!
                var shape = inCostList ? "shape=oval," : "shape=box,"; // Rectangular box means "not in the cost list"!

                var colour = "color=\"black\"";
                if (system == start.key)
                {
                    colour = "style=filled,fillcolor=\"green\"";
                }
                else if (system == end.key)
                {
                    colour = "style=filled,fillcolor=\"red\"";
                }

                g.Append($"{ToDotSafeValue(system)} [label=\"{costSoFarList[system].System.name}\",");
                g.Append(style);
                g.Append(shape);
                g.Append(colour);
                //if(system == start.key || system == end.key)
                //{
                //    g.Append(",shape=box");
                //}
                g.AppendLine("];");
            }
            g.AppendLine();

            foreach (var system in cameFromList)
            {
                g.Append($"{ToDotSafeValue(system.Value.System.key)} -> {ToDotSafeValue(system.Key)}");
                if (costSoFarList.ContainsKey(system.Key))
                {
                    g.Append($" [label=\"{system.Value.Distance:n0}LY\"]");
                }
                g.AppendLine(";");
            }

            g.AppendLine("}");

            var graphPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "graphs");
            if (!Directory.Exists(graphPath))
            {
                Directory.CreateDirectory(graphPath);
            }
            File.WriteAllText(Path.Combine(graphPath, $"{start.key} to {end.key}.dv"), g.ToString());
        }

        private string ToDotSafeValue(string input)
        {
            return "sys_" + input
                .Replace(' ', '_')
                .Replace('-', '_')
                .Replace('+', '_');
        }
    }
}
