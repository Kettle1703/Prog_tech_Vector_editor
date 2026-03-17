using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using vector_editor.Enums;
using vector_editor.Models;
using vector_editor.Services;

namespace vector_editor
{
    /// <summary>
    /// Главное окно векторного редактора.
    /// Управляет рисованием, выбором и перемещением фигур на Canvas,
    /// панелью свойств, масштабированием и файловыми операциями (сохранение, загрузка, экспорт).
    /// </summary>
    public partial class MainWindow : Window
    {
        // --- Сервисы ---
        private readonly Document_file_service document_file_service;
        private readonly Canvas_export_service canvas_export_service;
        private readonly Svg_export_service svg_export_service;

        // --- Коллекции ---
        private readonly List<Figure_model> figure_models;
        private readonly Dictionary<Shape, Figure_model> shape_figure_map;

        // --- Масштабирование ---
        private readonly ScaleTransform canvas_scale_transform = new ScaleTransform(1.0, 1.0);

        // --- Визуальные стили кнопок ---
        private readonly Brush active_tool_brush;
        private readonly Brush inactive_tool_brush;

        // --- Состояние редактора ---
        private Tool_mode current_tool_mode;
        private Figure_model selected_figure_model;
        private Shape selected_shape_visual;
        private Figure_model drawing_figure_model;
        private Shape drawing_shape_visual;
        private Point drag_start_point;

        private bool is_drawing_in_progress;
        private bool is_moving_in_progress;
        private bool is_property_panel_updating;

        public MainWindow()
        {
            InitializeComponent();

            document_file_service = new Document_file_service();
            canvas_export_service = new Canvas_export_service();
            svg_export_service = new Svg_export_service();
            figure_models = new List<Figure_model>();
            shape_figure_map = new Dictionary<Shape, Figure_model>();
            active_tool_brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D7ECDE"));
            inactive_tool_brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEF2F6"));

            drawing_canvas_frame_border.LayoutTransform = canvas_scale_transform;
            stroke_color_combo_box.SelectedIndex = 0;
            Set_tool_mode(Tool_mode.Select);
            Update_zoom_label();
            Update_property_panel_state();
        }

        // ======================================================================
        //  Обработчики команд меню «Файл»
        // ======================================================================

        /// <summary>Создает новый пустой документ, очищая холст.</summary>
        private void New_document_button_click(object sender, RoutedEventArgs e)
        {
            Clear_document();
        }

        /// <summary>Открывает XML-файл проекта и загружает фигуры на холст.</summary>
        private void Open_document_button_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog
            {
                Filter = "Документ редактора (*.xml)|*.xml|Все файлы (*.*)|*.*",
                DefaultExt = "xml"
            };

            if (open_file_dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                List<Figure_model> loaded_figure_models =
                    document_file_service.Load_document_from_file(open_file_dialog.FileName);

                Clear_document();

                foreach (Figure_model current_figure_model in loaded_figure_models)
                {
                    Add_figure_to_canvas(current_figure_model);
                }

                Set_tool_mode(Tool_mode.Select);
            }
            catch (Exception exception_object)
            {
                MessageBox.Show(
                    "Не удалось открыть документ.\n" + exception_object.Message,
                    "Ошибка открытия",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>Сохраняет текущий документ в XML-файл.</summary>
        private void Save_document_button_click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save_file_dialog = new SaveFileDialog
            {
                Filter = "Документ редактора (*.xml)|*.xml|Все файлы (*.*)|*.*",
                DefaultExt = "xml",
                FileName = "drawing.xml"
            };

            if (save_file_dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                document_file_service.Save_document_to_file(save_file_dialog.FileName, figure_models);

                MessageBox.Show(
                    "Проект успешно сохранен.\n" + save_file_dialog.FileName,
                    "Сохранение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception_object)
            {
                MessageBox.Show(
                    "Не удалось сохранить документ.\n" + exception_object.Message,
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>Экспортирует содержимое холста в растровый PNG-файл.</summary>
        private void Export_png_button_click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save_file_dialog = new SaveFileDialog
            {
                Filter = "PNG изображение (*.png)|*.png",
                DefaultExt = "png",
                FileName = "drawing.png"
            };

            if (save_file_dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                // Временно убираем пунктир выделения, чтобы он не попал в экспорт
                bool has_selected_shape = selected_shape_visual != null;
                Apply_selection_visual(selected_shape_visual, false);

                canvas_export_service.Export_canvas_to_png(drawing_canvas, save_file_dialog.FileName);

                Apply_selection_visual(selected_shape_visual, has_selected_shape);

                MessageBox.Show(
                    "PNG успешно экспортирован.\n" + save_file_dialog.FileName,
                    "Экспорт PNG",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception_object)
            {
                MessageBox.Show(
                    "Не удалось экспортировать PNG.\n" + exception_object.Message,
                    "Ошибка экспорта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>Экспортирует фигуры в векторный SVG-файл.</summary>
        private void Export_svg_button_click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save_file_dialog = new SaveFileDialog
            {
                Filter = "SVG вектор (*.svg)|*.svg",
                DefaultExt = "svg",
                FileName = "drawing.svg"
            };

            if (save_file_dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                svg_export_service.Export_figures_to_svg(
                    figure_models,
                    drawing_canvas.Width,
                    drawing_canvas.Height,
                    save_file_dialog.FileName);

                MessageBox.Show(
                    "SVG успешно экспортирован.\n" + save_file_dialog.FileName,
                    "Экспорт SVG",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception_object)
            {
                MessageBox.Show(
                    "Не удалось экспортировать SVG.\n" + exception_object.Message,
                    "Ошибка экспорта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ======================================================================
        //  Обработчики переключения инструментов
        // ======================================================================

        /// <summary>Активирует режим выбора и перемещения фигур.</summary>
        private void Select_tool_button_click(object sender, RoutedEventArgs e)
        {
            Set_tool_mode(Tool_mode.Select);
        }

        /// <summary>Активирует инструмент рисования прямоугольника.</summary>
        private void Rectangle_tool_button_click(object sender, RoutedEventArgs e)
        {
            Set_tool_mode(Tool_mode.Draw_rectangle);
        }

        /// <summary>Активирует инструмент рисования эллипса.</summary>
        private void Ellipse_tool_button_click(object sender, RoutedEventArgs e)
        {
            Set_tool_mode(Tool_mode.Draw_ellipse);
        }

        /// <summary>Активирует инструмент рисования линии.</summary>
        private void Line_tool_button_click(object sender, RoutedEventArgs e)
        {
            Set_tool_mode(Tool_mode.Draw_line);
        }

        // ======================================================================
        //  Обработчики событий мыши на Canvas
        // ======================================================================

        /// <summary>
        /// Обрабатывает нажатие левой кнопки мыши на холсте.
        /// Если клик пришелся на фигуру — выбирает её и начинает перемещение.
        /// Если активен инструмент рисования — начинает создание новой фигуры.
        /// </summary>
        private void Drawing_canvas_mouse_left_button_down(object sender, MouseButtonEventArgs e)
        {
            Point current_point = e.GetPosition(drawing_canvas);
            Shape clicked_shape_visual =
                Get_shape_from_original_source(e.OriginalSource as DependencyObject);

            // Клик по существующей фигуре — выбираем и готовим перемещение
            if (clicked_shape_visual != null)
            {
                Select_figure_by_shape(clicked_shape_visual);

                if (current_tool_mode != Tool_mode.Select)
                {
                    Set_tool_mode(Tool_mode.Select);
                }

                is_moving_in_progress = true;
                drag_start_point = current_point;
                drawing_canvas.CaptureMouse();
                return;
            }

            // Клик по пустому месту в режиме выбора — снимаем выделение
            if (current_tool_mode == Tool_mode.Select)
            {
                Select_figure_by_shape(null);
                return;
            }

            // Клик по пустому месту с активным инструментом — рисуем новую фигуру
            Start_new_figure_drawing(current_point);
        }

        /// <summary>
        /// Обрабатывает движение мыши: обновляет геометрию рисуемой
        /// или позицию перемещаемой фигуры в реальном времени.
        /// </summary>
        private void Drawing_canvas_mouse_move(object sender, MouseEventArgs e)
        {
            Point current_point = e.GetPosition(drawing_canvas);

            if (is_drawing_in_progress && drawing_figure_model != null && drawing_shape_visual != null)
            {
                drawing_figure_model.Update_geometry_from_drag(drag_start_point, current_point);
                drawing_figure_model.Apply_to_shape_visual(drawing_shape_visual);
                Refresh_property_panel_from_selection();
                return;
            }

            if (is_moving_in_progress && selected_figure_model != null && selected_shape_visual != null)
            {
                double delta_x = current_point.X - drag_start_point.X;
                double delta_y = current_point.Y - drag_start_point.Y;

                selected_figure_model.Move_by(delta_x, delta_y);
                selected_figure_model.Apply_to_shape_visual(selected_shape_visual);

                drag_start_point = current_point;
                Refresh_property_panel_from_selection();
            }
        }

        /// <summary>Завершает операцию рисования или перемещения.</summary>
        private void Drawing_canvas_mouse_left_button_up(object sender, MouseButtonEventArgs e)
        {
            is_drawing_in_progress = false;
            is_moving_in_progress = false;
            drawing_figure_model = null;
            drawing_shape_visual = null;
            drawing_canvas.ReleaseMouseCapture();
        }

        // ======================================================================
        //  Масштабирование
        // ======================================================================

        /// <summary>Обрабатывает Ctrl + колесо мыши для масштабирования холста.</summary>
        private void Drawing_canvas_preview_mouse_wheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                return;
            }

            double zoom_step = e.Delta > 0 ? 0.1 : -0.1;
            double target_zoom = zoom_slider.Value + zoom_step;
            zoom_slider.Value = Math.Max(zoom_slider.Minimum, Math.Min(zoom_slider.Maximum, target_zoom));
            e.Handled = true;
        }

        /// <summary>Применяет новое значение масштаба при движении слайдера.</summary>
        private void Zoom_slider_value_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            canvas_scale_transform.ScaleX = zoom_slider.Value;
            canvas_scale_transform.ScaleY = zoom_slider.Value;
            Update_zoom_label();
        }

        /// <summary>Сбрасывает масштаб к 100%.</summary>
        private void Zoom_reset_button_click(object sender, RoutedEventArgs e)
        {
            zoom_slider.Value = 1.0;
        }

        // ======================================================================
        //  Панель свойств: обработчики
        // ======================================================================

        /// <summary>
        /// Считывает значения из текстовых полей панели свойств
        /// и применяет их к выбранной фигуре.
        /// </summary>
        private void Apply_properties_button_click(object sender, RoutedEventArgs e)
        {
            if (selected_figure_model == null || selected_shape_visual == null)
            {
                return;
            }

            double position_x;
            double position_y;
            double size_width;
            double size_height;

            if (!Try_parse_double(position_x_text_box.Text, out position_x)
                || !Try_parse_double(position_y_text_box.Text, out position_y)
                || !Try_parse_double(size_width_text_box.Text, out size_width)
                || !Try_parse_double(size_height_text_box.Text, out size_height))
            {
                MessageBox.Show(
                    "Проверьте числовые поля X, Y, ширины и высоты.",
                    "Некорректные данные",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            selected_figure_model.Set_position_and_size(position_x, position_y, size_width, size_height);
            selected_figure_model.stroke_color_hex = Get_selected_stroke_color_hex();
            selected_figure_model.stroke_thickness = stroke_thickness_slider.Value;
            selected_figure_model.Apply_to_shape_visual(selected_shape_visual);
            Apply_selection_visual(selected_shape_visual, true);
            Refresh_property_panel_from_selection();
        }

        /// <summary>Удаляет выбранную фигуру с холста и из коллекции.</summary>
        private void Delete_selected_figure_button_click(object sender, RoutedEventArgs e)
        {
            if (selected_figure_model == null || selected_shape_visual == null)
            {
                return;
            }

            drawing_canvas.Children.Remove(selected_shape_visual);
            shape_figure_map.Remove(selected_shape_visual);
            figure_models.Remove(selected_figure_model);
            Select_figure_by_shape(null);
        }

        /// <summary>Обновляет цвет обводки при смене значения в ComboBox.</summary>
        private void Stroke_color_combo_box_selection_changed(object sender, SelectionChangedEventArgs e)
        {
            if (is_property_panel_updating)
            {
                return;
            }

            if (selected_figure_model == null || selected_shape_visual == null)
            {
                return;
            }

            selected_figure_model.stroke_color_hex = Get_selected_stroke_color_hex();
            selected_figure_model.Apply_to_shape_visual(selected_shape_visual);
            Apply_selection_visual(selected_shape_visual, true);
        }

        /// <summary>Обновляет толщину обводки при движении слайдера.</summary>
        private void Stroke_thickness_slider_value_changed(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (stroke_thickness_value_text_block == null)
            {
                return;
            }

            stroke_thickness_value_text_block.Text = Format_number(stroke_thickness_slider.Value);

            if (is_property_panel_updating)
            {
                return;
            }

            if (selected_figure_model == null || selected_shape_visual == null)
            {
                return;
            }

            selected_figure_model.stroke_thickness = stroke_thickness_slider.Value;
            selected_figure_model.Apply_to_shape_visual(selected_shape_visual);
            Apply_selection_visual(selected_shape_visual, true);
        }

        // ======================================================================
        //  Внутренняя логика: переключение инструментов
        // ======================================================================

        /// <summary>Переключает режим работы редактора и обновляет UI кнопок.</summary>
        private void Set_tool_mode(Tool_mode new_tool_mode)
        {
            current_tool_mode = new_tool_mode;
            Update_tool_button_states();
            Update_current_mode_text();

            if (drawing_canvas != null)
            {
                drawing_canvas.Cursor = current_tool_mode == Tool_mode.Select
                    ? Cursors.Arrow
                    : Cursors.Cross;
            }
        }

        /// <summary>Обновляет фон и шрифт кнопок панели инструментов.</summary>
        private void Update_tool_button_states()
        {
            Apply_tool_button_state(select_tool_button, current_tool_mode == Tool_mode.Select);
            Apply_tool_button_state(rectangle_tool_button, current_tool_mode == Tool_mode.Draw_rectangle);
            Apply_tool_button_state(ellipse_tool_button, current_tool_mode == Tool_mode.Draw_ellipse);
            Apply_tool_button_state(line_tool_button, current_tool_mode == Tool_mode.Draw_line);
        }

        /// <summary>Применяет визуальное состояние (активна/неактивна) к кнопке инструмента.</summary>
        private void Apply_tool_button_state(Button tool_button, bool is_active)
        {
            if (tool_button == null)
            {
                return;
            }

            tool_button.Background = is_active ? active_tool_brush : inactive_tool_brush;
            tool_button.FontWeight = is_active ? FontWeights.SemiBold : FontWeights.Normal;
        }

        /// <summary>Обновляет текстовую подпись текущего режима в верхней панели.</summary>
        private void Update_current_mode_text()
        {
            if (current_mode_text_block == null)
            {
                return;
            }

            switch (current_tool_mode)
            {
                case Tool_mode.Draw_rectangle:
                    current_mode_text_block.Text = "Прямоугольник";
                    break;
                case Tool_mode.Draw_ellipse:
                    current_mode_text_block.Text = "Эллипс";
                    break;
                case Tool_mode.Draw_line:
                    current_mode_text_block.Text = "Линия";
                    break;
                default:
                    current_mode_text_block.Text = "Выбор";
                    break;
            }
        }

        // ======================================================================
        //  Внутренняя логика: рисование и управление фигурами
        // ======================================================================

        /// <summary>
        /// Создает новую фигуру по текущему инструменту и начинает
        /// процесс перетягивания для задания размера.
        /// </summary>
        private void Start_new_figure_drawing(Point start_point)
        {
            Figure_model new_figure_model = Figure_factory_service.Create_figure_for_tool(
                current_tool_mode,
                start_point,
                Get_selected_stroke_color_hex(),
                stroke_thickness_slider.Value);

            if (new_figure_model == null)
            {
                return;
            }

            Shape new_shape_visual = new_figure_model.Create_shape_visual();
            Add_figure_to_canvas(new_figure_model, new_shape_visual);

            drawing_figure_model = new_figure_model;
            drawing_shape_visual = new_shape_visual;
            drag_start_point = start_point;
            is_drawing_in_progress = true;
            drawing_canvas.CaptureMouse();
            Select_figure_by_shape(new_shape_visual);
        }

        /// <summary>Добавляет фигуру на холст, создавая для неё новый Shape.</summary>
        private void Add_figure_to_canvas(Figure_model figure_model)
        {
            if (figure_model == null)
            {
                return;
            }

            Shape shape_visual = figure_model.Create_shape_visual();
            Add_figure_to_canvas(figure_model, shape_visual);
        }

        /// <summary>Регистрирует пару модель-Shape в коллекциях и добавляет Shape на Canvas.</summary>
        private void Add_figure_to_canvas(Figure_model figure_model, Shape shape_visual)
        {
            if (figure_model == null || shape_visual == null)
            {
                return;
            }

            figure_models.Add(figure_model);
            shape_figure_map[shape_visual] = figure_model;
            drawing_canvas.Children.Add(shape_visual);
        }

        /// <summary>
        /// Поднимается по визуальному дереву от точки клика,
        /// чтобы найти элемент Shape (фигуру на Canvas).
        /// </summary>
        private Shape Get_shape_from_original_source(DependencyObject original_source)
        {
            DependencyObject current_source = original_source;

            while (current_source != null)
            {
                Shape shape_visual = current_source as Shape;

                if (shape_visual != null)
                {
                    return shape_visual;
                }

                if (current_source == drawing_canvas)
                {
                    break;
                }

                current_source = VisualTreeHelper.GetParent(current_source);
            }

            return null;
        }

        // ======================================================================
        //  Внутренняя логика: выделение фигур
        // ======================================================================

        /// <summary>
        /// Устанавливает выбранную фигуру, обновляет визуальное выделение
        /// и синхронизирует панель свойств. Передача null снимает выделение.
        /// </summary>
        private void Select_figure_by_shape(Shape shape_visual)
        {
            Apply_selection_visual(selected_shape_visual, false);

            if (shape_visual == null || !shape_figure_map.ContainsKey(shape_visual))
            {
                selected_shape_visual = null;
                selected_figure_model = null;
                Update_property_panel_state();
                return;
            }

            selected_shape_visual = shape_visual;
            selected_figure_model = shape_figure_map[shape_visual];
            Apply_selection_visual(selected_shape_visual, true);
            Update_property_panel_state();
        }

        /// <summary>Включает или выключает пунктирную рамку выделения для Shape.</summary>
        private void Apply_selection_visual(Shape shape_visual, bool is_selected)
        {
            if (shape_visual == null)
            {
                return;
            }

            shape_visual.StrokeDashArray = is_selected
                ? new DoubleCollection { 4.0, 2.0 }
                : null;
        }

        // ======================================================================
        //  Внутренняя логика: панель свойств
        // ======================================================================

        /// <summary>
        /// Включает или выключает элементы управления панели свойств
        /// в зависимости от наличия выбранной фигуры.
        /// </summary>
        private void Update_property_panel_state()
        {
            bool has_selected_figure = selected_figure_model != null;

            position_x_text_box.IsEnabled = has_selected_figure;
            position_y_text_box.IsEnabled = has_selected_figure;
            size_width_text_box.IsEnabled = has_selected_figure;
            size_height_text_box.IsEnabled = has_selected_figure;
            stroke_color_combo_box.IsEnabled = has_selected_figure;
            stroke_thickness_slider.IsEnabled = has_selected_figure;
            apply_properties_button.IsEnabled = has_selected_figure;
            delete_selected_figure_button.IsEnabled = has_selected_figure;

            if (!has_selected_figure)
            {
                is_property_panel_updating = true;
                selected_figure_name_text_block.Text = "Ничего не выбрано";
                position_x_text_box.Text = string.Empty;
                position_y_text_box.Text = string.Empty;
                size_width_text_box.Text = string.Empty;
                size_height_text_box.Text = string.Empty;
                Update_property_labels_for_figure(null);
                stroke_thickness_value_text_block.Text = Format_number(stroke_thickness_slider.Value);
                is_property_panel_updating = false;
                return;
            }

            Refresh_property_panel_from_selection();
        }

        /// <summary>Заполняет панель свойств текущими параметрами выбранной фигуры.</summary>
        private void Refresh_property_panel_from_selection()
        {
            if (selected_figure_model == null)
            {
                return;
            }

            is_property_panel_updating = true;

            double position_x;
            double position_y;
            double size_width;
            double size_height;
            selected_figure_model.Get_position_and_size(
                out position_x, out position_y, out size_width, out size_height);

            selected_figure_name_text_block.Text = selected_figure_model.Get_figure_name();
            position_x_text_box.Text = Format_number(position_x);
            position_y_text_box.Text = Format_number(position_y);
            size_width_text_box.Text = Format_number(size_width);
            size_height_text_box.Text = Format_number(size_height);

            Select_color_combo_box_item(selected_figure_model.stroke_color_hex);
            stroke_thickness_slider.Value = Math.Max(1.0, selected_figure_model.stroke_thickness);
            stroke_thickness_value_text_block.Text = Format_number(stroke_thickness_slider.Value);
            Update_property_labels_for_figure(selected_figure_model);

            is_property_panel_updating = false;
        }

        /// <summary>
        /// Меняет подписи полей ввода: для линии отображает X1/Y1/dX/dY,
        /// для остальных фигур — X/Y/Ширина/Высота.
        /// </summary>
        private void Update_property_labels_for_figure(Figure_model figure_model)
        {
            if (figure_model is Line_figure_model)
            {
                position_x_label_text_block.Text = "X1";
                position_y_label_text_block.Text = "Y1";
                size_width_label_text_block.Text = "dX";
                size_height_label_text_block.Text = "dY";
                return;
            }

            position_x_label_text_block.Text = "X";
            position_y_label_text_block.Text = "Y";
            size_width_label_text_block.Text = "Ширина";
            size_height_label_text_block.Text = "Высота";
        }

        // ======================================================================
        //  Вспомогательные методы
        // ======================================================================

        /// <summary>Возвращает HEX-код цвета из выбранного элемента ComboBox.</summary>
        private string Get_selected_stroke_color_hex()
        {
            ComboBoxItem selected_color_item = stroke_color_combo_box.SelectedItem as ComboBoxItem;

            if (selected_color_item == null || selected_color_item.Tag == null)
            {
                return "#1F2937";
            }

            return selected_color_item.Tag.ToString();
        }

        /// <summary>Выбирает в ComboBox пункт, соответствующий указанному HEX-цвету.</summary>
        private void Select_color_combo_box_item(string stroke_color_hex)
        {
            if (stroke_color_combo_box.Items.Count == 0)
            {
                return;
            }

            string normalized_color_hex = (stroke_color_hex ?? string.Empty).Trim();

            foreach (object current_item in stroke_color_combo_box.Items)
            {
                ComboBoxItem current_color_item = current_item as ComboBoxItem;

                if (current_color_item == null || current_color_item.Tag == null)
                {
                    continue;
                }

                if (string.Equals(current_color_item.Tag.ToString(), normalized_color_hex,
                    StringComparison.OrdinalIgnoreCase))
                {
                    stroke_color_combo_box.SelectedItem = current_color_item;
                    return;
                }
            }

            stroke_color_combo_box.SelectedIndex = 0;
        }

        /// <summary>Парсит строку в число, принимая и запятую, и точку как десятичный разделитель.</summary>
        private bool Try_parse_double(string value_text, out double parsed_value)
        {
            if (double.TryParse(value_text, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed_value))
            {
                return true;
            }

            string normalized_value_text = (value_text ?? string.Empty).Replace(',', '.');
            return double.TryParse(normalized_value_text, NumberStyles.Float,
                CultureInfo.InvariantCulture, out parsed_value);
        }

        /// <summary>Форматирует число для компактного отображения в UI.</summary>
        private string Format_number(double value_number)
        {
            return value_number.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>Полностью очищает документ: фигуры, выделение и состояние мыши.</summary>
        private void Clear_document()
        {
            figure_models.Clear();
            shape_figure_map.Clear();
            drawing_canvas.Children.Clear();

            is_drawing_in_progress = false;
            is_moving_in_progress = false;
            drawing_figure_model = null;
            drawing_shape_visual = null;

            drawing_canvas.ReleaseMouseCapture();
            Select_figure_by_shape(null);
        }

        /// <summary>Обновляет текстовую метку процента масштабирования.</summary>
        private void Update_zoom_label()
        {
            if (zoom_value_text_block == null)
            {
                return;
            }

            zoom_value_text_block.Text =
                (zoom_slider.Value * 100.0).ToString("0", CultureInfo.InvariantCulture) + "%";
        }
    }
}
