using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedCore;

namespace Application.Features.Users;

public sealed record LoginUserCommand(string Email, string Password):IRequest<Result<string>>;

internal sealed class LoginUserCommandHandler(
    IAppDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(UserErrors.NotFoundByEmail);
        }

        bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!verified)
        {
            return Result.Failure<string>(UserErrors.NotFoundByEmail);
        }

        string token = tokenProvider.Create(user);

        return token;
    }
}