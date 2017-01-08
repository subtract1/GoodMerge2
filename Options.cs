using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Runtime.InteropServices; 

namespace GoodMerge {
    public class Options {
        #region <Properties>
        public StringCollection Biases;
        public StringCollection SetNames;
        public Hashtable BiasesDisplay;
        public Hashtable SourceFolders;
        public Hashtable OutputFolders;
        public Hashtable HaveFiles;
        public string SetName;
        public string Version;
        public string SourceCompression;
        public string OutputCompression;
        public bool TestMode;
        public bool Arguments;
        public bool Background;
        public bool DeleteFiles;
        public bool Minimal;
        public string CurrentFolder;
        public string WorkingFolder;
        public string ProgramFolder;
        public string SourceFolder;
        public string OutputFolder;
        public string HaveFile;
        public string SevenZip;
        public string Rar;
        public string Ace;
        public int UltraDict;
        public int MaxDict;
        public int TotalRAM;
        public int DesiredRAM;
        public string Language;
        public string[] Strings;
        public string FontName;
        public float FontSize;

        public int ScaleX;
        public int ScaleY;
        public Point MainWindowLocation;
        public Point LogWindowLocation;
        public Size LogWindowSize;
        #endregion

        public Options() { }

        [StructLayout(LayoutKind.Sequential)]   
            public struct MemoryStatus {
            public uint dwLength;
            public uint dwMemoryLoad;
            public uint dwTotalPhys;
            public uint dwAvailPhys;
            public uint dwTotalPageFile;
            public uint dwAvailPageFile;
            public uint dwTotalVirtual;
            public uint dwAvailVirtual;
        }
        [DllImport("kernel32")]
        static extern void GlobalMemoryStatus(ref MemoryStatus buf);

        public bool Initialize() {
            XmlNodeList xnl;
            CultureInfo CI = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            CurrentFolder = Environment.CurrentDirectory+"\\";
            ProgramFolder = Application.StartupPath+"\\";

            MemoryStatus ms = new MemoryStatus();				
            GlobalMemoryStatus (ref ms);
            TotalRAM=(int)(ms.dwTotalPhys/1024/1024)+1;

            XmlDocument xd= new XmlDocument();
            XmlValidatingReader xvr = new XmlValidatingReader(new XmlTextReader(Application.StartupPath+"\\Settings.xml"));
            try { xd.Load(xvr); }
            catch (Exception e) {
                MessageBox.Show(e.Message.Replace(" An error occurred at file:///", "\nFile: ").Replace(", (", "\nLine, Column: ("), "GoodMerge - XML Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally { xvr.Close(); }

            Biases = new StringCollection();
            foreach (XmlNode xn in xd.SelectNodes("/settings/biaspriority/zone/@name")) {
                Biases.Add(xn.Value);
            }

            SetName = xd.SelectSingleNode("/settings/romset/@name").Value;
            Version = xd.SelectSingleNode("/settings/romset/@version").Value;

            SourceCompression = xd.SelectSingleNode("/settings/compression/@source").Value;
            OutputCompression = xd.SelectSingleNode("/settings/compression/@output").Value;

            Arguments = xd.SelectSingleNode("/settings/misc/@arguments").Value.Equals("true");
            Background = xd.SelectSingleNode("/settings/misc/@background").Value.Equals("true");
            Minimal = xd.SelectSingleNode("/settings/misc/@minimal").Value.Equals("true");
            Language = xd.SelectSingleNode("/settings/misc/@language").Value;
            DeleteFiles = false;

            int x=0, y=0;
            try { x = Int32.Parse(xd.SelectSingleNode("/settings/windows/main/@x").Value); }
            catch { x=0; }
            try { y = Int32.Parse(xd.SelectSingleNode("/settings/windows/main/@y").Value); }
            catch { y=0; }
            MainWindowLocation = new Point(x, y);

            try { x = Int32.Parse(xd.SelectSingleNode("/settings/windows/log/@x").Value); }
            catch { x=0; }
            try { y = Int32.Parse(xd.SelectSingleNode("/settings/windows/log/@y").Value); }
            catch { y=0; }
            LogWindowLocation = new Point(x, y);

            try { x = Int32.Parse(xd.SelectSingleNode("/settings/windows/log/@width").Value); }
            catch { x=0; }
            try { y = Int32.Parse(xd.SelectSingleNode("/settings/windows/log/@height").Value); }
            catch { y=0; }
            LogWindowSize = new Size(x, y);

            int ram=0;
            try { ram = Int32.Parse(xd.SelectSingleNode("/settings/ram/@mb").Value); }
            catch {
                ram = TotalRAM-114;
                if (ram<57) ram = 57;
            }
            SetDesiredRAM(ram);

            SevenZip = xd.SelectSingleNode("/settings/program/@sevenzip").Value;
            Rar = xd.SelectSingleNode("/settings/program/@rar").Value;
            Ace = xd.SelectSingleNode("/settings/program/@ace").Value;
            WorkingFolder = xd.SelectSingleNode("/settings/program/@working").Value;
            if (WorkingFolder.Equals("")) WorkingFolder = CurrentFolder;

            if (SevenZip.Equals("")) Default7Zip();
            if (Rar.Equals("")) DefaultRar();
            if (Ace.Equals("")) DefaultAce();

            SourceFolders = new Hashtable();
            OutputFolders = new Hashtable();
            HaveFiles = new Hashtable();
            SetNames = new StringCollection();
            xnl = xd.SelectNodes("/settings/paths/setpath");
            foreach (XmlNode xn in xnl) {
                SetNames.Add(xn.Attributes["name"].Value);
                HaveFiles.Add(xn.Attributes["name"].Value, xn.Attributes["have"].Value);
                SourceFolders.Add(xn.Attributes["name"].Value, xn.Attributes["source"].Value);
                OutputFolders.Add(xn.Attributes["name"].Value, xn.Attributes["output"].Value);
            }
            if (HaveFiles.Contains(SetName)) HaveFile = HaveFiles[SetName].ToString();
            if (SourceFolders.Contains(SetName)) SourceFolder = SourceFolders[SetName].ToString();
            if (OutputFolders.Contains(SetName)) OutputFolder = OutputFolders[SetName].ToString();

            xd = new XmlDocument();
            xvr = new XmlValidatingReader(new XmlTextReader(Application.StartupPath+"\\Languages.xml"));
            try { xd.Load(xvr); }
            catch (Exception e) {
                MessageBox.Show(e.Message.Replace(" An error occurred at file:///", "\nFile: ").Replace(", (", "\nLine, Column: ("), "GoodMerge - XML Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally { xvr.Close(); }

            xnl = xd.SelectNodes("/languages/language[@name=\""+Language+"\"]/text");
            Strings = new string[xnl.Count+1];
            Strings[0]="GoodMerge";
            if (xnl.Count==0) {
                XmlNodeList xnll = xd.SelectNodes("/languages/language/@name");
                LanguagePicker lp = new LanguagePicker(xnll);
                lp.ShowDialog();
                if (lp.Tree.Nodes.Count==0) return false;
                Language = lp.Tree.SelectedNode.Text;
                xnl = xd.SelectNodes("/languages/language[@name=\""+Language+"\"]/text");
                Strings = new string[xnl.Count+1];
                Strings[0]="GoodMerge";
            }
            FontName = xd.SelectSingleNode("/languages/language[@name=\""+Language+"\"]/font/@face").Value;
            FontSize = Single.Parse(xd.SelectSingleNode("/languages/language[@name=\""+Language+"\"]/font/@size").Value);
            ScaleX = Int32.Parse(xd.SelectSingleNode("/languages/language[@name=\""+Language+"\"]/size/@scale-x").Value);
            ScaleY = Int32.Parse(xd.SelectSingleNode("/languages/language[@name=\""+Language+"\"]/size/@scale-y").Value);
            foreach (XmlNode xn in xnl) {
                Strings[Int32.Parse(xn.Attributes["n"].Value)] = xn.Attributes["t"].Value.Replace("$PRG", Strings[0]);
            }
            BiasesDisplay = new Hashtable();
            foreach (XmlNode xn in xd.SelectNodes("/languages/language[@name=\""+Language+"\"]/zonetext")) {
                BiasesDisplay.Add(xn.Attributes["n"].Value, xn.Attributes["t"].Value);
            }

            Thread.CurrentThread.CurrentCulture = CI;
            return true;
        }

        public void SetDesiredRAM(int d) {
            if (d<0) d=0;
            else if (d>2498) d=2498;
            DesiredRAM = d;
            if (d-65 < 0) UltraDict=0;
            else UltraDict = ((d-65)*2)/19;
            if (UltraDict > 256) UltraDict = 256;
            if (d-8 < 0) MaxDict=0;
            else MaxDict = ((d-8)*2)/19;
            if (MaxDict > 256) MaxDict = 256;
        }

        public void Default7Zip() {
            if (!File.Exists(SevenZip)) SevenZip = @"C:\Program Files\7-Zip\7za.exe";
            if (!File.Exists(SevenZip)) SevenZip = "7za.exe";
            if (File.Exists(SevenZip)) SevenZip = new FileInfo(SevenZip).FullName;
            else SevenZip = "";
        }
        public void DefaultRar() {
            if (!File.Exists(Rar)) Rar = @"C:\Program Files\WinRAR\Rar.exe";
            if (!File.Exists(Rar)) Rar = "Rar.exe";
            if (File.Exists(Rar)) Rar = new FileInfo(Rar).FullName;
            else Rar = "";
        }
        public void DefaultAce() {
            if (!File.Exists(Ace)) Ace = "ace32.exe";
            if (File.Exists(Ace)) Ace = new FileInfo(Ace).FullName;
            else Ace = "";
        }

        public void Save() {
            CultureInfo CI = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            XmlDocument xd= new XmlDocument();
            XmlValidatingReader xvr = new XmlValidatingReader(new XmlTextReader(Application.StartupPath+"\\Settings.xml"));
            try { xd.Load(xvr); }
            catch (Exception e) {
                MessageBox.Show(e.Message.Replace(" An error occurred at file:///", "\nFile: ").Replace(", (", "\nLine, Column: ("), "GoodMerge - XML Saving Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally { xvr.Close(); }
 
            xd.SelectSingleNode("/settings/romset/@name").Value = SetName;
            xd.SelectSingleNode("/settings/romset/@version").Value = Version;
            
            xd.SelectSingleNode("/settings/compression/@source").Value = SourceCompression;
            xd.SelectSingleNode("/settings/compression/@output").Value = OutputCompression;

            xd.SelectSingleNode("/settings/ram/@mb").Value = DesiredRAM.ToString();

            xd.SelectSingleNode("/settings/windows/main/@x").Value = MainWindowLocation.X.ToString();
            xd.SelectSingleNode("/settings/windows/main/@y").Value = MainWindowLocation.Y.ToString();
            xd.SelectSingleNode("/settings/windows/log/@x").Value = LogWindowLocation.X.ToString();
            xd.SelectSingleNode("/settings/windows/log/@y").Value = LogWindowLocation.Y.ToString();
            xd.SelectSingleNode("/settings/windows/log/@width").Value = LogWindowSize.Width.ToString();
            xd.SelectSingleNode("/settings/windows/log/@height").Value = LogWindowSize.Height.ToString();

            xd.SelectSingleNode("/settings/misc/@arguments").Value = Arguments ? "true" : "false";
            xd.SelectSingleNode("/settings/misc/@background").Value = Background ? "true" : "false";
            xd.SelectSingleNode("/settings/misc/@minimal").Value = Minimal ? "true" : "false";
            xd.SelectSingleNode("/settings/misc/@language").Value = Language;

            xd.SelectSingleNode("/settings/program/@sevenzip").Value = SevenZip;
            xd.SelectSingleNode("/settings/program/@rar").Value = Rar;
            xd.SelectSingleNode("/settings/program/@ace").Value = Ace;
            xd.SelectSingleNode("/settings/program/@working").Value = WorkingFolder;

            XmlNode biaspriority = xd.SelectSingleNode("/settings/biaspriority");
            biaspriority.RemoveAll();
            for (int loop=0; loop<Biases.Count; loop++) {
                XmlElement zone = xd.CreateElement("zone");
                XmlAttribute name = xd.CreateAttribute("name");
                name.Value = Biases[loop];
                zone.SetAttributeNode(name);
                biaspriority.AppendChild(zone);
            }

            XmlNode paths = xd.SelectSingleNode("/settings/paths");
            paths.RemoveAll();
            foreach (string s in SetNames) {
                XmlElement setpath = xd.CreateElement("setpath");
                XmlAttribute name = xd.CreateAttribute("name");
                XmlAttribute source = xd.CreateAttribute("source");
                XmlAttribute output = xd.CreateAttribute("output");
                XmlAttribute have = xd.CreateAttribute("have");
                name.Value = s;
                setpath.SetAttributeNode(name);
                source.Value = SourceFolders[s].ToString();
                setpath.SetAttributeNode(source);
                output.Value = OutputFolders[s].ToString();
                setpath.SetAttributeNode(output);
                have.Value = HaveFiles[s].ToString();
                setpath.SetAttributeNode(have);
                paths.AppendChild(setpath);
            }

            xd.Save(Application.StartupPath+"\\Settings.xml");
            Thread.CurrentThread.CurrentCulture = CI;
        }
    }
}