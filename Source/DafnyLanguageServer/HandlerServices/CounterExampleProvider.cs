using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.Handler;
using Microsoft.Boogie;
using Microsoft.Boogie.ModelViewer;
using Microsoft.Boogie.ModelViewer.Dafny;

namespace DafnyLanguageServer.HandlerServices
{

    public static class CounterExampleDefaultModelFile
    {
        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(CounterExampleDefaultModelFile).Assembly.Location);
        public static string FilePath => Path.GetFullPath(Path.Combine(assemblyPath, "../model.bvd"));

        public static void ClearDefaultModelFile()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }

    /// <summary>
    /// This class provides counter examples.
    /// It analyzes the file 'model.bvd' and extracts its information.
    /// To generate a model.bvd, one needs to compile with the argument /mv:[PathtoModel.bvd]
    /// The .bvd can also be injected for unit testing.
    /// </summary>
    public class CounterExampleProvider
    {
        public string ModelBvd { get; }
        private PhysicalFile PhysicalFile { get; }


        public CounterExampleProvider(PhysicalFile file) : this(file, CounterExampleDefaultModelFile.FilePath)
        {
        }

        public CounterExampleProvider(PhysicalFile file, string pathToModelBvd)
        {
            PhysicalFile = file;
            ModelBvd = pathToModelBvd;
        }

        public CounterExampleResults LoadCounterModel()
        {
            if (!File.Exists(ModelBvd))
            {
                return new CounterExampleResults();
            }
            List<ILanguageSpecificModel> specificModels = new ModelReader(ModelBvd).ReadModelFile();
            return new CounterExampleExtractor(PhysicalFile, specificModels).ExtractCounterExamples();
        }

        private class ModelReader
        {
            private string ModelBvd { get; }
            public ModelReader(string pathToBvdFile) => ModelBvd = pathToBvdFile;

            public List<ILanguageSpecificModel> ReadModelFile()
            {
                string RawBVDContent = ReadModelFileIntoString(ModelBvd);
                List<Model> models = ParseModels(RawBVDContent);
                return BuildModels(models);
            }

            private string ReadModelFileIntoString(string path)
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.Default))
                {
                    return sr.ReadToEnd();
                }
            }

            private List<Model> ParseModels(string modelString)
            {
                return Model.ParseModels(new StringReader(modelString));
            }

            private List<ILanguageSpecificModel> BuildModels(List<Model> modellist)
            {
                var specificModels = new List<ILanguageSpecificModel>();
                foreach (var model in modellist)
                {
                    var specifiedModel = Provider.Instance.GetLanguageSpecificModel(model, new ViewOptions() { DebugMode = true, ViewLevel = 3 });
                    specificModels.Add(specifiedModel);
                }
                return specificModels;
            }
        }

        private class CounterExampleExtractor
        {
            private PhysicalFile PhysicalFile { get; }
            private List<ILanguageSpecificModel> SpecificModels { get; }
            public CounterExampleExtractor(PhysicalFile file, List<ILanguageSpecificModel> specificModels)
            {
                PhysicalFile = file;
                SpecificModels = specificModels;
            }

            public CounterExampleResults ExtractCounterExamples()
            {
                var result = new CounterExampleResults();
                foreach (var specificModel in SpecificModels)
                {
                    StateNode relevantState = FindInitialState(specificModel);
                    CounterExample ce = ExtractCounterExampleFromState(relevantState);
                    if (ce.Variables.Count > 0)
                    {
                        result.CounterExamples.Add(ce);
                    }
                }
                return result;
            }


            private StateNode FindInitialState(ILanguageSpecificModel specificModel)
            {
                foreach (IState s in specificModel.States)
                {
                    if (!(s is StateNode state))
                    {
                        continue;
                    }
                    if (state.Name.Contains(":initial state"))
                    {
                        return state;
                    }
                }

                throw new InvalidOperationException("specific Model does not contain a :initial state");
            }

            private CounterExample ExtractCounterExampleFromState(StateNode state)
            {
                CounterExample ce = new CounterExample();
                AddPosition(ce, state.CapturedStateName);

                foreach (var variableNode in state.Vars)
                {
                    string name = variableNode.ShortName;
                    string value = RemoveBrackets(variableNode.Value);
                    value = RemoveSpaceAfterMinus(value);
                    if (IsReference(value))
                    {
                        value = "[Object Reference]";
                    }

                    if (!IsUnknown(value))
                    {
                        ce.Variables.Add(name, value);
                    }
                }
                return ce;
            }

            private void AddPosition(CounterExample ce, string stateCapturedStateName)
            {

                const string regex = @".*dfy\((\d+),(\d+)\)";   //anything, then dfy(00,00)
                var r = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var m = r.Match(stateCapturedStateName);
                if (m.Success)
                {
                    var lineStr = m.Groups[1].ToString();
                    int line = int.Parse(lineStr);
                    ce.Line = line;
                    ce.Col = PhysicalFile.GetLengthOfLine(line);
                }
                else
                {
                    ce.Line = 0;
                    ce.Col = 0;
                }
            }

            private static string RemoveBrackets(string s)
            {
                const string regex = @"\((.*)\)";       //anything surrounded by brackets at the beginning and the end
                var r = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var m = r.Match(s);

                while (m.Success)
                {
                    s = m.Groups[1].ToString();
                    m = r.Match(s);
                }
                return s;
            }

            private static string RemoveSpaceAfterMinus(string s) => s.Replace("- ", "-");
            private static bool IsUnknown(string s) => s.StartsWith("**") || s.StartsWith("'");
            private static bool IsReference(string s) => s.StartsWith("T@U!val!");
        }
    }



    


}
