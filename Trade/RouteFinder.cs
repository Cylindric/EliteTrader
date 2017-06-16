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

        private EDSystem _start { get; set; }
        private EDSystem _end { get; set; }

        private SimplePriorityQueue<EDSystem> _frontier;
        private Dictionary<string, EDSystem> _cameFrom;
        private Dictionary<string, float> _costSoFar;


        public List<EDSystem> Route(string fromSystem, string toSystem)
        {
            var edsm = EDSystemManager.Instance;
            _start = edsm.Find(fromSystem);
            _end = edsm.Find(toSystem);


            var distance = Astrogation.Distance(_start, _end);

            Console.WriteLine($"Looking for path between {_start.name} and {_end.name} ({distance:n2} Ly)...");

            _frontier = new SimplePriorityQueue<EDSystem>();
            _cameFrom = new Dictionary<string, EDSystem>();
            _costSoFar = new Dictionary<string, float>();

            _frontier.Enqueue(_start, 0);
            _costSoFar.Add(_start.name, 0);

            while(_frontier.Count > 0)
            {
                var current = _frontier.Dequeue();

                if (current == _end) {
                    break;
                }

                var neighbours = edsm.FindInRange(current, JumpRange).ToList();

                // sort neighbours to put those closest to the target first
                neighbours.OrderBy(x => Astrogation.Distance(x.Value, _end));

                foreach(var kvNext in neighbours)
                {
                    var next = kvNext.Value;

                    var new_cost = _costSoFar[current.name] + (float)(1 + 0.0001); // Astrogation.Distance(current, next);
                    
                    if (_costSoFar.ContainsKey(next.name) == false || new_cost < _costSoFar[next.name])
                    {
                        if (_costSoFar.ContainsKey(next.name))
                        {
                            _costSoFar[next.name] = new_cost;
                        }
                        else
                        {
                            _costSoFar.Add(next.name, new_cost);
                        }

                        var priority = new_cost + Astrogation.Distance(next, _end); //Astrogation.ManhattanDistance(_end, next);

                        _frontier.Enqueue(next, priority);
                        _cameFrom.Add(next.name, current);
                    }
                }
            }

            var path = new List<EDSystem>();
            var c = _end;
            while (c != _start)
            {
                path.Add(c);
                c = _cameFrom[c.name];
            }
            path.Add(_start);
            path.Reverse();

            return path; 
        }
    }
}
