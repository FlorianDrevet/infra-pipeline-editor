using ErrorOr;
using MediatR;

namespace BicepGenerator.Application.Commands.GenerateBicep;

public class GenerateBicepCommandHandler()
    : IRequestHandler<GenerateBicepCommand, ErrorOr<Uri>>
{
    public async Task<ErrorOr<Uri>> Handle(GenerateBicepCommand command, CancellationToken cancellationToken)
    {
        return new Uri("https://example.com/bicepfile.bicep");
    }
}