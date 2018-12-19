﻿using System;
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

        //<orderID, <freq, prevDay, timesPickedUp, strafkosten>>
        static Dictionary<int, Tuple<int, int, int, float>> orderFreqs = new Dictionary<int, Tuple<int, int, int, float>>();
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
            
            //afstanden[punt A, punt B] = (afstand, rijtijd)
            afstanden = new Tuple<int, int>[1099, 1099];
            for (int i = 0; i < bedrijvennetwerk.Length; i++)
            {
                string[] split = bedrijvennetwerk[i].Split(';');
                afstanden[int.Parse(split[0]), int.Parse(split[1])] = new Tuple<int, int>(int.Parse(split[2]), int.Parse(split[3]));
            }

            Console.WriteLine(StartSmart());
        }

        static float Start()
        {
            List<Order> auto1 = new List<Order>(), auto2 = new List<Order>();

            for (int i = 0; i < orderList.Length / 2; i++)
            {
                if (orderList[i].freq > 1)
                {
                    orderFreqs.Add(orderList[i].id, new Tuple<int, int, int, float>(orderList[i].freq, 0, 0, orderList[i].ledigingsDuur * 3));
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
                    orderFreqs.Add(orderList[i].id, new Tuple<int, int, int, float>(orderList[i].freq, 0, 0, orderList[i].ledigingsDuur * 3));
                    for (int j = 0; j < orderList[i].freq; j++)
                        auto2.Add(orderList[i].Clone());
                }
                else
                    auto2.Add(orderList[i].Clone());
            }
            return Iterate(auto1, auto2, 1000000);
        }

        static float StartSmart()
        {
            List<Order> auto1 = new List<Order>(), auto2 = new List<Order>();
            List<Order> sortedList = ShortestTimeSort(orderList);

            for (int i = 0; i < orderList.Length / 2; i++)
            {
                auto1.Add(orderList[i].Clone());
            }
            for (int i = orderList.Length / 2; i < orderList.Length; i++)
            {                
                auto2.Add(orderList[i].Clone());
            }

            auto1 = ShortestTimeSort(auto1.ToArray());
            auto2 = ShortestTimeSort(auto2.ToArray());

            foreach (Order o in auto1)
            {
                if (o.freq > 1)
                {
                    orderFreqs.Add(o.id, new Tuple<int, int, int, float>(o.freq, 0, 0, o.ledigingsDuur * 3));
                    for (int j = 1; j < o.freq; j++)
                        auto1.Add(o.Clone());
                }
            }
            foreach (Order o in auto2)
            {
                if (o.freq > 1)
                {
                    orderFreqs.Add(o.id, new Tuple<int, int, int, float>(o.freq, 0, 0, o.ledigingsDuur * 3));
                    for (int j = 1; j < o.freq; j++)
                        auto2.Add(o.Clone());
                }
            }

            return Iterate(auto1, auto2, 1000000);
        }

        static List<Order> ShortestTimeSort(Order[] input)
        {
            List<Order> sortedList = new List<Order>();
            Order best = new Order();
            int curLoc = 287, nextLoc = 287;
            int minTime, curTime;
            for (int i = 0; i < input.Length; i++)
            {
                minTime = int.MaxValue;
                foreach (Order o in orderList)
                {
                    if (o.matrixID != curLoc)
                    {
                        curTime = afstanden[curLoc, o.matrixID].Item2;
                        if (curTime < minTime)
                        {
                            minTime = curTime;
                            nextLoc = o.matrixID;
                            best = o;
                        }
                    }
                }
                curLoc = nextLoc;
                sortedList.Add(best);
            }
            return sortedList;
        }

        static float Iterate(List<Order> auto1, List<Order> auto2, int limit)
        {
            float minScore, t0 = 2, t = 0;
            int k = 0;
            Random r = new Random();
            minScore = Eval(auto1) + Eval(auto2);
            List<Order> bestAuto1 = new List<Order>(), bestAuto2 = new List<Order>();
            while (k < limit)
            {
                Tuple<List<Order>, List<Order>> newAutos = GenerateNeighbours(auto1, auto2);
                List<Order> newAuto1 = newAutos.Item1, newAuto2 = newAutos.Item2;
                float newScore = Eval(newAuto1) + Eval(newAuto2);
                if (newScore <= Eval(auto1) + Eval(auto2))
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
                else if (Math.Exp(newScore - minScore) / t > r.Next(0, 2))
                {
                    auto1 = CloneList(newAuto1);
                    auto2 = CloneList(newAuto2);
                    //Console.WriteLine(newScore);
                }
                k++;
                t = t0 * (float)Math.Pow(0.95, k);
            }

            return minScore;

        }

        static Tuple<List<Order>,List<Order>> GenerateNeighbours(List<Order> input1, List <Order> input2)
        {
            List<Order> nb1 = CloneList(input1); List<Order> nb2 = CloneList(input2);
            Random rnd = new Random();
            int index;
            for(int i = 0; i < 2; i++)
            {
                switch (rnd.Next(0, 9))
                {
                    case 0:
                        Swap(nb1, rnd.Next(0, nb1.Count), rnd.Next(0, nb1.Count));
                        break;
                    case 1:
                        Swap(nb2, rnd.Next(0, nb2.Count), rnd.Next(0, nb2.Count));
                        break;
                    case 2:
                        SwapBetween(nb1, nb2, rnd.Next(0, nb1.Count), rnd.Next(0, nb2.Count));
                        break;
                    case 3:
                        if (nb1.Count > 1)
                        {
                            index = rnd.Next(0, nb1.Count);
                            nb2.Add(nb1[index].Clone());
                            nb1.RemoveAt(index);
                        }
                        break;
                    case 4:
                        if (nb2.Count > 1)
                        {
                            index = rnd.Next(0, nb2.Count);
                            nb1.Add(nb2[index].Clone());
                            nb2.RemoveAt(index);
                        }
                        break;
                    case 5:
                        if(nb1.Count > 1)
                        {
                            index = rnd.Next(0,nb1.Count);
                            removed.Add(nb1[index]);
                            nb1.RemoveAt(index);
                        }
                        break;
                    case 6:
                        if (nb2.Count > 1)
                        {
                            index = rnd.Next(0, nb2.Count);
                            removed.Add(nb2[index]);
                            nb2.RemoveAt(index);
                        }
                        break;
                    case 7:
                        if (removed.Count > 0)
                        { 
                            index = rnd.Next(0, removed.Count);
                            nb1.Add(removed[index]);
                            removed.RemoveAt(index);
                        }
                        break;
                    case 8:
                        if (removed.Count > 0)
                        { 
                            index = rnd.Next(0, removed.Count);
                            nb2.Add(removed[index]);
                            removed.RemoveAt(index);
                        }
                        break;
                }
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
            List<Order> output = input;
            if (x != y)
            {
                Order temp = output[x];
                output[x] = output[y];
                output[y] = temp;
            }
            return output;
        }

        static Tuple<List<Order>, List<Order>> SwapBetween(List<Order> input1, List<Order> input2, int x, int y)
        {
            List<Order> output1 = input1, output2 = input2;
            Order temp = output1[x];
            output1[x] = output2[y];
            output2[y] = temp;
            return new Tuple<List<Order>, List<Order>>(output1, output2);
        }

        static float Eval(List<Order> input)
        {
            int day = 1;
            float time = 0, score = 0, currentLoad = 0;
            for (int i = 0; i < input.Count - 1; i++)
            {
                int id = input[i].matrixID, nextID = input[i + 1].matrixID;
                if (time == 0)
                    time += afstanden[287, id].Item2;

                //als er meerdere keren opgehaald moet worden
                if (orderFreqs.ContainsKey(input[i].id))
                {
                    Tuple<int, int, int, float> freqDayTimes = orderFreqs[input[i].id];
                    //als er al een keer opgehaald is moet er gekeken worden of dit de goede dag is om weer op te halen volgens het bijbehorende ophaalpatroon
                    if(freqDayTimes.Item2 != 0 && day - freqDayTimes.Item2 == 5 - freqDayTimes.Item1 && freqDayTimes.Item1 != freqDayTimes.Item3 || freqDayTimes.Item1 == 5)
                    {
                        time += input[i].ledigingsDuur;
                        currentLoad += input[i].volume * 0.2f;
                        orderFreqs[input[i].id] = new Tuple<int, int, int, float>(freqDayTimes.Item1, day, freqDayTimes.Item3 + 1, freqDayTimes.Item4);
                    }
                    //als er nog nooit is opgehaald
                    else if(freqDayTimes.Item2 == 0)
                    {
                        time += input[i].ledigingsDuur;
                        currentLoad += input[i].volume * 0.2f;
                        orderFreqs[input[i].id] = new Tuple<int, int, int, float>(freqDayTimes.Item1, day, freqDayTimes.Item3 + 1, freqDayTimes.Item4);
                    }
                }
                else
                {
                    time += input[i].ledigingsDuur;
                    currentLoad += input[i].volume * 0.2f;
                }

                float nextDestTime = afstanden[id, nextID].Item2;
                float toStortTime = afstanden[id, 287].Item2;

                //720 min per dag zijn de autos beschikbaar
                if (time + nextDestTime + afstanden[nextID, 287].Item2 + 30 >= 720)
                {
                    //terug naar stort aan het eind van de dag
                    time += toStortTime + 30;
                    score += time;
                    currentLoad = 0;
                    time = 0;
                    if (day == 5)
                        day = 1;
                    else
                        day++;
                }
                else if (currentLoad + input[i+1].volume * 0.2f > capacity)
                {
                    //naar stort en storten
                    time += toStortTime + 30;
                    currentLoad = 0;
                    //naar volgende bestemming
                    time += afstanden[287, nextID].Item2;
                }
                //storten als het onderweg kan 
                else if(nextDestTime >= toStortTime + afstanden[287, nextID].Item2)
                {
                    //naar stort en storten
                    time += toStortTime + 30;
                    currentLoad = 0;
                    //naar volgende bestemming
                    time += afstanden[287, nextID].Item2;
                }
                else
                    time += nextDestTime;                
            }
            score += afstanden[input[input.Count - 1].matrixID, 287].Item2;

            foreach(Tuple<int, int, int, float> val in orderFreqs.Values)
            {
                if (val.Item1 != val.Item3)
                    score += val.Item4;
            }
            foreach (Order o in removed)
            {
                if(!orderFreqs.ContainsKey(o.id))
                {
                    score += o.ledigingsDuur * 3;
                }
            }

            return score;
        }
    }
}
