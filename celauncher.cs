using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace CELauncher
{
    static class Launcher
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Prepare some variables
            string target = "";
            string workingdir = "";
            string startParams = "";

            // See if we can find the location in registry first
            RegistryKey srcDisk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Take2\Codename Eagle\SrcDisk");
            if (srcDisk != null)
            {
                // Found it, get the value
                workingdir = (string) srcDisk.GetValue("Path");
                
                // Key existed, but not the value. Reset working dir to a string so it doesn't fail.
                if (workingdir == null)
                {
                    workingdir = "";
                }
            }
            
            if (workingdir == "")
            {
                // No registry key found, look for it in location.txt
                // Surround it with a try block to make sure it doesn't crash if the file is deleted/unreadable
                try
                {
                    TextReader tr = new StreamReader(System.Windows.Forms.Application.StartupPath + "\\location.txt");
                    workingdir = tr.ReadLine();
                    tr.Close();
                }
                catch
                {
                    // Let it go, we'll error out further down
                }
            }

            // Append ce.exe to the working dir to create the full path to the game
            target = workingdir.TrimEnd('\\') + @"\ce.exe";

            // Verify the command line arguments
            if (args.Length == 0 || !args[0].StartsWith("cneagle"))
            {
                System.Windows.Forms.MessageBox.Show("CE Launcher must be given an address to connect to.", "Error during launch");
                return;
            }

            // Obtain the part of the protocol we're interested in
            string address = Regex.Match(args[0], "(?<=^cneagle://)\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}:\\d+").Value;

            // Create a new process for launching CE
            Process ce = new Process();

            // Needed for CE to find lobby.exe
            ce.StartInfo.WorkingDirectory = workingdir;

            // Filename and arguments
            ce.StartInfo.FileName = target;
            ce.StartInfo.Arguments = startParams + "+connect " + address;
            
            // Try to launch Codename Eagle
            try
            {
                ce.Start();
            }
            catch (Exception)
            {
                // Failed to launch, assume we can't find it
                System.Windows.Forms.MessageBox.Show("Couldn't locate your Codename Eagle installation directory.\nUpdate location.txt with your Codename Eagle installation directory.", @"Error during launch");
            }
        }
    }
}
