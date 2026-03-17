using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using vector_editor.Data;
using vector_editor.Models;

namespace vector_editor.Services
{
    /// <summary>
    /// Сервис работы с файлами проекта.
    /// Отвечает за сохранение и загрузку документа в формате XML.
    /// </summary>
    public class Document_file_service
    {
        /// <summary>
        /// Сохраняет коллекцию фигур в XML-файл по указанному пути.
        /// </summary>
        public void Save_document_to_file(string file_path, IList<Figure_model> figure_models)
        {
            Vector_document_record document_record = new Vector_document_record();

            if (figure_models != null)
            {
                foreach (Figure_model current_figure_model in figure_models)
                {
                    if (current_figure_model == null)
                    {
                        continue;
                    }

                    document_record.figure_records.Add(current_figure_model.To_record());
                }
            }

            XmlSerializer xml_serializer = new XmlSerializer(typeof(Vector_document_record));

            using (FileStream file_stream = File.Create(file_path))
            {
                xml_serializer.Serialize(file_stream, document_record);
            }
        }

        /// <summary>
        /// Загружает список фигур из XML-файла.
        /// Возвращает пустой список, если файл не найден или пуст.
        /// </summary>
        public List<Figure_model> Load_document_from_file(string file_path)
        {
            if (!File.Exists(file_path))
            {
                return new List<Figure_model>();
            }

            XmlSerializer xml_serializer = new XmlSerializer(typeof(Vector_document_record));

            using (FileStream file_stream = File.OpenRead(file_path))
            {
                Vector_document_record document_record =
                    xml_serializer.Deserialize(file_stream) as Vector_document_record;

                List<Figure_model> loaded_figure_models = new List<Figure_model>();

                if (document_record == null || document_record.figure_records == null)
                {
                    return loaded_figure_models;
                }

                // Восстанавливаем каждую фигуру через маппер
                foreach (Figure_record current_figure_record in document_record.figure_records)
                {
                    Figure_model figure_model =
                        Figure_mapper_service.Create_model_from_record(current_figure_record);

                    if (figure_model != null)
                    {
                        loaded_figure_models.Add(figure_model);
                    }
                }

                return loaded_figure_models;
            }
        }
    }
}
