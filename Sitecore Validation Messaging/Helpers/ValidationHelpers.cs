using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Validators;

namespace SitecoreValidationMessaging.Helpers
{
    public static class ValidationHelpers
    {
        public static List<ID> AreFieldsValid(ValidatorCollection validators)
        {
            var returnVal = new List<ID>();
            var publishBlockingValidators =
                Sitecore.Configuration.Settings.GetSetting("PublishBlockingValidators").Split('|');
            foreach (BaseValidator baseValidator in validators)
            {
                if (baseValidator.Result != ValidatorResult.Valid &&
                    publishBlockingValidators.Contains(baseValidator.ValidatorID.ToString()))
                {
                    returnVal.Add(baseValidator.FieldID);
                }
            }

            return returnVal;
        }

        public static ValidatorCollection GetValidatorCollection(Item sourceItem)
        {
            if (!sourceItem.Paths.IsContentItem) return null;

            try
            {
                sourceItem.Fields.ReadAll();
                var itemFieldDescriptors = sourceItem.Fields.Where(x => x != null && !string.IsNullOrEmpty(x.Name))
                    .Select(f => new FieldDescriptor(sourceItem, f.Name));
                // get sourceItem validators
                var validators = ValidatorManager.GetFieldsValidators(
                    ValidatorsMode.ValidateButton, itemFieldDescriptors, sourceItem.Database);
                var options = new ValidatorOptions(false);
                ValidatorManager.Validate(validators, options);
                return validators;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}