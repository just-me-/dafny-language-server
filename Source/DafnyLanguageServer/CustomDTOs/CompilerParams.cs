using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler;
using MediatR;

namespace DafnyLanguageServer.CustomDTOs
{
    public class CompilerParams : IRequest<CompilerResults>
    {
        public string FileToCompile { get; set; }
        public string[] CompilationArguments { get; set; }
    }
}
