using System.Windows;

namespace vector_editor.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса растрового экспорта (паттерн Strategy).
    /// Определяет контракт экспорта содержимого Canvas в PNG.
    /// </summary>
    public interface ICanvas_export_service
    {
        /// <summary>Экспортирует WPF-элемент в PNG-файл.</summary>
        void Export_canvas_to_png(FrameworkElement drawing_element, string file_path);
    }
}
