using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace EFRepository.Generator;

[Generator]
public class ExtensionMethodGenerator : ISourceGenerator
{

	public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

	public void Execute(GeneratorExecutionContext context)
	{
		if (context.SyntaxReceiver is not SyntaxReceiver receiver)
			return;

		var options = ((CSharpCompilation)context.Compilation).SyntaxTrees[0].Options as CSharpParseOptions;
		var compilation = context.Compilation;

		var dbContextSymbol = compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbContext");

		if (dbContextSymbol == null)
			dbContextSymbol = compilation.GetTypeByMetadataName("Microsoft.EntityFramework.DbContext");

		if (dbContextSymbol == null)
			return;

		var dbSetSymbol = compilation.GetTypeByMetadataName("Microsoft.EntityFrameworkCore.DbSet`1");

		if (dbSetSymbol == null)
			dbSetSymbol = compilation.GetTypeByMetadataName("Microsoft.EntityFramework.DbSet");

		if (dbSetSymbol == null)
			return;

		var dbContexts = receiver.Classes.Select(c =>
		{
			var sourceModel = compilation.GetSemanticModel(c.SyntaxTree);
			var sourceSymbol = sourceModel.GetDeclaredSymbol(c);

			if (sourceModel == null || sourceSymbol == null)
				return null;

			return new SourceRecord
			{
				Model = sourceModel,
				Symbol = sourceSymbol,
				Syntax = c
			};
		})
		.Where(c => c != null)
		.Where(c => c.Symbol != null && c.Symbol.ContainingSymbol.Equals(c.Symbol.ContainingNamespace, SymbolEqualityComparer.Default)
					&& c.Symbol.BaseType.Equals(dbContextSymbol, SymbolEqualityComparer.Default));
		// TODO: Need to check that this is actually a DbContext class

		var extensions = dbContexts.SelectMany(dbc => ProcessDbContext(dbc, dbSetSymbol, compilation));

		foreach (var (FileName, Content) in extensions)
		{
			context.AddSource(FileName, SourceText.From(Content, Encoding.UTF8));
		}
	}

	protected IEnumerable<(string FileName, string Content)> ProcessDbContext(SourceRecord record, INamedTypeSymbol dbSetSymbol, Compilation compilation)
	{
		var dbSets = record.Symbol.GetMembers()
		.Where(m => !m.IsStatic && !m.IsAbstract && !m.IsImplicitlyDeclared
					&& m.Kind is SymbolKind.Property && m.IsDefinition)
		.Cast<IPropertySymbol>()
		.Where(p => !p.IsReadOnly && !p.IsExtern)
		.Where(p =>
		{
			return p.Type.OriginalDefinition.Equals(dbSetSymbol, SymbolEqualityComparer.Default);
		});

		var list = new List<(string FileName, string Content)>();

		foreach (var dbSet in dbSets)
		{
			var genericType = ((INamedTypeSymbol)dbSet.Type).TypeArguments[0];

			if (genericType == null)
				continue;

			var sourceCode = BuildExtensionMethod((INamedTypeSymbol)genericType, compilation);

			list.Add(sourceCode);
		}

		return list;
	}

	protected (string FileName, string Content) BuildExtensionMethod(INamedTypeSymbol dbSetClass, Compilation compilation)
	{
		string fileName = $"{dbSetClass.Name}.EFExtensions.g.cs";

		var builder = new StringBuilder($@"
using System;
using System.Linq;

namespace {dbSetClass.ContainingNamespace}
{{
	public static class {dbSetClass.Name}Extensions
	{{
");

		var boolSymbol = compilation.GetTypeByMetadataName("System.Boolean");
		var byteSymbol = compilation.GetTypeByMetadataName("System.Byte");
		var shortSymbol = compilation.GetTypeByMetadataName("System.Int16");
		var intSymbol = compilation.GetTypeByMetadataName("System.Int32");
		var longSymbol = compilation.GetTypeByMetadataName("System.Int64");
		var floatSymbol = compilation.GetTypeByMetadataName("System.Single");
		var doubleSymbol = compilation.GetTypeByMetadataName("System.Double");
		var decimalSymbol = compilation.GetTypeByMetadataName("System.Decimal");
		var dateTimeSymbol = compilation.GetTypeByMetadataName("System.DateTime");

		var members = dbSetClass.GetMembers()
		.Where((m) => !m.IsStatic && !m.IsAbstract && !m.IsImplicitlyDeclared && m.IsDefinition
					&& m.Kind is SymbolKind.Property or SymbolKind.Field);

		foreach (var member in members)
		{
			ITypeSymbol type;

			if (member.Kind is SymbolKind.Property)
			{
				var property = (IPropertySymbol)member;
				type = property.Type;
			}
			else
			{
				var field = (IFieldSymbol)member;
				type = field.Type;
			}

			if (type.Equals(boolSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("bool", member.Name));
			}
			else if (type.Equals(byteSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("byte", member.Name));
			}
			else if (type.Equals(shortSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("short", member.Name));
			}
			else if (type.Equals(intSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("int", member.Name));
			}
			else if (type.Equals(longSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("long", member.Name));
			}
			else if (type.Equals(floatSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("float", member.Name));
			}
			else if (type.Equals(doubleSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("double", member.Name));
			}
			else if (type.Equals(decimalSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("decimal", member.Name));
			}
			else if (type.Equals(dateTimeSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine($@"/// <summary>Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not the provided <see cref=""DateTime"" /> is after {member.Name}</summary>
		/// <param name=""value"">The <see cref=""DateTime""/> that {member.Name} should be before</param>
		public static IQueryable<{dbSetClass}> Where{member.Name}IsBefore(this IQueryable<{dbSetClass.Name}> queryable, DateTime value)
		{{
			if (queryable == null)
				return queryable;

			return queryable.Where(n => n.{member.Name} < value);
		}}");

				builder.AppendLine($@"		/// <summary>Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not the provided <see cref=""DateTime"" /> is before {member.Name}</summary>
		/// <param name=""value"">The <see cref=""DateTime""/> that {member.Name} should be before</param>
		public static IQueryable<{dbSetClass}> Where{member.Name}IsAfter(this IQueryable<{dbSetClass.Name}> queryable, DateTime value)
		{{
			if (queryable == null)
				return queryable;

			return queryable.Where(n => n.{member.Name} > value);
		}}");

				builder.AppendLine($@"		/// <summary>Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not {member.Name} is between the two provided values.
		/// <param name=""start"">The <see cref=""DateTime""/> that should be before {member.Name}</param>
		/// <param name=""end"">The <see cref=""DateTime""/> that should be after {member.Name}</param>
		public static IQueryable<{dbSetClass}> Where{member.Name}IsBetween(this IQueryable<{dbSetClass.Name}> queryable, DateTime start, DateTime end)
		{{
			if (queryable == null)
				return queryable;

			return queryable.Where(n => n.{member.Name} > start && n.{member.Name} < end);
		}}");
			}

		}

		builder.AppendLine("	}")
		.AppendLine("}");

		return (fileName, builder.ToString());

		string CreateMethod(string type, string memberName)
		{
			return $@"		///<summary>Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName}</summary>
		<param name=""value"">The {type} that {memberName} should be equal to</param>
		public static IQueryable<{dbSetClass.Name}> By{memberName}(this IQueryable<{dbSetClass.Name}> queryable, {type} value)
		{{
			if (queryable == null)
				return queryable;

			return queryable.Where(n => n.{memberName} == value);
		}}";
		}
	}
}

public record SourceRecord
{
	public SemanticModel Model { get; init; }
	public INamedTypeSymbol Symbol { get; init; }
	public TypeDeclarationSyntax Syntax { get; init; }
}
