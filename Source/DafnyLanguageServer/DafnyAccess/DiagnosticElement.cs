using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;

namespace DafnyLanguageServer.DafnyAccess
{
    /// <summary>
    /// This class represents DiagnosticElements.
    /// Dafny, as well as Boogi DiagnosticElements can be converted into DiagnosticErrors.
    /// These can be used to create Diagnostics.
    /// </summary>
    public class DiagnosticElement
    {
        public IToken Tok { get; set; }
        public string Msg { get; set; }
        public List<ErrorInformation.AuxErrorInfo> Aux { get; set; } = new List<ErrorInformation.AuxErrorInfo>();

        public ErrorLevel Severity { get; set; }

        public DiagnosticElement(IToken tok, string msg, ErrorLevel l)
        {
            Tok = tok;
            Msg = msg;
            Severity = l;
        }

        public virtual void AddAuxInfo(IToken tok, string msg, string category = null)
        {
            Aux.Add(new ErrorInformation.AuxErrorInfo(tok, msg, category));
        }
    }

    public static class ConverterExtensions
    {
        //Converting Dafny DiagnosticElements. These can be info, warning and error.
        //If Error, the extra Info "Syntax Error" is added, to separate them from logical errors.
        public static DiagnosticElement ConvertToErrorInformation(this ErrorMessage eMsg, ErrorLevel severity)
        {
            if (severity == ErrorLevel.Error)
            {
                return new DiagnosticElement(eMsg.token, "Syntax Error: " + eMsg.message, severity);
            }
            return new DiagnosticElement(eMsg.token, eMsg.message, severity);

        }

        //Converting Boogie DiagnosticElements. These are also of Severity Error.
        public static DiagnosticElement ConvertToErrorInformation(this ErrorInformation eInfo)
        {
            if (eInfo is null) return null;
            return new DiagnosticElement(eInfo.Tok, "Logical Error: " + eInfo.Msg, ErrorLevel.Error)
            {
           
                Aux = eInfo.Aux
            };
        }

        public static List<DiagnosticElement> ToDiagnosticErrorList<T>(this IEnumerable<T> source, Func<T, DiagnosticElement> converter)
        {
            return source?.Select(converter).ToList();
        }
    }
}
