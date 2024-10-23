using CompanyEmployees.Presentation.Commands;
using FluentValidation;
using FluentValidation.Results;

namespace CompanyEmployees.Presentation.Validators;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(c => c.Company.Name)
            .NotEmpty().WithMessage("Name is required.")
            .NotNull()
            .MaximumLength(60).WithMessage("Name can't be longer than 60 characters.");

        RuleFor(c => c.Company.Address)
            .NotEmpty()
            .MaximumLength(60);
    }
    
    public override ValidationResult 
        Validate(ValidationContext<CreateCompanyCommand> context)
    {
        return context.InstanceToValidate.Company is null
            ? new ValidationResult(new[] { new
                ValidationFailure("CompanyForCreationDto",
                    "CompanyForCreationDto object is null") })
            : base.Validate(context);
    }
}