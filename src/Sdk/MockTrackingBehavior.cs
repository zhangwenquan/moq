﻿using System;
using Moq.Proxy;
using Moq.Sdk.Properties;

namespace Moq.Sdk
{
    public class MockTrackingBehavior : IProxyBehavior
    {
        public bool AppliesTo(IMethodInvocation invocation) => true;

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            // Allows subsequent extension methods on the fluent API to retrieve the 
            // current invocation being performed.
            CallContext<IMethodInvocation>.SetData(invocation);

            // Determines the current setup according to contextual 
            // matchers that may have been pushed to the MockSetup context. 
            // Allows subsequent extension methods on the fluent API to retrieve the 
            // current setup being performed.
            var setup = MockSetup.Freeze(invocation);
            CallContext<IMockSetup>.SetData(setup);

            var mock = invocation.Target is IMocked mocked ?
                mocked.Mock : throw new ArgumentException(Resources.TargetNotMocked);

            mock.Invocations.Add(invocation);
            if (mock is MockInfo info)
                info.LastSetup = setup;

            return getNext().Invoke(invocation, getNext);
        }
    }
}
