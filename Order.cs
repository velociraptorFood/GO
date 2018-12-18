using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GO
{
    class Order
    {
        public string plaats;
        public int id, freq, aantContainers, volume, matrixID, x, y;
        public float ledigingsDuur;

        public Order Clone()
        {
            Order o = new Order();
            o.plaats = plaats;
            o.id = id;
            o.freq = freq;
            o.aantContainers = aantContainers;
            o.volume = volume;
            o.matrixID = matrixID;
            o.x = x; o.y = y;
            o.ledigingsDuur = ledigingsDuur;
            return o;
        }
    }
}
