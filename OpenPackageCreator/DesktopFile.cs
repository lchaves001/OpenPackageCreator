﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenPackageCreator
{
    public class DesktopFile
    {
        private string platform = "";

        private string name = "";
        private string type = "Application";
        private string exec = "";
        private string icon = "";
        private string comment = "";
        private bool terminal = false;
        private string manual = "";
        private string categories = "";
        private string mimeTypes = "";
        private bool joystickMode = false;

        private bool withFile = false;

        private string lastError = "No error";

        public DesktopFile()
        {
        }

        public static string GetFilename(string platform)
        {
            if (platform == null || platform == "")
                return null;

            return "default." + platform + ".desktop";
        }

        public string Platform
        {
            get { return platform; }
            set { platform = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string Exec
        {
            get { return exec; }
            set
            {
                exec = value;
                if (exec.Contains("%f"))
                    withFile = true;
            }
        }

        public string Icon
        {
            get { return icon; }
            set { icon = value; }
        }

        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }

        public string Categories
        {
            get { return categories; }
            set { categories = value; }
        }

        public string MimeTypes
        {
            get { return mimeTypes; }
            set { mimeTypes = value; }
        }

        public bool Terminal
        {
            get { return terminal; }
            set { terminal = value; }
        }

        public string Manual
        {
            get { return manual; }
            set { manual = value; }
        }

        public bool JoystickMode
        {
            get { return joystickMode; }
            set { joystickMode = value; }
        }

        private string GetFilename()
        {
            return DesktopFile.GetFilename(platform);
        }

        public bool WithFile
        {
            get { return withFile; }
            set { withFile = value; }
        }

        public string LastError
        {
            get { return lastError; }
        }

        private string MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }

        public bool VerifyData()
        {
            if (platform == "")
            {
                lastError = "Platform not specified";
                return false;
            }

            if (name == "")
            {
                lastError = "Name not set";
                return false;
            }

            if (type == "")
            {
                lastError = "Type not set";
                return false;
            }

            if (exec == "")
            {
                lastError = "Executable not set";
                return false;
            }

            if (icon == "")
            {
                lastError = "Icon not set";
                return false;
            }

            if (categories == "" || categories == ";")
            {
                lastError = "No category chosen";
                return false;
            }

            return true;
        }

        public bool WriteDesktopFile(string folderPath)
        {
            string filename = GetFilename();
            if (filename == null)
                return false;

            string filenamePath = Path.Combine(new string[] { folderPath, filename });

            StreamWriter file = new StreamWriter(filenamePath);
            if (file == null)
                return false;

            file.Write("[Desktop Entry]\n");
            file.Write("Name=" + name + "\n");
            file.Write("Type=" + type + "\n");
            file.Write("Exec=" + exec + ((withFile && !exec.ToLower().Contains("%f")) ? " %f" : "") + "\n");

            string iconExt = Path.GetExtension(icon);
            if (iconExt != null && iconExt != "")
                icon = icon.Replace(iconExt, "");

            file.Write("Icon=" + icon + "\n");

            if (comment != "")
                file.Write("Comment=" + comment + "\n");

            file.Write("Terminal=" + (terminal ? "true" : "false") + "\n");

            if (categories != "")
            {
                if (!categories.EndsWith(";"))
                    categories += ";";

                file.Write("Categories=" + categories + "\n");
            }

            if (mimeTypes != "")
            {
                if (!mimeTypes.EndsWith(";"))
                    mimeTypes += ";";

                file.Write("MimeType=" + mimeTypes + "\n");
            }

            if (manual != "")
            {
                file.Write("X-OD-Manual=" + manual + "\n");
            }

            if (joystickMode)
            {
                file.Write("X-OD-NeedsJoystick=true\n");
            }

            file.Close();
            file.Dispose();

            return true;
        }

        public bool ReadDesktopFile(string folderPath, string platform)
        {
            this.platform = platform;

            try
            {
                string path = Path.Combine(new string[] { folderPath, GetFilename() });
                string line;
                StreamReader file = new System.IO.StreamReader(path);
                string firstLine = file.ReadLine();
                if (firstLine == null)
                {
                    lastError = "File empty";
                    return false;
                }
                if (firstLine.Trim() != "[Desktop Entry]")
                {
                    lastError = "Not a valid Desktop file";
                    return false;
                }

                while ((line = file.ReadLine()) != null)
                {
                    string[] split = line.Split(new char[] { '=' }, 2);
                    if (split.Length != 2)
                        continue;

                    string key = split[0].Trim().ToLower();
                    string value = split[1].Trim();

                    switch (key)
                    {
                        case "name":
                            name = value;
                            break;
                        case "type":
                            type = value;
                            break;
                        case "exec":
                            exec = value;
                            if (value.ToLower().Contains("%f"))
                                withFile = true;
                            break;
                        case "icon":
                            icon = value;
                            break;
                        case "comment":
                            comment = value;
                            break;
                        case "terminal":
                            terminal = value.ToLower() == "true";
                            break;
                        case "categories":
                            categories = value;
                            break;
                        case "mimetype":
                            mimeTypes = value;
                            break;
                        case "x-od-manual":
                            manual = value;
                            break;
                        case "x-od-needsjoystick":
                            joystickMode = value.ToLower() == "true";
                            break;
                    }
                }

                file.Close();
            }
            catch (Exception e)
            {
                lastError = "Error: " + e.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compares the current desktop file with another, to see if they have the same information or not.
        /// Does not compare executable or platform as executable obviously could be different between platforms.
        /// </summary>
        /// <returns></returns>
        public bool Compare(DesktopFile other)
        {
            if (name != other.Name)
                return false;

            if (type != other.Type)
                return false;

            if (icon != other.Icon)
                return false;

            if (comment != other.Comment)
                return false;

            if (terminal != other.Terminal)
                return false;

            if (categories != other.Categories)
                return false;

            if (mimeTypes != other.MimeTypes)
                return false;

            if (manual != other.Manual)
                return false;

            if (joystickMode != other.joystickMode)
                return false;

            return true;
        }

        public override string ToString()
        {
            return GetFilename();
        }
    }
}
