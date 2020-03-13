using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace TestCommons
{

        public static class Extension
        {
            public static string ToCustomString(this Range r)
            {
                return $"R[L{r.Start.Line} C{r.Start.Character} - L{r.End.Line} C{r.End.Character}]";
            }

            public static string ToCustomString(this Diagnostic diagItem)
            {
                if (diagItem == null)
                {
                    return null;
                }

                var severity = diagItem.Severity;
                string msg = diagItem.Message;

                string result = $"{diagItem.Range.ToCustomString()} - {severity} - {msg}";

                if (diagItem.RelatedInformation != null)
                {
                    string relatedMsg = diagItem.RelatedInformation.FirstOrDefault()?.Message;
                    string relatedPos = diagItem.RelatedInformation.FirstOrDefault()?.Location.Range.ToCustomString();
                    result += $" / {relatedPos} - {relatedMsg}";
                }

                return result;
            }


            

            public static List<string> ToStringList(this List<Diagnostic> source)
            {
                return source?.Select(ToCustomString).ToList();
            }
    }


    
}
