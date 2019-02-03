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
        public bool stort = false;

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
            o.stort = stort;
            return o;
        }

        public string Printorder()
        {
            string output = "plaats:" + plaats + " id:" + id + " freq:" + freq + " aantContainers:" + aantContainers + " volume:" + volume + " matrixID:" + matrixID + " ledigingsduur:" + ledigingsDuur + " x:" + x + " y:" + y;
            return output;
        }
    }
}
