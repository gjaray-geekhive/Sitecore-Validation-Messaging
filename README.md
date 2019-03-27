# Known ﻿Supported Sitecore Versions

8, 9

# Installation Instructions

Place zzz.DeterminePublishAction.config into App_config/Include

Place SitecoreValidationMessaging.dll in the bin folder`

# Customization Options

By default the Required validator is selected but this can be extended by updating the `PublishBlockingValidators` in `zzz.DeterminePublishAction.config`. This is a `|` separated list. For example, having Required and Minimum Length 8 will be formatted as

~~~~xml
<setting name="PublishBlockingValidators" value="{59D4EE10-627C-4FD3-A964-61A88B092CBC}|{F42F3E57-5A4B-49EF-A581-A60CEDC71305}"/>
~~~~

# Known Issues

For sitecore 9.0.X the sitecore support patch public reference number 221523 is required for this to function properly for Required fields. The bug solved by this patch
is resolved in sitecore 9.1
https://sitecore.stackexchange.com/questions/13191/standard-required-field-validation-reporting-field-is-empty-after-upgrade-to-sit for reference