using System;
using System.Threading;

namespace TryUlid
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Ulid.MaxValue.ToString());
        }
    }
}