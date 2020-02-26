extern alias Ulid_1_0_0;
extern alias newUlid;
using BenchmarkDotNet.Attributes;
using OldUlid = Ulid_1_0_0::System.Ulid;
using NewUlid = newUlid::System.Ulid;

namespace PerfBenchmark.Suite
{
    //[Config(typeof(BenchmarkConfig))]
    public class MaxValue
    {

        [Benchmark]
        public int Old()
        {
            return OldUlid.MaxValue.GetHashCode();
        }
        
        [Benchmark]
        public int New()
        {
            return NewUlid.MaxValue.GetHashCode();
        }
    }
}
