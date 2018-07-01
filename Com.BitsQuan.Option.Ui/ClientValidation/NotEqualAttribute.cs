using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;


namespace Com.BitsQuan.Option.Ui.ClientValidation
{
    //[AttributeUsage(AttributeTargets.Property)]
    //[SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed to be a base class for other attributes.")]
    //public class NotEqualAttribute : ValidationAttribute, IClientValidatable
    //{
    //    public NotEqualAttribute(string NotEqualValue)
    //    {
    //        this.NotEqualValue = NotEqualValue;
    //    }

    //    protected override ValidationResult IsValid(object name, ValidationContext validationContext)
    //    {
    //     //   PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetProperty(NotEqualValue);
    //      //  return string.Format("{0}不能够等于 {1}", name, otherPropertyInfo);
    //     //   return new ValidationResult(FormatErrorMessage(string.Format("{0}不能够等于 {1}", name, otherPropertyInfo)));


    //        PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetProperty(NotEqualValue);
    //        if (otherPropertyInfo == null)
    //        {
    //          //  return new ValidationResult(String.Format(CultureInfo.CurrentCulture, MvcResources.CompareAttribute_UnknownProperty, NotEqualValue));
    //        }

    //        object otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
    //        if (Equals(name, otherPropertyValue))
    //        {
    //            //if (OtherPropertyDisplayName == null)
    //            //{
    //            //    OtherPropertyDisplayName = ModelMetadataProviders.Current.GetMetadataForProperty(() => validationContext.ObjectInstance, validationContext.ObjectType, OtherProperty).GetDisplayName();
    //            //}
    //            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    //        }
    //        return null;

    //    }
    //    public string NotEqualValue { get; private set; }

    //    public static string FormatPropertyForClientValidation(string property)
    //    {
    //        if (property == null)
    //        {
    //          //  throw new ArgumentException(Common_NullOrEmpty, "property");
    //        }
    //        return "*." + property;
    //    }
    //    public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
    //    {
    //        var validationRule = new ModelClientValidationRule
    //        {
    //            ErrorMessage = FormatErrorMessage(metadata.DisplayName),
    //            ValidationType = "notequal",
    //        };
    //        validationRule.ValidationParameters.Add("other",FormatPropertyForClientValidation(NotEqualValue));

    //        yield return validationRule;
    //    }
    //}
    //public class NotEqualAttribute : ValidationAttribute, IClientValidatable
    //{
    //    public string OtherProperty { get; set; }

    //    public NotEqualAttribute(string otherProperty)
    //    {
    //        OtherProperty = otherProperty;
    //    }
    //    public override bool IsValid(object value)
    //    {
    //        return true;
    //    }
    //    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    //    {
    //        //从验证上下文中可以获取我们想要的的属性
    //        var property = validationContext.ObjectType.GetProperty(OtherProperty);
    //        if (property == null)
    //        {
    //            return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "{0} 不存在", OtherProperty));
    //        }

    //        //获取属性的值
    //        var otherValue = property.GetValue(validationContext.ObjectInstance, null);
    //        if (object.Equals(value, otherValue))
    //        {
    //            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    //        }
    //        return null;
    //    }

    //    public System.Collections.Generic.IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
    //    {
    //        var rule = new ModelClientValidationRule
    //        {
    //            ValidationType = "notequalto",
    //            ErrorMessage = FormatErrorMessage(metadata.GetDisplayName())
    //        };
    //        rule.ValidationParameters["other"] = OtherProperty;
    //        yield return rule;
    //    }

    //}

    public class NotEqualAttribute : ValidationAttribute, IClientValidatable
    {
        public NotEqualAttribute(string NotEqualValue)
        {
            this.NotEqualValue = NotEqualValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //从验证上下文中可以获取我们想要的的属性
            var property = validationContext.ObjectType.GetProperty(NotEqualValue);
            if (property == null)
            {
                return new ValidationResult(string.Format(CultureInfo.CurrentCulture, "{0} 不存在", NotEqualValue));
            }

            //获取属性的值
            var otherValue = property.GetValue(validationContext.ObjectInstance, null);
            if (object.Equals(value, otherValue))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return null;
        }
        public override string FormatErrorMessage(string name)
        {
            return string.Format("{0}不能够等于 {1}", name, NotEqualValue);
        }
        public string NotEqualValue { get; private set; }
        public IEnumerable<ModelClientValidationRule>
               GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var validationRule = new ModelClientValidationRule
            {
                //ErrorMessage = FormatErrorMessage(metadata.DisplayName),
                ErrorMessage =this.ErrorMessage,
                ValidationType = "notequal",
            };
            //validationRule.ValidationParameters.Add("value", NotEqualValue);
            //validationRule.ValidationParameters.Add("value", NotEqualValue);
            validationRule.ValidationParameters["other"] = NotEqualValue;
            yield return validationRule;
        }
    }
}

