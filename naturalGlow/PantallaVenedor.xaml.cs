using naturalGlow.DAO;
using naturalGlow.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Globalization;

namespace naturalGlow
{
    public partial class PantallaVenedor : Window
    {
        private IDAO dao;

        private Producte producteSeleccionat;
        private Proveidor proveidorSeleccionat;
        private List<Producte> productesDisponibles = new List<Producte>();
        private List<LiniaFacturaProveidor> liniesFactura = new List<LiniaFacturaProveidor>();

        private Comanda comandaSeleccionada = null;

        private bool carregantComanda = false;

        public PantallaVenedor()
        {
            InitializeComponent();

            dao = new IDAOFirebaseImpl(
                @"C:\Treball Final\naturalGlow\naturalGlow\naturalGlow\natural.json",
                "naturalglow");

            Loaded += PantallaVenedor_Loaded;
        }
        private class LiniaFacturaVisual
        {
            public string NomProducte { get; set; }

            public int Quantitat { get; set; }

            public double PreuCompra { get; set; }

            public double Subtotal { get; set; }
        }
        #region Carregar Pantalla

        private async void PantallaVenedor_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await CarregarProductes();

                await CarregarProveidors();

                dpDataComanda.SelectedDate =
                    DateTime.Now;

                NetejarFactura();

                await CarregarComandesClients();
                await CarregarVendes();
                await CarregarClientsMissatges();
                await CarregarGrafics();

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar la pantalla:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Productes

        #region Carregar Productes

        private async Task CarregarProductes()
        {
            try
            {
                List<Producte> productes =
                    await dao.GetAllProductes();

                productesDisponibles =
                    productes;

                dgProductes.ItemsSource =
                    null;

                dgProductes.ItemsSource =
                    productes;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar els productes:\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        #endregion

        #region Crear Producte
        private async void BtnCrear_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                bool dadesCorrectes = ValidarDadesProducte(
                    out double preuCompra,
                    out double preuVenda,
                    out int stock,
                    out int stockMinim);

                if (dadesCorrectes)
                {
                    Producte nouProducte = CrearNouProducte(
                        preuCompra,
                        preuVenda,
                        stock,
                        stockMinim);

                    await dao.AddProducte(nouProducte);

                    MessageBox.Show(
                        "El producte s'ha creat correctament.",
                        "Producte creat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    NetejarFormulari();

                    await CarregarProductes();
                    await ActualitzarDadesDashboard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en crear el producte:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidarDadesProducte(
            out double preuCompra,
            out double preuVenda,
            out int stock,
            out int stockMinim)
        {
            preuCompra = 0;
            preuVenda = 0;
            stock = 0;
            stockMinim = 0;

            bool correcte = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MostrarErrorValidacio(
                    "Has d'introduir el nom del producte.",
                    "Dades incompletes");

                correcte = false;
            }
            else if (string.IsNullOrWhiteSpace(txtCategoria.Text))
            {
                MostrarErrorValidacio(
                    "Has d'introduir la categoria.",
                    "Dades incompletes");

                correcte = false;
            }
            else if (!double.TryParse(
                txtPreuCompra.Text,
                out preuCompra))
            {
                MostrarErrorValidacio(
                    "El preu de compra no és correcte.",
                    "Dada incorrecta");

                correcte = false;
            }
            else if (!double.TryParse(
                txtPreuVenda.Text,
                out preuVenda))
            {
                MostrarErrorValidacio(
                    "El preu de venda no és correcte.",
                    "Dada incorrecta");

                correcte = false;
            }
            else if (!int.TryParse(
                txtStock.Text,
                out stock))
            {
                MostrarErrorValidacio(
                    "El stock no és correcte.",
                    "Dada incorrecta");

                correcte = false;
            }
            else if (!int.TryParse(
                txtStockMinim.Text,
                out stockMinim))
            {
                MostrarErrorValidacio(
                    "El stock mínim no és correcte.",
                    "Dada incorrecta");

                correcte = false;
            }
            else if (preuCompra < 0 || preuVenda < 0 ||
                     stock < 0 || stockMinim < 0)
            {
                MostrarErrorValidacio(
                    "Els preus i les quantitats no poden ser negatius.",
                    "Dades incorrectes");

                correcte = false;
            }

            return correcte;
        }

        private void MostrarErrorValidacio(
            string missatge,
            string titol)
        {
            MessageBox.Show(
                missatge,
                titol,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private Producte CrearNouProducte(
            double preuCompra,
            double preuVenda,
            int stock,
            int stockMinim)
        {
            Producte nouProducte = new Producte(
                Guid.NewGuid().ToString(),
                txtNom.Text.Trim(),
                txtDescripcio.Text.Trim(),
                txtCategoria.Text.Trim(),
                preuCompra,
                preuVenda,
                stock,
                stockMinim,
                "");

            return nouProducte;
        }

        #endregion

        #region Seleccionar Producte

        private void DgProductes_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            try
            {
                Producte producte = ObtenirProducteSeleccionat();

                if (producte != null)
                {
                    producteSeleccionat = producte;
                    CarregarDadesProducteAlFormulari(producte);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en seleccionar el producte:\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private Producte ObtenirProducteSeleccionat()
        {
            Producte producte = dgProductes.SelectedItem as Producte;

            return producte;
        }

        private void CarregarDadesProducteAlFormulari(Producte producte)
        {
            txtNom.Text = producte.Nom;
            txtDescripcio.Text = producte.Descripcio;
            txtCategoria.Text = producte.Categoria;
            txtPreuCompra.Text = producte.PreuCompra.ToString("0.00");
            txtPreuVenda.Text = producte.PreuVenda.ToString("0.00");
            txtStock.Text = producte.Stock.ToString();
            txtStockMinim.Text = producte.StockMinim.ToString();
        }

        #endregion

        #region Modificar Producte
        private async void BtnModificar_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                bool dadesCorrectes = ValidarModificacioProducte(
                    out double preuCompra,
                    out double preuVenda,
                    out int stock,
                    out int stockMinim);

                if (producteSeleccionat == null)
                {
                    MostrarErrorValidacio(
                        "Selecciona un producte de la llista.",
                        "Producte no seleccionat");
                }
                else if (dadesCorrectes)
                {
                    ActualitzarDadesProducte(
                        preuCompra,
                        preuVenda,
                        stock,
                        stockMinim);

                    await dao.UpdateProducte(producteSeleccionat);

                    MessageBox.Show(
                        "El producte s'ha modificat correctament.",
                        "Producte modificat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    NetejarFormulari();

                    await CarregarProductes();
                    await ActualitzarDadesDashboard();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en modificar el producte:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidarModificacioProducte(
            out double preuCompra,
            out double preuVenda,
            out int stock,
            out int stockMinim)
        {
            preuCompra = 0;
            preuVenda = 0;
            stock = 0;
            stockMinim = 0;

            bool correcte = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MostrarErrorValidacio(
                    "Has d'introduir el nom del producte.",
                    "Dades incompletes");

                correcte = false;
            }
            else if (string.IsNullOrWhiteSpace(txtCategoria.Text))
            {
                MostrarErrorValidacio(
                    "Has d'introduir la categoria.",
                    "Dades incompletes");

                correcte = false;
            }
            else if (!double.TryParse(txtPreuCompra.Text, out preuCompra))
            {
                MostrarErrorValidacio(
                    "El preu de compra no és correcte.",
                    "Dada incorrecta");

                correcte = false;
            }
            else if (!double.TryParse(txtPreuVenda.Text, out preuVenda))
            {
                MostrarErrorValidacio(
                    "El preu de venda no és correcte.",
                    "Dada incorrecta");

                correcte = false;
            }
            else if (!int.TryParse(txtStock.Text, out stock))
            {
                MostrarErrorValidacio(
                    "El stock no és correcte.",
                    "Dada incorrecta");

                correcte = false;
            }
            else if (!int.TryParse(txtStockMinim.Text, out stockMinim))
            {
                MostrarErrorValidacio(
                    "El stock mínim no és correcte.",
                    "Dada incorrecta");

                correcte = false;
            }
            else if (preuCompra < 0 || preuVenda < 0 ||
                     stock < 0 || stockMinim < 0)
            {
                MostrarErrorValidacio(
                    "Els preus i les quantitats no poden ser negatius.",
                    "Dades incorrectes");

                correcte = false;
            }

            return correcte;
        }

        private void ActualitzarDadesProducte(
            double preuCompra,
            double preuVenda,
            int stock,
            int stockMinim)
        {
            producteSeleccionat.Nom = txtNom.Text.Trim();
            producteSeleccionat.Descripcio = txtDescripcio.Text.Trim();
            producteSeleccionat.Categoria = txtCategoria.Text.Trim();
            producteSeleccionat.PreuCompra = preuCompra;
            producteSeleccionat.PreuVenda = preuVenda;
            producteSeleccionat.Stock = stock;
            producteSeleccionat.StockMinim = stockMinim;
        }

        #endregion

        #region Eliminar Producte
        private async void BtnEliminar_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (producteSeleccionat == null)
                {
                    MostrarErrorValidacio(
                        "Selecciona un producte de la llista.",
                        "Producte no seleccionat");
                }
                else
                {
                    bool confirmacio = ConfirmarEliminacioProducte();

                    if (confirmacio)
                    {
                        await EliminarProducteSeleccionat();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en eliminar el producte:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ConfirmarEliminacioProducte()
        {
            MessageBoxResult resultat = MessageBox.Show(
                "Estàs segur que vols eliminar el producte?\n\n"
                + producteSeleccionat.Nom,
                "Eliminar producte",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            bool confirmacio = resultat == MessageBoxResult.Yes;

            return confirmacio;
        }

        private async Task EliminarProducteSeleccionat()
        {
            await dao.DeleteProducte(producteSeleccionat);

            MessageBox.Show(
                "El producte s'ha eliminat correctament.",
                "Producte eliminat",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            NetejarFormulari();

            await CarregarProductes();
            await ActualitzarDadesDashboard();
        }

        #endregion

        #region Netejar Producte
        private void NetejarFormulari()
        {
            txtNom.Clear();
            txtDescripcio.Clear();
            txtCategoria.Clear();
            txtPreuCompra.Clear();
            txtPreuVenda.Clear();
            txtStock.Clear();
            txtStockMinim.Clear();

            producteSeleccionat =null;

            dgProductes.SelectedItem = null;
        }

        #endregion

        #endregion

        #region Proveïdors
        private async Task CarregarProveidors()
        {
            try
            {
                List<Proveidor> proveidors =
                    await dao.GetAllProveidors();

                dgProveidors.ItemsSource =
                    proveidors;

                cmbProveidor.ItemsSource =
                    proveidors;

                cmbProveidor.DisplayMemberPath =
                    "NomEmpresa";

                cmbProveidor.SelectedValuePath =
                    "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar els proveïdors:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #region Afegir Proveïdor

        private async void BtnAfegirProveidor_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                bool dadesCorrectes = ValidarNomEmpresa();

                if (dadesCorrectes)
                {
                    Proveidor nouProveidor = CrearNouProveidor();

                    await dao.AddProveidor(nouProveidor);

                    MessageBox.Show(
                        "El proveïdor s'ha afegit correctament.",
                        "Proveïdor creat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    NetejarFormulariProveidor();

                    await CarregarProveidors();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en afegir el proveïdor:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidarNomEmpresa()
        {
            bool nomCorrecte = true;

            if (string.IsNullOrWhiteSpace(txtNomEmpresa.Text))
            {
                MessageBox.Show(
                    "Has d'introduir el nom de l'empresa.",
                    "Dades incompletes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                nomCorrecte = false;
            }

            return nomCorrecte;
        }

        private Proveidor CrearNouProveidor()
        {
            Proveidor nouProveidor = new Proveidor(
                Guid.NewGuid().ToString(),
                txtNomEmpresa.Text.Trim(),
                txtPersonaContacte.Text.Trim(),
                txtNifCif.Text.Trim(),
                txtAdrecaProveidor.Text.Trim(),
                txtTelefonProveidor.Text.Trim(),
                txtCorreuProveidor.Text.Trim(),
                txtFormaPagament.Text.Trim()
            );

            return nouProveidor;
        }

        #endregion

        private async void DgProveidors_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (dgProveidors.SelectedItem
                is Proveidor proveidor)
            {
                proveidorSeleccionat =
                    proveidor;

                txtNomEmpresa.Text =
                    proveidor.NomEmpresa;

                txtPersonaContacte.Text =
                    proveidor.Contacte;

                txtTelefonProveidor.Text =
                    proveidor.Telefon;

                txtCorreuProveidor.Text =
                    proveidor.Email;

                txtNifCif.Text =
                    proveidor.Nif;

                txtAdrecaProveidor.Text =
                    proveidor.Adreca;

                txtFormaPagament.Text =
                    proveidor.FormaPagament;

                cmbProveidor.SelectedValue =
                    proveidor.Id;

                await CarregarHistorialProveidor(
                    proveidor.Id);
            }
        }


        private async void CmbProveidor_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (cmbProveidor.SelectedItem
                is Proveidor proveidor)
            {
                proveidorSeleccionat =
                    proveidor;

                await CarregarHistorialProveidor(
                    proveidor.Id);
            }
        }


        #region Modificar Proveïdor

        private async void BtnModificarProveidor_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                bool proveidorSeleccionatCorrecte =
                    ValidarProveidorSeleccionat();

                if (proveidorSeleccionatCorrecte)
                {
                    bool dadesCorrectes = ValidarNomEmpresa();

                    if (dadesCorrectes)
                    {
                        ActualitzarDadesProveidor();

                        await dao.UpdateProveidor(
                            proveidorSeleccionat);

                        MessageBox.Show(
                            "El proveïdor s'ha modificat correctament.",
                            "Proveïdor modificat",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        NetejarFormulariProveidor();

                        await CarregarProveidors();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en modificar el proveïdor:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidarProveidorSeleccionat()
        {
            bool seleccionCorrecta = true;

            if (proveidorSeleccionat == null)
            {
                MessageBox.Show(
                    "Selecciona un proveïdor de la llista.",
                    "Proveïdor no seleccionat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                seleccionCorrecta = false;
            }

            return seleccionCorrecta;
        }

        private void ActualitzarDadesProveidor()
        {
            proveidorSeleccionat.NomEmpresa =
                txtNomEmpresa.Text.Trim();

            proveidorSeleccionat.Contacte =
                txtPersonaContacte.Text.Trim();

            proveidorSeleccionat.Telefon =
                txtTelefonProveidor.Text.Trim();

            proveidorSeleccionat.Email =
                txtCorreuProveidor.Text.Trim();

            proveidorSeleccionat.Nif =
                txtNifCif.Text.Trim();

            proveidorSeleccionat.Adreca =
                txtAdrecaProveidor.Text.Trim();

            proveidorSeleccionat.FormaPagament =
                txtFormaPagament.Text.Trim();
        }

        #endregion

        #region Eliminar Proveïdor

        private async void BtnEliminarProveidor_Click(
            object sender,
            RoutedEventArgs e)
        {
            bool proveidorCorrecte;
            bool confirmacio;
            try
            {
                proveidorCorrecte =
                    ValidarProveidorSeleccionat();

                if (proveidorCorrecte)
                {
                   confirmacio =
                        ConfirmarEliminacioProveidor();

                    if (confirmacio)
                    {
                        await EliminarProveidorSeleccionat();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en eliminar el proveïdor:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ConfirmarEliminacioProveidor()
        {
            bool confirmacio;

            MessageBoxResult resultat = MessageBox.Show(
                "Estàs segur que vols eliminar aquest proveïdor?\n\n"
                + proveidorSeleccionat.NomEmpresa,
                "Eliminar proveïdor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            confirmacio =
                resultat == MessageBoxResult.Yes;

            return confirmacio;
        }

        private async Task EliminarProveidorSeleccionat()
        {
            await dao.DeleteProveidor(proveidorSeleccionat);

            MessageBox.Show(
                "El proveïdor s'ha eliminat correctament.",
                "Proveïdor eliminat",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            NetejarFormulariProveidor();

            await CarregarProveidors();
        }

        #endregion

        private void NetejarFormulariProveidor()
        {
            txtNomEmpresa.Clear();
            txtPersonaContacte.Clear();
            txtTelefonProveidor.Clear();
            txtCorreuProveidor.Clear();
            txtNifCif.Clear();
            txtAdrecaProveidor.Clear();
            txtFormaPagament.Clear();

            proveidorSeleccionat =
                null;

            dgProveidors.SelectedItem =
                null;

            cmbProveidor.SelectedItem =
                null;

            dgHistorialComandes.ItemsSource =
                null;
        }

        #endregion

        #region Comanda Proveïdor

        private async void BtnAfegirProducteComanda_Click(object sender, RoutedEventArgs e)
        {
            int quantitat = 0;
            bool dadesCorrectes = false;
            Producte producte = null;

            try
            {
                dadesCorrectes = ValidarDadesProducteComanda(out quantitat);

                if (dadesCorrectes)
                {
                    productesDisponibles = await dao.GetAllProductes();
                    producte = BuscarProducteComanda();

                    if (producte != null)
                    {
                        AfegirLiniaComanda(producte, quantitat);
                        ActualitzarGraellaFactura();
                        ActualitzarTotalFactura();
                        NetejarCampsProducteComanda();
                    }
                    else
                    {
                        MostrarErrorProducteNoTrobat();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Hi ha hagut un error en afegir el producte:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidarDadesProducteComanda(out int quantitat)
        {
            bool dadesCorrectes = true;
            quantitat = 0;

            if (cmbProveidor.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecciona un proveïdor abans d'afegir productes.",
                    "Proveïdor no seleccionat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                dadesCorrectes = false;
            }

            if (dadesCorrectes && string.IsNullOrWhiteSpace(txtProducteComanda.Text))
            {
                MessageBox.Show(
                    "Introdueix el nom o l'ID del producte.",
                    "Producte",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                dadesCorrectes = false;
            }

            if (dadesCorrectes &&
                (!int.TryParse(txtQuantitatComanda.Text.Trim(), out quantitat) ||
                 quantitat <= 0))
            {
                MessageBox.Show(
                    "La quantitat ha de ser un número enter superior a 0.",
                    "Quantitat incorrecta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                dadesCorrectes = false;
            }

            return dadesCorrectes;
        }
        /// <summary>
        /// Busca el producte pel seu nom o id i el retorna si el troba.
        /// </summary>
        /// <returns></returns>
        private Producte BuscarProducteComanda()
        {
            string textProducte = txtProducteComanda.Text.Trim();

            Producte producte = productesDisponibles.FirstOrDefault(p =>
                (!string.IsNullOrWhiteSpace(p.Id) &&
                 p.Id.Trim().Equals(textProducte, StringComparison.OrdinalIgnoreCase))
                ||
                (!string.IsNullOrWhiteSpace(p.Nom) &&
                 p.Nom.Trim().Equals(textProducte, StringComparison.OrdinalIgnoreCase)));

            return producte;
        }

        private void MostrarErrorProducteNoTrobat()
        {
            MessageBox.Show(
                "No s'ha trobat el producte.\n\n" +
                "Escriu el NOM o l'ID exactament com apareix a la llista de Productes.",
                "Producte no trobat",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void AfegirLiniaComanda(Producte producte, int quantitat)
        {
            LiniaFacturaProveidor liniaExistenta =
                liniesFactura.FirstOrDefault(l => l.IdProducte == producte.Id);

            if (liniaExistenta != null)
            {
                liniaExistenta.Quantitat += quantitat;
            }
            else
            {
                LiniaFacturaProveidor novaLinia =
                    new LiniaFacturaProveidor(producte.Id, quantitat);

                liniesFactura.Add(novaLinia);
            }
        }

        private void ActualitzarGraellaFactura()
        {
            List<LiniaFacturaVisual> dadesVisuals =
                new List<LiniaFacturaVisual>();

            Producte producte;
            LiniaFacturaVisual visual;
            double subtotal;

            foreach (LiniaFacturaProveidor linia in liniesFactura)
            {
                producte = productesDisponibles.FirstOrDefault(
                    p => p.Id == linia.IdProducte);

                subtotal = producte != null
                    ? producte.PreuCompra * linia.Quantitat
                    : 0;

                visual = new LiniaFacturaVisual
                {
                    NomProducte = producte != null ? producte.Nom : linia.IdProducte,
                    Quantitat = linia.Quantitat,
                    PreuCompra = producte != null ? producte.PreuCompra : 0,
                    Subtotal = subtotal
                };

                dadesVisuals.Add(visual);
            }

            dgProductesComanda.ItemsSource = null;
            dgProductesComanda.ItemsSource = dadesVisuals;
        }

        private void ActualitzarTotalFactura()
        {
            double total = 0;
            Producte producte;

            foreach (LiniaFacturaProveidor linia in liniesFactura)
            {
                producte = productesDisponibles.FirstOrDefault(
                    p => p.Id == linia.IdProducte);

                if (producte != null)
                {
                    total += producte.PreuCompra * linia.Quantitat;
                }
            }

            txtTotalComanda.Text = total.ToString("0.00") + " €";
        }

        private void NetejarCampsProducteComanda()
        {
            txtProducteComanda.Clear();
            txtQuantitatComanda.Clear();
            txtPreuCompraComanda.Clear();
            txtProducteComanda.Focus();
        }

        private void BtnNetejarComanda_Click(object sender, RoutedEventArgs e)
        {
            NetejarFactura();
        }

        private void NetejarFactura()
        {
            liniesFactura.Clear();
            dgProductesComanda.ItemsSource = null;

            txtProducteComanda.Clear();
            txtQuantitatComanda.Clear();
            txtPreuCompraComanda.Clear();

            txtTotalComanda.Text = "0.00 €";
            dpDataComanda.SelectedDate = DateTime.Now;
        }


        private async void BtnGuardarComanda_Click(object sender, RoutedEventArgs e)
        {
            Proveidor proveidor = null;
            DateTime dataComanda = DateTime.MinValue;
            FacturaProveidor factura = null;

            bool proveidorCorrecte = false;
            bool liniesCorrectes = false;
            bool dataCorrecta = false;

            try
            {
                proveidorCorrecte = ValidarProveidorComanda(out proveidor);

                if (proveidorCorrecte)
                {
                    liniesCorrectes = ValidarLiniesComanda();
                }

                if (proveidorCorrecte && liniesCorrectes)
                {
                    dataCorrecta = ValidarDataComanda(out dataComanda);
                }

                if (proveidorCorrecte && liniesCorrectes && dataCorrecta)
                {
                    factura = CrearFacturaProveidor(proveidor, dataComanda);

                    await dao.AddFacturaProveidor(factura);

                    foreach (LiniaFacturaProveidor linia in liniesFactura)
                    {
                        await dao.AddLiniaFacturaProveidor(
                            new LiniaFacturaProveidor(
                                linia.IdProducte,
                                linia.Quantitat));
                    }

                    MessageBox.Show(
                        "La comanda al proveïdor i les seves línies " +
                        "s'han guardat correctament.",
                        "Comanda guardada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await CarregarHistorialProveidor(proveidor.Id);

                    NetejarFactura();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en guardar la comanda del proveïdor:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private bool ValidarProveidorComanda(out Proveidor proveidor)
        {
            bool proveidorCorrecte = true;
            proveidor = cmbProveidor.SelectedItem as Proveidor;

            if (proveidor == null)
            {
                MessageBox.Show("Selecciona un proveïdor.");
                proveidorCorrecte = false;
            }

            return proveidorCorrecte;
        }

        private bool ValidarLiniesComanda()
        {
            bool liniesCorrectes = true;

            if (liniesFactura == null || liniesFactura.Count == 0)
            {
                MessageBox.Show("Afegeix almenys un producte a la comanda.");
                liniesCorrectes = false;
            }

            return liniesCorrectes;
        }

        private bool ValidarDataComanda(out DateTime dataComanda)
        {
            bool dataCorrecta = true;
            dataComanda = DateTime.MinValue;

            if (!dpDataComanda.SelectedDate.HasValue)
            {
                MessageBox.Show("Selecciona una data per a la comanda.");
                dataCorrecta = false;
            }
            else
            {
                dataComanda = DateTime.SpecifyKind(
                    dpDataComanda.SelectedDate.Value,
                    DateTimeKind.Local).ToUniversalTime();
            }

            return dataCorrecta;
        }

        private FacturaProveidor CrearFacturaProveidor(
            Proveidor proveidor,
            DateTime dataComanda)
        {
            FacturaProveidor factura = new FacturaProveidor(
                Guid.NewGuid().ToString(),
                proveidor.Id,
                dataComanda,
                new List<LiniaFacturaProveidor>(liniesFactura));

            return factura;
        }

        #endregion

        #region Historial Proveïdor

        private async Task CarregarHistorialProveidor(string idProveidor)
        {
            bool idCorrecte = false;

            try
            {
                idCorrecte = ValidarIdProveidorHistorial(idProveidor);

                if (idCorrecte)
                {
                    List<FacturaProveidor> factures =
                        await dao.GetFacturesProveidorByProveidor(idProveidor);

                    List<FacturaProveidor> facturesOrdenades =
                        OrdenarFacturesProveidor(factures);

                    dgHistorialComandes.ItemsSource = facturesOrdenades;
                }
                else
                {
                    dgHistorialComandes.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar l'historial del proveïdor:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidarIdProveidorHistorial(string idProveidor)
        {
            bool idCorrecte = true;

            if (string.IsNullOrWhiteSpace(idProveidor))
            {
                idCorrecte = false;
            }

            return idCorrecte;
        }

        private List<FacturaProveidor> OrdenarFacturesProveidor(
            List<FacturaProveidor> factures)
        {
            List<FacturaProveidor> facturesOrdenades =
                factures
                    .OrderByDescending(f => f.Data)
                    .ToList();

            return facturesOrdenades;
        }

        #endregion

        #region COMANDES

        private async Task CarregarComandesClients()
        {
            List<Comanda> comandes;

            try
            {
                comandes = await dao.GetAllComandes();

                comandes = comandes
                    .OrderByDescending(c => c.Data)
                    .ToList();

                dgComandesClients.ItemsSource = comandes;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar les comandes:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void DgComandesClients_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            Comanda comanda;
            bool comandaCorrecta;

            try
            {
                comanda = dgComandesClients.SelectedItem as Comanda;
                comandaCorrecta = comanda != null;

                if (comandaCorrecta)
                {
                    carregantComanda = true;

                    comandaSeleccionada = comanda;

                    txtClientComanda.Text = comanda.IdClient;

                    dpDataComandaClient.SelectedDate =
                        comanda.Data.ToLocalTime();

                    CarregarEstatComandaSeleccionada(comanda.Estat);

                    cmbEstatComanda.IsEnabled = true;

                    await ActualitzarProductesComandaClient();

                    txtTotalComandaClient.Text =
                        $"Total: {comanda.Total:0.00} €";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar el detall de la comanda:\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                carregantComanda = false;
            }
        }

        private void CarregarEstatComandaSeleccionada(string estat)
        {
            ComboBoxItem itemSeleccionat = null;

            cmbEstatComanda.SelectedItem = null;

            foreach (ComboBoxItem item in cmbEstatComanda.Items)
            {
                if (item.Content.ToString() == estat)
                {
                    itemSeleccionat = item;
                    break;
                }
            }

            cmbEstatComanda.SelectedItem = itemSeleccionat;
        }

        private async void CmbEstatComanda_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            ComboBoxItem item;
            string nouEstat;
            MessageBoxResult resultat;
            bool potCanviarEstat;

            try
            {
                item = cmbEstatComanda.SelectedItem as ComboBoxItem;

                potCanviarEstat =
                    !carregantComanda &&
                    comandaSeleccionada != null &&
                    item != null;

                if (potCanviarEstat)
                {
                    nouEstat = item.Content.ToString();

                    if (comandaSeleccionada.Estat != nouEstat)
                    {
                        resultat = MessageBox.Show(
                            $"Vols canviar l'estat de la comanda " +
                            $"{comandaSeleccionada.Id} a:\n\n" +
                            $"{nouEstat}?",
                            "Canviar estat",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (resultat == MessageBoxResult.Yes)
                        {
                            await ActualitzarEstatComandaAsync(nouEstat);
                        }
                        else
                        {
                            carregantComanda = true;

                            CarregarEstatComandaSeleccionada(
                                comandaSeleccionada.Estat);

                            carregantComanda = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                carregantComanda = false;

                MessageBox.Show(
                    "Error en actualitzar l'estat:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task ActualitzarEstatComandaAsync(string nouEstat)
        {
            comandaSeleccionada.Estat = nouEstat;

            await dao.UpdateComanda(comandaSeleccionada);

            MessageBox.Show(
                "L'estat de la comanda s'ha actualitzat correctament.",
                "Comanda actualitzada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            dgComandesClients.Items.Refresh();

            await ActualitzarDadesDashboard();
        }

        private async void BtnActualitzarComandes_Click(
            object sender,
            RoutedEventArgs e)
        {
            await CarregarComandesClients();
        }

        private async void BtnGestionarDevolucio_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await GestionarDevolucioAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en gestionar la devolució:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task GestionarDevolucioAsync()
        {
            LiniaComandaVisual producteVisual;
            LiniaComanda liniaSeleccionada = null;
            Producte producte = null;
            List<Devolucio> devolucionsAnteriors = null;

            int quantitatComprada = 0;
            int quantitatJaRetornada = 0;
            int quantitatDisponiblePerRetornar = 0;
            int quantitatRetornada = 0;

            string resposta = "";
            double importDevolucio = 0;

            MessageBoxResult confirmacio;
            bool continuar = true;

            if (comandaSeleccionada == null)
            {
                MostrarMissatgeDevolucio(
                    "Selecciona una comanda abans de gestionar una devolució.",
                    "Comanda no seleccionada",
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar &&
                (comandaSeleccionada.LlistaProductes == null ||
                 comandaSeleccionada.LlistaProductes.Count == 0))
            {
                MostrarMissatgeDevolucio(
                    "Aquesta comanda no té productes.",
                    "Devolució",
                    MessageBoxImage.Warning);

                continuar = false;
            }

            producteVisual =
                dgProductesComandaClient.SelectedItem as LiniaComandaVisual;

            if (continuar && producteVisual == null)
            {
                MostrarMissatgeDevolucio(
                    "Selecciona el producte que vols retornar.",
                    "Producte no seleccionat",
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar)
            {
                liniaSeleccionada =
                    BuscarLiniaComanda(producteVisual.IdProducte);

                if (liniaSeleccionada == null)
                {
                    MostrarMissatgeDevolucio(
                        "No s'ha pogut trobar la línia del producte seleccionat.",
                        "Error",
                        MessageBoxImage.Error);

                    continuar = false;
                }
            }

            if (continuar)
            {
                producte = await dao.GetProducteById(
                    liniaSeleccionada.IdProducte);

                if (producte == null)
                {
                    MostrarMissatgeDevolucio(
                        "No s'ha trobat el producte a Firebase.",
                        "Producte no trobat",
                        MessageBoxImage.Error);

                    continuar = false;
                }
            }

            if (continuar)
            {
                quantitatComprada = liniaSeleccionada.Quantitat;

                devolucionsAnteriors =
                    await dao.GetDevolucionsByComanda(
                        comandaSeleccionada.Id);

                quantitatJaRetornada =
                    ObtenirQuantitatRetornada(
                        devolucionsAnteriors,
                        liniaSeleccionada.IdProducte);

                quantitatDisponiblePerRetornar =
                    quantitatComprada - quantitatJaRetornada;

                if (quantitatDisponiblePerRetornar <= 0)
                {
                    MostrarMissatgeDevolucio(
                        "Aquest producte ja ha estat retornat completament.",
                        "Devolució",
                        MessageBoxImage.Information);

                    continuar = false;
                }
            }

            if (continuar)
            {
                resposta = Microsoft.VisualBasic.Interaction.InputBox(
                    "Producte: " + producte.Nom +
                    "\n\n" +
                    "Quantitat comprada: " + quantitatComprada +
                    "\n" +
                    "Quantitat ja retornada: " + quantitatJaRetornada +
                    "\n" +
                    "Quantitat disponible per retornar: " +
                    quantitatDisponiblePerRetornar +
                    "\n\n" +
                    "Introdueix la quantitat que vols retornar:",
                    "Gestionar devolució",
                    "1");

                if (!int.TryParse(resposta, out quantitatRetornada))
                {
                    MostrarMissatgeDevolucio(
                        "La quantitat introduïda no és correcta.",
                        "Quantitat incorrecta",
                        MessageBoxImage.Warning);

                    continuar = false;
                }
            }

            if (continuar && quantitatRetornada <= 0)
            {
                MostrarMissatgeDevolucio(
                    "La quantitat ha de ser superior a 0.",
                    "Quantitat incorrecta",
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar &&
                quantitatRetornada > quantitatDisponiblePerRetornar)
            {
                MostrarMissatgeDevolucio(
                    "No pots retornar més unitats de les disponibles.\n\n" +
                    "Unitats disponibles: " + quantitatDisponiblePerRetornar,
                    "Quantitat incorrecta",
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar)
            {
                importDevolucio =
                    producte.PreuVenda * quantitatRetornada;

                confirmacio = MessageBox.Show(
                    "Vols registrar aquesta devolució?\n\n" +
                    "Producte: " + producte.Nom + "\n" +
                    "Quantitat: " + quantitatRetornada + "\n" +
                    "Import retornat: " +
                    importDevolucio.ToString("0.00") + " €\n" +
                    "Comanda: " + comandaSeleccionada.Id,
                    "Confirmar devolució",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                continuar = confirmacio == MessageBoxResult.Yes;
            }

            if (continuar)
            {
                await RegistrarDevolucioAsync(
                    producte,
                    liniaSeleccionada,
                    quantitatRetornada,
                    importDevolucio);
            }
        }

        private LiniaComanda BuscarLiniaComanda(string idProducte)
        {
            LiniaComanda liniaSeleccionada =
                comandaSeleccionada.LlistaProductes.FirstOrDefault(
                    l => l.IdProducte == idProducte);

            return liniaSeleccionada;
        }

        private int ObtenirQuantitatRetornada(
            List<Devolucio> devolucions,
            string idProducte)
        {
            int quantitatRetornada = 0;

            if (devolucions != null)
            {
                quantitatRetornada = devolucions
                    .Where(d =>
                        d.IdProducte == idProducte &&
                        d.Estat == "Acceptada")
                    .Sum(d => d.Quantitat);
            }

            return quantitatRetornada;
        }

        private void MostrarMissatgeDevolucio(
            string missatge,
            string titol,
            MessageBoxImage icona)
        {
            MessageBox.Show(
                missatge,
                titol,
                MessageBoxButton.OK,
                icona);
        }

        private async Task RegistrarDevolucioAsync(
            Producte producte,
            LiniaComanda liniaSeleccionada,
            int quantitatRetornada,
            double importDevolucio)
        {
            Devolucio devolucio;
            List<Devolucio> totesLesDevolucions;
            bool totsElsProductesRetornats;

            devolucio = new Devolucio(
                Guid.NewGuid().ToString(),
                comandaSeleccionada.Id,
                producte.Id,
                quantitatRetornada,
                DateTime.UtcNow,
                "Acceptada");

            await dao.AddDevolucio(devolucio);

            producte.Stock += quantitatRetornada;

            await dao.UpdateProducte(producte);

            await CarregarProductes();

            dgProductes.Items.Refresh();

            comandaSeleccionada.Total -= importDevolucio;

            if (comandaSeleccionada.Total < 0)
            {
                comandaSeleccionada.Total = 0;
            }

            totesLesDevolucions =
                await dao.GetDevolucionsByComanda(
                    comandaSeleccionada.Id);

            totsElsProductesRetornats =
                ComprovarTotsElsProductesRetornats(
                    totesLesDevolucions);

            if (totsElsProductesRetornats)
            {
                comandaSeleccionada.Estat = "Cancel·lada";
                comandaSeleccionada.Total = 0;
            }

            await dao.UpdateComanda(comandaSeleccionada);

            await ActualitzarFacturaDevolucioAsync();

            txtTotalComandaClient.Text =
                $"Total: {comandaSeleccionada.Total:0.00} €";

            await CarregarComandesClients();

            carregantComanda = true;

            CarregarEstatComandaSeleccionada(
                comandaSeleccionada.Estat);

            cmbEstatComanda.IsEnabled = true;

            carregantComanda = false;

            await ActualitzarProductesComandaClient();

            await ActualitzarDadesDashboard();

            MostrarResultatDevolucio(
                producte,
                quantitatRetornada,
                importDevolucio,
                totsElsProductesRetornats);
        }

        private bool ComprovarTotsElsProductesRetornats(
            List<Devolucio> devolucions)
        {
            bool totsElsProductesRetornats = true;
            int quantitatComprada;
            int quantitatRetornada;

            foreach (LiniaComanda linia in comandaSeleccionada.LlistaProductes)
            {
                quantitatComprada = linia.Quantitat;

                quantitatRetornada = ObtenirQuantitatRetornada(
                    devolucions,
                    linia.IdProducte);

                if (quantitatRetornada < quantitatComprada)
                {
                    totsElsProductesRetornats = false;
                    break;
                }
            }

            return totsElsProductesRetornats;
        }

        private async Task ActualitzarFacturaDevolucioAsync()
        {
            Factura facturaActualitzada =
                await dao.GetFacturaByComanda(comandaSeleccionada.Id);

            if (facturaActualitzada != null)
            {
                facturaActualitzada.Total = comandaSeleccionada.Total;

                await dao.UpdateFactura(facturaActualitzada);
            }
        }

        private void MostrarResultatDevolucio(
            Producte producte,
            int quantitatRetornada,
            double importDevolucio,
            bool totsElsProductesRetornats)
        {
            string missatge;
            string titol;

            if (totsElsProductesRetornats)
            {
                missatge =
                    "La devolució s'ha registrat correctament.\n\n" +
                    "Producte: " + producte.Nom + "\n" +
                    "Unitats retornades: " + quantitatRetornada + "\n" +
                    "Import retornat: " +
                    importDevolucio.ToString("0.00") +
                    " €\n\n" +
                    "S'han retornat tots els productes de la comanda.\n" +
                    "El total ha passat a 0,00 €.\n" +
                    "La comanda ha estat cancel·lada.";

                titol = "Comanda cancel·lada";
            }
            else
            {
                missatge =
                    "La devolució s'ha registrat correctament.\n\n" +
                    "Producte: " + producte.Nom + "\n" +
                    "Unitats retornades: " + quantitatRetornada + "\n" +
                    "Import retornat: " +
                    importDevolucio.ToString("0.00") +
                    " €\n" +
                    "Nou stock: " + producte.Stock + "\n\n" +
                    "Nou total de la comanda: " +
                    comandaSeleccionada.Total.ToString("0.00") + " €";

                titol = "Devolució registrada";
            }

            MessageBox.Show(
                missatge,
                titol,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private class LiniaComandaVisual
        {
            public string IdProducte { get; set; }

            public string NomProducte { get; set; }

            public int Quantitat { get; set; }

            public double PreuVenda { get; set; }

            public double Subtotal { get; set; }
        }

        private async Task ActualitzarProductesComandaClient()
        {
            List<Devolucio> devolucions;
            List<LiniaComandaVisual> llistaVisual;
            Producte producte;
            LiniaComandaVisual visual;

            int quantitatRetornada;
            int quantitatActual;
            double subtotal;

            try
            {
                if (comandaSeleccionada == null ||
                    comandaSeleccionada.LlistaProductes == null)
                {
                    dgProductesComandaClient.ItemsSource = null;
                }
                else
                {
                    devolucions =
                        await dao.GetDevolucionsByComanda(
                            comandaSeleccionada.Id);

                    llistaVisual = new List<LiniaComandaVisual>();

                    foreach (LiniaComanda linia in comandaSeleccionada.LlistaProductes)
                    {
                        producte = await dao.GetProducteById(linia.IdProducte);

                        if (producte != null)
                        {
                            quantitatRetornada = ObtenirQuantitatRetornada(
                                devolucions,
                                linia.IdProducte);

                            quantitatActual =
                                linia.Quantitat - quantitatRetornada;

                            if (quantitatActual < 0)
                            {
                                quantitatActual = 0;
                            }

                            subtotal = producte.PreuVenda * quantitatActual;

                            visual = new LiniaComandaVisual
                            {
                                IdProducte = linia.IdProducte,
                                NomProducte = producte.Nom,
                                Quantitat = quantitatActual,
                                PreuVenda = producte.PreuVenda,
                                Subtotal = subtotal
                            };

                            llistaVisual.Add(visual);
                        }
                    }

                    dgProductesComandaClient.ItemsSource = null;
                    dgProductesComandaClient.ItemsSource = llistaVisual;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en actualitzar els productes de la comanda:\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void BtnVeureFactura_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await VeureFacturaAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en generar la factura PDF:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task VeureFacturaAsync()
        {
            Comanda comandaActualitzada = null;
            Factura factura = null;
            User client = null;

            List<Devolucio> devolucions = null;
            List<Producte> productes = null;

            string rutaPDF = "";
            bool continuar = true;

            if (comandaSeleccionada == null)
            {
                MostrarMissatgeDevolucio(
                    "Selecciona una comanda abans de veure la factura.",
                    "Comanda no seleccionada",
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar)
            {
                comandaActualitzada =
                    await dao.GetComandaById(comandaSeleccionada.Id);

                if (comandaActualitzada == null)
                {
                    MostrarMissatgeDevolucio(
                        "No s'ha pogut trobar la comanda.",
                        "Error",
                        MessageBoxImage.Error);

                    continuar = false;
                }
            }

            if (continuar)
            {
                comandaSeleccionada = comandaActualitzada;

                factura = await dao.GetFacturaByComanda(
                    comandaActualitzada.Id);

                if (factura == null)
                {
                    MostrarMissatgeDevolucio(
                        "Aquesta comanda no té cap factura.",
                        "Factura no trobada",
                        MessageBoxImage.Warning);

                    continuar = false;
                }
            }

            if (continuar)
            {
                factura.Total = comandaActualitzada.Total;

                await dao.UpdateFactura(factura);

                client = await dao.GetUserById(
                    comandaActualitzada.IdClient);

                if (client == null)
                {
                    MostrarMissatgeDevolucio(
                        "No s'ha pogut trobar el client.",
                        "Client no trobat",
                        MessageBoxImage.Error);

                    continuar = false;
                }
            }

            if (continuar)
            {
                devolucions = await dao.GetDevolucionsByComanda(
                    comandaActualitzada.Id);

                if (devolucions == null)
                {
                    devolucions = new List<Devolucio>();
                }

                productes = await dao.GetAllProductes();

                if (comandaActualitzada.LlistaProductes == null ||
                    comandaActualitzada.LlistaProductes.Count == 0)
                {
                    MostrarMissatgeDevolucio(
                        "La comanda no té productes.",
                        "Sense productes",
                        MessageBoxImage.Warning);

                    continuar = false;
                }
            }

            if (continuar)
            {
                rutaPDF = UTIL.GeneradorFactura.GenerarFacturaPDF(
                    factura,
                    comandaActualitzada,
                    client,
                    comandaActualitzada.LlistaProductes,
                    devolucions,
                    productes);

                txtTotalComandaClient.Text =
                    $"Total: {comandaActualitzada.Total:0.00} €";

                await ActualitzarProductesComandaClient();

                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = rutaPDF,
                        UseShellExecute = true
                    });
            }
        }

        #endregion

        #region vendes

        private class DevolucioVisual
        {
            public string IdDevolucio { get; set; }
            public string Comanda { get; set; }
            public string Client { get; set; }
            public string Data { get; set; }
            public DateTime DataOrdenacio { get; set; }
            public string Producte { get; set; }
            public int Quantitat { get; set; }
            public string Estat { get; set; }
        }

        private async Task CarregarVendes()
        {
            List<Comanda> comandes = null;
            List<Devolucio> devolucions = null;
            List<DevolucioVisual> historial = null;

            try
            {
                comandes = await dao.GetAllComandes();
                devolucions = await dao.GetAllDevolucions();

                if (comandes == null)
                {
                    comandes = new List<Comanda>();
                }

                if (devolucions == null)
                {
                    devolucions = new List<Devolucio>();
                }

                ActualitzarEstadistiquesVendes(
                    comandes,
                    devolucions);

                historial = await CrearHistorialDevolucions(
                    comandes,
                    devolucions);

                dgHistorialDevolucions.ItemsSource = null;
                dgHistorialDevolucions.ItemsSource = historial;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar les vendes:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ActualitzarEstadistiquesVendes(
            List<Comanda> comandes,
            List<Devolucio> devolucions)
        {
            double ingressosTotals = 0;
            int clientsCompradors = 0;
            int totalComandes = 0;
            int devolucionsAcceptades = 0;

            ingressosTotals = CalcularIngressosTotals(comandes);
            clientsCompradors = ComptarClientsCompradors(comandes);
            totalComandes = comandes.Count;
            devolucionsAcceptades = ComptarDevolucionsAcceptades(
                devolucions);

            txtIngressosTotals.Text =
                $"{ingressosTotals:0.00} €";

            txtClientsCompradors.Text =
                clientsCompradors.ToString();

            txtComandesTotals.Text =
                totalComandes.ToString();

            txtDevolucionsTotals.Text =
                devolucionsAcceptades.ToString();
        }

        private double CalcularIngressosTotals(
            List<Comanda> comandes)
        {
            double ingressosTotals = 0;

            ingressosTotals = comandes
                .Where(c =>
                    c != null &&
                    c.Estat != "Cancel·lada" &&
                    c.Total > 0)
                .Sum(c => c.Total);

            return ingressosTotals;
        }

        private int ComptarClientsCompradors(
            List<Comanda> comandes)
        {
            int clientsCompradors = 0;

            clientsCompradors = comandes
                .Where(c =>
                    c != null &&
                    c.Estat != "Cancel·lada" &&
                    c.Total > 0 &&
                    !string.IsNullOrWhiteSpace(c.IdClient))
                .Select(c => c.IdClient)
                .Distinct()
                .Count();

            return clientsCompradors;
        }

        private int ComptarDevolucionsAcceptades(
            List<Devolucio> devolucions)
        {
            int devolucionsAcceptades = 0;

            devolucionsAcceptades = devolucions
                .Count(d => d.Estat == "Acceptada");

            return devolucionsAcceptades;
        }

        private async Task<List<DevolucioVisual>>
            CrearHistorialDevolucions(
                List<Comanda> comandes,
                List<Devolucio> devolucions)
        {
            List<DevolucioVisual> historial =
                new List<DevolucioVisual>();

            DevolucioVisual devolucioVisual = null;

            foreach (Devolucio devolucio in devolucions)
            {
                devolucioVisual = await CrearDevolucioVisual(
                    comandes,
                    devolucio);

                historial.Add(devolucioVisual);
            }

            historial = historial
                .OrderByDescending(d => d.DataOrdenacio)
                .ToList();

            return historial;
        }

        private async Task<DevolucioVisual> CrearDevolucioVisual(
            List<Comanda> comandes,
            Devolucio devolucio)
        {
            Comanda comanda = null;
            Producte producte = null;
            string client = "-";
            string nomProducte = "-";
            DevolucioVisual devolucioVisual = null;

            comanda = comandes.FirstOrDefault(
                c => c.Id == devolucio.IdComanda);

            producte = await dao.GetProducteById(
                devolucio.IdProducte);

            if (comanda != null)
            {
                client = comanda.IdClient;
            }

            if (producte != null)
            {
                nomProducte = producte.Nom;
            }

            devolucioVisual = new DevolucioVisual
            {
                IdDevolucio = devolucio.Id,
                Comanda = devolucio.IdComanda,
                Client = client,
                Data = devolucio.Data
                    .ToLocalTime()
                    .ToString("dd/MM/yyyy"),
                DataOrdenacio = devolucio.Data,
                Producte = nomProducte,
                Quantitat = devolucio.Quantitat,
                Estat = devolucio.Estat
            };

            return devolucioVisual;
        }

        private async Task ActualitzarDadesDashboard()
        {
            await CarregarComandesClients();
            await CarregarVendes();
            await CarregarGrafics();
        }

        #endregion

        #region Messatges

        private const string ID_VENEDOR = "VENEDOR";

        private User clientSeleccionatMissatges = null;

        private List<User> clientsMissatges =
            new List<User>();

        private List<Missatge> missatgesActuals =
            new List<Missatge>();

        private async Task CarregarClientsMissatges()
        {
            List<User> usuaris = null;

            try
            {
                usuaris = await dao.GetAllUsers();

                if (usuaris == null)
                {
                    usuaris = new List<User>();
                }

                clientsMissatges = usuaris
                    .Where(u =>
                        u != null &&
                        !string.IsNullOrWhiteSpace(u.Id) &&
                        u.Id != "-" &&
                        u.Id != ID_VENEDOR)
                    .ToList();

                lstClients.ItemsSource = null;
                lstClients.ItemsSource = clientsMissatges;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar els clients dels missatges:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void LstClients_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            User client = null;

            try
            {
                client = lstClients.SelectedItem as User;

                if (client != null)
                {
                    clientSeleccionatMissatges = client;

                    txtClientSeleccionat.Text = client.Nom;
                    txtMissatge.Clear();

                    await CarregarMissatgesClient();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar la conversa:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task CarregarMissatgesClient()
        {
            List<Missatge> totsElsMissatges = null;

            try
            {
                if (clientSeleccionatMissatges != null)
                {
                    totsElsMissatges =
                        await dao.GetAllMissatges();

                    if (totsElsMissatges == null)
                    {
                        totsElsMissatges =
                            new List<Missatge>();
                    }

                    missatgesActuals =
                        FiltrarMissatgesClient(
                            totsElsMissatges);

                    panelMissatges.Children.Clear();

                    foreach (Missatge missatge in missatgesActuals)
                    {
                        AfegirMissatgeVisual(missatge);
                    }

                    await MarcarMissatgesComLlegits();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar els missatges:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private List<Missatge> FiltrarMissatgesClient(
            List<Missatge> totsElsMissatges)
        {
            List<Missatge> missatgesFiltrats =
                new List<Missatge>();

            missatgesFiltrats = totsElsMissatges
                .Where(m =>
                    (m.IdEmissor ==
                        clientSeleccionatMissatges.Id &&
                     m.IdReceptor == ID_VENEDOR) ||
                    (m.IdEmissor == ID_VENEDOR &&
                     m.IdReceptor ==
                        clientSeleccionatMissatges.Id))
                .OrderBy(m => m.Data)
                .ToList();

            return missatgesFiltrats;
        }

        private void AfegirMissatgeVisual(
            Missatge missatge)
        {
            Border contenidor = null;
            StackPanel contingut = null;
            TextBlock text = null;
            StackPanel filaInferior = null;
            TextBlock data = null;

            contenidor = CrearContenidorMissatge();
            contingut = new StackPanel();

            text = CrearTextMissatge(missatge);
            filaInferior = CrearFilaInferiorMissatge();
            data = CrearDataMissatge(missatge);

            filaInferior.Children.Add(data);

            AfegirChecksMissatge(
                filaInferior,
                missatge);

            contingut.Children.Add(text);
            contingut.Children.Add(filaInferior);

            AplicarEstilMissatge(
                contenidor,
                missatge);

            contenidor.Child = contingut;

            panelMissatges.Children.Add(contenidor);
        }

        private Border CrearContenidorMissatge()
        {
            Border contenidor = new Border();

            contenidor.Margin = new Thickness(0, 3, 0, 3);
            contenidor.Padding = new Thickness(10);
            contenidor.CornerRadius = new CornerRadius(8);
            contenidor.MaxWidth = 450;

            return contenidor;
        }

        private TextBlock CrearTextMissatge(
            Missatge missatge)
        {
            TextBlock text = new TextBlock();

            text.Text = missatge.Text;
            text.TextWrapping = TextWrapping.Wrap;
            text.FontSize = 14;

            return text;
        }

        private StackPanel CrearFilaInferiorMissatge()
        {
            StackPanel filaInferior = new StackPanel();

            filaInferior.Orientation = Orientation.Horizontal;

            filaInferior.HorizontalAlignment =
                HorizontalAlignment.Right;

            filaInferior.Margin = new Thickness(0, 5, 0, 0);

            return filaInferior;
        }

        private TextBlock CrearDataMissatge(
            Missatge missatge)
        {
            TextBlock data = new TextBlock();

            data.Text = missatge.Data
                .ToLocalTime()
                .ToString("dd/MM/yyyy HH:mm");

            data.FontSize = 10;

            return data;
        }

        private void AfegirChecksMissatge(
            StackPanel filaInferior,
            Missatge missatge)
        {
            TextBlock checks = null;

            if (missatge.IdEmissor != ID_VENEDOR)
            {
                checks = CrearChecksMissatge(missatge);

                filaInferior.Children.Add(checks);
            }
        }

        private TextBlock CrearChecksMissatge(
            Missatge missatge)
        {
            TextBlock checks = new TextBlock();

            checks.Text = "  ✓✓";
            checks.FontSize = 12;
            checks.FontWeight = FontWeights.Bold;

            if (missatge.Llegit)
            {
                checks.Foreground = Brushes.DeepSkyBlue;
            }
            else
            {
                checks.Foreground = Brushes.Gray;
            }

            return checks;
        }

        private void AplicarEstilMissatge(
            Border contenidor,
            Missatge missatge)
        {
            if (missatge.IdEmissor == ID_VENEDOR)
            {
                contenidor.HorizontalAlignment =
                    HorizontalAlignment.Right;

                contenidor.Background = Brushes.LightGreen;
            }
            else
            {
                contenidor.HorizontalAlignment =
                    HorizontalAlignment.Left;

                contenidor.Background = Brushes.LightGray;
            }
        }

        private async void BtnEnviar_Click(
            object sender,
            RoutedEventArgs e)
        {
            string text = string.Empty;
            Missatge missatge = null;

            try
            {
                if (ValidarClientMissatges())
                {
                    text = txtMissatge.Text.Trim();

                    if (ValidarTextMissatge(text))
                    {
                        missatge = CrearMissatge(text);

                        await dao.AddMissatge(missatge);

                        txtMissatge.Clear();

                        await CarregarMissatgesClient();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en enviar el missatge:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidarClientMissatges()
        {
            bool clientValid = false;

            clientValid = clientSeleccionatMissatges != null;

            if (!clientValid)
            {
                MessageBox.Show(
                    "Selecciona primer un client.",
                    "Client no seleccionat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            return clientValid;
        }

        private bool ValidarTextMissatge(
            string text)
        {
            bool textValid = false;

            textValid = !string.IsNullOrWhiteSpace(text);

            if (!textValid)
            {
                MessageBox.Show(
                    "Escriu un missatge abans d'enviar-lo.",
                    "Missatge buit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            return textValid;
        }

        private Missatge CrearMissatge(
            string text)
        {
            Missatge missatge = null;

            missatge = new Missatge(
                Guid.NewGuid().ToString(),
                ID_VENEDOR,
                clientSeleccionatMissatges.Id,
                text,
                DateTime.UtcNow,
                false);

            return missatge;
        }

        private async Task MarcarMissatgesComLlegits()
        {
            try
            {
                if (clientSeleccionatMissatges != null)
                {
                    foreach (Missatge missatge in missatgesActuals)
                    {
                        if (missatge.IdEmissor != ID_VENEDOR &&
                            missatge.IdReceptor == ID_VENEDOR &&
                            !missatge.Llegit)
                        {
                            await MarcarMissatgeComLlegit(missatge);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en marcar els missatges com a llegits:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private async Task MarcarMissatgeComLlegit(
            Missatge missatge)
        {
            missatge.Llegit = true;

            await dao.UpdateMissatge(missatge);
        }

        #endregion

        #region Grafics

        private async Task CarregarGrafics()
        {
            List<Comanda> comandes = null;
            List<Devolucio> devolucions = null;
            List<Producte> productes = null;

            try
            {
                comandes = await dao.GetAllComandes();
                devolucions = await dao.GetAllDevolucions();
                productes = await dao.GetAllProductes();

                if (comandes == null)
                {
                    comandes = new List<Comanda>();
                }

                if (devolucions == null)
                {
                    devolucions = new List<Devolucio>();
                }

                if (productes == null)
                {
                    productes = new List<Producte>();
                }

                await CarregarGraficsProductesMesVenuts(
                    comandes,
                    devolucions,
                    productes);

                CarregarGraficsIngressosMensuals(comandes);
                CarregarGraficsEvolucioVendes(comandes);
                CarregarGraficsStockCritic(productes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar els gràfics:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private async Task CarregarGraficsProductesMesVenuts(
            List<Comanda> comandes,
            List<Devolucio> devolucions,
            List<Producte> productes)
        {
            Dictionary<string, int> unitatsPerProducte =
                new Dictionary<string, int>();

            List<KeyValuePair<string, int>> productesOrdenats =
                new List<KeyValuePair<string, int>>();

            double amplada = 0;
            double alcada = 0;
            double margeEsquerra = 35;
            double margeBaix = 30;
            double margeSuperior = 10;
            double ampleDisponible = 0;
            double altDisponible = 0;
            double ampleBarra = 0;
            int valorMaxim = 1;

            try
            {
                canvasProductesMesVenuts.Children.Clear();

                unitatsPerProducte =
                    CalcularUnitatsVenudes(comandes);

                RestarDevolucionsAcceptades(
                    unitatsPerProducte,
                    devolucions);

                EvitarUnitatsNegatives(unitatsPerProducte);

                productesOrdenats = unitatsPerProducte
                    .OrderByDescending(p => p.Value)
                    .Take(5)
                    .ToList();

                if (productesOrdenats.Count == 0)
                {
                    AfegirTextCanvas(
                        canvasProductesMesVenuts,
                        "No hi ha dades de vendes.",
                        20,
                        50);
                }
                else
                {
                    amplada = canvasProductesMesVenuts.ActualWidth;
                    alcada = canvasProductesMesVenuts.ActualHeight;

                    if (amplada <= 0)
                    {
                        amplada = 350;
                    }

                    if (alcada <= 0)
                    {
                        alcada = 150;
                    }

                    ampleDisponible = amplada - margeEsquerra - 10;
                    altDisponible = alcada - margeBaix - margeSuperior;

                    valorMaxim = productesOrdenats.Max(p => p.Value);

                    if (valorMaxim <= 0)
                    {
                        valorMaxim = 1;
                    }

                    ampleBarra =
                        ampleDisponible / productesOrdenats.Count * 0.55;

                    DibuixarEixosProductesMesVenuts(
                        amplada,
                        alcada,
                        margeEsquerra,
                        margeBaix,
                        margeSuperior);

                    await DibuixarBarresProductesMesVenuts(
                        productesOrdenats,
                        productes,
                        amplada,
                        alcada,
                        margeEsquerra,
                        margeBaix,
                        ampleDisponible,
                        altDisponible,
                        ampleBarra,
                        valorMaxim);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar el gràfic de productes més venuts:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private Dictionary<string, int> CalcularUnitatsVenudes(
            List<Comanda> comandes)
        {
            Dictionary<string, int> unitatsPerProducte =
                new Dictionary<string, int>();

            string idProducte = string.Empty;

            foreach (Comanda comanda in comandes)
            {
                if (comanda == null ||
                    comanda.Estat == "Cancel·lada" ||
                    comanda.Total <= 0 ||
                    comanda.LlistaProductes == null)
                {
                    continue;
                }

                foreach (LiniaComanda linia in comanda.LlistaProductes)
                {
                    idProducte = linia.IdProducte;

                    if (!unitatsPerProducte.ContainsKey(idProducte))
                    {
                        unitatsPerProducte[idProducte] = 0;
                    }

                    unitatsPerProducte[idProducte] += linia.Quantitat;
                }
            }

            return unitatsPerProducte;
        }


        private void RestarDevolucionsAcceptades(
            Dictionary<string, int> unitatsPerProducte,
            List<Devolucio> devolucions)
        {
            foreach (Devolucio devolucio in devolucions)
            {
                if (devolucio.Estat == "Acceptada" &&
                    unitatsPerProducte.ContainsKey(devolucio.IdProducte))
                {
                    unitatsPerProducte[devolucio.IdProducte] -=
                        devolucio.Quantitat;
                }
            }
        }

        private void EvitarUnitatsNegatives(
            Dictionary<string, int> unitatsPerProducte)
        {
            List<string> identificadors =
                unitatsPerProducte.Keys.ToList();

            foreach (string idProducte in identificadors)
            {
                if (unitatsPerProducte[idProducte] < 0)
                {
                    unitatsPerProducte[idProducte] = 0;
                }
            }
        }


        private void DibuixarEixosProductesMesVenuts(
            double amplada,
            double alcada,
            double margeEsquerra,
            double margeBaix,
            double margeSuperior)
        {
            Line eixVertical = null;
            Line eixHoritzontal = null;

            eixVertical = new Line
            {
                X1 = margeEsquerra,
                Y1 = margeSuperior,
                X2 = margeEsquerra,
                Y2 = alcada - margeBaix,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            eixHoritzontal = new Line
            {
                X1 = margeEsquerra,
                Y1 = alcada - margeBaix,
                X2 = amplada - 10,
                Y2 = alcada - margeBaix,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            canvasProductesMesVenuts.Children.Add(eixVertical);
            canvasProductesMesVenuts.Children.Add(eixHoritzontal);
        }

        private async Task DibuixarBarresProductesMesVenuts(
            List<KeyValuePair<string, int>> productesOrdenats,
            List<Producte> productes,
            double amplada,
            double alcada,
            double margeEsquerra,
            double margeBaix,
            double ampleDisponible,
            double altDisponible,
            double ampleBarra,
            int valorMaxim)
        {
            string idProducte = string.Empty;
            string nom = string.Empty;
            string nomCurt = string.Empty;
            int quantitat = 0;
            double alturaBarra = 0;
            double x = 0;
            double y = 0;
            double ampladaGrup = 0;
            Producte producte = null;
            Rectangle barra = null;

            ampladaGrup = ampleDisponible / productesOrdenats.Count;

            for (int i = 0; i < productesOrdenats.Count; i++)
            {
                idProducte = productesOrdenats[i].Key;
                quantitat = productesOrdenats[i].Value;

                producte = productes.FirstOrDefault(
                    p => p.Id == idProducte);

                nom = producte != null ? producte.Nom : idProducte;

                alturaBarra =
                    (double)quantitat / valorMaxim * altDisponible;

                x = margeEsquerra +
                    (i + 0.5) * ampladaGrup -
                    ampleBarra / 2;

                y = alcada - margeBaix - alturaBarra;

                barra = new Rectangle
                {
                    Width = ampleBarra,
                    Height = alturaBarra,
                    Fill = Brushes.SteelBlue
                };

                Canvas.SetLeft(barra, x);
                Canvas.SetTop(barra, y);

                canvasProductesMesVenuts.Children.Add(barra);

                AfegirTextCanvas(
                    canvasProductesMesVenuts,
                    quantitat.ToString(),
                    x,
                    y - 18);

                nomCurt = nom.Length > 10
                    ? nom.Substring(0, 10)
                    : nom;

                AfegirTextCanvas(
                    canvasProductesMesVenuts,
                    nomCurt,
                    x - 5,
                    alcada - margeBaix + 5);
            }

            await Task.CompletedTask;
        }


        private void CarregarGraficsIngressosMensuals(
            List<Comanda> comandes)
        {
            List<DateTime> mesos = new List<DateTime>();
            List<double> ingressos = new List<double>();

            DateTime avui = DateTime.Now;
            DateTime inici = DateTime.MinValue;
            DateTime mesActual = DateTime.MinValue;

            double amplada = 0;
            double alcada = 0;
            double margeEsquerra = 45;
            double margeBaix = 35;
            double margeSuperior = 15;
            double margeDreta = 10;
            double ampleDisponible = 0;
            double altDisponible = 0;
            double valorMaxim = 0;
            double ampladaMes = 0;
            double ampleBarra = 0;

            try
            {
                canvasIngressosMensuals.Children.Clear();

                inici = ObtenirMesInicialGrafics(avui);
                mesActual = new DateTime(avui.Year, avui.Month, 1);

                mesos = CrearLlistaMesos(inici, mesActual);
                ingressos = CalcularIngressosMensuals(comandes, mesos);

                amplada = canvasIngressosMensuals.ActualWidth;
                alcada = canvasIngressosMensuals.ActualHeight;

                if (amplada <= 0)
                {
                    amplada = 350;
                }

                if (alcada <= 0)
                {
                    alcada = 150;
                }

                ampleDisponible =
                    amplada - margeEsquerra - margeDreta;

                altDisponible =
                    alcada - margeBaix - margeSuperior;

                if (ingressos.Count > 0)
                {
                    valorMaxim = ingressos.Max();
                }

                if (valorMaxim <= 0)
                {
                    valorMaxim = 1;
                }

                ampladaMes = ampleDisponible / mesos.Count;
                ampleBarra = ampladaMes * 0.55;

                DibuixarEixosIngressosMensuals(
                    amplada,
                    alcada,
                    margeEsquerra,
                    margeBaix,
                    margeSuperior,
                    margeDreta);

                DibuixarBarresIngressosMensuals(
                    mesos,
                    ingressos,
                    amplada,
                    alcada,
                    margeEsquerra,
                    margeBaix,
                    altDisponible,
                    valorMaxim,
                    ampladaMes,
                    ampleBarra);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar el gràfic d'ingressos mensuals:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private DateTime ObtenirMesInicialGrafics(
            DateTime avui)
        {
            DateTime inici = new DateTime(avui.Year, 8, 1);

            if (avui.Month < 8)
            {
                inici = inici.AddYears(-1);
            }

            return inici;
        }


        private List<DateTime> CrearLlistaMesos(
            DateTime inici,
            DateTime mesActual)
        {
            List<DateTime> mesos = new List<DateTime>();
            DateTime mesRecorregut = inici;

            while (mesRecorregut <= mesActual)
            {
                mesos.Add(mesRecorregut);
                mesRecorregut = mesRecorregut.AddMonths(1);
            }

            return mesos;
        }

        private List<double> CalcularIngressosMensuals(
            List<Comanda> comandes,
            List<DateTime> mesos)
        {
            List<double> ingressos = new List<double>();
            double total = 0;

            foreach (DateTime mes in mesos)
            {
                total = comandes
                    .Where(c =>
                        c != null &&
                        c.Estat != "Cancel·lada" &&
                        c.Total > 0 &&
                        c.Data.ToLocalTime().Year == mes.Year &&
                        c.Data.ToLocalTime().Month == mes.Month)
                    .Sum(c => c.Total);

                if (total < 0)
                {
                    total = 0;
                }

                ingressos.Add(total);
            }

            return ingressos;
        }

        private void DibuixarEixosIngressosMensuals(
            double amplada,
            double alcada,
            double margeEsquerra,
            double margeBaix,
            double margeSuperior,
            double margeDreta)
        {
            Line eixVertical = null;
            Line eixHoritzontal = null;

            eixVertical = new Line
            {
                X1 = margeEsquerra,
                Y1 = margeSuperior,
                X2 = margeEsquerra,
                Y2 = alcada - margeBaix,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            eixHoritzontal = new Line
            {
                X1 = margeEsquerra,
                Y1 = alcada - margeBaix,
                X2 = amplada - margeDreta,
                Y2 = alcada - margeBaix,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            canvasIngressosMensuals.Children.Add(eixVertical);
            canvasIngressosMensuals.Children.Add(eixHoritzontal);
        }

        private void DibuixarBarresIngressosMensuals(
            List<DateTime> mesos,
            List<double> ingressos,
            double amplada,
            double alcada,
            double margeEsquerra,
            double margeBaix,
            double altDisponible,
            double valorMaxim,
            double ampladaMes,
            double ampleBarra)
        {
            double valor = 0;
            double alturaBarra = 0;
            double x = 0;
            double y = 0;
            double xText = 0;
            string nomMes = string.Empty;
            Rectangle barra = null;

            for (int i = 0; i < mesos.Count; i++)
            {
                valor = ingressos[i];

                alturaBarra =
                    valor / valorMaxim * altDisponible;

                x = margeEsquerra +
                    i * ampladaMes +
                    (ampladaMes - ampleBarra) / 2;

                y = alcada - margeBaix - alturaBarra;

                barra = new Rectangle
                {
                    Width = ampleBarra,
                    Height = alturaBarra,
                    Fill = Brushes.MediumPurple
                };

                Canvas.SetLeft(barra, x);
                Canvas.SetTop(barra, y);

                canvasIngressosMensuals.Children.Add(barra);

                nomMes = ObtenirNomMes(mesos[i]);

                xText = x + (ampleBarra / 2) - 15;

                AfegirTextCanvas(
                    canvasIngressosMensuals,
                    nomMes,
                    xText,
                    alcada - margeBaix + 5);

                AfegirTextCanvas(
                    canvasIngressosMensuals,
                    valor.ToString("0.00") + " €",
                    x,
                    y - 18);
            }
        }


       
        private string ObtenirNomMes(
            DateTime mes)
        {
            string nomMes = string.Empty;

            nomMes = mes.ToString(
                "MMMM",
                new CultureInfo("ca-ES"));

            if (!string.IsNullOrWhiteSpace(nomMes))
            {
                nomMes = char.ToUpper(nomMes[0]) +
                    nomMes.Substring(1);
            }

            return nomMes;
        }


        private void CarregarGraficsEvolucioVendes(
            List<Comanda> comandes)
        {
            List<DateTime> mesos = new List<DateTime>();
            List<int> vendes = new List<int>();
            List<Point> punts = new List<Point>();

            DateTime avui = DateTime.Now;
            DateTime inici = DateTime.MinValue;
            DateTime mesActual = DateTime.MinValue;

            double amplada = 0;
            double alcada = 0;
            double margeEsquerra = 35;
            double margeBaix = 35;
            double margeSuperior = 15;
            double margeDreta = 10;
            double ampleDisponible = 0;
            double altDisponible = 0;
            double ampleMes = 0;
            int valorMaxim = 1;

            try
            {
                canvasEvolucioVendes.Children.Clear();

                inici = ObtenirMesInicialGrafics(avui);
                mesActual = new DateTime(avui.Year, avui.Month, 1);

                mesos = CrearLlistaMesos(inici, mesActual);
                vendes = CalcularVendesMensuals(comandes, mesos);

                amplada = canvasEvolucioVendes.ActualWidth;
                alcada = canvasEvolucioVendes.ActualHeight;

                if (amplada <= 0)
                {
                    amplada = 350;
                }

                if (alcada <= 0)
                {
                    alcada = 150;
                }

                ampleDisponible =
                    amplada - margeEsquerra - margeDreta;

                altDisponible =
                    alcada - margeBaix - margeSuperior;

                if (vendes.Count > 0)
                {
                    valorMaxim = vendes.Max();
                }

                if (valorMaxim <= 0)
                {
                    valorMaxim = 1;
                }

                ampleMes = ampleDisponible / mesos.Count;

                DibuixarEixosEvolucioVendes(
                    amplada,
                    alcada,
                    margeEsquerra,
                    margeBaix,
                    margeSuperior,
                    margeDreta);

                punts = CalcularPuntsEvolucioVendes(
                    mesos,
                    vendes,
                    alcada,
                    margeEsquerra,
                    margeBaix,
                    ampleMes,
                    altDisponible,
                    valorMaxim);

                DibuixarLiniesEvolucioVendes(punts);

                DibuixarPuntsEvolucioVendes(
                    punts,
                    mesos,
                    vendes,
                    alcada,
                    margeBaix);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar el gràfic d'evolució de vendes:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private List<int> CalcularVendesMensuals(
            List<Comanda> comandes,
            List<DateTime> mesos)
        {
            List<int> vendes = new List<int>();
            int quantitat = 0;

            foreach (DateTime mes in mesos)
            {
                quantitat = comandes.Count(c =>
                    c != null &&
                    c.Estat != "Cancel·lada" &&
                    c.Data.ToLocalTime().Year == mes.Year &&
                    c.Data.ToLocalTime().Month == mes.Month &&
                    c.Total > 0);

                vendes.Add(quantitat);
            }

            return vendes;
        }


        private void DibuixarEixosEvolucioVendes(
            double amplada,
            double alcada,
            double margeEsquerra,
            double margeBaix,
            double margeSuperior,
            double margeDreta)
        {
            Line eixVertical = null;
            Line eixHoritzontal = null;

            eixVertical = new Line
            {
                X1 = margeEsquerra,
                Y1 = margeSuperior,
                X2 = margeEsquerra,
                Y2 = alcada - margeBaix,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            eixHoritzontal = new Line
            {
                X1 = margeEsquerra,
                Y1 = alcada - margeBaix,
                X2 = amplada - margeDreta,
                Y2 = alcada - margeBaix,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            canvasEvolucioVendes.Children.Add(eixVertical);
            canvasEvolucioVendes.Children.Add(eixHoritzontal);
        }

        private List<Point> CalcularPuntsEvolucioVendes(
            List<DateTime> mesos,
            List<int> vendes,
            double alcada,
            double margeEsquerra,
            double margeBaix,
            double ampleMes,
            double altDisponible,
            int valorMaxim)
        {
            List<Point> punts = new List<Point>();
            int valor = 0;
            double x = 0;
            double y = 0;

            for (int i = 0; i < mesos.Count; i++)
            {
                valor = vendes[i];

                x = margeEsquerra +
                    i * ampleMes +
                    ampleMes / 2;

                y = alcada -
                    margeBaix -
                    ((double)valor / valorMaxim * altDisponible);

                punts.Add(new Point(x, y));
            }

            return punts;
        }

        private void DibuixarLiniesEvolucioVendes(
            List<Point> punts)
        {
            Line linia = null;

            for (int i = 0; i < punts.Count - 1; i++)
            {
                linia = new Line
                {
                    X1 = punts[i].X,
                    Y1 = punts[i].Y,
                    X2 = punts[i + 1].X,
                    Y2 = punts[i + 1].Y,
                    Stroke = Brushes.Purple,
                    StrokeThickness = 2
                };

                canvasEvolucioVendes.Children.Add(linia);
            }
        }

        private void DibuixarPuntsEvolucioVendes(
            List<Point> punts,
            List<DateTime> mesos,
            List<int> vendes,
            double alcada,
            double margeBaix)
        {
            int valor = 0;
            Point punt = new Point();
            Ellipse cercle = null;
            string nomMes = string.Empty;

            for (int i = 0; i < punts.Count; i++)
            {
                valor = vendes[i];
                punt = punts[i];

                cercle = new Ellipse
                {
                    Width = 9,
                    Height = 9,
                    Fill = Brushes.Purple
                };

                Canvas.SetLeft(cercle, punt.X - 4.5);
                Canvas.SetTop(cercle, punt.Y - 4.5);

                canvasEvolucioVendes.Children.Add(cercle);

                nomMes = ObtenirNomMes(mesos[i]);

                AfegirTextCanvas(
                    canvasEvolucioVendes,
                    nomMes,
                    punt.X - 15,
                    alcada - margeBaix + 5);

                AfegirTextCanvas(
                    canvasEvolucioVendes,
                    valor.ToString(),
                    punt.X - 5,
                    punt.Y - 20);
            }
        }

        private void CarregarGraficsStockCritic(
            List<Producte> productes)
        {
            List<Producte> productesCritics =
                new List<Producte>();

            TextBlock textSenseStock = null;

            try
            {
                panelStockCritic.Children.Clear();

                productesCritics = productes
                    .Where(p => p.Stock <= p.StockMinim)
                    .OrderBy(p => p.Stock)
                    .Take(10)
                    .ToList();

                if (productesCritics.Count == 0)
                {
                    textSenseStock = new TextBlock
                    {
                        Text = "No hi ha productes amb stock crític.",
                        Margin = new Thickness(5),
                        FontSize = 12
                    };

                    panelStockCritic.Children.Add(textSenseStock);
                }
                else
                {
                    foreach (Producte producte in productesCritics)
                    {
                        AfegirFilaStockCritic(producte);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar el stock crític:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AfegirFilaStockCritic(
            Producte producte)
        {
            Grid fila = null;
            TextBlock nom = null;
            Border stock = null;
            TextBlock textStock = null;

            fila = new Grid();
            fila.Margin = new Thickness(0, 3, 0, 3);

            fila.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });

            fila.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = GridLength.Auto
                });

            nom = new TextBlock
            {
                Text = producte.Nom,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 11
            };

            Grid.SetColumn(nom, 0);
            fila.Children.Add(nom);

            stock = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(5, 0, 0, 0)
            };

            textStock = new TextBlock
            {
                Text = producte.Stock + " unitats",
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stock.Child = textStock;

            Grid.SetColumn(stock, 1);
            fila.Children.Add(stock);

            panelStockCritic.Children.Add(fila);
        }

        private void AfegirTextCanvas(
            Canvas canvas,
            string text,
            double x,
            double y)
        {
            TextBlock textBlock = null;

            textBlock = new TextBlock
            {
                Text = text,
                FontSize = 9
            };

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);

            canvas.Children.Add(textBlock);
        }

        #endregion

        #region Tancar Sessió

        private void BtnTancarSessio_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            this.Close();
        }

        #endregion

    }
}
