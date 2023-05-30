using Cosmos.HAL;
using Cosmos.Core;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Xml.Linq;

namespace HatchOS
{
    internal class BackupShell
    {
        public static void Init()
        {
            string input = "";
            string LastCommand = "";
            string CurrentCommand = "";
            byte CurrentChar = new();
            bool ListContainsInput = false;
            bool KeepTerminalOpen = true;
            int CursorPositionX = 0;
            int HistoryIndex = 1;
            List<string> CommandHistory = new();
            List<string> Arguments = new();

            SerialPort.Enable(COMPort.COM1, BaudRate.BaudRate9600);

            SerialPort.Send((char)0xAE, COMPort.COM1 + 7);
            if (SerialPort.Receive(COMPort.COM1 + 7) != 0xAE)
            {
                //UseSerial = false;
            }

            SerialPort.SendString("\n\r[== HatchOS Serial Console ==]");
            while (KeepTerminalOpen)
            {
                input = "";
                SerialPort.SendString("\n\r(" + Directory.GetCurrentDirectory() + ") >> ");

                while (true)
                {
                    CurrentChar = SerialPort.Receive();
                    Console.WriteLine("Recieved byte: " + CurrentChar + " || (str='" + (char)CurrentChar + "')");

                    if (CurrentChar == 13)
                    {
                        ListContainsInput = false;
                        Arguments.Clear();
                        if (!string.IsNullOrWhiteSpace(input) && !string.IsNullOrEmpty(input))
                        {
                            for (int i = 0; i < CommandHistory.Count; i++)
                            {
                                if (CommandHistory[i] == input)
                                {
                                    ListContainsInput = true;
                                    CommandHistory.RemoveAt(i);
                                    CommandHistory.Add(input);
                                }
                            }

                            if(!ListContainsInput)
                                CommandHistory.Add(input);

                            LastCommand = input;
                            HistoryIndex = CommandHistory.Count;
                            CurrentCommand = input.Split(' ')[0];
                            foreach (var arg in input.Split(' '))
                            {
                                Arguments.Add(arg);
                            }

                            Arguments.RemoveAt(0);
                        }
                        else
                        {
                            CurrentCommand = "";
                        }

                        CursorPositionX = 0;
                        input = "";
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
                            input = input.Insert(CursorPositionX, Convert.ToChar(CurrentChar).ToString()); // The reason I'm using Convert.ToChar and then ToString is because of some funky cosmos shit, if I just use ToString, it'll output numbers, for some reason
                            SerialPort.SendString((char)CurrentChar + input.Substring(CursorPositionX + 1)); // This code contains a bug (The cursor stays in the same position). This makes appending text annoying, and I'm working on fixing it.

                            for (int i = 0; i < (input.Length - CursorPositionX); i++)
                                SerialPort.Send('\b');

                            CursorPositionX--;
                        }

                        CursorPositionX++;
                    }

                    if (CurrentChar == 27)
                    {
                        SerialPort.Receive();
                        byte data = SerialPort.Receive();
                        if (data == 'A' && CommandHistory.Count > 0 && HistoryIndex > 0)
                        {
                            SerialPort.SendString("\r" + new string(' ', 80));
                            SerialPort.SendString("\r(" + Directory.GetCurrentDirectory() + ") >> ");

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
                            SerialPort.SendString("\r" + new string(' ', 80));
                            SerialPort.SendString("\r(" + Directory.GetCurrentDirectory() + ") >> ");

                            if(HistoryIndex < CommandHistory.Count - 1)
                            {
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
                            else
                            {
                                CursorPositionX = 0;
                                CurrentCommand = "";
                                input = "";
                                HistoryIndex = CommandHistory.Count;
                            }
                        }

                        if (data == 'D' && CursorPositionX > 0)
                        {
                            CursorPositionX--;
                            SerialPort.SendString("\b");
                        }

                        if (data == 'C' && CursorPositionX < input.Length)
                        {
                            CursorPositionX++;
                            if (CursorPositionX > 0)
                            {
                                SerialPort.SendString(input[CursorPositionX - 1].ToString());
                            }
                            else
                            {
                                SerialPort.SendString(input[CursorPositionX - 1].ToString() + input[CursorPositionX].ToString());
                            }
                        }

                        if (data == '1' && CursorPositionX > 0)
                        {
                            for (int i = 0; i < input.Length; i++)
                                SerialPort.Send('\b');

                            CursorPositionX = 0;
                        }

                        else if (data == '4' && CursorPositionX < input.Length)
                        {
                            for (int i = 0; i < input.Length; i++)
                                SerialPort.Send(input[i]);

                            CursorPositionX = input.Length;
                        }
                    }
                }

                switch (CurrentCommand)
                {
                    case "echo":
                        SerialPort.SendString("\n\r");
                        foreach(var arg in Arguments)
                            SerialPort.SendString(arg);

                        break;

                    case "mem":
                        SerialPort.SendString($"\n\rMemory usage: {GCImplementation.GetUsedRAM() / 1024 + "/" + CPU.GetAmountOfRAM() * 1024 + " KB"}");
                        break;

                    case "keyinfo":
                        while (true)
                        {
                            SerialPort.SendString($"\n\rKey code: {(char)SerialPort.Receive()}");
                        }

                    case "shutdown":
                        PowerFunctions.Shutdown();
                        break;

                    case "reboot":
                        PowerFunctions.Restart();
                        break;

                    case "help":
                        SerialPort.SendString("\n\rhelp:\n\r1. format\n\r2. diskinfo\n\r3. shutdown\n\r4. reboot\n\r5. exit");
                        break;

                    case "gui":
                        Kernel.ShowGUI();
                        break;

                    case "memdump":
                        SerialPort.SendString("\n\r");
                        for (int i = 0; i < CPU.GetAmountOfRAM(); i++)
                        {
                            unsafe
                            {
                                SerialPort.SendString((*((int*)i)).ToString("X") + " ");
                            }
                        }
                        break;

                    case "cd":
                        try
                        {
                            if (Arguments.Count > 0 && !string.IsNullOrWhiteSpace(Arguments[0]))
                            {
                                var path = "";
                                foreach (var arg in Arguments)
                                {
                                    path += arg + " ";
                                }
                                path = HelperFunctions.TrimLastCharacter(path);

                                if (Directory.Exists(path))
                                {
                                    Directory.SetCurrentDirectory(path);
                                }
                                else
                                {
                                    throw new DirectoryNotFoundException("Directory \"" + path + "\" does not exist!");
                                }
                            }
                            else
                            {
                                throw new DirectoryNotFoundException("A directory name is required!");
                            }
                        }
                        catch(Exception ex)
                        {
                            SerialPort.SendString("\n\r[CD_ERROR] >> " + ex.Message + "\n\r");
                        }

                        break;

                    case "ls":
                        try
                        {
                            foreach (var DirName in Directory.GetDirectories(Directory.GetCurrentDirectory()))
                            {
                                SerialPort.SendString("\n\r[DIR] " + DirName);
                            }

                            foreach (var FileName in Directory.GetFiles(Directory.GetCurrentDirectory()))
                            {
                                SerialPort.SendString("\n\r[FILE] " + FileName);
                            }
                        }
                        catch(Exception ex)
                        {
                            SerialPort.SendString("\n\r[LS_ERROR] >> " + ex.Message + "\n\r");
                        }

                        break;

                    case "mkdir":
                        try
                        {
                            if (Arguments.Count > 0 && !string.IsNullOrWhiteSpace(Arguments[0]))
                            {
                                SerialPort.SendString("\n\rMaking directory \"" + Arguments[0] + "\"...");
                                Directory.CreateDirectory(Arguments[0]);
                            }
                            else
                            {
                                throw new Exception("A directory name must be specified!");
                            }
                        }
                        catch (Exception ex)
                        {
                            SerialPort.SendString("\n\r[MKDIR_ERROR] >> " + ex.Message + "\n\r");
                        }
                        break;

                    case "format":
                        try
                        {
                            var disk = 0;
                            var ans = "";
                            if (Arguments.Count > 0 && !string.IsNullOrWhiteSpace(Arguments[0]))
                            {
                                disk = Convert.ToInt32(Arguments[0]);
                            }
                            else
                            {
                                SerialPort.SendString("\n\rEnter disk number >> ");
                                disk = Convert.ToInt32(SerialPort.Receive()) - 48;
                                SerialPort.SendString(disk.ToString());
                            }
                            if (!(Arguments.Count > 1 && Arguments[1] == "--no-prompt"))
                            {
                                SerialPort.SendString("\n\rAre you sure you want to format disk #" + disk.ToString() + "?\n\rALL DATA WILL BE LOST! (Y/n) >> ");
                                ans = Convert.ToChar(SerialPort.Receive()).ToString();
                                SerialPort.SendString(ans);
                            }
                            else
                            {
                                ans = "Y";
                            }

                            if (ans == "Y")
                            {
                                SerialPort.SendString("\n\rFormatting disk #" + disk + "... (THIS MAY TAKE SOME TIME)");

                                if (Kernel.vfs.GetDisks().Count <= 0 && disk > Kernel.vfs.GetDisks().Count)
                                {
                                    SerialPort.SendString("\n\rInvalid disk number!");
                                }

                                else if (Kernel.vfs.GetDisks().Count == 0)
                                {
                                    SerialPort.SendString("\n\rThere are no disks installed.");
                                }

                                else
                                {
                                    if (Kernel.vfs.GetDisks()[disk].Partitions.Count <= 0)
                                    {
                                        SerialPort.SendString("\n\r[FORMAT_WARN] >> The disk contains no partitions! One will be created now.");
                                        Kernel.vfs.GetDisks()[disk].CreatePartition(512);
                                        break;
                                    }

                                    for (int i = 0; i < Kernel.vfs.GetDisks()[disk].Partitions.Count; i++)
                                    {
                                        SerialPort.SendString("\n\rFormatting partition " + (i + 1) + "/" + PrismAPI.Filesystem.FilesystemManager.VFS.GetDisks()[disk].Partitions.Count + "...");
                                        Kernel.vfs.GetDisks()[disk].FormatPartition(i, "FAT32", false);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SerialPort.SendString("\n\r[FORMAT_ERROR] >> " + ex.Message + "\n\r");
                        }                        

                        break;

                    case "pwd":
                        try
                        {
                            if(Arguments.Count > 0)
                            {
                                Directory.SetCurrentDirectory(Arguments[0]);
                            }
                        }
                        catch(Exception ex)
                        {
                            SerialPort.SendString("\n\r[PWD_ERROR] >> " + ex.Message + "\n\r");
                        }
                        break;

                    case "diskinfo":
                        try
                        {
                            if (Kernel.vfs.GetDisks().Count == 0)
                            {
                                SerialPort.SendString("\n\rThere are no disks installed.\n\r");
                                break;
                            }

                            SerialPort.SendString("\n\rThere are " + Kernel.vfs.GetDisks().Count + " installed disks.");
                            foreach (var dsk in Kernel.vfs.GetDisks())
                            {
                                SerialPort.SendString("\n\r" + Kernel.vfs.GetDisks().IndexOf(dsk) + ": ");
                                foreach(var partition in dsk.Partitions)
                                {
                                    SerialPort.SendString("\n\r    Root path: " + partition.RootPath + "\n\r    Size: " + dsk.Size + "\n\r");
                                }
                            }
                                
                        }
                        catch (Exception ex)
                        {
                            HelperFunctions.DisplayConsoleError("\n\r[DSK_ERROR] >> " + ex.Message + "\n\r");
                        }
                        break;

                    case "exit":
                        SerialPort.SendString("\n\r[INFO] >> Exitting serial terminal...");
                        KeepTerminalOpen = false;
                        Kernel.Debug = false;
                        break;

                    case "":
                        break;

                    default:
                        SerialPort.SendString("\n\rInvalid command: " + "'" + CurrentCommand + "'\n\r");
                        break;
                }

                HelperFunctions.CallGarbageCollector();
            }
        }
    }
}
