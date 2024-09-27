using FluentValidation;

namespace Core.Models;

public class Greetings_SayHi_RequestValidator : AbstractValidator<Greetings_SayHi_Request>
{
    public Greetings_SayHi_RequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
        RuleFor(x => x.AgeYears).GreaterThan(0);
    }
}
