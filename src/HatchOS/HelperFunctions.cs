/* DIRECTIVES */
using Cosmos.HAL;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    internal class HelperFunctions
    {
        /* FUNCTIONS */
        // Detect if a value is between two other values
        public static bool IsBetween(int Value, int Min, int Max)
        {
            try
            {
                return (Min <= Value && Value <= Max);
            }
            catch
            {
                return false;
            }
        }

        // Change the mouse cursor image
        public static void ChangeMouseCursor(Bitmap NewImage)
        {
            Kernel.Mouse = NewImage;
        }

        // Convert seconds to nanoseconds
        public static ulong SecondsToNanoseconds(ulong Seconds)
        {
            return Seconds * 1000000000;
        }

        // Convert milliseconds to nanoseconds
        public static ulong MillisecondsToNanoseconds(ulong milliseconds)
        {
            return milliseconds * 1000000;
        }

        // Move an item in a list to a new position in the list
        public static void MoveListItemToIndex<T>(List<T> list, int OldIndex, int NewIndex)
        {
            try
            {
                T item = list[OldIndex];
                list.Insert(NewIndex, item);
                list.RemoveAt(OldIndex);
            }
            catch
            {
                KernelPanic.Panic("Failed to rearrange list!", "0x76239");
            }
        }

        // Get the distance between two points
        public static double GetPointDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
        }

        // Get the difference between two numbers
        public static decimal FindDifference(decimal nr1, decimal nr2)
        {
            return Math.Abs(nr1 - nr2);
        }

        // Call the garbage collector
        public static void CallGarbageCollector()
        {
            // Call the garbage collector so we don't have as many memory leaks, which also
            // helps improve framerates
            Cosmos.Core.Memory.Heap.Collect();
        }

        // Display an error in console mode and send the same message over serial
        public static void DisplayConsoleError(string message)
        {
            Console.WriteLine(message);
            SerialPort.SendString(message + "\n", SerialPort.COM1);
        }
    }
}
