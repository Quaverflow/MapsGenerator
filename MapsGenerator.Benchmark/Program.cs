 using System.Text;
 using MapsGenerator;
using BenchmarkDotNet.Running;

 namespace MapsGenerator.Benchmark; 

 internal class Program
 {
     private static void Main()
     {
        var _ = BenchmarkRunner.Run<MapperBenchmark>();
    }
 }