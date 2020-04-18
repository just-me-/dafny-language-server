using Microsoft.Boogie;
using Microsoft.Boogie.ModelViewer;
using Microsoft.Boogie.ModelViewer.Dafny;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.Handler;
using Microsoft.Dafny;

namespace DafnyLanguageServer.DafnyAccess
{
    /// <summary>
    /// This class provides counter examples.
    /// It analyzes the file 'model.bvd' and extracts its information.
    /// To generate a model.bvd, one needs to compile with the argument /mv:[PathtoModel.bvd]
    /// The .bvd can also be injected for unit testing.
    /// </summary>
    public class CounterExampleProvider
    {
        private static readonly string _assemblyPath = Path.GetDirectoryName(typeof(CounterExampleProvider).Assembly.Location);
        public static string ModelBvd = Path.GetFullPath(Path.Combine(_assemblyPath, "../model.bvd"));

        private string Source { get; }
        public CounterExampleProvider(PhysicalFile file)
        {
            Source = file.Sourcecode;
        }

        public CounterExampleProvider(string source, string modelFile)
        {
            Source = source;
            ModelBvd = modelFile;
        }

        public CounterExampleResults LoadCounterModel()
        {
            if (!File.Exists(ModelBvd))
            {
                return new CounterExampleResults();
            }
            string RawBVDContent = ReadModelFile(ModelBvd);
            List<Model> models = ParseModels(RawBVDContent);
            List<ILanguageSpecificModel> specificModels = BuildModels(models);

            var result = new CounterExampleResults();
            foreach (var specificModel in specificModels)
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

        private string ReadModelFile(string path)
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
            const string regex = @".*dfy\((\d+),(\d+)\)"; // anything, then dfy(00,00)
            var r = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(stateCapturedStateName);
            if (m.Success)
            {
                var lineStr = m.Groups[1].ToString();
                int line = int.Parse(lineStr);
                ce.Line = line;
                ce.Col = FileHelper.GetLineLength(Source, line);
            }
            else
            {
                ce.Line = 0;
                ce.Col = 0;
            }
        }

        private static string RemoveBrackets(string s)
        {
            const string regex = @"\((.*)\)"; // anything surrounded by brackets at the beginning and the end
            var r = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(s);

            while (m.Success)
            {
                s = m.Groups[1].ToString();
                m = r.Match(s);
            }
            return s;
        }

        private static string RemoveSpaceAfterMinus(string s)
        {
            return s.Replace("- ", "-");
        }

        private static bool IsUnknown(string s) => s.StartsWith("**") || s.StartsWith("'");
        private static bool IsReference(string s) => s.StartsWith("T@U!val!");
        public static void RemoveExistingFileModel()
        {
            if (File.Exists(ModelBvd))
            {
                File.Delete(ModelBvd);
            }
        }
    }

    
}
