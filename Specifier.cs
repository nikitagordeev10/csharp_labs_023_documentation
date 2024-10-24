using System;
using System.Linq;
using System.Reflection;

namespace Documentation {
    public class Specifier<T> : ISpecifier { // класс, реализующий интерфейс ISpecifier для получения информации о документации API c класса T
        Type type = typeof(T); // одержит информацию о типе T

        /************************************** описание API **************************************/
        public string GetApiDescription() { // метод для получения описания API 
            var attribute = type.GetCustomAttributes(true).OfType<ApiDescriptionAttribute>().FirstOrDefault(); // получение атрибута ApiDescriptionAttribute для типа, он есть
            if (attribute == null) // атрибут не найден
                return null; // возвращаем null

            return attribute.Description; // возвращаем описание API 
        }

        /************************************** имена методов API **************************************/
        public string[] GetApiMethodNames() { // метод для получения имен методов API
            var methods = type.GetMethods(); // получение всех публичных методов типа
            var apiMethods = methods.Where(met => met.IsPublic && met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()); // выбор только тех методов, которые имеют атрибут ApiMethodAttribute
            return apiMethods.Select(met => met.Name).ToArray(); // получение имен методов API и возвращение их в виде массива
        }

        /************************************** описание метода API **************************************/
        public string GetApiMethodDescription(string methodName) { // метод для получения описания метода API
            var met = type.GetMethod(methodName); // получение метода по имени
            if (met == null || !met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()) // метод не найден или не имеет атрибута ApiMethodAttribute
                return null; // возвращаем null

            var attribute = met.GetCustomAttributes(true).OfType<ApiDescriptionAttribute>().FirstOrDefault(); // получение атрибута ApiDescriptionAttribute для метода
            if (attribute == null) // атрибут не найден
                return null; // возвращаем null

            return attribute.Description; // возвращаем описание метода
        }

        /************************************** имена параметров метода API **************************************/
        public string[] GetApiMethodParamNames(string methodName) { // метод для получения имен параметров метода API 
            var met = type.GetMethod(methodName); // получение метода по имени
            if (met == null || !met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()) // метод не найден или не имеет атрибута ApiMethodAttribute
                return null; // возвращаем null

            return met.GetParameters().Select(param => param.Name).ToArray();  // получение имен параметров метода и возвращение их в виде массива
        }

        /************************************** описание параметров метода API **************************************/
        public string GetApiMethodParamDescription(string methodName, string paramName) { // метод для получения описания параметра метода API
            var met = type.GetMethod(methodName); // получение метода по имени
            if (met == null || !met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()) // метод не найден или не имеет атрибута ApiMethodAttribute
                return null; // возвращаем null

            var parameter = met.GetParameters().Where(param => param.Name == paramName); // получение параметра метода по имени
            if (!parameter.Any()) // параметр не найден
                return null; // возвращаем null

            var attribute = parameter.First().GetCustomAttributes(true).OfType<ApiDescriptionAttribute>().FirstOrDefault(); // получение атрибута ApiDescriptionAttribute для параметра, он есть
            if (attribute == null) // атрибут не найден
                return null; // возвращаем null

            return attribute.Description; // возвращаем описание параметра 
        }

        /************************************** полное описание параметров метода API **************************************/
        public ApiParamDescription GetApiMethodParamFullDescription(string methodName, string paramName) { // метод для получения полного описания параметра API-метода

            var result = new ApiParamDescription { ParamDescription = new CommonDescription(paramName) }; // объект для хранения результата

            var met = type.GetMethod(methodName); // получаем метод по его названию
            if (met == null) { // метод не найден
                return result; // возвращаем  результат 
            }

            var apiMethodAttr = met.GetCustomAttribute<ApiMethodAttribute>(); // получаем атрибут, указывающий на то, что метод является методом API
            if (apiMethodAttr == null) { // атрибут не найден
                return result; // возвращаем результат
            }

            var parameter = met.GetParameters().SingleOrDefault(param => param.Name == paramName); // получаем параметр метода по его названию
            if (parameter == null) { // параметр не найден
                return result; // возвращаем результат  
            }

            var descriptionAttribute = parameter.GetCustomAttribute<ApiDescriptionAttribute>(); // получаем описание параметра
            if (descriptionAttribute != null) { // оно указано в атрибуте
                result.ParamDescription.Description = descriptionAttribute.Description;
            }

            var intValidationAttribute = parameter.GetCustomAttribute<ApiIntValidationAttribute>(); // получаем атрибуты проверки целочисленного параметра
            if (intValidationAttribute != null) { // они указаны?
                result.MinValue = intValidationAttribute.MinValue; // установка минимального значения для параметра
                result.MaxValue = intValidationAttribute.MaxValue; // установка максимального значения для параметра
            }

            var requiredAttribute = parameter.GetCustomAttribute<ApiRequiredAttribute>(); // получаем атрибут, указывающий на то, является ли параметр обязательным.
            if (requiredAttribute != null) {
                result.Required = requiredAttribute.Required; // установка значения обязательности
            }

            return result; // возвращаем результат с полным описанием параметра
        }

        /************************************** полное описание метода API**************************************/
        public ApiMethodDescription GetApiMethodFullDescription(string methodName) { // метод для получения подробной информации о методе API 

            var met = type.GetMethod(methodName); // получение метода по имени
            if (met == null || !met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()) // метод не найден или не имеет атрибута ApiMethodAttribute
                return null; // возвращаем null

            var result = new ApiMethodDescription(); // создание объекта ApiMethodDescription
            result.MethodDescription = new CommonDescription(methodName, GetApiMethodDescription(methodName)); // задание описания метода 
            result.ParamDescriptions = GetApiMethodParamNames(methodName).Select(param => GetApiMethodParamFullDescription(methodName, param)).ToArray(); // получение описаний параметров и добавление их в массив

            var returnParameter = met.ReturnParameter; // тип возвращаемого значения метода
            bool isNecessaryToSetReturnParameter = false; //  должно ли добавляться описание возвращаемого значения
            var returnParamDiscription = new ApiParamDescription(); // описание возвращаемого значения метода API
            returnParamDiscription.ParamDescription = new CommonDescription(); // описание свойства в объекте 

            var descriptionAttribute = returnParameter.GetCustomAttributes(true).OfType<ApiDescriptionAttribute>().FirstOrDefault(); // получение описания возвращаемого значения из атрибута ApiDescriptionAttribute
            if (descriptionAttribute != null) {
                returnParamDiscription.ParamDescription.Description = descriptionAttribute.Description;
                isNecessaryToSetReturnParameter = true;
            }

            var intValidationAttribute = returnParameter.GetCustomAttributes(true).OfType<ApiIntValidationAttribute>().FirstOrDefault(); // получение атрибута ApiIntValidationAttribute для возвращаемого значения
            if (intValidationAttribute != null) {
                returnParamDiscription.MinValue = intValidationAttribute.MinValue; // установка минимального значений для возвращаемого значения
                returnParamDiscription.MaxValue = intValidationAttribute.MaxValue; // установка максимального значений для возвращаемого значения
                isNecessaryToSetReturnParameter = true;
            }

            var requiredAttribute = returnParameter.GetCustomAttributes(true).OfType<ApiRequiredAttribute>().FirstOrDefault(); // получение атрибута ApiRequiredAttribute для возвращаемого значения
            if (requiredAttribute != null) {
                returnParamDiscription.Required = requiredAttribute.Required; // установка значения обязательности возвращаемого значения
                isNecessaryToSetReturnParameter = true;
            }

            if (isNecessaryToSetReturnParameter) // задание описания возвращаемого значения, если необходимо
                result.ReturnDescription = returnParamDiscription;

            return result; // возвращаем объект ApiMethodDescription с подробной информацией о методе API
        }
    }
}