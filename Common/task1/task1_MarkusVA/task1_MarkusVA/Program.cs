using System;
using System.Collections.Generic;
using System.Linq;
using MPI;
using System.Diagnostics;
using System.IO;

namespace task1_MarkusVA
{
    class Program
    {
        /*
        static void MakeSortData()
        {
            StreamWriter fileToSort = new StreamWriter("sortdata.txt", false);
            Random rnd = new Random();
            int size = 1000000;
            for (int i = 0; i < size; i++)
            {
                fileToSort.Write(rnd.Next(size) + " ");
            }
            fileToSort.Close();
            Console.WriteLine("File is made and ready to be used");
        } */

        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Stopwatch stopWatch = null;                 // Timer for time controlling
                int rank = Communicator.world.Rank;
                int procNum = Communicator.world.Size;
                if (rank == procNum - 1)
                {
                    Console.WriteLine("Hello, this program will now take the file 'sortData.txt' form it's directory, " +
                    "sort it with a regular set of samples, and write into the file 'results.txt.\nNow say hi to all the processes'");
                    stopWatch = new Stopwatch();
                    stopWatch.Start();
                }
                Console.WriteLine("Hello from process " + rank);
                int spaceCounter = 1;
                int[] indexies = new int[procNum];

                // Data distribution phase - each process obtains borders of it's oun data part

                if (rank == 0)
                {                                                   // There we will look through the file and get indexies
                    if (!File.Exists("sortdata.txt"))               // of data parts (for each process)
                    {
                        Console.WriteLine("No Data File");
                        Communicator.world.Abort(0);
                        return;
                    }
                    StreamReader sortData = new StreamReader("sortdata.txt");
                    spaceCounter = 0;
                    while (!sortData.EndOfStream)
                    {
                        spaceCounter += Convert.ToInt32(sortData.Read() == 32);
                    }
                    sortData.Close();
                    if (spaceCounter == 0)
                    {
                        Console.WriteLine("Data is empty or unable to read");
                        Communicator.world.Abort(0);
                        return;
                    }
                    int divider = spaceCounter;
                    if (procNum > 1)
                        divider = divider / (procNum - 1);
                    for(int i = 0; i < procNum - 1; i++)
                    {
                        indexies[i] = divider * i;
                    }
                    indexies[procNum - 1] = spaceCounter + 1;
                }
                Communicator.world.Broadcast(ref indexies, 0);              // Sending indexies of parts to each process
                Communicator.world.Barrier();
                int mIndex = 0;
                int[] dataToSend = new int[] { };
                if (rank != 0)
                {

                    // Reading phase - each "worker" process obtains it's own data part

                    Console.WriteLine("Process " + rank + " is doing reading phase");

                    StreamReader sortData = new StreamReader("sortdata.txt");
                    string part = "";
                    while (spaceCounter <= indexies[rank - 1] & !sortData.EndOfStream)
                        spaceCounter += Convert.ToInt32(sortData.Read() == 32);
                    int i;
                    part = sortData.ReadToEnd();
                    for (i = 0; i < part.Length; i++)
                    {
                        if (part[i] == ' ')
                            spaceCounter++;
                        if (spaceCounter > indexies[rank])
                            break;
                    }
                    sortData.Close();
                    if (part == "")
                    {
                        Console.WriteLine("Alarm from process " + rank + ", unable to read my part");
                        Communicator.world.Abort(0);
                        return;
                    }
                    int[] myData = part.Substring(0, i).Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).Select(int.Parse).ToArray();
                    Array.Sort(myData);
                    Communicator.world.Barrier();

                    // Backbone elements gathering phase - each process collects it's own collection and send it to the main process (0)

                    Console.WriteLine("Process " + rank + " is doing backbone elements gathering phase");

                    Communicator.world.Broadcast(ref mIndex, 0);
                    Communicator.world.Barrier();
                    List<int> backboneElements = new List<int>();
                    try
                    {
                        for (i = 0; i < myData.Length; i += mIndex)
                        {
                            backboneElements.Add(myData[i]);
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        Console.WriteLine("Process " + rank + " is out of mem");
                        Communicator.world.Abort(0);
                        return;
                    }
                    Communicator.world.Gather(backboneElements, 0);
                    backboneElements.Clear();

                    // Threshold elements gathering phase - threshold elements are required for the interprocessor data transmission

                    Console.WriteLine("Process " + rank + " is doing threshold elements gathering phase");

                    List<int> thresholdElements = new List<int>();
                    Communicator.world.Broadcast(ref thresholdElements, 0);
                    Communicator.world.Barrier();
                    int lowerBound = 0;
                    if (rank != 1)
                    {
                        while (myData[lowerBound] <= thresholdElements[rank - 1] && lowerBound < myData.Length - 1)
                            lowerBound++;
                    }
                    int upperBound = lowerBound;
                    while (myData[upperBound] <= thresholdElements[rank] && upperBound < myData.Length - 1)
                        upperBound++;
                    if (rank == procNum - 1)
                        upperBound++;

                    // Process communicationn and sorting phase - Each "worker" process keeps his part of the data for himself
                    // and sends other data directly to the appropriate process

                    Console.WriteLine("Process " + rank + " is begginning process communication and sorting phase");
                    List<int> finalDataPart = new ArraySegment<int>(myData, lowerBound, upperBound - lowerBound).ToList();
                    int[][] takenData;
                    for (i = 1; i < procNum; i++)
                    {
                        if (rank != i)
                        {
                            lowerBound = 0;
                            if (i != 1)
                            {
                                while (myData[lowerBound] <= thresholdElements[i - 1] && lowerBound < myData.Length - 1)
                                    lowerBound++;
                            }
                            upperBound = lowerBound;
                            while (myData[upperBound] <= thresholdElements[i] && upperBound < myData.Length - 1)
                                upperBound++;
                            if (i == procNum - 1)
                                upperBound++;
                            dataToSend = new ArraySegment<int>(myData, lowerBound, upperBound - lowerBound).ToArray();
                        }
                        else
                        {
                            takenData = new int[][] { };
                            dataToSend = new int[] { };
                        }
                        takenData = Communicator.world.Gather(dataToSend, i);
                        if (rank == i)
                        {
                            foreach (int[] element in takenData)
                            {
                                finalDataPart.AddRange(element);
                            }
                        }
                    }
                    thresholdElements.Clear();
                    finalDataPart.Sort();
                    Communicator.world.Barrier();

                    // Result recording phase - main process creates the file and gives the command, and all processes take turns
                    // starting to write their parts to a file

                    Console.WriteLine("Process " + rank + " is doing result recording phase");

                    Communicator.world.Receive<int>(rank - 1, rank);
                    StreamWriter resultFile = new StreamWriter("results.txt", true);
                    resultFile.Write(string.Join(" ", finalDataPart));
                    resultFile.Write(" ");
                    resultFile.Close();
                    finalDataPart.Clear();
                    if (rank < procNum - 1)
                        Communicator.world.Send(1, rank + 1, rank + 1);
                    if (rank == procNum - 1)                            // Time control - made by the last working process
                    {
                        stopWatch.Stop();
                        TimeSpan ts = stopWatch.Elapsed;
                        string elapsedTime = string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        Console.WriteLine("Time elapsed: " + elapsedTime);
                    }
                }
                else
                {
                    Communicator.world.Barrier();

                    // Backbone elements gathering phase - each process collects it's own collection and send it to the main process (0)

                    mIndex = spaceCounter / (procNum * procNum);
                    if (mIndex < 1)
                        mIndex = 1;
                    Communicator.world.Broadcast(ref mIndex, 0);
                    Communicator.world.Barrier();
                    List<int> backboneElements = new List<int>();
                    foreach (List<int> element in Communicator.world.Gather(backboneElements, 0))
                        backboneElements.AddRange(element);
                    backboneElements.Sort();

                    // Threshold elements gathering phase - threshold elements are required for the interprocessor data transmission

                    List<int> thresholdElements = new List<int> { 0 };
                    int lowerBound = procNum / 2 - 1;
                    for (int i = procNum + lowerBound; i < backboneElements.Count; i += procNum)
                    {
                        thresholdElements.Add(backboneElements[i]);
                        if (thresholdElements.Count >= procNum - 1)
                            break;
                    }
                    thresholdElements.Add(int.MaxValue);
                    Communicator.world.Broadcast(ref thresholdElements, 0);
                    Communicator.world.Barrier();
                    backboneElements.Clear();
                    thresholdElements.Clear();

                    // Process communicationn and sorting phase - Each "worker" process keeps his part of the data for himself
                    // and sends other data directly to the appropriate process

                    for (int i = 1; i < procNum; i++)
                    {
                        Communicator.world.Gather(dataToSend, i);
                    }

                    Communicator.world.Barrier();

                    // Result recording phase - main process creates the file and gives the command, and all processes take turns
                    // starting to write their parts to a file

                    File.CreateText("results.txt").Dispose();
                    if (procNum > 1)
                        Communicator.world.Send(1, rank + 1, rank + 1);
                }
            }
            return;
        }
    }
}
