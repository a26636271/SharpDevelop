﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using ICSharpCode.NRefactory;
using ICSharpCode.RubyBinding;
using NUnit.Framework;

namespace RubyBinding.Tests.Converter
{
	[TestFixture]
	public class LocalVariableDefinitionsOnSameLineTests
	{
		string csharp =
			"class Foo\r\n" +
			"{\r\n" +
			"    public Foo()\r\n" +
			"    {\r\n" +
			"        int i = 0, i = 2;\r\n" +
			"    }\r\n" +
			"}";
		
		[Test]
		public void ConvertedRubyCode()
		{
			NRefactoryToRubyConverter converter = new NRefactoryToRubyConverter(SupportedLanguage.CSharp);
			converter.IndentString = "    ";
			string Ruby = converter.Convert(csharp);
			string expectedRuby =
				"class Foo\r\n" +
				"    def initialize()\r\n" +
				"        i = 0\r\n" +
				"        i = 2\r\n" +
				"    end\r\n" +
				"end";
			
			Assert.AreEqual(expectedRuby, Ruby);
		}
		
		string vnetClassWithTwoArrayLocalVariablesOnSameLine =
			"class Foo\r\n" +
			"    Public Sub New()\r\n" +
			"    	Dim i(10), j(20) as integer\r\n" +
			"    End Sub\r\n" +
			"end class";
		
		[Test]
		public void ConvertVBNetClassWithTwoArrayVariablesOnSameLine()
		{
			NRefactoryToRubyConverter converter = new NRefactoryToRubyConverter(SupportedLanguage.VBNet);
			converter.IndentString = "    ";
			string ruby = converter.Convert(vnetClassWithTwoArrayLocalVariablesOnSameLine);
			string expectedRuby =
				"class Foo\r\n" +
				"    def initialize()\r\n" +
				"        i = Array.CreateInstance(System::Int32, 10)\r\n" +
				"        j = Array.CreateInstance(System::Int32, 20)\r\n" +
				"    end\r\n" +
				"end";
			
			Assert.AreEqual(expectedRuby, ruby);
		}
	}
}
