namespace Scriptcs.Wrappers
{
    using Roslyn.Scripting;
    using Roslyn.Scripting.CSharp;

    internal class ScriptEngineWrapper : IScriptEngine
    {
        private ScriptEngine scriptEngine;

        public ScriptEngineWrapper()
        {
            this.scriptEngine = new ScriptEngine();
        }

        public void AddReference(string assemblyDisplayNameOrPath)
        {
            this.scriptEngine.AddReference(assemblyDisplayNameOrPath);
        }

        public ISession CreateSession()
        {
            return new SessionWrapper(this.scriptEngine.CreateSession());
        }

        public string BaseDirectory
        {
            get 
            { 
                return this.scriptEngine.BaseDirectory; 
            }

            set 
            {
                this.scriptEngine.BaseDirectory = value;
            }
        }
    }
}