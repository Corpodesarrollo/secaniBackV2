using Core.DTOs;
using FluentValidation;

namespace Core.Validators
{
    public class ContactoEntidadRequestValidator : AbstractValidator<ContactoEntidadRequest>
    {
        public ContactoEntidadRequestValidator()
        {
            RuleFor(x => x.Nombres).NotEmpty().WithMessage("Nombres is required.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email address is required")
                                            .EmailAddress().WithMessage("Valid email is required.");
            RuleFor(x => x.Telefonos).NotEmpty().WithMessage("Telefonos is required.");
        }
    }
}