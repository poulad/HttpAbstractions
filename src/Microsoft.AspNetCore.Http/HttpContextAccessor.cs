// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.AspNetCore.Http
{
    public class HttpContextAccessor : IHttpContextAccessor
    {
        private static AsyncLocal<(string traceIdentifier, HttpContext context)> _httpContextCurrent = new AsyncLocal<(string traceIdentifier, HttpContext context)>();

        public HttpContext HttpContext
        {
            get
            {
                var value = _httpContextCurrent.Value;
                // Only return the context if the stored request id matches the stored trace identifier
                return value.traceIdentifier == value.context?.TraceIdentifier ? value.context : null;
            }
            set
            {
                var existing = _httpContextCurrent.Value;

                if (value != null)
                {
                    // Store the request id and the HttpContext
                    _httpContextCurrent.Value = (value.TraceIdentifier, value);
                }
                else
                {
                    // Setting the context to null means the request is over
                    existing.context.TraceIdentifier = null;
                    existing.context = null;
                    existing.traceIdentifier = null;
                }
            }
        }
    }
}
