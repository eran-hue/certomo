namespace ApiGateway.Features.Signals.SubmitSignal;

using MediatR;
using System;

public record SubmitSignalCommand(string SignalData) : IRequest<Guid>;
