using System;

namespace Konbini.RfidFridge.Test
{
    using System.ComponentModel;
    using Konbini.RfidFridge.Common;
    using Konbini.RfidFridge.Data.Core;
    using Konbini.RfidFridge.Domain.Entities;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class Program
    {


        static void Main(string[] args)
        {

            var settings = Assembly
                .GetAssembly(typeof(RfidFridgeSetting))
                .GetTypes();


            foreach (var setting in settings)
            {
                var properties = setting.GetProperties();
                if (properties.Length > 0)
                {

                    var settingKey = setting.FullName.Replace(setting.Namespace, string.Empty);
                    settingKey = settingKey.Substring(1, settingKey.Length - 1).Replace("+", ".");

                    foreach (var propertyInfo in properties)
                    {
                       //var dataa =  propertyInfo.GetValue(setting);
                        var key = $"{settingKey}.{propertyInfo.Name}";
                        var defaultValueAttributes = propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                        var defaultValue = string.Empty;
                        if (defaultValueAttributes.Length > 0)
                        {
                            defaultValue = Convert.ToString(((DefaultValueAttribute)defaultValueAttributes[0]).Value);
                        }
                        else
                        {
                            defaultValue = $"[{propertyInfo.Name}]";
                        }

                        Console.WriteLine($"{key} = {defaultValue}");

                        if (key == "RfidFridgeSetting.System.Comport.Inventory")
                        {
                            propertyInfo.SetValue(setting, "HIHI");
                            Console.WriteLine($"AFTER SET: { RfidFridgeSetting.System.Comport.Inventory}");
                        }

                    }
                }

            }
            Console.ReadLine();
        }
    }
}
