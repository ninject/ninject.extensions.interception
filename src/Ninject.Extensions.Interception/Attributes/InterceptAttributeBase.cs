﻿// -------------------------------------------------------------------------------------------------
// <copyright file="InterceptAttributeBase.cs" company="Ninject Project Contributors">
//   Copyright (c) 2007-2010, Enkari, Ltd.
//   Copyright (c) 2010-2017, Ninject Project Contributors
//   Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace Ninject.Extensions.Interception.Attributes
{
    using System;
    using Ninject.Extensions.Interception.Request;

    /// <summary>
    /// A baseline definition of an attribute that indicates one or more methods should be intercepted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class InterceptAttributeBase : Attribute
    {
        /// <summary>
        /// Gets or sets the interceptor's order number. Interceptors are invoked in ascending order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Creates the interceptor associated with the attribute.
        /// </summary>
        /// <param name="request">The request that is being intercepted.</param>
        /// <returns>The interceptor.</returns>
        public abstract IInterceptor CreateInterceptor(IProxyRequest request);
    }
}