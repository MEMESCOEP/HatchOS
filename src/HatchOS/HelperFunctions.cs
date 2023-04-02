/* DIRECTIVES */
using Cosmos.HAL;
using System;
using System.Drawing;
using System.Collections.Generic;

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
        public static void ChangeMouseCursor(PrismGraphics.Image NewImage)
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
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
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

        // Display a message in console mode and send the same message over serial
        public static void DisplayConsoleMsg(string message)
        {
            Console.WriteLine(message);
            SerialPort.SendString(message + "\n", SerialPort.COM1);
        }

        // Add a byte to a byte array
        public static byte[] AddByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 1);
            newArray[0] = newByte;
            return newArray;
        }

        // Remove the last character from a string
        public static string TrimLastCharacter(string str)
        {
            if(!string.IsNullOrEmpty(str))
            {
                return str.TrimEnd(str[str.Length - 1]);
            }
            return null;
        }

        // Remove the last n characters from a string
        public static string RemoveCharsFromEnd(string str, int CharNum)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return str.Substring(0, str.Length - CharNum);
            }
            return null;
        }

        // Find the number of times a char occurs in a string
        public static int StringContainsCharNTimes(string StrToTest, char CharToTest)
        {
            int Count = 0;
            foreach(char c in StrToTest)
            {
                if(c == CharToTest)
                {
                    Count++;
                }
            }
            return Count;
        }
    }
}
