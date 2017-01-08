using System;
using System.Xml;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace GoodMerge {
	public class LanguagePicker : Form {
        public TreeView Tree;
        private Button ButtonOK;
        private ImageList Images;

		public LanguagePicker(XmlNodeList xnl) {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GoodMerge));
            Images = new ImageList();
            ButtonOK = new Button();
            Tree = new TreeView();
            // 
            // Images
            // 
            Images.ImageSize = new Size(16, 16);
            Images.TransparentColor = Color.Transparent;
            // 
            // Populate Tree
            //
            Images.Images.Add(Image.FromStream(Assembly.GetEntryAssembly().GetManifestResourceStream("GoodMerge.Blank.ico")));
            int loop=1;
            foreach (XmlNode xn in xnl) {
                try {
                    Images.Images.Add(Image.FromStream(Assembly.GetEntryAssembly().GetManifestResourceStream("GoodMerge."+xn.Value+".ico")));
                    Tree.Nodes.Add(new TreeNode(xn.Value, loop, loop));
                    loop++;
                }
                catch { Tree.Nodes.Add(new TreeNode(xn.Value, 0, 0)); }
            }
            if (loop!=0) Tree.SelectedNode=Tree.Nodes[0];
            // 
            // Tree
            // 
            Tree.FullRowSelect = true;
            Tree.ImageList = Images;
            Tree.Location = new Point(0, 0);
            Tree.ShowRootLines = false;
            Tree.ClientSize = new Size(140, Tree.ItemHeight*Tree.Nodes.Count);
            Tree.Scrollable=false;
            // 
            // ButtonOK
            // 
            ButtonOK.FlatStyle=FlatStyle.System;
            ButtonOK.Location = new Point(0, Tree.Height);
            ButtonOK.Size = new Size(140, 24);
            ButtonOK.Text = "&OK";
            ButtonOK.DialogResult=DialogResult.OK;
            // 
            // LanguagePicker
            // 
            this.AcceptButton=ButtonOK;
            this.ClientSize = new Size(140, Tree.Height+24);
            this.ControlBox = false;
            this.Controls.Add(Tree);
            this.Controls.Add(ButtonOK);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text="GoodMerge";
            this.TopMost = true;
        }
	}
}
