namespace Scriptcs
{
    using System.ComponentModel.Composition;

    using Roslyn.Scripting;

    [InheritedExport]
    public interface IScriptEngine
    {
        void AddReference(string assemblyDisplayNameOrPath);

        ISession CreateSession();

        string BaseDirectory { get; set; }
    }
}
