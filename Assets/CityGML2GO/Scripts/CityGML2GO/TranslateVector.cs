using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace Assets.Scripts.CityGML2GO
{
    public static class TranslateVector
    {
        public static Vector3 GetTranslateVectorFromFile(FileInfo file)
        {
            Vector3 fromV3 = Vector3.zero;
            Vector3 toV3 = Vector3.zero;
            using (XmlReader reader = XmlReader.Create(file.OpenRead(), new XmlReaderSettings { IgnoreWhitespace = true })) 
			{
				////
				///Added by Neil Romblon, September 2020
				string srsName = "";
				/// End of Addition
				////
				while (reader.Read())
                {
					if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Envelope") {
						srsName = reader.GetAttribute("srsName");
					}

                    if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "lowerCorner")
                    {
                        reader.Read();
                        var parts = reader.Value.Split(' ');
                        fromV3 = new Vector3((float)double.Parse(parts[0]), (float)double.Parse(parts[2]),
                            (float)double.Parse(parts[1]));

						////
						///Added by Neil Romblon, September 2020
						Coordinates fromPoint = new Coordinates(double.Parse(parts[0]), double.Parse(parts[1]),
							double.Parse(parts[2]), srsName);

						CityProperties.CheckSetRawMinPoint(fromPoint);
						/// End of Addition
						////

					}

					if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "upperCorner")
                    {
                        reader.Read();
                        var parts = reader.Value.Split(' ');
                        toV3 = new Vector3((float)double.Parse(parts[0]), (float)double.Parse(parts[2]),
                            (float)double.Parse(parts[1]));

						////
						///Added by Neil Romblon, September 2020
						Coordinates toPoint = new Coordinates(double.Parse(parts[0]), double.Parse(parts[1]),
							double.Parse(parts[2]), srsName);

						CityProperties.raw_MaxPoint = toPoint;
						CityProperties.CheckSetRawMaxPoint(toPoint);
						/// End of Addition
						////
					}

                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "boundedBy")
                    {
                        break;
                    }
                }
            }

            return -((fromV3 + toV3) / 2);
        }
    }
}
