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

        public virtual void AddAuxInfo(IToken tok, string msg, string category = null)
        {
            this.Aux.Add(new ErrorInformation.AuxErrorInfo(tok, msg, category));
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
            if (eInfo is null) return null;
            return new DiagnosticError(eInfo.Tok, eInfo.Msg)
            {
           
                Aux = eInfo.Aux  //todo evtl clonen wegen refernezen zeug? ne oder?
            };
        }

        public static List<DiagnosticError> ToDiagnosticErrorList<T>(this IEnumerable<T> source, Func<T, DiagnosticError> converter)
        {
            return source?.Select(converter).ToList();
        }


    }
}
