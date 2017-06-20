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

        public bool LastResultWasFromCache { get; set; }

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
        private Dictionary<string, EDSystem> _cameFrom;
        private Dictionary<string, float> _costSoFar;

        private static Dictionary<CacheKey, List<EDSystem>> _routeCache = new Dictionary<CacheKey, List<EDSystem>>();

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
            _cameFrom = new Dictionary<string, EDSystem>();
            _costSoFar = new Dictionary<string, float>();

            _frontier.Enqueue(start, 0);
            _costSoFar.Add(start.key, 0);

            while (_frontier.Count > 0)
            {
                var current = _frontier.Dequeue();

                if (current == end) {
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

                    var new_cost = _costSoFar[current.key] + 1;
                    
                    if (_costSoFar.ContainsKey(next.key) == false || new_cost < _costSoFar[next.key])
                    {
                        if (_costSoFar.ContainsKey(next.key))
                        {
                            _costSoFar[next.key] = new_cost;
                        }
                        else
                        {
                            _costSoFar.Add(next.key, new_cost);
                        }

                        swPriority.Start();
                        var priority = new_cost + Astrogation.Distance(next, end);
                        swPriority.Stop();

                        _frontier.Enqueue(next, priority);

                        // TODO: Not sure why this check needs to be added. The A* implementations 
                        // I've seen always seem to assume the CameFrom list would not already contain the node.
                        //if (_cameFrom.ContainsKey(next.key))
                        //{
                            _cameFrom[next.key] = current;
                        //}
                        //else
                        //{
                        //    _cameFrom.Add(next.key, current);
                        //}
                    }
                }
            }

            DumpSearchSpaceGraph(start, end, _cameFrom, _costSoFar);

            var path = new List<EDSystem>();
            var c = end;
            while (c != start)
            {
                path.Add(c);
                c = _cameFrom[c.key];
            }
            path.Add(start);
            path.Reverse();

            _routeCache.Add(key, path);

            swTotal.Stop();
            // Console.WriteLine($"total:{swTotal.ElapsedMilliseconds}, sort:{swSort.ElapsedMilliseconds}, priority:{swPriority.ElapsedMilliseconds}, neigh:{swNeighbours.ElapsedMilliseconds}");

            return path; 
        }

        private void DumpSearchSpaceGraph(EDSystem start, EDSystem end, Dictionary<string, EDSystem> cameFromList, Dictionary<string, float> costSoFarList)
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

                g.Append($"{ToDotSafeValue(system)} [label=\"{system}\",");
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
                g.Append($"{ToDotSafeValue(system.Value.key)} -> {ToDotSafeValue(system.Key)}");
                if (costSoFarList.ContainsKey(system.Key))
                {
                    g.Append($" [label=\"{costSoFarList[system.Key]:n0}\"]");
                }
                g.AppendLine(";");
            }

            g.AppendLine("}");

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "graphs", "camefrom.dv"), g.ToString());
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
