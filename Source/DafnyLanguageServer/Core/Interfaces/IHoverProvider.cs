using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;

namespace DafnyLanguageServer.Core
{
    public interface IHoverProvider
    {
        Hover GetHoverInformation(Uri uri, int line, int col);
    }
}