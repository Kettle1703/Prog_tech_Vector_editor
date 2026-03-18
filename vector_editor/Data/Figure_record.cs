namespace vector_editor.Data
{
    /// <summary>
    /// DTO-запись одной фигуры для XML-сериализации.
    /// Хранит тип фигуры, координаты, цвет и толщину обводки.
    /// </summary>
    public class Figure_record
    {
        public string kind_code { get; set; }

        public double first_x { get; set; }

        public double first_y { get; set; }

        public double second_x { get; set; }

        public double second_y { get; set; }

        public string stroke_color_hex { get; set; }

        public double stroke_thickness { get; set; }
    }
}
