using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using vector_editor.Data;


namespace vector_editor.Models
{
    /// <summary>
    /// Абстрактная базовая модель векторной фигуры.
    /// Определяет общий контракт для создания визуального представления,
    /// обновления геометрии при рисовании, перемещения и сериализации.
    /// Каждый конкретный тип фигуры наследует этот класс.
    /// </summary>
    public abstract class Figure_model
    {
        /// <summary>HEX-код цвета обводки (например, "#1F2937").</summary>
        public string stroke_color_hex { get; set; }

        /// <summary>Толщина линии обводки в пикселях.</summary>
        public double stroke_thickness { get; set; }

        protected Figure_model()
        {
            stroke_color_hex = "#1F2937";
            stroke_thickness = 2.0;
        }

        /// <summary>Создает новый WPF-элемент Shape для отображения на Canvas.</summary>
        public abstract Shape Create_shape_visual();

        /// <summary>Применяет текущие параметры модели к существующему Shape.</summary>
        public abstract void Apply_to_shape_visual(Shape shape_visual);

        /// <summary>Обновляет геометрию фигуры по координатам перетягивания мышью.</summary>
        public abstract void Update_geometry_from_drag(Point start_point, Point current_point);

        /// <summary>Сдвигает фигуру на заданное смещение.</summary>
        public abstract void Move_by(double delta_x, double delta_y);

        /// <summary>Задает позицию и размер фигуры из панели свойств.</summary>
        public abstract void Set_position_and_size(double position_x, double position_y,
            double size_width, double size_height);

        /// <summary>Возвращает текущие позицию и размер фигуры.</summary>
        public abstract void Get_position_and_size(out double position_x, out double position_y,
            out double size_width, out double size_height);

        /// <summary>Преобразует модель в DTO-запись для XML-сериализации.</summary>
        public abstract Figure_record To_record();

        /// <summary>Возвращает локализованное название типа фигуры для UI.</summary>
        public abstract string Get_figure_name();

        /// <summary>
        /// Создает кисть обводки из строки HEX-цвета.
        /// При ошибке формата возвращает черный цвет.
        /// </summary>
        protected Brush Build_stroke_brush()
        {
            try
            {
                return (SolidColorBrush)new BrushConverter().ConvertFromString(stroke_color_hex);
            }
            catch (Exception)
            {
                return Brushes.Black;
            }
        }

        /// <summary>
        /// Применяет общие параметры обводки (цвет и толщину) к Shape.
        /// </summary>
        protected void Apply_common_style(Shape shape_visual)
        {
            if (shape_visual == null)
            {
                return;
            }

            shape_visual.Stroke = Build_stroke_brush();
            shape_visual.StrokeThickness = Math.Max(1.0, stroke_thickness);
        }

        /// <summary>
        /// Нормализует строку HEX-цвета: проверяет формат и приводит к верхнему регистру.
        /// При некорректном значении возвращает цвет по умолчанию.
        /// </summary>
        public static string Normalize_color_hex(string source_color_hex)
        {
            if (string.IsNullOrWhiteSpace(source_color_hex))
            {
                return "#1F2937";
            }

            string trimmed_value = source_color_hex.Trim();

            if (!trimmed_value.StartsWith("#", StringComparison.Ordinal))
            {
                return "#1F2937";
            }

            if (trimmed_value.Length != 7 && trimmed_value.Length != 9)
            {
                return "#1F2937";
            }

            return trimmed_value.ToUpperInvariant();
        }
    }
}
