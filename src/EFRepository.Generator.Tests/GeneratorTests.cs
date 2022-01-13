using System;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace EFRepository.Generator.Tests
{
	public class GeneratorTests
	{
		[Fact]
		public void ExtensionsWereGenerated()
		{
			string? file = File.ReadAllText(@"./TestData/AllClasses.txt");
			var compilation = CreateCompilation(file);
			var generator = new ExtensionMethodGenerator();
			var driver = CSharpGeneratorDriver.Create(generator);

			var name = typeof(DbSet<>);

			driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

			diagnostics.IsDefaultOrEmpty.ShouldBeTrue();
			var outputDiag = outputCompilation.GetDiagnostics();

			var allClasses = outputCompilation.SyntaxTrees.Where(st => st.FilePath.EndsWith(".EFRepoExtensions.g.cs"));

			allClasses.ShouldNotBeNull("No classes were generated");

			allClasses.Count().ShouldBe(3, "Not all classes were generated.");


		}

		protected static Compilation CreateCompilation(string source)
			=> CSharpCompilation.Create("compilation",
				new[] { CSharpSyntaxTree.ParseText(source) },
				new[]
				{
					MetadataReference.CreateFromFile(typeof(DateTime).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(Attribute).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(System.ComponentModel.INotifyPropertyChanged).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(MulticastDelegate).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(DbContext).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(DbSet<>).GetTypeInfo().Assembly.Location),
				},
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}
}
