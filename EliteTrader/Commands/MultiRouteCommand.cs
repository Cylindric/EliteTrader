using EliteTrader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trade;

namespace EliteTrader.Commands
{
    public class MultiRouteCommand : ICommand
    {
        private string _start { get; set; }
        private List<List<string>> _routes;
        private float _range { get; set; }

        /*
         * multiroute jump=30 start=olgrea to="Te Kaha,Cao Junga" to="Carnsan,MCC 105"
         */

        public MultiRouteCommand(ConsoleCommand cmd)
        {
            _routes = new List<List<string>>();

            if (cmd.Arguments.Count() == 1)
            {
                if (cmd.Arguments.First().Value == "test")
                {
                    _start = "olgrea";
                    var r1 = "te kaha,cao junga".Split(',').ToList();
                    var r2 = "carnsan,HIP 13179".Split(',').ToList();
                    _routes.Add(r1);
                    _routes.Add(r2);
                    _range = 30;
                }
            }


            foreach (var opt in cmd.Arguments)
            {
                switch (opt.Key)
                {
                    case "start":
                    case "from":
                        _start = opt.Value;
                        break;

                    case "route":
                        _routes.Add(opt.Value.Split(',').ToList());
                        break;

                    case "jump":
                    case "jmp":
                        _range = (float)Convert.ToDouble(opt.Value);
                        break;
                }
            }
        }

        public int Execute()
        {
            if (string.IsNullOrEmpty(_start))
            {
                throw new InvalidOperationException("_start has not been set");
            }
            if(_routes.Count() == 0)
            {
                throw new InvalidOperationException("No routes have been set");
            }
            if(_range <= 0)
            {
                throw new InvalidOperationException("No jump-range has been set");
            }

            // Build a list of routes with actual systems in them.
            var sw = new Stopwatch();
            sw.Start();
            var routes = new List<Queue<EDSystem>>();
            foreach (var route in _routes)
            {
                var r = new Queue<EDSystem>();
                foreach (var sys in route)
                {
                    r.Enqueue(EDSystemManager.Instance.Find(sys));
                }
                routes.Add(r);
            }

            var mrf = new MultiRouteFinder();
            mrf.JumpRange = _range;
            var start = EDSystemManager.Instance.Find(_start);
            var finalRoute = mrf.Route(start, routes);

            // var finalRoute = ClosestNeighbourRoute(routes);
            sw.Stop();

            Console.WriteLine($"Path found jumps. Took {sw.Elapsed.ToString()}");
            var i = 0;
            EDSystem previousSystem = null;
            while (finalRoute.Count() > 0)
            {
                var system = finalRoute.Dequeue();
                float jumpDistance = -1;

                if(previousSystem != null)
                {
                    jumpDistance = Astrogation.Distance(previousSystem, system);
                }

                Console.Write($"{i:n0} {system.name}");
                if(jumpDistance >= 0)
                {
                    Console.Write($" ({jumpDistance:n1} LY)");
                }
                Console.WriteLine();

                i++;
                previousSystem = system;
            }


            //// Pick the starting route based on closest-first-stop
            //var router = new RouteFinder();
            //router.JumpRange = _range;

            //Queue<EDSystem> first;
            //int distance = int.MaxValue;
            //foreach(var route in routes)
            //{
            //    var p = router.Route(start, route.Peek());
            //    if(p.Count() < distance)
            //    {
            //        distance = p.Count();
            //        first = route;
            //    }
            //}

            //throw new NotImplementedException();
            return 0;
        }


        //private void UpdateMap(EDSystem start, List<Queue<EDSystem>> routes)
        //{
        //    var nodes = new List<Node>();

        //    var startNode = new Node();
        //    startNode.System = start;
        //}

        //private void UpdateConnections(Node node, List<Queue<EDSystem>> routes)
        //{
        //    // create a copy of the queues
        //    var newRoutes = new List<Queue<EDSystem>>();
        //    foreach(var r in routes)
        //    {
        //        var q = new Queue<EDSystem>();
        //        while (r.Count() > 0)
        //        {
        //            q.Enqueue(r.Dequeue());
        //        }
        //        newRoutes.Add(q);
        //    }

        //    foreach(var route in newRoutes)
        //    {
        //        var nextStop = route.Dequeue();
        //        node.ConnectedSystems.Add(nextStop, 0);
        //    }
        //}

        //private class Node
        //{
        //    public EDSystem System { get; set; }
        //    public Dictionary<EDSystem, int> ConnectedSystems { get; set; }

        //    public Node()
        //    {
        //        ConnectedSystems = new Dictionary<EDSystem, int>();
        //    }
        //}
    }

}
