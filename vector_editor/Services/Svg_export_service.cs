using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using vector_editor.Interfaces;
using vector_editor.Models;

namespace vector_editor.Services
{
    /// <summary>
    /// Сервис экспорта фигур в векторный формат SVG.
    /// Реализует интерфейс ISvg_export_service (паттерн Strategy).
    /// </summary>
    public class Svg_export_service : ISvg_export_service
    {
        /// <summary>
        /// Экспортирует коллекцию фигур в SVG-файл с указанными размерами холста.
        /// </summary>
        public void Export_figures_to_svg(IList<Figure_model> figure_models,
            double canvas_width, double canvas_height, string file_path)
        {
            if (string.IsNullOrWhiteSpace(file_path))
            {
                return;
            }

            double normalized_canvas_width = Math.Max(1.0, canvas_width);
            double normalized_canvas_height = Math.Max(1.0, canvas_height);

            XmlWriterSettings xml_writer_settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = true,
                IndentChars = "  "
            };

            using (FileStream file_stream = File.Create(file_path))
            using (XmlWriter xml_writer = XmlWriter.Create(file_stream, xml_writer_settings))
            {
                xml_writer.WriteStartDocument();
                Write_svg_root_element(xml_writer, normalized_canvas_width, normalized_canvas_height);

                if (figure_models != null)
                {
                    foreach (Figure_model current_figure_model in figure_models)
                    {
                        Write_figure_element(xml_writer, current_figure_model);
                    }
                }

                xml_writer.WriteEndElement();
                xml_writer.WriteEndDocument();
            }
        }

        /// <summary>Записывает корневой SVG-элемент с размерами и viewBox.</summary>
        private void Write_svg_root_element(XmlWriter xml_writer,
            double canvas_width, double canvas_height)
        {
            xml_writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
            xml_writer.WriteAttributeString("version", "1.1");
            xml_writer.WriteAttributeString("width", Format_number(canvas_width));
            xml_writer.WriteAttributeString("height", Format_number(canvas_height));
            xml_writer.WriteAttributeString("viewBox",
                "0 0 " + Format_number(canvas_width) + " " + Format_number(canvas_height));
        }

        /// <summary>Направляет запись фигуры в подходящий метод по типу модели.</summary>
        private void Write_figure_element(XmlWriter xml_writer, Figure_model figure_model)
        {
            if (xml_writer == null || figure_model == null)
            {
                return;
            }

            Rectangle_figure_model rectangle_model = figure_model as Rectangle_figure_model;
            if (rectangle_model != null)
            {
                Write_rectangle_element(xml_writer, rectangle_model);
                return;
            }

            Ellipse_figure_model ellipse_model = figure_model as Ellipse_figure_model;
            if (ellipse_model != null)
            {
                Write_ellipse_element(xml_writer, ellipse_model);
                return;
            }

            Line_figure_model line_model = figure_model as Line_figure_model;
            if (line_model != null)
            {
                Write_line_element(xml_writer, line_model);
            }
        }

        /// <summary>Записывает элемент rect в SVG.</summary>
        private void Write_rectangle_element(XmlWriter xml_writer,
            Rectangle_figure_model rectangle_model)
        {
            double normalized_width = Math.Max(1.0, rectangle_model.width);
            double normalized_height = Math.Max(1.0, rectangle_model.height);

            xml_writer.WriteStartElement("rect");
            xml_writer.WriteAttributeString("x", Format_number(rectangle_model.left));
            xml_writer.WriteAttributeString("y", Format_number(rectangle_model.top));
            xml_writer.WriteAttributeString("width", Format_number(normalized_width));
            xml_writer.WriteAttributeString("height", Format_number(normalized_height));
            xml_writer.WriteAttributeString("fill", "none");
            Write_stroke_attributes(xml_writer, rectangle_model.stroke_color_hex,
                rectangle_model.stroke_thickness);
            xml_writer.WriteEndElement();
        }

        /// <summary>Записывает элемент ellipse в SVG с вычислением центра и радиусов.</summary>
        private void Write_ellipse_element(XmlWriter xml_writer,
            Ellipse_figure_model ellipse_model)
        {
            double normalized_width = Math.Max(1.0, ellipse_model.width);
            double normalized_height = Math.Max(1.0, ellipse_model.height);
            double center_x = ellipse_model.left + normalized_width / 2.0;
            double center_y = ellipse_model.top + normalized_height / 2.0;

            xml_writer.WriteStartElement("ellipse");
            xml_writer.WriteAttributeString("cx", Format_number(center_x));
            xml_writer.WriteAttributeString("cy", Format_number(center_y));
            xml_writer.WriteAttributeString("rx", Format_number(normalized_width / 2.0));
            xml_writer.WriteAttributeString("ry", Format_number(normalized_height / 2.0));
            xml_writer.WriteAttributeString("fill", "none");
            Write_stroke_attributes(xml_writer, ellipse_model.stroke_color_hex,
                ellipse_model.stroke_thickness);
            xml_writer.WriteEndElement();
        }

        /// <summary>Записывает элемент line в SVG.</summary>
        private void Write_line_element(XmlWriter xml_writer, Line_figure_model line_model)
        {
            xml_writer.WriteStartElement("line");
            xml_writer.WriteAttributeString("x1", Format_number(line_model.first_x));
            xml_writer.WriteAttributeString("y1", Format_number(line_model.first_y));
            xml_writer.WriteAttributeString("x2", Format_number(line_model.second_x));
            xml_writer.WriteAttributeString("y2", Format_number(line_model.second_y));
            xml_writer.WriteAttributeString("fill", "none");
            Write_stroke_attributes(xml_writer, line_model.stroke_color_hex,
                line_model.stroke_thickness);
            xml_writer.WriteEndElement();
        }

        /// <summary>
        /// Записывает SVG-атрибуты обводки. Если цвет содержит альфа-канал (#AARRGGBB),
        /// разделяет его на stroke и stroke-opacity.
        /// </summary>
        private void Write_stroke_attributes(XmlWriter xml_writer,
            string stroke_color_hex, double stroke_thickness)
        {
            string normalized_color_hex = Figure_model.Normalize_color_hex(stroke_color_hex);
            xml_writer.WriteAttributeString("stroke-width",
                Format_number(Math.Max(1.0, stroke_thickness)));

            if (normalized_color_hex.Length == 9)
            {
                int alpha_value = Parse_hex_byte(normalized_color_hex.Substring(1, 2));
                string rgb_color_hex = "#" + normalized_color_hex.Substring(3, 6);
                xml_writer.WriteAttributeString("stroke", rgb_color_hex);
                xml_writer.WriteAttributeString("stroke-opacity",
                    Format_number(alpha_value / 255.0));
                return;
            }

            xml_writer.WriteAttributeString("stroke", normalized_color_hex);
        }

        /// <summary>Преобразует двухсимвольную HEX-строку в числовое значение 0..255.</summary>
        private int Parse_hex_byte(string hex_byte_text)
        {
            int parsed_value;

            if (!int.TryParse(hex_byte_text, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out parsed_value))
            {
                return 255;
            }

            return Math.Max(0, Math.Min(255, parsed_value));
        }

        /// <summary>Форматирует число для записи в SVG-атрибуты.</summary>
        private string Format_number(double value_number)
        {
            return value_number.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
