using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace GoodMerge {
	public class CLIHelpWindow : System.Windows.Forms.Form {
        public static Font NormalFont;
        public static Font BoldFont;
        public static Font UnderlineFont;
        public static Color NormalColor = Color.Black;
        public static Color OptionsColor = Color.Blue;

        private System.Windows.Forms.RichTextBox TextArea;

		public CLIHelpWindow(Options options) {
            NormalFont = new Font(options.FontName, options.FontSize, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));
            BoldFont = new Font(options.FontName, options.FontSize, FontStyle.Bold, GraphicsUnit.Point, ((Byte)(0)));
            UnderlineFont = new Font(options.FontName, options.FontSize, FontStyle.Underline, GraphicsUnit.Point, ((Byte)(0)));
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GoodMerge));
            this.TextArea = new RichTextBox();
            this.SuspendLayout();
            // 
            // Text
            // 
            this.TextArea.BackColor = Color.White;
            this.TextArea.Cursor = Cursors.IBeam;
            this.TextArea.DetectUrls = false;
            this.TextArea.Font = NormalFont;
            this.TextArea.ForeColor = NormalColor;
            this.TextArea.Location = new Point(0, 0);
            this.TextArea.ReadOnly = true;
            this.TextArea.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
            this.TextArea.Size = new Size(500, 400);
            this.TextArea.WordWrap=true;

            this.TextArea.SelectionFont=BoldFont;
            this.TextArea.AppendText(options.Strings[148]+": \""+options.Strings[0]+" ");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("<"+options.Strings[149]+"> <"+options.Strings[150]+">");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(" [");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("<"+options.Strings[151]+">");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText("]\"\n\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.SelectionFont=BoldFont;
            this.TextArea.AppendText("<"+options.Strings[149]+">");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.SelectionFont=NormalFont;
            this.TextArea.AppendText(" "+options.Strings[152]+"\n\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.SelectionFont=BoldFont;
            this.TextArea.AppendText("<"+options.Strings[150]+">");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.SelectionFont=NormalFont;
            this.TextArea.AppendText(" "+options.Strings[153]+"\n\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.SelectionFont=BoldFont;
            this.TextArea.AppendText("<"+options.Strings[151]+">");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.SelectionFont=NormalFont;
            this.TextArea.AppendText(" "+options.Strings[154]+"\n");
            int start = this.TextArea.Text.Length;
            this.TextArea.SelectionFont=UnderlineFont;
            this.TextArea.AppendText(options.Strings[95]+":\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("oc=zip|rar|ace|7z");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[112]+" - "+options.Strings[113]+"\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("sc=zip|rar|ace|7z");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[110]+" - "+options.Strings[111]+options.Strings[157]+"\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("mr={"+options.Strings[158]+"}");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[144]+" - "+options.Strings[159]+"\n");
            this.TextArea.SelectionFont=UnderlineFont;
            this.TextArea.AppendText("\n"+options.Strings[94]+":\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("sf={"+options.Strings[161]+"}");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[102]+" - "+options.Strings[105]+" "+options.Strings[160]+" \"<"+options.Strings[149]+">ren\".\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("of={"+options.Strings[161]+"}");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[106]+" - "+options.Strings[107]+" "+options.Strings[160]+" \"<"+options.Strings[149]+">Merge\". "+options.Strings[164]+"\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("hf={"+options.Strings[162]+"}");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[108]+" - "+options.Strings[109]+" "+options.Strings[160]+" \"<"+options.Strings[149]+">Have.txt\".\n");
            this.TextArea.AppendText(options.Strings[163]+"\n");
            this.TextArea.SelectionFont=UnderlineFont;
            this.TextArea.AppendText("\n"+options.Strings[96]+":\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("ubp");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[120]+" - "+options.Strings[121]+"\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("sca");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[122]+" - "+options.Strings[123]+"\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("dsf");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[124]+" - "+options.Strings[125]+"\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("wf={"+options.Strings[161]+"}");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[126]+" - "+options.Strings[127]+" "+options.Strings[160]+" "+options.Strings[165]+"\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("min");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[166]+"\n");
            this.TextArea.SelectionColor=OptionsColor;
            this.TextArea.AppendText("pm");
            this.TextArea.SelectionColor=NormalColor;
            this.TextArea.AppendText(": "+options.Strings[167]+"\n");
            this.TextArea.SelectAll();
            this.TextArea.SelectionHangingIndent=20;
            this.TextArea.Select(start, this.TextArea.Text.Length);
            this.TextArea.SelectionIndent=20;
            this.TextArea.Select(0,0);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new Size(6, 16);
            this.ClientSize = new Size(500, 400);
            this.Controls.Add(this.TextArea);
            this.Font = NormalFont;
            this.Icon = new Icon(Assembly.GetEntryAssembly().GetManifestResourceStream("GoodMerge.GoodMerge.ico"));
            this.Text = options.Strings[168];
            this.Resize += new EventHandler(CLIHelpWindow_Resize);
            this.ResumeLayout(false);
        }

        private void CLIHelpWindow_Resize(object sender, EventArgs e) {
            TextArea.Size=this.ClientSize;
        }
    }
}
