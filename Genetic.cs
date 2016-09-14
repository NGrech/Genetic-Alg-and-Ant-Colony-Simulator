using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MachineLearning2015
{
    class Genetic
    {
        // Properties

        public List<City> cities { get; set; }
        public Population P { get; set; }
        public double m_rate { get; set; }
        public Tour s_Best { get; set; }
        public List<string> results = new List<string>();


        //Constructor

        public Genetic(List<City> city_list, int pop_size)
        {
            this.cities = city_list;
            m_rate = 0.015;

            P = new Population();
            P.PopInitalize(pop_size, city_list);
            s_Best = P.Fittest();
            Console.WriteLine("Current shortest path length: {0:0.00} at Start", s_Best.distance);
        }


        //Methods

        public List<string> runSimulation(int Max)
        {
            

            for (int i = 0; i < Max; i++)
            {
                NextGeneration();
                Tour genBest = P.Fittest();

                if (s_Best.distance > genBest.distance){
                    s_Best = genBest;
                    results.Add(string.Format("{0},{1}", s_Best.distance, i));
                    Console.WriteLine("Current shortest path length: {0:0.00} on iteration {1}", s_Best.distance, i);
                }
                    

            }
            return results;

        }

        public void NextGeneration()
        {
            Population temp = new Population();


            //creating a new population, the same size as the old one
            //each pair of parents creates 2 children
            for (int i = 0; i < P.tours.Count()/2; i++)
            {
                //selecting the two parents
                Tour P1 = Selection();
                Tour P2 = Selection();

                //creating the children one using greedy crossover the other with edge recombination
                Tour Child_GXO = GreedyXOver(P1, P2);
                Tour Child_ERXO = EdgeRecombinationXOver(P1, P2);

                //mutating the children
                Child_GXO = mutate(Child_GXO);
                Child_ERXO = mutate(Child_ERXO);

                //calculating the distance and fitness of the children
                Child_GXO.calc_D_F(cities);
                Child_ERXO.calc_D_F(cities);

                //adding the children to the temp population 
                temp.tours.Add(Child_GXO);
                temp.tours.Add(Child_ERXO);
            }

            //joining the children and parent populations into one and selecting the fittest members to make the new population 
            P.tours = temp.tours.Concat(P.tours).OrderByDescending(p => p.fitness).Take(P.tours.Count()).ToList();
        }

        public Tour Selection()
        {
            Population S_pop = new Population();
            S_pop.tours = P.tours.OrderBy(c => Guid.NewGuid()).Take(5).ToList();
            return S_pop.Fittest();
        }

        public Tour mutate(Tour child)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < child.tour.Count(); i++)
            {
                if (rnd.NextDouble() < m_rate)
                {
                    int s_index = rnd.Next(child.tour.Count());
                    int c1 = child.tour[i];
                    child.tour[i] = child.tour[s_index];
                    child.tour[s_index] = c1;
                }
            }

            return child;
        }

        public Tour EdgeRecombinationXOver(Tour P1, Tour P2)
        {
            Tour child = new Tour();
            Random rnd = new Random(DateTime.Now.Millisecond);

            List<HashSet<int>> edgeSets = CreateAdjList(P1.tour, P2.tour);

            List<int> unused_Vertices = new List<int>();

            for (int i = 0; i < cities.Count; i++)
                unused_Vertices.Add(i);

            //randomly picking start node
            int current = rnd.Next(0, cities.Count);
            int sel;
            child.tour = new int[cities.Count];

            child.tour[0] = current;


            for (int i = 1; i < child.tour.Count(); i++)
            {
                cleanList(edgeSets, current);
                unused_Vertices.Remove(current);

                

                if (edgeSets[current].Any())
                {
                    sel = edgeSet_Select(edgeSets[current]);
                    
                }
                else
                {
                    sel = unused_Vertices[rnd.Next(unused_Vertices.Count())];
                }
                child.tour[i] = sel;
                current = sel;
            }

            
            return child;
        }

        public int edgeSet_Select(HashSet<int> set)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int[] selctable = set.ToArray();
            return selctable[rnd.Next(selctable.Count())];
        }
        public void cleanList(List<HashSet<int>> adjList, int node)
        {
            foreach (HashSet<int> alist in adjList)
            {
                if (alist.Contains(node))
                    alist.Remove(node);
            }
        }

        public List<HashSet<int>> CreateAdjList(int[] P1, int[] P2)
        {
            //initializing adjacency list
            List<HashSet<int>> adjlist = new List<HashSet<int>>();
            for (int i = 0; i < cities.Count; i++)
                adjlist.Add(new HashSet<int>());

            //adding parent 1's edges
            adjlist[P1[0]].Add(P1[P1.Count() -1]);
            adjlist[P1[0]].Add(P1[1]);
            adjlist[P1[P1.Count() - 1]].Add(P1[0]);
            adjlist[P1[P1.Count() - 1]].Add(P1[P1.Count() - 2]);

            for (int i = 1; i < P1.Count() - 1; i++)
            {
                adjlist[P1[i]].Add(P1[i + 1]);
                adjlist[P1[i]].Add(P1[i - 1]);
            }

            //adding parent 2's edges
            adjlist[P2[0]].Add(P2[P2.Count() - 1]);
            adjlist[P2[0]].Add(P2[1]);
            adjlist[P2[P2.Count() - 1]].Add(P2[0]);
            adjlist[P2[P2.Count() - 1]].Add(P2[P1.Count() - 2]);

            for (int i = 1; i < P1.Count() - 1; i++)
            {
                adjlist[P2[i]].Add(P2[i + 1]);
                adjlist[P2[i]].Add(P2[i - 1]);
            }

            return adjlist;
        }

        public Tour GreedyXOver(Tour P1, Tour P2)
        {
            Tour child = new Tour();
            child.tour = new int[cities.Count];
            List<int> c_temp = new List<int>();


            Random rnd = new Random(DateTime.Now.Millisecond);

            //randomly picking start node
            int current  = rnd.Next(0, cities.Count);

            //assigning start node to child
            c_temp.Add(current);


            for (int i = 1; i < child.tour.Count(); i++)
            {
                //getting the cities adjacent to the current node
                List<int> adj_cities = getAdj(P1.tour, P2.tour, current).ToList();

                //getting the one with the minimum distance
                int next = getMin(adj_cities.ToArray(), current);

                //checking if the selected node already exists in the child
                if (c_temp.Contains(next))
                {
                    //making a new random selection from the remaining possibilities 
                    next = selectRan(adj_cities, c_temp);
                    c_temp.Add(next);
                }
                else
                {
                    c_temp.Add(next);
                }

                current = next;
            }

            child.tour = c_temp.ToArray();
            return child;
            
        }

        public int selectRan(List<int> poss, List<int> exclude)
        {
            int[] remaining = poss.Except(exclude).ToArray();

            Random rnd = new Random(DateTime.Now.Millisecond);

            if (remaining.Length > 0)
            {
                
                return remaining[rnd.Next(remaining.Count())];
            }
            else
            {
                List<int> ret = new List<int>();
                for (int i = 0; i < cities.Count; i++)
                    if (!exclude.Contains(i))
                        ret.Add(i);

                return ret[0];

            }

        }

        public int getMin(int[] adj, int current)
        {

            return adj.OrderBy(a => cities[current].edges[a]).First();

        }

        public int[] getAdj(int[] P1, int[] P2, int fn)
        {
            HashSet<int> ulist = new HashSet<int>();
            
            ulist.Add(cityL(P1, fn));
            ulist.Add(cityR(P1, fn));
            ulist.Add(cityL(P2, fn));
            ulist.Add(cityR(P2, fn));

            return ulist.ToArray();

        }

        public int cityL(int[] tour, int current)
        {
            int point = Array.IndexOf(tour, current);
            int leftindex = (point -1)% tour.Length;
            if (leftindex < 0)
            {
                return tour[tour.Length - 1];
            }
            return tour[leftindex];
        }

        public int cityR(int[] tour, int current)
        {
            int point = Array.IndexOf(tour, current);
            int rightindex = (point + 1) % tour.Length;
            return tour[rightindex];
        }
    }


    public class Population
    {
        // Properties

        public List<Tour> tours { get; set; }


        //Constructor

        public Population()
        {
            tours = new List<Tour>();
        }

        // Methods

        public void PopInitalize(int size, List<City> cities)
        {
            for (int i = 0; i < size; i++)
            {
                Tour t = new Tour();
                t.RandomTour(cities.Count);
                t.calc_D_F(cities);
                tours.Add(t);
            }
        }

        public Tour Fittest()
        {
            return tours.OrderByDescending(t => t.fitness).Last();
        }

    }


    public class Tour
    {
        // Properties
        public int[] tour { get; set; }

        public double fitness { get; set; }
        public double distance { get; set; }

        // Methods


        public void RandomTour(int size)
        {
            tour = new int[size];

            for (int i = 0; i < size; i++)
                tour[i] = i;

            tour = tour.OrderBy(c => Guid.NewGuid()).ToArray();
        }
        public void calc_D_F(List<City> cities)
        {
            for (int i = 0; i < tour.Length - 1; i++)
            {
                distance += cities[tour[i]].edges[tour[i + 1]];
            }

            distance += cities[tour[tour.Length - 1]].edges[tour[0]];

            fitness = 1 / distance;
        }


    }

}
