using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
#nullable disable

namespace a2n.DynData
{
    public class LiteExcelWriter
    {
        private const int ordA = 65; // ASCII code 'A'
        private const int ordZ = 90; // ASCII code 'Z'

        private static readonly Dictionary<string, string> dicExcelStrings = new Dictionary<string, string>();
        private static readonly Dictionary<int, ExcelFormater> dicExcelFormater = new Dictionary<int, ExcelFormater>();
        private static readonly DateTime originTime = new DateTime(1970, 1, 1);
        private static readonly Regex regexReplace = new Regex(@"[^\d\.\-]");
        private static readonly Regex regexNonStdCharReplace = new Regex(@"[\x00-\x09\x0B\x0C\x0E-\x1F\x7F-\x9F]");
        static LiteExcelWriter()
        {
            dicExcelStrings.Add("_rels/.rels", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
	<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>");
            dicExcelStrings.Add("xl/_rels/workbook.xml.rels", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
	<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
	<Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>
</Relationships>
");
            dicExcelStrings.Add("[Content_Types].xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
	<Default Extension=""xml"" ContentType=""application/xml"" />
	<Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml"" />
	<Default Extension=""jpeg"" ContentType=""image/jpeg"" />
	<Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"" />
	<Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"" />
	<Override PartName=""/xl/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"" />
</Types>");
            dicExcelStrings.Add("xl/workbook.xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
	<fileVersion appName=""xl"" lastEdited=""5"" lowestEdited=""5"" rupBuild=""24816""/>
	<workbookPr showInkAnnotation=""0"" autoCompressPictures=""0""/>
	<bookViews>
		<workbookView xWindow=""0"" yWindow=""0"" windowWidth=""25600"" windowHeight=""19020"" tabRatio=""500""/>
	</bookViews>
	<sheets>
		<sheet name=""Sheet1"" sheetId=""1"" r:id=""rId1""/>
	</sheets>
	<definedNames/>
</workbook>");
            dicExcelStrings.Add("xl/worksheets/sheet1.xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" mc:Ignorable=""x14ac"" xmlns:x14ac=""http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"">
	<sheetData/>
	<mergeCells count=""0""/>
</worksheet>");
            dicExcelStrings.Add("xl/styles.xml", @"<?xml version=""1.0"" encoding=""UTF-8""?>
<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" mc:Ignorable=""x14ac"" xmlns:x14ac=""http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"">
	<numFmts count=""6"">
		<numFmt numFmtId=""164"" formatCode=""#,##0.00_-\ [$$-45C]""/>
		<numFmt numFmtId=""165"" formatCode=""&quot;£&quot;#,##0.00""/>
		<numFmt numFmtId=""166"" formatCode=""[$€-2]\ #,##0.00""/>
		<numFmt numFmtId=""167"" formatCode=""0.0%""/>
		<numFmt numFmtId=""168"" formatCode=""#,##0;(#,##0)""/>
		<numFmt numFmtId=""169"" formatCode=""#,##0.00;(#,##0.00)""/>
	</numFmts>
	<fonts count=""5"" x14ac:knownFonts=""1"">
		<font>
			<sz val=""11"" />
			<name val=""Calibri"" />
		</font>
		<font>
			<sz val=""11"" />
			<name val=""Calibri"" />
			<color rgb=""FFFFFFFF"" />
		</font>
		<font>
			<sz val=""11"" />
			<name val=""Calibri"" />
			<b />
		</font>
		<font>
			<sz val=""11"" />
			<name val=""Calibri"" />
			<i />
		</font>
		<font>
			<sz val=""11"" />
			<name val=""Calibri"" />
			<u />
		</font>
	</fonts>
	<fills count=""6"">
		<fill>
			<patternFill patternType=""none"" />
		</fill>
		<fill> // Excel appears to use this as a dotted background regardless of values but
			<patternFill patternType=""none"" /> // to be valid to the schema, use a patternFill
		</fill>
		<fill>
			<patternFill patternType=""solid"">
				<fgColor rgb=""FFD9D9D9"" />
				<bgColor indexed=""64"" />
			</patternFill>
		</fill>
		<fill>
			<patternFill patternType=""solid"">
				<fgColor rgb=""FFD99795"" />
				<bgColor indexed=""64"" />
			</patternFill>
		</fill>
		<fill>
			<patternFill patternType=""solid"">
				<fgColor rgb=""ffc6efce"" />
				<bgColor indexed=""64"" />
			</patternFill>
		</fill>
		<fill>
			<patternFill patternType=""solid"">
				<fgColor rgb=""ffc6cfef"" />
				<bgColor indexed=""64"" />
			</patternFill>
		</fill>
	</fills>
	<borders count=""2"">
		<border>
			<left />
			<right />
			<top />
			<bottom />
			<diagonal />
		</border>
		<border diagonalUp=""false"" diagonalDown=""false"">
			<left style=""thin"">
				<color auto=""1"" />
			</left>
			<right style=""thin"">
				<color auto=""1"" />
			</right>
			<top style=""thin"">
				<color auto=""1"" />
			</top>
			<bottom style=""thin"">
				<color auto=""1"" />
			</bottom>
			<diagonal />
		</border>
	</borders>
	<cellStyleXfs count=""1"">
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" />
	</cellStyleXfs>
	<cellXfs count=""68"">
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""2"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""2"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""2"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""2"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""2"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""3"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""3"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""3"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""3"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""3"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""4"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""4"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""4"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""4"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""4"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""5"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""5"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""5"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""5"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""5"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""0"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""0"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""0"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""0"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""2"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""2"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""2"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""2"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""2"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""3"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""3"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""3"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""3"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""3"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""4"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""4"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""4"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""4"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""4"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""5"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""1"" fillId=""5"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""2"" fillId=""5"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""3"" fillId=""5"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""4"" fillId=""5"" borderId=""1"" applyFont=""1"" applyFill=""1"" applyBorder=""1""/>
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyAlignment=""1"">
			<alignment horizontal=""left""/>
		</xf>
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyAlignment=""1"">
			<alignment horizontal=""center""/>
		</xf>
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyAlignment=""1"">
			<alignment horizontal=""right""/>
		</xf>
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyAlignment=""1"">
			<alignment horizontal=""fill""/>
		</xf>
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyAlignment=""1"">
			<alignment textRotation=""90""/>
		</xf>
		<xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyAlignment=""1"">
			<alignment wrapText=""1""/>
		</xf>
		<xf numFmtId=""9""   fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""164"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""165"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""166"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""167"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""168"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""169"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""3"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""4"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""1"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""2"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
		<xf numFmtId=""14"" fontId=""0"" fillId=""0"" borderId=""0"" applyFont=""1"" applyFill=""1"" applyBorder=""1"" xfId=""0"" applyNumberFormat=""1""/>
	</cellXfs>
	<cellStyles count=""1"">
		<cellStyle name=""Normal"" xfId=""0"" builtinId=""0"" />
	</cellStyles>
	<dxfs count=""0"" />
	<tableStyles count=""0"" defaultTableStyle=""TableStyleMedium9"" defaultPivotStyle=""PivotStyleMedium4"" />
</styleSheet>");

            dicExcelFormater.Add(60, new ExcelFormater(@"^\-?\d+\.\d%$", 60, d => { return Convert.ToDouble(d) / 100d; })); // Precent with d.p.
            dicExcelFormater.Add(56, new ExcelFormater(@"^\-?\d+\.?\d*%$", 56, d => { return Convert.ToDouble(d) / 100d; }));  // Percent
            dicExcelFormater.Add(57, new ExcelFormater(@"^\-?\$[\d,]+.?\d*$", 57));     // Dollars
            dicExcelFormater.Add(58, new ExcelFormater(@"^\-?£[\d,]+.?\d*$", 58));      // Pounds
            dicExcelFormater.Add(59, new ExcelFormater(@"^\-?€[\d,]+.?\d*$", 59));      // Euros
            dicExcelFormater.Add(65, new ExcelFormater(@"^\-?\d+$", 65));               // Numbers without thousand separators
            dicExcelFormater.Add(66, new ExcelFormater(@"^\-?\d+\.\d{2}$", 66));        // Numbers 2 d.p. without thousands separators
            dicExcelFormater.Add(61, new ExcelFormater(@"^\([\d,]+\)$", 61, d => { return d != null ? Convert.ToDouble(d.ToString().Replace("(", "").Replace(")", "")) * -1d : null; }));           // Negative numbers indicated by brackets
            dicExcelFormater.Add(62, new ExcelFormater(@"^\([\d,]+\.\d{2}\)$", 62, d => { return d != null ? Convert.ToDouble(d.ToString().Replace("(", "").Replace(")", "")) * -1d : null; }));    // Negative numbers indicated by brackets - 2d.p.
            dicExcelFormater.Add(63, new ExcelFormater(@"^\-?[\d,]+$", 63));            // Numbers with thousand separators
            dicExcelFormater.Add(64, new ExcelFormater(@"^\-?[\d,]+\.\d{2}$", 64));     // Numbers 2 d.p. without thousands separators
            dicExcelFormater.Add(67, new ExcelFormater(@"^^[\d]{4}\-[\d]{2}\-[\d]{2}$", 67, d =>
            {
                var dt = DateTime.Parse(d.ToString());
                long diff = dt.Ticks - originTime.Ticks;
                var unixTimestamp = (diff / 10000);
                return Math.Round(25569d + (unixTimestamp / (86400 * 1000)));
            })); //Date yyyy-mm-dd
        }


        private int currentRowNo;
        private int currentColumnIndex;
        private Metadata[] metadataArr;
        private XDocument doc;
        private XNamespace ns;
        private XElement sheetDataEl;
        private XElement mergeCellsEl;

        private class ExcelFormater
        {
            public Func<object, object> func = null;

            public string match { get; set; }
            public int style { get; set; }
            public ExcelFormater()
            {
            }
            public ExcelFormater(string match, int style, Func<object, object> func = null)
            {
                this.match = match;
                this.style = style;
                this.func = func;
            }
        }

        private string CreateCellPos(int n)
        {
            decimal d = n;
            var len = ordZ - ordA + 1;
            var s = "";
            while (d >= 0)
            {
                byte intByte = BitConverter.GetBytes(Convert.ToInt32(d) % len + ordA)[0];
                byte[] bytes = new byte[] { intByte };
                s = Encoding.ASCII.GetString(bytes) + s;
                d = Math.Floor(d / len) - 1;
            }

            return s;
        }
        private void MergeCell(int rowNo, int startColIndex, int colSpan)
        {
            int currentCount = Convert.ToInt32(mergeCellsEl.Attribute("count").Value);
            currentCount++;
            var colRef = string.Format("{0}{1}:{2}{1}", CreateCellPos(startColIndex), rowNo, CreateCellPos(startColIndex + colSpan - 1));
            XElement mergeCellEl = new XElement(ns + "mergeCell", new XAttribute("ref", colRef));
            mergeCellsEl.SetAttributeValue("count", currentCount);
            mergeCellsEl.Add(mergeCellEl);
        }

        private XElement CreateRowData(object data)
        {
            currentColumnIndex = -1;
            currentRowNo++;
            XElement rowDataEl = new XElement(ns + "row", new XAttribute("r", currentRowNo));
            foreach (var metadata in metadataArr)
            {
                currentColumnIndex++;
                XElement cellEl = null;
                var cellId = CreateCellPos(currentColumnIndex) + currentRowNo.ToString();
                var value = metadata.PropertyInfo.GetValue(data, null);
                if (value == null)
                    continue;

                var valueString = string.Empty; value.ToString().Trim();
                foreach (var key in dicExcelFormater.Keys)
                {
                    var formater = dicExcelFormater[key];
                    var regex = new Regex(formater.match);
                    valueString = value.ToString().Trim();

                    if (!regex.IsMatch(valueString))
                        continue;

                    object valueFmt = valueString;
                    valueString = regexReplace.Replace(valueString, string.Empty);
                    if (formater.func != null)
                        valueFmt = formater.func(valueString);
                    cellEl = new XElement(ns + "c");
                    cellEl.SetAttributeValue("r", cellId);
                    cellEl.SetAttributeValue("s", formater.style);
                    cellEl.Add(new XElement(ns + "v", valueFmt));
                    break;
                }
                if (cellEl == null)
                {
                    if (IsNumeric(metadata.PropertyInfo))
                    {
                        cellEl = new XElement(ns + "c");
                        cellEl.SetAttributeValue("r", cellId);
                        cellEl.SetAttributeValue("t", "n");
                        cellEl.Add(new XElement(ns + "v", value));
                    }
                    else
                    {
                        valueString = regexNonStdCharReplace.Replace(valueString, string.Empty);
                        cellEl = new XElement(ns + "c");
                        cellEl.SetAttributeValue("r", cellId);
                        cellEl.SetAttributeValue("t", "inlineStr");
                        var childEl1 = new XElement(ns + "is");
                        var childEl2 = new XElement(ns + "t", valueString);
                        childEl1.Add(childEl2);
                        childEl2.SetAttributeValue(XNamespace.Xml + "space", "preserve");
                        cellEl.Add(childEl1);
                    }
                }

                rowDataEl.Add(cellEl);
            }

            sheetDataEl.Add(rowDataEl);
            return rowDataEl;
        }
        private XElement CreateRowTitle(string title, bool center)
        {
            currentRowNo++;
            var cellId = CreateCellPos(0) + currentRowNo.ToString();
            XElement rowDataEl = new XElement(ns + "row", new XAttribute("r", currentRowNo));
            var valueString = regexNonStdCharReplace.Replace(title, string.Empty);
            var cellEl = new XElement(ns + "c");
            cellEl.SetAttributeValue("r", cellId);
            cellEl.SetAttributeValue("t", "inlineStr");
            var childEl1 = new XElement(ns + "is");
            var childEl2 = new XElement(ns + "t", valueString);
            childEl1.Add(childEl2);
            childEl2.SetAttributeValue(XNamespace.Xml + "space", "preserve");

            cellEl.Add(childEl1);
            rowDataEl.Add(cellEl);
            if (center)
                cellEl.SetAttributeValue("s", "51");

            sheetDataEl.Add(rowDataEl);
            return rowDataEl;
        }
        private XElement CreateRowHeader(bool bold)
        {
            currentRowNo++;
            currentColumnIndex = 0;
            XElement rowDataEl = new XElement(ns + "row", new XAttribute("r", currentRowNo));
            
            foreach (var metadata in metadataArr)
            {
                var cellId = CreateCellPos(currentColumnIndex++) + currentRowNo.ToString();
                var valueString = regexNonStdCharReplace.Replace(metadata.FieldLabel, string.Empty);
                var cellEl = new XElement(ns + "c");
                cellEl.SetAttributeValue("r", cellId);
                cellEl.SetAttributeValue("t", "inlineStr");
                var childEl1 = new XElement(ns + "is");
                var childEl2 = new XElement(ns + "t", valueString);
                childEl1.Add(childEl2);
                childEl2.SetAttributeValue(XNamespace.Xml + "space", "preserve");
                cellEl.Add(childEl1);
                if (bold)
                    cellEl.SetAttributeValue("s", "2");

                rowDataEl.Add(cellEl);
            }

            sheetDataEl.Add(rowDataEl);
            return rowDataEl;
        }
        private void SetColumnWidth()
        {
            XElement cols = new XElement(ns + "cols");
            int val = 0;
            foreach (var metadata in metadataArr)
            {
                val++;
                XElement col = new XElement(ns + "col");
                col.SetAttributeValue("min", val);
                col.SetAttributeValue("max", val);
                col.SetAttributeValue("customWidth", 1);
                double width = metadata.FieldLabel.Length * 3.5d;
                col.SetAttributeValue("width", width);
                cols.Add(col);
            }

            doc.Root.AddFirst(cols);
        }
        public void Render<T>(IQueryable<T> query, Stream strm)
        {
            var metadataArr = typeof(T).GetProperties().Where(t => t.PropertyType.Namespace == "System").Select(t => new Metadata(t)).ToArray();
            Render(query, typeof(T), metadataArr,  strm);
        }

        public void Render<dynamic>(IQueryable<dynamic> query, Type valueType, Metadata[] metadataArr, Stream strm)
        {
            this.metadataArr = metadataArr;
            string title = valueType.Name.ToHumanReadable();
            doc = XDocument.Parse(dicExcelStrings["xl/worksheets/sheet1.xml"]);
            ns = doc.Root.GetDefaultNamespace();
            sheetDataEl = doc.Root.Element(ns + "sheetData");
            mergeCellsEl = doc.Root.Element(ns + "mergeCells");
            currentRowNo = 0;
            currentColumnIndex = -1;

            SetColumnWidth();
            CreateRowTitle(title, true);
            MergeCell(currentRowNo, 0, metadataArr.Length);
            CreateRowHeader(true);
            foreach (var data in query)
                CreateRowData(data);

            if (mergeCellsEl.Elements().Count() == 0)
                mergeCellsEl.RemoveAll();

            using (var archive = new ZipArchive(strm, ZipArchiveMode.Create, true))
            {
                foreach (var key in dicExcelStrings.Keys)
                {
                    var arcFile = archive.CreateEntry(key);
                    using (var entryStream = arcFile.Open())
                    {
                        using (var sw = new StreamWriter(entryStream))
                        {
                            if (key == "xl/worksheets/sheet1.xml")
                            {
                                sw.WriteLine(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>");
                                sw.Write(doc.ToString(SaveOptions.DisableFormatting));
                            }
                            else
                                sw.Write(dicExcelStrings[key]);
                        }
                        //if (key == "xl/worksheets/sheet1.xml")
                        //{
                        //    using (var xw = XmlWriter.Create(entryStream))
                        //    {
                        //        doc.WriteTo(xw);
                        //    }
                        //}
                        //else
                        //{
                        //    using (var sw = new StreamWriter(entryStream))
                        //    {
                        //        sw.Write(dicExcelStrings[key]);
                        //    }
                        //}
                    }
                }
            }
        }

        private bool IsNumeric(PropertyInfo pi)
        {
            var type = pi.PropertyType;
            if (type == typeof(Int16)
                || type == typeof(Int32)
                || type == typeof(Int64)
                || type == typeof(UInt16)
                || type == typeof(UInt32)
                || type == typeof(UInt64)
                || type == typeof(Single)
                || type == typeof(Double)
                || type == typeof(Decimal)
                || type == typeof(Int16?)
                || type == typeof(Int32?)
                || type == typeof(Int64?)
                || type == typeof(UInt16?)
                || type == typeof(UInt32?)
                || type == typeof(UInt64?)
                || type == typeof(Single?)
                || type == typeof(Double?)
                || type == typeof(Decimal?)
                )
                return true;
            else
                return false;
        }
    }
}
