using System;
using EnsureThat;
using Microsoft.Extensions.Configuration;

namespace ConsoleApp
{
    public static class Extensions
    {
        public static TOptions GetOptions<TOptions>(
            this IConfiguration configuration,
            string sectionName)
            where TOptions : class
        {
            EnsureArg.IsNotNull<IConfiguration>(configuration, nameof (configuration), (OptsFn) null);
            EnsureArg.IsNotNullOrWhiteSpace(sectionName, nameof (sectionName), (OptsFn) null);
            IConfigurationSection section = configuration.GetSection(sectionName);
            if (section == null)
                throw new Exception("Configuration section " + sectionName + " is not found.");
            TOptions options = section.Get<TOptions>((Action<BinderOptions>) (binderOptions => binderOptions.BindNonPublicProperties = true));
            if ((object) options != null)
                return options;
            throw new Exception("Configuration section " + sectionName + " does not contain options of the " + typeof (TOptions).FullName + " type.");
        }
    }
}