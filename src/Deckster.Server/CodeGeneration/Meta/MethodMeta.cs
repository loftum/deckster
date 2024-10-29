namespace Deckster.Server.CodeGeneration.Meta;

public class MethodMeta
{
    public string Name { get; init; }
    public List<ParameterMeta> Parameters { get; init; }
    public TypeMeta ReturnType { get; init; }
}