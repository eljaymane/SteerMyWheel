using SteerMyWheel.Model;
using SteerMyWheel.Reader;
using System;
using System.IO;

namespace SteerMyWheel
{
    internal class Program
    {
 //args[0] => cron file
        static void Main(string[] args)
        {

            CronReader _reader = new CronReader("C:/scripts.txt", new Host("PRDFRTAPP901", "PRDFRTAPP901", 22, "KCH-FRONT", "Supervision!"));
            _reader.Read();

            
        }
    }
}
