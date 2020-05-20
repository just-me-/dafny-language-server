using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    public interface IHoverProvider
    {
        Hover GetHoverInformation(Uri uri, int line, int col);
    }
}
