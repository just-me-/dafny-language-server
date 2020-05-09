using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler.LspStandard;
using Newtonsoft.Json.Linq;

namespace DafnyLanguageServer.ProgramServices
{
    public class ReservedWordsProvider
    {

        private static readonly HashSet<string> defaultReservedWords = new HashSet<string>
        {
            "abstract", "array", "as", "assert", "assume", "bool", "break",
            "calc", "case", "char", "class", "codatatype", "colemma",
            "constructor", "copredicate", "datatype", "decreases",
            "default", "else", "ensures", "exists", "extends", "false",
            "forall", "free", "fresh", "function", "ghost", "if", "imap", "import",
            "in", "include", "inductive", "int", "invariant", "iset", "iterator", "label",
            "lemma", "map", "match", "method", "modifies", "modify",
            "module", "multiset", "nat", "new", "newtype", "null", "object",
            "old", "opened", "predicate", "print", "protected",
            "reads", "real", "refines", "requires", "return", "returns", "seq",
            "set", "static", "string", "then", "this", "trait", "true", "type",
            "var", "where", "while", "yield", "yields"
        };

        public HashSet<string> GetReservedWords()
        {
            string assemblyPath = Path.GetDirectoryName(typeof(ReservedWordsProvider).Assembly.Location);
            string jsonFile = Path.GetFullPath(Path.Combine(assemblyPath, "ReservedDafnyWords.json"));

            if (!File.Exists(jsonFile))
            {
                return defaultReservedWords;
            }

            JObject j = JObject.Parse(File.ReadAllText(jsonFile));

            var words = j["words"];
            JArray a = (JArray)words;

            IList<string> l = a.ToObject<IList<string>>();
            return new HashSet<string>(l);
        }
    }
}
