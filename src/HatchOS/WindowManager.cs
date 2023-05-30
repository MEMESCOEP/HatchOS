/* DIRECTIVES */
using System;
using System.Collections.Generic;
using System.Drawing;
using static HatchOS.HelperFunctions;
using Color = PrismAPI.Graphics.Color;

/* NAMESPACES */
namespace HatchOS
{
    /* CLASSES */
    public class WindowManager
    {
        /* FUNCTIONS */
        // Create a new window with the specified location, size, colors, and title
        public static void CreateNewWindow(List<Window> WindowList, Point Location, Point Size, List<Color> Colors, string Title, bool UseGradient = false)
        {
            Window window = new Window();
            window.WindowTitle = Title;
            window.WindowColors = Colors;
            window.WindowSize = Size;
            window.WindowLocation = Location;
            Kernel.ActiveWindow = window;
            WindowList.Add(window);
            MoveListItemToIndex(WindowList, WindowList.IndexOf(window), WindowList.Count);
            DisplayConsoleMsg("[INFO] >> Created window \"" + Title + "\" with dimensions " + Size.X + "x" + Size.Y);
        }

        // Create a message box
        public static void CreateMessageBox(string Title, string Message, int MessageType, bool Silent = false)
        {
            int MSGBoxHeight = 128;
            int MSGBoxLength = 32;

            if(Message.Contains('\n'))
            {                
                for(int i = 0; i < Message.Split('\n').Length; i++)
                    MSGBoxHeight += 10;

                int MaxLength = 0;
                foreach(var part in Message.Split('\n'))
                {
                    if(part.Length > MaxLength)
                        MaxLength = part.Length;
                }

                for (int i = 0; i < MaxLength; i++)
                    MSGBoxLength += 10;
            }
            else
            {
                for (int i = 0; i < Message.Length; i++)
                    MSGBoxLength += 8;
            }

            CreateNewWindow(Kernel.WindowList, new(Kernel.ScreenWidth / 2 - MSGBoxLength / 2, Kernel.ScreenHeight / 2 - MSGBoxHeight / 2 - 40), new(MSGBoxLength, MSGBoxHeight), new List<Color> { new(System.Drawing.Color.DarkSlateGray.A, System.Drawing.Color.DarkSlateGray.R, System.Drawing.Color.DarkSlateGray.G, System.Drawing.Color.DarkSlateGray.B), Color.LightGray, Color.Black, new(255, 40, 65, 65) }, Title);
            WindowElement BodyText = new();
            WindowElement BodyImage = new();
            BodyText.ElementData = Message;
            BodyText.ElementColor = Color.Black;
            BodyText.ElementPosition = new(44 , 20);
            BodyText.ElementType = "StringElement";
            Kernel.ActiveWindow.WindowElements.Add(BodyText);
            BodyImage.ElementPosition = new(8 , MSGBoxHeight / 2 - 40);
            BodyImage.ElementType = "ImageElement";

            switch (MessageType)
            {
                case 0:
                    if (!Silent)
                        PlayAudioFromMemory(Kernel.ErrorAudio, Kernel.AudioVolume, false);

                    BodyImage.ElementData = Convert.ToBase64String(Kernel.ErrorSymbol);
                    break;

                case 1:
                    if (!Silent)
                        PlayAudioFromMemory(Kernel.ExclamationAudio, Kernel.AudioVolume, false);

                    BodyImage.ElementData = Convert.ToBase64String(Kernel.WarningSymbol);
                    break;

                case 2:
                    if (!Silent)
                        PlayAudioFromMemory(Kernel.ExclamationAudio, Kernel.AudioVolume, false);

                    BodyImage.ElementData = Convert.ToBase64String(Kernel.ExclamationSymbol);
                    break;

                case 3:
                    if (!Silent)
                        PlayAudioFromMemory(Kernel.ExclamationAudio, Kernel.AudioVolume, false);

                    BodyImage.ElementData = Convert.ToBase64String(Kernel.QuestionSymbol);
                    break;
            }
            
            Kernel.ActiveWindow.WindowElements.Add(BodyImage);
        }
    }
}
