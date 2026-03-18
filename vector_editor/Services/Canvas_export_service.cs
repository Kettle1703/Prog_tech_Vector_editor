using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using vector_editor.Interfaces;

namespace vector_editor.Services
{
    /// <summary>
    /// Сервис экспорта содержимого Canvas в растровое PNG-изображение.
    /// Реализует интерфейс ICanvas_export_service (паттерн Strategy).
    /// </summary>
    public class Canvas_export_service : ICanvas_export_service
    {
        /// <summary>
        /// Рендерит WPF-элемент в PNG-файл с разрешением 96 dpi.
        /// </summary>
        public void Export_canvas_to_png(FrameworkElement drawing_element, string file_path)
        {
            if (drawing_element == null)
            {
                return;
            }

            drawing_element.UpdateLayout();

            int bitmap_width = (int)Math.Ceiling(drawing_element.ActualWidth);
            int bitmap_height = (int)Math.Ceiling(drawing_element.ActualHeight);

            if (bitmap_width <= 0 || bitmap_height <= 0)
            {
                return;
            }

            // Рендерим элемент в растровое изображение в памяти
            RenderTargetBitmap render_target_bitmap = new RenderTargetBitmap(
                bitmap_width, bitmap_height, 96.0, 96.0, PixelFormats.Pbgra32);
            render_target_bitmap.Render(drawing_element);

            // Кодируем и сохраняем в файл формата PNG
            PngBitmapEncoder png_encoder = new PngBitmapEncoder();
            png_encoder.Frames.Add(BitmapFrame.Create(render_target_bitmap));

            using (FileStream file_stream = File.Create(file_path))
            {
                png_encoder.Save(file_stream);
            }
        }
    }
}
