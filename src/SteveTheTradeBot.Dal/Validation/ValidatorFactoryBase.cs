using System.Reflection;
using Bumbershoot.Utilities.Helpers;
using FluentValidation;
using FluentValidation.Results;

namespace SteveTheTradeBot.Dal.Validation
{
    public abstract class ValidatorFactoryBase : IValidatorFactory
    {
        #region IValidatorFactory Members

        public ValidationResult For<T>(T user)
        {
            var validationRules = GetValidationRules<T>();
            return validationRules != null ? validationRules.Validate(user) : new ValidationResult();
        }

        public void ValidateAndThrow<T>(T user)
        {
            var validationRules = GetValidationRules<T>();
            validationRules?.ValidateAndThrow(user);
        }

        public IValidator<T> Validator<T>()
        {
            return GetValidationRules<T>();
        }

        #endregion

        protected IValidator<T> GetValidationRules<T>()
        {
            TryResolve(out IValidator<T> output);
            return output;
        }

        protected abstract void TryResolve<T>(out IValidator<T> output);
    }

   
}