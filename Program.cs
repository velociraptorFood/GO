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

            Console.WriteLine(Eval(Start()));
            Console.ReadLine();
        }

        static List<Order> Start()
        {
            List<Order> start = orderList.ToList();
            return start;
        }

        static float Eval(List<Order> input)
        {
            float score = 0;
            float currentLoad = 0;   
            for (int i = 0; i < input.Count - 1; i++)
            {
                score += input[i].ledigingsDuur;
                currentLoad += input[i].volume * 0.2f;

                //720 min per dag zijn de autos beschikbaar
                float nextDestTime = afstanden[input[i].matrixID, input[i++].matrixID].Item2;
                if(score % 720 + nextDestTime + afstanden[input[i++].matrixID, 287].Item2 + 30 >= 720)
                {
                    score += afstanden[input[i].matrixID, 287].Item2 + 30;
                }
                else if (currentLoad + input[i++].volume * 0.2f > capacity)
                {
                    //naar stort en storten
                    score += afstanden[input[i].matrixID, 287].Item2 + 30;
                    currentLoad = 0;
                    //naar volgende bestemming
                    score += afstanden[287, input[i++].matrixID].Item2;
                }
                else
                    score += nextDestTime;                
            }
            score += afstanden[input[input.Count - 1].matrixID, 287].Item2;
            return score;
        }
    }
}
