using System;
using System.Threading;

namespace TryUlid
{
    class Program
    {
        static void Main(string[] args)
        {
            var now = DateTimeOffset.UtcNow;
            var foo1 = Ulid.NewUlid(now);
            var foo2 = NUlid.Ulid.NewUlid(now);
        }
    }
}
