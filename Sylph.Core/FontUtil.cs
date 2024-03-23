using FontStashSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SylphGame {
    public static class FontUtil {

        public static IEnumerable<string> WrapText(this DynamicSpriteFont font, SGame sgame, string s, int maxWidth) {
            maxWidth = (int)(maxWidth * sgame.Config.Scale);
            foreach(string line in s.Replace("\r\n", "\n").Split('\n')) {
                string remaining = line;
                while (true) {
                    if (font.MeasureString(remaining).X <= maxWidth) {
                        yield return remaining.Trim();
                        break;
                    }
                    //TODO could binary search 
                    int lastWhitespace = -1;
                    foreach(int i in Enumerable.Range(2, remaining.Length - 1)) {
                        if (char.IsWhiteSpace(remaining[i]))
                            lastWhitespace = i;
                        if (font.MeasureString(remaining.Substring(0, i)).X > maxWidth) {
                            int split = lastWhitespace > 0 ? lastWhitespace : i - 1;
                            yield return remaining.Substring(0, split).Trim();
                            remaining = remaining.Substring(split);
                            break;
                        }
                    }
                }
            }            
        }
    }
}
