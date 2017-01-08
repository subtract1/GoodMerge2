using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace GoodMerge {
    public class Merger {
        #region <Variables>
        private Hashtable explicitClones;
        private ListDictionary wildCardClones;
        private Hashtable ROMs;
        private Hashtable alternateParents;
        private StringCollection Hacks;
        private Hashtable HackROMs;

        private Regex[] Flags;
        private Regex[] HackFlags=null;
        private Regex[] Ignores=null;
        private LogWindow log;
        
        private Options options;

        private char[] STAR = new char[] {'*'};

        #endregion

        public Merger(Options o, LogWindow l) {
            options = o;
            log = l;
            explicitClones=CollectionsUtil.CreateCaseInsensitiveHashtable();
            wildCardClones=new ListDictionary();
            ROMs=CollectionsUtil.CreateCaseInsensitiveHashtable();
            HackROMs=CollectionsUtil.CreateCaseInsensitiveHashtable();
            alternateParents=CollectionsUtil.CreateCaseInsensitiveHashtable();
            Hacks = new StringCollection();
            log.Exe = new Process();
        }

        public void Begin() {
            log.Show();
            log.StartTimer();
            Merge();
            log.Finish();
        }

        private void Merge() {
            #region <Locals>
            string TEMP_FILE="~gmlst~";
            string TEMP_DIR=options.WorkingFolder+"~gmtdir~\\";
            char[] SLASH = new char[] {'/'};
            char[] SPACE = new char[] {' '};
            char[] ENDLINE = new char[] {'\n','\r'};
            char[] PIPE = new char[] {'\xB3'};

            Regex hackRE = new Regex("\\(([^\\)]+) (Hack|hack)\\)");
            string[] splitText, split2;
            string[] romExtensions={};
            StringCollection files;

            FileInfo fileStats=null;
            FileInfo file=null;
            StreamWriter outputWriter;
            XmlDocument xd;
            XmlNodeList xnl;
            XmlTextReader xtr;
            LineParser lp;

            long pauseTime;
            DateTime opStart;

            int loop;
            int expectedCount=0;
            int actualCount=0;
            int fileCount=0;
            int archiveNumber=1;
            long inputSizeBytes;
            long inputSize;
            long totalInputSize=0;
            long totalOutputSize=0;
            bool matched;

            string curLine, fullOutput, lastFound, parent="";
            string coArgStart, coArgMid, coArgEnd, deArgStart, deArgEnd, settings="";
            string name, version;

            string decompressExe;
            string compressExe;
            if (options.SourceCompression=="rar") decompressExe=options.Rar;
            else if (options.SourceCompression=="ace") decompressExe=options.Ace;
            else decompressExe=options.SevenZip;
            if (options.OutputCompression=="rar") compressExe=options.Rar;
            else if (options.OutputCompression=="ace") compressExe=options.Ace;
            else compressExe=options.SevenZip;

            #endregion

            #region <Parse *.gmdb>
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            / Read "*.gmdb" for information about clones.
            / Format of the file is as follows:
            /     <SET VER/ext[/ext...]>
            /     Parent/Clone[/Clone...]
            /     <BIAS/b1[[/b2][/b3...]]>
            /     Parent1[[/Parent2][/Parent3...]][/Clone...]
            /     <SET...
            / There may be any number of clones listed, but at least one
            / must appear for each parent in the first section. In the
            / second section, each parent corresponds to the biases listed
            / in order, thus there must be as many parents as biases. Any
            / clone containing a '*' is a wild card match.
            /
            / Empty lines and those starting with ';' are ignored.
            /
            / Construct a hash table for explicit clones where the key is
            / the clone name and the value is the associated parent.
            / Construct a hash table for '*' clones where the key is the
            / match text and the value is the associated parent.
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
            
            // Find file with the correct ROMSet.
            matched=false;
            foreach (FileInfo f in new DirectoryInfo(options.ProgramFolder).GetFiles("*.xmdb")) {
                xtr=null;
                try {
                    xtr = new XmlTextReader(f.FullName);

                    // Check that the root node is <romsets>.
                    if (xtr.MoveToContent()==XmlNodeType.None || !xtr.Name.Equals("romsets")) continue;

                    // Read each child <set> node.
                    xtr.Read();
                    while (xtr.MoveToContent()!=XmlNodeType.None) {
                        if (xtr.Name.Equals("set")) {
                            name=null;
                            version=null;
                            // Check that name="SetName" and version="Version".
                            while (xtr.MoveToNextAttribute()) {
                                if (xtr.Name.Equals("name")) name=xtr.Value;
                                else if (xtr.Name.Equals("version")) version=xtr.Value;
                            }
                            if (name!=null && version!=null && name.Equals(options.SetName) && version.Equals(options.Version)) {
                                matched=true;
                                break;
                            }
                        }
                        xtr.Skip();
                    }
                    if (matched) {
                        file = f;
                        break;
                    }
                }
                catch { }
                finally { if (xtr!=null) xtr.Close(); }
            }
            if (!matched) { doError(options.Strings[42]+" \""+options.SetName+"\" ("+options.Strings[43]+" \""+options.Version+"\") "+options.Strings[44]); return; }

            log.AppendText(options.Strings[45]+" \""+file.FullName+"\"… ", LogWindow.GoodColor);
            string n="";

            // Attempt to load the document.
            xd = new XmlDocument();
            XmlValidatingReader tempxvr = new XmlValidatingReader(new XmlTextReader(file.FullName));
            try { xd.Load(tempxvr); }
            catch (Exception e) { doError("XML loading error.\n"+e.Message.Replace(" An error occurred at file:///", "\nFile: ").Replace(", (", "\nLine, Column: (")); return; }
            finally { tempxvr.Close(); }
            string root = "/romsets/set[@name=\""+options.SetName+"\" and @version=\""+options.Version+"\"]";
            xnl = xd.SelectNodes(root);

            // Parse the extensions.
            xnl = xd.SelectNodes(root+"/options/ext/@text");
            if (xnl.Count==0) { doError(options.Strings[46]+options.Strings[47]); return; }
            romExtensions = new string[xnl.Count];
            for (loop=0; loop<xnl.Count; loop++) { romExtensions[loop]=xnl[loop].Value; }

            // Parse the flags.
            xnl = xd.SelectNodes(root+"/options/flag/@reg");
            if (xnl.Count==0) { doError(options.Strings[46]+options.Strings[48]); return; }
            Flags = new Regex[xnl.Count];
            for (loop=0; loop<xnl.Count; loop++) { Flags[loop]=new Regex(xnl[loop].Value); }

            // Parse the hackflags, if present.
            xnl = xd.SelectNodes(root+"/options/hackflag/@reg");
            if (xnl.Count!=0) {
                HackFlags = new Regex[xnl.Count];
                for (loop=0; loop<xnl.Count; loop++) { HackFlags[loop]=new Regex(xnl[loop].Value); }
            }

            // Parse the ignores, if present.
            xnl = xd.SelectNodes(root+"/options/ignore/@reg");
            if (xnl.Count!=0) {
                Ignores = new Regex[xnl.Count];
                for (loop=0; loop<xnl.Count; loop++) { Ignores[loop]=new Regex(xnl[loop].Value); }
            }

            // Add all explicit parents.
            xnl = xd.SelectNodes(root+"/parents/parent");
            foreach (XmlNode xn in xnl) {
                parent=xn.Attributes["name"].Value;
                foreach (XmlNode child in xn.ChildNodes) {
                    if (child.NodeType==XmlNodeType.Element && child.Name=="clone") AddClone(parent, child.Attributes["name"].Value);
                    else if (child.NodeType==XmlNodeType.Element && child.Name=="group") wildCardClones.Add(new Regex(child.Attributes["reg"].Value), parent);
                }
            }

            // Add all zoned parents.
            xnl = xd.SelectNodes(root+"/parents/zoned");
            bool deferred;
            foreach (XmlNode xn in xnl) {
                XmlNodeList biasNodes = ((XmlElement)xn).GetElementsByTagName("bias");
                int rank=Int32.MaxValue;
                // Find the parent with the highest priority.
                foreach (XmlNode bias in biasNodes) {
                    if (!options.Biases.Contains(bias.Attributes["zone"].Value)) {
                        n="\n";
                        if (options.TestMode) log.AppendText(n+options.Strings[49]+bias.Attributes["zone"].Value, LogWindow.BadColor);
                    }
                    else if (options.Biases.IndexOf(bias.Attributes["zone"].Value)<rank) {
                        parent=bias.Attributes["name"].Value;
                        rank=options.Biases.IndexOf(bias.Attributes["zone"].Value);
                    }
                }
                if (rank==Int32.MaxValue) continue;
                deferred = xn.Attributes["type"].Value.Equals("deferred");
                // Add all clones.
                foreach (XmlNode child in xn.ChildNodes) {
                    if (child.NodeType==XmlNodeType.Element && child.Name=="bias" && !deferred) AddClone(parent, child.Attributes["name"].Value);
                    else if (child.NodeType==XmlNodeType.Element && child.Name=="clone") AddClone(parent, child.Attributes["name"].Value);
                    else if (child.NodeType==XmlNodeType.Element && child.Name=="group") wildCardClones.Add(new Regex(child.Attributes["reg"].Value), parent);
                }
            }

            log.AppendText(n+options.Strings[50]+"\n", LogWindow.GoodColor);

            #endregion
            #region <Parse Have.txt>
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            / Read "XXXXHave.txt" for the list of the actual ROMs to be
            / compressed. Sort them by the parent ROM's name. Note how many
            / files should be compressed.
            /
            / Construct a hash table where the key is the parent name and
            / the value is a list of clone names. To determine the parent,
            / first check if the full name contains any of the '*' strings.
            / If so, use the parent of the first matching string. Next,
            / strip the flags off the full name and check if the result is
            / present in the explicit clones table. If so, add this entry
            / using the parent from that table. Otherwise, just use the
            / flag-stripped name as the parent.
            /
            / Hacks are handled separately so that they can be checked
            / against the names already in the hash table.
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

            log.AppendText(options.Strings[45]+" \""+options.HaveFile+"\"… ", LogWindow.GoodColor);
   
            matched=false;
            lp = new LineParser(options.HaveFile);

            // Get the expected count.
            curLine=lp.GetNextLine();
            splitText=curLine.Split(SPACE);
            try { expectedCount=Int32.Parse(splitText[4]); }
            catch {
                try { expectedCount=Int32.Parse(splitText[3]); }
                catch { doError(options.Strings[51]); return; }
            }

            // Read each entry.
            curLine=lp.GetNextLine();
            while (curLine!=null) {
                matched=false;

                // First, try to match against the wild cards.
                foreach (DictionaryEntry entry in wildCardClones) {
                    if (((Regex)(entry.Key)).Match(curLine).Success) {
                        matched=true;
                        parent=entry.Value.ToString();
                        break;
                    }
                }
                if (!matched) {
                    // If it's a hack, process it later.
                    if (hackRE.IsMatch(curLine)) Hacks.Add(curLine);
                    else {
                        // Otherwise, do the flag strip dance.
                        parent=StripFlags(curLine);
                        if (explicitClones.Contains(parent)) parent=explicitClones[parent].ToString();
                        matched=true;
                    }
                }
                if (matched) { // Not a hack.
                    if (ROMs.Contains(parent)) ((StringCollection)(ROMs[parent])).Add(curLine);
                    else {
                        files = new StringCollection();
                        files.Add(curLine);
                        ROMs.Add(parent, files);
                    }
                }
      
                curLine=lp.GetNextLine();
            }

            // Now handle the hacks. Add the hack under the parent name that's
            // part of the filename if that name is already listed anywhere.
            // Otherwise, make a new entry with the flag stripped name.

            if (Hacks.Count!=0 && HackFlags!=null) {
                foreach (DictionaryEntry de in ROMs) {
                    try { HackROMs.Add(StripHackFlags(de.Key.ToString()), de.Key); }
                    catch { }
                }
            }
                    
            foreach (string str in Hacks) {
                string hackname=hackRE.Match(str).Groups[1].Value;
                if (explicitClones.Contains(hackname)) parent=explicitClones[hackname].ToString();
                else if (!HackROMs.Contains(hackname)) {
                    parent=StripFlags(str);
                    if (explicitClones.Contains(parent)) parent=explicitClones[parent].ToString();
                }
                else parent=HackROMs[hackname].ToString();
                if (ROMs.Contains(parent)) ((StringCollection)(ROMs[parent])).Add(str);
                else {
                    files = new StringCollection();
                    files.Add(str);
                    ROMs.Add(parent, files);
                }
            }

            log.AppendText(options.Strings[50]+"\n", LogWindow.GoodColor);
            log.AppendText(options.Strings[52], LogWindow.GoodColor);
            log.PopulateTree(ROMs);
            log.AppendText(options.Strings[53]+"\n", LogWindow.GoodColor);
            log.AddPauseTime(DateTime.Now.Ticks);
            
            // Skip merging if in Test Mode
            if (options.TestMode) {
                log.SetTotalArchives(expectedCount);
                log.SetCurrentArchive(ROMs.Count);
                log.AppendText("\n"+expectedCount+options.Strings[54]+ROMs.Count+options.Strings[55], LogWindow.GoodColor);
                log.SelectFilesOutput();
                return;
            }

            log.SetTotalArchives(ROMs.Count);

            #endregion
            #region <Prepare Process>
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            / Assemble the appropriate arguments for the chosen compressor
            / and decompressor. Prepare the process for execution.
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

            coArgMid="-w\"";
            if (options.OutputCompression=="7z") coArgStart="a -bd -t7z -ms -mmt -m0fb=128 ";
            else if (options.OutputCompression=="zip") coArgStart="a -bd -tzip -mx=9 -mfb=128 ";
            else if (options.OutputCompression=="ace") {
                coArgStart="a -m5 -d4096 -s -c2 -std ";
                coArgMid="-t\"";
            }
            else coArgStart="a -m5 -mdG -s ";
            coArgMid+=options.WorkingFolder.Substring(0,options.WorkingFolder.Length-1)+"\" \""+options.OutputFolder;
            coArgEnd="."+options.OutputCompression+"\" @\""+options.WorkingFolder+TEMP_FILE+"\"";

            deArgStart = decompressExe.EndsWith("7za.exe") ? "e -aos \"" : "e -o- \"";
            deArgStart+=options.SourceFolder;
            deArgEnd = decompressExe.EndsWith("7za.exe") ? "."+options.SourceCompression+"\" -o\"" : "."+options.SourceCompression+"\" \"";
            deArgEnd+=TEMP_DIR+"\"";

            if (options.SourceCompression!="none") {
                if (Directory.Exists(TEMP_DIR)) {
                    try { Directory.Delete(TEMP_DIR, true); }
                    catch { doError(options.Strings[88]); return; }
                }
            }

            log.Exe.StartInfo.UseShellExecute=false;
            log.Exe.StartInfo.CreateNoWindow=true;
            log.Exe.StartInfo.RedirectStandardOutput=true;
            log.Exe.StartInfo.FileName=compressExe;
            if (options.SourceCompression!="none") log.Exe.StartInfo.WorkingDirectory=TEMP_DIR;
            else log.Exe.StartInfo.WorkingDirectory=options.SourceFolder;

            #endregion
            #region <Perform Merge>
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            / For each game, decompress if necessary, write a list file,
            / then compress. Output stats.
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
            log.ClearText();
            log.AppendText(options.Strings[0]+" "+Assembly.GetExecutingAssembly().GetName().Version.ToString()+" - "+DateTime.Now.ToString()+"\n", LogWindow.GoodColor);
            log.AppendText(options.SetName+" "+options.Version+" - "+options.Strings[57]+options.DesiredRAM+options.Strings[58]+"\n", LogWindow.GoodColor);

            try { Directory.CreateDirectory(options.OutputFolder); }
            catch { doError(options.Strings[59]+"\n("+options.OutputFolder+")"); return; }

            foreach (DictionaryEntry entry in ROMs) {
                #region <Update Log Window>
                if (log.Canceled) break;
                // Update log window
                log.SetPercentCompletion(actualCount*100/expectedCount);

                if (log.Paused) {
                    log.StopTimer();
                    pauseTime=DateTime.Now.Ticks;
                    while (log.Paused) {
                        Thread.Sleep(50);
                    }
                    if (log.Canceled) break;
                    log.AddPauseTime(DateTime.Now.Ticks - pauseTime);
                    log.StartTimer();
                }

                parent=entry.Key.ToString();
                log.SetCurrentArchive(archiveNumber++);
                log.AppendText("\n"+parent+"\n", LogWindow.NormalColor, LogWindow.BoldUnderlineFont);

                #endregion
                inputSizeBytes=0;
                // If the output file already exists, get its stats and continue.
                fileStats = new FileInfo(options.OutputFolder+parent+"."+options.OutputCompression);
                if (fileStats.Exists) {
                    opStart=DateTime.Now;
                    #region <Get Stats from Existing File>
                    log.Exe.StartInfo.Arguments="l \""+fileStats.FullName+"\"";
                    log.Exe.StartInfo.WorkingDirectory=options.CurrentFolder;
                    if (log.Canceled) break;
                    try { log.Exe.Start(); }
                    catch { doError(options.Strings[60]+"\n("+compressExe+")"); return; }
                    fullOutput=log.Exe.StandardOutput.ReadToEnd();
                    try { log.Exe.WaitForExit(); log.Exe.Close(); }
                    catch { doError(options.Strings[61]+"\n("+compressExe+")"); return; }
                    if (log.Canceled) break;

                    try {
                        splitText = fullOutput.Split(ENDLINE);
                        if (options.OutputCompression=="ace") {
                            loop=0;
                            inputSizeBytes=0;
                            while (!splitText[++loop].StartsWith("Contents of archive"));
                            loop+=7;
                            while (!splitText[++loop].Replace(" ","").Equals("")) {
                                split2 = splitText[loop++].Split(PIPE);
                                inputSizeBytes+=int.Parse(split2[3]);
                            }
                            loop++;
                            fileCount = int.Parse(splitText[++loop].Split(SPACE)[1].Replace(".", ""));
                        }
                        else if (options.OutputCompression=="rar") {
                            loop=0;
                            splitText = splitText[splitText.Length-5].Split(SPACE);
                            while (splitText[loop++]=="");
                            fileCount = int.Parse(splitText[loop-1]);
                            while (splitText[loop++]=="");
                            inputSizeBytes = int.Parse(splitText[loop-1]);
                        }
                        else {
                            loop=0;
                            splitText = splitText[splitText.Length-3].Split(SPACE);
                            while (splitText[loop++]=="");
                            inputSizeBytes = int.Parse(splitText[loop-1]);
                            while (splitText[loop++]=="");
                            while (splitText[loop++]=="");
                            fileCount = int.Parse(splitText[loop-1]);
                        }
                    }
                    catch { doError(options.Strings[62]+"\n"+fullOutput); return; }
                    if (options.Arguments) {
                        if (options.OutputCompression=="7z") {
                            inputSize=(long)Math.Ceiling((inputSizeBytes/1024.0)/1024.0);
                            if (inputSize <= options.UltraDict) settings="-mx=9 -m0d="+inputSize+"m ";
                            else if (inputSize <= options.MaxDict) settings="-mx=7 -m0d="+inputSize+"m ";
                            else settings="-mx=7 -m0d="+options.MaxDict+"m ";
                        }
                        log.AppendText(options.Strings[75]+coArgStart+settings+"\n", LogWindow.ExtraColor);
                    }
                    log.AppendText(opStart.ToString()+String.Format(" - {0:N0} "+options.Strings[63]+" {1:N0} ", inputSizeBytes, fileCount));
                    if (fileCount==1) log.AppendText(options.Strings[64]);
                    else log.AppendText(options.Strings[65]);
                    log.AppendText(" "+options.Strings[66]+"\n");
                    if (options.SourceCompression!="none") log.Exe.StartInfo.WorkingDirectory=TEMP_DIR;
                    else log.Exe.StartInfo.WorkingDirectory=options.SourceFolder;
                    #endregion
                }
                else {
                    opStart=DateTime.Now;
                    if (options.SourceCompression!="none") {
                        #region <Decompress>
                        // If we need to, decompress all relevant files.
                        try { Directory.CreateDirectory(TEMP_DIR); }
                        catch { doError(options.Strings[67]+"\n("+TEMP_DIR+")"); return; }

                        log.AppendText(options.Strings[68]);
                        log.Exe.StartInfo.FileName=decompressExe;
                        files=(StringCollection)(entry.Value);
                        string[] temp;
                        if (alternateParents.Contains(parent)) {
                            temp = new string[((StringCollection)(alternateParents[parent])).Count+1];
                            ((StringCollection)(alternateParents[parent])).CopyTo(temp, 0);
                            temp[temp.Length-1]=parent;
                        }
                        else {
                            temp = new string[1];
                            temp[0]=parent;
                        }
                        foreach (string str in files) {
                            if (File.Exists(options.SourceFolder+str+"."+options.SourceCompression)) {
                                log.Exe.StartInfo.Arguments=deArgStart+str+deArgEnd;
                                if (log.Canceled) break;
                                try { log.Exe.Start(); }
                                catch { doError(options.Strings[60]+"\n("+decompressExe+")"); return; }
                                if (options.Background) { try { log.Exe.PriorityClass=ProcessPriorityClass.Idle; } catch { } }
                                log.Exe.StandardOutput.ReadToEnd();
                                try { log.Exe.WaitForExit(); log.Exe.Close(); }
                                catch { doError(options.Strings[61]+"\n("+decompressExe+")"); return; }
                            }
                        }
                        foreach (string str in temp) {
                            if (File.Exists(options.SourceFolder+str+"."+options.SourceCompression)) {
                                log.Exe.StartInfo.Arguments=deArgStart+str+deArgEnd;
                                if (log.Canceled) break;
                                try { log.Exe.Start(); }
                                catch { doError(options.Strings[60]+"\n("+decompressExe+")"); return; }
                                if (options.Background) { try { log.Exe.PriorityClass=ProcessPriorityClass.Idle; } catch { } }
                                log.Exe.StandardOutput.ReadToEnd();
                                try { log.Exe.WaitForExit(); log.Exe.Close(); }
                                catch { doError(options.Strings[61]+"\n("+decompressExe+")"); return; }
                            }
                        }
                        if (log.Canceled) break;

                        log.Exe.StartInfo.FileName=compressExe;
                        log.AppendText(options.Strings[53]+"\n");
                        #endregion
                    }
                    #region <Create List File>
                    // Create a list file containing only valid entries and gather stats.
                    try { outputWriter = new StreamWriter(options.WorkingFolder+TEMP_FILE); }
                    catch { doError(options.Strings[69]); return; }

                    files=(StringCollection)(entry.Value);
                    fileCount=0;
                    lastFound="";
                    foreach (string str in files) {
                        matched=false;
                        foreach (string ext in romExtensions) {
                            try {
                                if (options.SourceCompression!="none") fileStats = new FileInfo(TEMP_DIR+str+"."+ext);
                                else fileStats = new FileInfo(options.SourceFolder+str+"."+ext);
                                if (fileStats.Exists) {
                                    if (matched) { doError(options.Strings[70]+"\n"+lastFound+"\n"+fileStats.Name); return; }
                                    lastFound=fileStats.Name;
                                    matched=true;
                                    fileCount++;
                                    inputSizeBytes+=fileStats.Length;
                                    try { outputWriter.WriteLine(str+"."+ext); }
                                    catch {
                                        doError(options.Strings[71]);
                                        try { outputWriter.Flush(); outputWriter.Close(); }
                                        catch { }
                                        return;
                                    }
                                }
                            }
                            catch { doError(options.Strings[72]+"\n("+str+")"); return; }
                        }
                        if (!matched) { doError(options.Strings[73]+" \""+str+".*\""); return; }
                    }
                    try { outputWriter.Flush(); outputWriter.Close(); }
                    catch { doError(options.Strings[74]); return; }
                    #endregion
                    #region <Execute Compressor>
                    // If working with 7-zip, find the optimum arguments.
                    if (options.OutputCompression=="7z") {
                        inputSize=(long)Math.Ceiling((inputSizeBytes/1024.0)/1024.0);
                        if (inputSize <= options.UltraDict) settings="-mx=9 -m0d="+inputSize+"m ";
                        else if (inputSize <= options.MaxDict) settings="-mx=7 -m0d="+inputSize+"m ";
                        else settings="-mx=7 -m0d="+options.MaxDict+"m ";
                    }

                    if (options.Arguments) {
                        log.AppendText(options.Strings[75]+coArgStart+settings+"\n", LogWindow.ExtraColor);
                    }

                    log.AppendText(opStart.ToString()+" - "+String.Format("{0:N0} "+options.Strings[63]+" {1:N0} ", inputSizeBytes, fileCount));
                    if (fileCount==1) log.AppendText(options.Strings[64]);
                    else log.AppendText(options.Strings[65]);
                    log.AppendText(" "+options.Strings[76]+"\n");

                    // Execute the compressor and filter its output.
                    if (log.Canceled) break;
                    log.Exe.StartInfo.Arguments=coArgStart+settings+coArgMid+parent+coArgEnd;
                    log.CurrentOutputFileName=options.OutputFolder+parent+"."+options.OutputCompression;
                    try { log.Exe.Start(); }
                    catch { doError(options.Strings[60]+"\n("+compressExe+")"); return; }
                    if (options.Background) { try { log.Exe.PriorityClass=ProcessPriorityClass.Idle; } catch { } }
                    fullOutput=log.Exe.StandardOutput.ReadToEnd();
                    try { log.Exe.WaitForExit(); log.Exe.Close(); }
                    catch { doError(options.Strings[61]+"\n("+compressExe+")"); return; }
                    log.CurrentOutputFileName="~gmt~";
                    if (log.Canceled) break;

                    fileStats = new FileInfo(options.OutputFolder+parent+"."+options.OutputCompression);
                    if (!fileStats.Exists) {
                        doError(options.Strings[77]+compressExe+" "+log.Exe.StartInfo.Arguments+")\n");
                        log.AppendText(fullOutput, LogWindow.BadColor);
                        return;
                    }

                    if (!options.SourceCompression.Equals("none")) {
                        // Clean out the temporary folder
                        FileInfo[] tempFiles = (new DirectoryInfo(TEMP_DIR)).GetFiles();
                        foreach (FileInfo fi in tempFiles) {
                            try { fi.Attributes=FileAttributes.Normal; }
                            catch { doError(options.Strings[72]); return; }
                        }
                        try { Directory.Delete(TEMP_DIR, true); }
                        catch { doError(options.Strings[56]); return; }
                    }
                    #endregion
               }
                #region <Add Stats>
                actualCount+=fileCount;
                totalInputSize+=inputSizeBytes;
                totalOutputSize+=fileStats.Length;

                string time = options.Strings[40];
                TimeSpan t = TimeSpan.FromTicks(DateTime.Now.Ticks-opStart.Ticks);
                time+=(t.Days*24+t.Hours)+":";
                if (t.Minutes<10) time+="0";
                time+=t.Minutes+":";
                if (t.Seconds<10) time+="0";
                time+=t.Seconds;

                log.AppendText(DateTime.Now.ToString()+String.Format(" - {0:N0} "+options.Strings[87]+" ("+fileStats.Length*100/inputSizeBytes+options.Strings[78]+"). "+time+".\n", fileStats.Length));
                #endregion
                #region <Delete Files>
                if (options.SourceCompression.Equals("none") && options.DeleteFiles) {
                    log.AppendText(options.Strings[79], LogWindow.BadColor);
                    n="";
                    files=(StringCollection)(entry.Value);
                    foreach (string str in files) {
                        foreach (string ext in romExtensions) {
                            fileStats = new FileInfo(options.SourceFolder+str+"."+ext);
                            if (fileStats.Exists) {
                                try {
                                    fileStats.Attributes=FileAttributes.Normal;
                                    fileStats.Delete();
                                }
                                catch {
                                    log.AppendText("\n"+options.Strings[80]+" \""+fileStats.FullName+"\"", LogWindow.BadColor);
                                    n="\n";
                                }
                            }
                        }
                    }
                    log.AppendText(n+options.Strings[53]+"\n", LogWindow.BadColor);
                }
                #endregion
                #region <Update Log Window>
                log.SetProjectedSize(totalOutputSize*expectedCount/actualCount);
                
                log.SetPercentCompression((int)(totalOutputSize*100/totalInputSize));
                log.SetTimeRemaining((log.GetElapsedTicks()/actualCount)*(expectedCount-actualCount));
                #endregion
            }
            #endregion
            #region <Clean Up>
            try { File.Delete(options.WorkingFolder+TEMP_FILE); }
            catch { doError(options.Strings[81]); }

            if (log.Canceled) {
                doError(options.Strings[82]+actualCount*100/expectedCount+"%.");
                if (options.SourceCompression!="none") {
                    // Clean out the temporary folder
                    try {
                        FileInfo[] tempFiles = (new DirectoryInfo(TEMP_DIR)).GetFiles();
                        foreach (FileInfo fi in tempFiles) fi.Attributes=FileAttributes.Normal;
                        Directory.Delete(TEMP_DIR, true);
                    }
                    catch { }
                }
            }
            else {
                log.AppendText("\n"+options.Strings[83], LogWindow.GoodColor, LogWindow.BoldUnderlineFont);
                log.AppendText(String.Format("\n{0:N0} "+options.Strings[63]+" {1:N0} "+options.Strings[84], totalInputSize, actualCount), LogWindow.GoodColor);
                log.AppendText(String.Format("\n{0:N0} "+options.Strings[87]+" ("+totalOutputSize*100/totalInputSize+options.Strings[78]+") "+options.Strings[85]+" {1:N0} "+options.Strings[65]+" ("+ROMs.Count*100/actualCount+options.Strings[78]+").\n", totalOutputSize, ROMs.Count), LogWindow.GoodColor);
                string time = options.Strings[40];
                TimeSpan t = TimeSpan.FromTicks(log.GetElapsedTicks());
                time+=(t.Days*24+t.Hours)+":";
                if (t.Minutes<10) time+="0";
                time+=t.Minutes+":";
                if (t.Seconds<10) time+="0";
                time+=t.Seconds;
                log.AppendText(time+"\n", LogWindow.GoodColor);
                log.AppendText(options.Strings[28]+" - "+DateTime.Now.ToString()+"\n", LogWindow.GoodColor);
            }
            #endregion
        }

        private string StripHackFlags(string name) {
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            / StripFlags: return a string that corresponds to the passed string
            / without any flags attached. Takes into account various special
            / settings based on the ROM set being scanned.
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
            int flagsStart=name.Length;
            Match m;
            foreach (Regex re in HackFlags) {
                m=re.Match(name);
                if (m.Success && m.Index<flagsStart) flagsStart=m.Index;
            }
            return name.Substring(0, flagsStart);
        }

        private string StripFlags(string name) {
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            / StripFlags: return a string that corresponds to the passed string
            / without any flags attached. Takes into account various special
            / settings based on the ROM set being scanned.
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
            int flagsStart=name.Length;
            string stripped;
            Match m;
            foreach (Regex re in Flags) {
                m=re.Match(name);
                if (m.Success && m.Index<flagsStart) flagsStart=m.Index;
            }
            stripped = name.Substring(0, flagsStart);

            if (Ignores!=null) {
                foreach (Regex re in Ignores) {
                    stripped = re.Replace(stripped, "");
                }
            }

            return stripped;
        }

        private void AddClone(string parent, string clone) {
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            / AddClone: Adds the specified clone to the appropriate hash table
            / with the specified parent.
            /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
            if (parent.Equals(clone)) return;
            if (explicitClones.Contains(clone)) {
                if (!explicitClones[clone].ToString().ToLower().Equals(parent.ToLower())) {
                    if (options.TestMode) log.AppendText("\n"+options.Strings[86]+clone, LogWindow.BadColor);
                }
            }
            else explicitClones.Add(clone, parent);
        }

        private void doError(string text) {
            log.AppendText("\n\n"+options.Strings[1]+": "+text+"\n", LogWindow.BadColor);
        }
    }
}