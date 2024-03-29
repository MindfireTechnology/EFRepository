using System.Diagnostics;
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

			return new SourceRecord(sourceModel, sourceSymbol, c);
		}).Where(c => c != null)
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

	protected IEnumerable<(string FileName, string Content)> ProcessDbContext(SourceRecord? record, INamedTypeSymbol dbSetSymbol, Compilation compilation)
	{
		if (record != null)
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

			foreach (var dbSet in dbSets)
			{
				var genericType = ((INamedTypeSymbol)dbSet.Type).TypeArguments[0];

				if (genericType == null)
					continue;

				var sourceCode = BuildExtensionMethod((INamedTypeSymbol)genericType, compilation);

				yield return sourceCode;
			}
		}
	}

	protected (string FileName, string Content) BuildExtensionMethod(INamedTypeSymbol dbSetClass, Compilation compilation)
	{
		string fileName = $"{dbSetClass.Name}.EFRepoExtensions.g.cs";

		var builder = new StringBuilder(
$@"// <auto-generated> This file has been auto generated on {DateTime.Now.ToShortDateString()} at {DateTime.Now.ToLongTimeString()} </auto-generated>
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace {dbSetClass.ContainingNamespace}
{{

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
		var dateTimeOffsetSymbol = compilation.GetTypeByMetadataName("System.DateTimeOffset");
		var dateOnlySymbol = compilation.GetTypeByMetadataName("System.DateOnly");
		var timeOnlySymbol = compilation.GetTypeByMetadataName("System.TimeOnly");

		var valueTypes = new[] { boolSymbol, byteSymbol, shortSymbol, intSymbol, longSymbol,
			floatSymbol, doubleSymbol, decimalSymbol, dateTimeSymbol, dateTimeOffsetSymbol,
			dateOnlySymbol, timeOnlySymbol };

		var members = dbSetClass.GetMembers()
		.Where((m) => !m.IsStatic && !m.IsAbstract && !m.IsImplicitlyDeclared && m.IsDefinition
					&& m.Kind is SymbolKind.Property or SymbolKind.Field);

		foreach (var member in members)
		{
			ITypeSymbol type;
			string typeString;
			bool nullable;

			if (member.Kind is SymbolKind.Property)
			{
				var property = (IPropertySymbol)member;
				type = property.Type;
				typeString = property.Type.ToDisplayString();
				nullable = property.NullableAnnotation == NullableAnnotation.Annotated;
			}
			else
			{
				var field = (IFieldSymbol)member;
				type = field.Type;
				typeString = field.Type.ToDisplayString();
				nullable = field.NullableAnnotation == NullableAnnotation.Annotated;
			}

			if (typeString.EndsWith("?") && typeString != "string?")
			{
				typeString = typeString.TrimEnd('?');
				var mappedType = valueTypes.SingleOrDefault(n => n != null && n.ToDisplayString() == typeString);

				if (mappedType == null)
				{
					Trace.Write($"Unable to find mapping from type {typeString}? to a non-nullable type");
					continue;
				}
				else
					type = mappedType;
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
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name} is null or whitespace
		/// </summary>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{member.Name}IsNullOrWhiteSpace(this IQueryable<{dbSetClass.Name}> query)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return query.Where(n => string.IsNullOrWhiteSpace(n.{member.Name}));
		}}

		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name} is not null or whitespace
		/// </summary>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{member.Name}IsNotNullOrWhiteSpace(this IQueryable<{dbSetClass.Name}> query)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return query.Where(n => !string.IsNullOrWhiteSpace(n.{member.Name}));
		}}");

				builder.AppendLine($@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name}
		/// </summary>
		/// <param name=""value"">The string which {member.Name} should be equal</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{member.Name}(this IQueryable<{dbSetClass.Name}> query, string? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.{member.Name} == value);
		}}");

				builder.AppendLine($@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name} contains a value
		/// </summary>
		/// <param name=""value"">The string which {member.Name} should contain</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{member.Name}Contains(this IQueryable<{dbSetClass.Name}> query, string? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.{member.Name} != null && n.{member.Name}.Contains(value));
		}}");

				builder.AppendLine($@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name} starts with a value
		/// </summary>
		/// <param name=""value"">The string which {member.Name} should start with</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{member.Name}StartsWith(this IQueryable<{dbSetClass.Name}> query, string? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.{member.Name} != null && n.{member.Name}.StartsWith(value));
		}}");

				builder.AppendLine($@"

		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {member.Name} ends with a value
		/// </summary>
		/// <param name=""value"">The string which {member.Name} should end with</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{member.Name}EndsWith(this IQueryable<{dbSetClass.Name}> query, string? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (string.IsNullOrWhiteSpace(value))
				return query;

			return query.Where(n => n.{member.Name} != null && n.{member.Name}.EndsWith(value));
		}}");
			}
			else if (type.Equals(dateTimeSymbol, SymbolEqualityComparer.Default) ||
				type.Equals(dateTimeOffsetSymbol, SymbolEqualityComparer.Default) ||
				type.Equals(dateOnlySymbol, SymbolEqualityComparer.Default) ||
				type.Equals(timeOnlySymbol, SymbolEqualityComparer.Default))
			{
				string dateType;

				if (type.Equals(dateTimeSymbol, SymbolEqualityComparer.Default))
					dateType = "DateTime";
				else if (type.Equals(dateTimeOffsetSymbol, SymbolEqualityComparer.Default))
					dateType = "DateTimeOffset";
				else if (type.Equals(dateOnlySymbol, SymbolEqualityComparer.Default))
					dateType = "DateOnly";
				else if (type.Equals(timeOnlySymbol, SymbolEqualityComparer.Default))
					dateType = "TimeOnly";
				else
					continue;

				// By (is)
				builder.AppendLine(CreateMethod(dateType, member.Name, nullable));

				// IsBefore
				builder.AppendLine($@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not the provided <see cref=""{dateType}"" /> is after {member.Name}
		/// </summary>
		/// <param name=""value"">The <see cref=""{dateType}""/> that {member.Name} should be before</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass}> By{member.Name}IsBefore(this IQueryable<{dbSetClass.Name}> query, {dateType}? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (value == null)
				return query;

			return query.Where(n => n.{member.Name} < value);
		}}");

				// IsAfter
				builder.AppendLine($@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not the provided <see cref=""{dateType}"" /> is after {member.Name}
		/// </summary>
		/// <param name=""value"">The <see cref=""{dateType}""/> that {member.Name} should be after</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass}> By{member.Name}IsAfter(this IQueryable<{dbSetClass.Name}> query, {dateType}? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (value == null)
				return query;

			return query.Where(n => n.{member.Name} > value);
		}}");

				// Between
				builder.AppendLine($@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not {member.Name} is between the two provided values.
		/// </summary>
		/// <param name=""start"">The <see cref=""{dateType}""/> that should be before {member.Name}</param>
		/// <param name=""end"">The <see cref=""{dateType}""/> that should be after {member.Name}</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass}> By{member.Name}Between(this IQueryable<{dbSetClass.Name}> query, {dateType}? start, {dateType}? end)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (start != null)
				query = query.Where(n => n.{member.Name} > start);

			if (end != null)
				query = query.Where(n => n.{member.Name} < end);

			return query;
		}}");

				// OnDate (Skip for DateOnly and TimeOnly types)
				if (dateType != "DateOnly" && dateType != "TimeOnly")
					builder.AppendLine($@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by whether or not {member.Name} is between the two provided values.
		/// </summary>
		/// <param name=""value"">The <see cref=""{dateType}""/> that should the same date as {member.Name}, excluding time</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass}> By{member.Name}OnDate(this IQueryable<{dbSetClass.Name}> query, {dateType}? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (value != null)
				return query.Where(n => n.{member.Name}{(nullable ? ".GetValueOrDefault()" : "")}.Date == value.Value.Date);
			else
				return query;
		}}");
			}
		}

		// Close Class & Namespace
		builder.AppendLine("	}")
			.AppendLine("}");

		return (fileName, builder.ToString());

		string CreateMethod(string type, string memberName, bool nullable)
		{
			StringBuilder result = new();
			result.AppendLine($@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName}
		/// </summary>
		/// <param name=""value"">The {type} which should equal {memberName}</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{memberName}(this IQueryable<{dbSetClass.Name}> query, {type}? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (value == null)
				return query;

			return query.Where(n => n.{memberName} == value);
		}}");

			// This is for numeric types only
			if (!type.Contains("bool") && !type.Contains("DateTime") && !type.Contains("string"))
			{
				result.AppendLine($@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName} greater than value
		/// </summary>
		/// <param name=""value"">The {type} which should be greater than {memberName}</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{memberName}GreaterThan(this IQueryable<{dbSetClass.Name}> query, {type}? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (value == null)
				return query;

			return query.Where(n => n.{memberName} > value);
		}}

		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName} greater than or equal value
		/// </summary>
		/// <param name=""value"">The {type} which should be greater than {memberName}</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{memberName}GreaterThanOrEqual(this IQueryable<{dbSetClass.Name}> query, {type}? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (value == null)
				return query;

			return query.Where(n => n.{memberName} >= value);
		}}

		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName} less than value
		/// </summary>
		/// <param name=""value"">The {type} which should be less than {memberName}</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{memberName}LessThan(this IQueryable<{dbSetClass.Name}> query, {type}? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (value == null)
				return query;

			return query.Where(n => n.{memberName} < value);
		}}

		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName} less than or qual to value
		/// </summary>
		/// <param name=""value"">The {type} which should be less than or equal to {memberName}</param>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{memberName}LessThanOrEqual(this IQueryable<{dbSetClass.Name}> query, {type}? value)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			if (value == null)
				return query;

			return query.Where(n => n.{memberName} <= value);
		}}");
			}

			if (nullable)
			{
				result.AppendLine(CreateNullMethodFunctions(memberName));
			}

			return result.ToString();
		}

		string CreateNullMethodFunctions(string memberName)
		{
			return $@"
		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName} is null
		/// </summary>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{memberName}IsNull(this IQueryable<{dbSetClass.Name}> query)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return query.Where(n => n.{memberName} == null);
		}}

		/// <summary>
		/// Filter the <see cref=""IQueryable""/> of {dbSetClass.Name} by {memberName} is not null
		/// </summary>
		/// <exception cref=""ArgumentNullException"">thows if query is null</exception>
		public static IQueryable<{dbSetClass.Name}> By{memberName}IsNotNull(this IQueryable<{dbSetClass.Name}> query)
		{{
			if (query == null) throw new ArgumentNullException(nameof(query));

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

	public SourceRecord(SemanticModel model, INamedTypeSymbol symbol, TypeDeclarationSyntax syntax)
	{
		Model = model;
		Symbol = symbol;
		Syntax = syntax;
	}
}
