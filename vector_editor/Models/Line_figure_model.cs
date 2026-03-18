using System;
using System.Windows;
using System.Windows.Shapes;
using vector_editor.Data;

namespace vector_editor.Models
{
    /// <summary>
    /// Модель линии. Хранит координаты двух конечных точек.
    /// Умеет создавать и обновлять WPF-элемент Line.
    /// </summary>
    public class Line_figure_model : Figure_model
    {
        public double first_x { get; set; }
        public double first_y { get; set; }
        public double second_x { get; set; }
        public double second_y { get; set; }

        /// <summary>Создает визуальный объект линии на Canvas.</summary>
        public override Shape Create_shape_visual()
        {
            Line line_shape = new Line();
            Apply_to_shape_visual(line_shape);
            return line_shape;
        }

        /// <summary>Обновляет координаты и стиль линии.</summary>
        public override void Apply_to_shape_visual(Shape shape_visual)
        {
            Line line_shape = shape_visual as Line;

            if (line_shape == null)
            {
                return;
            }

            Apply_common_style(line_shape);
            line_shape.X1 = first_x;
            line_shape.Y1 = first_y;
            line_shape.X2 = second_x;
            line_shape.Y2 = second_y;
        }

        /// <summary>Строит линию от точки начала до текущей точки мыши.</summary>
        public override void Update_geometry_from_drag(Point start_point, Point current_point)
        {
            first_x = start_point.X;
            first_y = start_point.Y;
            second_x = current_point.X;
            second_y = current_point.Y;
        }

        /// <summary>Сдвигает обе точки линии на заданное смещение.</summary>
        public override void Move_by(double delta_x, double delta_y)
        {
            first_x += delta_x;
            first_y += delta_y;
            second_x += delta_x;
            second_y += delta_y;
        }

        /// <summary>Задает начало линии и вектор (dX, dY) из панели свойств.</summary>
        public override void Set_position_and_size(double position_x, double position_y,
            double size_width, double size_height)
        {
            first_x = position_x;
            first_y = position_y;
            second_x = position_x + size_width;
            second_y = position_y + size_height;
        }

        /// <summary>Возвращает начало линии и её вектор (dX, dY).</summary>
        public override void Get_position_and_size(out double position_x, out double position_y,
            out double size_width, out double size_height)
        {
            position_x = first_x;
            position_y = first_y;
            size_width = second_x - first_x;
            size_height = second_y - first_y;
        }

        /// <summary>Формирует DTO-запись линии для сериализации.</summary>
        public override Figure_record To_record()
        {
            return new Figure_record
            {
                kind_code = "line",
                first_x = first_x,
                first_y = first_y,
                second_x = second_x,
                second_y = second_y,
                stroke_color_hex = Normalize_color_hex(stroke_color_hex),
                stroke_thickness = Math.Max(1.0, stroke_thickness)
            };
        }

        /// <summary>Возвращает название типа фигуры для отображения в UI.</summary>
        public override string Get_figure_name()
        {
            return "Линия";
        }
    }
}
