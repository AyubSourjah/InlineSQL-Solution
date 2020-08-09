using System;

namespace InLineSQL
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SqlScriptAttribute : System.Attribute
    {
        public SqlScriptAttribute(string scriptNamespace, Type containingAssembly)
        {
            this.ScriptNamespace = scriptNamespace;
            this.ContainingAssembly = containingAssembly;
        }
        public string ScriptNamespace { get; private set; }
        public Type ContainingAssembly { get; private set; }
    }
}
