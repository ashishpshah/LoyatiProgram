using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace Seed_Admin
{
    public static class AppHttpContextAccessor
    {
        private static IHttpContextAccessor _httpContextAccessor;
        private static IDataProtector _dataProtector;
        private static IConfiguration _iConfig;
        private static IHttpClientFactory _clientFactory;
        private static string _contentRootPath;
        private static string _webRootPath;

        //private static List<UserMenuAccess> UserMenuAccess;
        //private static List<UserMenuAccess> UserMenuPermission;

        public static void Configure(IHttpContextAccessor httpContextAccessor, IHostEnvironment env_Host, IWebHostEnvironment env_Web, IDataProtectionProvider provider, IConfiguration iConfig, IHttpClientFactory clientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _contentRootPath = env_Host.ContentRootPath;
            _webRootPath = env_Web.WebRootPath;
            _dataProtector = provider.CreateProtector("Seed_Admin");
            _iConfig = iConfig;
            _clientFactory = clientFactory;
        }

        //public static void Configure_UserMenuAccess(List<UserMenuAccess> userMenuAccess, List<UserMenuAccess> userMenuPermission)
        //{
        //	UserMenuAccess = userMenuAccess;
        //	UserMenuPermission = userMenuPermission;
        //}

        public static IHttpContextAccessor HttpContextAccessor => _httpContextAccessor;
        public static HttpContext AppHttpContext => _httpContextAccessor.HttpContext;
        public static ISession AppSession => AppHttpContext.Session;
        //public static string AppBaseUrl => $"{AppHttpContext.Request.Scheme}://{AppHttpContext.Request.Host}{AppHttpContext.Request.PathBase}";
        public static string ContentRootPath => $"{_contentRootPath}";
        public static string WebRootPath => $"{_webRootPath}";
        public static IConfiguration AppConfiguration => _iConfig;
        public static string CurrentLoogedUser => $"{(AppHttpContext.Session.GetInt32("UserId") ?? 0)}";
        //public static string Loading_Bay => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("LOADING_BAY").Value ?? "");
        public static string EncrKey => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("EncrKey").Value ?? "");
        public static bool IsLogActive => Convert.ToBoolean(AppHttpContextAccessor.AppConfiguration.GetSection("IsLogActive").Value);
        public static bool IsLogActive_Error => Convert.ToBoolean(AppHttpContextAccessor.AppConfiguration.GetSection("IsLogActive_Error").Value);
        //public static bool IsCloudDBActive => Convert.ToBoolean(AppHttpContextAccessor.AppConfiguration.GetSection("IsCloudDBActive").Value);
        //public static bool IsSendInvoiceQRToCloud => Convert.ToBoolean(AppHttpContextAccessor.AppConfiguration.GetSection("IsSendInvoiceQRToCloud").Value);
        ////public static bool IsInvoiceAPIActive => Convert.ToBoolean(AppHttpContextAccessor.AppConfiguration.GetSection("IsInvoiceAPIActive").Value);
        //public static string API_Url => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("API_Url").Value);
        //public static string API_Url_MDA => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("API_Url_MDA").Value);
        //public static string Invoice_QR_Image_URL_Domain => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("Invoice_QR_Image_URL_Domain").Value);
        //public static string PlantCode => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("PlantCode").Value);
        //public static Int64 PlantId => Convert.ToInt64(AppHttpContextAccessor.AppConfiguration.GetSection("PlantId").Value);
        //public static string Vendor_Portal_Url => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("Vendor_Portal_Url").Value);

        public static bool IsSendMail => Convert.ToBoolean(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("IsSendMail").Value);
        public static bool IsSendMail_Vendor => Convert.ToBoolean(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("IsSendMail_Vendor").Value);
        public static string AdminToMail => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("To").Value);
        public static string AdminFromMail => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("From").Value);
        public static string DisplayName => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("DisplayName").Value);
        public static string Host => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("Host").Value);
        public static int Port => Convert.ToInt32(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("Port").Value);
        public static bool DefaultCredentials => Convert.ToBoolean(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("DefaultCredentials").Value);
        public static bool EnableSsl => Convert.ToBoolean(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("EnableSsl").Value);
        public static string MailPassword => Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("Email_Configuration").GetSection("Password").Value);

        //public static List<UserMenuAccess> GetUserMenuAccesses() => UserMenuAccess;
        //public static List<UserMenuAccess> GetUserMenuPermission() => UserMenuPermission;

        public static void SetSession(string key, string value) => AppHttpContext.Session.SetString(key, value);
        public static string? GetSession(string key) => AppHttpContext.Session.GetString(key) ?? null;


        public static void Clear_Session()
        {
            if (AppHttpContext.Session != null)
                AppHttpContext.Session.Clear();
        }

        public static int? CalculateAge(DateTime dob, DateTime? calculateFromDate = null) => Common.CalculateAge(dob, calculateFromDate);

        public static List<(string Name, string IP, int Port)> GetWeighbridgeList()
        {
            var list = new List<(string, string, int)>();

            try
            {
                if (!string.IsNullOrEmpty(Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("Weighbridge_Interface_IPs_Ports").Value ?? "")))
                {
                    var entries = Convert.ToString(AppHttpContextAccessor.AppConfiguration.GetSection("Weighbridge_Interface_IPs_Ports").Value ?? "").Split(',');

                    foreach (var entry in entries)
                    {
                        var parts = entry.Trim().Split(':');
                        if (parts.Length == 3 && int.TryParse(parts[2], out int port))
                        {
                            list.Add((parts[0], parts[1], port));
                        }
                    }
                }
            }
            catch { }

            return list;
        }
    }

    public class StartsNumericConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out object routeValue))
            {
                string qr_prefix = routeValue?.ToString();
                if (!string.IsNullOrEmpty(qr_prefix) && char.IsDigit(qr_prefix[0]))
                {
                    return true; // Match if qr_prefix starts with a digit
                }
            }
            return false;
        }
    }
}
