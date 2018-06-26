// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Xunit;

namespace Microsoft.AspNetCore.Http
{
    public class HttpContextAccessorTests
    {
        [Fact]
        public async Task HttpContextAccessor_GettingHttpContextReturnsHttpContext()
        {
            var accessor = new HttpContextAccessor();

            var context = new DefaultHttpContext();
            context.TraceIdentifier = "1";
            accessor.HttpContext = context;

            await Task.Delay(100);

            Assert.Same(context, accessor.HttpContext);
        }

        [Fact]
        public async Task HttpContextAccessor_GettingHttpContextReturnsNullHttpContextIfSetToNull()
        {
            var accessor = new HttpContextAccessor();

            var context = new DefaultHttpContext();
            context.TraceIdentifier = "1";
            accessor.HttpContext = context;

            var checkAsyncFlowTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var waitForNullTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var afterNullCheckTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            ThreadPool.QueueUserWorkItem(async _ =>
            {
                // The HttpContext flows with the execution context
                Assert.Same(context, accessor.HttpContext);

                checkAsyncFlowTcs.SetResult(null);

                await waitForNullTcs.Task;

                try
                {
                    Assert.Null(accessor.HttpContext);

                    afterNullCheckTcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    afterNullCheckTcs.SetException(ex);
                }
            });

            await checkAsyncFlowTcs.Task;

            // Null out the accessor
            accessor.HttpContext = null;

            waitForNullTcs.SetResult(null);

            Assert.Null(accessor.HttpContext);

            await afterNullCheckTcs.Task;
        }
    }
}