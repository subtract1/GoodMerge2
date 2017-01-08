using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Specialized;

namespace GoodMerge {
    public delegate void dPopulateTree(Hashtable ROMs);

    public class LogWindow : Form {
        #region <Statics>
        public static Font NormalFont;
        public static Font BoldFont;
        public static Font UnderlineFont;
        public static Font BoldUnderlineFont;

        public static Color NormalColor = Color.Black;
        public static Color BadColor = Color.Red;
        public static Color GoodColor = Color.DarkGreen;
        public static Color ExtraColor = Color.DarkCyan;

        private static int ButtonWidth=80;
        private static int ButtonPadding=4;
        private static int ChromeWidth;
        private static int ChromeHeight;
        #endregion
        #region <Private Properties>
        private Options options;
        private Size PrevSize;
        private Size MinimalSize;
        private Size MinSize;

        private int TotalArchives=0;
        private long ProjectedSize=0;
        private int CurrentArchive=0;
        private int PercentCompression=0;
        private int PercentCompletion=0;
        private long TimeRemaining=0;
        private long StartTicks=0;

        private bool ArchivesChanged=false;
        private bool ProjectedSizeChanged=false;
        private bool PercentCompressionChanged=false;
        private bool PercentCompletionChanged=false;
        private bool TimeRemainingChanged=false;
        #endregion
        #region <Public Properties>
        public string CurrentOutputFileName;
        public bool Paused=true;
        public bool Canceled=false;
        public Process Exe;
        #endregion
        #region <Form Elements>
        private RichTextBox LogOutput;

        private System.Windows.Forms.Timer TimerUpdate;
        private ToolTip Tooltip;

        private StatusBar TheStatusBar;
        private StatusBarPanel SBPanelTimeElapsed;
        private StatusBarPanel SBPanelTimeRemaining;
        private StatusBarPanel SBPanelPercentCompression;
        private StatusBarPanel SBPanelBackground;
        private StatusBarPanel SBPanelArchives;
        private StatusBarPanel SBPanelProjectedSize;

        private Button ButtonDone;
        private Button ButtonSave;
        private Button ButtonPause;

        private SaveFileDialog DialogSaveFile;

        private TreeView FilesOutput;

        private ProgressBar TheProgressBar;

        private TabControl Tabs;
        private TabPage Page1;
        private TabPage Page2;

        private CheckBox CheckboxMinimal;

        private MouseEventHandler EventListenerMouse;
        private EventHandler EventListenerAll;

        private ContextMenu ContextMenuFilesOutput;
        private MenuItem CMItemFilesOutputCopyText;
        private MenuItem CMItemFilesOutputCopyNode;
        private ContextMenu ContextMenuLogOutput;
        private MenuItem CMItemLogOutputCopyText;

        #endregion

        protected override void WndProc(ref Message m) {
            if(m.Msg != 0x0010) base.WndProc(ref m);
            else {
                if (ButtonDone.Text.Equals(options.Strings[19])) EventHandlerAll(ButtonDone, new EventArgs());
                else {
                    if (CheckboxMinimal.Checked) options.LogWindowSize=PrevSize;
                    else options.LogWindowSize=this.Size;
                    options.LogWindowLocation=this.Location;
                    base.WndProc(ref m);
                }
            }
        }

        public LogWindow(Options o) {
            options=o;

            NormalFont = new Font(options.FontName, options.FontSize, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));
            BoldFont = new Font(options.FontName, options.FontSize, FontStyle.Bold, GraphicsUnit.Point, ((Byte)(0)));
            UnderlineFont = new Font(options.FontName, options.FontSize, FontStyle.Underline, GraphicsUnit.Point, ((Byte)(0)));
            BoldUnderlineFont = new Font(options.FontName, options.FontSize, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, ((Byte)(0)));
            #region <Initializers>
            LogOutput = new RichTextBox();

            TheProgressBar = new ProgressBar();

            TheStatusBar = new StatusBar();
            SBPanelPercentCompression = new StatusBarPanel();
            SBPanelTimeElapsed = new StatusBarPanel();
            SBPanelTimeRemaining = new StatusBarPanel();
            SBPanelBackground = new StatusBarPanel();
            SBPanelArchives = new StatusBarPanel();
            SBPanelProjectedSize = new StatusBarPanel();

            ButtonDone = new Button();
            ButtonSave = new Button();
            ButtonPause = new Button();

            TimerUpdate = new System.Windows.Forms.Timer();

            Tooltip = new ToolTip();

            DialogSaveFile = new SaveFileDialog();

            FilesOutput = new TreeView();
            
            Tabs = new TabControl();
            Page1 = new TabPage();
            Page2 = new TabPage();

            CheckboxMinimal = new CheckBox();

            EventListenerMouse = new MouseEventHandler(EventHandlerMouse);
            EventListenerAll = new EventHandler(EventHandlerAll);

            ContextMenuFilesOutput = new ContextMenu();
            CMItemFilesOutputCopyText = new MenuItem();
            CMItemFilesOutputCopyNode = new MenuItem();
            ContextMenuLogOutput = new ContextMenu();
            CMItemLogOutputCopyText = new MenuItem();
            #endregion
            #region <StatusBar>
            // 
            // TheStatusBar
            // 
            TheStatusBar.Panels.Add(SBPanelTimeElapsed);
            TheStatusBar.Panels.Add(SBPanelTimeRemaining);
            TheStatusBar.Panels.Add(SBPanelProjectedSize);
            TheStatusBar.Panels.Add(SBPanelArchives);
            TheStatusBar.Panels.Add(SBPanelPercentCompression);
            TheStatusBar.Panels.Add(SBPanelBackground);
            TheStatusBar.Height=20;
            TheStatusBar.ShowPanels=false;
            TheStatusBar.SizingGrip=true;
            TheStatusBar.DoubleClick += EventListenerAll;
            // 
            // SBPanelTimeElapsed
            // 
            SBPanelTimeElapsed.Text=options.Strings[11];
            SBPanelTimeElapsed.MinWidth=0;
            SBPanelTimeElapsed.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            SBPanelTimeElapsed.ToolTipText = options.Strings[13];
            SBPanelTimeElapsed.AutoSize=StatusBarPanelAutoSize.Contents;
            // 
            // SBPanelTimeRemaining
            // 
            SBPanelTimeRemaining.Text = options.Strings[12];
            SBPanelTimeRemaining.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            SBPanelTimeRemaining.ToolTipText = options.Strings[14];
            SBPanelTimeRemaining.AutoSize=StatusBarPanelAutoSize.Contents;
            SBPanelTimeRemaining.MinWidth=0;
            // 
            // SBPanelArchives
            // 
            SBPanelArchives.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            SBPanelArchives.Text="0/0";
            SBPanelArchives.ToolTipText = options.Strings[15];
            SBPanelArchives.AutoSize=StatusBarPanelAutoSize.Contents;
            SBPanelArchives.MinWidth=0;
            // 
            // SBPanelPercentCompression
            // 
            SBPanelPercentCompression.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            SBPanelPercentCompression.Text="0%";
            SBPanelPercentCompression.ToolTipText = options.Strings[16];
            SBPanelPercentCompression.AutoSize=StatusBarPanelAutoSize.Contents;
            SBPanelPercentCompression.MinWidth=0;
            // 
            // SBPanelProjectedSize
            // 
            SBPanelProjectedSize.Text="0B";
            SBPanelProjectedSize.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            SBPanelProjectedSize.ToolTipText = options.Strings[17];
            SBPanelProjectedSize.AutoSize=StatusBarPanelAutoSize.Contents;
            SBPanelProjectedSize.MinWidth=0;
            // 
            // SBPanelBackground
            // 
            SBPanelBackground.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            SBPanelBackground.ToolTipText = options.Strings[18];
            SBPanelBackground.MinWidth=0;
            // 
            // TheProgressBar
            // 
            TheProgressBar.Step = 1;
            TheProgressBar.Height=20;
            #endregion
            #region <Buttons>
            // 
            // ButtonDone
            // 
            ButtonDone.FlatStyle = FlatStyle.System;
            ButtonDone.Size = new Size(80, 24);
            ButtonDone.Text = options.Strings[19];
            ButtonDone.Top = 5;
            ButtonDone.Click += EventListenerAll;
            ButtonDone.MouseUp += EventListenerMouse;
            // 
            // ButtonSave
            // 
            ButtonSave.Enabled = false;
            ButtonSave.FlatStyle = FlatStyle.System;
            ButtonSave.Size = new Size(80, 24);
            ButtonSave.Text = options.Strings[20];
            ButtonSave.Top = 5;
            ButtonSave.Click += EventListenerAll;
            ButtonSave.MouseUp += EventListenerMouse;
            // 
            // ButtonPause
            // 
            ButtonPause.FlatStyle = FlatStyle.System;
            ButtonPause.Size = new Size(80, 24);
            ButtonPause.Text = options.Strings[21];
            ButtonPause.Top = 5;
            ButtonPause.Click += EventListenerAll;
            ButtonPause.MouseUp += EventListenerMouse;
            // 
            // CheckboxMinimal
            // 
            CheckboxMinimal.Appearance = Appearance.Button;
            CheckboxMinimal.Checked = false;
            CheckboxMinimal.CheckedChanged += EventListenerAll;
            CheckboxMinimal.FlatStyle=FlatStyle.System;
            CheckboxMinimal.Size = new Size(80, 24);
            CheckboxMinimal.Text = options.Strings[22];
            CheckboxMinimal.TextAlign = ContentAlignment.MiddleCenter;
            CheckboxMinimal.Top = 5;
            #endregion
            #region <Tabs>
            // 
            // Tabs
            // 
            Tabs.Controls.Add(Page1);
            Tabs.Location = new Point(0, 4);
            Tabs.SelectedIndex = 0;
            Tabs.Appearance=TabAppearance.FlatButtons;
            Tabs.HotTrack=true;
            Tabs.SelectedIndexChanged += EventListenerAll;
            // 
            // Page1
            // 
            Page1.BorderStyle=BorderStyle.Fixed3D;
            Page1.Controls.Add(LogOutput);
            Page1.Text = options.Strings[23];
            // 
            // Page2
            // 
            Page2.BorderStyle=BorderStyle.Fixed3D;
            Page2.Controls.Add(FilesOutput);
            Page2.Text = options.Strings[24];

            // 
            // LogOutput
            // 
            LogOutput.BackColor = Color.White;
            LogOutput.BorderStyle=BorderStyle.None;
            LogOutput.Cursor = Cursors.IBeam;
            LogOutput.DetectUrls = false;
            LogOutput.Font = NormalFont;
            LogOutput.ForeColor = NormalColor;
            LogOutput.HideSelection=false;
            LogOutput.ReadOnly = true;
            LogOutput.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
            LogOutput.Text = "";
            LogOutput.WordWrap = false;
            LogOutput.ContextMenu=ContextMenuLogOutput;

            // 
            // FilesOutput
            // 
            FilesOutput.BackColor = Color.White;
            FilesOutput.BorderStyle=BorderStyle.None;
            FilesOutput.Font = NormalFont;
            FilesOutput.ForeColor = NormalColor;
            FilesOutput.Indent = 20;
            FilesOutput.ItemHeight = 20;
            FilesOutput.Sorted = true;
            FilesOutput.KeyPress += new KeyPressEventHandler(onKey);
            FilesOutput.MouseDown += EventListenerMouse;
            FilesOutput.ContextMenu=ContextMenuFilesOutput;
            #endregion
            #region <Non-Visibles>
            // 
            // TimerUpdate
            // 
            TimerUpdate.Enabled = true;
            TimerUpdate.Interval = 250;
            TimerUpdate.Tick += new EventHandler(UpdateStatusBar);

            // 
            // DialogSaveFile
            // 
            DialogSaveFile.Filter = options.Strings[25]+"|*.log";

            // 
            // ToolTip
            // 
            Tooltip.InitialDelay=0;
            #endregion
            #region <Menus>
            // 
            // ContextMenuFilesOutput
            // 
            ContextMenuFilesOutput.MenuItems.Add(CMItemFilesOutputCopyText);
            ContextMenuFilesOutput.MenuItems.Add(CMItemFilesOutputCopyNode);
            // 
            // CMItemFilesOutputCopyText
            // 
            CMItemFilesOutputCopyText.Text=options.Strings[26];
            CMItemFilesOutputCopyText.Click+=EventListenerAll;
            // 
            // CMItemFilesOutputCopyNode
            // 
            CMItemFilesOutputCopyNode.Text=options.Strings[27];
            CMItemFilesOutputCopyNode.Click+=EventListenerAll;
            // 
            // ContextMenuLogOutput
            // 
            ContextMenuLogOutput.MenuItems.Add(CMItemLogOutputCopyText);
            // 
            // CMItemLogOutputCopyText
            // 
            CMItemLogOutputCopyText.Text=options.Strings[26];
            CMItemLogOutputCopyText.Click+=EventListenerAll;
            #endregion
            #region <LogWindow>
            // 
            // LogWindow
            // 
            this.AcceptButton=ButtonPause;
            this.AutoScaleBaseSize = new Size(options.ScaleX, options.ScaleY);
            this.ClientSize = new Size(604, 432);
            this.Controls.Add(TheStatusBar);
            this.Controls.Add(ButtonDone);
            this.Controls.Add(ButtonSave);
            this.Controls.Add(ButtonPause);
            this.Controls.Add(CheckboxMinimal);
            this.Controls.Add(Tabs);
            this.Controls.Add(TheProgressBar);
            this.SizeGripStyle = SizeGripStyle.Hide;
            if (options.LogWindowLocation.X==0 && options.LogWindowLocation.Y==0) this.StartPosition = FormStartPosition.CenterScreen;
            else {
                this.StartPosition=FormStartPosition.Manual;
                this.Location=options.LogWindowLocation;
            }
            this.Text = options.Strings[0]+" - "+options.SetName+" "+options.Version;
            this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("GoodMerge.GoodMerge.ico"));
            this.Font = NormalFont;

            this.SizeChanged += new EventHandler(RepositionElements);
            this.Load += new EventHandler(StartUp);
            #endregion
        }

        public void Finish() {
            if (this.InvokeRequired) { this.BeginInvoke(new MethodInvoker(Finish)); }
            else {
                StopTimer();
                SetPercentCompletion(100);
                UpdateStatusBar(this, new EventArgs());
                SBPanelTimeRemaining.Text=options.Strings[28];
                ButtonDone.Text=options.Strings[29];
                ButtonDone.DialogResult=DialogResult.OK;
                ButtonSave.Enabled=true;
                ButtonPause.Enabled=false;
                if (ButtonPause.Focused) ButtonSave.Select();
                AcceptButton=ButtonSave;
                if (options.Minimal) {
                    int i = ClientSize.Width-(SBPanelTimeElapsed.Width+SBPanelTimeRemaining.Width+SBPanelProjectedSize.Width+SBPanelArchives.Width+SBPanelPercentCompression.Width);
                    if (i<SBPanelBackground.MinWidth) SBPanelBackground.Width=SBPanelBackground.MinWidth;
                    else SBPanelBackground.Width=i;
                }
                else {
                    int i = ClientSize.Width-(SBPanelTimeElapsed.Width+SBPanelTimeRemaining.Width+SBPanelProjectedSize.Width+SBPanelArchives.Width+SBPanelPercentCompression.Width+18);
                    if (i<SBPanelBackground.MinWidth) SBPanelBackground.Width=SBPanelBackground.MinWidth;
                    else SBPanelBackground.Width=i;
                }
            }
        }

        public void PopulateTree(Hashtable ROMs) {
            if (this.InvokeRequired) { this.BeginInvoke(new dPopulateTree(PopulateTree), new object[]{ROMs}); }
            else {
                int loop;
                Cursor.Current=Cursors.WaitCursor;
                FilesOutput.BeginUpdate();
                foreach (DictionaryEntry de in ROMs) {
                    TreeNode[] tn = new TreeNode[((StringCollection)(de.Value)).Count];
                    for (loop=0;loop<tn.Length;loop++) {
                        tn[loop]=new TreeNode(((StringCollection)(de.Value))[loop]);
                    }
                    FilesOutput.Nodes.Add(new TreeNode(de.Key.ToString(), tn));
                }
                FilesOutput.EndUpdate();
                Tabs.Controls.Add(Page2);
                Cursor.Current=Cursors.Default;
            }
        }

        private void EventHandlerAll(object s, EventArgs e) {
                #region <Buttons>
            if (s==ButtonPause) {
                Paused=!Paused;
                if (Paused) ButtonPause.Text=options.Strings[30];
                else ButtonPause.Text=options.Strings[21];
                ButtonSave.Enabled=Paused;
                try { if (Paused && !Exe.HasExited) MessageBox.Show(options.Strings[31], options.Strings[0]+"- "+options.Strings[32], MessageBoxButtons.OK, MessageBoxIcon.Information); }
                catch { }
            }
            else if (s==ButtonSave) {
                if (Tabs.SelectedIndex==0) DialogSaveFile.FileName=options.SetName+" "+options.Version+" Merge.log";
                else DialogSaveFile.FileName=options.SetName+" "+options.Version+" Files.log";
                if (DialogSaveFile.ShowDialog()==DialogResult.OK) {
                    if (Tabs.SelectedIndex==0) {
                        try { LogOutput.SaveFile(DialogSaveFile.FileName,RichTextBoxStreamType.PlainText); }
                        catch (Exception x) { DoError(options.Strings[33]+DialogSaveFile.FileName+"\n"+options.Strings[34]+x.Message); }
                    }
                    else {
                        try {
                            StreamWriter sw = new StreamWriter(DialogSaveFile.FileName);
                            foreach (TreeNode tna in FilesOutput.Nodes) {
                                sw.Write(tna.Text+"\r\n");
                                foreach (TreeNode tn in tna.Nodes) {
                                    sw.Write("\t"+tn.Text+"\r\n");
                                }
                                sw.WriteLine();
                            }
                            sw.Flush();
                            sw.Close();
                        }
                        catch (Exception x) { DoError(options.Strings[33]+DialogSaveFile.FileName+"\n"+options.Strings[34]+x.Message); }
                    }
                }
            }
            else if (s==ButtonDone) {
                if (ButtonDone.DialogResult!=DialogResult.OK) {
                    if (MessageBox.Show(this, options.Strings[35], options.Strings[0]+" - "+options.Strings[36],
                      MessageBoxButtons.YesNo, MessageBoxIcon.Question)==DialogResult.Yes) {
                        Canceled=true;
                        Paused=false;
                        StopTimer();
                        TimerUpdate.Dispose();
                        try {
                            Exe.Kill();
                            System.Threading.Thread.Sleep(100);
                            File.Delete(CurrentOutputFileName);
                        }
                        catch { };
                    }
                }
                else this.Close();
            }
            else if (s==CheckboxMinimal) {
                options.Minimal=CheckboxMinimal.Checked;
                if (options.Minimal) {
                    Tabs.Visible=false;
                    PrevSize=this.Size;
                    this.MaximumSize=MinimalSize;
                    this.MinimumSize=MinimalSize;
                    this.ClientSize=MinimalSize;
                    TheStatusBar.SizingGrip=false;
                }
                else {
                    this.MaximumSize=new Size(0,0);
                    this.MinimumSize=MinSize;
                    this.Size=PrevSize;
                    TheStatusBar.SizingGrip=true;
                    Tabs.Visible=true;
                    if (Tabs.SelectedIndex==0) LogOutput.ScrollToCaret();
                }
            }
                #endregion
                #region <Menu Items>
            else if (s==CMItemLogOutputCopyText) {
                if (LogOutput.SelectedText.Equals("")) {
                    LogOutput.SelectAll();
                    LogOutput.Copy();
                }
                else LogOutput.Copy();
            }
            else if (s==CMItemFilesOutputCopyText) {
                Clipboard.SetDataObject(FilesOutput.SelectedNode.Text);
            }
            else if (s==CMItemFilesOutputCopyNode) {
                string copy=FilesOutput.SelectedNode.Text+"\r\n";
                foreach (TreeNode tn in FilesOutput.SelectedNode.Nodes) {
                    copy+="\t"+tn.Text+"\r\n";
                }
                Clipboard.SetDataObject(copy);
            }
                #endregion
                #region <Others>
            else if (s==Tabs) {
                if (Tabs.SelectedIndex==0) {
                    ButtonSave.Text=options.Strings[20];
                    LogOutput.Select();
                }
                else {
                    ButtonSave.Text=options.Strings[37];
                    FilesOutput.Select();
                }
            }
            else if (s==TheStatusBar) {
                options.Background=!options.Background;
                if (options.Background) {
                    SBPanelBackground.Text=options.Strings[38];
                    try { Exe.PriorityClass=ProcessPriorityClass.Idle; }
                    catch { }
                }
                else {
                    SBPanelBackground.Text=options.Strings[39];
                    try { Exe.PriorityClass=ProcessPriorityClass.Normal; }
                    catch { }
                }
            }
                #endregion
        }

        private void EventHandlerMouse(object sender, MouseEventArgs e) {
            if (sender.GetType()==typeof(Button)) LogOutput.Select();
            else FilesOutput.SelectedNode=FilesOutput.GetNodeAt(e.X, e.Y);
        }

        private void UpdateStatusBar(object sender, EventArgs e) {
            if (StartTicks!=0) {
                string time;
                if (options.Minimal) time="";
                else time = options.Strings[40];
                TimeSpan t = TimeSpan.FromTicks(DateTime.Now.Ticks-StartTicks);
                time+=(t.Days*24+t.Hours)+":";
                if (t.Minutes<10) time+="0";
                time+=t.Minutes+":";
                if (t.Seconds<10) time+="0";
                time+=t.Seconds;
                SBPanelTimeElapsed.Text=time;
            }

            if (TimeRemainingChanged) {
                string time;
                if (options.Minimal) time="";
                else time = options.Strings[41];
                TimeSpan t = TimeSpan.FromTicks(TimeRemaining);
                time+=(t.Days*24+t.Hours)+":";
                if (t.Minutes<10) time+="0";
                time+=t.Minutes+":";
                if (t.Seconds<10) time+="0";
                time+=t.Seconds;
                SBPanelTimeRemaining.Text=time;
                TimeRemainingChanged=false;
            }
            
            if (ProjectedSizeChanged) {
                if (ProjectedSize>=10L*1000L*1000L*1000L) SBPanelProjectedSize.Text=String.Format("{0:F1}G", ProjectedSize/1024.0/1024.0/1024.0);
                else if (ProjectedSize>=1000*1000*1000) SBPanelProjectedSize.Text=String.Format("{0:F2}G", ProjectedSize/1024.0/1024.0/1024.0);
                else if (ProjectedSize>=100*1000*1000) SBPanelProjectedSize.Text=String.Format("{0:F0}M", ProjectedSize/1024.0/1024.0);
                else if (ProjectedSize>=10*1000*1000) SBPanelProjectedSize.Text=String.Format("{0:F1}M", ProjectedSize/1024.0/1024.0);
                else if (ProjectedSize>=1000*1000) SBPanelProjectedSize.Text=String.Format("{0:F2}M", ProjectedSize/1024.0/1024.0);
                else if (ProjectedSize>=100*1000) SBPanelProjectedSize.Text=String.Format("{0:F0}K", ProjectedSize/1024.0);
                else if (ProjectedSize>=10*1000) SBPanelProjectedSize.Text=String.Format("{0:F1}K", ProjectedSize/1024.0);
                else if (ProjectedSize>=1000) SBPanelProjectedSize.Text=String.Format("{0:F2}K", ProjectedSize/1024.0);
                else SBPanelProjectedSize.Text=ProjectedSize.ToString()+"B";
                ProjectedSizeChanged=false;
            }

            if (ArchivesChanged) {
                SBPanelArchives.Text=CurrentArchive.ToString()+"/"+TotalArchives.ToString();
                ArchivesChanged=false;
            }

            if (PercentCompressionChanged) {
                SBPanelPercentCompression.Text=PercentCompression.ToString()+"%";
                PercentCompressionChanged=false;
            }

            if (PercentCompletionChanged) {
                TheProgressBar.Value=PercentCompletion;
                Tooltip.SetToolTip(TheProgressBar, PercentCompletion.ToString()+"%");
                PercentCompletionChanged=false;
            }

            if (options.Minimal) {
                int i = ClientSize.Width-(SBPanelTimeElapsed.Width+SBPanelTimeRemaining.Width+SBPanelProjectedSize.Width+SBPanelArchives.Width+SBPanelPercentCompression.Width);
                if (i<SBPanelBackground.MinWidth) SBPanelBackground.Width=SBPanelBackground.MinWidth;
                else SBPanelBackground.Width=i;
            }
            else {
                int i = ClientSize.Width-(SBPanelTimeElapsed.Width+SBPanelTimeRemaining.Width+SBPanelProjectedSize.Width+SBPanelArchives.Width+SBPanelPercentCompression.Width+18);
                if (i<SBPanelBackground.MinWidth) SBPanelBackground.Width=SBPanelBackground.MinWidth;
                else SBPanelBackground.Width=i;
            }
        }

        private void RepositionElements(object sender, EventArgs e) {
            ButtonDone.Left=ClientSize.Width-(ButtonWidth+ButtonPadding);
            ButtonPause.Left=ClientSize.Width-2*(ButtonWidth+ButtonPadding);
            ButtonSave.Left=ClientSize.Width-3*(ButtonWidth+ButtonPadding);
            CheckboxMinimal.Left=ClientSize.Width-4*(ButtonWidth+ButtonPadding);
            TheProgressBar.Top=ClientSize.Height-TheStatusBar.Height-TheProgressBar.Height;
            TheProgressBar.Width=ClientSize.Width;
            Tabs.Width=ClientSize.Width;
            Tabs.Height=ClientSize.Height-TheProgressBar.Height-TheStatusBar.Height-4;
            LogOutput.Size=new Size(Tabs.ClientSize.Width-12, Tabs.ClientSize.Height-38);
            FilesOutput.Size=new Size(Tabs.ClientSize.Width-12, Tabs.ClientSize.Height-38);
            if (options.Minimal) {
                int i = ClientSize.Width-(SBPanelTimeElapsed.Width+SBPanelTimeRemaining.Width+SBPanelProjectedSize.Width+SBPanelArchives.Width+SBPanelPercentCompression.Width);
                if (i<SBPanelBackground.MinWidth) SBPanelBackground.Width=SBPanelBackground.MinWidth;
                else SBPanelBackground.Width=i;
            }
            else {
                int i = ClientSize.Width-(SBPanelTimeElapsed.Width+SBPanelTimeRemaining.Width+SBPanelProjectedSize.Width+SBPanelArchives.Width+SBPanelPercentCompression.Width+18);
                if (i<SBPanelBackground.MinWidth) SBPanelBackground.Width=SBPanelBackground.MinWidth;
                else SBPanelBackground.Width=i;
            }
        }

        private void StartUp(object sender, EventArgs e) {
            ChromeHeight=this.Size.Height-this.ClientSize.Height;
            ChromeWidth=this.Size.Width-this.ClientSize.Width;
            MinimalSize=new Size((4*ButtonWidth)+(5*ButtonPadding)+ChromeWidth, ButtonDone.Top+ButtonDone.Height+ButtonPadding+TheProgressBar.Height+TheStatusBar.Height+ChromeHeight);
            MinSize=new Size(516+ChromeWidth, 151+ChromeHeight);
            this.MinimumSize=MinSize;

            if (options.LogWindowSize.Width>=this.MinimumSize.Width && options.LogWindowSize.Height>=this.MinimumSize.Height) {
                this.Size=options.LogWindowSize;
            }

            SBPanelBackground.Text=options.Background?options.Strings[38]:options.Strings[39];
            TheStatusBar.ShowPanels=true;
            PrevSize=this.Size;
            CheckboxMinimal.Checked=options.Minimal;
            RepositionElements(this, new EventArgs());
            LogOutput.Select();
            Paused=false;
            new Thread(new ThreadStart(new Merger(options, this).Begin)).Start();
        }

        private void onKey(object sender, KeyPressEventArgs e) {
            if (e.KeyChar=='+') { FilesOutput.BeginUpdate(); FilesOutput.ExpandAll(); FilesOutput.EndUpdate(); }
            else if (e.KeyChar=='-') { FilesOutput.BeginUpdate(); FilesOutput.CollapseAll(); FilesOutput.EndUpdate(); }
        }

        private void DoError(string text) {
            MessageBox.Show(text, options.Strings[0]+" - "+options.Strings[1], MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ClearText() {
            LogOutput.Clear();
        }

        public void AppendText(string s) {
            LogOutput.AppendText(s);
        }

        public void AppendText(string s, Color c) {
            LogOutput.SelectionColor=c;
            LogOutput.AppendText(s);
            LogOutput.SelectionColor=NormalColor;
        }

        public void AppendText(string s, Color c, Font f) {
            LogOutput.SelectionFont=f;
            LogOutput.SelectionColor=c;
            LogOutput.AppendText(s);
            LogOutput.SelectionFont=NormalFont;
            LogOutput.SelectionColor=NormalColor;
        }

        public void SetPercentCompletion(int i) {
            PercentCompletion=i;
            PercentCompletionChanged=true;
        }

        public void SetPercentCompression(int i) {
            PercentCompression=i;
            PercentCompressionChanged=true;
        }

        public void SetTimeRemaining(long t) {
            TimeRemaining=t;
            TimeRemainingChanged=true;
        }

        public void SetCurrentArchive(int i) {
            CurrentArchive=i;
            ArchivesChanged=true;
        }

        public void SetTotalArchives(int i) {
            TotalArchives=i;
            ArchivesChanged=true;
        }

        public void SetProjectedSize(long i) {
            ProjectedSize=i;
            ProjectedSizeChanged=true;
        }

        public void StartTimer() {
            if (this.InvokeRequired) BeginInvoke(new MethodInvoker(StartTimer));
            else TimerUpdate.Enabled=true;
        }

        public void StopTimer() {
            if (this.InvokeRequired) BeginInvoke(new MethodInvoker(StopTimer));
            else TimerUpdate.Enabled=false;
        }

        public void AddPauseTime(long t) {
            StartTicks+=t;
        }

        public long GetElapsedTicks() {
            return DateTime.Now.Ticks-StartTicks;
        }

        public void SelectFilesOutput() {
            if (this.InvokeRequired) BeginInvoke(new MethodInvoker(SelectFilesOutput));
            else {
                Tabs.SelectedIndex=1;
                FilesOutput.SelectedNode=FilesOutput.Nodes[0];
                Page2.Select();
                FilesOutput.Select();
            }
        }
    }
}