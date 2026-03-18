using System.Windows;
using vector_editor.Enums;
using vector_editor.Models;

namespace vector_editor.Interfaces
{
    /// <summary>
    /// Интерфейс фабрики фигур (паттерн Factory Method).
    /// Определяет контракт создания фигуры по выбранному инструменту.
    /// </summary>
    public interface IFigure_factory
    {
        /// <summary>Создает фигуру нужного типа по режиму инструмента.</summary>
        Figure_model Create_figure_for_tool(Tool_mode tool_mode,
            Point start_point, string stroke_color_hex, double stroke_thickness);
    }
}
