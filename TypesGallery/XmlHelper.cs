using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Cryptosoft.TypesGallery
{
    public static class XmlHelper
    {
        public static void ChangeNodeValue(XmlNode currentNode, string requiredNodeName, string requiredAttrName, string value)
        {
            // Проходим по всем нодам
            foreach (XmlNode node in currentNode.ChildNodes)
            {
                // Если нода - запрошенная, идём по атрибутам, иначе рекурсивно идём дальше
                if (String.Compare(node.Name, requiredNodeName) != 0)
                {
                    ChangeNodeValue(node, requiredNodeName, requiredAttrName, value);
                }
                else
                {
                    foreach (XmlAttribute attr in node.Attributes)
                    {
                        if (String.Compare(requiredAttrName, attr.Name) == 0)
                        {
                            // Выставляем нужное значение
                            attr.Value = value;
                            break;
                        }
                    }

                    break;
                }
            }
        }

        public static void GetNodeValue(XmlNode currentNode, string requiredNodeName, string requiredAttrName, ref string value)
        {
            foreach (XmlNode node in currentNode.ChildNodes)
            {
                if (String.Compare(node.Name, requiredNodeName) != 0)
                {
                    GetNodeValue(node, requiredNodeName, requiredAttrName, ref value);
                }
                else
                {
                    foreach (XmlAttribute attr in node.Attributes)
                    {
                        if (String.Compare(requiredAttrName, attr.Name) == 0)
                        {
                            value = attr.Value;
                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}
