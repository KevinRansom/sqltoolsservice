﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

extern alias ASAScriptDom;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.SqlTools.ServiceLayer.DacFx.Contracts;
using Microsoft.SqlTools.ServiceLayer.Utility;
using Microsoft.SqlTools.Utility;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.SqlTools.ServiceLayer.DacFx
{
    /// <summary>
    /// Class to represent inserting an input sql binding into an Azure Function
    /// </summary>
    class InsertSqlInputBindingOperation
    {
        const string functionAttributeText = "FunctionName";

        public InsertSqlBindingParams Parameters { get; }

        public InsertSqlInputBindingOperation(InsertSqlBindingParams parameters)
        {
            Validate.IsNotNull("parameters", parameters);
            this.Parameters = parameters;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public ResultStatus AddInputBinding()
        {
            try
            {
                string text = File.ReadAllText(Parameters.filePath);

                SyntaxTree tree = CSharpSyntaxTree.ParseText(text);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                // look for Azure Function to update
                IEnumerable<MethodDeclarationSyntax> azureFunctionMethods = from methodDeclaration in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                                                                            where methodDeclaration.AttributeLists.Count > 0
                                                                            where methodDeclaration.AttributeLists.Where(a => a.Attributes.Where(attr => attr.Name.ToString().Contains(functionAttributeText) && attr.ArgumentList.Arguments.First().ToString().Equals($"\"{Parameters.functionName}\"")).Count() > 0).Count() > 0
                                                                            select methodDeclaration;

                 if (azureFunctionMethods.Count() == 0)
                {
                    return new ResultStatus()
                    {
                        Success = false,
                        ErrorMessage = $"Couldn't find Azure function with FunctionName {Parameters.functionName} in {Parameters.filePath}"
                    };
                }
                else if (azureFunctionMethods.Count() > 1)
                {
                    return new ResultStatus()
                    {
                        Success = false,
                        ErrorMessage = $"More than one Azure function found with the FunctionName {Parameters.functionName} in {Parameters.filePath}"
                    };
                }

                MethodDeclarationSyntax azureFunction = azureFunctionMethods.First();

                // Create arguments for the Sql Binding attribute
                var argumentList = SyntaxFactory.AttributeArgumentList();
                argumentList = argumentList.AddArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.IdentifierName($"\"select * from {Parameters.objectName}\"")));
                argumentList = argumentList.AddArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("CommandType"), SyntaxFactory.IdentifierName("System.Data.CommandType.Text"))));
                argumentList = argumentList.AddArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("ConnectionStringSetting"), SyntaxFactory.IdentifierName("\"SqlConnectionString\""))));

                // Create Sql Binding attribute
                SyntaxList<AttributeListSyntax> attributesList = new SyntaxList<AttributeListSyntax>();
                attributesList = attributesList.Add(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Sql")).WithArgumentList(argumentList))));

                // Create new parameter
                ParameterSyntax newParam = SyntaxFactory.Parameter(attributesList, new SyntaxTokenList(), SyntaxFactory.ParseTypeName("IEnumerable<Object>"), SyntaxFactory.Identifier("result"), null);

                // Generate updated method with the new parameter
                // normalizewhitespace gets rid of any newline whitespace in the leading trivia, so we ad that back
                var newLineTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n");
                var updatedMethod = azureFunction.AddParameterListParameters(newParam).NormalizeWhitespace().WithLeadingTrivia(azureFunction.GetLeadingTrivia()).WithTrailingTrivia(azureFunction.GetTrailingTrivia());

                // Replace the node in the tree
                root = root.ReplaceNode(azureFunction, updatedMethod);

                // write updated tree to file
                var workspace = new AdhocWorkspace();

                var syntaxTree = CSharpSyntaxTree.ParseText(root.ToString());
                var formattedNode = Microsoft.CodeAnalysis.Formatting.Formatter.Format(syntaxTree.GetRoot(), workspace);
                StringBuilder sb = new StringBuilder(formattedNode.ToString());
                string content = sb.ToString();
                File.WriteAllText(Parameters.filePath, content);

                return new ResultStatus()
                {
                    Success = true
                };
            }
            catch(Exception e)
            {
                return new ResultStatus()
                {
                    Success = false,
                    ErrorMessage = e.ToString()
                };
            }
        }
    }
}

