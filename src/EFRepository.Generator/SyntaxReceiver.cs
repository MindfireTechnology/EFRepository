namespace EFRepository.Generator;

public class SyntaxReceiver : ISyntaxReceiver
{
	public IEnumerable<TypeDeclarationSyntax> Classes => ClassList;
	protected HashSet<TypeDeclarationSyntax> ClassList { get; } = new HashSet<TypeDeclarationSyntax>();

	public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
	{
		if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
		{
			ClassList.Add(typeDeclarationSyntax);
		}
	}
}
