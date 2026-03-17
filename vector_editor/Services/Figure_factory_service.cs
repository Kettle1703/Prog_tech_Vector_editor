using System.Windows;
using vector_editor.Enums;
using vector_editor.Models;

namespace vector_editor.Services
{
    /// <summary>
    /// Фабрика фигур. Создает экземпляр нужного типа модели
    /// в зависимости от выбранного инструмента рисования (паттерн Factory Method).
    /// </summary>
    public static class Figure_factory_service
    {
        /// <summary>
        /// Создает новую фигуру для указанного инструмента с заданными параметрами обводки.
        /// Возвращает null, если инструмент не предполагает рисование.
        /// </summary>
        public static Figure_model Create_figure_for_tool(Tool_mode tool_mode,
            Point start_point, string stroke_color_hex, double stroke_thickness)
        {
            Figure_model new_figure_model = null;

            switch (tool_mode)
            {
                case Tool_mode.Draw_rectangle:
                    new_figure_model = new Rectangle_figure_model();
                    break;

                case Tool_mode.Draw_ellipse:
                    new_figure_model = new Ellipse_figure_model();
                    break;

                case Tool_mode.Draw_line:
                    new_figure_model = new Line_figure_model();
                    break;
            }

            if (new_figure_model == null)
            {
                return null;
            }

            new_figure_model.stroke_color_hex = stroke_color_hex;
            new_figure_model.stroke_thickness = stroke_thickness;
            new_figure_model.Update_geometry_from_drag(start_point, start_point);
            return new_figure_model;
        }
    }
}
