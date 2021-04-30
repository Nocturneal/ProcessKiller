using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessKiller {
    internal class Program {
        internal bool finished = false, killed = false;
        internal string processName = "", userInput = "";

        internal Program(string[] args) {
            if (args.Length >= 1) {
                processName = args[0];
                var shouldLoop = (args.Length == 2 && args[1].ToLower().Equals("true"));
                int loops = 1;
                while (shouldLoop || loops > 0) {
                    Console.WriteLine($"Searching for process '{processName}'...");
                    var matching = Process.GetProcessesByName(processName);
                    if (matching.Length > 0) {
                        Console.WriteLine("Found matching process(es):");
                        Console.WriteLine(" - " + string.Join("\n - ", matching.Select(p => p.Id + ": " + p.MainModule.FileName)));
                        var updated = Process.GetProcessesByName(processName);
                        updated.ForEach(p => p.Kill());
                        Console.WriteLine($"Killed process(es).");
                    }
                    else {
                        Console.WriteLine($"Could not find any processes matching '{processName}'.");
                    }
                    Thread.Sleep(50);
                    --loops;
                }
                Console.WriteLine("Press ENTER to start over.");
            }
            while (!finished) {
                Console.Clear();
                Console.WriteLine("Welcome to process killer. Please type the name of the executable you want to kill.");
                processName = Console.ReadLine();
                if (processName.Equals("quit:confirm")) Environment.Exit(0);
                if (processName.StartsWith("emergency:")) {
                    var search = processName.Split(':')[1];
                    var matching = Process.GetProcessesByName(search);
                    int sleepRate = 20,
                        maxSeconds = 120, // Max two minutes, which at 20ms loop will be 6000 loops.
                        maxLoops = maxSeconds * 1000 / sleepRate;
                    int maxThreads = 20, currentThreads = 0;
                    Console.WriteLine($"Listening for process '{search}'...");
                    while (maxLoops > 0) {
                        Thread.Sleep(sleepRate);
                        --maxLoops;
                        matching = Process.GetProcessesByName(search);
                        if (matching.Any() && (currentThreads < maxThreads || !killed)) {
                            Console.WriteLine($"Process found! Spawning {maxThreads} threads to kill process...");
                            while (currentThreads < maxThreads && !killed) {
                                ++currentThreads;
                                var task = Task.Run(() => {
                                    try {
                                        if (matching.Any()) {
                                            matching.First().Kill();
                                            Console.WriteLine($"[{currentThreads}] Found {matching.Length} matching process(es), killing first one [{matching.First().Id}]...");
                                            killed = !Process.GetProcessesByName(search).Any();
                                        }
                                    }
                                    catch (Exception ex) {
                                        Console.WriteLine($"[{currentThreads}] An error occured while trying to kill process {matching.First().Id}: {ex.Message}\n");
                                    }
                                });
                                task.ContinueWith((t) => {
                                    --currentThreads;
                                });
                            }
                        }
                        else if (killed) {
                            Console.WriteLine("Assuming all processes are killed. Breaking loop. Press ENTER to continue.");
                            break;
                        }
                    }
                    if (!killed) Console.WriteLine("Max timeout reached. Please start again.");
                    while (Console.ReadKey().Key != ConsoleKey.Enter) { Thread.Sleep(20); };
                }
                else {
                    Console.WriteLine($"You've selected '{processName}' as the proces you want to kill. Is this correct? [Y/n]:");
                    userInput = Console.ReadLine();
                    if (userInput.ToLower().Contains("y")) {
                        Console.WriteLine($"Searching for process '{processName}'...");
                        var matching = Process.GetProcessesByName(processName);
                        if (matching.Length > 0) {
                            Console.WriteLine("Found matching process(es):");
                            Console.WriteLine(" - " + string.Join("\n - ", matching.Select(p => p.Id + ": " + p.MainModule.FileName)));
                            Console.WriteLine($"Press ENTER to kill process(es), or any key to cancel.");
                            if (Console.ReadKey().Key == ConsoleKey.Enter) {
                                var updated = Process.GetProcessesByName(processName);
                                updated.ForEach(p => p.Kill());
                                Console.WriteLine($"Killed process(es). Press ENTER to start over.");
                            }
                            else Console.WriteLine($"Cancelled killing process. Press ENTER to start over.");
                            while (Console.ReadKey().Key != ConsoleKey.Enter) { Thread.Sleep(20); };
                        }
                        else {
                            Console.WriteLine($"Could not find any processes matching '{processName}'. Press ENTER to start over.");
                            while (Console.ReadKey().Key != ConsoleKey.Enter) { Thread.Sleep(20); };
                        }
                    }
                }
            }
        }
        public static void Main(string[] args) => new Program(args);
    }

    internal static class Extensions {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");
            foreach (T item in source) {
                action(item);
            }
        }
    }
}
