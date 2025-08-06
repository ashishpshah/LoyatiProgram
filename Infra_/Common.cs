using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Seed_Admin
{
    public class Common
    {
        private static string EncrKey = AppHttpContextAccessor.EncrKey;
        public static string USER_ID_KEY => "AuthenticatedUser";
        public static string IS_SUPER_USER_KEY => "AuthenticatedSuperUser";
        public static string USER_COMPANY_ID_KEY => "AuthenticatedUser_Company";
        public static string USER_BRANCH_ID_KEY => "AuthenticatedUser_Branch";
        public static string USER_NAME_KEY => "AuthenticatedUser_Name";
        public static string USER_TYPE_KEY => "AuthenticatedUser_Type";
        public static string USER_ROLE_Id_KEY => "AuthenticatedUserRole";
        public static string USER_ROLE_KEY => "AuthenticatedUserRole_Name";
        public static string USER_MENUACCESS_KEY => "AuthenticatedUserMenuAccess";
        public static string HTTPCOOKIE_KEY => "Seed_Admin.HttpCookie";

        private static List<Menu> UserMenuAccess;
        //private static List<Plant> UserPlantAccess;

        public static string DateTimeFormat_ddMMyyyy => "dd/MM/yyyy";
        public static string DateTimeFormat_ddMMyyyy_HHmm => "dd/MM/yyyy HH:mm";
        public static string DateTimeFormat_ddMMyyyy_HHmmss => "dd/MM/yyyy HH:mm:ss";
        //public static string DateTimeFormat_ddMMyyyy_HHmmss => "dd/MM/yyyy HH:mm:ss";
        public static bool CheckValueIsNull(object val) => !(val != DBNull.Value && val != null && !string.IsNullOrEmpty(Convert.ToString(val)));
        public static void Clear_Session() => AppHttpContextAccessor.Clear_Session();

        public static void Set_Session_Int(string key, Int64 value) => AppHttpContextAccessor.AppHttpContext.Session.SetString(key, value.ToString());
        public static long Get_Session_Int(string key) => (AppHttpContextAccessor.AppHttpContext != null && AppHttpContextAccessor.AppHttpContext.Session.Keys.Any(x => x == key) ? Convert.ToInt64(AppHttpContextAccessor.AppHttpContext.Session.GetString(key) ?? "0") : -1);

        public static void Set_Session(string key, string value) => AppHttpContextAccessor.AppHttpContext.Session.SetString(key, value);
        public static string Get_Session(string key) => (AppHttpContextAccessor.AppHttpContext.Session.Keys.Any(x => x == key) ? AppHttpContextAccessor.AppHttpContext.Session.GetString(key) : null);



        private static string controller_action;
        public static void Set_Controller_Action(string _value) => controller_action = _value;
        public static string Get_Controller_Action => controller_action;


        public static bool IsUserLogged() => Get_Session_Int(SessionKey.USER_ID) > 0;
        public static bool IsSuperAdmin() => Get_Session_Int(SessionKey.ROLE_ID) == 1;
        public static bool IsAdmin() => Get_Session_Int(SessionKey.ROLE_ADMIN) == 1;
        public static Int64 LoggedUser_Id() => Get_Session_Int(SessionKey.USER_ID);
        public static Int64 LoggedUser_Plant_Id() => Get_Session_Int(SessionKey.PLANT_ID);



        public static void Configure_UserMenuAccess(List<Menu> userMenuAccess) => UserMenuAccess = userMenuAccess;
        //public static void Configure_UserPlantAccess(List<Plant> userPlantAccess) => UserPlantAccess = userPlantAccess;

        public static List<Menu> GetUserMenuAccesses() => UserMenuAccess;
        //public static List<Plant> GetUserPlantAccesses() => UserPlantAccess;
        public static long GetCurrentMenuId(string Area = "", string Controller = "")
        {
            try { return UserMenuAccess.Where(x => x.Area == (Area ?? "") && x.Controller == (Controller ?? "")).Select(x => x.Id).FirstOrDefault(); } catch { return 0; }
        }


        public static string Encrypt(string strText)
        {
            try
            {
                if (!string.IsNullOrEmpty(strText))
                {
                    byte[] byKey = { };
                    byte[] IV = {
                            0x12,
                            0x34,
                            0x56,
                            0x78,
                            0x90,
                            0xab,
                            0xcd,
                            0xef
                        };

                    //byKey = System.Text.Encoding.UTF8.GetBytes(Strings.Left(strEncrKey, 8));
                    byKey = System.Text.Encoding.UTF8.GetBytes(EncrKey.Substring(0, 8));
                    //byKey = System.Text.Encoding.UTF8.GetBytes(Strings.Left(strEncrKey, 8));
                    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                    byte[] inputByteArray = Encoding.UTF8.GetBytes(strText);
                    MemoryStream ms = new MemoryStream();
                    CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());

                }
            }
            catch (ExecutionEngineException ex) { }

            return "";
        }

        public static string Decrypt(string strText)
        {
            byte[] byKey = { };
            byte[] IV = {
                            0x12,
                            0x34,
                            0x56,
                            0x78,
                            0x90,
                            0xab,
                            0xcd,
                            0xef
                        };
            byte[] inputByteArray = new byte[strText.Length + 1];
            try
            {
                //byKey = System.Text.Encoding.UTF8.GetBytes(Strings.Left(sDecrKey, 8));
                byKey = System.Text.Encoding.UTF8.GetBytes(EncrKey.Substring(0, 8));
                using (System.Security.Cryptography.DESCryptoServiceProvider des = new System.Security.Cryptography.DESCryptoServiceProvider())
                {
                    inputByteArray = Convert.FromBase64String(strText);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write))
                        {
                            cs.Write(inputByteArray, 0, inputByteArray.Length);
                            cs.FlushFinalBlock();
                            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
                            return encoding.GetString(ms.ToArray());
                        }
                    }
                }

            }
            catch (ExecutionEngineException ex)
            {
                return ex.Message;
            }
        }


        //public static System.Data.DataTable ReadExcel(string filePath, string sheetName = null)
        //{
        //	System.Data.DataTable dataTable = new System.Data.DataTable();

        //	FileInfo file = new FileInfo(filePath);

        //	ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        //	if (file.Exists)
        //	{
        //		var package = new ExcelPackage(file);
        //		var worksheetData = package.Workbook.Worksheets.First();

        //		for (int i = 1; i <= worksheetData.Columns.Count(); i++)
        //		{
        //			dataTable.Columns.Add(worksheetData.Columns[i].ToString());
        //		}

        //		for (int i = 1; i <= worksheetData.Rows.Count(); i++)
        //		{
        //			DataRow row = dataTable.NewRow();
        //			for (int j = 1; j <= worksheetData.Columns.Count(); j++)
        //			{
        //				row[j - 1] = worksheetData.Cells[i, j].Value;
        //			}
        //			dataTable.Rows.Add(row);
        //		}

        //	}

        //	return dataTable;
        //}


        public static async Task<(bool IsSuccess, string Message)> SendEmail(string subject, string body, string[] to_mails, List<(Stream contentStream, string contentType, string? fileDownloadName)> attachments, bool isBodyHtml = false)
        {
            //LogService.LogInsert("Common - SendEmail", $"Send Email => Starting at {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/")} | Subject : {subject} | To : {string.Join(", ", to_mails)}", null);

            (bool IsSuccess, string Message) result = (false, "Sending Mail service is stop.");

            try
            {
                if (AppHttpContextAccessor.IsSendMail)
                {
                    if (to_mails == null || !AppHttpContextAccessor.IsSendMail_Vendor)
                        to_mails = new string[] { };

                    if (to_mails == null || to_mails.Length == 0)
                        to_mails = AppHttpContextAccessor.AdminToMail.Replace(" ", "").Replace(";", ",").Split(",").ToArray();

                    //LogService.LogInsert("Common - SendEmail", $"Send Email => Update To mail address at {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/")} | Subject : {subject} | To : {string.Join(", ", to_mails)}", null);

                    using (System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage())
                    {
                        mailMessage.From = new System.Net.Mail.MailAddress(AppHttpContextAccessor.AdminFromMail, AppHttpContextAccessor.DisplayName);

                        mailMessage.Subject = subject;

                        mailMessage.Body = body;

                        mailMessage.IsBodyHtml = isBodyHtml;

                        foreach (string item in to_mails)
                            mailMessage.To.Add(new System.Net.Mail.MailAddress(item.Trim())); // Trim to remove leading/trailing spaces

                        //if (to_mails.Contains(","))
                        //{
                        //	foreach (string item in toEmail.Split(','))
                        //	{
                        //		mailMessage.To.Add(new System.Net.Mail.MailAddress(item.Trim())); // Trim to remove leading/trailing spaces
                        //	}
                        //}
                        //else if (toEmail.Contains(";"))
                        //{
                        //	//foreach (string item in toEmail.Split(';'))
                        //	//{
                        //	//	mailMessage.To.Add(new System.Net.Mail.MailAddress(item.Trim())); // Trim to remove leading/trailing spaces
                        //	//}
                        //}
                        //else
                        //{
                        //	mailMessage.To.Add(new System.Net.Mail.MailAddress(toEmail));
                        //}

                        // Create and configure SMTP client
                        //System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                        //smtp.Host = AppHttpContextAccessor.Host;
                        //smtp.Port = int.Parse(AppHttpContextAccessor.Port);
                        //smtp.EnableSsl = Convert.ToBoolean(AppHttpContextAccessor.EnableSsl);

                        //// Provide authentication credentials
                        //smtp.UseDefaultCredentials = false; // Set to false to specify custom credentials
                        //smtp.Credentials = new System.Net.NetworkCredential(AppHttpContextAccessor.AdminFromMail, AppHttpContextAccessor.MailPassword);

                        // Configure the SMTP client
                        System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(AppHttpContextAccessor.Host, AppHttpContextAccessor.Port)
                        {
                            EnableSsl = AppHttpContextAccessor.EnableSsl,  // Enable SSL (Implicit TLS)
                            Credentials = new System.Net.NetworkCredential(AppHttpContextAccessor.AdminFromMail, AppHttpContextAccessor.MailPassword),
                            DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = AppHttpContextAccessor.DefaultCredentials
                        };

                        // Send the email
                        await smtpClient.SendMailAsync(mailMessage);

                        result.IsSuccess = true;
                        result.Message = "Mail Sent successfully.";
                    }

                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Sending Mail service is stop.";
                }

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;

                if (ex != null)
                {
                    result.Message = "Error : " + ex.Message.ToString() + Environment.NewLine;

                    if (ex.InnerException != null)
                    {
                        try { result.Message = result.Message + " | InnerException: " + ex.InnerException.ToString().Substring(0, (ex.InnerException.ToString().Length > 1000 ? 1000 : ex.InnerException.ToString().Length)); } catch { result.Message = result.Message + "InnerException: " + ex.InnerException?.ToString(); }
                    }

                    if (ex.StackTrace != null)
                    {
                        try { result.Message = result.Message + " | StackTrace: " + ex.StackTrace.ToString().Substring(0, (ex.StackTrace.ToString().Length > 1000 ? 1000 : ex.StackTrace.ToString().Length)); } catch { result.Message = result.Message + "InnerException: " + ex.StackTrace?.ToString(); }
                    }

                    if (ex.Source != null)
                    {
                        try { result.Message = result.Message + " | Source: " + ex.Source.ToString().Substring(0, (ex.Source.ToString().Length > 1000 ? 1000 : ex.Source.ToString().Length)); } catch { result.Message = result.Message + "InnerException: " + ex.Source?.ToString(); }
                    }

                    if (ex.StackTrace == null && ex.Source == null)
                    {
                        try { result.Message = result.Message + " | Exception: " + ex.ToString().Substring(0, (ex.Source.ToString().Length > 3000 ? 3000 : ex.Source.ToString().Length)); } catch { result.Message = result.Message + "Exception: " + ex?.ToString(); }
                    }
                }

            }

            //LogService.LogInsert("Common - SendEmail", $"Send Email => Completed at {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/")} | IsSuccess : {result.IsSuccess} | Message : {result.Message} | Subject : {subject} | To : {string.Join(", ", to_mails)}", null);

            return result;
        }


        //public static string GenerateQrCodeBase64_ZXing(string qrText, int size = 250)
        //{
        //	var qrCodeWriter = new ZXing.BarcodeWriterPixelData
        //	{
        //		Format = ZXing.BarcodeFormat.QR_CODE,
        //		Options = new QrCodeEncodingOptions
        //		{
        //			Height = size,
        //			Width = size,
        //			Margin = 0
        //		}
        //	};

        //	var pixelData = qrCodeWriter.Write(qrText);

        //	// Generate the QR code as a bitmap
        //	Bitmap qrCodeBitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

        //	using (MemoryStream memoryStream = new MemoryStream())
        //	{
        //		var bitmapData = qrCodeBitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

        //		try
        //		{
        //			// we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image
        //			System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
        //		}
        //		finally
        //		{
        //			qrCodeBitmap.UnlockBits(bitmapData);
        //		}

        //		// Save the bitmap to the MemoryStream in PNG format
        //		qrCodeBitmap.Save(memoryStream, ImageFormat.Png);

        //		// Convert the MemoryStream to a byte array
        //		byte[] imageBytes = memoryStream.ToArray();

        //		// Convert the byte array to a Base64 string
        //		string base64String = Convert.ToBase64String(imageBytes);

        //		return base64String;
        //	}
        //}

        //public static string GenerateQrCodeBase64(string text, int size = 250)
        //{
        //	using (var qrGenerator = new QRCodeGenerator())
        //	using (var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
        //	using (var qrCode = new BitmapByteQRCode(qrCodeData))
        //	using (var qrCodeImage = new Bitmap(new MemoryStream(qrCode.GetGraphic(20))))
        //	using (var resizedQrCodeImage = new Bitmap(qrCodeImage, new System.Drawing.Size(size, size)))
        //	using (var ms = new MemoryStream())
        //	{
        //		resizedQrCodeImage.Save(ms, ImageFormat.Png);
        //		return Convert.ToBase64String(ms.ToArray());
        //	}
        //}

        public static int? CalculateAge(DateTime? dob = null, DateTime? calculateFromDate = null)
        {
            if (dob == null) return null; // Ensure dob is a valid date
            if (dob == DateTime.MinValue) return null; // Ensure dob is a valid date

            DateTime referenceDate = calculateFromDate ?? DateTime.Today; // Use provided date or default to today

            int age = referenceDate.Year - dob?.Year ?? 0;
            if (referenceDate < dob?.AddYears(age)) age--; // Adjust if birthday hasn't occurred yet

            return age;
        }
    }

    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static string UserName { get; set; }
        public static int RoleId { get; set; }
        public static string Role { get; set; }
        public static string UserImagePath { get; set; }
    }

    public static class Status
    {
        public static int FAILED => 0;
        public static int SUCCESS => 1;
        public static int NOTFOUND => 2;
        public static int ALREADYEXIST => 3;

    };

    public static class AccessType
    {
        public static int Read => 0;
        public static int Write => 1;
        public static int Delete => 2;

        public static int Get(AccessType_Enum x)
        {
            if (x == AccessType_Enum.Read)
                return Read;
            else if (x == AccessType_Enum.Write)
                return Write;
            //else if (x == AccessType_Enum.Create)
            //    return Create;
            //else if (x == AccessType_Enum.Update)
            //    return Update;
            else
                return Delete;
        }
    };

    public enum AccessType_Enum { Read = 0x0, Write = 0x1, /*Create = 0x1, Update = 0x2, */ Delete = 0x2 };

    public static class AccessControlType
    {
        public static bool Allow => true;
        public static bool Deny => false;
    };

    //public static class UserPermission
    //{
    //	public static bool VerifyPermission(string controller, AccessType_Enum permission)
    //	{
    //		return GetPermission(controller, permission);
    //	}

    //	private static bool GetPermission(string controller, AccessType_Enum permission)
    //	{
    //		if (HttpContext.Current.Session[Common.AUTH_USER_KEY] == null) return false;

    //		if (HttpContext.Current.Session[Common.AUTH_USER_MENUACCESS_KEY] == null) return false;

    //		if (HttpContext.Current.Session[Common.AUTH_USER_MENU_PERMISSION_KEY] == null) return false;

    //		List<UserMenuAccess> listMenuAccess = HttpContext.Current.Session[Common.AUTH_USER_MENU_PERMISSION_KEY] as List<UserMenuAccess>;

    //		if (listMenuAccess != null && listMenuAccess.Count > 0)
    //		{
    //			if (permission == AccessType_Enum.Read && listMenuAccess.Count(x => x.Controller == controller && x.IsRead == true) > 0) return true;
    //			else if (permission == AccessType_Enum.Write && listMenuAccess.Count(x => x.Controller == controller && x.IsCreate == true) > 0) return true;
    //			else if (permission == AccessType_Enum.Write && listMenuAccess.Count(x => x.Controller == controller && x.IsUpdate == true) > 0) return true;
    //			else if (permission == AccessType_Enum.Delete && listMenuAccess.Count(x => x.Controller == controller && x.IsDelete == true) > 0) return true;
    //			else return false;
    //		}

    //		return false;
    //	}

    //}



    public class CustomFileOrderComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            // Customize the comparison logic based on your requirements
            if (x.EndsWith("_Delete.json") && !y.EndsWith("_Delete.json"))
            {
                return 1; // x comes after y
            }
            else if (!x.EndsWith("_Delete.json") && y.EndsWith("_Delete.json"))
            {
                return -1; // x comes before y
            }
            else
            {
                // If neither ends with "_Delete.json", or both do, use default string comparison
                return string.Compare(x, y);
            }
        }
    }
}
