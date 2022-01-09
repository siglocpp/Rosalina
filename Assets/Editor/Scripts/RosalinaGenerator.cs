﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

internal static class RosalinaGenerator
{
    private static string GeneratedCodeHeader = @$"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rosalina Code Generator tool.
//     Version: {RosalinaConstants.Version}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

";

    /// <summary>
    /// Generates the UI document code behind.
    /// </summary>
    /// <param name="uiDocumentPath">UI Document path.</param>
    public static void Generate(string uiDocumentPath)
    {
        Debug.Log($"Generating UI code behind for {uiDocumentPath}");
        EditorUtility.DisplayProgressBar("Generating UI code behind", "Working...", 25);

        string outputPath = Path.GetDirectoryName(uiDocumentPath);
        string className = Path.GetFileNameWithoutExtension(uiDocumentPath);
        string generatedCodeBehindFileName = Path.Combine(outputPath, $"{className}.g.cs");

        UsingDirectiveSyntax[] usings = GetDefaultUsingDirectives();
        ClassDeclarationSyntax @class = SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseName(typeof(MonoBehaviour).Name))
            )
            .AddMembers(
                CreateDocumentVariable(),
                CreateVisualElementRootProperty(),
                CreateInitializeMethod()
            );

        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usings)
            .AddMembers(@class);

        string code = compilationUnit
            .NormalizeWhitespace()
            .ToFullString();
        string generatedCode = GeneratedCodeHeader + code;

        File.WriteAllText(generatedCodeBehindFileName, generatedCode);
        EditorUtility.ClearProgressBar();
        Debug.Log($"Done generating: {generatedCodeBehindFileName}");
    }

    private static UsingDirectiveSyntax[] GetDefaultUsingDirectives()
    {
        return new UsingDirectiveSyntax[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("UnityEngine")),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("UnityEngine.UIElements"))
        };
    }

    private static MemberDeclarationSyntax CreateDocumentVariable()
    {
        var documentVariableDeclaration = CreateVariable("_document", typeof(UIDocument));
        var documentFieldDeclaration = SyntaxFactory.FieldDeclaration(documentVariableDeclaration)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
            .AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.ParseName(typeof(SerializeField).Name))
                    )
                )
            );

        return documentFieldDeclaration;
    }

    private static VariableDeclarationSyntax CreateVariable(string variableName, Type variableType)
    {
        var variableTypeName = SyntaxFactory.ParseName(variableType.Name);
        var documentVariableDeclaration = SyntaxFactory.VariableDeclaration(variableTypeName)
            .AddVariables(SyntaxFactory.VariableDeclarator(variableName));

        return documentVariableDeclaration;
    }

    private static MemberDeclarationSyntax CreateVisualElementRootProperty()
    {
        var propertyTypeName = SyntaxFactory.ParseName(typeof(VisualElement).Name);
        var property = SyntaxFactory.PropertyDeclaration(propertyTypeName, "Root")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                SyntaxFactory.AccessorDeclaration(
                    SyntaxKind.GetAccessorDeclaration,
                    SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement("return _document?.rootVisualElement;")
                    )
                )
            );

        return property;
    }

    private static MemberDeclarationSyntax CreateInitializeMethod()
    {
        var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "InitializeDocument")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithBody(SyntaxFactory.Block());

        return method;
    }
}
