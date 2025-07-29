using System.Data;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Users;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedCore;

namespace Application.Features.Users;

public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string Password):IRequest<Result<Guid>>;

internal sealed class RegisterUserCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher) :
    IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        if (await context.Users.AnyAsync(u => u.Email == command.Email, ct))
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PasswordHash = passwordHasher.Hash(command.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(ct);
        return user.Id;
    }
}

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
    }
}
