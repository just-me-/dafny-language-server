using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DafnyLanguageServer.SymbolTable;
using Microsoft.Boogie;
using Microsoft.Dafny;
using Type = Microsoft.Dafny.Type;

namespace SymbolTableTest
{
    public class SymbolInformationFake : ISymbolInformation
    {


        public SymbolInformationFake(int mainLine, int mainCol,
            int bodyStartLine = -1, int bodyStartCol = -1,
            int bodyEndLine = -1, int bodyEndCol = -1,
            string file = @"C:/file.dfy", string name = "no_name"
            )
        {

            ChildrenHash = new Dictionary<string, ISymbolInformation>();
            Descendants = null;
            Parent = null;
            Name = name;

            IToken mainTok = new Token()
            {
                line = mainLine,
                col = mainCol,
                filename = file
            };

            IToken bodyStart = null;
            IToken bodyEnd = null;
            if (bodyStartLine != -1)
            {
                bodyStart = new Token()
                {
                    line = bodyStartLine,
                    col = bodyStartCol,
                    filename = file
                };
                bodyEnd = new Token()
                {
                    line = bodyEndLine,
                    col = bodyEndCol,
                    filename = file
                };
            }

            Position = new TokenPosition()
            {
                Token = mainTok,
                BodyStartToken = bodyStart,
                BodyEndToken = bodyEnd
            };
        }

        public void AddChild(ISymbolInformation child)
        {
            ChildrenHash.Add(child.Name, child);
        }

        public void AddSubsymbol(ISymbolInformation child)
        {
            if (Descendants == null)
            {
                Descendants = new List<ISymbolInformation>();
            }
            Descendants.Add(child);
        }

        public void SetParent(ISymbolInformation parent)
        {
            Parent = parent;
        }

        public TokenPosition Position { get; set; }

        public int Line => Position.Token.line;
        public int Column => Position.Token.col;
        public int IdentifierEndColumn => Position.Token.col + Name.Length;
        public bool HasBody => Position.BodyStartToken != null && Position.BodyEndToken != null;

        public string Name { get; set; }
        public Uri FileUri => new Uri(Position?.Token?.filename ?? "N:\\u\\l.l");
        public string FileName => Path.GetFileName(FileUri.LocalPath);

        public bool HasInheritedMembers => Kind == Kind.Class && (BaseClasses?.Any() ?? false);
        public bool HasChildren => ChildrenHash != null && ChildrenHash.Any();
        public List<ISymbolInformation> Children => ChildrenHash?.Values.ToList();      //children: only declarations
        public bool IsDeclaration => ReferenceEquals(DeclarationOrigin, this);
        public UserDefinedType UserTypeDefinition
        {
            get
            {
                if (Type is UserDefinedType type)
                {
                    return type;
                }
                return null;
            }
        }

        public Kind Kind { get; set; }
        public Type Type { get; set; }
        public ISymbolInformation Parent { get; set; }
        public ISymbolInformation DeclarationOrigin { get; set; }
        public Dictionary<string, ISymbolInformation> ChildrenHash { get; set; }
        public List<ISymbolInformation> Usages { get; set; }
        public List<ISymbolInformation> BaseClasses { get; set; }
        public List<ISymbolInformation> Params { get; set; }
        public List<ISymbolInformation> Descendants { get; set; }
        public ISymbolInformation Module { get; set; }
        public ISymbolInformation AssociatedDefaultClass { get; }

        public string PositionToFormattedString()
        {
            throw new InvalidOperationException();

        }

        public string ToDebugString()
        {
            throw new InvalidOperationException();
        }

        public ISymbolInformation this[string index]
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }
    }
}
