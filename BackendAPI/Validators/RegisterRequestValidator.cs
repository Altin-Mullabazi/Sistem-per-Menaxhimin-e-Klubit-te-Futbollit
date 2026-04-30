using FluentValidation;
using FootballClubAPI.DTOs;
using FootballClubAPI.Helpers;

namespace FootballClubAPI.Validators
{
    /// <summary>
    /// Fluent validator for user registration requests.
    /// Validates email format, password complexity, name fields, and password confirmation.
    /// </summary>
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Email format is invalid")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters");

            // Password validation
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters")
                .MaximumLength(255)
                .WithMessage("Password cannot exceed 255 characters")
                .Custom((password, context) =>
                {
                    var (isValid, errorMessage) = PasswordValidator.ValidatePassword(password);
                    if (!isValid)
                    {
                        context.AddFailure("Password", errorMessage ?? "Password does not meet complexity requirements");
                    }
                });

            // Confirm password validation
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required")
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match");

            // First name validation
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .Length(2, 100)
                .WithMessage("First name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z\s'-]+$")
                .WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

            // Last name validation
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .Length(2, 100)
                .WithMessage("Last name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z\s'-]+$")
                .WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");
        }
    }
}
