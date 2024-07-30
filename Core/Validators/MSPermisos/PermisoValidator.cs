using Core.DTOs.MSPermisos;
using FluentValidation;

namespace Core.Validators.MSPermisos
{
    public class PermisoValidator : AbstractValidator<PermisoRequestDTO>
    {
        public PermisoValidator()
        {
            RuleFor(x => x.FuncionalidadId).NotEmpty().WithMessage("FuncionalidadId is required");
            RuleFor(x => x.RoleId).NotEmpty().WithMessage("RoleId is required");
            RuleFor(x => x.CanView).NotEmpty().WithMessage("Can View is required");
            RuleFor(x => x.CanAdd).NotEmpty().WithMessage("Can Add is required");
            RuleFor(x => x.CanEdit).NotEmpty().WithMessage("Can Edit is required");
            RuleFor(x => x.CanDele).NotEmpty().WithMessage("Can Delete is required");
        }
    }
}
