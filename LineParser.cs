using System;
using System.IO;

namespace GoodMerge {
    public class LineParser {
        #region <Variables>
        private StreamReader file;
        private bool open;
        private string curLine;
        #endregion
        
        public LineParser(string name) {
            try {
                file = new StreamReader(name);
                open = true;
            }
            catch { open = false; }
        }

        ~LineParser() {
            if (open) {
                try { file.Close(); }
                catch { }
            }
        }

        public string GetNextLine() {
            if (!open) return null;
            do {
                try { curLine = file.ReadLine(); }
                catch { return null; }
            } while (curLine!=null && (curLine.StartsWith(";") || curLine.Equals("")));
            return curLine;
        }
    }
}