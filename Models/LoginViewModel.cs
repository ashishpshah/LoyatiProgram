using System.ComponentModel.DataAnnotations;

namespace Seed_Admin
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "Please enter user name.")] public string UserName { get; set; }
		[Required(ErrorMessage = "Please enter password.")] public string Password { get; set; }
		public long Id { get; set; }
		public long PlantId { get; set; }
		public long RoleId { get; set; }

    }	
	public class ForgotPassword
	{
		public string User_Id { get; set; }
		public string User_Name { get; set; }
		public string PlantId { get; set; }
		public string Password { get; set; }
		public string User_Type { get; set; }
		public string Email { get; set; }
	}

	public class DashboardData
	{
		public int FG_GateInCount { get; set; }
		public int FG_GateInCountMonth { get; set; }
		public int FG_GateOutCount { get; set; }
		public int FG_GateOutCountMonth { get; set; }
        public int FG_CancelGateInCount { get; set; }
        public int FG_CancelGateInCountMonth { get; set; }

        public int RM_GateInCount { get; set; }
		public int RM_GateInCountMonth { get; set; }
		public int RM_GateOutCount { get; set; }
		public int RM_GateOutCountMonth { get; set; }
        public int RM_CancelGateInCount { get; set; }
        public int RM_CancelGateInCountMonth { get; set; }

        public int SP_GateInCount { get; set; }
		public int SP_GateInCountMonth { get; set; }
		public int SP_GateOutCount { get; set; }
		public int SP_GateOutCountMonth { get; set; }
        public int SP_CancelGateInCount { get; set; }
        public int SP_CancelGateInCountMonth { get; set; }

        public int OT_GateInCount { get; set; }
		public int OT_GateInCountMonth { get; set; }
		public int OT_GateOutCount { get; set; }
		public int OT_GateOutCountMonth { get; set; }
        public int OT_CancelGateInCount { get; set; }
        public int OT_CancelGateInCountMonth { get; set; }

        public int DsFgQtyLoadingPendingCount { get; set; }
		public int DsFgQtyLoadingPendingCountMonth { get; set; }
		public int DsFgShipperBoxDispCount { get; set; }
		public int DsFgShipperBoxDispCountMonth { get; set; }

		public int TgFgNoOfCount { get; set; }
		public int TgFgNoOfCountMonth { get; set; }
		public int DsSpQtyLoadingPendingCount { get; set; }
		public int DsSpQtyLoadingPendingCountMonth { get; set; }
		public int DsSpShipperBoxDispCount { get; set; }
		public int DsSpShipperBoxDispCountMonth { get; set; }

		public int TgSgNoOfCount { get; set; }
		public int TgSgNoOfCountMonth { get; set; }
		public int PoSoRmCount { get; set; }
		public int PoSoRmCountMonth { get; set; }
		public int RgRmNoOfCount { get; set; }
		public int RgRmNoOfCountMonth { get; set; }

		public int PoSoSpCount { get; set; }
		public int PoSoSpCountMonth { get; set; }
		public int SpNoOfCount { get; set; }
		public int SpNoOfCountMonth { get; set; }

		public int PoSoVendorPoCount { get; set; }
		public int PoSoVendorPoCountMonth { get; set; }

		public int QrGeneratedCount { get; set; }
		public int QrGeneratedCountMonth { get; set; }
		public int QrReceivedCount { get; set; }
		public int QrReceivedCountMonth { get; set; }
	}


	public class User_Log
	{
		public string USER_LOG_SYS_ID { get; set; }
		public string USER_SYS_ID { get; set; }
		public string FORM_NAME { get; set; }
		public string RECORD_ADD { get; set; }
		public string RECORD_MODIFY { get; set; }
		public string RECORD_VIEW { get; set; }
		public string REMARK { get; set; }
		public string STATION_ID { get; set; }
		public string PLANT_ID { get; set; }
		public string Created_DateTime { get; set; }
	}
}
