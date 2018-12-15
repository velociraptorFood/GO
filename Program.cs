using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GO
{
    class Program
    {
        static void Main()
        {
            Order[] orderList = new Order[1177];
            string input = Console.ReadLine();
            for (int i = 0; i < 1177; i++)
            {
                string[] split = input.Split(';');
                Order newOrder = new Order();
                newOrder.id = int.Parse(split[0]);                     newOrder.plaats = split[1].Trim();
                newOrder.freq = int.Parse(split[2][0].ToString());     newOrder.aantContainers = int.Parse(split[3]);
                newOrder.volume = int.Parse(split[4]);                 newOrder.ledigingsDuur = float.Parse(split[5]);
                newOrder.x = int.Parse(split[7]);                      newOrder.y = int.Parse(split[8]);
                newOrder.matrixID = int.Parse(split[6]);
                orderList[i] = newOrder;
                input = Console.ReadLine();
            }
        
            Tuple<int, int>[,] afstanden = new Tuple<int, int>[1099, 1099];
            Console.ReadLine();
            input = Console.ReadLine();
            while (!String.IsNullOrEmpty(input))
            {
                string[] split = input.Split(';');
                afstanden[int.Parse(split[0]), int.Parse(split[1])] = new Tuple<int, int>(int.Parse(split[2]), int.Parse(split[3]));
                input = Console.ReadLine();
            }
        }
    }
}
