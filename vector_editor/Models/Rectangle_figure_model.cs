using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using vector_editor.Data;

namespace vector_editor.Models
{
    /// <summary>
    /// Модель прямоугольника. Хранит координаты левого верхнего угла,
    /// ширину и высоту. Умеет создавать и обновлять WPF-элемент Rectangle.
    /// </summary>
    public class Rectangle_figure_model : Figure_model
    {
        public double left { get; set; }
        public double top { get; set; }
        public double width { get; set; }
        public double height { get; set; }

        /// <summary>Создает визуальный объект прямоугольника на Canvas.</summary>
        public override Shape Create_shape_visual()
        {
            Rectangle rectangle_shape = new Rectangle();
            Apply_to_shape_visual(rectangle_shape);
            return rectangle_shape;
        }

        /// <summary>Обновляет позицию, размер и стиль прямоугольника.</summary>
        public override void Apply_to_shape_visual(Shape shape_visual)
        {
            if (shape_visual == null)
            {
                return;
            }

            Apply_common_style(shape_visual);
            shape_visual.Fill = Brushes.Transparent;
            shape_visual.Width = Math.Max(1.0, width);
            shape_visual.Height = Math.Max(1.0, height);
            Canvas.SetLeft(shape_visual, left);
            Canvas.SetTop(shape_visual, top);
        }

        /// <summary>Строит прямоугольник по начальной и текущей точкам мыши.</summary>
        public override void Update_geometry_from_drag(Point start_point, Point current_point)
        {
            left = Math.Min(start_point.X, current_point.X);
            top = Math.Min(start_point.Y, current_point.Y);
            width = Math.Max(1.0, Math.Abs(current_point.X - start_point.X));
            height = Math.Max(1.0, Math.Abs(current_point.Y - start_point.Y));
        }

        /// <summary>Сдвигает прямоугольник на заданное смещение.</summary>
        public override void Move_by(double delta_x, double delta_y)
        {
            left += delta_x;
            top += delta_y;
        }

        /// <summary>Устанавливает позицию и размер из панели свойств.</summary>
        public override void Set_position_and_size(double position_x, double position_y,
            double size_width, double size_height)
        {
            left = position_x;
            top = position_y;
            width = Math.Max(1.0, size_width);
            height = Math.Max(1.0, size_height);
        }

        /// <summary>Возвращает текущие координаты и размеры прямоугольника.</summary>
        public override void Get_position_and_size(out double position_x, out double position_y,
            out double size_width, out double size_height)
        {
            position_x = left;
            position_y = top;
            size_width = width;
            size_height = height;
        }

        /// <summary>Формирует DTO-запись прямоугольника для сериализации.</summary>
        public override Figure_record To_record()
        {
            return new Figure_record
            {
                kind_code = "rectangle",
                first_x = left,
                first_y = top,
                second_x = width,
                second_y = height,
                stroke_color_hex = Normalize_color_hex(stroke_color_hex),
                stroke_thickness = Math.Max(1.0, stroke_thickness)
            };
        }

        /// <summary>Возвращает название типа фигуры для отображения в UI.</summary>
        public override string Get_figure_name()
        {
            return "Прямоугольник";
        }
    }
}
