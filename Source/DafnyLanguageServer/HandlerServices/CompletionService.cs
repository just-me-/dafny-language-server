using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.HandlerServices
{
    public enum CompletionType
    {
        afterDot,
        afterNew,
        allInScope
    }

    public class CompletionService
    {

        public CompletionType GetSupposedDesire(int colPos, string line, out string symbolName)
        {
            return CompletionType.afterNew;
        }
        private CompletionType oLDGetSupposedDesire(int colPos, string line, out string symbolName) // hmm evt doch besser ins file selbst? 
        {
            // diese ganze funktion ist hässlich und buggy. 
            var characters = line.ToCharArray();
            int position = colPos - 2;
            if (position > characters.Length || position < 0)
            {
                throw new ArgumentException("Curser position ist ausserhalb der Zeilenreichweite "); // todo translation 
            }
            if (characters[position] == '.')
            {
                position--;
                var symbolString = "";
                while (position >= 0)
                {
                    if (char.IsLetter(characters[position])
                        || char.IsNumber(characters[position])
                        || characters[position] == '_'
                        || characters[position] == '-'
                    ) // hmm ned mit regex weil chars... testen ob das a-zA-Z0-9-_ gleichwertig ist... 
                    {
                        symbolString += characters[position];
                        position--;
                    }
                    else // das else iwie streichen mit != 
                    {
                        break;
                    }
                }
                char[] symbolCharArray = symbolString.ToCharArray();
                Array.Reverse(symbolCharArray);
                symbolName = new string(symbolCharArray);
                return CompletionType.afterDot;
            }
            symbolName = "";
            if (characters.Length >= 3 && position >= 3
                                       && characters[position] == ' '
                                       && characters[position - 1] == 'w'
                                       && characters[position - 2] == 'e'
                                       && characters[position - 3] == 'n')
            {
                return CompletionType.afterNew;
            }
            return CompletionType.allInScope;
        }
    }
}
