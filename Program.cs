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

            Console.WriteLine(StartSmart());
            Console.ReadLine();
        }

        //start zonder "slim" een basisoplossing te genereren
        static float Start()
        {
            List<Order> auto1 = new List<Order>(), auto2 = new List<Order>();

            for (int i = 0; i < orderList.Length / 2; i++)
            {
                if (orderList[i].freq > 1)
                {
                    orderFreqs.Add(orderList[i].id, new Tuple<int, float>(orderList[i].freq, orderList[i].ledigingsDuur * orderList[i].aantContainers));
                    for (int j = 0; j < orderList[i].freq; j++)
                        auto1.Add(orderList[i].Clone());
                }
                else
                    auto1.Add(orderList[i].Clone());
            }
            for (int i = orderList.Length / 2; i < orderList.Length; i++)
            {
                if (orderList[i].freq > 1)
                {
                    orderFreqs.Add(orderList[i].id, new Tuple<int, float>(orderList[i].freq, orderList[i].ledigingsDuur * orderList[i].aantContainers));
                    for (int j = 0; j < orderList[i].freq; j++)
                        auto2.Add(orderList[i].Clone());
                }
                else
                    auto2.Add(orderList[i].Clone());
            }
            return Iterate(auto1, auto2, 100000);
        }

        //gebruik ShortestTimeSort() om de 2 opgedeelde delen greedy te sorten
        //voegt ook voor orders met freq > 1 data toe aan een dict zodat er bijgehouden kan worden of deze orders goed behandeld worden
        static float StartSmart()
        {
            List<Order> auto1 = new List<Order>(), auto2 = new List<Order>();

            for (int i = 0; i < orderList.Length / 2; i++)
            {
                auto1.Add(orderList[i].Clone());
            }
            for (int i = orderList.Length / 2; i < orderList.Length; i++)
            {
                auto2.Add(orderList[i].Clone());
            }

            auto1 = ShortestTimeSort(CloneList(auto1));
            int max = auto1.Count;
            for (int i = 0; i < max; i++)
            {
                if (auto1[i].freq > 1)
                {
                    orderFreqs.Add(auto1[i].id, new Tuple<int, float>(auto1[i].freq, auto1[i].ledigingsDuur * auto1[i].aantContainers));
                    for (int j = 1; j < auto1[i].freq; j++)
                        auto1.Add(auto1[i].Clone());
                }
            }

            auto2 = ShortestTimeSort(CloneList(auto2));
            max = auto2.Count;
            for (int i = 0; i < max; i++)
            {
                if (auto2[i].freq > 1)
                {
                    orderFreqs.Add(auto2[i].id, new Tuple<int, float>(auto2[i].freq, auto2[i].ledigingsDuur * auto2[i].aantContainers));
                    for (int j = 1; j < auto2[i].freq; j++)
                        auto2.Add(auto2[i].Clone());
                }
            }
            return Iterate(auto1, auto2, 100000);
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
        static float Iterate(List<Order> auto1, List<Order> auto2, int limit)
        {
            float minScore, t = 0;
            int k = 0;
            Random r = new Random();
            minScore = Eval(auto1, auto2);
            List<Order> bestAuto1 = new List<Order>(), bestAuto2 = new List<Order>();
            while (k < limit)
            {
                Tuple<List<Order>, List<Order>> newAutos = GenerateNeighbours(auto1, auto2);
                List<Order> newAuto1 = newAutos.Item1, newAuto2 = newAutos.Item2;
                float newScore = Eval(newAuto1, newAuto2);
                double p = Math.Exp(newScore - minScore) / t;
                if (newScore <= Eval(auto1, auto2))
                {
                    auto1 = CloneList(newAuto1);
                    auto2 = CloneList(newAuto2);
                    if (newScore < minScore)
                    {
                        minScore = newScore;
                        bestAuto1 = CloneList(auto1); bestAuto2 = CloneList(auto2);
                        Console.WriteLine(newScore);
                    }
                }
                //old: Math.Exp(newScore - minScore) / t > r.Next(0, 2) and limit - k > r.Next(0, 2 * limit)
                
                else if (p > r.Next(0, 2))
                {
                    auto1 = CloneList(newAuto1);
                    auto2 = CloneList(newAuto2);
                }
                k++;
                t = (float)Math.Pow(0.99, k);
                Console.WriteLine(newScore);
            }
            Console.WriteLine((bestAuto1.Count + bestAuto2.Count) + " " + removed.Count);
            Console.WriteLine("storttime " + totalStortTime);
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
        static Tuple<List<Order>,List<Order>> GenerateNeighbours(List<Order> input1, List <Order> input2)
        {
            List<Order> nb1 = CloneList(input1); List<Order> nb2 = CloneList(input2);
            Random rnd = new Random();
            int index;

            //random index            
            switch (rnd.Next(0, 10))
            {
                //swap orders binnen lijst
                case 0:
                    Swap(nb1, rnd.Next(0, nb1.Count), rnd.Next(0, nb1.Count));
                    break;
                case 1:
                    Swap(nb2, rnd.Next(0, nb2.Count), rnd.Next(0, nb2.Count));
                    break;
                //swap orders tussen de lijsten
                case 2:
                    SwapBetween(nb1, nb2, rnd.Next(0, nb1.Count), rnd.Next(0, nb2.Count));
                    break;
                //haal order uit lijst 1 en voeg toe aan lijst 2 
                case 3:
                    if (nb1.Count > 1)
                    {
                        index = rnd.Next(0, nb1.Count);
                        nb2.Add(nb1[index].Clone());
                        nb1.RemoveAt(index);
                    }
                    break;
                //omgekeerd
                case 4:
                    if (nb2.Count > 1)
                    {
                        index = rnd.Next(0, nb2.Count);
                        nb1.Add(nb2[index].Clone());
                        nb2.RemoveAt(index);
                    }
                    break;
                //swap naastgelegen buren
                case 5:
                    int swapplek = rnd.Next(0, (nb1.Count - 1));
                    Swap(nb1, swapplek, swapplek + 1);
                    break;
                case 6:
                    int swapplek2 = rnd.Next(0, (nb2.Count - 1));
                    Swap(nb2, swapplek2, swapplek2 + 1);
                    break;
                //swap 3 orders tegelijk
                case 7:
                    int swap3nb11 = rnd.Next(0, (nb1.Count - 3));
                    int swap3nb12 = rnd.Next(0, (nb1.Count - 3));
                    Swap3(nb1, swap3nb11, swap3nb12);
                    break;
                case 8:
                    int swap3nb21 = rnd.Next(0, (nb1.Count - 3));
                    int swap3nb22 = rnd.Next(0, (nb1.Count - 3));
                    Swap3(nb1, swap3nb21, swap3nb22);
                    break;
                //swap een range van 0 tot 5 orders
                case 9:
                    int amountnb1 = rnd.Next(0, 5);
                    int swaprangenb11 = rnd.Next(0, (nb1.Count - amountnb1));
                    int swaprangenb12 = rnd.Next(0, (nb1.Count - amountnb1));
                    SwapAmount(nb1, swaprangenb11, swaprangenb12, amountnb1);
                    break;
                case 10:
                    int amountnb2 = rnd.Next(0, 5);
                    int swaprangenb21 = rnd.Next(0, (nb1.Count - amountnb2));
                    int swaprangenb22 = rnd.Next(0, (nb1.Count - amountnb2));
                    SwapAmount(nb2, swaprangenb21, swaprangenb22, amountnb2);
                    break;
                    /*
                    //verwijder uit lijst 1 wordt bijgehouden met list<> removed
                    case 7:
                        if(nb1.Count > 1)
                        {
                            index = rnd.Next(0,nb1.Count);
                            removed.Add(nb1[index].Clone());
                            nb1.RemoveAt(index);
                        }
                        break;
                    //omgekeerd
                    case 8:
                        if (nb2.Count > 1)
                        {
                            index = rnd.Next(0, nb2.Count);
                            removed.Add(nb2[index].Clone());
                            nb2.RemoveAt(index);
                        }
                        break;
                    //bugged
                    //makes orders disappear   

                    case 9:
                        if (removed.Count > 0)
                        { 
                            index = rnd.Next(0, removed.Count);
                            nb1.Add(removed[index].Clone());
                            removed.RemoveAt(index);
                        }
                        break;
                    case 10:
                        if (removed.Count > 0)
                        { 
                            index = rnd.Next(0, removed.Count);
                            nb2.Add(removed[index].Clone());
                            removed.RemoveAt(index);
                        }
                        break;
                      */
            }           

            return new Tuple<List<Order>, List<Order>>(nb1, nb2);
        }
        
        static List<Order> CloneList(List<Order> input)
        {
            
            
            List<Order> newList = new List<Order>();
            foreach (Order o in input)
            {
                newList.Add(o.Clone());
            }
            return newList;
            
        }
        
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

        static Tuple<List<Order>, List<Order>> SwapBetween(List<Order> input1, List<Order> input2, int x, int y)
        {
            List<Order> output1 = CloneList(input1), output2 = CloneList(input2);
            Order temp = output1[x];
            output1[x] = output2[y];
            output2[y] = temp;
            return new Tuple<List<Order>, List<Order>>(output1, output2);
        }

        static float Eval(List<Order> input1, List<Order> input2)
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

        static float CalcCost(List<Order> input, Dictionary<int, Tuple<int,int>> timesVisited)
        {
            int day = 1;
            float cost = 0, time = 0, currentLoad = 0;
            for (int i = 0; i < input.Count - 1; i++)
            {
                float totalVolume = input[i].volume * input[i].aantContainers * 0.2f;
                int id = input[i].matrixID, nextID = input[i + 1].matrixID;
                if (time == 0)
                    time += afstanden[287, id].Item2;

                //behandel eerst het ophalen van het afval van deze locatie

                //als er meerdere keren opgehaald moet worden
                if (orderFreqs.ContainsKey(input[i].id))
                {
                    Tuple<int, float> freq = orderFreqs[input[i].id];
                    bool visitedOnce = timesVisited.ContainsKey(input[i].id);
                    Tuple<int, int> times = new Tuple<int, int>(0,0);
                    if (visitedOnce)
                        times = timesVisited[input[i].id];                    
                    //als er nog nooit is opgehaald
                    if (!visitedOnce)
                    {
                        time += input[i].ledigingsDuur;
                        currentLoad += totalVolume;
                        timesVisited.Add(input[i].id, new Tuple<int, int>(1, day));
                    }
                    //als er al een keer opgehaald is moet er gekeken worden of dit de goede dag is om weer op te halen volgens het bijbehorende ophaalpatroon
                    else if (times.Item1 != 0 && day - times.Item2 == 5 - freq.Item1 && freq.Item1 != times.Item2 || freq.Item1 == 5 && day != times.Item2)
                    {
                        time += input[i].ledigingsDuur;
                        currentLoad += totalVolume;
                        timesVisited[input[i].id] = new Tuple<int, int>(times.Item2 + 1, day);
                    }
                }
                else
                {
                    time += input[i].ledigingsDuur;
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
                    if (day == 5)
                        day = 1;
                    else
                        day++;

                    totalStortTime += toStortTime + 30;
                }
                else if (currentLoad + input[i + 1].volume * 0.2f > capacity)
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
            cost += afstanden[input[input.Count - 1].matrixID, 287].Item2;
            return cost;
        }
    }
}
