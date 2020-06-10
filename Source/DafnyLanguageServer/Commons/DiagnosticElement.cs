using Microsoft.Boogie;
using Microsoft.Dafny;
using System.Collections.Generic;

namespace DafnyLanguageServer.Commons
{
    /// <summary>
    /// This class represents DiagnosticElements, such as an Error or a Warning. Since Dafny and
    /// Boogie have different types of Errors, and since they are sometimes not accessible, error
    /// informations are unified by this class.
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
            IToken editedToken = RepositionErrorsWithoutPosToL1C1(eMsg.token);
            if (severity == ErrorLevel.Error)
            {
                return new DiagnosticElement(editedToken, Resources.LoggingMessages.diagnostic_syntax_error + "\n " + eMsg.message, severity);
            }
            return new DiagnosticElement(editedToken, eMsg.message, severity);
        }

        /// <summary>
        ///Converts Boogie Errors into DiagnosticElements. These can only be of type error.
        ///The extra text "Logical Error" is added, to separate them from syntax errors.
        /// </summary>
        public static DiagnosticElement ConvertToErrorInformation(this ErrorInformation eInfo)
        {
            if (eInfo is null)
            {
                return null;
            }
            IToken editedToken = RepositionErrorsWithoutPosToL1C1(eInfo.Tok);

            return new DiagnosticElement(editedToken, Resources.LoggingMessages.diagnostic_logical_error + "\n " + eInfo.Msg, ErrorLevel.Error)
            {
                Aux = eInfo.Aux
            };
        }

        private static IToken RepositionErrorsWithoutPosToL1C1(IToken originalToken)
        {
            IToken result = new Token();
            int finalLine = originalToken.line == 0 ? 1 : originalToken.line;
            int finalCol = originalToken.col == 0 ? 1 : originalToken.col;
            result.line = finalLine;
            result.col = finalCol;
            result.val = originalToken.val;
            result.kind = originalToken.kind;
            result.pos = originalToken.pos;
            return result;
        }
    }
}