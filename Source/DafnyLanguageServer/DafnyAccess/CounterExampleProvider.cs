﻿using Microsoft.Boogie;
using Microsoft.Boogie.ModelViewer;
using Microsoft.Boogie.ModelViewer.Dafny;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using DafnyLanguageServer.Handler;
using Microsoft.Dafny;

namespace DafnyLanguageServer.DafnyAccess
{
    public class CounterExampleProvider
    {
        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(CounterExampleProvider).Assembly.Location);
        public static readonly string ModelBvd = Path.GetFullPath(Path.Combine(assemblyPath, "../model.bvd"));

        public CounterExampleResults LoadCounterModel()
        {
            string RawBVDContent = ReadModelFile(ModelBvd);
            List<Model> models = ParseModels(RawBVDContent);
            List<ILanguageSpecificModel> specificModels = BuildModels(models);

            var result = new CounterExampleResults();
            foreach (var specificModel in specificModels)
            {
                StateNode relevantState = FindInitialState(specificModel);
                CounterExample ce = ExtractCounterExampleFromState(relevantState);
                result.CounterExamples.Add(ce);
            }
            return result;
        }



        private string ReadModelFile(string path)
        {
            if (!File.Exists((path)))
            {
                throw new FileNotFoundException("Could not find counter model file, which should have been generated: " + ModelBvd);
            }

            using var wr = new StreamReader(path);
            return wr.ReadToEnd();
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



        private StateNode FindInitialState(ILanguageSpecificModel specificModel)  //todo verhebt das?
        {
            foreach (IState s in specificModel.States)
            {
                if (!(s is StateNode state))
                {
                    continue;
                }
                if (state.Name.Contains(":initial state"))  //todo korrekt?
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

            foreach (var variableNode in state.Vars) //extrahiere variablen.
            {
                string name = variableNode.ShortName;
                string value = ParseValue(variableNode.Value);
                ce.Variables.Add(name, value);
            }

            return ce;
        }


        private void AddPosition(CounterExample ce, string stateCapturedStateName)
        {

            var regex = ".*?(dfy)(\\()(\\d+)(,)(\\d+)(\\))";   //todo net so behindert
            var r = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(stateCapturedStateName);
            if (m.Success)
            {
                var lineStr = m.Groups[3].ToString();
                ce.Line = int.Parse(lineStr);
                var columnStr = m.Groups[5].ToString();
                ce.Col = int.Parse(columnStr);  //todo alwys 0. wo anders machen.
            }
            else
            {
                ce.Line = 0;
                ce.Col = 0;
            }
        }
        private string ParseValue(string s)
        {
            s = RemoveBrackets(s);
            s = ParseNumbers(s);
            s = 
            return s;
        }

        private string RemoveBrackets(string s)
        {
            var regex = @"\((.*)\)";   
            var r = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(s);

            while (m.Success)
            {
                s = m.Groups[1].ToString();
                m = r.Match(s);
            }

            return s;
        }

        private string ParseNumbers(string s)
        {
            //int parsing
            string regex = @"(-?) ?(\d+)";
            MatchCollection matchCollection = Regex.Matches(s, regex);
            if (matchCollection.Count == 1)
            {
                var match = matchCollection[0];
                bool isNegative = match.Groups[1].Value == "-";
                int value = int.Parse(match.Groups[2].Value);
                value = isNegative ? -value : value;
                return value.ToString();
            }

            //float parsing
            regex = @"(-?) ?(\d+\.\d+)";
            matchCollection = Regex.Matches(s, regex);
            if (matchCollection.Count == 1)
            {
                var match = matchCollection[0];
                bool isNegative = match.Groups[1].Value == "-";
                float value = float.Parse(match.Groups[2].Value);
                value = isNegative ? -value : value;
                return value.ToString();
            }

            return s;
        }
    }
}
