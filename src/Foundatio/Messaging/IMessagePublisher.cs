﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundatio.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(Type messageType, object message, MessageOptions options = null, CancellationToken cancellationToken = default);

    Task PublishAsync<T>(T message, MessageOptions options = null, CancellationToken cancellationToken = default) where T : class;
}

public static class MessagePublisherExtensions
{
    public static Task PublishAsync<T>(this IMessagePublisher publisher, T message, TimeSpan delay, CancellationToken cancellationToken = default) where T : class
    {
        return publisher.PublishAsync(typeof(T), message, new MessageOptions { DeliveryDelay = delay }, cancellationToken);
    }
}
