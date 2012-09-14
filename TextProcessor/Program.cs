using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = File.ReadAllText("test.txt");
            var sb = new StringBuilder(str);

            // Find all occurrences of eqno and extract number
            var matches = Regex.Matches(str, @"eqno *\{(.+)\}");
            // Process matches (to avoid changing matching positions during replace we do processing from end to begin) 
            foreach (Match match in matches.OfType<Match>().OrderByDescending(m=>m.Index))
            {
                Console.WriteLine("{0}({1})", match.Groups[1].Value.Trim(), match.Index);

                int endDollarsPos = sb.ToString().IndexOf("$$", match.Index);
                sb = sb.Replace("$$", @"\end{equation}", endDollarsPos, 2);

                int beginDollarsPos = sb.ToString().LastIndexOf("$$", 0, match.Index);
                sb = sb.Replace("$$", @"\begin{equation}", beginDollarsPos, 2);
                
                //Console.WriteLine(str.IndexOf("$$", match.Index));
                
            }
            Console.WriteLine(sb.ToString());
        }
    }
}
