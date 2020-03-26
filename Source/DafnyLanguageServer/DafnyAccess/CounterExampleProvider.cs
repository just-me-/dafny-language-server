using Microsoft.Boogie;
using Microsoft.Boogie.ModelViewer;
using Microsoft.Boogie.ModelViewer.Dafny;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using DafnyLanguageServer.Handler;

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


            return null;
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
                specificModels.Add(specifiedModel); //hier wird wohl iwie konvertiert einfach nach iwas.
            }
            return specificModels;
        }



        private CounterExample ConvertModels(List<ILanguageSpecificModel> specificModels)
        {
            var counterExample = new CounterExample();  //leeres base model als result erstellen.

            foreach (var languageSpecificModel in specificModels) //iteriere durch die liste der modelle
            {
                foreach (var s in languageSpecificModel.States)  //geh durch die states, also so initial etc pp
                {
                    var state = s as StateNode;
                    if (state == null) continue;  //nehme den state

                    var counterExampleState = new CounterExampleState        //neuen state
                    {
                        Name = state.CapturedStateName //extrahiere name
                    };
                    AddLineInformation(counterExampleState, state.CapturedStateName);  //extrahiere line: das ist so wie g:\Dokumente\VisualStudio\BA\dafny-language-server\Test\LanguageServerTest\DafnyFiles\counterExample\two_methods.dfy(9,0): aber nur halt beim nicht-initial

                    foreach (var variableNode in state.Vars) //extrahiere variablen.
                    {
                        counterExampleState.Variables.Add(new CounterExampleVariable
                        {
                            Name = variableNode.ShortName,
                            Value = variableNode.Value,
                            CanonicalName = languageSpecificModel.CanonicalName(variableNode.Element)
                        });
                        GetExpansions(state, variableNode, counterExampleState, languageSpecificModel); //iwie connected variablen auch noch holen.
                    }
                    var index = counterExample.States.FindIndex(c => c.Column == counterExampleState.Column && c.Line == counterExampleState.Line);
                    if (index != -1)
                    {
                        counterExample.States[index] = counterExampleState;
                    }
                    else
                    {
                        counterExample.States.Add(counterExampleState);
                    }
                }
            }
            return counterExample;

        }

        private static void GetExpansions(StateNode state, ElementNode elementNode, CounterExampleState counterExampleState,
          ILanguageSpecificModel languageSpecificModel)
        {
            try
            {
                var dafnyModel = GetFieldValue<DafnyModel>(state, "dm");
                var elt = GetFieldValue<Model.Element>(elementNode, "elt");
                var extras = dafnyModel.GetExpansion(state, elt);
                foreach (var el in extras)
                {
                    counterExampleState.Variables.Add(new CounterExampleVariable
                    {
                        Name = elementNode.Name + "." + el.Name,
                        Value = el.Value,
                        CanonicalName = languageSpecificModel.CanonicalName(el.Element)
                    });
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        private static T GetFieldValue<T>(object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var field = instance.GetType().GetField(fieldName, bindFlags);
            return field == null ? default(T) : (T)field.GetValue(instance);
        }

        private void AddLineInformation(CounterExampleState state, string stateCapturedStateName)
        {
            if ("<initial>".Equals(stateCapturedStateName))
            {
                state.Line = 0;
                state.Column = 0;
                return;
            }

            var regex = ".*?(dfy)(\\()(\\d+)(,)(\\d+)(\\))";
            var r = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(stateCapturedStateName);
            if (m.Success)
            {
                var lineStr = m.Groups[3].ToString();
                state.Line = int.Parse(lineStr);
                var columnStr = m.Groups[5].ToString();
                state.Column = int.Parse(columnStr);
            }
        }

    }
}
