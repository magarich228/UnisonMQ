namespace UnisonMQ.Modules;

[AttributeUsage(AttributeTargets.Class)]
public sealed class UnisonModuleAttribute<T> : Attribute
{
    internal Type ModuleType { get; } = typeof(T);
}