using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;

namespace DafnyLanguageServer.DafnyAccess
{
    public class DiagnosticError
    {
        public IToken Tok { get; set; }
        public string Msg { get; set; }
        public List<ErrorInformation.AuxErrorInfo> Aux { get; set; } = new List<ErrorInformation.AuxErrorInfo>();

        public DiagnosticError(IToken tok, string msg)
        {
            Tok = tok;
            Msg = msg;
        }
    }

    public static class ConverterExtensions
    {

        public static DiagnosticError ConvertToErrorInformation(this ErrorMessage eMsg)
        {
            return new DiagnosticError(eMsg.token, eMsg.message);
        }

        public static DiagnosticError ConvertToErrorInformation(this ErrorInformation eInfo)
        {
            var result = new DiagnosticError(eInfo.Tok, eInfo.Msg);
            result.Aux = eInfo.Aux;  //todo evtl clonen wegen refernezen zeug? ne oder?
            return result;
        }

        public static List<DiagnosticError> ToDiagnosticErrorList<T>(this IEnumerable<T> source, Func<T, DiagnosticError> converter)
        {
            return source?.Select(converter).ToList();
        }


    }
}
