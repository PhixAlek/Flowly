using MediatR;
namespace PersonalFinance.Domain.Common.Primitives;
/// <summary>Domain events are also MediatR notifications for in-process dispatching.</summary>
public interface IDomainEvent : INotification;
