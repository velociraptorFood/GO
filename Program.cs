using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GO
{
    class Program
    {
        static Order[] orderList;
        static Tuple<int, int>[,] afstanden;
        static int capacity = 20000;
        static int orderCount;
        static float totalStortTime;

        //<orderID, <freq, kosten>>
        static Dictionary<int, Tuple<int, float>> orderFreqs = new Dictionary<int, Tuple<int, float>>();
        static List<Order> removed = new List<Order>();

        //Ophalen orderlijst en bedrijvennetwerk uit bijbehorende bestanden
        static string[] orders = System.IO.File.ReadAllLines(@"../../orders.txt");
        static string[] bedrijvennetwerk = System.IO.File.ReadAllLines(@"../../bedrijven.txt");

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
            afstanden = new Tuple<int, int>[1099, 1099];
            for (int i = 0; i < bedrijvennetwerk.Length; i++)
            {
                string[] split = bedrijvennetwerk[i].Split(';');
                afstanden[int.Parse(split[0]), int.Parse(split[1])] = new Tuple<int, int>(int.Parse(split[2]), int.Parse(split[3]));
            }

            Console.WriteLine("best result: " + StartSmart());
            Console.ReadLine();
        }        

        //gebruik ShortestTimeSort() om de 2 opgedeelde delen greedy te sorten
        //voegt ook voor orders met freq > 1 data toe aan een dict zodat er bijgehouden kan worden of deze orders goed behandeld worden
        static float StartSmart()
        {
            List<Order>[] auto1 = new List<Order>[5], auto2 = new List<Order>[5];
            List<Order> auto1Ma = new List<Order>(), auto1Di = new List<Order>(), auto1Wo = new List<Order>(), auto1Do = new List<Order>(), auto1Vr = new List<Order>(),
                auto2Ma = new List<Order>(), auto2Di = new List<Order>(), auto2Wo = new List<Order>(), auto2Do = new List<Order>(), auto2Vr = new List<Order>();
            auto1[0] = auto1Ma; auto1[1] = auto1Di; auto1[2] = auto1Wo; auto1[3] = auto1Do; auto1[4] = auto1Vr;
            auto2[0] = auto2Ma; auto2[1] = auto2Di; auto2[2] = auto2Wo; auto2[3] = auto2Do; auto2[4] = auto2Vr;

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
            return Iterate(auto1, auto2, 10000);
        }



        //zoek steeds de dichtstbijzijnde locatie en voeg die toe als volgende in de list
        static List<Order> ShortestTimeSort(List<Order> input)
        {
            List<Order> sortedList = new List<Order>();
            Order best = new Order();
            int curLoc = 287, nextLoc = 287;
            int minTime, curTime;
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
            float minScore, t = 0;
            int k = 0;
            Random r = new Random();
            minScore = Eval(auto1, auto2);
            List<Order>[] bestAuto1 = new List<Order>[5], bestAuto2 = new List<Order>[5];
            //loop voor de iteraties
            while (k < limit)
            {
                Tuple<List<Order>[], List<Order>[]> newAutos = GenerateNeighbours(auto1, auto2);
                List<Order>[] newAuto1 = newAutos.Item1, newAuto2 = newAutos.Item2;
                float newScore = Eval(newAuto1, newAuto2);
                double p = Math.Exp(newScore - minScore) / t;
                if (newScore <= Eval(auto1, auto2))
                {
                    auto1 = newAuto1;
                    auto2 = newAuto2;
                    if (newScore < minScore)
                    {
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
                //t neemt iedere iteratie af
                t = (float)Math.Pow(0.99, (k / 1000));
            }
            //Console.WriteLine("orders in cars: " + (bestAuto1.Count + bestAuto2.Count) + " removed orders: " + removed.Count);
            Console.WriteLine("storttime: " + totalStortTime);
            return minScore;

        }

        //bereken index van de order die het verst van de rest vandaan ligt
        static int MostExpensive(List<Order> input)
        {
            int index = 0;
            int time, maxTime = 0;

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
            List<Order>[] nb1 = new List<Order>[5]; List<Order>[] nb2 = new List<Order>[5];
            for(int i = 0; i < 5; i++)
            {
                nb1[i] = CloneList(input1[i]);
                nb2[i] = CloneList(input2[i]);
            }
            Random rnd = new Random();
            int index;

                 

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
        
        //swap 2 elementen in een list op de index x en y
        static List<Order> Swap(List<Order> input, int x, int y)
        {
            List<Order> output = CloneList(input);
            if (x != y)
            {
                Order temp = output[x];
                output[x] = output[y];
                output[y] = temp;
            }
            return output;
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
        static Tuple<List<Order>, List<Order>> SwapBetween(List<Order> input1, List<Order> input2, int x, int y)
        {
            List<Order> output1 = CloneList(input1), output2 = CloneList(input2);
            Order temp = output1[x];
            output1[x] = output2[y];
            output2[y] = temp;
            return new Tuple<List<Order>, List<Order>>(output1, output2);
        }

        //berekend de score van een state
        static float Eval(List<Order>[] input1, List<Order>[] input2)
        {
            totalStortTime = 0;
            float score = 0;
            //track how many times orders with freq > 1 are visited and what the previous day was
            Dictionary<int, Tuple<int,int>> timesVisited = new Dictionary<int, Tuple<int, int>>();

            score += CalcCost(input1, timesVisited);
            score += CalcCost(input2, timesVisited);

            //strafpunten voor orders die niet vaak genoeg zijn opgehaald
            foreach (KeyValuePair<int, Tuple<int, float>> keyval in orderFreqs)
            {
                if (keyval.Value.Item1 != timesVisited[keyval.Key].Item1)
                    score += keyval.Value.Item2 * 3f;
            }
            //strafpunten voor orders die overgeslagen zijn 
            //maar niet al hierboven behandeld zijn
            foreach (Order o in removed)
            {
                if(!orderFreqs.ContainsKey(o.id))
                {
                    score += o.ledigingsDuur * 3f;
                }
            }
            return score;
        }

        //brekend de score van een enkele auto
        static float CalcCost(List<Order>[] input, Dictionary<int, Tuple<int,int>> timesVisited)
        {
            int day;
            float cost = 0, time = 0, currentLoad = 0;
            for (int j = 0; j < 5; j++)
            {
                day = j + 1;
                for (int i = 0; i < input[j].Count - 1; i++)
                {
                    float totalVolume = input[j][i].volume * input[j][i].aantContainers * 0.2f;
                    int id = input[j][i].matrixID, nextID = input[j][i + 1].matrixID;
                    if (time == 0)
                        time += afstanden[287, id].Item2;

                    //behandel eerst het ophalen van het afval van deze locatie

                    //als er meerdere keren opgehaald moet worden
                    if (orderFreqs.ContainsKey(input[j][i].id))
                    {
                        Tuple<int, float> freq = orderFreqs[input[j][i].id];
                        bool visitedOnce = timesVisited.ContainsKey(input[j][i].id);
                        Tuple<int, int> times = new Tuple<int, int>(0, 0);
                        if (visitedOnce)
                            times = timesVisited[input[i][j].id];
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

                    float nextDestTime = afstanden[id, nextID].Item2;
                    float toStortTime = afstanden[id, 287].Item2;

                    //kijk daarna naar wat de volgende bestemming wordt

                    //720 min per dag zijn de autos beschikbaar
                    if (time + nextDestTime + afstanden[nextID, 287].Item2 + 30 >= 720)
                    {
                        //terug naar stort aan het eind van de dag
                        time += toStortTime + 30;
                        cost += time;
                        currentLoad = 0;
                        time = 0;
                        totalStortTime += toStortTime + 30;

                        //NIET ALLE ORDERS IN EEN DAGLIJST HOEVEN BEHANDELD TE WORDEN

                    }
                    else if (currentLoad + input[j][i + 1].volume * 0.2f > capacity)
                    {
                        //naar stort en storten
                        time += toStortTime + 30;
                        currentLoad = 0;
                        //naar volgende bestemming
                        time += afstanden[287, nextID].Item2;

                        totalStortTime += toStortTime + 30 + afstanden[287, nextID].Item2;
                    }
                    //storten als het onderweg kan 
                    else if (nextDestTime >= toStortTime + afstanden[287, nextID].Item2)
                    {
                        //naar stort en storten
                        time += toStortTime + 30;
                        currentLoad = 0;
                        //naar volgende bestemming
                        time += afstanden[287, nextID].Item2;

                        totalStortTime += toStortTime + 30 + afstanden[287, nextID].Item2;
                    }
                    else
                        time += nextDestTime;
                }
                cost += afstanden[input[j][input[j].Count - 1].matrixID, 287].Item2;
            }
            return cost;
        }
    }
}
