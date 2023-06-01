 using System.Text;
 using MapsGenerator;
using BenchmarkDotNet.Running;

 namespace MapsGenerator.Benchmark; 

 internal class Program
 {
     private static void Main(string[] _)
     {
        var summary = BenchmarkRunner.Run<MapperBenchmark>();
    }
 }