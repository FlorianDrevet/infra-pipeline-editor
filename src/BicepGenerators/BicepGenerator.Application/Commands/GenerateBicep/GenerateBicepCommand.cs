using ErrorOr;
using MediatR;

namespace BicepGenerator.Application.Commands.GenerateBicep;

public record GenerateBicepCommand(
) : IRequest<ErrorOr<Uri>>;