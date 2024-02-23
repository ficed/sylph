using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public class Data {

        private string _root;

        public Data(string root) {
            _root = root;
        }

        public Stream Open(string category, string name) {
            var s = TryOpen(category, name);
            if (s == null)
                throw new FileNotFoundException();
            return s;
        }

        public Stream TryOpen(string category, string name) {
            string fn = Path.Combine(_root, category, name);
            if (File.Exists(fn))
                return File.OpenRead(fn);
            else
                return null;
        }
    }
}
