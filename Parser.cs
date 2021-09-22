using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SVG_Parsing {
    class Parser {
    public static int linkCount = 0;
    public static string[] parts;
    public static int[] linkType;
 
    public static int pageCount;
    public static string[] partCounts;
    public static string path;
    public static string fileName;
    public static string rootFile;

        static void Main(string[] args) {
            Console.WriteLine("VERSION 1.11");
            bool running = true;
            while (running){
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nFile path");
                path = Console.ReadLine();
                if(path == ""){
                    path = "F:\\DOC\\Parts Breakdowns\\~~svg parser~~\\default\\";
                } else {
                    path = path +"\\";
                }
                Console.WriteLine("File location: " + path);
                fileName = GetFileName() + ".svg";

                while (!File.Exists(path + fileName)) {
                    Console.WriteLine("File Doesnt Exist...");
                    fileName = GetFileName() + ".svg";
                }

                //ReadFile and add lines to a list
                string[] lines = File.ReadAllLines(path + fileName);
                List<string> formatted = new List<string>();

                if(lines.Length < 159){
                    formatted = FormatFile(lines);
                } else {
                    int i = 0;
                    foreach(string n in lines){
                        formatted.Add(lines[i]);
                        i++;
                    }
                }

                //Get link count
                linkCount = GetLinkCount(formatted);

                //Get part numbers and counts
                parts = GetPartNumbers(formatted, linkCount);
                partCounts = GetPartCounts(formatted, linkCount);
                for(int i = 0; i < linkCount; i++) {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("BOM ID: " + (i+1).ToString() + " | ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(parts[i] + ": ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(partCounts[i]);
                }

                //get if the item is a product or a linked page
                linkType = SetLinkTypes(parts);

                //add the links to the correct hotspot
                formatted = AddLinks(formatted);

                //print book pages
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Book Pages: ");
                int lk = 0;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(rootFile);
                foreach(string s in parts){
                    if(linkType[lk] == 2){
                        Console.WriteLine(s);
                    }
                    lk++;
                }

                //SaveBookPages();
            
                //Save the file
                PrintFile(formatted.ToArray(), path+fileName);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Would you like parse another file?");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[y] - yes : [n] - no \n");

                ConsoleKeyInfo input = Console.ReadKey();
                if(input.KeyChar != 'y' && input.KeyChar != 'Y'){
                    running = false;
                } else {
                    running = true;
                    Console.WriteLine("");
                }            
            }
        }

        static void SaveBookPages(){
            int i = 0;
            string csv = "";
            string p = "";
            csv = String.Concat(csv, rootFile+";");
            csv = String.Concat(csv, rootFile+";\n");
            foreach(string s in parts){
                if(linkType[i] == 2){
                    csv = String.Concat(csv, rootFile+";");
                    csv = String.Concat(csv, parts[i]+";\n");
                }
                i++;
            }

            using (StreamWriter bookPages = new StreamWriter(p + "BookPages.csv"))
                {
                    bookPages.WriteLine(csv);
                }
        }
        static string GetFileName(){
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Enter assembly file name");
            string f = Console.ReadLine();
            f = f.Replace("\n", String.Empty);
            rootFile = f;
            Console.WriteLine("creating svg file named " + f + ".svg");
            return f;
        }

        static List<string> FormatFile(string[] lines){
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Formatting file...");
            List<string> formatted = new List<string>();
            int i = 0;
            foreach(string l in lines){
                if(i == 157){
                    string[] newLine = l.Split('>');
                    int j = 0;
                    foreach(string n in newLine){
                        formatted.Add(newLine[j]+">");
                        j++;
                    }
                } else {
                    formatted.Add(l);
                }
                i++;
            }

            formatted[formatted.Count-1] = String.Empty;

            formatted = FixFormat(formatted);

            return formatted;
        }

        static string[] GetBookPages(string path){
            string[] bookPages = new string[linkCount];

            return bookPages;
        }

        static List<string> AddLinks(List<string> lines){
            int i = 0;
            int ii = 0;
            int pn = 1;
            string linkString;
            string l = "";
            int x = 0;
            int xx = 9;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Adding Correct Tooltips and Links...");

            for(int j = 0; j < lines.Count; j++){
                l = lines[j];
                if(l.Contains("hotspot."+i.ToString())){
                    
                    //the ones position of the final loop
                    int y = (linkCount % 10);
                    //the 10's position
                    int yy = linkCount / 10;

                    if(i == ii*11){
                        ii++;
                        pn = ii;
                        x = 0;
                        if(ii >= yy){
                            xx = y;
                        } else {
                            xx = 9;
                        }
                    } else if(x <= xx && yy >= ii){
                        pn = ProductPosition(ii, x);
                        x++;
                    } else {
                        ii++;
                        pn = ii;
                    }

                    //TODO add a check for if the link already exists
                    if(linkType[pn-1] == 1) {  
                        linkString ="<a xlink:href=\"ProductItems:" + (pn).ToString() + "|" + parts[pn-1] + ";" + partCounts[pn-1] + "\">";
                    } else {
                        linkString = "<a xlink:href=\"LinkedPage:" + pn.ToString() + "|" + parts[pn-1] + "\">";
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Added link: " + linkString + " to line " + (j+1).ToString());
                    if(lines[j+1].Contains("xlink")){
                        lines[j+1] = linkString;
                    } else {
                        lines.Insert(j+1, linkString);
                        lines.Insert(j+3, "</a>");
                    }

                    //change tooltips to the correct part number    
                    lines[j] = FixTooltip(l, parts[pn-1]);
                    i++;
                }
            }
            return lines;
        }

        static int ProductPosition(int x, int xx){
            int pn = int.Parse(x.ToString() + xx.ToString());
            return pn;
        }
        static string FixTooltip(string line, string pn){
            int start = line.IndexOf(";")+1;
            int end = line.IndexOf("&", start+1);
            var s = new StringBuilder(line);
            s.Remove(start, end-start);
            s.Insert(start, pn);

            return s.ToString();
        }

        static int GetLinkCount(List<string> lines){
            int linkCount = 0;

            foreach(string l in lines){
                if(l.Contains("hotspot." + linkCount.ToString())){
                    linkCount++;
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(linkCount.ToString() + " BOMS");

            return linkCount;
        }

        static string[] GetPartCounts(List<string> lines, int linkCount){
            string[] partCounts = new string[linkCount];
            int j = 0;
            int i = 1;
            foreach(string l in lines){
                if(l.Contains(("Line" + i.ToString()))){
                    string s = lines[j+5];
                    s = s.Substring(s.IndexOf('>') + 1);
                    s = s.Replace("</text>", String.Empty);
                    partCounts[i-1] = s;
                    i++;
                }
                j++;
            }
        
            return partCounts;
        }

        static string[] GetPartNumbers(List<string> lines, int linkCount){
            string[] partNumbers = new string[linkCount];
            int j = 0;
            int i = 1;
            foreach(string l in lines){
                if(l.Contains(("Line" + i.ToString()))){
                    string s = lines[j+3];
                    s = s.Substring(s.IndexOf('>') + 1);
                    s = s.Replace("</text>", String.Empty);
                    partNumbers[i-1] = s;
                    i++;
                }
                j++;
            }
        
            return partNumbers;
        }

        static int[] SetLinkTypes(string[] parts){
            int[] linkType = new int[linkCount];
            ConsoleKeyInfo input;
            for(int i = 0; i < linkCount; i++){
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("ID : " + (i+1).ToString() + " | ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Part number: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(parts[i] + " - ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(" product item[1] or a linked page[2]?\n");        
                Console.ForegroundColor = ConsoleColor.Yellow;
                input = Console.ReadKey();
                //TODO add a catch if the input is not a 1 or 2
                if (char.IsDigit(input.KeyChar)){
                    linkType[i] = int.Parse(input.KeyChar.ToString());
                    Console.WriteLine("");
                } else {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nNot a valid option. Try again");
                    i--;
                }
            }

            return linkType;
        }

        static List<string> FixFormat(List<string> lines){
            List<string> newList = new List<string>();
            int i = 0;
            int j = 0;
            //add the text lines together
            foreach(string l in lines){
                if(l.Contains("<text")){
                    string newLine = String.Concat(lines[i], lines[i+1]);
                    newList.Insert(j, newLine);
                    j++;
                } else if(!l.Contains("<text") && l.Contains("</text>")){
                    //skip
                } else {
                    newList.Insert(j, l);
                    j++;
                }
                i++;
            }

/*
            // remove the end tag text lines
            for(int j = 0; j < lines.Count; j++){
                if(lines[j].Contains("</text>") && !lines[j].Contains("<text>")){
                    lines.RemoveAt(j);
                }
            }
            */

            return newList;
        }
        static void PrintFile(string[] lines, string fileName){
            using (StreamWriter outputFile = new StreamWriter(fileName)) {
                foreach (string line in lines) {
                    outputFile.WriteLine(line);
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("File " + fileName + " created at: " + path);
        }
    }
}
