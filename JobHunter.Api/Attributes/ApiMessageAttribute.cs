namespace JobHunter.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ApiMessageAttribute : Attribute
{
    public ApiMessageAttribute(string value)
    {
        Value = value;
    }

    public string Value { get; }
}
