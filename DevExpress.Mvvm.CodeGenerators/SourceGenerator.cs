﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace DevExpress.Mvvm.CodeGenerators {
    [Generator]
    public class ViewModelGenerator : ISourceGenerator {
        public void Initialize(GeneratorInitializationContext context) {
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context) {
            ViewModelGeneratorCore.Execute(context);
        }
    }

    class SyntaxContextReceiver : ISyntaxContextReceiver {
        readonly List<ClassDeclarationSyntax> classSyntaxes = new();
        public IEnumerable<ClassDeclarationSyntax> ClassSyntaxes { get => classSyntaxes; }

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
            if(context.Node is ClassDeclarationSyntax classDeclarationSyntax) {
                classSyntaxes.Add(classDeclarationSyntax);
            }
        }
    }
}
