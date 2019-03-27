using System.Linq;
using Sitecore.Diagnostics;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.PublishItem;
using SitecoreValidationMessaging.Helpers;

namespace SitecoreValidationMessaging.Pipelines
{
    public class DeterminePublishAction : PublishItemProcessor
    {
        public override void Process(PublishItemContext context)
        {
            if (context == null)
                return;

            if (context.Aborted)
                return;

            var sourceItem = context.PublishHelper.GetSourceItem(context.ItemId);

            if (sourceItem == null)
                return;

            if (!sourceItem.Paths.IsContentItem)
                return;
            sourceItem.Fields.ReadAll();

            var validators = ValidationHelpers.GetValidatorCollection(sourceItem);
            if (validators == null)
            {
                return;
            }
            var invalidFields = ValidationHelpers.AreFieldsValid(validators).Select(i => sourceItem.Database.GetItem(i).Name).ToList();

            if (!invalidFields.Any()) return;
            if (invalidFields.Any())
            {
                Log.Info(string.Format(
                        "{0}: Item '{1}' in '{4}' will not be publised to database '{2}' because '{3}' field values are not valid",
                        GetType().Name,
                        AuditFormatter.FormatItem(sourceItem),
                        context.PublishContext.PublishOptions.TargetDatabase,
                        string.Join(", ", invalidFields), sourceItem.Language.Name),
                    this);
            }

            context.Action = PublishAction.Skip;

        }
    }
}