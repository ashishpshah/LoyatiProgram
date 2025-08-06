﻿
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Seed_Admin.Infra
{
	public partial class DataContext : DbContext
	{
		public DataContext(DbContextOptions<DataContext> options) : base(options) { }

		public virtual DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }

		public virtual DbSet<LoyaltyPointsQrcode> LoyaltyPointsQrcodes { get; set; }

		public virtual DbSet<Menu> Menus { get; set; }
		public virtual DbSet<Role> Roles { get; set; }
		public virtual DbSet<User> Users { get; set; }
		public virtual DbSet<UserMenuAccess> UserMenuAccesses { get; set; }
		public virtual DbSet<UserRoleMapping> UserRoleMappings { get; set; }
		public virtual DbSet<RoleMenuAccess> RoleMenuAccesses { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//modelBuilder.HasDefaultSchema("padhyaso_Leoz");

			modelBuilder.HasDefaultSchema("padhyaso_seed");

			modelBuilder.Entity<LoyaltyPoint>(entity =>
			{
				entity.ToTable("LoyaltyPoints", "dbo");

				entity.Property(e => e.Points).HasColumnType("decimal(18, 0)");
				entity.Property(e => e.QrcodeId).HasColumnName("QRCodeId");
			});

			modelBuilder.Entity<LoyaltyPointsQrcode>(entity =>
			{
				entity.ToTable("LoyaltyPoints_QRCode", "dbo");

				entity.Property(e => e.Qrcode).HasColumnName("QRCode");
			});

			modelBuilder.Entity<User>(entity =>
			{
				entity.HasKey(e => e.Id).HasName("PK_Users_1");

				entity.ToTable("Users", "dbo");

				entity.Property(e => e.Gstno).HasColumnName("GSTNo");
				entity.Property(e => e.LandSize).HasColumnType("numeric(18, 3)");
				entity.Property(e => e.NextChangePasswordDate).HasColumnName("Next_Change_Password_Date");
				entity.Property(e => e.NoOfWrongPasswordAttempts).HasColumnName("No_Of_Wrong_Password_Attempts");
			});

			modelBuilder.Entity<Menu>(entity =>
			{
				entity.HasKey(e => new { e.Id, e.ParentId });

				entity.ToTable("Menu", "dbo");

				entity.Property(e => e.Id).ValueGeneratedOnAdd();
			});


			modelBuilder.Entity<Role>(entity =>
			{
				entity.ToTable("Roles", "dbo");

				entity.Property(e => e.Name).HasMaxLength(50);
			});

			modelBuilder.Entity<RoleMenuAccess>(entity =>
			{
				entity
					.HasNoKey()
					.ToTable("RoleMenuAccess", "dbo");
			});

			modelBuilder.Entity<User>(entity =>
			{
				entity.ToTable("Users", "dbo");

				entity.Property(e => e.CreatedBy).HasDefaultValue(0L);
				entity.Property(e => e.LastModifiedBy).HasDefaultValue(0L);
				entity.Property(e => e.NextChangePasswordDate).HasColumnName("Next_Change_Password_Date");
				entity.Property(e => e.NoOfWrongPasswordAttempts).HasColumnName("No_Of_Wrong_Password_Attempts");
			});

			modelBuilder.Entity<UserMenuAccess>(entity =>
			{
				entity
					.HasNoKey()
					.ToTable("UserMenuAccess", "dbo");
			});

			modelBuilder.Entity<UserRoleMapping>(entity =>
			{
				entity.HasKey(e => e.Id).HasName("PK_UserRoleMapping_1");

				entity.ToTable("UserRoleMapping", "dbo");
			});


			modelBuilder.Entity<User>().HasKey(e => new { e.Id });
			modelBuilder.Entity<Role>().HasKey(e => new { e.Id });
			modelBuilder.Entity<UserRoleMapping>().HasKey(e => new { e.Id, e.UserId, e.RoleId });
			modelBuilder.Entity<Menu>().HasKey(e => new { e.Id });
			modelBuilder.Entity<UserMenuAccess>().HasKey(e => new { e.UserId, e.RoleId, e.MenuId, e.IsCreate, e.IsUpdate, e.IsRead, e.IsDelete });
			modelBuilder.Entity<RoleMenuAccess>().HasKey(e => new { e.RoleId, e.MenuId, e.IsCreate, e.IsUpdate, e.IsRead, e.IsDelete });

			base.OnModelCreating(modelBuilder);
		}

		public int SaveChanges(CancellationToken cancellationToken = default)
		{
			var entities = (from entry in ChangeTracker.Entries()
							where (entry.State == EntityState.Modified || entry.State == EntityState.Added)
							//&& (entry.Entity.ToString() != (typeof(Doctor_Department_Mapping)).FullName)
							select entry).ToList();

			var user = Common.LoggedUser_Id();

			if (user == null || user <= 0)
				throw new InvalidOperationException("Opps...! An unexpected error occurred while saving.");
			else
			{
				foreach (var entity in entities)
				{
					if (entity.State == EntityState.Added)
					{
						((EntitiesBase)entity.Entity).IsActive = true;
						((EntitiesBase)entity.Entity).IsDeleted = false;
						((EntitiesBase)entity.Entity).CreatedDate = DateTime.Now;
						((EntitiesBase)entity.Entity).CreatedBy = ((EntitiesBase)entity.Entity).CreatedBy == 0 ? user : ((EntitiesBase)entity.Entity).CreatedBy;
						((EntitiesBase)entity.Entity).LastModifiedDate = DateTime.Now;
						((EntitiesBase)entity.Entity).LastModifiedBy = ((EntitiesBase)entity.Entity).CreatedBy == 0 ? user : ((EntitiesBase)entity.Entity).CreatedBy;
					}

					if (entity.State == EntityState.Modified)
					{
						((EntitiesBase)entity.Entity).LastModifiedDate = DateTime.Now;
						((EntitiesBase)entity.Entity).LastModifiedBy = user;
					}

					if (entity.State == EntityState.Deleted)
					{
						((EntitiesBase)entity.Entity).IsActive = false;
						((EntitiesBase)entity.Entity).IsDeleted = true;
						((EntitiesBase)entity.Entity).LastModifiedDate = DateTime.Now;
						((EntitiesBase)entity.Entity).LastModifiedBy = user;
					}

				}
			}

			return base.SaveChanges();
		}
	}


	public static class DataContext_Command
	{
		public static string _connectionString = AppHttpContextAccessor.AppConfiguration.GetSection("DataConnection").Value;

		public static string Get_DbSchemaName()
		{
			string keyValue = "database=";
			int startIndex = _connectionString.IndexOf(keyValue) + keyValue.Length;
			int endIndex = _connectionString.IndexOf(';', startIndex);
			return _connectionString.Substring(startIndex, endIndex - startIndex);
		}

		public static DataTable ExecuteQuery(string query)
		{
			try
			{
				DataTable dt = new DataTable();

				SqlConnection connection = new SqlConnection(_connectionString);

				SqlDataAdapter oraAdapter = new SqlDataAdapter(query, connection);

				oraAdapter.Fill(dt);

				return dt;
			}
			catch (Exception ex)
			{
				LogService.LogInsert("ExecuteQuery_DataTable - DataContext", "", ex);
				return null;
			}

		}

		public static DataSet ExecuteQuery_DataSet(string sqlquerys)
		{
			DataSet ds = new DataSet();

			try
			{
				DataTable dt = new DataTable();

				SqlConnection connection = new SqlConnection(_connectionString);

				foreach (var sqlquery in sqlquerys.Split(";"))
				{
					dt = new DataTable();

					SqlDataAdapter oraAdapter = new SqlDataAdapter(sqlquery, connection);

					SqlCommandBuilder oraBuilder = new SqlCommandBuilder(oraAdapter);

					oraAdapter.Fill(dt);

					if (dt != null)
						ds.Tables.Add(dt);
				}

			}
			catch (Exception ex)
			{
				LogService.LogInsert("ExecuteQuery_DataSet - DataContext", "", ex);
				return null;
			}

			return ds;
		}

		public static DataTable ExecuteStoredProcedure_DataTable(string query, List<SqlParameter> parameters = null, bool returnParameter = false)
		{
			DataTable dt = new DataTable();

			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (parameters != null)
							foreach (SqlParameter param in parameters)
								cmd.Parameters.Add(param);

						SqlDataAdapter da = new SqlDataAdapter(cmd);

						da.Fill(dt);

						parameters = null;
					}
					conn.Close();
				}
			}
			catch (Exception ex)
			{
				LogService.LogInsert("ExecuteStoredProcedure_DataTable - DataContext", "", ex);
				return null;
			}

			return dt;
		}

		public static DataSet ExecuteStoredProcedure_DataSet(string sp, List<SqlParameter> spCol = null)
		{
			DataSet ds = new DataSet();

			try
			{
				using (SqlConnection con = new SqlConnection(_connectionString))
				{
					con.Open();

					using (SqlCommand cmd = new SqlCommand(sp, con))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						if (spCol != null && spCol.Count > 0)
							cmd.Parameters.AddRange(spCol.ToArray());

						using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
						{
							adp.Fill(ds);
						}
					}

					con.Close();
				}
			}
			catch (Exception ex) { LogService.LogInsert("ExecuteStoredProcedure_DataSet - DataContext", "", ex); }

			return ds;
		}

		public static bool ExecuteNonQuery(string query, List<SqlParameter> parameters = null)
		{
			try
			{
				using (SqlConnection con = new SqlConnection(_connectionString))
				{
					con.Open();

					SqlCommand cmd = con.CreateCommand();

					cmd.CommandType = CommandType.Text;
					cmd.CommandText = query;

					if (parameters != null)
						foreach (SqlParameter param in parameters)
							cmd.Parameters.Add(param);

					cmd.ExecuteNonQuery();
				}

				return true;
			}
			catch (Exception ex)
			{
				LogService.LogInsert("ExecuteNonQuery - DataContext", "", ex);
				return false;
			}
		}

		public static (bool, string, long) ExecuteStoredProcedure(string query, List<SqlParameter> parameters, bool returnParameter = false)
		{
			var response = string.Empty;

			using (SqlConnection con = new SqlConnection(_connectionString))
			{
				using (SqlCommand cmd = con.CreateCommand())
				{
					try
					{
						con.Open();

						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = query;
						//cmd.DeriveParameters();

						if (parameters != null && parameters.Count > 0)
							cmd.Parameters.AddRange(parameters.ToArray());

						if (returnParameter)
							cmd.Parameters.Add(new SqlParameter("@response", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });

						cmd.CommandTimeout = 86400;
						cmd.ExecuteNonQuery();

						//RETURN VALUE
						//response = cmd.Parameters["P_Response"].Value.ToString();

						response = "S|Success";

						if (cmd.Parameters.Contains("@response"))
						{
							response = cmd.Parameters["@response"].Value.ToString();
						}

						con.Close();
						cmd.Parameters.Clear();
						cmd.Dispose();

					}
					catch (Exception ex)
					{
						con.Close();
						cmd.Parameters.Clear();
						cmd.Dispose();

						response = "E|Opps!... Something went wrong. " + JsonConvert.SerializeObject(ex) + "|0";
					}
				}
			}

			if (!string.IsNullOrEmpty(response) && response.Contains("|"))
			{
				var msgtype = response.Split('|').Length > 0 ? Convert.ToString(response.Split('|')[0]) : "";
				var message = response.Split('|').Length > 1 ? Convert.ToString(response.Split('|')[1]).Replace("\"", "") : "";

				Int64 strid = 0;
				if (Int64.TryParse(response.Split('|').Length > 2 ? Convert.ToString(response.Split('|')[2]).Replace("\"", "") : "0", out strid)) { }
				//string paths = response.Split('|').Length > 3 ? response.Split('|')[3].Replace("\"", "") : "0";


				return (msgtype.Contains("S"), message, strid);
			}
			else
				return (false, ResponseStatusMessage.Error, 0);
		}


		public static (bool, string, long, string) ExecuteStoredProcedure_SQLwithpath(string query, List<SqlParameter> parameters, bool returnParameter = false)
		{
			var response = string.Empty;

			using (SqlConnection con = new SqlConnection(_connectionString))
			{
				using (SqlCommand cmd = con.CreateCommand())
				{
					try
					{
						con.Open();

						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandText = query;
						//cmd.DeriveParameters();

						if (parameters != null && parameters.Count > 0)
							cmd.Parameters.AddRange(parameters.ToArray());

						if (returnParameter)
							cmd.Parameters.Add(new SqlParameter("@response", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });

						cmd.CommandTimeout = 86400;
						cmd.ExecuteNonQuery();

						//RETURN VALUE
						//response = cmd.Parameters["P_Response"].Value.ToString();

						response = "S|Success";

						if (cmd.Parameters.Contains("@response"))
						{
							response = cmd.Parameters["@response"].Value.ToString();
						}

						con.Close();
						cmd.Parameters.Clear();
						cmd.Dispose();

					}
					catch (Exception ex)
					{
						con.Close();
						cmd.Parameters.Clear();
						cmd.Dispose();

						response = "E|Opps!... Something went wrong. " + JsonConvert.SerializeObject(ex) + "|0";
					}
				}
			}

			if (!string.IsNullOrEmpty(response) && response.Contains("|"))
			{
				var msgtype = response.Split('|').Length > 0 ? Convert.ToString(response.Split('|')[0]) : "";
				var message = response.Split('|').Length > 1 ? Convert.ToString(response.Split('|')[1]).Replace("\"", "") : "";

				Int64 strid = 0;
				if (Int64.TryParse(response.Split('|').Length > 2 ? Convert.ToString(response.Split('|')[2]).Replace("\"", "") : "0", out strid)) { }
				string paths = response.Split('|').Length > 3 ? response.Split('|')[3].Replace("\"", "") : "0";


				return (msgtype.Contains("S"), message, strid, paths);
			}
			else
				return (false, ResponseStatusMessage.Error, 0, "0");
		}

		public static string ExecuteStoredProcedure(string sp, SqlParameter[] spCol)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					using (SqlCommand cmd = new SqlCommand(sp, conn))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						SqlParameter returnParameter = new SqlParameter("@response", SqlDbType.NVarChar, 1000);

						returnParameter.Direction = ParameterDirection.Output;

						if (spCol != null && spCol.Length > 0)
							cmd.Parameters.AddRange(spCol);


						cmd.Parameters.Add(returnParameter);

						conn.Open();
						cmd.ExecuteNonQuery();
						conn.Close();

						return returnParameter.Value.ToString();
					}
				}

			}
			catch (SqlException ex)
			{
				StringBuilder errorMessages = new StringBuilder();
				for (int i = 0; i < ex.Errors.Count; i++)
				{
					errorMessages.Append("Index #......" + i.ToString() + Environment.NewLine +
										 "Message:....." + ex.Errors[i].Message + Environment.NewLine +
										 "LineNumber:.." + ex.Errors[i].LineNumber + Environment.NewLine);
				}
				//Activity_Log.SendToDB("Database Oparation", "Error: " + "StoredProcedure: " + sp, ex);
				return "E|" + errorMessages.ToString();
			}
			catch (Exception ex)
			{
				//Activity_Log.SendToDB("Database Oparation", "Error: " + "StoredProcedure: " + sp, ex);
				return "E|" + ex.Message.ToString();
			}
		}

		public static bool ExecuteNonQuery_Delete(string query, List<SqlParameter> parameters = null)
		{
			try
			{
				using (SqlConnection con = new SqlConnection(_connectionString))
				{
					con.Open();

					SqlCommand cmd = con.CreateCommand();
					cmd.CommandType = CommandType.Text;
					cmd.CommandText = query;

					if (parameters != null)
						foreach (SqlParameter param in parameters)
							cmd.Parameters.Add(param);

					cmd.ExecuteNonQuery();
				}

				return true;
			}
			catch (Exception ex)
			{
				LogService.LogInsert("ExecuteNonQuery_Delete - DataContext", "", ex);
				return false;
			}
		}


		//public static List<Employee> Employee_Get(long id = 0, long Logged_In_VendorId = 0)
		//{
		//	DateTime? nullDateTime = null;
		//	var listObj = new List<Employee>();

		//	try
		//	{
		//		var parameters = new List<SqlParameter>();
		//		parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = id, Direction = ParameterDirection.Input, IsNullable = true });
		//		parameters.Add(new SqlParameter("VendorId", SqlDbType.BigInt) { Value = Logged_In_VendorId, Direction = ParameterDirection.Input, IsNullable = true });

		//		parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//		parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//		parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });

		//		var dt = ExecuteStoredProcedure_DataTable("SP_Employee_GET", parameters.ToList());

		//		if (dt != null && dt.Rows.Count > 0)
		//			foreach (DataRow dr in dt.Rows)
		//				listObj.Add(new Employee()
		//				{
		//					Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
		//					RoleId = dr["RoleId"] != DBNull.Value ? Convert.ToInt64(dr["RoleId"]) : 0,
		//					UserId = dr["UserId"] != DBNull.Value ? Convert.ToInt64(dr["UserId"]) : 0,
		//					VendorId = dr["VendorId"] != DBNull.Value ? Convert.ToInt64(dr["VendorId"]) : 0,
		//					UserName = dr["UserName"] != DBNull.Value ? Convert.ToString(dr["UserName"]) : "",
		//					FirstName = dr["FirstName"] != DBNull.Value ? Convert.ToString(dr["FirstName"]) : "",
		//					MiddleName = dr["MiddleName"] != DBNull.Value ? Convert.ToString(dr["MiddleName"]) : "",
		//					LastName = dr["LastName"] != DBNull.Value ? Convert.ToString(dr["LastName"]) : "",
		//					UserType = dr["UserType"] != DBNull.Value ? Convert.ToString(dr["UserType"]) : "",
		//					BirthDate = dr["BirthDate"] != DBNull.Value ? Convert.ToDateTime(dr["BirthDate"]) : nullDateTime,
		//					BirthDate_Text = dr["BirthDate_Text"] != DBNull.Value ? Convert.ToString(dr["BirthDate_Text"]) : "",
		//					IsActive = dr["IsActive"] != DBNull.Value ? Convert.ToBoolean(dr["IsActive"]) : false,
		//					CreatedBy = dr["CreatedBy"] != DBNull.Value ? Convert.ToInt64(dr["CreatedBy"]) : 0
		//				});
		//	}
		//	catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

		//	return listObj;
		//}

		//public static (bool, string, long) Employee_Save(Employee obj = null)
		//{
		//	if (obj != null)
		//		try
		//		{
		//			var parameters = new List<SqlParameter>();

		//			parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = obj.Id, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("UserId", SqlDbType.BigInt) { Value = obj.UserId, Direction = ParameterDirection.Input, IsNullable = true });
		//			//parameters.Add(new SqlParameter("RoleId", SqlDbType.BigInt) { Value = obj.RoleId, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("VendorId", SqlDbType.BigInt) { Value = obj.VendorId, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("UserName", SqlDbType.NVarChar) { Value = obj.UserName, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Password", SqlDbType.NVarChar) { Value = obj.Password, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("FirstName", SqlDbType.NVarChar) { Value = obj.FirstName, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("MiddleName", SqlDbType.NVarChar) { Value = obj.MiddleName, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("LastName", SqlDbType.NVarChar) { Value = obj.LastName, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("UserType", SqlDbType.NVarChar) { Value = obj.UserType, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("BirthDate", SqlDbType.NVarChar) { Value = obj.BirthDate?.ToString("dd/MM/yyyy"), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("IsActive", SqlDbType.NVarChar) { Value = obj.IsActive, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = obj.Id > 0 ? "UPDATE" : "INSERT", Direction = ParameterDirection.Input, IsNullable = true });

		//			var response = ExecuteStoredProcedure("SP_Employee_Save", parameters.ToArray());

		//			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
		//			var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
		//			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

		//			return (msgtype.Contains("S"), message, Convert.ToInt64(strid));

		//		}
		//		catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

		//	return (false, ResponseStatusMessage.Error, 0);
		//}

		//public static (bool, string) Employee_Status(long Id = 0, long Logged_In_VendorId = 0, bool IsActive = false, bool IsDelete = false)
		//{
		//	if (Id > 0)
		//		try
		//		{
		//			var parameters = new List<SqlParameter>();

		//			parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = Id, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("VendorId", SqlDbType.BigInt) { Value = Logged_In_VendorId, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("IsActive", SqlDbType.NVarChar) { Value = IsActive, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = IsDelete ? "DELETE" : "STATUS", Direction = ParameterDirection.Input, IsNullable = true });

		//			var response = ExecuteStoredProcedure("SP_Employee_Status", parameters.ToArray());

		//			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
		//			var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
		//			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

		//			return (msgtype.Contains("S"), message);

		//		}
		//		catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

		//	return (false, ResponseStatusMessage.Error);
		//}

		//public static List<Vendor> Vendor_Get(long id = 0)
		//{
		//	DateTime? nullDateTime = null;
		//	var listObj = new List<Vendor>();

		//	try
		//	{
		//		var parameters = new List<SqlParameter>();
		//		parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = id, Direction = ParameterDirection.Input, IsNullable = true });

		//		parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//		parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//		parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });

		//		var dt = ExecuteStoredProcedure_DataTable("SP_Vendor_GET", parameters.ToList());

		//		if (dt != null && dt.Rows.Count > 0)
		//			foreach (DataRow dr in dt.Rows)
		//				listObj.Add(new Vendor()
		//				{
		//					Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
		//					UserId = dr["UserId"] != DBNull.Value ? Convert.ToInt64(dr["UserId"]) : 0,
		//					UserName = dr["UserName"] != DBNull.Value ? Convert.ToString(dr["UserName"]) : "",
		//					FirstName = dr["FirstName"] != DBNull.Value ? Convert.ToString(dr["FirstName"]) : "",
		//					MiddleName = dr["MiddleName"] != DBNull.Value ? Convert.ToString(dr["MiddleName"]) : "",
		//					LastName = dr["LastName"] != DBNull.Value ? Convert.ToString(dr["LastName"]) : "",
		//					Email = dr["Email"] != DBNull.Value ? Convert.ToString(dr["Email"]) : "",
		//					ContactNo = dr["ContactNo"] != DBNull.Value ? Convert.ToString(dr["ContactNo"]) : "",
		//					ContactNo_Alternate = dr["ContactNo_Alternate"] != DBNull.Value ? Convert.ToString(dr["ContactNo_Alternate"]) : "",
		//					IsActive = dr["IsActive"] != DBNull.Value ? Convert.ToBoolean(dr["IsActive"]) : false,
		//					CreatedBy = dr["CreatedBy"] != DBNull.Value ? Convert.ToInt64(dr["CreatedBy"]) : 0
		//				});
		//	}
		//	catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

		//	return listObj;
		//}

		//public static (bool, string, long) Vendor_Save(Vendor obj = null)
		//{
		//	if (obj != null)
		//		try
		//		{
		//			var parameters = new List<SqlParameter>();

		//			parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = obj.Id, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("UserId", SqlDbType.BigInt) { Value = obj.UserId, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("UserName", SqlDbType.NVarChar) { Value = obj.UserName, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Password", SqlDbType.NVarChar) { Value = obj.Password, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("FirstName", SqlDbType.NVarChar) { Value = obj.FirstName, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("MiddleName", SqlDbType.NVarChar) { Value = obj.MiddleName, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("LastName", SqlDbType.NVarChar) { Value = obj.LastName, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Email", SqlDbType.NVarChar) { Value = obj.Email, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("ContactNo", SqlDbType.NVarChar) { Value = obj.ContactNo, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("ContactNo_Alternate", SqlDbType.NVarChar) { Value = obj.ContactNo_Alternate, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("IsActive", SqlDbType.NVarChar) { Value = obj.IsActive, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = obj.Id > 0 ? "UPDATE" : "INSERT", Direction = ParameterDirection.Input, IsNullable = true });

		//			var response = ExecuteStoredProcedure("SP_Vendor_Save", parameters.ToArray());

		//			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
		//			var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
		//			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

		//			return (msgtype.Contains("S"), message, Convert.ToInt64(strid));

		//		}
		//		catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

		//	return (false, ResponseStatusMessage.Error, 0);
		//}

		//public static (bool, string) Vendor_Status(long Id = 0, bool IsActive = false, bool IsDelete = false)
		//{
		//	if (Id > 0)
		//		try
		//		{
		//			var parameters = new List<SqlParameter>();

		//			parameters.Add(new SqlParameter("Id", SqlDbType.BigInt) { Value = Id, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("IsActive", SqlDbType.NVarChar) { Value = IsActive, Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_By", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_RoleId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.KEY_USER_ROLE_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Operated_MenuId", SqlDbType.BigInt) { Value = Common.Get_Session_Int(SessionKey.CURRENT_MENU_ID), Direction = ParameterDirection.Input, IsNullable = true });
		//			parameters.Add(new SqlParameter("Action", SqlDbType.NVarChar) { Value = IsDelete ? "DELETE" : "STATUS", Direction = ParameterDirection.Input, IsNullable = true });

		//			var response = ExecuteStoredProcedure("SP_Vendor_Status", parameters.ToArray());

		//			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
		//			var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
		//			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

		//			return (msgtype.Contains("S"), message);

		//		}
		//		catch (Exception ex) { /*LogService.LogInsert(GetCurrentAction(), "", ex);*/ }

		//	return (false, ResponseStatusMessage.Error);
		//}

	}

}
