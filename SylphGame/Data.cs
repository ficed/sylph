using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public class Data {

        private List<string> _roots;

        public Data(IEnumerable<string> roots) {
            _roots = roots.ToList();
        }

        public Stream Open(string category, string name) {
            var s = TryOpen(category, name);
            if (s == null)
                throw new FileNotFoundException();
            return s;
        }

        public Stream TryOpen(string category, string name) {
            foreach (string root in _roots) {
                string fn = Path.Combine(root, category, name);
                if (File.Exists(fn))
                    return File.OpenRead(fn);
            }
            return null;
        }
    }
}
