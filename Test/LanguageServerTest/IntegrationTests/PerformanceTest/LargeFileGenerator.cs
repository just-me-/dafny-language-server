using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework.Internal;
using Files = TestCommons.Paths;


namespace PerformanceTest
{
    public class LargeFileGenerator
    {
        public Random r;
        public StringBuilder b = new StringBuilder();
        public int depth;
        public int varCount;
        public int loc;
        public int maxLength;

        public List<string> globalScopeVars = new List<string>();

        public LargeFileGenerator(int maxLength, int seed)
        {
            this.maxLength = maxLength;
            r = new Randomizer(seed);
            b.AppendLine("class T {");
            b.AppendLine(" var v1 : int;");
            globalScopeVars.Add("v1");
            b.AppendLine(" constructor() { v1:=0; }");
            b.AppendLine(" method m() {");
            depth = 2;
            varCount = 1;
            loc = 4;
        }

        public void Generate()
        {
            while (loc < maxLength)
            {
                var nextCodeItem = CreateNextCodeItem();
                string code = GetIndentation();
                code += ToCode(nextCodeItem);
                b.AppendLine(code);
            }

            while (depth > 0)
            {
                string code = GetIndentation();
                code += "}";
                b.AppendLine(code);
                depth--;
            }
        }

        public void Store(string file)
        {
            File.WriteAllText(file, b.ToString());
        }

        public string GetIndentation()
        {
            string code = "";
            for (int i = 0; i < depth; i++)
                code += " ";
            return code;
        }


        public string ToCode(CodeItem codeItem)
        {
            switch (codeItem)
            {
                case CodeItem.EndBlock:
                    depth--;
                    loc++;
                    return "}";

                case CodeItem.Block:
                    depth++;
                    loc++;
                    return "while (true) {";

                case CodeItem.Var:
                    loc++;
                    string varname = $"v{++varCount}";
                    if (depth == 2)
                    {
                        globalScopeVars.Add(varname);
                    }
                    return $"var {varname} := {GetRandomExistingVar()};";

                case CodeItem.Print:
                    loc++;
                    return $"print {GetRandomExistingVar()};";
                default:
                    throw new InvalidOperationException();
            }
        }

        public string GetRandomExistingVar()
        {
            int rnd = r.Next(globalScopeVars.Count);
            return globalScopeVars[rnd];
        }

        public CodeItem CreateNextCodeItem()
        {
            double d = r.NextDouble();
            if (d < 0.05)
            {
                return CodeItem.Block;
            }

            if (d < 0.1 && depth > 2)
            {
                return CodeItem.EndBlock;
            }

            if (d < 0.3)
            {
                return CodeItem.Print;
            }

            return CodeItem.Var;
        }
    }


    public enum CodeItem { Block, Var, Print, EndBlock }
}
