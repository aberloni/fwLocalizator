using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.localizator
{
    using fwp.localizator;
    using System;
    using System.IO;
    using System.Text;

    [System.Serializable]
    public class CsvParser
    {
        public string tabUid;
        public string fileContentRaw;

        [System.Serializable]
        public struct CsvLine
        {
            public string raw;
            public List<string> cell;

            public string stringify()
            {
                string output = string.Empty;
                for (int i = 0; i < cell.Count; i++)
                {
                    if (i > 0) output += ParserStatics.SPREAD_CELL_SEPARATOR;
                    output += cell[i];
                }
                return output;
            }
        }

        /// <summary>
        /// each line is a variant of localization/language
        /// </summary>
        public List<CsvLine> lines = new List<CsvLine>();

        public CsvParser()
        { }

        public void save() => CsvSerializer.save(this,
            LocalizationPaths.sysImports + tabUid + CsvSerializer.parserExtDot);

        public static CsvParser loadFromTabUid(string tabUid) => CsvSerializer.load(
                LocalizationPaths.sysImports + tabUid + CsvSerializer.parserExtDot);

        static public CsvParser load(string parserPath) => CsvSerializer.load(parserPath);

        public CsvParser generateFromPath(string path, int skipLineCount)
        {
            string tabContent = File.ReadAllText(path);

            var csv = generateFromRaw(tabContent, skipLineCount);

            // tab txt path => txt name
            path = path.Substring(path.LastIndexOf("/") + 1);
            path = path.Substring(0, path.LastIndexOf("."));

            //Debug.Log(path);

            csv.tabUid = path;

            return csv;
        }

        CsvParser generateFromRaw(string raw, int skipLineCount)
        {
            // some cleaning
            raw = presetupRaw(raw);

            fileContentRaw = raw;
            //Debug.Log(raw);

            // this will split lines endings & remove empty lines
            string[] rawLines = raw.Split(new char[] { ParserStatics.SPREAD_LINE_BREAK }, System.StringSplitOptions.RemoveEmptyEntries);

            int originalCount = rawLines.Length;

            for (int i = skipLineCount; i < rawLines.Length; i++)
            {
                CsvLine line = new CsvLine();
                line.cell = new List<string>();
                line.raw = rawLines[i];

                string[] split = cellsSeparator(rawLines[i]);

                // check if any content in line
                int cntCellWithContent = 0;
                for (int j = 0; j < split.Length; j++)
                {
                    if (split[j].Length > 0)
                    {
                        //Debug.Log(split[j] + " (" + split[j].Length + ")");
                        cntCellWithContent++;
                    }
                    line.cell.Add(split[j]);
                }

                //Debug.Log(line.raw + " ? " + cntCellWithContent);

                //skip line of only empty cells
                if (cntCellWithContent > 0) lines.Add(line);
            }

            //if (lines.Count != originalCount) Debug.Log("csv solved lines x" + lines.Count + " out of x" + originalCount);

            return this;
        }

        public void fillAutoUid(int uidColumn)
        {

            string _uid = string.Empty;
            int cnt = 0;

            //first line is languages
            for (int j = 1; j < lines.Count; j++)
            {
                var values = lines[j].cell;

                if (values.Count < 2)
                {
                    _uid = string.Empty;
                    cnt = 0;
                    continue; // skip empty lines
                }

                var key = values[uidColumn]; // uid key

                if (key.Length < 3)
                {
                    Debug.Assert(_uid.Length > 0, "need uid here");
                    key = _uid; // empty key = last known
                }
                else
                {
                    if (key != _uid)
                    {
                        _uid = key;
                        cnt = 0;
                    }
                }

                cnt++;
                if (cnt < 10) key = key + "-0" + cnt;
                else key = key + "-" + cnt;

                values[uidColumn] = key;

                Debug.Log("#" + j + " => " + key);
            }

        }

        /// <summary>
        /// crawl a string line
        /// catch when value is between quotes ""
        /// separate when catch ","
        /// </summary>
        string[] cellsSeparator(string lineRaw)
        {

            List<string> cells = new List<string>();

            StringBuilder sb = new StringBuilder();

            bool inValue = false;

            //browse the string
            for (int i = 0; i < lineRaw.Length; i++)
            {
                char cur = lineRaw[i];

                if (cur == ParserStatics.SPREAD_CELL_ESCAPE_VALUE) // "
                {
                    inValue = !inValue;
                }

                if (cur == ParserStatics.SPREAD_CELL_SEPARATOR && !inValue) // ,
                {
                    cells.Add(sb.ToString());
                    sb.Clear();

                    //Debug.Log("added value : " + sb);
                }
                else
                {
                    //don't add separator "," symbol
                    sb.Append(cur);
                }
            }

            cells.Add(sb.ToString());

            return cells.ToArray();
        }

        public string getCleanedCsvText()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < lines.Count; i++)
            {
                sb.AppendLine(lines[i].stringify());
            }

            return sb.ToString();
        }

        string presetupRaw(string raw)
        {

            //search & replace all LINE breaks
            // a line break char OUTSIDE of ""

            StringBuilder sb = new StringBuilder(raw);

            bool inCellValue = false;
            for (int i = 0; i < sb.Length; i++)
            {
                char cur = sb[i];

                if (cur == '"') inCellValue = !inCellValue;

                if (isCharLineBreak(cur.ToString()))
                {
                    cur = inCellValue ? ParserStatics.CELL_LINE_BREAK : ParserStatics.SPREAD_LINE_BREAK;
                }

                sb[i] = cur;
            }

            //quand on tombe sur \r puis \n ça fait @@
            sb.Replace("@@", "@");
            sb.Replace("||", "|");

            raw = sb.ToString();

            return raw;
        }

        static bool isCharLineBreak(string cur)
        {
            if (cur == System.Environment.NewLine) return true;
            if (cur == "\r\n") return true;
            if (cur == "\r") return true;
            if (cur == "\n") return true;
            return false;
        }

        static CsvParser[] _parsers;

        static public void refreshCache()
        {
            //Debug.Log("refresh CSV cache");

            string[] csvPaths = Directory.GetFiles(
                LocalizationPaths.sysImports, "*." + CsvSerializer.parserExt);

            if (csvPaths.Length <= 0)
            {
                Debug.LogWarning("no csvs found @ " + LocalizationPaths.sysImports);
                return;
            }

            List<CsvParser> output = new List<CsvParser>();
            for (int i = 0; i < csvPaths.Length; i++)
            {
                var csv = CsvParser.load(csvPaths[i]);
                output.Add(csv);
            }

            _parsers = output.ToArray();
        }

        static public CsvParser[] loadParsers()
        {
            if (_parsers == null) refreshCache();
            return _parsers;
        }

    }

}
