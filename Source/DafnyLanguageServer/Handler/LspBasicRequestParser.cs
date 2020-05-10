using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Handler
{
    public class LspBasicRequestParser<T> where T : PrepareRenameParams, RenameParams,
    {
        public int GetLine(T request)
        {
            return (int)request.Position.Line + 1;
        }

        public int GetCol(T request)
        {
            return (int)request.Position.Character + 1;
        }
    }
}
