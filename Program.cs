using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;

namespace GO
{
    class Program
    {
        static Order[] orderList;
        static Tuple<int, float>[,] afstanden;
        static int capacity = 20000;
        static int orderCount;
        static float totalStortTime;

        //<orderID, <freq, kosten>>
        static Dictionary<int, Tuple<int, float>> orderFreqs = new Dictionary<int, Tuple<int, float>>();

        //Ophalen orderlijst en bedrijvennetwerk uit bijbehorende bestanden
        static string[] orders = File.ReadAllLines(@"../../orders.txt");
        static string[] bedrijvennetwerk = File.ReadAllLines(@"../../bedrijven.txt");

        static List<string> sol = new List<string>();

        static void Main()
        {
            orderList = new Order[1177];

            //orders inlezen
            for (int i = 0; i < 1177; i++)
            {
                string[] split = orders[i].Split(';');
                Order newOrder = new Order();
                newOrder.id = int.Parse(split[0]);                     newOrder.plaats = split[1].Trim();
                newOrder.freq = int.Parse(split[2][0].ToString());     newOrder.aantContainers = int.Parse(split[3]);
                newOrder.volume = int.Parse(split[4]);                 newOrder.ledigingsDuur = float.Parse(split[5]);
                newOrder.x = int.Parse(split[7]);                      newOrder.y = int.Parse(split[8]);
                newOrder.matrixID = int.Parse(split[6]);
                orderList[i] = newOrder;
            }

            foreach(Order o in orderList)
            {
                orderCount += o.freq;
            }

            //afstanden[punt A, punt B] = (afstand, rijtijd)
            afstanden = new Tuple<int, float>[1099, 1099];
            for (int i = 0; i < bedrijvennetwerk.Length; i++)
            {
                string[] split = bedrijvennetwerk[i].Split(';');
                afstanden[int.Parse(split[0]), int.Parse(split[1])] = new Tuple<int, float>(int.Parse(split[2]), float.Parse(split[3]));
            }

            Console.WriteLine("best result: " + StartSmart());
            Console.ReadLine();
        }        

        //gebruik ShortestTimeSort() om de 2 opgedeelde delen greedy te sorten
        //voegt ook voor orders met freq > 1 data toe aan een dict zodat er bijgehouden kan worden of deze orders goed behandeld worden
        static float StartSmart()
        {             
            List<Order>[] auto1 = new List<Order>[6], auto2 = new List<Order>[6];
            List<Order> auto1Ma = new List<Order>(), auto1Di = new List<Order>(), auto1Wo = new List<Order>(), auto1Do = new List<Order>(), auto1Vr = new List<Order>(),
                auto2Ma = new List<Order>(), auto2Di = new List<Order>(), auto2Wo = new List<Order>(), auto2Do = new List<Order>(), auto2Vr = new List<Order>();
            auto1[0] = auto1Ma; auto1[1] = auto1Di; auto1[2] = auto1Wo; auto1[3] = auto1Do; auto1[4] = auto1Vr; auto1[5] = new List<Order>();
            auto2[0] = auto2Ma; auto2[1] = auto2Di; auto2[2] = auto2Wo; auto2[3] = auto2Do; auto2[4] = auto2Vr; auto2[5] = new List<Order>();

            //orderList = ShortestTimeSort(orderList.ToList()).ToArray();

            int step = orderList.Length / 10;
            for (int i = 0; i < orderList.Length; i++)
            {
                int freq = orderList[i].freq;
                if (freq > 1 && i < orderList.Length / 2)
                {
                    orderFreqs.Add(orderList[i].id, new Tuple<int, float>(freq, orderList[i].ledigingsDuur * orderList[i].aantContainers));
                    if (freq == 2)
                    {
                        auto1[0].Add(orderList[i].Clone());
                        auto1[3].Add(orderList[i].Clone());
                    }
                    else if (freq == 3)
                    {
                        auto1[0].Add(orderList[i].Clone());
                        auto1[2].Add(orderList[i].Clone());
                        auto1[4].Add(orderList[i].Clone());
                    }
                    else if (freq == 4)
                    {
                        auto1[0].Add(orderList[i].Clone());
                        auto1[1].Add(orderList[i].Clone());
                        auto1[2].Add(orderList[i].Clone());
                        auto1[3].Add(orderList[i].Clone());
                    }
                    else if (freq == 5)
                    {
                        auto1[0].Add(orderList[i].Clone());
                        auto1[1].Add(orderList[i].Clone());
                        auto1[2].Add(orderList[i].Clone());
                        auto1[3].Add(orderList[i].Clone());
                        auto1[4].Add(orderList[i].Clone());
                    }
                }
                else if (freq > 1 && i < orderList.Length)
                {
                    orderFreqs.Add(orderList[i].id, new Tuple<int, float>(freq, orderList[i].ledigingsDuur * orderList[i].aantContainers));
                    if (freq == 2)
                    {
                        auto2[0].Add(orderList[i].Clone());
                        auto2[3].Add(orderList[i].Clone());
                    }
                    else if (freq == 3)
                    {
                        auto2[0].Add(orderList[i].Clone());
                        auto2[2].Add(orderList[i].Clone());
                        auto2[4].Add(orderList[i].Clone());
                    }
                    else if (freq == 4)
                    {
                        auto2[0].Add(orderList[i].Clone());
                        auto2[1].Add(orderList[i].Clone());
                        auto2[2].Add(orderList[i].Clone());
                        auto2[3].Add(orderList[i].Clone());
                    }
                    else if (freq == 5)
                    {
                        auto2[0].Add(orderList[i].Clone());
                        auto2[1].Add(orderList[i].Clone());
                        auto2[2].Add(orderList[i].Clone());
                        auto2[3].Add(orderList[i].Clone());
                        auto2[4].Add(orderList[i].Clone());
                    }
                }

                else if (i < step)
                    auto1[0].Add(orderList[i].Clone());
                else if (i < 2*step)
                    auto1[1].Add(orderList[i].Clone());
                else if (i < 3*step)
                    auto1[2].Add(orderList[i].Clone());
                else if (i < 4*step)
                    auto1[3].Add(orderList[i].Clone());
                else if (i < 5*step)
                    auto1[4].Add(orderList[i].Clone());
                else if (i < 6*step)
                    auto2[0].Add(orderList[i].Clone());
                else if (i < 7*step)
                    auto2[1].Add(orderList[i].Clone());
                else if (i < 8*step)
                    auto2[2].Add(orderList[i].Clone());
                else if (i < 9*step)
                    auto2[3].Add(orderList[i].Clone());
                else
                    auto2[4].Add(orderList[i].Clone());
            }           
            return Iterate(auto1, auto2, 10000000);
        }



        //zoek steeds de dichtstbijzijnde locatie en voeg die toe als volgende in de list
        static List<Order> ShortestTimeSort(List<Order> input)
        {
            List<Order> sortedList = new List<Order>();
            Order best = new Order();
            int curLoc = 287, nextLoc = 287;
            float minTime, curTime;
            foreach(Order o_ in input)
            {
                minTime = int.MaxValue;
                foreach (Order o in input)
                {
                    if (!sortedList.Any(f => f.id == o.id))
                    {
                        curTime = afstanden[curLoc, o.matrixID].Item2;
                        if (curTime <= minTime)
                        {
                            minTime = curTime;
                            nextLoc = o.matrixID;
                            best = o.Clone();
                        }
                    }
                }
                curLoc = nextLoc;                
                sortedList.Add(best);
            }
            return sortedList;
        }

        //gebruik simulated annealing om een oplossing te vinden
        static float Iterate(List<Order>[] auto1, List<Order>[] auto2, int limit)
        {
            float minScore, t = 100;
            int k = 0;
            Random r = new Random();
            minScore = Eval(auto1, auto2, false);
            List<Order>[] bestAuto1 = auto1, bestAuto2 = auto2;
            //loop voor de iteraties
            while (k < limit)
            {
                Tuple<List<Order>[], List<Order>[]> newAutos = GenerateNeighbours(auto1, auto2);
                List<Order>[] newAuto1 = newAutos.Item1, newAuto2 = newAutos.Item2;
                float newScore = Eval(newAuto1, newAuto2, false); float oldScore = Eval(auto1, auto2, false);
                double p = Math.Exp(-(newScore - oldScore) / t);
                if (newScore <= oldScore)
                {
                    auto1 = newAuto1;
                    auto2 = newAuto2;
                    if (newScore < minScore)
                    {
                        Console.WriteLine(newScore);
                        minScore = newScore;
                        bestAuto1 = auto1; bestAuto2 = auto2;
                    }
                }
                // met p wordt de kans op acceptatie van een slechter resultaat bepaald
                else if (p > r.NextDouble())
                {
                    auto1 = newAuto1;
                    auto2 = newAuto2;
                }
                k++;
                //t neemt iedere x iteraties af
                if(k % 10000 == 0)
                {
                    t = t * 0.99f;
                    Console.WriteLine(k);
                }

            }

            Eval(bestAuto1, bestAuto2, true);
            Console.WriteLine(sol.Count);
            File.WriteAllLines("output.txt", sol);

            int ordersInCars = 0;
            for(int i = 0; i < 5; i++)
                ordersInCars += bestAuto1[i].Count;
            for (int i = 0; i < 5; i++)
                ordersInCars += bestAuto2[i].Count;

            int removedOrders = bestAuto1[5].Count + bestAuto2[5].Count;

            Console.WriteLine(orderCount);
            Console.WriteLine("orders in cars: " + ordersInCars + " removed orders: " + removedOrders);
            Console.WriteLine("storttime: " + totalStortTime);
            return minScore;

        }

        //bereken index van de order die het verst van de rest vandaan ligt
        static int MostExpensive(List<Order> input)
        {
            int index = 0;
            float time, maxTime = 0;

            for(int i = 0; i < input.Count; i++)
            {
                time = 0;
                if (i > 0)
                    time += afstanden[input[i - 1].matrixID, input[i].matrixID].Item2;
                if (i < input.Count-1)
                    time += afstanden[input[i + 1].matrixID, input[i].matrixID].Item2;
                if (time >= maxTime)
                {
                    maxTime = time;
                    index = i;
                }                       
            }
            return index;
        }

        //genereer nbs voor de 2 input lijsten door:
        static Tuple<List<Order>[],List<Order>[]> GenerateNeighbours(List<Order>[] input1, List <Order>[] input2)
        {
            List<Order>[] nb1 = new List<Order>[6]; List<Order>[] nb2 = new List<Order>[6];
            for(int i = 0; i < 6; i++)
            {
                nb1[i] = CloneList(input1[i]);
                nb2[i] = CloneList(input2[i]);
            }
            Random rnd = new Random();
            int index1, index2, index3, index4,
                choice = rnd.Next(0,13);

            //swap in nb1
            if (choice == 0)
            {
                index1 = rnd.Next(0, nb1.Length); index2 = rnd.Next(0, nb1.Length);
                index3 = rnd.Next(0, 5);
                Swap(nb1[index3], index1, index2);
            }
            //swap in nb2
            else if (choice == 1)
            {
                index1 = rnd.Next(0, nb1.Length); index2 = rnd.Next(0, nb1.Length);
                index3 = rnd.Next(0, 5);
                Swap(nb2[index3], index1, index2);
            }
            //swap between nb1 and nb2
            else if (choice == 2)
            {
                index1 = rnd.Next(0, nb1.Length); index2 = rnd.Next(0, nb1.Length);
                index3 = rnd.Next(0, 5); index4 = rnd.Next(0, 5);
                SwapBetween(nb1[index3],nb2[index3], index1, index2);
            }
            //remove from nb1
            else if (choice == 3)
            {
                index1 = rnd.Next(0, 5); index2 = rnd.Next(0, nb1.Length);
                Remove(nb1, index1, index2);
            }
            //remove from nb2
            else if (choice == 4)
            {
                index1 = rnd.Next(0, 5); index2 = rnd.Next(0, nb2.Length);
                Remove(nb2, index1, index2);
            }
            //add from remove on index2 to day index1 to nb1
            else if (choice == 5)
            {
                index1 = rnd.Next(0, 5); index2 = rnd.Next(0, nb1[5].Count);
                AddFromRemove(nb1, index1, index2);
            }
            //add from remove on index2 to day index1 to nb2
            else if (choice == 6)
            {
                index1 = rnd.Next(0, 5); index2 = rnd.Next(0, nb2[5].Count);
                AddFromRemove(nb2, index1, index2);
            }
            //move order from nb1 on any day or removed to any day or removed on nb2
            else if (choice == 7)
            {
                index1 = rnd.Next(0, 6); index2 = rnd.Next(0, 6);
                index3 = rnd.Next(0, nb1[index1].Count);
                if(!(index2 == 6 && nb1[index1][index3].stort))
                    Shift(nb1[index1], nb2[index2], index3);
            }
            //move order from nb2 on any day or removed to any day or removed on nb1
            else if (choice == 8)
            {
                index1 = rnd.Next(0, 6); index2 = rnd.Next(0, 6);
                index3 = rnd.Next(0, nb2[index1].Count);
                if (!(index2 == 6 && nb2[index1][index3].stort))
                    Shift(nb2[index1], nb1[index2], index3);
            }
            //shift from nb2 removed to a random day in nb1
            else if (choice == 9)
            {
                index1 = rnd.Next(0, 5); index2 = rnd.Next(0, nb2[5].Count);
                Shift(nb2[5], nb1[index1], index2);
            }
            //shift from nb1 removed to a random day in nb2
            else if (choice == 10)
            {
                index1 = rnd.Next(0, 5); index2 = rnd.Next(0, nb2[5].Count);
                Shift(nb1[5], nb2[index1], index2);
            }
            //add extra stort to nb1
            else if (choice == 11)
            {
                Order o = new Order();
                o.stort = true;
                index1 = rnd.Next(0, 5); index2 = rnd.Next(0, nb1[index1].Count);
                nb1[index1].Insert(index2, o);
            }
            //add extra stort to nb2
            else if (choice == 12)
            {
                Order o = new Order();
                o.stort = true;
                index1 = rnd.Next(0, 5); index2 = rnd.Next(0, nb2[index1].Count);
                nb2[index1].Insert(index2, o);
            }
            else if (choice == 13)
            {
                SwapRemoveLists(nb1, nb2);
            }

            return new Tuple<List<Order>[], List<Order>[]>(nb1, nb2);
        }
        
        //maak een copie van een list
        static List<Order> CloneList(List<Order> input)
        {     
            List<Order> newList = new List<Order>();
            foreach (Order o in input)
            {
                newList.Add(o.Clone());
            }
            return newList;
            
        }

        static void Remove(List<Order>[] input, int x, int y)
        {
            if (y < input[x].Count && !input[x][y].stort)
            {
                input[5].Add(input[x][y]);
                input[x].RemoveAt(y);
            }
            else if (y < input[x].Count)
            {
                input[x].RemoveAt(y);
            }
        }

        static void AddFromRemove(List<Order>[] input, int x, int y)
        {
            if (y < input[5].Count)
            {
                input[x].Add(input[5][y]);
                input[5].RemoveAt(y);
            }
        }

        static void SwapRemoveLists(List<Order>[] list1, List<Order>[] list2)
        {
            List<Order> templist = new List<Order>();
            templist = list1[5];
            list1[5] = list2[5];
            list2[5] = templist;
        }

        static void Shift(List<Order> input1, List<Order> input2, int x)
        {
            if (x < input1.Count)
            {
                input2.Add(input1[x]);
                input1.RemoveAt(x);
            }
        }
        
        //swap 2 elementen in een list op de index x en y
        static void Swap(List<Order> input, int x, int y)
        {
            if (x != y && x < input.Count && y < input.Count)
            {
                Order temp = input[x];
                input[x] = input[y];
                input[y] = temp;
            }
        }

        //swap 3 elementen tegelijk vanaf index x en y
        static List<Order> Swap3(List<Order> input, int x, int y)
        {
            List<Order> output = CloneList(input);

            if ((x + 3) < y || x > (y + 3))
            {
                Order temp = output[x];
                Order temp2 = output[x+1];
                Order temp3 = output[x+2];
                output[x] = output[y];
                output[x+1] = output[y+1];
                output[x+2] = output[y+2];
                output[y] = temp;
                output[y+1] = temp2;
                output[y+2] = temp3;
            }
            return output;
        }

        //swap een willekeurig aantal elementen tegelijk vanaf index x en y
        static List<Order> SwapAmount(List<Order> input, int x, int y, int amount)
        {
            List<Order> output = CloneList(input);
            Order[] templist = new Order[amount];
            if ((x + amount) < y || x > (y + amount))
            {
                for(int i = 0; i < amount; i++)
                {
                    templist[i] = output[x + i];
                }
                for(int i = 0; i < amount; i++)
                {
                    output[x + i] = output[y + i];
                    output[y + i] = templist[i];
                }
            }
            return output;
        }

        //swap orders tussen de 2 verschillende lijsten (auto1 en auto2)
        static void SwapBetween(List<Order> input1, List<Order> input2, int x, int y)
        {
            if (x < input1.Count && y < input2.Count)
            {
                Order temp = input1[x];
                input1[x] = input2[y];
                input2[y] = temp;
            }
        }

        //berekend de score van een state
        static float Eval(List<Order>[] input1, List<Order>[] input2, bool final)
        {
            totalStortTime = 0;
            float score = 0;
            //track how many times orders with freq > 1 are visited and what the previous day was
            Dictionary<int, Tuple<int,int>> timesVisited = new Dictionary<int, Tuple<int, int>>();

            score += CalcCost(input1, timesVisited, final, 1);
            score += CalcCost(input2, timesVisited, final, 2);

            //strafpunten voor orders die niet vaak genoeg zijn opgehaald
            foreach (KeyValuePair<int, Tuple<int, float>> keyval in orderFreqs)
            {
                if (!timesVisited.ContainsKey(keyval.Key) || keyval.Value.Item1 != timesVisited[keyval.Key].Item1)
                    score += keyval.Value.Item2 * 3f;
            }
            //strafpunten voor orders die overgeslagen zijn 
            //maar niet al hierboven behandeld zijn
            foreach (Order o in input1[5])
            {
                if(!orderFreqs.ContainsKey(o.id))
                {
                    score += o.ledigingsDuur * 3f;
                }
            }
            foreach (Order o in input2[5])
            {
                if (!orderFreqs.ContainsKey(o.id))
                {
                    score += o.ledigingsDuur * 3f;
                }
            }
            return score;
        }

        //brekend de score van een enkele auto
        static float CalcCost(List<Order>[] input, Dictionary<int, Tuple<int,int>> timesVisited, bool final, int autonr)
        {
            int day, stops;
            float cost = 0, time = 0, currentLoad = 0;
            for (int j = 0; j < 5; j++)
            {
                time = 0; 
                day = j + 1; stops = 1;
                for (int i = 0; i < input[j].Count; i++)
                {
                    int id = input[j][i].matrixID;
                    float toStortTime = afstanden[id, 287].Item2 / 60;
                    if (input[j][i].stort && i < input[j].Count - 1)
                    {
                        if (final)
                            sol.Add(autonr + "; " + (j + 1) + "; " + stops + "; " + 0 + " extra");
                        stops++;
                        //naar stort en storten
                        time += toStortTime + 30;
                        currentLoad = 0;
                        //naar volgende bestemming
                        int nextID = input[j][i + 1].matrixID;
                        time += afstanden[287, nextID].Item2 / 60;

                        totalStortTime += toStortTime + 30 + (afstanden[287, nextID].Item2 / 60);
                    }
                    else
                    {


                        if (final)
                            sol.Add(autonr + "; " + (j + 1) + "; " + stops + "; " + input[j][i].id);
                        stops++;

                        float totalVolume = input[j][i].volume * input[j][i].aantContainers * 0.2f;
                        if (time == 0)
                            time += afstanden[287, id].Item2 / 60;

                        //behandel eerst het ophalen van het afval van deze locatie

                        //als er meerdere keren opgehaald moet worden
                        if (orderFreqs.ContainsKey(input[j][i].id))
                        {
                            Tuple<int, float> freq = orderFreqs[input[j][i].id];
                            bool visitedOnce = timesVisited.ContainsKey(input[j][i].id);
                            Tuple<int, int> times = new Tuple<int, int>(0, 0);
                            if (visitedOnce)
                                times = timesVisited[input[j][i].id];
                            //als er nog nooit is opgehaald
                            if (!visitedOnce)
                            {
                                time += input[j][i].ledigingsDuur;
                                currentLoad += totalVolume;
                                timesVisited.Add(input[j][i].id, new Tuple<int, int>(1, day));
                            }
                            //als er al een keer opgehaald is moet er gekeken worden of dit de goede dag is om weer op te halen volgens het bijbehorende ophaalpatroon
                            else if (times.Item1 != 0 && day - times.Item2 == 5 - freq.Item1 && freq.Item1 != times.Item2 || freq.Item1 == 5 && day != times.Item2)
                            {
                                time += input[j][i].ledigingsDuur;
                                currentLoad += totalVolume;
                                timesVisited[input[j][i].id] = new Tuple<int, int>(times.Item2 + 1, day);
                            }
                        }
                        else
                        {
                            time += input[j][i].ledigingsDuur;
                            currentLoad += totalVolume;
                        }
                        
                        //kijk daarna naar wat de volgende bestemming wordt

                        if (i == input[j].Count - 1)
                        {
                            if (final)
                                sol.Add(autonr + "; " + (j + 1) + "; " + stops + "; " + 0);
                            stops++;
                            currentLoad = 0;
                            cost += toStortTime + 30;
                        }
                        else
                        {
                            int nextID = input[j][i + 1].matrixID;
                            float nextDestTime = afstanden[id, nextID].Item2 / 60;
                            //720 min per dag zijn de autos beschikbaar
                            if (time + nextDestTime + (afstanden[nextID, 287].Item2 / 60) + 30 >= 720)
                            {
                                if (final)
                                    sol.Add(autonr + "; " + (j + 1) + "; " + stops + "; " + 0);
                                //terug naar stort aan het eind van de dag
                                time += toStortTime + 30;
                                currentLoad = 0;
                                totalStortTime += toStortTime + 30;
                                for (int z = i + 1; z < input[j].Count; z++)
                                {
                                    Remove(input, j, z);
                                }
                                break;
                            }
                            else if (currentLoad + input[j][i + 1].volume * input[j][i + 1].aantContainers * 0.2f > capacity)
                            {
                                if (final)
                                    sol.Add(autonr + "; " + (j + 1) + "; " + stops + "; " + 0);
                                stops++;
                                //naar stort en storten
                                time += toStortTime + 30;
                                currentLoad = 0;
                                //naar volgende bestemming
                                time += afstanden[287, nextID].Item2 / 60;

                                totalStortTime += toStortTime + 30 + (afstanden[287, nextID].Item2 / 60);
                            }
                            //storten als het onderweg kan 
                            else if (nextDestTime >= toStortTime + (afstanden[287, nextID].Item2 / 60))
                            {
                                if (final)
                                    sol.Add(autonr + "; " + (j + 1) + "; " + stops + "; " + 0);
                                stops++;
                                //naar stort en storten
                                time += toStortTime + 30;
                                currentLoad = 0;
                                //naar volgende bestemming
                                time += afstanden[287, nextID].Item2 / 60;

                                totalStortTime += toStortTime + 30 + (afstanden[287, nextID].Item2 / 60);
                            }
                            else
                                time += nextDestTime;
                        }
                    }               
                }
                cost += time;
            }
            return cost;
        }
    }
}
