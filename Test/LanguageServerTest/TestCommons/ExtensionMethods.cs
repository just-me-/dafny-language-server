using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using DafnyLanguageServer.CustomDTOs;
using DafnyLanguageServer.Handler;
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

            if (diagItem.RelatedInformation != null && diagItem.RelatedInformation.Any())
            {
                result += " Related Information: ";
                foreach (var r in diagItem.RelatedInformation)
                {
                    string relatedMsg = r.Message;
                    string relatedPos = r.Location.Range.ToCustomString();
                    result += $"{relatedPos} {relatedMsg}";
                }
            }

            return result;
        }

        public static string ToCustomString(this CounterExample ce)
        {
            if (ce == null)
            {
                return null;
            }
            string result = $"L{ce.Line} C{ce.Col}: ";
            result = ce.Variables.Aggregate(result, (current, kvp) => current + $"{kvp.Key} = {kvp.Value}; ");
            return result;
        }

        public static string ToCustomString(this TextEdit te)
        {
            if (te == null)
            {
                return null;
            }

            return $"{te.NewText} at L{te.Range.Start.Line}:C{te.Range.Start.Character} - L{te.Range.End.Line}:C{te.Range.End.Character}";
        }

        public static List<string> ToStringList(this IEnumerable<Diagnostic> source)
        {
            return GenericToStringList(source, ToCustomString);
        }

        public static List<string> ToStringList(this IEnumerable<CounterExample> source)
        {
            return GenericToStringList(source, ToCustomString);
        }

        public static List<string> ToStringList(this IEnumerable<TextEdit> source)
        {
            return GenericToStringList(source, ToCustomString);
        }

        private static List<string> GenericToStringList<T>(this IEnumerable<T> source, Func<T, string> converter)
        {
            return source?.Select(converter).ToList();
        }
    }



}
