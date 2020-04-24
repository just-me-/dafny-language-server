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
    /// This class represents DiagnosticElements, such as an Error or a Warning.
    /// Since Dafny and Boogie have different types of Errors, and since they are sometimes not accessible, error informations are unified by this class.
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
        ///<summary>
        ///Converts Dafny Errors into DiagnosticElements. These can be of type info, warning and error.
        ///If error, the extra text "Syntax Error" is added, to separate them from logical errors.
        /// </summary>  
        public static DiagnosticElement ConvertToErrorInformation(this ErrorMessage eMsg, ErrorLevel severity)
        {
            if (severity == ErrorLevel.Error)
            {
                return new DiagnosticElement(eMsg.token, "Syntax Error: " + eMsg.message, severity);
            }
            return new DiagnosticElement(eMsg.token, eMsg.message, severity);

        }

        /// <summary>
        ///Converts Boogie Errors into DiagnosticElements. These can only be of type error.
        ///The extra text "Logical Error" is added, to separate them from syntax errors.
        /// </summary>  
        public static DiagnosticElement ConvertToErrorInformation(this ErrorInformation eInfo)
        {
            if (eInfo is null) return null;
            return new DiagnosticElement(eInfo.Tok, "Logical Error: " + eInfo.Msg, ErrorLevel.Error)
            {
                Aux = eInfo.Aux
            };
        }

        /// <summary>
        /// Generic method that converts a list of errors into a list of DiagnosticElements using a converter.
        /// </summary>
        /// <typeparam name="T">Origin Type, either Dafny or Boogie Error [ErrorInformation or ErrorMessage]</typeparam>
        /// <param name="source">List of errors of Type T</param>
        /// <param name="converter">A converter, converting an Error from T to DiagnosticElement</param>
        /// <returns>Converted List</returns>
        public static List<DiagnosticElement> ToDiagnosticErrorList<T>(this IEnumerable<T> source, Func<T, DiagnosticElement> converter)
        {
            return source?.Select(converter).ToList();
        }
    }
}
