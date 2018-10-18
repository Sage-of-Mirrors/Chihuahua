using System;
using System.IO;
using System.Text;
using Chihuahua;

namespace Beverly
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] file_data;

            using (FileStream reader = new FileStream(args[0], FileMode.Open))
            {
                file_data = new byte[reader.Length];
                reader.Read(file_data, 0, (int)reader.Length);
            }

            Event ev = new Event();
            ev.LoadBinary(file_data);

            byte[] test = ev.WriteText();
            string blah = Encoding.ASCII.GetString(test);
            System.Console.Write(blah);
        }
    }
}
