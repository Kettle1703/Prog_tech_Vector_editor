using System.Collections.Generic;
using vector_editor.Models;

namespace vector_editor.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса векторного экспорта (паттерн Strategy).
    /// Определяет контракт экспорта фигур в формат SVG.
    /// </summary>
    public interface ISvg_export_service
    {
        /// <summary>Экспортирует коллекцию фигур в SVG-файл.</summary>
        void Export_figures_to_svg(IList<Figure_model> figure_models,
            double canvas_width, double canvas_height, string file_path);
    }
}
