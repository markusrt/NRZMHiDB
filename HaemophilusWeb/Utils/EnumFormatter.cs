using SmartFormat.Core.Extensions;

namespace HaemophilusWeb.Utils
{
    public class EnumFormatter : IFormatter
    {
        public string[] Names { get; set; } = { "enum" };

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var currentValue = formattingInfo.CurrentValue;
            var type = formattingInfo.CurrentValue.GetType();
            var iCanHandleThisInput = type.IsEnum;
            if (!iCanHandleThisInput)
                return false;

            formattingInfo.Write(EnumUtils.GetEnumDescription(type, currentValue));

            return true;
        }
    }
}