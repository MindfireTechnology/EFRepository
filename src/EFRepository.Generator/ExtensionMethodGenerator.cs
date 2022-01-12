using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace EFRepository.Generator;

[Generator]
public class ExtensionMethodGenerator : ISourceGenerator
{

	public void Initialize(GeneratorInitializationContext context)
	{
		context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
	}

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
		.Where(c =>
			c != null &&
			c.Symbol != null &&
			c.Symbol.ContainingSymbol.Equals(c.Symbol.ContainingNamespace, SymbolEqualityComparer.Default) &&
			c.Symbol.BaseType != null && c.Symbol.BaseType.Equals(dbContextSymbol, SymbolEqualityComparer.Default));

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

		var builder = new StringBuilder(
$@"using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace {dbSetClass.ContainingNamespace};

	public static partial class {dbSetClass.Name}Extensions
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
		var stringSymbol = compilation.GetTypeByMetadataName("System.String");
		var dateTimeSymbol = compilation.GetTypeByMetadataName("System.DateTime");

		var members = dbSetClass.GetMembers()
		.Where((m) => !m.IsStatic && !m.IsAbstract && !m.IsImplicitlyDeclared && m.IsDefinition
					&& m.Kind is SymbolKind.Property or SymbolKind.Field);

		foreach (var member in members)
		{
			ITypeSymbol type;
			bool nullable;

			if (member.Kind is SymbolKind.Property)
			{
				var property = (IPropertySymbol)member;
				type = property.Type;
				nullable = property.NullableAnnotation == NullableAnnotation.Annotated;
			}
			else
			{
				var field = (IFieldSymbol)member;
				type = field.Type;
				nullable = field.NullableAnnotation == NullableAnnotation.Annotated;
			}

			if (type.Equals(boolSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("bool", member.Name, nullable));
			}
			else if (type.Equals(byteSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("byte", member.Name, nullable));
			}
			else if (type.Equals(shortSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("short", member.Name, nullable));
			}
			else if (type.Equals(intSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("int", member.Name, nullable));
			}
			else if (type.Equals(longSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("long", member.Name, nullable));
			}
			else if (type.Equals(floatSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("float", member.Name, nullable));
			}
			else if (type.Equals(doubleSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("double", member.Name, nullable));
			}
			else if (type.Equals(decimalSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("decimal", member.Name, nullable));
			}
			else if (type.Equals(stringSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateNullMethodFunctions(member.Name));

				builder.AppendLine($@"
	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name}
	/// </summary>
	/// <param name=""value"">The {type} which {member.Name} should be equal</param>
	public static IQueryable<{dbSetClass.Name}> By{member.Name}(this IQueryable<{dbSetClass.Name}> query, {type}? value)
	{{
		if (query == null)
			return query;

		if (string.IsNullOrWhiteSpace(value))
			return query;

		return query.Where(n => n.{member.Name} == value);
	}}");

				builder.AppendLine($@"
	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name} contains a value
	/// </summary>
	/// <param name=""value"">The {type} which {member.Name} should contain</param>
	public static IQueryable<{dbSetClass.Name}> By{member.Name}Contains(this IQueryable<{dbSetClass.Name}> query, {type}? value)
	{{
		if (query == null)
			return query;

		if (string.IsNullOrWhiteSpace(value))
			return query;

		return query.Where(n => n.{member.Name}.Contains(value));
	}}");

				builder.AppendLine($@"
	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name} starts with a value
	/// </summary>
	/// <param name=""value"">The {type} which {member.Name} should start with</param>
	public static IQueryable<{dbSetClass.Name}> By{member.Name}StartsWith(this IQueryable<{dbSetClass.Name}> query, {type}? value)
	{{
		if (query == null)
			return query;

		if (string.IsNullOrWhiteSpace(value))
			return query;

		return query.Where(n => n.{member.Name}.StartsWith(value));
	}}");

				builder.AppendLine($@"

	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name} ends with a value
	/// </summary>
	/// <param name=""value"">The {type} which {member.Name} should end with</param>
	public static IQueryable<{dbSetClass.Name}> By{member.Name}EndsWith(this IQueryable<{dbSetClass.Name}> query, {type}? value)
	{{
		if (query == null)
			return query;

		if (string.IsNullOrWhiteSpace(value))
			return query;

		return query.Where(n => n.{member.Name}.EndsWith(value));
	}}");

			}
			else if (type.Equals(dateTimeSymbol, SymbolEqualityComparer.Default))
			{
				builder.AppendLine(CreateMethod("DateTime", member.Name, nullable));

				builder.AppendLine($@"
	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not the provided <see cref=""DateTime"" /> is after {member.Name}
	/// </summary>
	/// <param name=""value"">The <see cref=""DateTime""/> that {member.Name} should be before</param>
	public static IQueryable<{dbSetClass}> By{member.Name}IsBefore(this IQueryable<{dbSetClass.Name}> query, DateTime? value)
	{{
		if (query == null)
			return query;

		if (value == null)
			return query;

		return query.Where(n => n.{member.Name} < value);
	}}");

				builder.AppendLine($@"
	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not the provided <see cref=""DateTime"" /> is after {member.Name}
	/// </summary>
	/// <param name=""value"">The <see cref=""DateTime""/> that {member.Name} should be after</param>
	public static IQueryable<{dbSetClass}> By{member.Name}IsAfter(this IQueryable<{dbSetClass.Name}> query, DateTime? value)
	{{
		if (query == null)
			return query;

		if (value == null)
			return query;

		return query.Where(n => n.{member.Name} > value);
	}}");

				builder.AppendLine($@"
	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not {member.Name} is between the two provided values.
	/// </summary>
	/// <param name=""start"">The <see cref=""DateTime""/> that should be before {member.Name}</param>
	/// <param name=""end"">The <see cref=""DateTime""/> that should be after {member.Name}</param>
	public static IQueryable<{dbSetClass}> By{member.Name}Between(this IQueryable<{dbSetClass.Name}> query, DateTime? start, DateTime? end)
	{{
		if (query == null)
			return query;

		if (start != null)
			query = query.Where(n => n.{member.Name} > start);

		if (end != null)
			query = query.Where(n => n.{member.Name} < end);

		return query;
	}}");

				builder.AppendLine($@"
	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not {member.Name} is between the two provided values.
	/// </summary>
	/// <param name=""value"">The <see cref=""DateTime""/> that should the same date as {member.Name}, excluding time</param>
	public static IQueryable<{dbSetClass}> By{member.Name}OnDate(this IQueryable<{dbSetClass.Name}> query, DateTime? value)
	{{
		if (query == null)
			return query;

		if (value != null)
			return query.Where(n => n.{member.Name}.Date > value.Value.Date);
		else
			return query;
	}}");
			}
		}

		// Close Class
		builder.AppendLine("}");

		return (fileName, builder.ToString());

		string CreateMethod(string type, string memberName, bool nullable)
		{
			string result = $@"
	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName}
	/// </summary>
	/// <param name=""value"">The {type} which should equal {memberName}</param>
	public static IQueryable<{dbSetClass.Name}> By{memberName}(this IQueryable<{dbSetClass.Name}> query, {type}? value)
	{{
		if (query == null)
			return query;

		if (value == null)
			return query;

		return query.Where(n => n.{memberName} == value);
	}}";

			if (nullable)
			{
				result += CreateNullMethodFunctions(memberName);
			}

			return result;
		}

		string CreateNullMethodFunctions(string memberName)
		{
			return $@"
	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName} is null
	/// </summary>
	public static IQueryable<{dbSetClass.Name}> By{memberName}IsNull(this IQueryable<{dbSetClass.Name}> query)
	{{
		if (query == null)
			return query;

		return query.Where(n => n.{memberName} == null);
	}}

	/// <summary>
	/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName} is not null
	/// </summary>
	public static IQueryable<{dbSetClass.Name}> By{memberName}IsNotNull(this IQueryable<{dbSetClass.Name}> query)
	{{
		if (query == null)
			return query;

		return query.Where(n => n.{memberName} != null);
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
