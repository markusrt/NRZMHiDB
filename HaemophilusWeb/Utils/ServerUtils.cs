using System.Web;

namespace HaemophilusWeb.Utils
{
    public static class ServerUtils
    {
        public static string ReverseMapPath(this string path)
        {
            string appPath = HttpContext.Current.Server.MapPath("~");
            string res = string.Format("~/{0}", path.Replace(appPath, "").Replace("\\", "/"));
            return res;
        }
    }
}