using SmartFormat.Core.Extensions;

namespace HaemophilusWeb.Utils
{
    public class EnumFormatter : IFormatter
    {
        public string Name { get; set; } = "enum";
        public bool CanAutoDetect { get; set; }

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