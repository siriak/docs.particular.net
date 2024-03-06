using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Pipeline;

namespace Shared;

public class OutgoingBehavior : Behavior<IOutgoingPhysicalMessageContext>
{
    public override Task Invoke(IOutgoingPhysicalMessageContext context, Func<Task> next)
    {
        var type = context.Headers[Headers.EnclosedMessageTypes];
        // hard coded for demo purposes
        context.Headers[Headers.EnclosedMessageTypes] =
            type.Replace(", Shared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", string.Empty);
        return next();
    }
}