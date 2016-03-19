using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.IO.Ports;

namespace iSOBOT
{
    class Program
    {
        static void Main(string[] args)
        {
            Isobot bot = new Isobot(38400, "COM3");

            while (true)
            {
                bot.SendCommand(Commands.WalkForward, Channel.A, true);
                bot.SendCommand(Commands.GetupBack, Channel.B, false);
                Thread.Sleep(3000);
                bot.SendCommand(Commands.StopRepeating, Channel.A, false);
                Thread.Sleep(10000);
            }


        }
    }
       
}
