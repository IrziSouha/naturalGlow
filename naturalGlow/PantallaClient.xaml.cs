using naturalGlow.Auth;
using naturalGlow.DAO;
using naturalGlow.MODEL;
using naturalGlow.UTIL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace naturalGlow
{
    public partial class PantallaClient : Window
    {
        private IDAO dao;
        private List<Producte> productes;

        private List<LiniaComanda> carrito;
        private string idClient;

        private List<Missatge> missatgesActuals =
         new List<Missatge>();
        public PantallaClient(string idClient)
        {
            InitializeComponent();

            this.idClient = idClient;

            carrito = new List<LiniaComanda>();

            dao = new IDAOFirebaseImpl(
                @"C:\Treball Final\naturalGlow\naturalGlow\natural.json",
                "naturalglow");

            Loaded += PantallaClient_Loaded;
        }

        #region Carregar Pantalla
        private async void PantallaClient_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CarregarProductes();
            await CarregarComandes();
            await CarregarPerfil(); 
            await CarregarMissatgesVenedor();

        }
        #endregion

        #region Productes

        #region Carregar Productes
        private async Task CarregarProductes()
        {
            try
            {
                productes =
                    await dao.GetAllProductes();

                dgProductes.ItemsSource =
                    productes;

                CarregarCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar els productes:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion


        #region Carregar Categories
        private void CarregarCategories()
        {
            cmbCategoria.Items.Clear();

            cmbCategoria.Items.Add("Totes");

            if (productes != null)
            {
                List<string> categories =
                    productes
                    .Where(p =>
                        !string.IsNullOrWhiteSpace(p.Categoria))
                    .Select(p => p.Categoria)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                foreach (string categoria in categories)
                {
                    cmbCategoria.Items.Add(categoria);
                }
            }

            cmbCategoria.SelectedIndex = 0;
        }

        #endregion


        #region Cerca Productes
        private void TxtCerca_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            FiltrarProductes();
        }

        #endregion


        #region Filtrar Categoria

        private void CmbCategoria_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (cmbCategoria != null)
            {
                FiltrarProductes();
            }
        }

        #endregion


        #region Ordenar Productes

        private void CmbOrdenar_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (cmbOrdenar != null)
            {
                FiltrarProductes();
            }
        }

        #endregion


        #region Filtrar Productes

        private void FiltrarProductes()
        {
            if (productes != null)
            {
                IEnumerable<Producte> resultat =
                    productes;

                string cerca =
                    txtCerca.Text.Trim();

                if (!string.IsNullOrWhiteSpace(cerca))
                {
                    resultat =
                        resultat.Where(p =>
                            p.Nom.IndexOf(
                                cerca,
                                StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (cmbCategoria.SelectedItem != null)
                {
                    string categoriaSeleccionada =
                        cmbCategoria.SelectedItem.ToString();

                    if (categoriaSeleccionada != "Totes")
                    {
                        resultat =
                            resultat.Where(p =>
                                p.Categoria ==
                                categoriaSeleccionada);
                    }
                }

                if (cmbOrdenar.SelectedItem
                    is ComboBoxItem opcio)
                {
                    string ordre =
                        opcio.Content.ToString();

                    if (ordre ==
                        "Preu: menor a major")
                    {
                        resultat =
                            resultat.OrderBy(
                                p => p.PreuVenda);
                    }
                    else if (ordre ==
                        "Preu: major a menor")
                    {
                        resultat =
                            resultat.OrderByDescending(
                                p => p.PreuVenda);
                    }
                    else if (ordre ==
                        "Novetats")
                    {
                        resultat = resultat;
                    }
                }

                dgProductes.ItemsSource =
                    resultat.ToList();
            }
        }

        #endregion

        #region Comprar Producte

        private void BtnComprar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button boto =
                sender as Button;

            Producte producte =
                boto.DataContext as Producte;

            if (producte != null)
            {
                if (producte.Stock > 0)
                {
                    LiniaComanda linia =
                        carrito.FirstOrDefault(
                            l => l.IdProducte == producte.Id);

                    if (linia != null)
                    {
                        if (linia.Quantitat < producte.Stock)
                        {
                            linia.Quantitat++;

                            linia.Subtotal =
                                linia.Quantitat *
                                linia.PreuUnitari;

                            ActualitzarCarreto();

                            MessageBox.Show(
                                producte.Nom
                                + " s'ha afegit al carretó.",
                                "Carretó",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "No pots afegir més unitats "
                                + "d'aquest producte.",
                                "Stock insuficient",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        LiniaComanda novaLinia =
                            new LiniaComanda(
                                producte.Id,
                                producte.Nom,
                                1,
                                producte.PreuVenda);

                        carrito.Add(novaLinia);

                        ActualitzarCarreto();

                        MessageBox.Show(
                            producte.Nom
                            + " s'ha afegit al carretó.",
                            "Carretó",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Aquest producte està esgotat.",
                        "Producte esgotat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                    "No s'ha pogut seleccionar el producte.");
            }
        }

        #endregion

        #endregion

        #region Carreto

        #region Actualitzar Carreto
        private void ActualitzarCarreto()
        {
            dgCarreto.ItemsSource = null;

            dgCarreto.ItemsSource =
                carrito;


            double total =
                carrito.Sum(
                    l => l.Subtotal);


            txtTotal.Text =
                total.ToString("F2")
                + " €";
        }

        #endregion

        #region Augmentar Quantitat

        private void BtnAugmentarQuantitat_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button boto =
                sender as Button;

            LiniaComanda linia =
                boto.DataContext as LiniaComanda;

            bool continuar = true;

            if (linia == null)
            {
                continuar = false;
            }

            Producte producte = null;

            if (continuar)
            {
                producte =
                    productes.FirstOrDefault(
                        p => p.Id ==
                        linia.IdProducte);

                if (producte == null)
                {
                    continuar = false;
                }
            }

            if (continuar)
            {
                if (linia.Quantitat >= producte.Stock)
                {
                    MessageBox.Show(
                        "No hi ha més stock disponible.",
                        "Stock insuficient",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    continuar = false;
                }
            }

            if (continuar)
            {
                linia.Quantitat++;

                linia.Subtotal =
                    linia.Quantitat *
                    linia.PreuUnitari;

                ActualitzarCarreto();
            }
        }

        #endregion

        #region Disminuir Quantitat

        private void BtnDisminuirQuantitat_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button boto =
                sender as Button;

            LiniaComanda linia =
                boto.DataContext as LiniaComanda;

            bool continuar = true;

            if (linia == null)
            {
                continuar = false;
            }

            if (continuar)
            {
                if (linia.Quantitat > 1)
                {
                    linia.Quantitat--;

                    linia.Subtotal =
                        linia.Quantitat *
                        linia.PreuUnitari;

                    ActualitzarCarreto();
                }
            }
        }

        #endregion

        #region Eliminar Producte

        private void BtnEliminarCarreto_Click(
            object sender,
            RoutedEventArgs e)
        {
            Button boto =
                sender as Button;

            LiniaComanda linia =
                boto.DataContext as LiniaComanda;

            bool continuar = true;

            if (linia == null)
            {
                continuar = false;
            }

            if (continuar)
            {
                carrito.Remove(linia);

                ActualitzarCarreto();
            }
        }

        #endregion

        #region Factura PDF

        private async void BtnFactura_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                Button boto =
                    sender as Button;

                bool continuar = true;

                if (boto == null)
                {
                    continuar = false;
                }

                Comanda comanda = null;

                if (continuar)
                {
                    comanda =
                        boto.DataContext as Comanda;

                    if (comanda == null)
                    {
                        MessageBox.Show(
                            "No s'ha pogut seleccionar la comanda.",
                            "Factura",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        continuar = false;
                    }
                }

                if (continuar)
                {
                    if (comanda.IdClient != idClient)
                    {
                        MessageBox.Show(
                            "No pots accedir a aquesta factura.",
                            "Accés denegat",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        continuar = false;
                    }
                }

                Factura factura = null;

                if (continuar)
                {
                    factura =
                        await dao.GetFacturaByComanda(
                            comanda.Id);

                    if (factura == null)
                    {
                        MessageBox.Show(
                            "No s'ha trobat la factura d'aquesta comanda.",
                            "Factura",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        continuar = false;
                    }
                }

                if (continuar)
                {
                    if (string.IsNullOrWhiteSpace(
                        factura.PdfURL))
                    {
                        MessageBox.Show(
                            "La factura no té cap PDF associat.",
                            "Factura",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        continuar = false;
                    }
                }

                if (continuar)
                {
                    if (!System.IO.File.Exists(
                        factura.PdfURL))
                    {
                        MessageBox.Show(
                            "No s'ha trobat el PDF de la factura.\n\n"
                            + factura.PdfURL,
                            "Factura",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        continuar = false;
                    }
                }

                if (continuar)
                {
                    Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = factura.PdfURL,
                            UseShellExecute = true
                        });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en obrir la factura:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Pagar

        private async void BtnPagar_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                bool continuar =
                    ValidarPagament();

                if (continuar)
                {
                    continuar =
                        await ComprovarStock();
                }

                if (continuar)
                {
                    continuar =
                        ConfirmarPagament();
                }

                if (continuar)
                {
                    await ProcessarCompra();
                }
            }
            catch (Exception ex)
            {
                MostrarErrorCompra(ex);
            }
        }


        #endregion

        #region Validar Pagament

        private bool ValidarPagament()
        {
            bool continuar = true;

            if (carrito == null || carrito.Count == 0)
            {
                MessageBox.Show(
                    "El carretó està buit.",
                    "Carretó buit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar &&
                string.IsNullOrWhiteSpace(idClient))
            {
                MessageBox.Show(
                    "No s'ha pogut identificar el client.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                continuar = false;
            }

            if (continuar &&
                string.IsNullOrWhiteSpace(
                    txtNomTitular.Text))
            {
                MessageBox.Show(
                    "Introdueix el nom del titular.",
                    "Pagament",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar &&
                string.IsNullOrWhiteSpace(
                    txtNumeroTargeta.Text))
            {
                MessageBox.Show(
                    "Introdueix el número de targeta.",
                    "Pagament",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar &&
                string.IsNullOrWhiteSpace(
                    txtDataCaducitat.Text))
            {
                MessageBox.Show(
                    "Introdueix la data de caducitat.",
                    "Pagament",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar &&
                string.IsNullOrWhiteSpace(
                    txtCVV.Text))
            {
                MessageBox.Show(
                    "Introdueix el CVV.",
                    "Pagament",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                continuar = false;
            }

            return continuar;
        }

        #endregion

        #region Comprovar Stock

        private async Task<bool> ComprovarStock()
        {
            bool continuar = true;

            foreach (LiniaComanda linia in carrito)
            {
                Producte producte =
                    productes.FirstOrDefault(
                        p => p.Id ==
                        linia.IdProducte);

                if (producte == null)
                {
                    MessageBox.Show(
                        "No s'ha trobat el producte "
                        + linia.NomProducte,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    continuar = false;
                }
                else if (producte.Stock < linia.Quantitat)
                {
                    MessageBox.Show(
                        "No hi ha prou stock de:\n\n"
                        + producte.Nom,
                        "Stock insuficient",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    continuar = false;
                }
            }

            return continuar;
        }

        #endregion

        #region Confirmar Pagament

        private bool ConfirmarPagament()
        {
            double total =
                carrito.Sum(
                    l => l.Subtotal);

            MessageBoxResult resultat =
                MessageBox.Show(
                    "Vols confirmar el pagament de "
                    + total.ToString("F2")
                    + " €?",
                    "Confirmar pagament",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            bool continuar = true;

            if (resultat != MessageBoxResult.Yes)
            {
                continuar = false;
            }

            return continuar;
        }

        #endregion

        #region Processar Compra

        private async Task ProcessarCompra()
        {
            double total =
                carrito.Sum(
                    l => l.Subtotal);

            DateTime dataCompra =
                DateTime.UtcNow;

            dataCompra =
                DateTime.SpecifyKind(
                    dataCompra,
                    DateTimeKind.Utc);

            Comanda comanda =
                new Comanda(
                    Guid.NewGuid().ToString(),
                    idClient,
                    dataCompra,
                    total,
                    "En preparació",
                    new List<LiniaComanda>(
                        carrito));

            await dao.AddComanda(
                comanda);

            User client =
                await dao.GetUserById(
                    idClient);

            bool continuar = true;

            if (client == null)
            {
                MessageBox.Show(
                    "La comanda s'ha creat, però no s'ha pogut trobar el client.",
                    "Factura",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                continuar = false;
            }

            if (continuar)
            {
                Factura factura =
                    await CrearFactura(
                        comanda,
                        client);

                await GuardarLinies();

                await ActualitzarStock();

                FinalitzarCompra();

                await CarregarProductes();
                await CarregarComandes();

                MostrarCompraCorrecta(
                    comanda,
                    factura);
            }
        }

        #endregion

        #region Crear Factura

        private async Task<Factura> CrearFactura(
            Comanda comanda,
            User client)
        {
            Factura factura =
                new Factura(
                    Guid.NewGuid().ToString(),
                    comanda.Id,
                    DateTime.UtcNow,
                    comanda.Total,
                    "");

            List<Devolucio> devolucions =
                await dao.GetDevolucionsByComanda(
                    comanda.Id);

            if (devolucions == null)
            {
                devolucions =
                    new List<Devolucio>();
            }

            List<Producte> productesActuals =
                await dao.GetAllProductes();

            string rutaPDF =
                GeneradorFactura.GenerarFacturaPDF(
                    factura,
                    comanda,
                    client,
                    new List<LiniaComanda>(
                        carrito),
                    devolucions,
                    productesActuals);

            factura.PdfURL =
                rutaPDF;

            await dao.AddFactura(
                factura);

            return factura;
        }

        #endregion

        #region Guardar Linies

        private async Task GuardarLinies()
        {
            foreach (LiniaComanda linia in carrito)
            {
                await dao.AddLiniaComanda(
                    linia);
            }
        }

        #endregion

        #region Actualitzar Stock

        private async Task ActualitzarStock()
        {
            foreach (LiniaComanda linia in carrito)
            {
                Producte producte =
                    productes.FirstOrDefault(
                        p => p.Id ==
                        linia.IdProducte);

                if (producte != null)
                {
                    producte.Stock -=
                        linia.Quantitat;

                    await dao.UpdateProducte(
                        producte);
                }
            }
        }

        #endregion

        #region Finalitzar Compra

        private void FinalitzarCompra()
        {
            carrito.Clear();

            ActualitzarCarreto();

            txtNomTitular.Clear();
            txtNumeroTargeta.Clear();
            txtDataCaducitat.Clear();
            txtCVV.Clear();
        }

        #endregion

        #region Missatge Compra Correcta

        private void MostrarCompraCorrecta(
            Comanda comanda,
            Factura factura)
        {
            MessageBox.Show(
                "La compra s'ha realitzat correctament.\n\n"
                + "Comanda: "
                + comanda.Id
                + "\n"
                + "Factura: "
                + factura.Id
                + "\n"
                + "Total: "
                + factura.Total.ToString("F2")
                + " €\n\n"
                + "La factura s'ha guardat correctament a Firebase Firestore.",
                "Compra realitzada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion

        #region Error Compra

        private void MostrarErrorCompra(
            Exception ex)
        {
            MessageBox.Show(
                "Error en realitzar la compra:\n\n"
                + ex.Message
                + "\n\nTipus d'error:\n"
                + ex.GetType().Name,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        #endregion

        #endregion

        #region Comandes

        #region Carregar Comandes

        private async Task CarregarComandes()
        {
            try
            {
                bool continuar = true;

                if (string.IsNullOrWhiteSpace(idClient))
                {
                    MessageBox.Show(
                        "No s'ha pogut identificar el client.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    continuar = false;
                }

                if (continuar)
                {
                    List<Comanda> comandes =
                        await dao.GetComandesByClient(
                            idClient);

                    comandes =
                        comandes
                        .OrderByDescending(c => c.Data)
                        .ToList();

                    dgComandes.ItemsSource =
                        comandes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar les comandes:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Actualitzar Comandes

        private async void BtnActualitzarComandes_Click(
            object sender,
            RoutedEventArgs e)
        {
            await CarregarComandes();
        }

        #endregion

        #endregion

        #region Misatges

        private const string ID_VENEDOR = "VENEDOR";

        private async Task CarregarMissatgesVenedor()
        {
            try
            {
                List<Missatge> totsElsMissatges =
                    await dao.GetAllMissatges();

                missatgesActuals =
                    totsElsMissatges
                        .Where(m =>
                            (m.IdEmissor == idClient &&
                             m.IdReceptor == ID_VENEDOR) ||
                            (m.IdEmissor == ID_VENEDOR &&
                             m.IdReceptor == idClient))
                        .OrderBy(m => m.Data)
                        .ToList();

                panelMissatges.Children.Clear();

                foreach (
                    Missatge missatge
                    in missatgesActuals)
                {
                    AfegirMissatgeVisual(
                        missatge);
                }

                await MarcarMissatgesComLlegits();

                scrollMissatges.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar els missatges amb el venedor:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void AfegirMissatgeVisual(
            Missatge missatge)
        {
            Border contenidor = new Border();
            contenidor.Margin = new Thickness(5);
            contenidor.Padding = new Thickness(10);
            contenidor.CornerRadius = new CornerRadius(8);
            contenidor.MaxWidth = 500;
            StackPanel contingut = new StackPanel();
            TextBlock text = new TextBlock();
            text.Text = missatge.Text;
            text.TextWrapping = TextWrapping.Wrap;
            text.FontSize = 14;
            StackPanel filaInferior = new StackPanel(); 
            filaInferior.Orientation = Orientation.Horizontal;
            filaInferior.HorizontalAlignment = HorizontalAlignment.Right;
            filaInferior.Margin = new Thickness(0, 5, 0, 0);

            TextBlock data = new TextBlock();

            data.Text =
                missatge.Data
                    .ToLocalTime()
                    .ToString("dd/MM/yyyy HH:mm");

            data.FontSize = 10;


            filaInferior.Children.Add(
                data);

            if (missatge.IdEmissor == idClient)
            {
                TextBlock checks =
                    new TextBlock();

                checks.Text =
                    "  ✓✓";

                checks.FontSize = 12;

                checks.FontWeight =
                    FontWeights.Bold;

                if (missatge.Llegit)
                {
                    checks.Foreground =
                        Brushes.DeepSkyBlue;
                }
                else
                {
                    checks.Foreground =
                        Brushes.Gray;
                }

                filaInferior.Children.Add(
                    checks);
            }


            contingut.Children.Add(
                text);

            contingut.Children.Add(
                filaInferior);


            if (missatge.IdEmissor == idClient)
            {
                contenidor.HorizontalAlignment =
                    HorizontalAlignment.Right;

                contenidor.Background =
                    Brushes.LightBlue;
            }
            else
            {
                contenidor.HorizontalAlignment =
                    HorizontalAlignment.Left;

                contenidor.Background =
                    Brushes.LightGray;
            }


            contenidor.Child =
                contingut;

            panelMissatges.Children.Add(
                contenidor);
        }


        private async void BtnEnviarMissatgeVenedor_Click(
             object sender,
             RoutedEventArgs e)
        {
            try
            {
                string text =
                    txtMissatgeVenedor.Text.Trim();

                bool continuar = true;

                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show(
                        "Escriu un missatge abans d'enviar-lo.",
                        "Missatge buit",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    continuar = false;
                }

                if (continuar)
                {
                    if (string.IsNullOrWhiteSpace(idClient))
                    {
                        MessageBox.Show(
                            "No s'ha pogut identificar el client.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        continuar = false;
                    }
                }

                if (continuar)
                {
                    Missatge missatge =
                        new Missatge(
                            Guid.NewGuid().ToString(),
                            idClient,
                            ID_VENEDOR,
                            text,
                            DateTime.UtcNow,
                            false);

                    await dao.AddMissatge(
                        missatge);

                    txtMissatgeVenedor.Clear();

                    await CarregarMissatgesVenedor();
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

        private async Task MarcarMissatgesComLlegits()
        {
            try
            {
                foreach (
                    Missatge missatge
                    in missatgesActuals)
                {
                    if (missatge.IdReceptor == idClient &&
                        missatge.IdEmissor == ID_VENEDOR &&
                        !missatge.Llegit)
                    {
                        missatge.Llegit = true;

                        await dao.UpdateMissatge(
                            missatge);
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

        #endregion

        #region PERFIL

        #region Canviar Contrasenya

        private async void BtnCanviarContrasenya_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                bool continuar = true;

                if (string.IsNullOrWhiteSpace(idClient))
                {
                    MessageBox.Show(
                        "No s'ha pogut identificar el client.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    continuar = false;
                }

                User user = null;

                if (continuar)
                {
                    user =
                        await dao.GetUserById(
                            idClient);

                    if (user == null)
                    {
                        MessageBox.Show(
                            "No s'ha trobat l'usuari.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        continuar = false;
                    }
                }

                if (continuar)
                {
                    if (string.IsNullOrWhiteSpace(
                        user.Email))
                    {
                        MessageBox.Show(
                            "Aquest usuari no té cap correu electrònic associat.",
                            "Canviar contrasenya",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        continuar = false;
                    }
                }

                MessageBoxResult resultat =
                    MessageBoxResult.No;

                if (continuar)
                {
                    resultat =
                        MessageBox.Show(
                            "S'enviarà un correu de recuperació a:\n\n"
                            + user.Email
                            + "\n\nVols continuar?",
                            "Canviar contrasenya",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                    if (resultat != MessageBoxResult.Yes)
                    {
                        continuar = false;
                    }
                }

                if (continuar)
                {
                    bool enviat =
                        await Autentication.RecuperarContrasenya(
                            user.Email);

                    if (enviat)
                    {
                        MessageBox.Show(
                            "S'ha enviat un correu a:\n\n"
                            + user.Email
                            + "\n\n"
                            + "Obre el correu i segueix les instruccions "
                            + "per establir una nova contrasenya.",
                            "Canviar contrasenya",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "No s'ha pogut enviar el correu de recuperació.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en canviar la contrasenya:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        private async Task CarregarPerfil()
        {
            try
            {
                bool continuar = true;

                if (string.IsNullOrWhiteSpace(idClient))
                {
                    MessageBox.Show(
                        "No s'ha pogut identificar el client.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    continuar = false;
                }

                User user = null;

                if (continuar)
                {
                    user =
                        await dao.GetUserById(
                            idClient);

                    if (user == null)
                    {
                        MessageBox.Show(
                            "No s'han trobat les dades del client.",
                            "Perfil",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        continuar = false;
                    }
                }

                if (continuar)
                {
                    txtNom.Text =
                        user.Nom;

                    txtEmail.Text =
                        user.Email;

                    txtTelefon.Text =
                        user.Telefon;

                    txtAdreca.Text =
                        user.Adreca;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en carregar el perfil:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void BtnGuardarPerfil_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                bool continuar = true;

                if (string.IsNullOrWhiteSpace(idClient))
                {
                    MessageBox.Show(
                        "No s'ha pogut identificar el client.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    continuar = false;
                }

                if (continuar)
                {
                    if (string.IsNullOrWhiteSpace(
                        txtNom.Text))
                    {
                        MessageBox.Show(
                            "Introdueix el teu nom.",
                            "Perfil",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        continuar = false;
                    }
                }

                if (continuar)
                {
                    if (string.IsNullOrWhiteSpace(
                        txtEmail.Text))
                    {
                        MessageBox.Show(
                            "Introdueix el teu email.",
                            "Perfil",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        continuar = false;
                    }
                }

                User user = null;

                if (continuar)
                {
                    user =
                        await dao.GetUserById(
                            idClient);

                    if (user == null)
                    {
                        MessageBox.Show(
                            "No s'ha trobat l'usuari.",
                            "Perfil",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        continuar = false;
                    }
                }

                if (continuar)
                {
                    user.Nom =
                        txtNom.Text.Trim();

                    user.Email =
                        txtEmail.Text.Trim();

                    user.Telefon =
                        txtTelefon.Text.Trim();

                    user.Adreca =
                        txtAdreca.Text.Trim();

                    await dao.UpdateUser(
                        user);

                    MessageBox.Show(
                        "Les dades del perfil s'han guardat correctament.",
                        "Perfil",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en guardar el perfil:\n\n"
                    + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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