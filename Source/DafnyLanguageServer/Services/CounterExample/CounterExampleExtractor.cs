using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.Handler;
using Microsoft.Boogie.ModelViewer;
using Microsoft.Boogie.ModelViewer.Dafny;

namespace DafnyLanguageServer.Services.CounterExample
{
    /// <summary>
    /// This class extracts counter examples from a given list of
    /// ILanguageSpecificModels. It will omit variables of referencial style
    /// or unknown variables.
    /// </summary>
    public class CounterExampleExtractor
    {
        private string Source { get; }
        private List<ILanguageSpecificModel> Models { get; }

        public CounterExampleExtractor(PhysicalFile file, List<ILanguageSpecificModel> models)
        {
            Source = file.Sourcecode;
            Models = models;
        }

        public CounterExampleResults Extract()
        {
            var result = new CounterExampleResults();
            foreach (var specificModel in Models)
            {
                StateNode relevantState = FindInitialState(specificModel);
                Handler.CounterExample ce = ExtractCounterExampleFromState(relevantState);
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

        private Handler.CounterExample ExtractCounterExampleFromState(StateNode state)
        {
            Handler.CounterExample ce = new Handler.CounterExample();
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

        private void AddPosition(Handler.CounterExample ce, string stateCapturedStateName)
        {

            const string regex = @".*dfy\((\d+),(\d+)\)";   //anything, then dfy(00,00)
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
