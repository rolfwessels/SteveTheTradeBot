using FluentValidation;
using FluentValidation.Results;

namespace SteveTheTradeBot.Dal.Validation
{
    public interface IValidatorFactory
    {
        ValidationResult For<T>(T user);
        void ValidateAndThrow<T>(T user);
        IValidator<T> Validator<T>();
    }
}