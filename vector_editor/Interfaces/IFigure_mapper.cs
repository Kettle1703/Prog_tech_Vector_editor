using vector_editor.Data;
using vector_editor.Models;

namespace vector_editor.Interfaces
{
    /// <summary>
    /// Интерфейс маппера фигур (паттерн Data Mapper).
    /// Определяет контракт преобразования DTO-записи в доменную модель.
    /// </summary>
    public interface IFigure_mapper
    {
        /// <summary>Создает модель фигуры по записи из файла.</summary>
        Figure_model Create_model_from_record(Figure_record figure_record);
    }
}
