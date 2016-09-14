using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning2015
{
    class ACO
    {
        //array of arrays to store the ants
        public int[][] ants { get; set; }
        //the list of pheromone trails 
        public double[][] Pheromones { get; set; }
        //cities to visit 
        public List<City> cities { get; set; }
        //number of cities
        public int n_cities { get; set; }
        //bust current tail
        public int[] Best_Trail  { get; set; }
        public double Best_Dist { get; set; }

        
        //used to calculate the probability
        //pheromone influence factor
        public const int alpha = 3;
        //city distance factor
        public const int beta = 2;
        public double maxTau { get; set; }

        //pheromone decrease factor
        public const double pdf = 0.01;
        //pheromone increase factor
        public const double pif = 2.0;
        public List<string> results = new List<string>();

        public ACO(int ant_num, List<City> _cities)
        {
            //initializing properties
            this.cities = _cities;
            n_cities = cities.Count;
            maxTau = double.MaxValue / (n_cities * 100);

            //initializing ants
            initAnts(ant_num);
            Best_Trail = FindBestTrail();
            Best_Dist = TrailLenght(Best_Trail);
            Console.WriteLine("Current shortest path length: {0:0.00} at Start", Best_Dist);

            //initializing pheromones 
            initPheromones();

        }

        public List<string> runSimulation(int Max_Time)
        {
            int t =0;
            while (t < Max_Time)
            {
                AntUpdate();
                PheromonesUpdate();

                int[] temp_best = FindBestTrail();
                double temp_dist = TrailLenght(temp_best);

                if (temp_dist < Best_Dist)
                {
                    Best_Dist = temp_dist;
                    Best_Trail = temp_best;

                    results.Add(string.Format("{0},{1}", Best_Dist, t));
                    Console.WriteLine("Current shortest path length: {0:0.00} on iteration {1}", Best_Dist, t);

                }
                t++;
            }

            return results;
        }


        public void initAnts(int n_ants)
        {

            //initializing the ant population 
            this.ants = new int[n_ants][];

            Random rnd = new Random(DateTime.Now.Millisecond);

            //randomly selecting a start city for the ants 
            for (int i = 0; i < n_ants; i++)
            {
                ants[i] = new int[n_cities];

                for (int j = 0; j < n_cities; j++)
                    ants[i][j] = j;

                //creating random trail
                ants[i] = ants[i].OrderBy(c => Guid.NewGuid()).ToArray();
            }

        }

        public int[] FindBestTrail()
        {
            int bestAnt = 0;
            double distance = TrailLenght(ants[0]);

            for (int i = 1; i < ants.Length - 1; i++)
            {
                double temp = TrailLenght(ants[i]);
                if (temp < distance)
                {
                    distance = temp;
                    bestAnt = i;
                }
            }

            return ants[bestAnt];
        }

        public double TrailLenght(int[] trail)
        {
            double distance = 0.0;
            for (int i = 0; i < trail.Length - 1; i++)
            {
                distance += cities[trail[i]].edges[trail[i + 1]];
            }
            return distance += cities[trail[trail.Length - 1]].edges[trail[0]];
        }

        public void initPheromones()
        {
            //initializing the pheromone trails between cities 

            Pheromones = new double[n_cities][];

            //setting all the pheromones to a small value, this will help start the ant and pheromone updating
            for (int i = 0; i < n_cities; i++)
            {
                Pheromones[i] = new double[n_cities];

                for (int j = 0; j < n_cities; j++)
                {
                    Pheromones[i][j] = 0.01;
                }
            }
        }

        public void AntUpdate()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < ants.Length; i++)
            {
                int startCity = rnd.Next(0, n_cities);
                int[] trail = NewTrail(startCity);
                ants[i] = trail;
            }
        }

        public void PheromonesUpdate()
        {
            for (int i = 0; i <= Pheromones.Length -1; i++)
            {
                for (int j = 0; j <= Pheromones[i].Length-1; j++)
                {
                    for (int k = 0; k <= ants.Length -1; k++)
                    {
                        double trail_len = TrailLenght(ants[k]);
                        double decrease = (1.0 - pdf) * Pheromones[i][j];
                        double increase = 0.0;

                        if (TrailHasEdge(k, i,j))
                        {
                            increase = (pif / trail_len);
                        }

                        Pheromones[i][j] = decrease + increase;

                        if ( Pheromones[i][j] < 0.0001)
                        {
                            Pheromones[i][j] = 0.0001;
                        }
                        if (Pheromones[i][j] > 100000.0)
                        {
                            Pheromones[i][j] = 100000.0;
                        }

                    }
                }
            }
        }

        public bool TrailHasEdge(int ant, int p_c1, int p_c2 )
        {
            int index_c1 = Array.IndexOf(ants[ant], p_c1);

            if (index_c1 == 0)
            {
                if (ants[ant][ants[ant].Length -1] == p_c2)
                {
                    return true;
                }
            }
            else if (index_c1 == ants[ant].Length - 1)
            {
                if (ants[ant][0] == p_c2)
                {
                    return true;
                }
            }
            else
            {
                if (ants[ant][(index_c1 + 1) % ants[ant].Length] == p_c2 || ants[ant][(index_c1 - 1) % ants[ant].Length] == p_c2)
                {
                    return true;
                }
            }

            

            return false;
        }

        public int[] NewTrail(int s_city)
        {
            int[] trail = new int[n_cities];
            bool[] visited = new bool[n_cities];
            trail[0] = s_city;
            visited[s_city] = true;

            for (int i = 0; i < n_cities - 1; i++)
            {
                int c_city = trail[i];
                int nextCity = selectCity(c_city, visited);
                trail[i + 1] = nextCity;
                visited[nextCity] = true;

            }

            return trail;
        }

        public int selectCity(int city, bool[] visited)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);

            double[] prob = CalculateProbability(city, visited);
            double[] prob_sum = new double[n_cities + 1];

            for (int i = 0; i < n_cities; i++)
                prob_sum[i + 1] = prob[i] + prob_sum[i];

            if (prob_sum[prob_sum.Count() - 1] > 1)
                prob_sum[prob_sum.Count() - 1] = 1;

            double p = rnd.NextDouble();

            for (int i = 0; i < n_cities; i++)
                if (p >= prob_sum[i] && p < prob_sum[i + 1])
                    return i;

            throw new Exception("Failure to return valid city in NextCity");
        }

        public double[] CalculateProbability(int curr_city, bool[] visited) 
        {
            double[] T = new double[n_cities];
            double sum = 0.0;

            for (int i = 0; i < n_cities; i++)
            {
                if (i == curr_city)
                    T[i] = 0.0;
                else if(visited[i] == true)
                    T[i] = 0.0;
                else
                {
                    T[i] = Math.Pow(Pheromones[curr_city][i], alpha) * Math.Pow((1 / cities[curr_city].edges[i]), beta);
                    if (T[i] < 0.0001)
                        T[i] = 0.0001;
                    else if (T[i] > maxTau)
                        T[i] = maxTau;
                }
                sum += T[i];
            }

            double[] probs = new double[n_cities];

            for (int i = 0; i < n_cities; i++)
                probs[i] = T[i] / sum;

            return probs;
        }


        
    }


}
