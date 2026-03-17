using System.Collections.Generic;
using System.Xml.Serialization;

namespace vector_editor.Data
{
    /// <summary>
    /// Корневой DTO-объект документа для XML-сериализации.
    /// Содержит список записей всех фигур проекта.
    /// </summary>
    [XmlRoot("vector_document")]
    public class Vector_document_record
    {
        [XmlArray("figures")]
        [XmlArrayItem("figure")]
        public List<Figure_record> figure_records { get; set; }

        public Vector_document_record()
        {
            figure_records = new List<Figure_record>();
        }
    }
}
