using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;

namespace DafnyLanguageServer.DafnyAccess
{
    /// <summary>
    /// This class ... todo 
    /// </summary>
    public class DafnyConsolePrinter : ConsolePrinter
    {
        public override void ReportBplError(IToken tok, string message, bool error, TextWriter tw, string category = null)
        {
            // Dafny has 0-indexed columns, but Boogie counts from 1
            var realigned_tok = new Token(tok.line, tok.col - 1);
            realigned_tok.kind = tok.kind;
            realigned_tok.pos = tok.pos;
            realigned_tok.val = tok.val;
            realigned_tok.filename = tok.filename;
            base.ReportBplError(realigned_tok, message, error, tw, category);

            if (tok is NestedToken nt)
            {
                ReportBplError(nt.Inner, "Related location", false, tw);
            }
        }
    }
}
