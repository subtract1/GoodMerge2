using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Specialized;

namespace GoodMerge {
    class SettingsDialog : Form {
        #region <Variables>
        private Options options;
        private ArrayList ROMSetList;
        private ArrayList SourceList;
        private ArrayList OutputList;
        #endregion
        #region <Form Elements>
        private TabControl Tabs;
        private Button ButtonMerge;
        private Button ButtonNext;
        private Button ButtonBack;
        private Button ButtonRefreshROMSets;
        private Button ButtonResetMaximumRAM;
        private OpenFileDialog DialogOpenHaveFile;
        private OpenFileDialog DialogOpenExeFile;
        private FolderBrowserDialog DialogOpenFolder;
        private EventHandler FocusEventListener;
        private EventHandler EventListener;
        #endregion
        #region <Page 1 - ROM Set>
        private TabPage Tab1;
        private Label LabelROMSet;
        private ListBox ListboxROMSet;
        private GroupBox GroupboxWelcome;
        #endregion
        #region <Page 2 - Files>
        private TabPage Tab2;
        private GroupBox GroupboxSourceFolder;
        private GroupBox GroupboxOutputFolder;
        private GroupBox GroupboxHaveFile;
        private TextBox TextboxOutputFolder;
        private TextBox TextboxSourceFolder;
        private TextBox TextboxHaveFile;
        private Label LabelSourceFolder;
        private Label LabelOutputFolder;
        private Label LabelHaveFile;
        private Button ButtonDefaultSourceFolder;
        private Button ButtonBrowseSourceFolder;
        private Button ButtonDefaultOutputFolder;
        private Button ButtonBrowseOutputFolder;
        private Button ButtonDefaultHaveFile;
        private Button ButtonBrowseHaveFile;
        #endregion
        #region <Page 3 - Compression>
        private TabPage Tab3;
        private GroupBox GroupboxSourceCompression;
        private GroupBox GroupboxOutputCompression;
        private GroupBox Groupbox7ZipOptions;
        private ListBox ListboxSourceCompression;
        private ListBox ListboxOutputCompression;
        private Label LabelSourceCompression;
        private Label LabelOutputCompression;
        private Label Label7ZipOptions;
        private Label LabelMaxRAM;
        private Label LabelMaxDict;
        private Label LabelUltraDict;
        private Numeric NumericMaximumRAM;
        private TextBox TextboxUltraDictionary;
        private TextBox TextboxMaxDictionary;
        #endregion
        #region <Page 4 - Advanced>
        private TabPage Tab4;
        private GroupBox GroupboxBackground;
        private GroupBox GroupboxArguments;
        private GroupBox GroupboxDeleteFiles;
        private GroupBox GroupboxBiasZonePriority;
        private GroupBox GroupboxResetLanguage;
        private CheckBox CheckboxBackground;
        private CheckBox CheckboxArguments;
        private CheckBox CheckboxDeleteFiles;
        private CheckBox CheckboxResetLanguage;
        private Label LabelBackground;
        private Label LabelArguments;
        private Label LabelDeleteFiles;
        private Label LabelResetLanguage;
        private Label LabelWorkingFolder;
        private Label LabelBiasZonePriority;
        private Button ButtonMoveUp;
        private Button ButtonMoveDown;
        private ListBox ListboxBiasPriority;
        #endregion
        #region <Page 5 - Programs>
        private TabPage Tab5;
        private GroupBox Groupbox7ZipLocation;
        private GroupBox GroupboxRarLocation;
        private GroupBox GroupboxAceLocation;
        private GroupBox GroupboxWorkingFolder;
        private TextBox Textbox7ZipLocation;
        private TextBox TextboxRarLocation;
        private TextBox TextboxAceLocation;
        private TextBox TextboxWorkingFolder;
        private Button ButtonDefaultRarLocation;
        private Button ButtonBrowseRarLocation;
        private Button ButtonDefault7ZipLocation;
        private Button ButtonBrowse7ZipLocation;
        private Button ButtonDefaultAceLocation;
        private Button ButtonBrowseAceLocation;
        private Button ButtonDefaultWorkingFolder;
        private Button ButtonBrowseWorkingFolder;
        #endregion
        #region <Page 6 - About>
        private TabPage Tab6;
        private Label LabelAbout;
        private LinkLabel LinkWebpage;
        private GroupBox GroupboxAbout;
        #endregion

        protected override void WndProc(ref Message m) {
            if(m.Msg != 0x0010) base.WndProc(ref m);
            else {
                options.MainWindowLocation=this.Location;
                if (options.SetNames.Contains(options.SetName)) {
                    options.SourceFolders.Remove(options.SetName);
                    options.OutputFolders.Remove(options.SetName);
                    options.HaveFiles.Remove(options.SetName);
                }
                else options.SetNames.Add(options.SetName);
                options.HaveFiles.Add(options.SetName, TextboxHaveFile.Text);
                options.SourceFolders.Add(options.SetName, TextboxSourceFolder.Text);
                options.OutputFolders.Add(options.SetName, TextboxOutputFolder.Text);
                base.WndProc(ref m);
            }
        }

        public SettingsDialog(Options o) {
            options = o;
            InitializeComponent();
            searchForProgs();
            searchForSets();
            PrepareDialog();
        }

        private void PrepareDialog() {
            TextboxWorkingFolder.Text = options.WorkingFolder;
            TextboxSourceFolder.Text = options.SourceFolder;
            TextboxOutputFolder.Text = options.OutputFolder;
            TextboxHaveFile.Text = options.HaveFile;
            TextboxSourceFolder.Select(TextboxSourceFolder.Text.Length, 0);
            TextboxOutputFolder.Select(TextboxOutputFolder.Text.Length, 0);
            TextboxWorkingFolder.Select(TextboxWorkingFolder.Text.Length, 0);
            TextboxHaveFile.Select(TextboxHaveFile.Text.Length, 0);

            Textbox7ZipLocation.Text = options.SevenZip;
            TextboxRarLocation.Text = options.Rar;
            TextboxAceLocation.Text = options.Ace;

            if (ListboxROMSet.Items.Contains(options.SetName+" "+options.Version)) ListboxROMSet.SelectedIndex=ListboxROMSet.FindStringExact(options.SetName+" "+options.Version);
            if (ListboxSourceCompression.Items.Contains(options.SourceCompression)) ListboxSourceCompression.SelectedIndex=ListboxSourceCompression.FindStringExact(options.SourceCompression);
            else ListboxSourceCompression.SelectedIndex=0;
            if (ListboxOutputCompression.Items.Contains(options.OutputCompression)) ListboxOutputCompression.SelectedIndex=ListboxOutputCompression.FindStringExact(options.OutputCompression);
            else if (ListboxOutputCompression.Items.Count!=0) ListboxOutputCompression.SelectedIndex=0;

            NumericMaximumRAM.Value=options.DesiredRAM;

            CheckboxBackground.Checked=options.Background;
            CheckboxArguments.Checked=options.Arguments;

            foreach (string s in options.Biases) ListboxBiasPriority.Items.Add(options.BiasesDisplay[s]);
            ListboxBiasPriority.SelectedIndex=0;
        }

        private void InitializeComponent() {
            #region <Initializers>
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GoodMerge));

            Tabs = new TabControl();

            Tab1 = new TabPage();
            Tab2 = new TabPage();
            Tab3 = new TabPage();
            Tab4 = new TabPage();
            Tab5 = new TabPage();
            Tab6 = new TabPage();

            ButtonMerge = new Button();
            ButtonNext = new Button();
            ButtonBack = new Button();
            ButtonRefreshROMSets = new Button();
            ButtonResetMaximumRAM = new Button();
            ButtonDefaultWorkingFolder = new Button();
            ButtonBrowseWorkingFolder = new Button();
            ButtonDefaultRarLocation = new Button();
            ButtonBrowseRarLocation = new Button();
            ButtonDefault7ZipLocation = new Button();
            ButtonBrowse7ZipLocation = new Button();
            ButtonDefaultAceLocation = new Button();
            ButtonBrowseAceLocation = new Button();
            ButtonDefaultOutputFolder = new Button();
            ButtonBrowseOutputFolder = new Button();
            ButtonDefaultSourceFolder = new Button();
            ButtonBrowseSourceFolder = new Button();
            ButtonDefaultHaveFile = new Button();
            ButtonBrowseHaveFile = new Button();
            ButtonMoveUp = new Button();
            ButtonMoveDown = new Button();

            LabelROMSet = new Label();
            LabelOutputFolder = new Label();
            LabelSourceFolder = new Label();
            LabelHaveFile = new Label();
            LabelSourceCompression = new Label();
            Label7ZipOptions = new Label();
            LabelMaxDict = new Label();
            LabelMaxRAM = new Label();
            LabelUltraDict = new Label();
            LabelOutputCompression = new Label();
            LabelBiasZonePriority = new Label();
            LabelWorkingFolder = new Label();
            LabelAbout = new Label();
            LabelDeleteFiles = new Label();
            LabelArguments = new Label();
            LabelBackground = new Label();
            LabelResetLanguage = new Label();
            LinkWebpage = new LinkLabel();

            ListboxROMSet = new ListBox();
            ListboxOutputCompression = new ListBox();
            ListboxSourceCompression = new ListBox();
            ListboxBiasPriority = new ListBox();

            GroupboxWelcome = new GroupBox();
            GroupboxOutputFolder = new GroupBox();
            GroupboxSourceFolder = new GroupBox();
            GroupboxHaveFile = new GroupBox();
            GroupboxSourceCompression = new GroupBox();
            Groupbox7ZipOptions = new GroupBox();
            GroupboxOutputCompression = new GroupBox();
            GroupboxDeleteFiles = new GroupBox();
            GroupboxArguments = new GroupBox();
            GroupboxBackground = new GroupBox();
            GroupboxBiasZonePriority = new GroupBox();
            GroupboxResetLanguage = new GroupBox();
            GroupboxRarLocation = new GroupBox();
            Groupbox7ZipLocation = new GroupBox();
            GroupboxAceLocation = new GroupBox();
            GroupboxWorkingFolder = new GroupBox();
            GroupboxAbout = new GroupBox();

            TextboxOutputFolder = new TextBox();
            TextboxSourceFolder = new TextBox();
            TextboxMaxDictionary = new TextBox();
            TextboxUltraDictionary = new TextBox();
            TextboxHaveFile = new TextBox();
            TextboxWorkingFolder = new TextBox();
            TextboxRarLocation = new TextBox();
            Textbox7ZipLocation = new TextBox();
            TextboxAceLocation = new TextBox();

            CheckboxDeleteFiles = new CheckBox();
            CheckboxArguments = new CheckBox();
            CheckboxBackground = new CheckBox();
            CheckboxResetLanguage = new CheckBox();

            NumericMaximumRAM = new Numeric();

            DialogOpenHaveFile = new OpenFileDialog();
            DialogOpenExeFile = new OpenFileDialog();
            DialogOpenFolder = new FolderBrowserDialog();

            FocusEventListener = new EventHandler(focusEventHandler);
            EventListener = new EventHandler(eventHandler);
            #endregion
            #region <SuspendLayouts>
            SuspendLayout();
            Tabs.SuspendLayout();
            Tab1.SuspendLayout();
            Tab2.SuspendLayout();
            Tab3.SuspendLayout();
            Tab4.SuspendLayout();
            Tab5.SuspendLayout();
            Tab6.SuspendLayout();
            GroupboxWelcome.SuspendLayout();
            GroupboxOutputFolder.SuspendLayout();
            GroupboxSourceFolder.SuspendLayout();
            GroupboxHaveFile.SuspendLayout();
            GroupboxSourceCompression.SuspendLayout();
            Groupbox7ZipOptions.SuspendLayout();
            GroupboxOutputCompression.SuspendLayout();
            GroupboxDeleteFiles.SuspendLayout();
            GroupboxArguments.SuspendLayout();
            GroupboxBackground.SuspendLayout();
            GroupboxResetLanguage.SuspendLayout();
            GroupboxBiasZonePriority.SuspendLayout();
            GroupboxRarLocation.SuspendLayout();
            Groupbox7ZipLocation.SuspendLayout();
            GroupboxAceLocation.SuspendLayout();
            GroupboxWorkingFolder.SuspendLayout();
            GroupboxAbout.SuspendLayout();
            #endregion
            #region <Main Interface>
            // 
            // SettingsDialog
            // 
            this.AutoScaleBaseSize = new Size(options.ScaleX, options.ScaleY);
            this.ClientSize = new Size(604, 432);
            this.Controls.Add(Tabs);
            this.Controls.Add(ButtonNext);
            this.Controls.Add(ButtonMerge);
            this.Controls.Add(ButtonBack);
            this.Font = new Font(options.FontName, options.FontSize, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Icon = new Icon(Assembly.GetEntryAssembly().GetManifestResourceStream("GoodMerge.GoodMerge.ico"));
            this.MaximizeBox = false;
            if (options.MainWindowLocation.X==0 && options.MainWindowLocation.Y==0) this.StartPosition = FormStartPosition.CenterScreen;
            else {
                this.StartPosition=FormStartPosition.Manual;
                this.Location=options.MainWindowLocation;
            }
            this.Text = options.Strings[0];
            // 
            // Tabs
            // 
            Tabs.Appearance = TabAppearance.FlatButtons;
            Tabs.Controls.Add(Tab1);
            Tabs.Controls.Add(Tab2);
            Tabs.Controls.Add(Tab3);
            Tabs.Controls.Add(Tab4);
            Tabs.Controls.Add(Tab5);
            Tabs.Controls.Add(Tab6);
            Tabs.HotTrack = true;
            Tabs.Location = new Point(0, 4);
            Tabs.Multiline = true;
            Tabs.Padding = new Point(12, 3);
            Tabs.SelectedIndexChanged += EventListener;
            Tabs.Size = new Size(604, 390);
            Tabs.Multiline=false;
            // 
            // ButtonMerge
            // 
            ButtonMerge.FlatStyle = FlatStyle.System;
            ButtonMerge.Font = new Font(options.FontName, options.FontSize*1.231F, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));
            ButtonMerge.Location = new Point(488, 396);
            ButtonMerge.Size = new Size(112, 32);
            ButtonMerge.Text = options.Strings[89];
            ButtonMerge.Click += EventListener;
            // 
            // ButtonNext
            // 
            ButtonNext.FlatStyle = FlatStyle.System;
            ButtonNext.Location = new Point(360, 400);
            ButtonNext.Size = new Size(112, 24);
            ButtonNext.Text = options.Strings[90];
            ButtonNext.Click += EventListener;
            // 
            // ButtonBack
            // 
            ButtonBack.Enabled=false;
            ButtonBack.FlatStyle = FlatStyle.System;
            ButtonBack.Location = new Point(240, 400);
            ButtonBack.Size = new Size(112, 24);
            ButtonBack.Text = options.Strings[91];
            ButtonBack.Click += EventListener;
            #endregion
            #region <Non-Visibles>
            // 
            // DialogOpenHaveFile
            // 
            DialogOpenHaveFile.Filter = "*Have.txt|*have.txt|*Miss.txt|*miss.txt|"+options.Strings[92]+"|*.*";
            #endregion
            #region <Page 1 - ROM Set>
            // 
            // Tab1
            // 
            Tab1.Controls.Add(ListboxROMSet);
            Tab1.Controls.Add(GroupboxWelcome);
            Tab1.Controls.Add(ButtonRefreshROMSets);
            Tab1.BorderStyle=BorderStyle.Fixed3D;
            Tab1.Size = new Size(596, 356);
            Tab1.Text = options.Strings[93];
            // 
            // GroupboxWelcome
            // 
            GroupboxWelcome.Controls.Add(LabelROMSet);
            GroupboxWelcome.FlatStyle = FlatStyle.System;
            GroupboxWelcome.Location = new Point(152, 8);
            GroupboxWelcome.Size = new Size(432, 336);
            GroupboxWelcome.Text = options.Strings[99];
            // 
            // LabelROMSet
            // 
            LabelROMSet.FlatStyle = FlatStyle.System;
            LabelROMSet.Location = new Point(8, 24);
            LabelROMSet.Size = new Size(416, 304);
            LabelROMSet.Text = options.Strings[100];
            LabelROMSet.UseMnemonic = false;
            // 
            // ListboxROMSet
            // 
            ListboxROMSet.IntegralHeight = false;
            ListboxROMSet.ItemHeight = 18;
            ListboxROMSet.Location = new Point(8, 8);
            ListboxROMSet.SelectedIndexChanged += EventListener;
            ListboxROMSet.Size = new Size(136, 304);
            ListboxROMSet.ScrollAlwaysVisible=true;
            // 
            // ButtonRefreshROMSets
            // 
            ButtonRefreshROMSets.FlatStyle = FlatStyle.System;
            ButtonRefreshROMSets.Location = new Point(8, 320);
            ButtonRefreshROMSets.Size = new Size(136, 24);
            ButtonRefreshROMSets.Text = options.Strings[101];
            ButtonRefreshROMSets.Visible = true;
            ButtonRefreshROMSets.Click += EventListener;
            #endregion
            #region <Page 2 - Files>
            // 
            // Tab2
            // 
            Tab2.BorderStyle = BorderStyle.Fixed3D;
            Tab2.Controls.Add(GroupboxSourceFolder);
            Tab2.Controls.Add(GroupboxOutputFolder);
            Tab2.Controls.Add(GroupboxHaveFile);
            Tab2.Size = new Size(596, 361);
            Tab2.Text = options.Strings[94];
            #region <Source Folder>
            // 
            // GroupboxSourceFolder
            // 
            GroupboxSourceFolder.Controls.Add(LabelSourceFolder);
            GroupboxSourceFolder.Controls.Add(TextboxSourceFolder);
            GroupboxSourceFolder.Controls.Add(ButtonDefaultSourceFolder);
            GroupboxSourceFolder.Controls.Add(ButtonBrowseSourceFolder);
            GroupboxSourceFolder.FlatStyle = FlatStyle.System;
            GroupboxSourceFolder.Location = new Point(8, 8);
            GroupboxSourceFolder.Size = new Size(576, 104);
            GroupboxSourceFolder.Text = options.Strings[102];
            // 
            // TextboxSourceFolder
            // 
            TextboxSourceFolder.Location = new Point(8, 24);
            TextboxSourceFolder.Size = new Size(400, 23);
            TextboxSourceFolder.Enter += FocusEventListener;
            // 
            // ButtonDefaultSourceFolder
            // 
            ButtonDefaultSourceFolder.FlatStyle = FlatStyle.System;
            ButtonDefaultSourceFolder.Location = new Point(416, 24);
            ButtonDefaultSourceFolder.Size = new Size(72, 24);
            ButtonDefaultSourceFolder.Text = options.Strings[103];
            ButtonDefaultSourceFolder.Click += EventListener;
            // 
            // ButtonBrowseSourceFolder
            // 
            ButtonBrowseSourceFolder.FlatStyle = FlatStyle.System;
            ButtonBrowseSourceFolder.Location = new Point(496, 24);
            ButtonBrowseSourceFolder.Size = new Size(72, 24);
            ButtonBrowseSourceFolder.Text = options.Strings[104];
            ButtonBrowseSourceFolder.Click += EventListener;
            // 
            // LabelSourceFolder
            // 
            LabelSourceFolder.FlatStyle = FlatStyle.System;
            LabelSourceFolder.Location = new Point(8, 56);
            LabelSourceFolder.Size = new Size(560, 40);
            LabelSourceFolder.Text = options.Strings[105];
            LabelSourceFolder.UseMnemonic = false;
            #endregion
            #region <Output Folder>
            // 
            // GroupboxOutputFolder
            // 
            GroupboxOutputFolder.Controls.Add(LabelOutputFolder);
            GroupboxOutputFolder.Controls.Add(TextboxOutputFolder);
            GroupboxOutputFolder.Controls.Add(ButtonDefaultOutputFolder);
            GroupboxOutputFolder.Controls.Add(ButtonBrowseOutputFolder);
            GroupboxOutputFolder.FlatStyle = FlatStyle.System;
            GroupboxOutputFolder.Location = new Point(8, 120);
            GroupboxOutputFolder.Size = new Size(576, 104);
            GroupboxOutputFolder.Text = options.Strings[106];
            // 
            // TextboxOutputFolder
            // 
            TextboxOutputFolder.Location = new Point(8, 24);
            TextboxOutputFolder.Size = new Size(400, 23);
            TextboxOutputFolder.Enter += FocusEventListener;
            // 
            // ButtonDefaultOutputFolder
            // 
            ButtonDefaultOutputFolder.FlatStyle = FlatStyle.System;
            ButtonDefaultOutputFolder.Location = new Point(416, 24);
            ButtonDefaultOutputFolder.Size = new Size(72, 24);
            ButtonDefaultOutputFolder.Text = options.Strings[103];
            ButtonDefaultOutputFolder.Click += EventListener;
            // 
            // ButtonBrowseOutputFolder
            // 
            ButtonBrowseOutputFolder.FlatStyle = FlatStyle.System;
            ButtonBrowseOutputFolder.Location = new Point(496, 24);
            ButtonBrowseOutputFolder.Size = new Size(72, 24);
            ButtonBrowseOutputFolder.Text = options.Strings[104];
            ButtonBrowseOutputFolder.Click += EventListener;
            // 
            // LabelOutputFolder
            // 
            LabelOutputFolder.FlatStyle = FlatStyle.System;
            LabelOutputFolder.Location = new Point(8, 56);
            LabelOutputFolder.Size = new Size(560, 40);
            LabelOutputFolder.Text = options.Strings[107];
            LabelOutputFolder.UseMnemonic = false;
            #endregion
            #region <Have File>
            // 
            // GroupboxHaveFile
            // 
            GroupboxHaveFile.Controls.Add(LabelHaveFile);
            GroupboxHaveFile.Controls.Add(TextboxHaveFile);
            GroupboxHaveFile.Controls.Add(ButtonDefaultHaveFile);
            GroupboxHaveFile.Controls.Add(ButtonBrowseHaveFile);
            GroupboxHaveFile.FlatStyle = FlatStyle.System;
            GroupboxHaveFile.Location = new Point(8, 232);
            GroupboxHaveFile.Size = new Size(576, 104);
            GroupboxHaveFile.Text = options.Strings[108];
            // 
            // TextboxHaveFile
            // 
            TextboxHaveFile.Location = new Point(8, 24);
            TextboxHaveFile.Size = new Size(400, 23);
            TextboxHaveFile.Enter += FocusEventListener;
            // 
            // ButtonDefaultHaveFile
            // 
            ButtonDefaultHaveFile.FlatStyle = FlatStyle.System;
            ButtonDefaultHaveFile.Location = new Point(416, 24);
            ButtonDefaultHaveFile.Size = new Size(72, 24);
            ButtonDefaultHaveFile.Text = options.Strings[103];
            ButtonDefaultHaveFile.Click += EventListener;
            // 
            // ButtonBrowseHaveFile
            // 
            ButtonBrowseHaveFile.FlatStyle = FlatStyle.System;
            ButtonBrowseHaveFile.Location = new Point(496, 24);
            ButtonBrowseHaveFile.Size = new Size(72, 24);
            ButtonBrowseHaveFile.Text = options.Strings[104];
            ButtonBrowseHaveFile.Click += EventListener;
            // 
            // LabelHaveFile
            // 
            LabelHaveFile.FlatStyle = FlatStyle.System;
            LabelHaveFile.Location = new Point(8, 56);
            LabelHaveFile.Size = new Size(560, 40);
            LabelHaveFile.Text = options.Strings[109];
            LabelHaveFile.UseMnemonic = false;
            #endregion
            #endregion
            #region <Page 3 - Compression>
            // 
            // Tab3
            // 
            Tab3.BorderStyle = BorderStyle.Fixed3D;
            Tab3.Controls.Add(GroupboxSourceCompression);
            Tab3.Controls.Add(GroupboxOutputCompression);
            Tab3.Controls.Add(Groupbox7ZipOptions);
            Tab3.Size = new Size(596, 361);
            Tab3.Text = options.Strings[95];
            #region <Source Compression>
            // 
            // GroupboxSourceCompression
            // 
            GroupboxSourceCompression.Controls.Add(ListboxSourceCompression);
            GroupboxSourceCompression.Controls.Add(LabelSourceCompression);
            GroupboxSourceCompression.FlatStyle = FlatStyle.System;
            GroupboxSourceCompression.Location = new Point(8, 8);
            GroupboxSourceCompression.Size = new Size(576, 104);
            GroupboxSourceCompression.Text = options.Strings[110];
            // 
            // ListboxSourceCompression
            // 
            ListboxSourceCompression.ColumnWidth=48;
            ListboxSourceCompression.IntegralHeight = false;
            ListboxSourceCompression.ItemHeight = 18;
            ListboxSourceCompression.Location = new Point(8, 28);
            ListboxSourceCompression.MultiColumn=true;
            ListboxSourceCompression.SelectedIndexChanged += EventListener;
            ListboxSourceCompression.Size = new Size(100, 64);
            // 
            // LabelSourceCompression
            // 
            LabelSourceCompression.FlatStyle = FlatStyle.System;
            LabelSourceCompression.Location = new Point(116, 24);
            LabelSourceCompression.Size = new Size(452, 72);
            LabelSourceCompression.Text = options.Strings[111]+options.Strings[156];
            LabelSourceCompression.UseMnemonic = false;
            #endregion
            #region <Output Compression>
            // 
            // GroupboxOutputCompression
            // 
            GroupboxOutputCompression.Controls.Add(LabelOutputCompression);
            GroupboxOutputCompression.Controls.Add(ListboxOutputCompression);
            GroupboxOutputCompression.FlatStyle = FlatStyle.System;
            GroupboxOutputCompression.Location = new Point(8, 120);
            GroupboxOutputCompression.Size = new Size(576, 88);
            GroupboxOutputCompression.Text = options.Strings[112];
            // 
            // ListboxOutputCompression
            // 
            ListboxOutputCompression.ColumnWidth=48;
            ListboxOutputCompression.IntegralHeight = false;
            ListboxOutputCompression.ItemHeight = 18;
            ListboxOutputCompression.Location = new Point(8, 28);
            ListboxOutputCompression.MultiColumn=true;
            ListboxOutputCompression.SelectedIndexChanged += EventListener;
            ListboxOutputCompression.Size = new Size(100, 48);
            // 
            // LabelOutputCompression
            // 
            LabelOutputCompression.FlatStyle = FlatStyle.System;
            LabelOutputCompression.Location = new Point(116, 24);
            LabelOutputCompression.Size = new Size(452, 56);
            LabelOutputCompression.Text = options.Strings[113]+" "+options.Strings[155];
            LabelOutputCompression.UseMnemonic = false;
            #endregion
            #region <7-Zip Options>
            // 
            // Groupbox7ZipOptions
            // 
            Groupbox7ZipOptions.Controls.Add(Label7ZipOptions);
            Groupbox7ZipOptions.Controls.Add(LabelMaxRAM);
            Groupbox7ZipOptions.Controls.Add(LabelMaxDict);
            Groupbox7ZipOptions.Controls.Add(LabelUltraDict);
            Groupbox7ZipOptions.Controls.Add(NumericMaximumRAM);
            Groupbox7ZipOptions.Controls.Add(TextboxMaxDictionary);
            Groupbox7ZipOptions.Controls.Add(TextboxUltraDictionary);
            Groupbox7ZipOptions.Controls.Add(ButtonResetMaximumRAM);
            Groupbox7ZipOptions.FlatStyle = FlatStyle.System;
            Groupbox7ZipOptions.Location = new Point(8, 216);
            Groupbox7ZipOptions.Size = new Size(576, 128);
            Groupbox7ZipOptions.Text = options.Strings[114];
            // 
            // NumericMaximumRAM
            // 
            NumericMaximumRAM.Increment = new System.Decimal(9.5);
            NumericMaximumRAM.Location = new Point(8, 24);
            NumericMaximumRAM.Maximum = new System.Decimal(2498);
            NumericMaximumRAM.Size = new Size(52, 23);
            NumericMaximumRAM.TextAlign = HorizontalAlignment.Right;
            NumericMaximumRAM.UpDownAlign = LeftRightAlignment.Left;
            NumericMaximumRAM.TextChanged += EventListener;
            NumericMaximumRAM.Enter += FocusEventListener;
            // 
            // LabelMaxRAM
            // 
            LabelMaxRAM.Location = new Point(60, 24);
            LabelMaxRAM.Size = new Size(104, 24);
            LabelMaxRAM.Text =options.Strings[115];
            LabelMaxRAM.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // TextboxMaxDictionary
            // 
            TextboxMaxDictionary.Location = new Point(8, 48);
            TextboxMaxDictionary.ReadOnly = true;
            TextboxMaxDictionary.Size = new Size(52, 23);
            TextboxMaxDictionary.TextAlign = HorizontalAlignment.Right;
            TextboxMaxDictionary.TabStop = false;
            // 
            // LabelMaxDict
            // 
            LabelMaxDict.Location = new Point(60, 48);
            LabelMaxDict.Size = new Size(104, 24);
            LabelMaxDict.Text = options.Strings[116];
            LabelMaxDict.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // TextboxUltraDictionary
            // 
            TextboxUltraDictionary.Location = new Point(8, 72);
            TextboxUltraDictionary.ReadOnly = true;
            TextboxUltraDictionary.Size = new Size(52, 23);
            TextboxUltraDictionary.TabStop = false;
            TextboxUltraDictionary.TextAlign = HorizontalAlignment.Right;
            // 
            // LabelUltraDict
            // 
            LabelUltraDict.Location = new Point(60, 72);
            LabelUltraDict.Size = new Size(104, 24);
            LabelUltraDict.Text = options.Strings[117];
            LabelUltraDict.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ButtonResetMaximumRAM
            // 
            ButtonResetMaximumRAM.FlatStyle = FlatStyle.System;
            ButtonResetMaximumRAM.Location = new Point(8, 96);
            ButtonResetMaximumRAM.Size = new Size(152, 24);
            ButtonResetMaximumRAM.Text = options.Strings[118];
            ButtonResetMaximumRAM.Click += EventListener;
            // 
            // Label7ZipOptions
            // 
            Label7ZipOptions.FlatStyle = FlatStyle.System;
            Label7ZipOptions.Location = new Point(176, 24);
            Label7ZipOptions.Size = new Size(392, 90);
            Label7ZipOptions.Text = options.Strings[119];
            Label7ZipOptions.UseMnemonic = false;
            #endregion
            #endregion
            #region <Page 4 - Advanced>
            // 
            // Tab4
            // 
            Tab4.BorderStyle = BorderStyle.Fixed3D;
            Tab4.Controls.Add(GroupboxBackground);
            Tab4.Controls.Add(GroupboxArguments);
            Tab4.Controls.Add(GroupboxDeleteFiles);
            Tab4.Controls.Add(GroupboxResetLanguage);
            Tab4.Controls.Add(GroupboxBiasZonePriority);
            Tab4.Size = new Size(596, 361);
            Tab4.Text = options.Strings[96];
            #region <Background>
            // 
            // GroupboxBackground
            // 
            GroupboxBackground.Controls.Add(LabelBackground);
            GroupboxBackground.Controls.Add(CheckboxBackground);
            GroupboxBackground.FlatStyle = FlatStyle.System;
            GroupboxBackground.Location = new Point(8, 8);
            GroupboxBackground.Size = new Size(336, 80);
            GroupboxBackground.Text = options.Strings[120];
            // 
            // CheckboxBackground
            // 
            CheckboxBackground.CheckedChanged += EventListener;
            CheckboxBackground.FlatStyle = FlatStyle.System;
            CheckboxBackground.Location = new Point(8, 18);
            CheckboxBackground.Size = new Size(16, 20);
            // 
            // LabelBackground
            // 
            LabelBackground.FlatStyle = FlatStyle.System;
            LabelBackground.Location = new Point(24, 18);
            LabelBackground.Size = new Size(304, 54);
            LabelBackground.Text = options.Strings[121];
            LabelBackground.Click += EventListener;
            #endregion
            #region <Arguments>
            // 
            // GroupboxArguments
            // 
            GroupboxArguments.Controls.Add(LabelArguments);
            GroupboxArguments.Controls.Add(CheckboxArguments);
            GroupboxArguments.FlatStyle = FlatStyle.System;
            GroupboxArguments.Location = new Point(8, 96);
            GroupboxArguments.Size = new Size(336, 80);
            GroupboxArguments.Text = options.Strings[122];
            // 
            // CheckboxArguments
            // 
            CheckboxArguments.CheckedChanged += EventListener;
            CheckboxArguments.FlatStyle = FlatStyle.System;
            CheckboxArguments.Location = new Point(8, 18);
            CheckboxArguments.Size = new Size(16, 20);
            // 
            // LabelArguments
            // 
            LabelArguments.FlatStyle = FlatStyle.System;
            LabelArguments.Location = new Point(24, 18);
            LabelArguments.Size = new Size(304, 54);
            LabelArguments.Text = options.Strings[123];
            LabelArguments.Click += EventListener;
            #endregion
            #region <Delete Files>
            // 
            // GroupboxDeleteFiles
            // 
            GroupboxDeleteFiles.Controls.Add(LabelDeleteFiles);
            GroupboxDeleteFiles.Controls.Add(CheckboxDeleteFiles);
            GroupboxDeleteFiles.FlatStyle = FlatStyle.System;
            GroupboxDeleteFiles.Location = new Point(8, 184);
            GroupboxDeleteFiles.Size = new Size(336, 80);
            GroupboxDeleteFiles.Text = options.Strings[124];
            // 
            // CheckboxDeleteFiles
            // 
            CheckboxDeleteFiles.FlatStyle = FlatStyle.System;
            CheckboxDeleteFiles.Location = new Point(8, 18);
            CheckboxDeleteFiles.Size = new Size(16, 20);
            CheckboxDeleteFiles.CheckedChanged += EventListener;
            // 
            // LabelDeleteFiles
            // 
            LabelDeleteFiles.FlatStyle = FlatStyle.System;
            LabelDeleteFiles.Location = new Point(24, 18);
            LabelDeleteFiles.Size = new Size(304, 54);
            LabelDeleteFiles.Text = options.Strings[125];
            LabelDeleteFiles.Click += EventListener;
            #endregion
            #region <Reset Language>
            // 
            // GroupboxResetLanguage
            // 
            GroupboxResetLanguage.Controls.Add(LabelResetLanguage);
            GroupboxResetLanguage.Controls.Add(CheckboxResetLanguage);
            GroupboxResetLanguage.FlatStyle = FlatStyle.System;
            GroupboxResetLanguage.Location = new Point(8, 272);
            GroupboxResetLanguage.Size = new Size(336, 72);
            GroupboxResetLanguage.Text = options.Strings[146];
            // 
            // CheckboxResetLanguage
            // 
            CheckboxResetLanguage.FlatStyle = FlatStyle.System;
            CheckboxResetLanguage.Location = new Point(8, 18);
            CheckboxResetLanguage.Size = new Size(16, 20);
            CheckboxResetLanguage.CheckedChanged += EventListener;
            // 
            // LabelResetLanguage
            // 
            LabelResetLanguage.FlatStyle = FlatStyle.System;
            LabelResetLanguage.Location = new Point(24, 18);
            LabelResetLanguage.Size = new Size(304, 50);
            LabelResetLanguage.Text = options.Strings[147];
            LabelResetLanguage.Click += EventListener;
            #endregion
            #region <Bias Zone Priority>
            // 
            // GroupboxBiasZonePriority
            // 
            GroupboxBiasZonePriority.Controls.Add(LabelBiasZonePriority);
            GroupboxBiasZonePriority.Controls.Add(ListboxBiasPriority);
            GroupboxBiasZonePriority.Controls.Add(ButtonMoveUp);
            GroupboxBiasZonePriority.Controls.Add(ButtonMoveDown);
            GroupboxBiasZonePriority.FlatStyle = FlatStyle.System;
            GroupboxBiasZonePriority.Location = new Point(352, 8);
            GroupboxBiasZonePriority.Size = new Size(232, 336);
            GroupboxBiasZonePriority.Text = options.Strings[128];
            // 
            // LabelBiasZonePriority
            // 
            LabelBiasZonePriority.FlatStyle = FlatStyle.System;
            LabelBiasZonePriority.Location = new Point(8, 144);
            LabelBiasZonePriority.Size = new Size(216, 184);
            LabelBiasZonePriority.Text = options.Strings[129];
            LabelBiasZonePriority.UseMnemonic = false;
            // 
            // ListboxBiasPriority
            // 
            ListboxBiasPriority.IntegralHeight = false;
            ListboxBiasPriority.ItemHeight = 18;
            ListboxBiasPriority.Location = new Point(8, 24);
            ListboxBiasPriority.Size = new Size(188, 112);
            // 
            // ButtonMoveUp
            // 
            ButtonMoveUp.Image = Image.FromStream(Assembly.GetEntryAssembly().GetManifestResourceStream("GoodMerge.Up.ico"));
            ButtonMoveUp.Location = new Point(204, 24);
            ButtonMoveUp.Size = new Size(17, 28);
            ButtonMoveUp.Click += EventListener;
            // 
            // ButtonMoveDown
            // 
            ButtonMoveDown.Image = Image.FromStream(Assembly.GetEntryAssembly().GetManifestResourceStream("GoodMerge.Down.ico"));
            ButtonMoveDown.Location = new Point(204, 108);
            ButtonMoveDown.Size = new Size(17, 28);
            ButtonMoveDown.Click += EventListener;
            #endregion
            #endregion
            #region <Page 5 - Other Locations>
            // 
            // Tab5
            // 
            Tab5.BorderStyle = BorderStyle.Fixed3D;
            Tab5.Controls.Add(Groupbox7ZipLocation);
            Tab5.Controls.Add(GroupboxRarLocation);
            Tab5.Controls.Add(GroupboxAceLocation);
            Tab5.Controls.Add(GroupboxWorkingFolder);
            Tab5.Size = new Size(596, 361);
            Tab5.Text = options.Strings[97];
            #region <7-Zip Location>
            // 
            // Groupbox7ZipLocation
            // 
            Groupbox7ZipLocation.Controls.Add(Textbox7ZipLocation);
            Groupbox7ZipLocation.Controls.Add(ButtonDefault7ZipLocation);
            Groupbox7ZipLocation.Controls.Add(ButtonBrowse7ZipLocation);
            Groupbox7ZipLocation.FlatStyle = FlatStyle.System;
            Groupbox7ZipLocation.Location = new Point(8, 12);
            Groupbox7ZipLocation.Size = new Size(576, 60);
            Groupbox7ZipLocation.Text = options.Strings[130]+" 7-Zip";
            // 
            // Textbox7ZipLocation
            // 
            Textbox7ZipLocation.Location = new Point(8, 24);
            Textbox7ZipLocation.Size = new Size(400, 23);
            Textbox7ZipLocation.Text = "";
            Textbox7ZipLocation.Enter += FocusEventListener;
            Textbox7ZipLocation.Leave += EventListener;
            // 
            // ButtonDefault7ZipLocation
            // 
            ButtonDefault7ZipLocation.FlatStyle = FlatStyle.System;
            ButtonDefault7ZipLocation.Location = new Point(416, 24);
            ButtonDefault7ZipLocation.Size = new Size(72, 24);
            ButtonDefault7ZipLocation.Text = options.Strings[103];
            ButtonDefault7ZipLocation.Click += EventListener;
            // 
            // ButtonBrowse7ZipLocation
            // 
            ButtonBrowse7ZipLocation.FlatStyle = FlatStyle.System;
            ButtonBrowse7ZipLocation.Location = new Point(496, 24);
            ButtonBrowse7ZipLocation.Size = new Size(72, 24);
            ButtonBrowse7ZipLocation.Text = options.Strings[104];
            ButtonBrowse7ZipLocation.Click += EventListener;
            #endregion
            #region <Rar Location>
            // 
            // GroupboxRarLocation
            // 
            GroupboxRarLocation.Controls.Add(TextboxRarLocation);
            GroupboxRarLocation.Controls.Add(ButtonDefaultRarLocation);
            GroupboxRarLocation.Controls.Add(ButtonBrowseRarLocation);
            GroupboxRarLocation.FlatStyle = FlatStyle.System;
            GroupboxRarLocation.Location = new Point(8, 88);
            GroupboxRarLocation.Size = new Size(576, 56);
            GroupboxRarLocation.Text = options.Strings[130]+" Rar";
            // 
            // TextboxRarLocation
            // 
            TextboxRarLocation.Location = new Point(8, 24);
            TextboxRarLocation.Size = new Size(400, 23);
            TextboxRarLocation.Text = "";
            TextboxRarLocation.Enter += FocusEventListener;
            TextboxRarLocation.Leave += EventListener;
            // 
            // ButtonDefaultRarLocation
            // 
            ButtonDefaultRarLocation.FlatStyle = FlatStyle.System;
            ButtonDefaultRarLocation.Location = new Point(416, 24);
            ButtonDefaultRarLocation.Size = new Size(72, 24);
            ButtonDefaultRarLocation.Text = options.Strings[103];
            ButtonDefaultRarLocation.Click += EventListener;
            // 
            // ButtonBrowseRarLocation
            // 
            ButtonBrowseRarLocation.FlatStyle = FlatStyle.System;
            ButtonBrowseRarLocation.Location = new Point(496, 24);
            ButtonBrowseRarLocation.Size = new Size(72, 24);
            ButtonBrowseRarLocation.Text = options.Strings[104];
            ButtonBrowseRarLocation.Click += EventListener;
            #endregion
            #region <Ace Location>
            // 
            // GroupboxAceLocation
            // 
            GroupboxAceLocation.Controls.Add(TextboxAceLocation);
            GroupboxAceLocation.Controls.Add(ButtonDefaultAceLocation);
            GroupboxAceLocation.Controls.Add(ButtonBrowseAceLocation);
            GroupboxAceLocation.FlatStyle = FlatStyle.System;
            GroupboxAceLocation.Location = new Point(8, 160);
            GroupboxAceLocation.Size = new Size(576, 56);
            GroupboxAceLocation.Text = options.Strings[130]+" Ace";
            // 
            // TextboxAceLocation
            // 
            TextboxAceLocation.Location = new Point(8, 24);
            TextboxAceLocation.Size = new Size(400, 23);
            TextboxAceLocation.Text = "";
            TextboxAceLocation.Enter += FocusEventListener;
            TextboxAceLocation.Leave += EventListener;
            // 
            // ButtonDefaultAceLocation
            // 
            ButtonDefaultAceLocation.FlatStyle = FlatStyle.System;
            ButtonDefaultAceLocation.Location = new Point(416, 24);
            ButtonDefaultAceLocation.Size = new Size(72, 24);
            ButtonDefaultAceLocation.Text = options.Strings[103];
            ButtonDefaultAceLocation.Click += EventListener;
            // 
            // ButtonBrowseAceLocation
            // 
            ButtonBrowseAceLocation.FlatStyle = FlatStyle.System;
            ButtonBrowseAceLocation.Location = new Point(496, 24);
            ButtonBrowseAceLocation.Size = new Size(72, 24);
            ButtonBrowseAceLocation.Text = options.Strings[104];
            ButtonBrowseAceLocation.Click += EventListener;
            #endregion
            #region <Working Folder>
            // 
            // GroupboxWorkingFolder
            // 
            GroupboxWorkingFolder.Controls.Add(LabelWorkingFolder);
            GroupboxWorkingFolder.Controls.Add(TextboxWorkingFolder);
            GroupboxWorkingFolder.Controls.Add(ButtonDefaultWorkingFolder);
            GroupboxWorkingFolder.Controls.Add(ButtonBrowseWorkingFolder);
            GroupboxWorkingFolder.FlatStyle = FlatStyle.System;
            GroupboxWorkingFolder.Location = new Point(8, 232);
            GroupboxWorkingFolder.Size = new Size(576, 88);
            GroupboxWorkingFolder.Text = options.Strings[126];
            // 
            // TextboxWorkingFolder
            // 
            TextboxWorkingFolder.Location = new Point(8, 24);
            TextboxWorkingFolder.Size = new Size(400, 23);
            TextboxWorkingFolder.TextChanged += EventListener;
            TextboxWorkingFolder.Enter += FocusEventListener;
            // 
            // ButtonDefaultWorkingFolder
            // 
            ButtonDefaultWorkingFolder.FlatStyle = FlatStyle.System;
            ButtonDefaultWorkingFolder.Location = new Point(416, 24);
            ButtonDefaultWorkingFolder.Size = new Size(72, 24);
            ButtonDefaultWorkingFolder.Text = options.Strings[103];
            ButtonDefaultWorkingFolder.Click += EventListener;
            // 
            // ButtonBrowseWorkingFolder
            // 
            ButtonBrowseWorkingFolder.FlatStyle = FlatStyle.System;
            ButtonBrowseWorkingFolder.Location = new Point(496, 24);
            ButtonBrowseWorkingFolder.Size = new Size(72, 24);
            ButtonBrowseWorkingFolder.Text = options.Strings[104];
            ButtonBrowseWorkingFolder.Click += EventListener;
            // 
            // LabelWorkingFolder
            // 
            LabelWorkingFolder.FlatStyle = FlatStyle.System;
            LabelWorkingFolder.Location = new Point(8, 56);
            LabelWorkingFolder.Size = new Size(560, 24);
            LabelWorkingFolder.Text = options.Strings[127];
            LabelWorkingFolder.UseMnemonic = false;
            #endregion
            #endregion
            #region <Page 6 - About>
            // 
            // Tab6
            // 
            Tab6.BorderStyle = BorderStyle.Fixed3D;
            Tab6.Controls.Add(GroupboxAbout);
            Tab6.Size = new Size(596, 356);
            Tab6.Text = options.Strings[98];
            // 
            // GroupboxAbout
            // 
            GroupboxAbout.Controls.Add(LabelAbout);
            GroupboxAbout.Controls.Add(LinkWebpage);
            GroupboxAbout.FlatStyle = FlatStyle.System;
            GroupboxAbout.Location = new Point(40, 32);
            GroupboxAbout.Size = new Size(512, 280);
            GroupboxAbout.Text = options.Strings[169];
            // 
            // LabelAbout
            // 
            LabelAbout.FlatStyle = FlatStyle.System;
            LabelAbout.Location = new Point(8, 24);
            LabelAbout.Size = new Size(496, 216);
            LabelAbout.Text = options.Strings[131]+" "+Assembly.GetExecutingAssembly().GetName().Version.ToString()+" - 2006/01/08\nCopyright ©2004-2006 John Paul Taylor II (q^-o|o-^p) - BSD License\n\n"+options.Strings[132]+" kiczek, xphaze, Nebula, [vEX], kn, indigital, romar, kox, BRaiNL3Ss, CANI, Hakkk, accolon, SpkLeader, Bekir HIZ "+options.Strings[145];
            LabelAbout.UseMnemonic = false;
            // 
            // LinkWebpage
            // 
            LinkWebpage.FlatStyle = FlatStyle.System;
            LinkWebpage.LinkColor = Color.Blue;
            LinkWebpage.Location = new Point(8, 252);
            LinkWebpage.Size = new Size(496, 24);
            LinkWebpage.Text = "http://goodmerge.sourceforge.net/";
            LinkWebpage.TextAlign = ContentAlignment.TopCenter;
            LinkWebpage.UseMnemonic = false;
            LinkWebpage.VisitedLinkColor = Color.Blue;
            LinkWebpage.Click += EventListener;
            #endregion
            #region <Resumes>
            Tabs.ResumeLayout(false);
            Tab1.ResumeLayout(false);
            Tab2.ResumeLayout(false);
            Tab3.ResumeLayout(false);
            Tab4.ResumeLayout(false);
            Tab5.ResumeLayout(false);
            Tab6.ResumeLayout(false);
            GroupboxWelcome.ResumeLayout(false);
            GroupboxOutputFolder.ResumeLayout(false);
            GroupboxSourceFolder.ResumeLayout(false);
            GroupboxHaveFile.ResumeLayout(false);
            GroupboxSourceCompression.ResumeLayout(false);
            Groupbox7ZipOptions.ResumeLayout(false);
            GroupboxOutputCompression.ResumeLayout(false);
            GroupboxDeleteFiles.ResumeLayout(false);
            GroupboxArguments.ResumeLayout(false);
            GroupboxBiasZonePriority.ResumeLayout(false);
            GroupboxBackground.ResumeLayout(false);
            GroupboxWorkingFolder.ResumeLayout(false);
            GroupboxRarLocation.ResumeLayout(false);
            Groupbox7ZipLocation.ResumeLayout(false);
            GroupboxAceLocation.ResumeLayout(false);
            GroupboxAbout.ResumeLayout(false);
            ResumeLayout(false);
            #endregion
        }

        private void searchForProgs() {
            string prevSource=ListboxSourceCompression.Text;
            string prevOutput=ListboxOutputCompression.Text;
            if (prevSource.Equals("")) prevSource = options.SourceCompression;
            if (prevOutput.Equals("")) prevOutput = options.OutputCompression;
            SourceList = new ArrayList();
            OutputList = new ArrayList();
            ListboxSourceCompression.Items.Clear();
            ListboxOutputCompression.Items.Clear();
            SourceList.Add("none");
            if (!options.SevenZip.Equals("") && File.Exists(options.SevenZip)) { SourceList.Add("7z"); SourceList.Add("zip"); OutputList.Add("7z"); OutputList.Add("zip"); }
            if (!options.Rar.Equals("") && File.Exists(options.Rar)) { SourceList.Add("rar"); OutputList.Add("rar"); }
            if (!options.Ace.Equals("") && File.Exists(options.Ace)) { SourceList.Add("ace"); OutputList.Add("ace"); }
            ListboxSourceCompression.Items.AddRange(SourceList.ToArray());
            if (ListboxSourceCompression.Items.Contains(prevSource)) ListboxSourceCompression.SelectedIndex=ListboxSourceCompression.FindStringExact(prevSource);
            else ListboxSourceCompression.SelectedIndex=0;
            ListboxOutputCompression.Items.AddRange(OutputList.ToArray());
            if (ListboxOutputCompression.Items.Contains(prevOutput)) ListboxOutputCompression.SelectedIndex=ListboxOutputCompression.FindStringExact(prevOutput);
            else if (ListboxOutputCompression.Items.Count!=0) ListboxOutputCompression.SelectedIndex=0;
            else Groupbox7ZipOptions.Enabled=false;
        }

        private void searchForSets() {
            char[] SLASH = new char[] {'/'};
            ROMSetList = new ArrayList();
            ListboxROMSet.Items.Clear();
            XmlTextReader xtr;
            string name, version;
            foreach (FileInfo file in new DirectoryInfo(options.ProgramFolder).GetFiles("*.xmdb")) {
                xtr=null;
                try {
                    xtr = new XmlTextReader(file.FullName);

                    // Check that the root node is <romsets>.
                    if (xtr.MoveToContent()!=XmlNodeType.None && xtr.Name.Equals("romsets")) {
                        xtr.Read();
                        // Read each child <set> node.
                        while (xtr.MoveToContent()!=XmlNodeType.None) {
                            if (xtr.Name.Equals("set")) {
                                name=null;
                                version=null;
                                // Check that attributes name= and version= are defined.
                                while (xtr.MoveToNextAttribute()) {
                                    if (xtr.Name.Equals("name")) name=xtr.Value;
                                    else if (xtr.Name.Equals("version")) version=xtr.Value;
                                }
                                if (name!=null && version!=null) {
                                    ROMSetList.Add(name+" "+version);
                                }
                            }
                            xtr.Skip();
                        }
                    }
                }
                catch { }
                finally { if (xtr!=null) xtr.Close(); }
            }
            ListboxROMSet.Items.AddRange(ROMSetList.ToArray());
        }

        private void eventHandler(object s, EventArgs e) {
                #region <Text Boxes>
            if (s==Textbox7ZipLocation) {
                options.SevenZip=Textbox7ZipLocation.Text;
                searchForProgs();
            }
            else if (s==TextboxAceLocation) {
                options.Ace=TextboxAceLocation.Text;
                searchForProgs();
            }
            else if (s==TextboxRarLocation) {
                options.Rar=TextboxRarLocation.Text;
                searchForProgs();
            }
            else if (s==TextboxWorkingFolder) {
                options.WorkingFolder=TextboxWorkingFolder.Text;
            }
                #endregion
                #region <List Boxes>
            else if (s==ListboxROMSet) {
                this.Text=options.Strings[0]+" - "+ListboxROMSet.Text;
                options.SetName=(ListboxROMSet.Text.Split(new char[] {' '}))[0];
                options.Version=(ListboxROMSet.Text.Split(new char[] {' '}))[1];
                if (options.SetNames.Contains(options.SetName)) {
                    TextboxHaveFile.Text = options.HaveFiles[options.SetName].ToString();
                    TextboxSourceFolder.Text = options.SourceFolders[options.SetName].ToString();
                    TextboxOutputFolder.Text = options.OutputFolders[options.SetName].ToString();
                }
                else {
                    TextboxHaveFile.Text = "";
                    TextboxSourceFolder.Text = "";
                    TextboxOutputFolder.Text = "";
                }
                if (TextboxHaveFile.Text.Equals("")) TextboxHaveFile.Text=options.CurrentFolder+options.SetName+"Have.txt";
                if (TextboxSourceFolder.Text.Equals("")) TextboxSourceFolder.Text=options.CurrentFolder+options.SetName+"Ren\\";
                if (TextboxOutputFolder.Text.Equals("")) TextboxOutputFolder.Text = options.CurrentFolder+options.SetName+"Merge\\";
                TextboxHaveFile.Select(TextboxHaveFile.Text.Length, 0);
                TextboxSourceFolder.Select(TextboxSourceFolder.Text.Length, 0);
                TextboxOutputFolder.Select(TextboxOutputFolder.Text.Length, 0);
            }
            else if (s==ListboxOutputCompression) {
                options.OutputCompression=ListboxOutputCompression.Text;
                if (ListboxOutputCompression.Text.Equals("7z")) Groupbox7ZipOptions.Enabled=true;
                else Groupbox7ZipOptions.Enabled=false;
            }
            else if (s==ListboxSourceCompression) {
                options.SourceCompression=ListboxSourceCompression.Text;
                if (ListboxSourceCompression.Text.Equals("none")) GroupboxDeleteFiles.Enabled=true;
                else {
                    CheckboxDeleteFiles.Checked=false;
                    GroupboxDeleteFiles.Enabled=false;
                }
            }
                #endregion
                #region <Check Boxes>
            else if (s==CheckboxDeleteFiles) {
                if (CheckboxDeleteFiles.Checked) {
                    if (MessageBox.Show(options.Strings[133], options.Strings[134], MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes) CheckboxDeleteFiles.Checked=false;
                }
            }
            else if (s==CheckboxResetLanguage) {
                options.Language="";
                ButtonMerge.Enabled=false;
                GroupboxResetLanguage.Enabled=false;
            }
            else if (s==CheckboxArguments) {
                options.Arguments=CheckboxArguments.Checked;
            }
            else if (s==CheckboxBackground) {
                options.Background=CheckboxBackground.Checked;
            }
                #endregion
                #region <Numerics>
            else if (s==NumericMaximumRAM) {
                options.SetDesiredRAM((int)(NumericMaximumRAM.Value));
                TextboxUltraDictionary.Text=options.UltraDict.ToString();
                TextboxMaxDictionary.Text=options.MaxDict.ToString();
            }
                #endregion
                #region <Buttons - Browse>
            else if (s==ButtonBrowseSourceFolder) {
                DialogOpenFolder.Description=options.Strings[135];
                DialogOpenFolder.SelectedPath=TextboxSourceFolder.Text;
                if (DialogOpenFolder.ShowDialog() == DialogResult.OK) {
                    if (DialogOpenFolder.SelectedPath.EndsWith("\\")) TextboxSourceFolder.Text = DialogOpenFolder.SelectedPath;
                    else TextboxSourceFolder.Text = DialogOpenFolder.SelectedPath+"\\";
                    TextboxSourceFolder.Select(TextboxSourceFolder.Text.Length, 0);
                }
            }
            else if (s==ButtonBrowseOutputFolder) {
                DialogOpenFolder.Description=options.Strings[136];
                DialogOpenFolder.SelectedPath=TextboxOutputFolder.Text;
                if (DialogOpenFolder.ShowDialog() == DialogResult.OK) {
                    if (DialogOpenFolder.SelectedPath.EndsWith("\\")) TextboxOutputFolder.Text = DialogOpenFolder.SelectedPath;
                    else TextboxOutputFolder.Text = DialogOpenFolder.SelectedPath+"\\";
                    TextboxOutputFolder.Select(TextboxOutputFolder.Text.Length, 0);
                }
            }
            else if (s==ButtonBrowseWorkingFolder) {
                DialogOpenFolder.Description=options.Strings[137];
                DialogOpenFolder.SelectedPath=TextboxWorkingFolder.Text;
                if (DialogOpenFolder.ShowDialog() == DialogResult.OK) {
                    if (DialogOpenFolder.SelectedPath.EndsWith("\\")) TextboxWorkingFolder.Text = DialogOpenFolder.SelectedPath;
                    else TextboxWorkingFolder.Text = DialogOpenFolder.SelectedPath+"\\";
                    TextboxWorkingFolder.Select(TextboxSourceFolder.Text.Length, 0);
                }
            }
            else if (s==ButtonBrowseHaveFile) {
                if (DialogOpenHaveFile.ShowDialog() == DialogResult.OK) {
                    TextboxHaveFile.Text = DialogOpenHaveFile.FileName;
                    TextboxHaveFile.Select(TextboxHaveFile.Text.Length, 0);
                }
            }
            else if (s==ButtonBrowse7ZipLocation) {
                DialogOpenExeFile.InitialDirectory=options.SevenZip;
                DialogOpenExeFile.Filter="7za.exe|7za.exe|*.exe|*.exe";
                if (DialogOpenExeFile.ShowDialog() == DialogResult.OK) {
                    Textbox7ZipLocation.Text = DialogOpenExeFile.FileName;
                    Textbox7ZipLocation.Select(Textbox7ZipLocation.Text.Length, 0);
                    options.SevenZip=Textbox7ZipLocation.Text;
                    searchForProgs();
                }
            }
            else if (s==ButtonBrowseRarLocation) {
                DialogOpenExeFile.InitialDirectory=options.Rar;
                DialogOpenExeFile.Filter="rar.exe|rar.exe|*.exe|*.exe";
                if (DialogOpenExeFile.ShowDialog() == DialogResult.OK) {
                    TextboxRarLocation.Text = DialogOpenExeFile.FileName;
                    TextboxRarLocation.Select(TextboxRarLocation.Text.Length, 0);
                    options.Rar=TextboxRarLocation.Text;
                    searchForProgs();
                }
            }
            else if (s==ButtonBrowseAceLocation) {
                DialogOpenExeFile.InitialDirectory=options.Ace;
                DialogOpenExeFile.Filter="ace32.exe|ace32.exe|*.exe|*.exe";
                if (DialogOpenExeFile.ShowDialog() == DialogResult.OK) {
                    TextboxAceLocation.Text = DialogOpenExeFile.FileName;
                    TextboxAceLocation.Select(TextboxAceLocation.Text.Length, 0);
                    options.Ace=TextboxAceLocation.Text;
                    searchForProgs();
                }
            }
                #endregion
                #region <Buttons - Merge>
            else if (s==ButtonMerge) {
                if ((Control.ModifierKeys & Keys.Control) != 0) options.TestMode=true;
                else options.TestMode=false;
                if (!TextboxSourceFolder.Text.Equals("") && !TextboxSourceFolder.Text.EndsWith("\\")) TextboxSourceFolder.Text+="\\";
                if (!TextboxOutputFolder.Text.Equals("") && !TextboxOutputFolder.Text.EndsWith("\\")) TextboxOutputFolder.Text+="\\";
                if (!TextboxWorkingFolder.Text.Equals("") && !TextboxWorkingFolder.Text.EndsWith("\\")) TextboxWorkingFolder.Text+="\\";
                if (ListboxOutputCompression.Items.Count==0) ErrorMessage(options.Strings[138], 5);
                else if (ListboxROMSet.SelectedIndex==-1) ErrorMessage(options.Strings[139], 1);
                else if (TextboxSourceFolder.Text.Equals("") || TextboxHaveFile.Text.Equals("") || TextboxOutputFolder.Text.Equals("") || TextboxWorkingFolder.Text.Equals("")) {
                    if (TextboxWorkingFolder.Text.Equals("")) {
                        TextboxWorkingFolder.Text = options.WorkingFolder;
                        TextboxWorkingFolder.Select(TextboxWorkingFolder.Text.Length, 0);
                    }
                    ListboxROMSet.SetSelected(ListboxROMSet.SelectedIndex, true);
                    ErrorMessage(options.Strings[140], 2);
                }
                else if (Groupbox7ZipOptions.Enabled && options.MaxDict<1) ErrorMessage(options.Strings[141], 3);
                else if (!Directory.Exists(TextboxSourceFolder.Text) && !options.TestMode) ErrorMessage(options.Strings[142], 2);
                else if (!Directory.Exists(TextboxWorkingFolder.Text)) ErrorMessage(options.Strings[143], 5);
                else if (!File.Exists(TextboxHaveFile.Text)) ErrorMessage(options.Strings[164], 2);
                else {
                    if (options.SetNames.Contains(options.SetName)) {
                        options.SourceFolders.Remove(options.SetName);
                        options.OutputFolders.Remove(options.SetName);
                        options.HaveFiles.Remove(options.SetName);
                    }
                    else options.SetNames.Add(options.SetName);
                    options.HaveFiles.Add(options.SetName, TextboxHaveFile.Text);
                    options.SourceFolders.Add(options.SetName, TextboxSourceFolder.Text);
                    options.OutputFolders.Add(options.SetName, TextboxOutputFolder.Text);
                    options.SourceFolder=TextboxSourceFolder.Text;
                    options.OutputFolder=TextboxOutputFolder.Text;
                    options.HaveFile=TextboxHaveFile.Text;
                    options.DeleteFiles=CheckboxDeleteFiles.Checked;
                    options.MainWindowLocation=this.Location;
                    this.DialogResult = DialogResult.OK;
                    this.Dispose(true);
                }
            }
                #endregion
                #region <Buttons - Default>
            else if (s==ButtonRefreshROMSets) {
                Text=options.Strings[0];
                string prev=ListboxROMSet.Text;
                searchForSets();
                if (ListboxROMSet.Items.Contains(prev)) ListboxROMSet.SelectedIndex=ListboxROMSet.FindStringExact(prev);
            }
            else if (s==ButtonResetMaximumRAM) {
                options.SetDesiredRAM(options.TotalRAM-114);
                NumericMaximumRAM.Value = options.DesiredRAM;
            }
            else if (s==ButtonDefaultSourceFolder) {
                if (ListboxROMSet.SelectedIndex!=-1) {
                    TextboxSourceFolder.Text=options.CurrentFolder+options.SetName+"Ren\\";
                    TextboxSourceFolder.Select(TextboxSourceFolder.Text.Length, 0);
                }
                else TextboxSourceFolder.Text="";
            }
            else if (s==ButtonDefaultOutputFolder) {
                if (ListboxROMSet.SelectedIndex!=-1) {
                    TextboxOutputFolder.Text = options.CurrentFolder+options.SetName+"Merge\\";
                    TextboxOutputFolder.Select(TextboxOutputFolder.Text.Length, 0);
                }
                else TextboxOutputFolder.Text="";
            }
            else if (s==ButtonDefaultWorkingFolder) {
                TextboxWorkingFolder.Text=options.CurrentFolder;
                TextboxWorkingFolder.Select(TextboxWorkingFolder.Text.Length, 0);
            }
            else if (s==ButtonDefaultHaveFile) {
                if (ListboxROMSet.SelectedIndex!=-1) {
                    TextboxHaveFile.Text=options.CurrentFolder+options.SetName+"Have.txt";
                    TextboxHaveFile.Select(TextboxHaveFile.Text.Length, 0);
                }
                else TextboxHaveFile.Text="";
            }
            else if (s==ButtonDefault7ZipLocation) {
                options.Default7Zip();
                Textbox7ZipLocation.Text=options.SevenZip;
                searchForProgs();
            }
            else if (s==ButtonDefaultRarLocation) {
                options.DefaultRar();
                TextboxRarLocation.Text=options.Rar;
                searchForProgs();
            }
            else if (s==ButtonDefaultAceLocation) {
                options.DefaultAce();
                TextboxAceLocation.Text=options.Ace;
                searchForProgs();
            }
                #endregion
                #region <Buttons - Move>
            else if (s==ButtonNext) {
                if (Tabs.SelectedIndex != Tabs.Controls.Count-1) Tabs.SelectedIndex+=1;
            }
            else if (s==ButtonBack) {
                if (Tabs.SelectedIndex != 0) Tabs.SelectedIndex-=1;
            }
            else if (s==ButtonMoveUp) {
                if (ListboxBiasPriority.SelectedIndex!=0) {
                    object p = ListboxBiasPriority.Items[ListboxBiasPriority.SelectedIndex];
                    ListboxBiasPriority.Items[ListboxBiasPriority.SelectedIndex]=ListboxBiasPriority.Items[ListboxBiasPriority.SelectedIndex-1];
                    ListboxBiasPriority.Items[ListboxBiasPriority.SelectedIndex-1]=p;
                    string b = options.Biases[ListboxBiasPriority.SelectedIndex];
                    options.Biases[ListboxBiasPriority.SelectedIndex]=options.Biases[ListboxBiasPriority.SelectedIndex-1];
                    options.Biases[ListboxBiasPriority.SelectedIndex-1]=b;
                    ListboxBiasPriority.SelectedIndex-=1;
                }
            }
            else if (s==ButtonMoveDown) {
                if (ListboxBiasPriority.SelectedIndex!=ListboxBiasPriority.Items.Count-1) {
                    object p = ListboxBiasPriority.Items[ListboxBiasPriority.SelectedIndex];
                    ListboxBiasPriority.Items[ListboxBiasPriority.SelectedIndex]=ListboxBiasPriority.Items[ListboxBiasPriority.SelectedIndex+1];
                    ListboxBiasPriority.Items[ListboxBiasPriority.SelectedIndex+1]=p;
                    string b = options.Biases[ListboxBiasPriority.SelectedIndex];
                    options.Biases[ListboxBiasPriority.SelectedIndex]=options.Biases[ListboxBiasPriority.SelectedIndex+1];
                    options.Biases[ListboxBiasPriority.SelectedIndex+1]=b;
                    ListboxBiasPriority.SelectedIndex+=1;
                }
            }
                #endregion
                #region <Tabs>
            else if (s==Tabs) {
                ButtonBack.Enabled=(Tabs.SelectedIndex!=0);
                ButtonNext.Enabled=(Tabs.SelectedIndex!=Tabs.Controls.Count-1);
                if (options.SetNames.Contains(options.SetName)) {
                    options.SourceFolders.Remove(options.SetName);
                    options.OutputFolders.Remove(options.SetName);
                    options.HaveFiles.Remove(options.SetName);
                }
                else options.SetNames.Add(options.SetName);
                options.HaveFiles.Add(options.SetName, TextboxHaveFile.Text);
                options.SourceFolders.Add(options.SetName, TextboxSourceFolder.Text);
                options.OutputFolders.Add(options.SetName, TextboxOutputFolder.Text);
            }
                #endregion
                #region <Link>
            else if (s==LinkWebpage) {
                try { System.Diagnostics.Process.Start(LinkWebpage.Text); }
                catch { }
            }
                #endregion
                #region <Labels>
            else if (s==LabelBackground) { CheckboxBackground.Checked=!CheckboxBackground.Checked; }
            else if (s==LabelArguments) { CheckboxArguments.Checked=!CheckboxArguments.Checked; }
            else if (s==LabelDeleteFiles) { CheckboxDeleteFiles.Checked=!CheckboxDeleteFiles.Checked; }
            else if (s==LabelResetLanguage) { CheckboxResetLanguage.Checked=!CheckboxResetLanguage.Checked; }
                #endregion
        }
        private void focusEventHandler(object sender, EventArgs e) {
            if (sender.GetType() == typeof(TextBox)) ((TextBox)sender).SelectAll();
            else ((Numeric)sender).SelectAll();
        }
        private void ErrorMessage(string s, int i) {
            Tabs.SelectedIndex=i-1;
            MessageBox.Show(s, options.Strings[0]+" - "+options.Strings[1], MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    class Numeric : NumericUpDown {
        public void SelectAll() {
            Select(0, Text.Length);
        }
    }
}