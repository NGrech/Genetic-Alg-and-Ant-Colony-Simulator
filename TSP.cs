using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MachineLearning2015
{
    class TSP
    {
        public List<City> cities { get; set; }


        public TSP(string path)
        {
            this.cities = new List<City>();

            using (XmlTextReader reader = new XmlTextReader(path))
            {
                XmlDocument x = new XmlDocument();

                x.Load(reader);

                XmlNodeList vert = x.SelectNodes("/travellingSalesmanProblemInstance/graph/vertex");

                foreach (XmlNode v in vert)
                {
                    City c = new City();
                    c.edges = new double[vert.Count];

                    for (int i = 0; i < v.ChildNodes.Count; i++)
                        c.edges[int.Parse(v.ChildNodes[i].InnerText)] = double.Parse(v.ChildNodes[i].Attributes["cost"].Value);
                    
                    this.cities.Add(c);
                }
            }

        }
    }

    public class City
    {
        public double[] edges { get; set; }

    }
}
