using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // Run as Test <filename> <toXML|toJSON>
            if (!CheckOptions(args))
                return;
  
            string fname = args[0];
            List<string[]> csvData = ReadCSVFile(fname);

            string opt = args[1];
            switch (opt)
            {
                case "toJSON":
                    JObject jo = ToJSON(csvData);
                    Console.WriteLine(jo);
                    break;

                case "toXML":
                    JObject XMLjo = ToJSON(csvData);
                    string xmlString = @"{
  '?xml': {
    '@version': '1.0',
    '@standalone': 'no'
  },
  'root': " + XMLjo.ToString() + @"
}";
                    XmlDocument doc = (XmlDocument)Newtonsoft.Json.JsonConvert.DeserializeXmlNode(xmlString);
                    doc.Save(Console.Out);
                    break;
            }
        }

        private static JObject ToJSON(List<string[]> csvData)
        {
            JObject jo = new JObject();

            string[] header = csvData[0];
            int cols = header.Length;
            int rows = csvData.Count - 1;

            for (int i = 0; i < rows; i++)
            {
                string[] rowData = csvData[i + 1];
                JObject joLine = WriteLine(rowData, cols, header);

                jo.Add("Line" + (i + 1), joLine);
            }

            return jo;
        }

        private static JObject WriteLine(string[] rowData, int cols, string[] header)
        {
            JObject joLine = new JObject();
            List<string> names = new List<string>();

            for (int j = 0; j < cols; j++)
            {
                if (!header[j].Contains('_'))
                {
                    // For simple objects.
                    joLine.Add(header[j], rowData[j]);
                }
                else
                {
                    // For objects like 'address_'.
                    int pos = header[j].IndexOf('_');
                    string name = header[j].Substring(0, pos);
                    if (!names.Contains(name))
                    {
                        names.Add(name);
                        joLine.Add(name, SubLines(rowData, name, cols, header));
                    }
                }
            }

            return joLine;
        }

        private static JObject SubLines(string[] rowData, string name, int cols, string[] header)
        {
            JObject joSub = new JObject();

            for (int j2 = 0; j2 < cols; j2++)
            {
                if (header[j2].StartsWith(name))
                {
                    string name2 = header[j2].Substring(name.Length + 1);
                    joSub.Add(name2, rowData[j2]);
                }
            }

            return joSub;
        }

        private static List<string[]> ReadCSVFile(string fname)
        {
            List<string[]> lines = new List<string[]>();

            using (TextFieldParser csvParser = new TextFieldParser(fname))
            {
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Get the header.
                lines.Add(csvParser.ReadFields());    

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    lines.Add(fields);
                }
            }

            return lines;
        }

        private static bool CheckOptions(string[] args)
        {
            if (args.Length != 2 || !CheckFile(args[0]) || !CheckToOpt(args[1]))
            {
                Usage();
                return false;
            }
            return true;
        }

        private static bool CheckFile(string fname)
        {
            if (!File.Exists(fname))
            {
                Console.Error.WriteLine("File does exist.");
                return false;
            }
            return true;
        }

        private static bool CheckToOpt(string toOpt)
        {
            if (toOpt != "toXML" && toOpt != "toJSON")
            {
                Console.Error.WriteLine("Wrong to option entered.");
                return false;
            }
            return true;
        }

        private static void Usage()
        {
            Console.Error.WriteLine("Run as Test <filename> <toXML|toJSON>\n");
        }
    }
}
