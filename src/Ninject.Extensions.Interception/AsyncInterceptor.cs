// -------------------------------------------------------------------------------------------------
// <copyright file="AsyncInterceptor.cs" company="Ninject Project Contributors">
//   Copyright (c) 2007-2010, Enkari, Ltd.
//   Copyright (c) 2010-2017, Ninject Project Contributors
//   Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// </copyright>
// -------------------------------------------------------------------------------------------------

#if !NET_35 && !SILVERLIGHT
namespace Ninject.Extensions.Interception
{
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// A simple definition of an interceptor, which can take action both before and after
    /// the invocation proceeds and supports async methods.
    /// </summary>
    public abstract class AsyncInterceptor : IInterceptor
    {
        private static MethodInfo startTaskMethodInfo = typeof(AsyncInterceptor).GetMethod("InterceptTaskWithResult", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation to intercept.</param>
        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Request.Method.ReturnType;
            if (returnType == typeof(Task))
            {
                this.InterceptTask(invocation);
                return;
            }

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultType = returnType.GetGenericArguments()[0];
                var mi = startTaskMethodInfo.MakeGenericMethod(resultType);
                mi.Invoke(this, new object[] { invocation });
                return;
            }

            this.BeforeInvoke(invocation);
            invocation.Proceed();
            this.AfterInvoke(invocation);
        }

        /// <summary>
        /// Takes some action before the invocation proceeds.
        /// </summary>
        /// <param name="invocation">The invocation that is being intercepted.</param>
        protected virtual void BeforeInvoke(IInvocation invocation)
        {
        }

        /// <summary>
        /// Takes some action after the invocation proceeds.
        /// </summary>
        /// <remarks>Use one AfterInvoke method overload.</remarks>
        /// <param name="invocation">The invocation that is being intercepted.</param>
        protected virtual void AfterInvoke(IInvocation invocation)
        {
        }

        /// <summary>
        /// Takes some action after the invocation proceeds.
        /// </summary>
        /// <remarks>Use one AfterInvoke method overload.</remarks>
        /// <param name="invocation">The invocation that is being intercepted.</param>
        /// <param name="task">The task that was executed.</param>
        protected virtual void AfterInvoke(IInvocation invocation, Task task)
        {
        }

        private void InterceptTask(IInvocation invocation)
        {
            var invocationClone = invocation.Clone();
            invocation.ReturnValue = Task.Factory
                .StartNew(() => this.BeforeInvoke(invocation))
                .ContinueWith(t =>
                    {
                        invocationClone.Proceed();
                        return invocationClone.ReturnValue as Task;
                    }).Unwrap()
                .ContinueWith(t =>
                            {
                                this.AfterInvoke(invocation);
                                this.AfterInvoke(invocation, t);
                            });
        }

        private void InterceptTaskWithResult<TResult>(IInvocation invocation)
        {
            var invocationClone = invocation.Clone();
            invocation.ReturnValue = Task.Factory
                .StartNew(() => this.BeforeInvoke(invocation))
                .ContinueWith(t =>
                    {
                        invocationClone.Proceed();
                        return invocationClone.ReturnValue as Task<TResult>;
                    }).Unwrap()
                .ContinueWith(t =>
                        {
                            invocationClone.ReturnValue = t.Result;
                            this.AfterInvoke(invocationClone);
                            this.AfterInvoke(invocationClone, t);
                            return (TResult)invocationClone.ReturnValue;
                        });
        }
    }
}
#endif