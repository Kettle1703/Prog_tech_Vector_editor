using System.Collections.Generic;
using vector_editor.Models;

namespace vector_editor.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса документов (паттерн Repository).
    /// Определяет контракт сохранения и загрузки проекта.
    /// </summary>
    public interface IDocument_service
    {
        /// <summary>Сохраняет коллекцию фигур в файл.</summary>
        void Save_document_to_file(string file_path, IList<Figure_model> figure_models);

        /// <summary>Загружает коллекцию фигур из файла.</summary>
        List<Figure_model> Load_document_from_file(string file_path);
    }
}
