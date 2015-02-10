using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eventful.Models
{
	public class ModalDialogModel
	{
		public enum StandardButtons
		{
			Close,
			Cancel,
			OK,
			Save
		}
		public string ModalID { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public bool ShowOpenModalButton { get; set; }
		public ModalDialogButtonModel OpenModalButton { get; set; }
		public List<ModalDialogButtonModel> ButtonModels { get; set; }

		public ModalDialogModel()
		{
			ButtonModels = new List<ModalDialogButtonModel>();
		}

		public ModalDialogModel(string modalID, string title, string body, bool showOpenModalButton, ModalDialogButtonModel openModalButton) : this()
		{
			ModalID = modalID;
			Title = title;
			Body = body;
			ShowOpenModalButton = showOpenModalButton;
			OpenModalButton = openModalButton;
		}

		public ModalDialogButtonModel AddStandardButton(StandardButtons sb)
		{
			ModalDialogButtonModel b = new ModalDialogButtonModel();
			b.ButtonDismissesModal = true;
			b.Text = sb.ToString();
			switch (sb)
			{
				case StandardButtons.Cancel:
				case StandardButtons.Close:
					b.CssClass = "btn-default";
					break;
				case StandardButtons.OK:
				case StandardButtons.Save:
					b.CssClass = "btn-primary";
					break;
			}

			if (ButtonModels == null)
			{
				ButtonModels = new List<ModalDialogButtonModel>();
			}

			ButtonModels.Add(b);

			return b;
		}
	}
}