using DafnyLanguageServer.Commons;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace DafnyLanguageServer.Tools
{
    /// <summary>
    /// Provides a HashSet of reserved Dafny words that are not suited as names.
    /// </summary>
    public class ReservedWordsProvider
    {
        /// <summary>
        /// Inline defined set as backup, in case no custom file is provided.
        /// </summary>
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

        /// <summary>
        /// Reads a maybe more recent list from the json-configurable file.
        /// </summary>
        /// <returns>HashSet of reserved words</returns>
        public HashSet<string> GetReservedWords()
        {
            string jsonFile = FileAndFolderLocations.reservedWordList;

            if (!File.Exists(jsonFile))
            {
                return defaultReservedWords;
            }

            try
            {
                JObject j = JObject.Parse(File.ReadAllText(jsonFile));

                var words = j["words"];
                JArray a = (JArray)words;
                IList<string> l = a.ToObject<IList<string>>();
                return new HashSet<string>(l);
            }
            catch
            {
                return defaultReservedWords;
            }
        }
    }
}