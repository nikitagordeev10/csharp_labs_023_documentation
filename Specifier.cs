using System;
using System.Linq;
using System.Reflection;

namespace Documentation {
    public class Specifier<T> : ISpecifier { // �����, ����������� ��������� ISpecifier ��� ��������� ���������� � ������������ API c ������ T
        Type type = typeof(T); // ������� ���������� � ���� T

        /************************************** �������� API **************************************/
        public string GetApiDescription() { // ����� ��� ��������� �������� API 
            var attribute = type.GetCustomAttributes(true).OfType<ApiDescriptionAttribute>().FirstOrDefault(); // ��������� �������� ApiDescriptionAttribute ��� ����, �� ����
            if (attribute == null) // ������� �� ������
                return null; // ���������� null

            return attribute.Description; // ���������� �������� API 
        }

        /************************************** ����� ������� API **************************************/
        public string[] GetApiMethodNames() { // ����� ��� ��������� ���� ������� API
            var methods = type.GetMethods(); // ��������� ���� ��������� ������� ����
            var apiMethods = methods.Where(met => met.IsPublic && met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()); // ����� ������ ��� �������, ������� ����� ������� ApiMethodAttribute
            return apiMethods.Select(met => met.Name).ToArray(); // ��������� ���� ������� API � ����������� �� � ���� �������
        }

        /************************************** �������� ������ API **************************************/
        public string GetApiMethodDescription(string methodName) { // ����� ��� ��������� �������� ������ API
            var met = type.GetMethod(methodName); // ��������� ������ �� �����
            if (met == null || !met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()) // ����� �� ������ ��� �� ����� �������� ApiMethodAttribute
                return null; // ���������� null

            var attribute = met.GetCustomAttributes(true).OfType<ApiDescriptionAttribute>().FirstOrDefault(); // ��������� �������� ApiDescriptionAttribute ��� ������
            if (attribute == null) // ������� �� ������
                return null; // ���������� null

            return attribute.Description; // ���������� �������� ������
        }

        /************************************** ����� ���������� ������ API **************************************/
        public string[] GetApiMethodParamNames(string methodName) { // ����� ��� ��������� ���� ���������� ������ API 
            var met = type.GetMethod(methodName); // ��������� ������ �� �����
            if (met == null || !met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()) // ����� �� ������ ��� �� ����� �������� ApiMethodAttribute
                return null; // ���������� null

            return met.GetParameters().Select(param => param.Name).ToArray();  // ��������� ���� ���������� ������ � ����������� �� � ���� �������
        }

        /************************************** �������� ���������� ������ API **************************************/
        public string GetApiMethodParamDescription(string methodName, string paramName) { // ����� ��� ��������� �������� ��������� ������ API
            var met = type.GetMethod(methodName); // ��������� ������ �� �����
            if (met == null || !met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()) // ����� �� ������ ��� �� ����� �������� ApiMethodAttribute
                return null; // ���������� null

            var parameter = met.GetParameters().Where(param => param.Name == paramName); // ��������� ��������� ������ �� �����
            if (!parameter.Any()) // �������� �� ������
                return null; // ���������� null

            var attribute = parameter.First().GetCustomAttributes(true).OfType<ApiDescriptionAttribute>().FirstOrDefault(); // ��������� �������� ApiDescriptionAttribute ��� ���������, �� ����
            if (attribute == null) // ������� �� ������
                return null; // ���������� null

            return attribute.Description; // ���������� �������� ��������� 
        }

        /************************************** ������ �������� ���������� ������ API **************************************/
        public ApiParamDescription GetApiMethodParamFullDescription(string methodName, string paramName) { // ����� ��� ��������� ������� �������� ��������� API-������

            var result = new ApiParamDescription { ParamDescription = new CommonDescription(paramName) }; // ������ ��� �������� ����������

            var met = type.GetMethod(methodName); // �������� ����� �� ��� ��������
            if (met == null) { // ����� �� ������
                return result; // ����������  ��������� 
            }

            var apiMethodAttr = met.GetCustomAttribute<ApiMethodAttribute>(); // �������� �������, ����������� �� ��, ��� ����� �������� ������� API
            if (apiMethodAttr == null) { // ������� �� ������
                return result; // ���������� ���������
            }

            var parameter = met.GetParameters().SingleOrDefault(param => param.Name == paramName); // �������� �������� ������ �� ��� ��������
            if (parameter == null) { // �������� �� ������
                return result; // ���������� ���������  
            }

            var descriptionAttribute = parameter.GetCustomAttribute<ApiDescriptionAttribute>(); // �������� �������� ���������
            if (descriptionAttribute != null) { // ��� ������� � ��������
                result.ParamDescription.Description = descriptionAttribute.Description;
            }

            var intValidationAttribute = parameter.GetCustomAttribute<ApiIntValidationAttribute>(); // �������� �������� �������� �������������� ���������
            if (intValidationAttribute != null) { // ��� �������?
                result.MinValue = intValidationAttribute.MinValue; // ��������� ������������ �������� ��� ���������
                result.MaxValue = intValidationAttribute.MaxValue; // ��������� ������������� �������� ��� ���������
            }

            var requiredAttribute = parameter.GetCustomAttribute<ApiRequiredAttribute>(); // �������� �������, ����������� �� ��, �������� �� �������� ������������.
            if (requiredAttribute != null) {
                result.Required = requiredAttribute.Required; // ��������� �������� ��������������
            }

            return result; // ���������� ��������� � ������ ��������� ���������
        }

        /************************************** ������ �������� ������ API**************************************/
        public ApiMethodDescription GetApiMethodFullDescription(string methodName) { // ����� ��� ��������� ��������� ���������� � ������ API 

            var met = type.GetMethod(methodName); // ��������� ������ �� �����
            if (met == null || !met.GetCustomAttributes(true).OfType<ApiMethodAttribute>().Any()) // ����� �� ������ ��� �� ����� �������� ApiMethodAttribute
                return null; // ���������� null

            var result = new ApiMethodDescription(); // �������� ������� ApiMethodDescription
            result.MethodDescription = new CommonDescription(methodName, GetApiMethodDescription(methodName)); // ������� �������� ������ 
            result.ParamDescriptions = GetApiMethodParamNames(methodName).Select(param => GetApiMethodParamFullDescription(methodName, param)).ToArray(); // ��������� �������� ���������� � ���������� �� � ������

            var returnParameter = met.ReturnParameter; // ��� ������������� �������� ������
            bool isNecessaryToSetReturnParameter = false; //  ������ �� ����������� �������� ������������� ��������
            var returnParamDiscription = new ApiParamDescription(); // �������� ������������� �������� ������ API
            returnParamDiscription.ParamDescription = new CommonDescription(); // �������� �������� � ������� 

            var descriptionAttribute = returnParameter.GetCustomAttributes(true).OfType<ApiDescriptionAttribute>().FirstOrDefault(); // ��������� �������� ������������� �������� �� �������� ApiDescriptionAttribute
            if (descriptionAttribute != null) {
                returnParamDiscription.ParamDescription.Description = descriptionAttribute.Description;
                isNecessaryToSetReturnParameter = true;
            }

            var intValidationAttribute = returnParameter.GetCustomAttributes(true).OfType<ApiIntValidationAttribute>().FirstOrDefault(); // ��������� �������� ApiIntValidationAttribute ��� ������������� ��������
            if (intValidationAttribute != null) {
                returnParamDiscription.MinValue = intValidationAttribute.MinValue; // ��������� ������������ �������� ��� ������������� ��������
                returnParamDiscription.MaxValue = intValidationAttribute.MaxValue; // ��������� ������������� �������� ��� ������������� ��������
                isNecessaryToSetReturnParameter = true;
            }

            var requiredAttribute = returnParameter.GetCustomAttributes(true).OfType<ApiRequiredAttribute>().FirstOrDefault(); // ��������� �������� ApiRequiredAttribute ��� ������������� ��������
            if (requiredAttribute != null) {
                returnParamDiscription.Required = requiredAttribute.Required; // ��������� �������� �������������� ������������� ��������
                isNecessaryToSetReturnParameter = true;
            }

            if (isNecessaryToSetReturnParameter) // ������� �������� ������������� ��������, ���� ����������
                result.ReturnDescription = returnParamDiscription;

            return result; // ���������� ������ ApiMethodDescription � ��������� ����������� � ������ API
        }
    }
}