using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearning2015
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = new string[] {
                @"C:\Resources\burma14.xml",
                @"C:\Resources\bays29.xml",
                @"C:\Resources\berlin52.xml",
                @"C:\Resources\gr96.xml",
            };

            string[] Names = new string[]{
                "burma14.xml",
                "bays29.xml",
                "berlin52.xml",
                "gr96.xml",
            };

            

            for (int i = 0; i < files.Length; i++)
            {
                TSP tsp = new TSP(files[i]);

                
                runExperiment(tsp, Names[i]);

            }


            







            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        public static void runExperiment(TSP tsp, string Name)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Console.WriteLine("Instance Name: " + Name);
            Console.WriteLine("Method: Genetic Algorithm");
            Genetic G = new Genetic(tsp.cities, 10);
            List<string> results = G.runSimulation(1000);
            watch.Stop();
            results.Add(string.Format("Total Time {0}", watch.Elapsed));
            saveResult(string.Format("GA_{0}",Name), results);


            watch = new Stopwatch();
            watch.Start();

            Console.WriteLine("Instance Name: " + Name);
            Console.WriteLine("Method: ACO");
            ACO a = new ACO(10, tsp.cities);
            results = a.runSimulation(1000);
            watch.Stop();
            results.Add(string.Format("Total Time {0}", watch.Elapsed));
            saveResult(string.Format("ACO_{0}", Name), results);
        }

        private static void saveResult(string FileName, List<string> results){

           string filePath = @"C:\\Resources\\Results\\" + FileName + ".csv";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
			{
				foreach (string line in results)
				{
					file.WriteLine(line);
				}
			}
        }

    }



}
