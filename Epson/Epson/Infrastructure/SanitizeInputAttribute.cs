using Ganss.Xss;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Epson.Infrastructure
{
    public class SanitizeInputAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sanitizer = new HtmlSanitizer();

            foreach (var arg in context.ActionArguments)
            {
                SanitizeObjectProperties(arg.Value, sanitizer);
            }

            base.OnActionExecuting(context);
        }

        private void SanitizeObjectProperties(object obj, HtmlSanitizer sanitizer)
        {
            if (obj == null) return;

            var properties = obj.GetType().GetProperties().Where(p => p.CanRead && p.CanWrite);

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var originalValue = (string)property.GetValue(obj);
                    var sanitizedValue = sanitizer.Sanitize(originalValue);
                    property.SetValue(obj, sanitizedValue);
                }
                else if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                {
                    var propertyValue = property.GetValue(obj);
                    SanitizeObjectProperties(propertyValue, sanitizer);
                }
            }
        }

    }

}
