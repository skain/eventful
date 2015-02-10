using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace eventful.Code
{
	public class ModalHelper : IDisposable
	{
		private int _curIndent;
		private TextWriter _writer = null;
		public ModalHelper(HtmlHelper Html, string id, string cssClass, string title, bool includeCloseButton)
		{
			_curIndent = 0;
			_writer = Html.ViewContext.Writer;
			_writer.WriteLine("{0}<div class='modal fade {1}' id='{2}' tabindex='-1' role='dialog' aria-hidden='true'>", getIndent(), cssClass, id);
			_curIndent++;
			_writer.WriteLine("{0}<div class='modal-dialog'>", getIndent());
			_curIndent++;
			_writer.WriteLine("{0}<div class='modal-content'>", getIndent());
			_curIndent++;
			if ((!string.IsNullOrWhiteSpace(title)) || includeCloseButton)
			{
				_writer.WriteLine("{0}<div class='modal-header'>", getIndent());
				_curIndent++;
				if (includeCloseButton)
				{
					_writer.WriteLine("{0}<button type='button' class='close' data-dismiss='modal' aria-hidden='true'>&times;</button>", getIndent());
				}

				if (!string.IsNullOrWhiteSpace(title))
				{
					_writer.WriteLine("{0}<h4 class='modal-title'>{1}</h4>", getIndent(), title);
				}
				_writer.WriteLine("{0}</div>", getIndent());
				_curIndent--;
			}
			_writer.WriteLine("{0}<div class='modal-body'>", getIndent());
			_curIndent++;

		}

		private string getIndent()
		{
			return new string('\t', _curIndent);
		}

		#region IDisposable Members

		public void Dispose()
		{
			_writer.WriteLine("{0}</div>", getIndent());
			_curIndent--;
			_writer.WriteLine("{0}</div>", getIndent());
			_curIndent--;
			_writer.WriteLine("{0}</div>", getIndent());
			_curIndent--;
			_writer.WriteLine("{0}</div>", getIndent());
			_curIndent--;
		}

		#endregion
	}
}
