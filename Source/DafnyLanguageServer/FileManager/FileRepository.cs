﻿using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using DafnyLanguageServer.HandlerServices;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using CounterExampleProvider = DafnyLanguageServer.HandlerServices.CounterExampleProvider;
using Type = Microsoft.Dafny.Type;

namespace DafnyLanguageServer.FileManager
{
    /// <summary>
    /// This class represents a buffered version of a Dafny file.
    /// Attributes like Uri, Dafny source code and not buffered informations are stored in the <c>PhysicalFile</c>.
    ///
    /// A <c>FileRepository</c> has also its own <c>DafnyTranslationUnit</c> to verify this Dafny file and provide related information.
    /// </summary>
    public class FileRepository
    {
        public PhysicalFile PhysicalFile { get; set; }
        public TranslationResult Result { get; set; }

        public void UpdateFile(string sourceCodeOfFile)
        {
            PhysicalFile.Sourcecode = sourceCodeOfFile;
            GenerateTranslationResult();
        }

        public void UpdateFile(Container<TextDocumentContentChangeEvent> changes)
        {
            foreach (var change in changes)
            {
                PhysicalFile.Apply(change);
            }

            GenerateTranslationResult();
        }

        private void GenerateTranslationResult()
        {
            if(PhysicalFile != null) {
                DafnyTranslationUnit translationUnit = new DafnyTranslationUnit(PhysicalFile);
                Result = translationUnit.Verify();
            }
        }

        private List<DafnyServer.OldSymbolTable.OldSymbolInformation> Symboltable()
        {
            if (Result.TranslationStatus >= TranslationStatus.Resolved && Result.DafnyProgram != null) //todo
            {
                Type.ResetScopes();
                var symbolTable = new DafnyServer.OldSymbolTable(Result.DafnyProgram);
                return symbolTable.CalculateSymbols();
            }
            else
            {
                return new List<DafnyServer.OldSymbolTable.OldSymbolInformation>();
            }
        }

        public FileSymboltableProcessor SymboleProcessor()
        {
            return new FileSymboltableProcessor(this.Symboltable());
        }

        public CounterExampleResults CounterExample()
        {
            if (!File.Exists(PhysicalFile.Filepath))
            {
                throw new FileNotFoundException("CounterExample requires a valid filename. Invalid Path: " + PhysicalFile.Filepath);
            }

            try
            {
                if (Result.TranslationStatus >= TranslationStatus.Translated)
                {
                    return new CounterExampleProvider(PhysicalFile).LoadCounterModel();
                }
            }

            catch (Exception e)
            {
                throw new InvalidOperationException("Error while collecting models. " + e.Message);

            }

            return new CounterExampleResults();
        }

        public CompilerResults Compile(string[] requestCompilationArguments)
        {
            try
            {
                return new CompilationService(this, requestCompilationArguments).Compile();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while executing compilation. " + e.Message);
            }
        }
    }
}
