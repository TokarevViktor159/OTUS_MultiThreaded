using System.Drawing;

namespace OTUS_MultiThreaded
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[] sizes = { 100000, 1000000, 10000000 };
            Random random = new Random();

            foreach (int size in sizes)
            {
                int[] array = new int[size];
                for (int i = 0; i < size; i++)
                {
                    array[i] = random.Next(1, 10);
                }
                int sum;

                Console.WriteLine("");
                Console.WriteLine($"Размер массива {size}:");
                var watch = System.Diagnostics.Stopwatch.StartNew();
                sum = SumSequential(array);
                watch.Stop();
                Console.WriteLine($"Обычное вычисление: \t\t\t{watch.ElapsedMilliseconds} мс. Сумма {sum}");

                watch.Restart();
                sum = SumParallelThreadPool(array, 3);
                watch.Stop();
                Console.WriteLine($"Параллельное вычисление (Threads): \t{watch.ElapsedMilliseconds} мс. Сумма {sum}");

                watch.Restart();
                sum = SumParallelLINQ(array, 3);
                watch.Stop();
                Console.WriteLine($"Параллельное вычисление (LINQ): \t{watch.ElapsedMilliseconds} мс. Сумма {sum}");
            }
            Console.WriteLine("");
            Console.WriteLine("Нажмите любую клавишу...");
            Console.ReadKey();
        }

        public static int SumSequential(int[] array)
        {
            int sum = 0;
            foreach (var number in array)
            {
                sum += number;
            }
            return sum;
        }

        public static int SumParallelThreadPool(int[] array, int useThreadCount = 2)
        {
            int section = array.Length / useThreadCount;
            int[] sum = new int[useThreadCount];
            ManualResetEvent[] doneEvents = new ManualResetEvent[useThreadCount];

            for (int i = 0; i < useThreadCount; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                int index = i;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    int min = section * index;
                    int max = index + 1 == useThreadCount ? array.Length : section * (index + 1);

                    for (int j = min; j < max; j++)
                    {
                        Interlocked.Add(ref sum[index], array[j]);
                    }
                    doneEvents[index].Set();
                });
            }

            WaitHandle.WaitAll(doneEvents);
            return sum.Sum();
        }

        public static int SumParallelLINQ(int[] array, int threadsCount)
        {
            return array.AsParallel().WithDegreeOfParallelism(threadsCount).Sum();
        }
    }
}
