﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;

namespace Moq.Proxy
{
    /// <summary>
    /// Default implementation of <see cref="IMethodInvocation"/>.
    /// </summary>
    public class MethodInvocation : IEquatable<MethodInvocation>, IMethodInvocation
    {
        Lazy<IStructuralEquatable> equatable;

        public MethodInvocation(object target, MethodBase method, params object[] arguments)
        {
            // TODO: validate that arguments length and type match the method info?
            Target = target;
            MethodBase = method;
            Arguments = new ArgumentCollection(arguments, method.GetParameters());
            Context = new Dictionary<string, object>();
            equatable = new Lazy<IStructuralEquatable>(() => Tuple.Create(target, method, arguments));
        }

        public IArgumentCollection Arguments { get; }

        public IDictionary<string, object> Context { get; }

        public MethodBase MethodBase { get; }

        public object Target { get; }

        public IMethodReturn CreateExceptionReturn(Exception exception) => new MethodReturn(this, exception);

        public IMethodReturn CreateValueReturn(object returnValue, params object[] allArguments) => new MethodReturn(this, returnValue, allArguments);

        public override string ToString()
        {
            var result = new StringBuilder();
            if (MethodBase is MethodInfo info)
            {
                if (info.ReturnType != typeof(void))
                    result.Append(Stringly.ToTypeName(info.ReturnType)).Append(" ");
                else
                    result.Append("void ");
            }

            result.Append(MethodBase.Name);
            if (MethodBase.IsGenericMethod)
            {
                var generic = ((MethodInfo)MethodBase).GetGenericMethodDefinition();
                result
                    .Append("<")
                    .Append(string.Join(", ", generic.GetGenericArguments().Select(t => t.Name)))
                    .Append(">");
            }

            result
                .Append("(")
                .Append(Arguments.ToString())
                .Append(")");

            return result.ToString();
        }

        #region Equality

        public bool Equals(MethodInvocation other) => equatable.Value.Equals(other?.equatable?.Value, EqualityComparer<object>.Default);

        public bool Equals(object other, IEqualityComparer comparer) => equatable.Value.Equals((other as MethodInvocation)?.equatable?.Value, comparer);

        public int GetHashCode(IEqualityComparer comparer) => equatable.Value.GetHashCode(comparer);

        public override bool Equals(object obj) => Equals(obj as MethodInvocation);

        public override int GetHashCode() => equatable.Value.GetHashCode();

        #endregion
    }
}