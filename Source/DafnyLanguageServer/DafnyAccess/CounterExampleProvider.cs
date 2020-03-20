using Microsoft.Boogie;
using Microsoft.Boogie.ModelViewer;
using Microsoft.Boogie.ModelViewer.Dafny;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace DafnyLanguageServer.DafnyAccess
{
    public class CounterExampleProvider
    {
        private List<ILanguageSpecificModel> _languageSpecificModels;
        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(CounterExampleProvider).Assembly.Location);
        public static readonly string ModelBvd = Path.GetFullPath(Path.Combine(assemblyPath, "../model.bvd"));

        public CounterExample LoadCounterModel()
        {
                var models = LoadModelFromFile();
                return ConvertModels(models);
        }

        private List<ILanguageSpecificModel> LoadModelFromFile()
        {
            if (!File.Exists((ModelBvd)))
            {
                throw new IOException("Model.bvd File is not existing");
            }
            using (var wr = new StreamReader(ModelBvd))
            {
                var output = wr.ReadToEnd();
                var models = ExtractModels(output);   //schaut im model.bvd was zwischen ***MODEL und ***ND MODEL Steht. nutzt dann boogie um das zu parsen.
                _languageSpecificModels = BuildModels(models); //konvertiert sie noch iwie in nen anderes format.
            }
            return _languageSpecificModels;
        }

        private List<Model> ExtractModels(string output)
        {
            const string begin = "*** MODEL";
            const string end = "*** END_MODEL";
            var beginIndex = output.IndexOf(begin, StringComparison.Ordinal);
            var endIndex = output.IndexOf(end, StringComparison.Ordinal);
            if (beginIndex == -1 || endIndex == -1)
            {
                return new List<Model>();
            }

            var modelString = output.Substring(beginIndex, endIndex + end.Length - beginIndex);
            var models = Model.ParseModels(new StringReader(modelString));

            return models;
        }

        private List<ILanguageSpecificModel> BuildModels(List<Model> modellist)
        {
            var list = new List<ILanguageSpecificModel>();
            foreach (var model in modellist)
            {
                var specifiedModel = Provider.Instance.GetLanguageSpecificModel(model, new ViewOptions() { DebugMode = true, ViewLevel = 3 });
                list.Add(specifiedModel);
            }
            return list;
        }



        private CounterExample ConvertModels(List<ILanguageSpecificModel> specificModels)
        {
            var counterExample = new CounterExample();  //leeres base model als result erstellen.

            foreach (var languageSpecificModel in specificModels) //iteriere durch die liste der modelle
            {
                foreach (var s in languageSpecificModel.States)  //geh durch die states der modells
                {
                    var state = s as StateNode;
                    if (state == null) continue;  //nehme den state

                    var counterExampleState = new CounterExampleState
                    {
                        Name = state.CapturedStateName //extrahiere name
                    };
                    AddLineInformation(counterExampleState, state.CapturedStateName);  //extrahiere line

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

        [Serializable]
        [DataContract]
        public class CounterExample
        {
            [DataMember]
            public List<CounterExampleState> States { get; set; }

            public CounterExample()
            {
                States = new List<CounterExampleState>();
            }
        }

        [Serializable]
        [DataContract]
        public class CounterExampleState
        {
            [DataMember]
            public List<CounterExampleVariable> Variables { get; set; }
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public int Line { get; set; }
            [DataMember]
            public int Column { get; set; }
            public CounterExampleState()
            {
                Variables = new List<CounterExampleVariable>();
            }
        }

        [Serializable]
        [DataContract]
        public class CounterExampleVariable
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public string RealName { get; set; }
            [DataMember]
            public string Value { get; set; }
            [DataMember]
            public string CanonicalName { get; set; }
        }
    }
}
