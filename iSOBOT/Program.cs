using System.Threading;
using System.IO;
using System;

namespace iSOBOT
{
    class Program
    {
        static void Main(string[] args)
        {
            Isobot bot = new Isobot(38400, "COM3");

            StreamReader reader = File.OpenText("commands.txt");
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                Commands cmd;
                Channel chan;
                bool repeat;
                int delay;

                string[] sections = line.Split(',');

                // Check for length
                if (sections.Length != 4)
                    continue;

                // Get command
                if (!Enum.TryParse(sections[0], out cmd))
                    continue;

                // Get Channel
                if (!Enum.TryParse(sections[1], out chan))
                    continue;

                // Get repeat
                if (sections[2] == "0")
                    repeat = false;
                else if (sections[2] == "1")
                    repeat = true;
                else
                    continue;

                // Get delay
                if (!int.TryParse(sections[3], out delay))
                    continue;

                bot.SendCommand(cmd, chan, repeat);
                Thread.Sleep(delay);

            }

            bot.SendCommand(Commands.StopRepeating, Channel.A, false);
            bot.SendCommand(Commands.StopRepeating, Channel.B, false);

        }

    }
       
}
