using Esprima;
using Esprima.Ast;
using Jint.Runtime.Interpreter.Statements;

namespace Jint.Runtime.Interpreter
{
    internal sealed class JintFunctionDefinition
    {
        internal readonly IFunction _function;
        internal readonly string _name;
        internal readonly bool _strict;
        internal readonly string[] _parameterNames;
        internal readonly JintStatement _body;
        internal bool _hasRestParameter;

        public readonly HoistingScope _hoistingScope;

        public JintFunctionDefinition(Engine engine, IFunction function)
        {
            _function = function;
            _hoistingScope = function.HoistingScope;
            _name = !string.IsNullOrEmpty(function.Id?.Name) ? function.Id.Name : null;
            _strict = function.Strict;
            _parameterNames = GetParameterNames(function);

            Statement bodyStatement;
            if (function.Expression)
            {
                bodyStatement = new ReturnStatement((Expression) function.Body);
            }
            else
            {
                bodyStatement = (BlockStatement) function.Body;
            }

            _body = JintStatement.Build(engine, bodyStatement);
        }

        private string[] GetParameterNames(IFunction functionDeclaration)
        {
            var list = functionDeclaration.Params;
            var count = list.Count;

            if (count == 0)
            {
                return System.ArrayExt.Empty<string>();
            }

            var names = new string[count];
            for (var i = 0; i < count; ++i)
            {
                var node = list[i];
                if (node is Identifier identifier)
                {
                    names[i] = identifier.Name;
                }
                else if (node is AssignmentPattern ap)
                {
                    names[i] = ((Identifier) ap.Left).Name;
                }
                else if (node is RestElement re)
                {
                    if (re.Argument is Identifier id)
                    {
                        names[i] = id.Name;
                    }
                    else
                    {
                        names[i] = "";
                    }
                    _hasRestParameter = true;
                }
                else if (node is BindingPattern)
                {
                    names[i] = "";
                }
                else
                {
                    ExceptionHelper.ThrowArgumentOutOfRangeException(
                        nameof(functionDeclaration),
                        "Unable to determine how to handle parameter of type " + node.GetType());
                }
            }

            return names;
        }

    }
}