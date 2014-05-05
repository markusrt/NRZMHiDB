using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace HaemophilusWeb.TestUtils
{
    public static class JsonUtils
    {
        public static T ConvertTo<T>(this JsonResult jsonResult)
        {
            if (jsonResult == null)
            {
                return default(T);
            }
            var serializer = new JavaScriptSerializer();
            var result = serializer.Deserialize<T>(serializer.Serialize(jsonResult.Data));
            return result;
        }
    }
}