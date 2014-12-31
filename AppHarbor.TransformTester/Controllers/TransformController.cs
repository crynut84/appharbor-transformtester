using Microsoft.Web.XmlTransform;
using System;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace AppHarbor.TransformTester.Controllers
{
	[RequireHttps]
	public class TransformController : Controller
	{
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Create(string webConfigXml, string transformXml, bool beautify = default(bool))
		{
			try
			{
				using (var document = new XmlTransformableDocument())
				{
					document.PreserveWhitespace = true;
					document.LoadXml(webConfigXml);

					using (var transform = new XmlTransformation(transformXml, false, null))
					{
						if (transform.Apply(document))
						{
							var stringBuilder = new StringBuilder();
							var xmlWriterSettings = new XmlWriterSettings();
							xmlWriterSettings.Indent = true;
							xmlWriterSettings.IndentChars = new String(' ', 4);
							using (var xmlTextWriter = XmlTextWriter.Create(stringBuilder, xmlWriterSettings))
							{
								document.WriteTo(xmlTextWriter);
							}
                            string result = stringBuilder.ToString();
                            if (beautify)
                                result = this.Beautify(result);
							return Content(result, "text/xml");
						}
						else
						{
							return ErrorXml("Transformation failed for unkown reason");
						}
					}
				}
			}
			catch (XmlTransformationException xmlTransformationException)
			{
				return ErrorXml(xmlTransformationException.Message);
			}
			catch (XmlException xmlException)
			{
				return ErrorXml(xmlException.Message);
			}
		}

		private ContentResult ErrorXml(string errorMessage)
		{
			return Content(
					new XDocument(
						new XElement("error", errorMessage)
				).ToString(), "text/xml");
		}

        // http://blogs.msdn.com/b/erikaehrli/archive/2005/11/16/indentxmlfilesanddocuments.aspx
        private string Beautify(string content)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = new String(' ', 4);
            settings.NewLineChars = Environment.NewLine;
            settings.NewLineHandling = NewLineHandling.Replace;
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }
	}
}
