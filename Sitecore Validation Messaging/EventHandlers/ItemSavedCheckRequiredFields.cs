using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitecoreValidationMessaging.Helpers;
using Sitecore.Data.Items;
using Sitecore.Events;

namespace SitecoreValidationMessaging.EventHandlers
{
    public class ItemSavedCheckRequiredFields
    {
        protected void OnItemSaved(object sender, EventArgs args)
        {
            var savedItem = Event.ExtractParameter(args, 0) as Item;
            if (savedItem == null)
            {
                return;
            }
            var validators = ValidationHelpers.GetValidatorCollection(savedItem);
            if (validators == null)
            {
                return;
            }
            var invalidFields = ValidationHelpers.AreFieldsValid(validators).Select(i => savedItem.Database.GetItem(i).Name).ToList();
            if (invalidFields.Any() && Sitecore.Context.Items[savedItem.ID.ToString() + savedItem.Language] == null)
            {
                Sitecore.Context.Items[savedItem.ID.ToString() + savedItem.Language] = true;
                try
                {
                    if (Sitecore.Context.ClientPage != null && Sitecore.Context.ClientPage.ClientResponse != null)
                    {
                        Sitecore.Context.ClientPage.ClientResponse.Alert(
                            $"{savedItem.DisplayName ?? savedItem.Name} has content entry errors in {savedItem.Language} on the following fields:" +
                            $" {Environment.NewLine}{string.Join(Environment.NewLine, invalidFields)}{Environment.NewLine}" +
                            "It will not be published in that language", "", "Publishing Blocked");
                    }

                }
                catch (NullReferenceException nullref)
                {
                    Sitecore.Diagnostics.Error.LogError("Context of OnItemSaved is incorrect, stacktrace is " + Environment.NewLine + nullref.StackTrace);
                }
            }

        }
    }
}