//-----------------------------------------------------------------------
// <copyright file="NerdBankRules.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace NerdBank.StyleCop.Rules {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;

	using Microsoft.StyleCop;
	using Microsoft.StyleCop.CSharp;

	/// <summary>
	/// The several rules that can be enabled/disabled individually.
	/// </summary>
	public enum NerdBankRule {
		/// <summary>
		/// Ensures that code lines never end with trailing whitespace.
		/// </summary>
		NoTrailingWhiteSpace,

		/// <summary>
		/// Ensures that code uses tab-based indentation instead of spaces.
		/// </summary>
		IndentUsingTabs,

		/// <summary>
		/// Ensures that code never has a space before a tab.
		/// </summary>
		NoSpacesBeforeTabs,

		/// <summary>
		/// Ensures that code uses spaces to line characters up with the previous line in specific place.
		/// </summary>
		OneTabIndent,
	}

	/// <summary>
	/// NerdBank custom StyleCop rules.
	/// </summary>
	[SourceAnalyzer(typeof(CsParser))]
	public class NerdBankRules : SourceAnalyzer {
		/// <summary>
		/// Analyzes a code file.
		/// </summary>
		/// <param name="document">The document to be analyzed.</param>
		public override void AnalyzeDocument(CodeDocument document) {
			CsDocument csharpDocument = (CsDocument)document;
			if (csharpDocument.RootElement != null && !csharpDocument.RootElement.Generated) {
				bool lastTokenWasWhitespace = false;
				bool lastTokenWasEndOfLine = false;
				bool firstToken = true;
				string lastLineIndentation = string.Empty;
				int lengthOfLastLine = 0;
				int lengthOfCurrentLine = 0;
				CsToken lastNonWhitespaceToken = null;
				foreach (CsToken token in csharpDocument.Tokens) {
					bool startOfLine = lastTokenWasEndOfLine || firstToken;
					lengthOfCurrentLine += token.Text.Length;

					if (token.CsTokenType == CsTokenType.WhiteSpace) {
						if (token.Text.Contains(" \t")) {
							this.AddViolation(csharpDocument.RootElement, token.LineNumber, NerdBankRule.NoSpacesBeforeTabs);
						}
					}

					// WIP: Support this: (note the use of tab-space-tab to support proper character alignment and indentation)
					////	var contacts = from entry in contactsDocument.Root.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom"))
					////	               select new {
					////	               	Name = entry.Element(XName.Get("title", "http://www.w3.org/2005/Atom")).Value,
					////	               	Email = entry.Element(XName.Get("email", "http://schemas.google.com/g/2005")).Attribute("address").Value,
					////	               };


					if (token.CsTokenType == CsTokenType.WhiteSpace && startOfLine) {
						// Only allow spaces after as many tabs as were on the previous line,
						// and no more spaces than the length of the previous line minus the tabs,
						// and only one more tab than the previous line had (or else they're probably trying 
						// to do some kind of character alignment with the previous line, which should be
						// done with spaces.
						int numberOfIndentingTabsThisLine = token.Text.ToCharArray().TakeWhile(ch => ch == '\t').Count();
						int numberOfIndentingTabsLastLine = lastLineIndentation.ToCharArray().TakeWhile(ch => ch == '\t').Count();

						if (numberOfIndentingTabsThisLine > numberOfIndentingTabsLastLine + 1) {
							// Disabling until the above WIP is fixed.
							////this.AddViolation(csharpDocument.RootElement, token.LineNumber, NerdBankRule.OneTabIndent);
						}

						if (token.Text.Contains(" ")) {
							if (numberOfIndentingTabsThisLine < numberOfIndentingTabsLastLine) {
								// This line has fewer tabs indenting it than the previous line did,
								// so there definitely should not have been any spaces in the indentation.
								this.AddViolation(csharpDocument.RootElement, token.LineNumber, NerdBankRule.IndentUsingTabs);
							} else {
								int lastLineContentLength = lengthOfLastLine - numberOfIndentingTabsLastLine - 1; // -1 to not include \n
								int numberOfIndentingSpacesThisLine = token.Text.ToCharArray().SkipWhile(ch => ch == '\t').TakeWhile(ch => ch == ' ').Count();
								if (numberOfIndentingSpacesThisLine > lastLineContentLength) {
									// This line is indented with spaces that go further out than the last character of the last line.
									this.AddViolation(csharpDocument.RootElement, token.LineNumber, NerdBankRule.IndentUsingTabs);
								} else {
									if (lastNonWhitespaceToken.CsTokenType == CsTokenType.OpenCurlyBracket) {
										// This is the beginning of a new block.  No spaces should be used in indentation.
										this.AddViolation(csharpDocument.RootElement, token.LineNumber, NerdBankRule.IndentUsingTabs);
									}
								}
							}
						}

						lastLineIndentation = token.Text;
					}

					if (token.CsTokenType == CsTokenType.EndOfLine && lastTokenWasWhitespace) {
						this.AddViolation(csharpDocument.RootElement, token.LineNumber, NerdBankRule.NoTrailingWhiteSpace);
					}

					lastTokenWasEndOfLine = token.CsTokenType == CsTokenType.EndOfLine;
					lastTokenWasWhitespace = token.CsTokenType == CsTokenType.WhiteSpace;
					firstToken = false;
					if (token.CsTokenType != CsTokenType.WhiteSpace && token.CsTokenType != CsTokenType.EndOfLine) {
						lastNonWhitespaceToken = token;
					}

					if (token.CsTokenType == CsTokenType.EndOfLine) {
						lengthOfLastLine = lengthOfCurrentLine;
						lengthOfCurrentLine = 0;
					}
				}
			}
		}
	}
}
