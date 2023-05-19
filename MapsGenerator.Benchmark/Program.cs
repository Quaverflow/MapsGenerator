 using System.Text;
 using MapsGenerator;
using BenchmarkDotNet.Running;

 namespace MapsGenerator.Benchmark; 

 internal class Program
 {
     private static void Main(string[] args)
     {
        var summary = BenchmarkRunner.Run<MapperBenchmark>();
    }
 }