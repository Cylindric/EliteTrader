using EliteTrader.Models;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trade
{
    public class RouteFinder
    {
        public float JumpRange { get; set; }

        private SimplePriorityQueue<EDSystem> _frontier;
        private Dictionary<string, EDSystem> _cameFrom;
        private Dictionary<string, float> _costSoFar;


        public List<EDSystem> Route(EDSystem start, EDSystem end)
        {
            var swTotal = new System.Diagnostics.Stopwatch();
            var swSort = new System.Diagnostics.Stopwatch();
            var swPriority = new System.Diagnostics.Stopwatch();
            var swNeighbours = new System.Diagnostics.Stopwatch();
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

                    var new_cost = _costSoFar[current.key] + (float)(1 + 0.0001); // Astrogation.Distance(current, next);
                    
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
                        var priority = new_cost + Astrogation.Distance(next, end); //Astrogation.ManhattanDistance(_end, next);
                        swPriority.Stop();

                        _frontier.Enqueue(next, priority);
                        _cameFrom.Add(next.key, current);
                    }
                }
            }

            var path = new List<EDSystem>();
            var c = end;
            while (c != start)
            {
                path.Add(c);
                c = _cameFrom[c.key];
            }
            path.Add(start);
            path.Reverse();

            swTotal.Stop();
            Console.WriteLine($"total:{swTotal.ElapsedMilliseconds}, sort:{swSort.ElapsedMilliseconds}, priority:{swPriority.ElapsedMilliseconds}, neigh:{swNeighbours.ElapsedMilliseconds}");

            return path; 
        }
    }
}
