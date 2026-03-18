using vector_editor.Data;
using vector_editor.Interfaces;
using vector_editor.Models;

namespace vector_editor.Services
{
    /// <summary>
    /// Маппер фигур. Преобразует DTO-запись Figure_record из XML-файла
    /// обратно в рабочую модель Figure_model соответствующего типа (паттерн Data Mapper).
    /// </summary>
    public class Figure_mapper_service : IFigure_mapper
    {
        /// <summary>
        /// Создает модель фигуры по записи из файла.
        /// Возвращает null, если тип фигуры не распознан.
        /// </summary>
        public Figure_model Create_model_from_record(Figure_record figure_record)
        {
            if (figure_record == null || string.IsNullOrWhiteSpace(figure_record.kind_code))
            {
                return null;
            }

            Figure_model figure_model;

            // Определяем тип фигуры по строковому коду из записи
            switch (figure_record.kind_code.Trim().ToLowerInvariant())
            {
                case "rectangle":
                    Rectangle_figure_model rectangle_model = new Rectangle_figure_model();
                    rectangle_model.Set_position_and_size(
                        figure_record.first_x, figure_record.first_y,
                        figure_record.second_x, figure_record.second_y);
                    figure_model = rectangle_model;
                    break;

                case "ellipse":
                    Ellipse_figure_model ellipse_model = new Ellipse_figure_model();
                    ellipse_model.Set_position_and_size(
                        figure_record.first_x, figure_record.first_y,
                        figure_record.second_x, figure_record.second_y);
                    figure_model = ellipse_model;
                    break;

                case "line":
                    Line_figure_model line_model = new Line_figure_model
                    {
                        first_x = figure_record.first_x,
                        first_y = figure_record.first_y,
                        second_x = figure_record.second_x,
                        second_y = figure_record.second_y
                    };
                    figure_model = line_model;
                    break;

                default:
                    return null;
            }

            // Восстанавливаем параметры обводки с проверкой на корректность
            figure_model.stroke_color_hex = string.IsNullOrWhiteSpace(figure_record.stroke_color_hex)
                ? "#1F2937"
                : figure_record.stroke_color_hex;

            figure_model.stroke_thickness = figure_record.stroke_thickness <= 0.0
                ? 2.0
                : figure_record.stroke_thickness;

            return figure_model;
        }
    }
}
