using EliteTrader.Models;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trade
{
    public class RouteFinder
    {
        private class Node
        {
            public EDSystem system { get; set; }
        }

        public float JumpRange { get; set; }

        private EDSystem _start { get; set; }
        private EDSystem _end { get; set; }

        private SimplePriorityQueue<Node> _frontier;
        private Dictionary<string, Node> _cameFrom;
        private Dictionary<string, float> _costSoFar;


        public void Route(string fromSystem, string toSystem)
        {
            _start = EDSystem.Find(fromSystem);
            _end = EDSystem.Find(toSystem);


            var distance = Astrogation.Distance(_start, _end);

            Console.WriteLine($"Looking for path between {_start.name} and {_end.name} ({distance:n2} Ly)...");

            _frontier = new SimplePriorityQueue<Node>();
            _cameFrom = new Dictionary<string, Node>();
            _costSoFar = new Dictionary<string, float>();

            var startNode = new Node() { system = _start };

            _frontier.Enqueue(startNode, 0);
            _costSoFar.Add(startNode.system.name, 0);

            while(_frontier.Count > 0)
            {
                var current = _frontier.Dequeue();

                if (current.system == _end) {
                    Console.WriteLine("Path found.");
                    break;
                }

                var neighbours = current.system.FindInRange(JumpRange).ToList();
                Console.WriteLine($"Found {neighbours.Count()} systems in range of {current.system.name}.");

                foreach(var nextSystem in neighbours)
                {
                    var next = new Node() { system = nextSystem.Value };

                    var new_cost = _costSoFar[current.system.name] + Astrogation.Distance(current.system, next.system);

                    if(_costSoFar.ContainsKey(next.system.name) == false || new_cost < _costSoFar[next.system.name])
                    {
                        Console.WriteLine($"Found a shorter range from {current.system.name} to {next.system.name}: {new_cost:n2} Ly");

                        _costSoFar.Add(next.system.name, new_cost);
                        var priority = new_cost + Astrogation.ManhattanDistance(_end, next.system);

                        _frontier.Enqueue(next, priority);
                        _cameFrom.Add(next.system.name, current);
                    }
                    
                }
            }

            Console.WriteLine("Path found");
            //var step1 = EDSystem.FindInRange(_start, JumpRange);

        }


    }
}
