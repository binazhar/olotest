using Combinatorics.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OloSET
{
    class Program
    {
        static void Main(string[] args)
        {


            string json = string.Empty;
            //Dictionary<int, List<string>> toppingPairs = new Dictionary<int, List<string>>();
            Dictionary< List < string > , int> toppingPairs = new Dictionary<List<string>, int>(new ListComparer());


            /* Read directly from the websource */
            Uri uri = new Uri("http://files.olo.com/pizzas.json");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            json = reader.ReadToEnd();
            response.Close();

            /* Read from File */
            //using (StreamReader r = new StreamReader(@"E:\workspace\OloSET\OloSET\bin\pizzas.json"))
            //{
            //    json = r.ReadToEnd();
            //}


            /* As the json only have toppings information, we are not creating class, resorting to dynamic instead */
            dynamic array = JsonConvert.DeserializeObject(json);

            foreach (var order in array)                    // For all the orders
            {
                
                string[] test = order.toppings.ToObject<string[]>();        // Convert the topings to string array 

                List<string> toppings = new List<string>(test);             // and to List for permutation library

                toppings.Sort();                                            // Sort to make comparison easier

                if (toppings.Count() == 1)                                  // If there is only one topping skip to next order
                {

                    continue;
                }
                else if (toppings.Count() == 2)                         
                {
                    if (!toppingPairs.ContainsKey(toppings))               // If order has two toppings, check of we have already recorded it
                    {
                        toppingPairs.Add(toppings, 1);                      // No? add to list 
                    }
                    else
                    {
                        toppingPairs[toppings]++;                           // Yes ? increment order frequency
                    }
                }
                else
                {
                    for (int i = 2; i <= toppings.Count; i++)              // More that two toppings,
                    {
                        Combinations<string> toppingCombination = new Combinations<string>(toppings, i);        // Generate all possible combinations i.e. pick 2, pick 3 ...
                        foreach (IList<string> combinationsIList in toppingCombination)
                        {
                            var combinations = combinationsIList.ToList();
                            combinations.Sort();

                            if (!toppingPairs.ContainsKey(combinations))                                    // Test for existence
                            {
                                toppingPairs.Add(combinations, 1);                                          // If no, then add
                            }
                            else
                            {
                                toppingPairs[combinations]++;                                               // Otherwise just increment
                            }
                        }

                    }
                }


           }

            var sortedDict = from entry in toppingPairs orderby entry.Value descending select entry;    // Sort Dictionary

            int rank = 1;

            foreach (var item in sortedDict.OrderByDescending(r => r.Value).Take(20))                   // for top 20
            {
                Console.Write("Rank: {0} \t", rank);                                                   // Print Rank
                item.Key.ForEach(i => Console.Write(" {0} ", i));                                // Topping combination
                Console.WriteLine("\t\t\t Value: {0}", item.Value);                                        // Number of orders

                rank++;

                
            }
                    

        }

        
    }
    // Override default key comparison class in dictionary to accomodate for Keys containing list
    class ListComparer : IEqualityComparer<List<string>>
    {
        public bool Equals(List<string> x, List<string> y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(List<string> obj)
        {
            int hashcode = 0;
            foreach (string t in obj)
            {
                hashcode ^= t.GetHashCode();
            }
            return hashcode;
        }
    }
}
