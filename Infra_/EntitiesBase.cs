using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Seed_Admin.Infra
{
	public class EntitiesBase
	{
		//[Key, Column(Order = 1)]
		//[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public virtual long Id { get; set; }
		public virtual long CreatedBy { get; set; }
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
		public virtual Nullable<System.DateTime> CreatedDate { get; set; }
		public virtual long LastModifiedBy { get; set; }
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
		public virtual Nullable<System.DateTime> LastModifiedDate { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual bool IsDeleted { get; set; }
		[NotMapped] public virtual string CreatedDate_Text { get; set; }
		[NotMapped] public virtual string LastModifiedDate_Text { get; set; }
		[NotMapped] public virtual bool IsSetDefault { get; set; }

		public EntitiesBase()
		{
			CreatedDate_Text = CreatedDate != null ? CreatedDate?.ToString("dd/MM/yyyy").Replace("-", "/") : "";
			LastModifiedDate_Text = LastModifiedDate != null ? LastModifiedDate?.ToString("dd/MM/yyyy").Replace("-", "/") : "";
		}
	}
}
