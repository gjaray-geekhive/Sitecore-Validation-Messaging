using System;
using System.Collections.Generic;
using System.Linq;
using SitecoreValidationMessaging.Helpers;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.ExperienceEditor.Abstractions;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.ExperienceEditor.Utils;
using Sitecore.Pipelines.Save;
using Sitecore.Shell.Applications.WebEdit.Commands;

namespace SitecoreValidationMessaging.Speak
{
    public class ValidateFields : Sitecore.ExperienceEditor.Speak.Ribbon.Requests.SaveItem.ValidateFields
    {
        protected IItemFieldValidator ItemFieldValidator { get; }

        public ValidateFields(IItemFieldValidator itemFieldValidator) : base(itemFieldValidator)
        {
            ItemFieldValidator = itemFieldValidator;
        }

        public override PipelineProcessorResponseValue ProcessRequest()
        {
            var processorResponseValue = new PipelineProcessorResponseValue();
            var items = base.RequestContext.GetSaveArgs().Items;
            var list = ((IEnumerable<SaveArgs.SaveItem>)items).SelectMany<SaveArgs.SaveItem, SaveArgs.SaveField>((Func<SaveArgs.SaveItem, IEnumerable<SaveArgs.SaveField>>)(i => (IEnumerable<SaveArgs.SaveField>)i.Fields)).ToList<SaveArgs.SaveField>();
            var obj = this.RequestContext.Item.Database.GetItem(items[0].ID, items[0].Language);
            if (obj == null || obj.Paths.IsMasterPart || StandardValuesManager.IsStandardValuesHolder(obj))
                return processorResponseValue;
            var fields = WebUtility.GetFields(this.RequestContext.Item.Database, this.RequestContext.FieldValues);
            var fieldValidationErrorList = new List<FieldValidationError>();
            foreach (var pageEditorField in fields)
            {
                var field = this.RequestContext.Item.Database.GetItem(pageEditorField.ItemID, pageEditorField.Language).Fields[pageEditorField.FieldID];
                var valueToValidate = list.FirstOrDefault<SaveArgs.SaveField>((Func<SaveArgs.SaveField, bool>)(f => f.ID == field.ID))?.Value ?? field.Value;
                var fieldValidationError = this.ItemFieldValidator.GetFieldTypeValidationError(field, valueToValidate) ?? this.ItemFieldValidator.GetFieldRegexValidationError(field, valueToValidate);
                if (fieldValidationError != null)
                    fieldValidationErrorList.Add(fieldValidationError);
            }

            var validators = ValidationHelpers.GetValidatorCollection(obj);
            if (validators == null)
            {
                return processorResponseValue;
            }
            var validationErrorsCustom =
                ValidationHelpers.AreFieldsValid(validators);
            if (validationErrorsCustom.Any())
            {
                var invalidFields = validationErrorsCustom.Select(i => obj.Database.GetItem(i).Name).ToList();

                processorResponseValue.ConfirmMessage = $"{obj.DisplayName ?? obj.Name} has content entry errors in {obj.Language} on the following fields: {Environment.NewLine}" +
                                                        $"{string.Join(Environment.NewLine, invalidFields)}{Environment.NewLine} " +
                                                        "It will not be published in that language";
            }
            processorResponseValue.Value = (object)fieldValidationErrorList;
            return processorResponseValue;
        }
    }
}