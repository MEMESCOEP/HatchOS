using Cosmos.HAL;
using System;
using System.Collections.Generic;

namespace HatchOS
{
    internal class BackupShell
    {
        public static void Init()
        {
            string input = "";
            string LastCommand = "";
            byte CurrentChar = new();
            bool UseSerial = true;
            bool KeepTerminalOpen = true;
            int CursorPositionX = 0;
            int HistoryIndex = 1;
            List<string> CommandHistory = new();

            SerialPort.Enable(SerialPort.COM1);

            SerialPort.Send((char)0xAE, SerialPort.COM1 + 7);
            if (SerialPort.Receive(SerialPort.COM1 + 7) != 0xAE)
            {
                UseSerial = false;
            }

            Console.WriteLine("\n\n[== HatchOS Serial Shell ==]\nPlease use the serial port.");
            SerialPort.SendString("\r\n[== HatchOS Serial Shell ==]");

            while (KeepTerminalOpen)
            {
                input = "";
                SerialPort.SendString("\r\n>> ");

                while (true)
                {
                    if (UseSerial)
                    {
                        CurrentChar = SerialPort.Receive();
                        Console.WriteLine("Recieved byte: " + CurrentChar + " || (str='" + (char)CurrentChar + "')");
                    }

                    if (CurrentChar == 13)
                    {
                        if (!string.IsNullOrWhiteSpace(input) && !string.IsNullOrEmpty(input))
                        {
                            CommandHistory.Add(input);
                            LastCommand = input;
                            HistoryIndex = CommandHistory.Count;
                        }

                        CursorPositionX = 0;
                        break;
                    }

                    if (CurrentChar == 127 && input.Length > 0 && CursorPositionX > 0)
                    {
                        input = input.Remove(CursorPositionX - 1, 1);

                        if (CursorPositionX != input.Length + 1)
                        {
                            for (int i = 0; i < input.Substring(CursorPositionX).Length + 1; i++)
                                SerialPort.Send(' ');

                            for (int i = 0; i < input.Length + 1; i++)
                                SerialPort.Send('\b');

                            for (int i = 0; i < input.Length + 1; i++)
                                SerialPort.Send(' ');

                            for (int i = 0; i < input.Length + 1; i++)
                                SerialPort.Send('\b');

                            SerialPort.SendString(" \b \b");
                            SerialPort.SendString(input);

                            for (int i = 0; i < input.Length - (CursorPositionX - 1); i++)
                                SerialPort.Send('\b');
                        }
                        else
                        {
                            SerialPort.SendString("\b \b");
                        }

                        CursorPositionX--;
                        continue;
                    }

                    if (char.IsLetterOrDigit((char)CurrentChar) || char.IsPunctuation((char)CurrentChar) || char.IsWhiteSpace((char)CurrentChar))
                    {
                        if (CursorPositionX == input.Length)
                        {
                            input += Convert.ToChar(CurrentChar);
                            SerialPort.Send((char)CurrentChar);
                        }
                        else
                        {
                            input = input.Insert(CursorPositionX, Convert.ToChar(CurrentChar).ToString());
                            SerialPort.SendString((char)CurrentChar + input.Substring(CursorPositionX + 1));

                            for (int i = 0; i < input.Length - (CursorPositionX); i++)
                                SerialPort.Send('\b');

                            CursorPositionX--;
                        }

                        CursorPositionX++;
                    }

                    if (CurrentChar == 27)
                    {
                        SerialPort.Receive();
                        byte data = SerialPort.Receive();
                        if (data == 'A' && CommandHistory.Count > 0)
                        {
                            SerialPort.SendString("\r" + new string(' ', 80) + "\r>> ");

                            HistoryIndex--;

                            if (HistoryIndex < 0)
                                HistoryIndex = 0;

                            else if (HistoryIndex > CommandHistory.Count - 1)
                                HistoryIndex = CommandHistory.Count - 1;

                            else
                            {
                                input = CommandHistory[HistoryIndex];
                                CursorPositionX = input.Length;
                                SerialPort.SendString(input);
                            }
                        }

                        if (data == 'B' && CommandHistory.Count > 0)
                        {
                            SerialPort.SendString("\r" + new string(' ', 80) + "\r>> ");

                            HistoryIndex++;

                            if (HistoryIndex < 0)
                                HistoryIndex = 0;

                            else if (HistoryIndex > CommandHistory.Count - 1)
                                HistoryIndex = CommandHistory.Count - 1;

                            else
                            {
                                input = CommandHistory[HistoryIndex];
                                CursorPositionX = input.Length;
                                SerialPort.SendString(input);
                            }
                        }

                        if (data == 'D')
                        { 
                            if (CursorPositionX > 0)
                            {
                                CursorPositionX--;
                                SerialPort.SendString("\b");
                            }
                        }

                        if (data == 'C')
                        {
                            if (CursorPositionX < input.Length)
                            {
                                CursorPositionX++;
                                if (CursorPositionX > 0)
                                {
                                    SerialPort.SendString("\b" + input[CursorPositionX - 1] + input[CursorPositionX]);
                                }
                                else
                                {
                                    SerialPort.SendString(input[CursorPositionX - 1].ToString() + input[CursorPositionX].ToString());
                                }
                            }
                        }
                    }
                }

                switch (input)
                {
                    case "shutdown":
                        PowerFunctions.Shutdown();
                        break;

                    case "reboot":
                        PowerFunctions.Restart();
                        break;

                    case "help":
                        SerialPort.SendString("\n\rhelp: lol no (I'm too lazy to fill this in right now)");
                        break;

                    case "gui":
                        Kernel.ShowGUI();
                        break;

                    case "memdump":
                        SerialPort.SendString("\n\r");
                        for (int i = 0; i < Cosmos.Core.CPU.GetAmountOfRAM(); i++)
                        {
                            unsafe
                            {
                                SerialPort.SendString((*((int*)i)).ToString("X") + " ");
                            }
                        }
                        break;

                    case "format":
                        SerialPort.SendString("\n\rEnter disk number >> ");
                        var disk = SerialPort.Receive();

                        if (Kernel.vfs.GetDisks().Count > 0 && disk < Kernel.vfs.GetDisks().Count)
                        {
                            Kernel.vfs.GetDisks()[disk].FormatPartition(0, "FAT32", true);
                            SerialPort.SendString("\n\rSize: " + Kernel.vfs.GetDisks()[disk].Size.ToString());
                        }
                        else if(Kernel.vfs.GetDisks().Count == 0)
                        {
                            SerialPort.SendString("\n\rThere are no disks installed.");
                        }

                        break;

                    case "diskinfo":
                        if (Kernel.vfs.GetDisks().Count == 0)
                        {
                            SerialPort.SendString("\n\rThere are no disks installed.");
                            break;
                        }
                        foreach (var dsk in Kernel.vfs.GetDisks())
                            SerialPort.SendString(Kernel.vfs.Disks.IndexOf(dsk) + ": " + dsk.Type.ToString());
                        break;

                    case "exit":
                        SerialPort.SendString("\n\r[INFO] >> Exitting serial terminal...");
                        KeepTerminalOpen = false;
                        break;

                    case "":
                        break;

                    default:
                        SerialPort.SendString("\r\nInvalid command: " + "'" + input + "'");
                        break;
                }
            }
        }
    }
}
