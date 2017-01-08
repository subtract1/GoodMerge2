using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace GoodMerge {
    public class GoodMerge {

        private static void SecurityTest() { new Process(); }
        private static Options options;
        
        [STAThread]
        public static void Main(string[] args) {
            try { SecurityTest(); }
            catch {
                MessageBox.Show(options.Strings[1], options.Strings[0]+" - "+options.Strings[0], MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            options = new Options();
            if (!options.Initialize()) return;
            Start(args);
        }

        private static void Start(string[] args) {
            #region <GUI>
            if (args.Length==0) {
                SettingsDialog settings = new SettingsDialog(options);
                while (true) {
                    if (settings.ShowDialog()!=DialogResult.OK) { options.Save(); return; }
                    LogWindow log = new LogWindow(options);
                    log.ShowDialog();

                    foreach (FileInfo file in new DirectoryInfo(options.WorkingFolder).GetFiles("7z*.tmp")) {
                        try { file.Delete(); }
                        catch { }
                    }
                    settings = new SettingsDialog(options);
                }
            }
            #endregion
            #region <CLI>
            else if (args.Length==1) {
                (new CLIHelpWindow(options)).ShowDialog();
            }
            else {
                options.SetName=args[0];
                options.Version=args[1];
                options.TestMode=false;

                if (options.HaveFile.Equals("")) options.HaveFile=options.CurrentFolder+options.SetName+"Have.txt";
                if (options.SourceFolder.Equals("")) options.SourceFolder=options.CurrentFolder+options.SetName+"ren\\";
                if (options.OutputFolder.Equals("")) options.OutputFolder=options.CurrentFolder+options.SetName+"Merge\\";

                int loop;

                for (loop=2; loop<args.Length; loop++) {
                    if (args[loop].Equals("pm")) options.TestMode=true;
                    else if (args[loop].Equals("ubp")) options.Background=true;
                    else if (args[loop].Equals("sca")) options.Arguments=true;
                    else if (args[loop].Equals("dsf")) options.DeleteFiles=true;
                    else if (args[loop].Equals("min")) options.Minimal=true;
                    else if (args[loop].StartsWith("oc=") || args[loop].StartsWith("sc=")) {
                        string type=args[loop].Substring(3).ToLower();
                        if (!type.Equals("7z") && !type.Equals("zip") && !type.Equals("rar") && !type.Equals("ace")) { doError(options.Strings[3]+type); return; }
                        if (args[loop][0]=='o') options.OutputCompression=type;
                        else options.SourceCompression=type;
                    }
                    else if (args[loop].StartsWith("mr=")) {
                        try { options.SetDesiredRAM(Int32.Parse(args[loop].Substring(3))); }
                        catch { doError(options.Strings[4]+args[loop]); return; }
                    }
                    else if (args[loop].StartsWith("sf=") || args[loop].StartsWith("of=") || args[loop].StartsWith("wf=")) {
                        string dir;
                        if (args[loop].Substring(2).Equals("=.")) dir = options.CurrentFolder;
                        else if (args[loop].Length>4 && args[loop][4]==':') dir = args[loop].Substring(3)+"\\";
                        else if (args[loop].Substring(1).StartsWith("=\\")) dir = options.CurrentFolder.Substring(0,2)+args[loop].Substring(3)+"\\";
                        else dir = options.CurrentFolder+args[loop].Substring(3)+"\\";
                        if (args[loop][0]=='s') options.SourceFolder=dir;
                        else if (args[loop][0]=='o') options.OutputFolder=dir;
                        else if (args[loop][0]=='w') options.WorkingFolder=dir;
                    }
                    else if (args[loop].StartsWith("hf=")) {
                        options.HaveFile=args[loop].Substring(3);
                    }
                    else { doError(options.Strings[5]+args[loop]); return; }
                }
                if (!Directory.Exists(options.SourceFolder) && !options.TestMode) { doError(options.Strings[6]+"\n("+options.SourceFolder+")"); return; }
                if (!Directory.Exists(options.WorkingFolder) && !options.TestMode) { doError(options.Strings[7]+"\n("+options.WorkingFolder+")"); return; }
                if (!File.Exists(options.HaveFile)) { doError(options.Strings[8]+"\n("+options.HaveFile+")"); return; }
                if ((options.SourceCompression.Equals("rar") || options.OutputCompression.Equals("rar")) && options.Rar.Equals("none")) { doError(options.Strings[9]+" \"Rar.exe\""); return; }
                if ((options.SourceCompression.Equals("ace") || options.OutputCompression.Equals("ace")) && options.Ace.Equals("none")) { doError(options.Strings[9]+" \"ace32.exe\""); return; }
                if ((options.SourceCompression.Equals("7z") || options.SourceCompression.Equals("zip") || options.OutputCompression.Equals("7z") || options.OutputCompression.Equals("zip")) && options.SevenZip.Equals("none")) { doError(options.Strings[9]+" \"7za.exe\""); return; }
                if (options.OutputCompression.Equals("7z") && options.MaxDict==0) { doError(options.Strings[10]); return; }

                LogWindow log = new LogWindow(options);
                log.ShowDialog();
                options.Save();

                foreach (FileInfo file in new DirectoryInfo(options.WorkingFolder).GetFiles("7z*.tmp")) {
                    try { file.Delete(); }
                    catch { }
                }
            }
            #endregion
        }

        private static void doError(string text) {
            MessageBox.Show(text, options.Strings[0]+" - "+options.Strings[1], MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
