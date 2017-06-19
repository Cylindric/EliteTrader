using EliteTrader.Models;
using System.Collections.Generic;
using System.Linq;

namespace Trade
{
    public class MultiRouteFinder
    {
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

        public Queue<EDSystem> Route(EDSystem start, List<Queue<EDSystem>> routes)
        {
            return ClosestNeighbourRoute(start, routes);
        }

        /// <summary>
        /// This method utilises a "closest neighbour" approach to route finding, always suggesting the next-closest
        /// available destination.
        /// </summary>
        /// <param name="start">The start system.</param>
        /// <param name="routes">A list of routes.</param>
        /// <returns></returns>
        private Queue<EDSystem> ClosestNeighbourRoute(EDSystem start, List<Queue<EDSystem>> routes)
        {
            var final = new Queue<EDSystem>();
            var nextSystem = start;

            final.Enqueue(nextSystem);

            int activeRoutes = routes.Count();
            while (activeRoutes != 0)
            {
                if (activeRoutes == 1)
                {
                    // There's only one route with systems left in it, so don't bother finding the closest, just spin through them.
                    foreach (var route in routes.Where(r => r.Count() > 0))
                    {
                        while (route.Count() > 0)
                        {
                            final.Enqueue(route.Dequeue());
                        }
                        activeRoutes--;
                    }
                }
                else
                {
                    // There are multiple active routes, so we need to find the one with the closest next system in it.
                    var closestSystem = GetClosestSystemInRoutes(nextSystem, routes);

                    foreach (var route in routes)
                    {
                        if (route.Peek().key == closestSystem.key)
                        {
                            route.Dequeue();
                        }

                        // If this route is now empty, decrease the number of active routes.
                        if (route.Count() == 0)
                        {
                            activeRoutes--;
                        }
                    }

                    nextSystem = closestSystem;
                    final.Enqueue(closestSystem);
                }

            }

            // Do we want to return to the start? Probably I guess, for Passengers
            // TODO: Add config option for "return to start"
            final.Enqueue(start);

            return final;
        }

        private EDSystem GetClosestSystemInRoutes(EDSystem current, List<Queue<EDSystem>> routes)
        {
            int clostestJumps = int.MaxValue;
            EDSystem closestSystem = null;

            var router = new RouteFinder();
            router.JumpRange = JumpRange;

            foreach (var route in routes)
            {
                var j = router.Route(current, route.Peek());
                if (j.Count() < clostestJumps)
                {
                    clostestJumps = j.Count();
                    closestSystem = route.Peek();
                }
            }

            return closestSystem;
        }

    }
}
