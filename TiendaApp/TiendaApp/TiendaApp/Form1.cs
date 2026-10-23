using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TiendaApp.Models;

namespace TiendaApp
{
    public partial class Form1 : Form
    {
        private readonly TiendaContext _context = new TiendaContext();

        private readonly TextBox txtNombre = new TextBox();
        private readonly TextBox txtPrecio = new TextBox();
        private readonly ComboBox cmbCategorias = new ComboBox();
        private readonly DataGridView dgvProductos = new DataGridView();
        private readonly Button btnGuardar = new Button();
        private readonly Button btnEditar = new Button();
        private readonly Button btnEliminar = new Button();
        private readonly Button btnLimpiar = new Button();

        private int _productoIdSeleccionado;

        public Form1()
        {
            CrearInterfaz();
            Load += Form1_Load;
            FormClosed += (_, _) => _context.Dispose();
        }

private void CrearInterfaz()
        {
            Text = "TiendaApp - Gestión de productos";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(850, 550);
            Size = new Size(1000, 650);

            var lblTitulo = new Label
            {
                Text = "GESTIÓN DE PRODUCTOS",
                Dock = DockStyle.Top,
                Height = 55,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblNombre = new Label { Text = "Nombre del producto:", AutoSize = true };
            var lblPrecio = new Label { Text = "Precio:", AutoSize = true };
            var lblCategoria = new Label { Text = "Categoría:", AutoSize = true };

            txtNombre.Dock = DockStyle.Fill;
            txtPrecio.Dock = DockStyle.Fill;
            cmbCategorias.Dock = DockStyle.Fill;
            cmbCategorias.DropDownStyle = ComboBoxStyle.DropDownList;

            btnGuardar.Text = "Guardar";
            btnEditar.Text = "Editar";
            btnEliminar.Text = "Eliminar";
            btnLimpiar.Text = "Limpiar";

            foreach (var button in new[] { btnGuardar, btnEditar, btnEliminar, btnLimpiar })
            {
                button.AutoSize = true;
                button.MinimumSize = new Size(110, 36);
                button.Margin = new Padding(6);
            }

            btnGuardar.Click += btnGuardar_Click;
            btnEditar.Click += btnEditar_Click;
            btnEliminar.Click += btnEliminar_Click;
            btnLimpiar.Click += (_, _) => LimpiarFormulario();

            var campos = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(12)
            };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            campos.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            campos.Controls.Add(lblNombre, 0, 0);
            campos.Controls.Add(txtNombre, 1, 0);
            campos.Controls.Add(lblPrecio, 0, 1);
            campos.Controls.Add(txtPrecio, 1, 1);
            campos.Controls.Add(lblCategoria, 0, 2);
            campos.Controls.Add(cmbCategorias, 1, 2);

            var botones = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            botones.Controls.AddRange(new Control[] { btnGuardar, btnEditar, btnEliminar, btnLimpiar });

            var tablaBotones = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 8, 0, 8)
            };
            tablaBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tablaBotones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tablaBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tablaBotones.Controls.Add(botones, 1, 0);
            var panelSuperior = new Panel { Dock = DockStyle.Top, AutoSize = true };
            panelSuperior.Controls.Add(tablaBotones);
            panelSuperior.Controls.Add(campos);

            var lblLista = new Label
            {
                Text = "Lista de productos (selecciona una fila para editar o eliminar)",
                Dock = DockStyle.Top,
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            dgvProductos.Dock = DockStyle.Fill;
            dgvProductos.ReadOnly = true;
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.AllowUserToDeleteRows = false;
            dgvProductos.MultiSelect = false;
            dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProductos.BackgroundColor = Color.White;
            dgvProductos.BorderStyle = BorderStyle.Fixed3D;
            dgvProductos.RowHeadersVisible = false;
            dgvProductos.ColumnHeadersVisible = true;
            dgvProductos.EnableHeadersVisualStyles = false;
            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.LightSteelBlue;
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvProductos.RowTemplate.Height = 30;
            dgvProductos.GridColor = Color.LightGray;
            dgvProductos.CellClick += dgvProductos_CellClick;

            Controls.Add(dgvProductos);
            Controls.Add(lblLista);
            Controls.Add(panelSuperior);
            Controls.Add(lblTitulo);
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
          
                _context.Database.EnsureCreated();

                if (!_context.Categorias.Any())
                {
                    _context.Categorias.AddRange(
                        new Categoria { Nombre = "Electrónica" },
                        new Categoria { Nombre = "Ropa" });
                    _context.SaveChanges();
                }

                CargarCategorias();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo iniciar la base de datos.\n\n" +
                    "Comprueba que SQL Server Express LocalDB esté instalado.\n\nDetalle: " + ex.Message,
                    "Error de base de datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void CargarCategorias()
        {
            cmbCategorias.DataSource = _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ToList();
            cmbCategorias.DisplayMember = nameof(Categoria.Nombre);
            cmbCategorias.ValueMember = nameof(Categoria.Id);
        }

        private void CargarProductos()
        {
            var productos = _context.Productos
                .AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    Producto = p.Nombre,
                    p.Precio,
                    Categoria = p.Categoria.Nombre,
                    p.CategoriaId
                })
                .OrderBy(p => p.Producto)
                .ToList();

            dgvProductos.DataSource = productos;

            if (dgvProductos.Columns[nameof(Producto.Id)] != null)
            {
                dgvProductos.Columns[nameof(Producto.Id)]!.Visible = false;
            }

            if (dgvProductos.Columns[nameof(Producto.CategoriaId)] != null)
            {
                dgvProductos.Columns[nameof(Producto.CategoriaId)]!.Visible = false;
            }

            if (dgvProductos.Columns[nameof(Producto.Precio)] != null)
            {
                    dgvProductos.Columns[nameof(Producto.Precio)]!.DefaultCellStyle.Format = "C2";
            }
        }

        private bool ValidarCampos(out decimal precio)
        {
            precio = 0;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Escribe el nombre del producto.");
                txtNombre.Focus();
                return false;
            }

            var textoPrecio = txtPrecio.Text.Trim();
            if (!decimal.TryParse(textoPrecio, NumberStyles.Number, CultureInfo.CurrentCulture, out precio)
                && !decimal.TryParse(textoPrecio, NumberStyles.Number, CultureInfo.InvariantCulture, out precio))
            {
                MessageBox.Show("Escribe un precio válido, por ejemplo 12,50 o 12.50.");
                txtPrecio.Focus();
                return false;
            }

            if (precio < 0)
            {
                MessageBox.Show("El precio no puede ser negativo.");
                txtPrecio.Focus();
                return false;
            }

            if (cmbCategorias.SelectedValue == null)
            {
                MessageBox.Show("Selecciona una categoría.");
                return false;
            }

            return true;
        }

        private void btnGuardar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_productoIdSeleccionado != 0)
                {
                    MessageBox.Show("Pulsa Limpiar para crear un producto nuevo, o Editar para modificar el seleccionado.");
                    return;
                }

                if (!ValidarCampos(out decimal precio)) return;

                _context.Productos.Add(new Producto
                {
                    Nombre = txtNombre.Text.Trim(),
                    Precio = precio,
                    CategoriaId = Convert.ToInt32(cmbCategorias.SelectedValue)
                });

                _context.SaveChanges();
                CargarProductos();
                LimpiarFormulario();
                MessageBox.Show("Producto guardado correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar el producto.\n" + ex.Message);
            }
        }

        private void dgvProductos_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var fila = dgvProductos.Rows[e.RowIndex];
            _productoIdSeleccionado = Convert.ToInt32(fila.Cells["Id"].Value);
            txtNombre.Text = fila.Cells["Producto"].Value?.ToString() ?? "";
            txtPrecio.Text = Convert.ToDecimal(fila.Cells["Precio"].Value)
                .ToString(CultureInfo.CurrentCulture);
            cmbCategorias.SelectedValue = Convert.ToInt32(fila.Cells["CategoriaId"].Value);
        }

        private void btnEditar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_productoIdSeleccionado == 0)
                {
                    MessageBox.Show("Selecciona un producto de la tabla para editarlo.");
                    return;
                }

                if (!ValidarCampos(out decimal precio)) return;

                var producto = _context.Productos.Find(_productoIdSeleccionado);
                if (producto == null)
                {
                    MessageBox.Show("El producto ya no existe.");
                    CargarProductos();
                    LimpiarFormulario();
                    return;
                }

                producto.Nombre = txtNombre.Text.Trim();
                producto.Precio = precio;
                producto.CategoriaId = Convert.ToInt32(cmbCategorias.SelectedValue);

                _context.SaveChanges();
                CargarProductos();
                LimpiarFormulario();
                MessageBox.Show("Producto actualizado correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo actualizar el producto.\n" + ex.Message);
            }
        }

        private void btnEliminar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_productoIdSeleccionado == 0)
                {
                    MessageBox.Show("Selecciona un producto de la tabla para eliminarlo.");
                    return;
                }

                var confirmacion = MessageBox.Show(
                    "¿Seguro que quieres eliminar este producto?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmacion != DialogResult.Yes) return;

                var producto = _context.Productos.Find(_productoIdSeleccionado);
                if (producto != null)
                {
                    _context.Productos.Remove(producto);
                    _context.SaveChanges();
                }

                CargarProductos();
                LimpiarFormulario();
                MessageBox.Show("Producto eliminado.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar el producto.\n" + ex.Message);
            }
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtPrecio.Clear();

            if (cmbCategorias.Items.Count > 0)
                cmbCategorias.SelectedIndex = 0;

            _productoIdSeleccionado = 0;
            dgvProductos.ClearSelection();
            txtNombre.Focus();
        }
    }
}
